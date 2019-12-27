using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using CollectionUtils;
using System.Runtime.InteropServices;
using System.Threading;

namespace PinscapeCmd
{
    class Program
    {
        static bool helpShown = false;
        static void Help()
        {
            // show help only once, even if the multiple args request it
            if (!helpShown)
            {
                helpShown = true;
                System.Console.WriteLine(
                    "This is the Pinscape Command tool (PinscapeCmd), which lets you send special\n"
                    + "command sequences to your Pinscape Controller unit(s).  This program is\n"
                    + "designed as a command-line utility to make it convenient to use in Windows\n"
                    + "command shell scripts (.CMD or .BAT files), to help you automate tasks.\n"
                    + "\n"
                    + "To use this tool from the command line, write PinscapeCmd (the program\n"
                    + "name), then follow it on the same line with one or more of the following\n"
                    + "items:\n"
                    + "\n"
                    + "Unit=n : direct the items that follow to the specified unit number.  'n'\n"
                    + "    is the Pinscape unit number you selected in the setup, usually 1 for\n"
                    + "    the first unit, 2 for the second, etc.  If you have more than one\n"
                    + "    Pinscape unit, you MUST include this before any other commands to\n"
                    + "    tell the program which unit you're addressing.  If you only have\n"
                    + "    one Pinscape unit in your system, this isn't needed, since there's\n"
                    + "    no ambiguity about which unit you want to use in this case.\n"
                    + "\n"
                    + "NightMode=state : turn night mode ON or OFF (replace 'state' with ON or\n"
                    + "    OFF according to which state you want to engage).\n"
                    + "\n"
                    + "TVON=mode : turn the TV relay on or off, or pulse it.  'mode' can be ON to\n"
                    + "    turn the relay on, OFF to turn it off, or PULSE to pulse the relay for\n"
                    + "    a moment (on and then off), the same way the controller normally pulses\n"
                    + "    it at system startup to turn on the TVs.  If no mode is supplied at all,\n"
                    + "    it's the same as TVON=PULSE.  This command only works if the TV ON feature\n"
                    + "    is enabled.  This only affects the relay; it doesn't send any IR commands.\n"
                    + "    To do that, use SendIR=n.\n"
                    + "\n"
                    + "SendIR=n : transmit IR remote control command #n, using the command\n"
                    + "    numbering in the setup tool.\n"
                    + "\n"
                    + "Quiet : don't pause before the program exits.  The program normally waits\n"
                    + "    for you to press a key before exiting when you run it from the Windows\n"
                    + "    desktop, to allow you to see any messages displayed before the console\n"
                    + "    window closes.  If you're running the program through an automated\n"
                    + "    process, such as a command script or a Windows startup shortcut, use\n"
                    + "    this option to make the program exit immediately when finished."
                );
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern uint GetConsoleProcessList(
            uint[] ProcessList,
            uint ProcessCount
        );
        
        static void Main(string[] args)
        {
            // If we're the only process in our console process group, it
            // means that the console window will close automatically as soon
            // as the process exits.  This happens if we're launched from the
            // desktop shell, for example, rather than via a CMD window.  In
            // this case, pause before exiting so that the user can see our
            // message output.
            uint[] processList = new uint[2];
            bool pause = (GetConsoleProcessList(processList, 2) == 1);

            try
            {
                // find the Pinscape devices
                List<DeviceInfo> devices = DeviceInfo.FindDevices();

                // if there's only one device, make it the default; otherwise,
                // the command line will have to select one explicitly
                DeviceInfo device = devices.Count == 1 ? devices[0] : null;
                Exception missingDevice = new Exception(
                    devices.Count == 0 ? "No Pinscape units are present in your system." :
                    "Multiple Pinscape units are present in your system, so you have to specify\n"
                    + "which one you want to address.  Write \"unit=n\", where 'n' is the Pinscape\n"
                    + "unit number of the unit you're addressing, before any other item in the\n"
                    + "command list.");

                // show help if there aren't any arguments
                if (args.Length == 0)
                    Help();

                // parse and execute the commands in the argument list
                for (int i = 0; i < args.Length; ++i)
                {
                    Match m;
                    String a = args[i], al = a.ToLower();
                    if ((m = Regex.Match(al, @"^unit=(.+)")).Success)
                    {
                        // find the device
                        int n;
                        if (!int.TryParse(m.Groups[1].Value, out n)) n = -1;
                        if ((device = devices.FirstOrDefault(d => d.PinscapeUnitNo == n)) == null)
                            throw new Exception("The unit number specified by \"" + a + "\" isn't present.\n"
                                + (devices.Count == 1 ? "The only unit currently present is #" : "The following units are currently present: ")
                                + devices.Select(d => d.PinscapeUnitNo).SerialJoin()
                                + ".");
                    }
                    else if (al == "unit")
                    {
                        throw new Exception("The \"unit\" argument is missing the unit number.  Write this\n"
                        + "as \"unit=n\", where 'n' is the Pinscape unit number you want to address.");
                    }
                    else if ((m = Regex.Match(al, @"^nightmode=(.+)")).Success)
                    {
                        String s = m.Groups[1].Value.ToLower();
                        if (device == null)
                            throw missingDevice;
                        if (s == "on")
                            device.SpecialRequest(8, new byte[] { 1 });
                        else if (s == "off")
                            device.SpecialRequest(8, new byte[] { 0 });
                        else
                            throw new Exception("The NightMode setting \"" + a + "\" isn't one of the "
                                + "valid options.\n"
                                + "Please specify NightMode=ON or NightMode=OFF.");
                    }
                    else if (al == "nightmode")
                    {
                        throw new Exception("The \"NightMode\" command requires an ON or OFF parameter.  Write\n"
                            + "this as NightMode=ON or NightMode=OFF.");
                    }
                    else if (al == "tvon")
                    {
                        if (device == null)
                            throw missingDevice;
                        device.SpecialRequest(11, new byte[] { 2 });
                    }
                    else if ((m = Regex.Match(al, @"^tvon=(.+)")).Success)
                    {
                        String s = m.Groups[1].Value.ToLower();
                        if (device == null)
                            throw missingDevice;
                        if (s == "on")
                            device.SpecialRequest(11, new byte[] { 1 });
                        else if (s == "off")
                            device.SpecialRequest(11, new byte[] { 0 });
                        else if (s == "pulse")
                            device.SpecialRequest(11, new byte[] { 2 });
                        else
                            throw new Exception("The TVON mode isn't one of the valid options.  Please specify\n"
                                + "TVON=ON, TVON=OFF, or TVON=PULSE.");
                    }
                    else if ((m = Regex.Match(al, @"^sendir=(.+)")).Success)
                    {
                        // we need a device for this operation
                        if (device == null)
                            throw missingDevice;

                        // get the IR slot number
                        int n;
                        if (!int.TryParse(m.Groups[1].Value, out n)) n = -1;

                        // get the number of IR slots
                        byte[] buf = device.QueryConfigVar(252, 0);
                        if (buf == null)
                            throw new Exception("An error occurred getting the number of IR slots on your Pinscape\n"
                                + "unit.  You might have older firmware installed that doesn't support IR remotes.");

                        // validate the slot number
                        if (n < 1 || n > buf[0])
                            throw new Exception("The IR command slot number in \"" + a + "\" isn't within the "
                                + "valid range\n"
                                + "(1 to " + buf[0] + ").");

                        // Pause briefly in case an earlier IR command was transmitted.
                        // Note that we know whether or not we transmitted any earlier
                        // commands ourselves, but we wait unconditionally anyway, because
                        // the user could have sent a command via a separate invocation
                        // of this program just before we started.  The unconditional
                        // wait will handle that case.  Of course, we can't rule out
                        // other reasons the IR transmitter is busy, such as the user
                        // pressing IR-mapped cabinet buttons.  But the worst that
                        // happens is that the command gets dropped, so our main goal
                        // here is to avoid interfering with our own transmissions
                        // rather than every possible source of contention.  Most IR
                        // commands in most protocols complete within 100ms, so 250ms
                        // should give us reasonable confidence that any previous
                        // command has finished.
                        Thread.Sleep(250);

                        // transmit the command
                        device.SpecialRequest(17, new byte[] { (byte)n });
                    }
                    else if (al == "sendir")
                    {
                        throw new Exception("The \"SendIR\" command is incomplete.  Write this as SendIR=n,\n"
                            + "where n is the IR command slot (using the numbering in the device setup).");
                    }
                    else if (Regex.IsMatch(al, @"/?(help|\?)"))
                    {
                        Help();
                    }
                    else if (al == "quiet")
                    {
                        // quiet mode - disable the exit pause
                        pause = false;
                    }
                    else
                    {
                        throw new Exception("Unknown command: \"" + a + "\"");
                    }
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
            }

            // pause before exiting, if desired
            if (pause)
            {
                System.Console.Write("\n[ Press a key to exit (use the QUIET option to skip this next time) ]");
                System.Console.ReadKey();
            }
        }
    }
}
