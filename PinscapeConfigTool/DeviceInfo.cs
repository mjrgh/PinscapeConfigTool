using System;
using System.Collections.Generic;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Security.Permissions;
using CollectionUtils;
using System.Windows.Input;

// Pinscape Controller device information.  This represents one connected
// KL25Z controller device.
public class DeviceInfo : IDisposable
{
    public class Comparer: IEqualityComparer<DeviceInfo>
    {
        public bool Equals(DeviceInfo a, DeviceInfo b)
        {
            return a.CPUID == b.CPUID
                && a.LedWizUnitNo == b.LedWizUnitNo
                && a.PinscapeUnitNo == b.PinscapeUnitNo;
        }

        public int GetHashCode(DeviceInfo a)
        {
            return a.CPUID.GetHashCode();
        }
    }

    ~DeviceInfo()
    {
        Dispose(false);
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (evov != null)
            {
                evov.Dispose();
                evov = null;
            }
        }

        if (fp != IntPtr.Zero && fp.ToInt32() != -1)
        {
            HIDImports.CloseHandle(fp);
            fp = IntPtr.Zero;
        }
    }

    public static List<String> findDeviceDebugInfo = new List<String>();

    // Get a list of connected Pinscape Controller devices
    public delegate void FindDeviceCallback(DeviceInfo dev, IntPtr hDeviceInfoSet, ref HIDImports.SP_DEVINFO_DATA devInfoData);
    public static List<DeviceInfo> FindDevices(FindDeviceCallback callback = null)
    {
        findDeviceDebugInfo.Clear();
        
        // set up an empty return list
        List<DeviceInfo> devices = new List<DeviceInfo>();

        // get the list of devices matching the HID class GUID
        Guid guid = Guid.Empty;
        HIDImports.HidD_GetHidGuid(out guid);
        IntPtr hdev = HIDImports.SetupDiGetClassDevs(ref guid, null, IntPtr.Zero, HIDImports.DIGCF_DEVICEINTERFACE);

        // set up the attribute structure buffer
        HIDImports.SP_DEVICE_INTERFACE_DATA diData = new HIDImports.SP_DEVICE_INTERFACE_DATA();
        diData.cbSize = Marshal.SizeOf(diData);

        // read the devices in our list
        for (uint i = 0;
            HIDImports.SetupDiEnumDeviceInterfaces(hdev, IntPtr.Zero, ref guid, i, ref diData);
            ++i)
        {
            // get the size of the detail data structure
            UInt32 size = 0;
            HIDImports.SetupDiGetDeviceInterfaceDetail(hdev, ref diData, IntPtr.Zero, 0, out size, IntPtr.Zero);

            String debugInfo = "#" + i + " (" + hdev.ToString() + ")";

            // now actually read the detail data structure
            HIDImports.SP_DEVICE_INTERFACE_DETAIL_DATA diDetail = new HIDImports.SP_DEVICE_INTERFACE_DETAIL_DATA();
            diDetail.cbSize = (IntPtr.Size == 8) ? (uint)8 : (uint)5;
            HIDImports.SP_DEVINFO_DATA devInfoData = new HIDImports.SP_DEVINFO_DATA();
            devInfoData.cbSize = Marshal.SizeOf(devInfoData);
            if (HIDImports.SetupDiGetDeviceInterfaceDetail(hdev, ref diData, ref diDetail, size, out size, out devInfoData))
            {
                debugInfo += " path=" + diDetail.DevicePath;

                // create a file handle to access the device
                IntPtr fp = HIDImports.CreateFile(
                    diDetail.DevicePath, HIDImports.GENERIC_READ_WRITE, HIDImports.SHARE_READ_WRITE,
                    IntPtr.Zero, HIDImports.OPEN_EXISTING, 0, IntPtr.Zero);

                debugInfo += " fp=" + fp.ToString();

                // read the attributes
                Thread.Sleep(1);
                HIDImports.HIDD_ATTRIBUTES attrs = new HIDImports.HIDD_ATTRIBUTES();
                attrs.Size = Marshal.SizeOf(attrs);
                if (HIDImports.HidD_GetAttributes(fp, ref attrs))
                {
                    // Read the product name string.  Note that 
                    String name = "<not available>";
                    byte[] nameBuf = new byte[256];
                    if (HIDImports.HidD_GetProductString(fp, nameBuf, 256))
                        name = System.Text.Encoding.Unicode.GetString(nameBuf).TrimEnd('\0');
                    else
                        debugInfo += " [GetProductString failed]";

                    debugInfo += " productString=\"" + name + "\"";

                    // if the vendor and product ID match an LedWiz, and the
                    // product name contains "pinscape", it's one of ours
                    DeviceInfo di;
                    if (CheckIDMatch(attrs)
                        && Regex.IsMatch(name, @"\b(?i)pinscape controller\b")
                        && (di = DeviceInfo.Create(
                            diDetail.DevicePath, name, attrs.VendorID, attrs.ProductID, attrs.VersionNumber)) != null)
                    {
                        debugInfo += " [device added]";

                        // add the device to our list
                        devices.Add(di);

                        // if there's a callback, invoke it
                        if (callback != null)
                            callback(di, hdev, ref devInfoData);
                    }

#if false
                    // report the results for debugging
                    Console.Out.WriteLine(
                        "Product " + attrs.ProductID.ToString("X4")
                        + ", vendor " + attrs.VendorID.ToString("X4")
                        + ", version " + attrs.VersionNumber.ToString("X4")
                        + ", name " + name);
#endif
                }
                else
                    debugInfo += " [GetAttributes failed]";

                // done with the file handle
                if (fp.ToInt32() != 0 && fp.ToInt32() != -1)
                    HIDImports.CloseHandle(fp);
            }
            else
                debugInfo += " [GetDeviceInterfaceDetail failed]";

            findDeviceDebugInfo.Add(debugInfo);
        }

        // done with the device info list
        HIDImports.SetupDiDestroyDeviceInfoList(hdev);

        // return the device list
        return devices;
    }

    // Check for a USB Product/Vendor ID match to the known values.
    //
    // NB! We're permissive on PID/VID matching.  We'll match ANY IDs, and
    // instead let the caller rely on the product ID string and device query 
    // messages.  We go through the motions of checking for the known ID 
    // codes (the LedWiz codes and our private registered code), but this 
    // is purely for the sake of documentation - we always return true in 
    // the end.
    public static bool CheckIDMatch(HIDImports.HIDD_ATTRIBUTES attrs)
    {
        ushort vid = (ushort)attrs.VendorID;
        ushort pid = (ushort)attrs.ProductID;

        // if it's an LedWiz-compatible code, it's one of ours
        if (vid == 0xFAFA && (pid >= 0x00F0 && pid <= 0x00FF))
            return true;

        // if it's our private Pinscape registration code, it's one of ours
        if (vid == 0x1209 && pid == 0xEAEA)
            return true;

        // It's not one of our known VID/PID combos, but allow it anyway,
        // in case the user is using a custom ID for some reason.  We'll
        // filter out non-Pinscape devices via other, better tests later.
        return true;
    }
    
    // save the device configuration and reboot
    public bool SaveConfigAndReboot()
    {
        return SaveConfig(true);
    }

    // Save the device configuration, and optionally reboot the KL25Z
    public bool SaveConfig(bool reboot)
    {
        // show a wait cursor while writing
        Mouse.SetCursor(Cursors.Wait);

        // get a configuration report
        ConfigReport cfg = GetConfigReport();

        // set the request flags
        //   0x01 -> do not reboot (default is to reboot)
        byte flags = 0;
        if (!reboot) flags |= 0x01;

        // Save updates and reboot the KL25Z.  Tell the device to wait
        // a second after completing the write before it reboots, to
        // allow time for pending USB messages to finish, and to allow
        // for a few status updates after the save showing the "config
        // save succeeded" bit in the status.
        FlushReadUSB();
        if (SpecialRequest(6, new byte[] { 1, flags }))
        {
            // We successful sent the request, but we don't know yet if
            // the save actually succeeded.  For newer firmware versions,
            // the device tells us that a save was successful by setting 
            // bit 0x40 in the regular status report status byte.  Older
            // versions didn't have this, so we can't tell.  If the bit
            // is supported, wait a few seconds to see if we get an OK
            // report.  Don't wait too long; on failure, the device will
            // simply either return to normal operation or reboot itself,
            // so we won't see a negative acknowledgment.
            if (cfg != null && cfg.flashStatusFeature)
            {
                DateTime t0 = DateTime.Now;
                while ((DateTime.Now - t0).TotalMilliseconds < 3000)
                {
                    // read a status report and check for bit 0x40 ("config
                    // save succeeded") in the status byte
                    byte[] buf = ReadStatusReport();
                    if (buf != null && (buf[1] & 0x40) != 0)
                        return true;
                }
            }
            else
            {
                // it's an older firmware version that doesn't report
                // status; assume success
                return true;
            }
        }

        // we failed to send the request, or we didn't hear back 
        // from the device that it was successful
        return false;
    }

    // Get the config report
    public class ConfigReport
    {
        public ConfigReport(byte[] buf)
        {
            numOutputs = buf[3] | (buf[4] << 8);
            psUnitNo = (buf[5] | (buf[6] << 8)) + 1;
            plungerZero = buf[7] | (buf[8] << 8);
            plungerMax = buf[9] | (buf[10] << 8);
            plungerTime = buf[11];
            configured = (buf[12] & 0x01) != 0;
            sbxpbx = (buf[12] & 0x02) != 0;
            accelFeatures = (buf[12] & 0x04) != 0;
            flashStatusFeature = (buf[12] & 0x08) != 0;
            reportTimingFeatures = (buf[12] & 0x10) != 0;
            chimeLogicFeatures = (buf[12] & 0x20) != 0;
            freeHeapBytes = buf[13] | (buf[14] << 8);
        }

        public bool configured;     // a saved configuration has been loaded; 
                                    // if this is false, factory defaults are in effect
        public bool sbxpbx;         // SBX/PBX extended commands supported
        public bool accelFeatures;  // accelerometer customization features supported
                                    // (adjustable dynamic range, auto centering on/off,
                                    // adjustable auto centering time)
        public bool reportTimingFeatures; // joystick report timing features (report interval,
                                    // accelerometer stutter counter)
        public bool flashStatusFeature;  // "flash write ok" status bit is supported in
                                    // joystick reports
        public bool chimeLogicFeatures; // chime logic features enabled
        public int psUnitNo;        // Pinscape unit number, 1-16
        public int numOutputs;      // number of configured (in-use) feedback device outputs
        public int plungerZero;     // plunger calibration zero point
        public int plungerMax;      // plunger calibration maximum
        public int plungerTime;     // plunger release time, milliseconds
        public int freeHeapBytes;   // number of heap memory bytes available
    }
    public ConfigReport GetConfigReport()
    {
        byte[] buf = SpecialRequest(4, new byte[] { }, (r) => r[1] == 0 && r[2] == 0x88);
        if (buf != null)
            return new ConfigReport(buf);
        else
            return null;
    }

    // Create a copy of a device info object from a given source object.
    // The new object has its own independent file handle to the device's
    // control protocol interface endpoint.  This is useful when a separate
    // file channel is needed, such as for a thread.
    public DeviceInfo(DeviceInfo d)
    {
        this.path = d.path;
        this.name = d.name;
        this.vendorID = d.vendorID;
        this.productID = d.productID;
        this.version = d.version;
        this.PlungerEnabled = d.PlungerEnabled;
        this.JoystickEnabled = d.JoystickEnabled;
        this.TvOnEnabled = d.TvOnEnabled;
        this.isValid = d.isValid;
        this.CPUID = d.CPUID;
        this.OpenSDAID = d.OpenSDAID;
        this.BuildDD = d.BuildDD;
        this.BuildTT = d.BuildTT;
        this.BuildID = d.BuildID;
        this.LedWizUnitNo = d.LedWizUnitNo;
        this.PinscapeUnitNo = d.PinscapeUnitNo;
        this.inputReportLength = d.inputReportLength;
        this.fp = OpenFile();
    }

    private DeviceInfo(string path, string name, ushort vendorID, ushort productID, ushort version)
    {
        // remember the settings
        this.path = path;
        this.name = name;
        this.vendorID = (ushort)vendorID;
        this.productID = (ushort)productID;        
        this.version = version;
        
        // set some defaults
        PlungerEnabled = true;
        JoystickEnabled = false;
        TvOnEnabled = false;

        // open the USB interface 
        this.fp = OpenFile();

        // presume valid
        isValid = true;
        
        // assume the maximum input (device to host) report length
        inputReportLength = 65;

        // Check the HID interface to see if the HID Usage type is 
        // type 4, for Joystick.  If so, the joystick interface is
        // enabled, which the device uses to send nudge and plunger
        // readings.  If not, the joystick interface is disabled, so
        // the device only sends private status messages and query
        // responses.
        IntPtr ppdata;
        if (HIDImports.HidD_GetPreparsedData(this.fp, out ppdata))
        {
            // Check the usage.  If the joystick is enabled, the
            // usage will be 4 = Joystick (on usage page 1, "generic
            // desktop").  If not, the usage is 0 = Undefined, indicating 
            // our private status and query interface.
            HIDImports.HIDP_CAPS caps;
            HIDImports.HidP_GetCaps(ppdata, out caps);
            bool isJoystick = caps.UsagePage == 1 && caps.Usage == 4;
            bool isPrivate = caps.UsagePage == 1 && caps.Usage == 0;
            if (isJoystick)
                this.JoystickEnabled = true;

            // Only count this as a control interface if it's the joystick
            // interface or the private interface.  The device can expose
            // other interfaces for device-to-host inputs only, such as a
            // keyboard interface and a media key interface.  Also ignore
            // it if it doesn't accept output reports (host-to-device),
            // as indicated by a zero output report length.
            if (!(isJoystick || isPrivate) || caps.OutputReportByteLength == 0)
                isValid = false;

            // remember the input report (device to host) length
            inputReportLength = caps.InputReportByteLength;

            // free the preparsed data
            HIDImports.HidD_FreePreparsedData(ppdata);
        }
        else
        {
            // couldn't get the HID caps - mark as invalid
            isValid = false;
        }

        // if it looks like a valid interface so far, try reading a status report
        byte[] buf;
        if (isValid)
        {
            buf = ReadUSB();
            if (buf != null)
            {
                // parse the reponse
                this.PlungerEnabled = (buf[1] & 0x01) != 0;
            }
            else
            {
                // couldn't read a report - mark it as invalid
                isValid = false;
            }
        }

        // Ask about the TV ON feature.  This is config variable 9; the
        // results are: [0] power status input pin; [1] latch output pin;
        // [2] relay trigger pin; [3,4] delay time in 10ms increments.
        // This is enabled only if all pins are assigned and the delay
        // is non-zero.
        if (isValid && (buf = QueryConfigVar(9)) != null)
        {
            int delay = buf[3] | (buf[4] << 8);
            this.TvOnEnabled =
                (buf[0] != 0xFF && buf[1] != 0xFF && buf[2] != 0xFF
                && delay != 0);
        }

        // figure the LedWiz unit number
        LedWizUnitNo = (vendorID == 0xFAFA ? ((productID & 0x0F) + 1) : 0);

        // get more data if the unit is responding
        if (isValid)
        {
            // get the CPU ID
            this.CPUID = QueryCPUID();
            this.OpenSDAID = QueryOpenSDAID();

            // get the pinscape unit number
            if ((buf = QueryConfigVar(2)) != null)
                PinscapeUnitNo = buf[0];

            // get the build ID
            String s;
            QueryBuildID(out this.BuildDD, out this.BuildTT, out s);
            this.BuildID = s;
        }
    }

    private static DeviceInfo Create(string path, string name, ushort vendorID, ushort productID, ushort version)
    {
        DeviceInfo di = new DeviceInfo(path, name, vendorID, productID, version);
        return di.isValid ? di : null;
    }

    public override string ToString()
    {
        return name + " (Pinscape unit " + PinscapeUnitNo + "/LedWiz unit " + LedWizUnitNo + ")";
    }

    // native overlapped object, and event handle to access it
    private EventWaitHandle evov = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.AutoReset);

    // clear out the USB input buffer
    public void FlushReadUSB()
    {
        // note the starting time so we don't wait forever
        DateTime t0 = DateTime.Now;

        // wait until a read would block
        int rptLen = inputReportLength;
        while ((DateTime.Now - t0).TotalMilliseconds < 100)
        {
            // set up a non-blocking read
            IntPtr buf = Marshal.AllocHGlobal(rptLen);
            try
            {
                unsafe
                {
                    // set up the overlapped I/O descriptor
                    Overlapped o = new Overlapped(0, 0, evov.SafeWaitHandle.DangerousGetHandle(), null);
                    NativeOverlapped* no = o.Pack(null, null);

                    // start the non-blocking read
                    Marshal.WriteByte(buf, 0);
                    HIDImports.ReadFile(fp, buf, rptLen, IntPtr.Zero, no);

                    // check to see if it's ready immediately
                    bool ready = evov.WaitOne(0);
                    if (ready)
                    {
                        // it's ready - complete the read
                        UInt32 readLen;
                        int result = HIDImports.GetOverlappedResult(fp, no, out readLen, 0);
                    }
                    else
                    { 
                        // Not ready - we'd have to wait to do a read, so the
                        // buffer is empty.  Cancel the read.
                        HIDImports.CancelIo(fp);
                    }

                    // done with the overlapped I/O descriptor
                    Overlapped.Unpack(no);
                    Overlapped.Free(no);

                    // if there was nothing ready to read, we've cleared out buffered
                    // inputs, so we're done
                    if (!ready)
                        return;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buf);
            }
        }
    }

    public byte[] ReadUSB()
    {
        // try reading a few times, in case we lose the connection briefly
        int rptLen = inputReportLength;
        for (int tries = 0; tries < 3; ++tries)
        {
            unsafe
            {
                // set up a non-blocking ("overlapped") read
                IntPtr buf = Marshal.AllocHGlobal((int)rptLen);
                try
                {
                    Marshal.WriteByte(buf, 0);
                    Overlapped o = new Overlapped(0, 0, evov.SafeWaitHandle.DangerousGetHandle(), null);
                    NativeOverlapped* no = o.Pack(null, null);
                    HIDImports.ReadFile(fp, buf, rptLen, IntPtr.Zero, no);

                    // Wait briefly for the read to complete.  But don't wait forever - we might
                    // be talking to a device interface that doesn't provide the type of status
                    // report we're looking for, in which case we don't want to get stuck waiting
                    // for something that will never happen.  If this is indeed the controller
                    // interface we're interested in, it will respond within a few milliseconds
                    // with our status report.
                    if (evov.WaitOne(100))
                    {
                        // The read completed successfully!  Get the result.
                        UInt32 readLen;
                        int result = HIDImports.GetOverlappedResult(fp, no, out readLen, 0);

                        Overlapped.Unpack(no);
                        Overlapped.Free(no);

                        if (result == 0)
                        {
                            // The read failed.  Try re-opening the file handle in case we
                            // dropped the connection, then re-try the whole read.
                            TryReopenHandle();
                            continue;
                        }
                        else if (readLen != rptLen)
                        {
                            // The read length didn't match what we expected.  This might be
                            // a different device (not a Pinscape controller) or a different
                            // version that we don't know how to talk to.  In either case,
                            // return failure.
                            return null;
                        }
                        else
                        {
                            // The read succeed and was the correct size.  Return the data.
                            byte[] retbuf = new byte[rptLen];
                            Marshal.Copy(buf, retbuf, 0, rptLen);
                            return retbuf;
                        }
                    }
                    else
                    {
                        // The read timed out.  This must not be our control interface after
                        // all.  Cancel the read and try reopening the handle.
                        HIDImports.CancelIo(fp);
                        Overlapped.Unpack(no);
                        Overlapped.Free(no);
                        if (TryReopenHandle())
                            continue;
                        return null;
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(buf);
                }
            }
        }

        // don't retry more than a few times
        return null;
    }


    private IntPtr OpenFile()
    {
        return HIDImports.CreateFile(
            path, HIDImports.GENERIC_READ_WRITE, HIDImports.SHARE_READ_WRITE,
            IntPtr.Zero, HIDImports.OPEN_EXISTING, HIDImports.EFileAttributes.Overlapped, IntPtr.Zero);
    }

    private bool TryReopenHandle()
    {
        // if the last error is 6 ("invalid handle") or 1167 ("Device not connected"), 
        // try re-opening the handle
        int err = Marshal.GetLastWin32Error();
        if (err == 6 || err == 1167)
        {
            // try opening a new handle on the device path
            Console.WriteLine("manipulation invalide en lecture / écriture - tentative de réouverture");
            IntPtr fp2 = OpenFile();

            // if that succeeded, replace the old handle with the new one and retry the read
            if (fp2 != IntPtr.Zero && fp2.ToInt32() != -1)
            {
                // close the old handle
                if (fp != IntPtr.Zero && fp.ToInt32() != -1)
                    HIDImports.CloseHandle(fp);

                // replace the handle
                fp = fp2;

                // tell the caller to try again
                return true;
            }
        }

        // we didn't successfully reopen the handle
        return false;
    }
    
    public String GetLastWin32ErrMsg()
    {
        int errno = Marshal.GetLastWin32Error();
        return String.Format("{0} (Win32 error {1})", 
            new System.ComponentModel.Win32Exception(errno).Message, errno);
    }

    // read a status report
    public byte[] ReadStatusReport()
    {
        // flush any buffered reports so we get real-time status
        FlushReadUSB();

        // read reports until we get a status report
        for (int i = 0; i < 32; ++i)
        {
            // read a report
            byte[] buf = ReadUSB();
            if (buf == null)
                return null;

            // if it's a status report, the high bit of byte 2 will be clear
            if ((buf[2] & 0x80) == 0)
                return buf;
        }

        // we didn't get a status report after several tries, so give up
        return null;
    }

    // report ID for sending commands to the Pinscape unit
    const int CMD_REPORT_ID = 0;

    // send a special request to the device - message type 65 with subtype 'id'
    public bool SpecialRequest(byte id)
    {
        byte[] buf = new byte[9];
        buf[0] = CMD_REPORT_ID;
        buf[1] = 0x41;  // 0x41 -> Pinscape special request
        buf[2] = id;    // special request type
        return WriteUSB(buf);
    }

    // send a special request to the device, with additional parameter bytes
    // specified by 'args'
    public bool SpecialRequest(byte id, byte[] args)
    {
        // set up the message
        byte[] buf = new byte[9];
        buf[0] = CMD_REPORT_ID;
        buf[1] = 0x41;  // 0x41 -> Pinscape special request
        buf[2] = id;    // special request type
        for (int i = 0; i < args.Length && i < 6; ++i)
            buf[i + 3] = args[i];

        // send the request
        return WriteUSB(buf);
    }

    // Send a special request to the device, awaiting a reply.  'match' tests
    // each reply to see if it's the one matching the request.  On success,
    // sets 'reply' to the first matching reply and returns true.  Returns
    // false on failure.
    public delegate bool MatchReply(byte[] reply);
    public byte[] SpecialRequest(byte id, byte[] args, MatchReply match)
    {
        // set up the message
        byte[] buf = new byte[9];
        buf[0] = CMD_REPORT_ID;
        buf[1] = 0x41;  // 0x41 -> Pinscape special request
        buf[2] = id;    // special request type
        for (int i = 0; i < args.Length && i < 6; ++i)
            buf[i + 3] = args[i];

        // Since we're going to await a reply, clear out any buffered
        // inputs on the endpoint.  The joystick endpoint continuously
        // sends joystick reports, so we want to discard these ahead
        // of time.
        FlushReadUSB();

        // send the request
        if (WriteUSB(buf))
        {
            // await the reply
            for (int i = 0 ; i < 16 ; ++i)
            {
                // read a reply
                byte[] r = ReadUSB();
                if (r != null && match(r))
                    return r;
            }
        }

        // didn't get a matching reply
        return null;
    }

    // query a configuration variable
    public byte[] QueryConfigVar(byte id)
    {
        // send special request 9 <id>, get response 00 98 <id>
        byte[] buf = SpecialRequest(9, new byte[] { id },
            (r) => r[1] == 0 && r[2] == 0x98 && r[3] == id);

        // if successful, strip out the prefix data
        if (buf != null)
            Array.Copy(buf, 4, buf, 0, buf.Length - 4);

        // return the result
        return buf;
    }

    // query an array configuration variable
    public byte[] QueryConfigVar(byte id, byte index)
    {
        // send special request 9 <id> <index>, get response 00 98 <id> <index>
        byte[] buf = SpecialRequest(9, new byte[] { id, index },
            (r) => r[1] == 0 && r[2] == 0x98 && r[3] == id && r[4] == index);

        // if successful, strip out the prefix data
        if (buf != null)
            Array.Copy(buf, 5, buf, 0, buf.Length - 5);

        // return the result
        return buf;
    }

    // write a configuration variable from a preformatted buffer
    public bool SetConfigVar(byte[] data)
    {
        // give the device a moment to digest input
        Thread.Sleep(2);

        // set up the Set Config Var command: 0x42 <varnum> <valueData>
        byte[] buf = new byte[9];
        buf[0] = CMD_REPORT_ID;
        buf[1] = 0x42;      // Set Config Variable command code
        for (var i = 0; i < data.Length && i < 7; ++i)
            buf[i + 2] = data[i];

        // write the command
        return WriteUSB(buf);
    }

    // write a configuration variable; returns true on success, false on failure
    public bool SetConfigVar(byte varID, byte[] data)
    {
        return SetConfigVar(new byte[] { varID }.Concat(data));
    }

    // write an array config variable
    public bool SetConfigVar(byte varID, byte arrIdx, byte[] data)
    {
        return SetConfigVar(new byte[] { varID, arrIdx }.Concat(data));
    }

    // Convert a USB pin ID byte to a string pin name
    static public String WireToPinName(int n)
    {
        // special value 0xFF means NC (not connected)
        if (n == 0xFF)
            return "NC";

        // the high 3 bits are the port name (A-E = 0-4)
        char port = (char)('A' + ((n >> 5) & 0x07));

        // the low 5 bits are the pin number
        int pin = n & 0x1F;

        // if the port name is invalid, return NC
        if (port > 'E')
            return "NC";

        // build the name - PT<port><pin>
        return "PT" + port + pin;
    }

    // Convert a pin name to a USB wire byte
    static public byte PinNameToWire(String name)
    {
        // NC is special - maps to 0xFF
        if (name == "NC")
            return 0xff;

        // otherwise, parse the PT<port><pin>
        Match m = Regex.Match(name, @"PT([A-E])(\d\d?)");
        if (m.Success)
        {
            return (byte)(
                ((m.Groups[1].Value.ToCharArray()[0] - 'A') << 5)
                | (int.Parse(m.Groups[2].Value) & 0x1F));
        }
        else
            return 0xFF;
    }

    // query the CPU ID
    public String QueryCPUID()
    {
        return QueryDeviceIDString(1);
    }

    // query the OpenSDA ID
    public String QueryOpenSDAID()
    {
        return QueryDeviceIDString(2);
    }

    // query an identifier string
    //   1 = KL25Z CPU ID
    //   2 = OpenSDA TUID
    public String QueryDeviceIDString(byte index)
    {
        // send the device ID query and await the response (0x00 0x90 ...)
        byte[] buf = SpecialRequest(7, new byte[] { index },
            (r) => r[1] == 0x00 && r[2] == 0x90 && r[3] == index);

        // if we got the response, decode the CPU ID
        if (buf != null)
        {
            return String.Format("{0:X2}{1:X2}", buf[4], buf[5])
                + String.Format("-{0:X2}{1:X2}{2:X2}{3:X2}", buf[6], buf[7], buf[8], buf[9])
                + String.Format("-{0:X2}{1:X2}{2:X2}{3:X2}", buf[10], buf[11], buf[12], buf[13]);
        }
        else
        {
            // no response from the device
            return null;
        }
    }

    // Query the device software build ID.  On success, we fill in 'dd' with
    // the build date (a decimal value encoded as YYYYMMDD) and 'tt' with the
    // build time (a decimal value encoded as HHMMSS on a 24-hour clock). 
    // These give the timestamp when the firmware was compiled, which serves
    // as a build identifier.  Also fills in 's' with the same information
    // formatted as a string "YYYYMMDDHHMM" (leaving off the seconds, since
    // it's more precision than is necessary).  Returns false on failure.
    public bool QueryBuildID(out int dd, out int tt, out String s)
    {
        // send the build ID query and await the response (0x00 0xA0 ...)
        byte[] buf = SpecialRequest(10, new byte[] { },
            (r) => r[1] == 0x00 && r[2] == 0xA0);

        // check for failure
        if (buf == null)
        {
            dd = tt = 0;
            s = null;
            return false;
        }

        // decode the date and time values
        dd = buf[3] + (buf[4] << 8) + (buf[5] << 16) + (buf[6] << 24);
        tt = buf[7] + (buf[8] << 8) + (buf[9] << 16) + (buf[10] << 24);

        // format a string as YYYYMMDDHHMM (note that we omit the seconds from the time)
        s = String.Format("{0:D4}{1:D2}{2:D2}", dd / 10000 % 10000, dd / 100 % 100, dd % 100);
        s += String.Format("{0:D2}{1:D2}", tt / 10000 % 100, tt / 100 % 100);
        return true;
    }

    public bool WriteUSB(byte[] buf)
    {
        for (int tries = 0; tries < 3; ++tries)
        {
            unsafe
            {
                // write the data - the file handle is in overlapped mode, so we have to do 
                // this as an overlapped write with an OVERLAPPED structure and an event to
                // monitor for completion
                Overlapped o = new Overlapped(0, 0, evov.SafeWaitHandle.DangerousGetHandle(), null);
                NativeOverlapped* no = o.Pack(null, null);
                IntPtr hbuf = Marshal.AllocHGlobal(buf.Length);
                try
                {
                    Marshal.Copy(buf, 0, hbuf, buf.Length);
                    HIDImports.WriteFile(fp, hbuf, buf.Length, IntPtr.Zero, no);

                    // wait briefly for the write to complete
                    if (evov.WaitOne(250))
                    {
                        // successful completion - get the result
                        UInt32 actual;
                        int result = HIDImports.GetOverlappedResult(fp, no, out actual, 0);

                        Overlapped.Unpack(no);
                        Overlapped.Free(no);

                        if (result == 0)
                        {
                            // the write failed - try re-opening the handle and go back
                            // for another try
                            TryReopenHandle();
                            continue;
                        }
                        else if (actual != buf.Length)
                        {
                            // length is wrong - the write failed
                            return false;
                        }
                        else
                        {
                            // success
                            return true;
                        }
                    }
                    else
                    {
                        // The write timed out.  Cancel the write and try reopening the handle.
                        HIDImports.CancelIo(fp);
                        Overlapped.Unpack(no);
                        Overlapped.Free(no);
                        if (TryReopenHandle())
                            continue;
                        return false;
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(hbuf);
                }
            }
        }

        // maximum retries exceeded - return failure
        return false;
    }

    public IntPtr fp;
    public string path;
    public string name;
    public ushort vendorID;
    public ushort productID;
    public ushort version;
    public int LedWizUnitNo;
    public int PinscapeUnitNo;
    public bool PlungerEnabled;
    public bool JoystickEnabled;
    public bool TvOnEnabled;
    public bool isValid;
    public string CPUID;
    public string OpenSDAID;
    public int BuildDD, BuildTT;
    public string BuildID;
    public int inputReportLength;
}
