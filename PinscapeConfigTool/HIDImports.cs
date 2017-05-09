//////////////////////////////////////////////////////////////////////////////////
//	HIDImports.cs
//	For more information: http://wiimotelib.codeplex.com/
//////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

public class HIDImports
{
    // Flags controlling what is included in the device information set built by SetupDiGetClassDevs
    internal const int DIGCF_DEFAULT = 0x00000001;       // only valid with DIGCF_DEVICEINTERFACE
    internal const int DIGCF_PRESENT = 0x00000002;
    internal const int DIGCF_ALLCLASSES = 0x00000004;
    internal const int DIGCF_PROFILE = 0x00000008;
    internal const int DIGCF_DEVICEINTERFACE = 0x00000010;

    [Flags]
    internal enum EFileAttributes : uint
    {
        Readonly = 0x00000001,
        Hidden = 0x00000002,
        System = 0x00000004,
        Directory = 0x00000010,
        Archive = 0x00000020,
        Device = 0x00000040,
        Normal = 0x00000080,
        Temporary = 0x00000100,
        SparseFile = 0x00000200,
        ReparsePoint = 0x00000400,
        Compressed = 0x00000800,
        Offline = 0x00001000,
        NotContentIndexed = 0x00002000,
        Encrypted = 0x00004000,
        Write_Through = 0x80000000,
        Overlapped = 0x40000000,
        NoBuffering = 0x20000000,
        RandomAccess = 0x10000000,
        SequentialScan = 0x08000000,
        DeleteOnClose = 0x04000000,
        BackupSemantics = 0x02000000,
        PosixSemantics = 0x01000000,
        OpenReparsePoint = 0x00200000,
        OpenNoRecall = 0x00100000,
        FirstPipeInstance = 0x00080000
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SP_DEVINFO_DATA
    {
        public int cbSize;
        public Guid ClassGuid;
        public uint DevInst;
        public IntPtr Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SP_DEVICE_INTERFACE_DATA
    {
        public int cbSize;
        public Guid InterfaceClassGuid;
        public int Flags;
        public IntPtr RESERVED;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct SP_DEVICE_INTERFACE_DETAIL_DATA
    {
        public UInt32 cbSize;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string DevicePath;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HIDD_ATTRIBUTES
    {
        public int Size;
        public ushort VendorID;
        public ushort ProductID;
        public ushort VersionNumber;
    }

    [DllImport(@"hid.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern void HidD_GetHidGuid(out Guid gHid);

    [DllImport("hid.dll")]
    internal static extern Boolean HidD_GetAttributes(IntPtr HidDeviceObject, ref HIDD_ATTRIBUTES Attributes);

    [DllImport("hid.dll")]
    internal extern static bool HidD_SetOutputReport(
            IntPtr HidDeviceObject,
            byte[] lpReportBuffer,
            uint ReportBufferLength);

    [DllImport("hid.dll")]
    internal extern static bool HidD_GetInputReport(
            IntPtr HidDeviceObject,
            byte[] lpREportBuffer,
            uint ReportBufferLength);

    [DllImport("hid.dll")]
    internal extern static bool HidD_GetPreparsedData(
            IntPtr HidDeviceObject,
            out IntPtr PreparsedData);

    [DllImport("hid.dll")]
    internal extern static bool HidD_FreePreparsedData(
            IntPtr PreparsedData);

    [StructLayout(LayoutKind.Sequential)]
    public struct HIDP_CAPS
    {
        public UInt16 Usage;
        public UInt16 UsagePage;
        public UInt16 InputReportByteLength;
        public UInt16 OutputReportByteLength;
        public UInt16 FeatureReportByteLength;
        public UInt16 Reserved0, Reserved1, Reserved2, Reserved3, Reserved4, Reserved5, Reserved6, Reserved7;
        public UInt16 Reserved8, Reserved9, Reserved10, Reserved11, Reserved12, Reserved13, Reserved14, Reserved15;
        public UInt16 Reserved16;
        public UInt16 NumberLinkCollectionNodes;
        public UInt16 NumberInputButtonCaps;
        public UInt16 NumberInputValueCaps;
        public UInt16 NumberInputDataIndices;
        public UInt16 NumberOutputButtonCaps;
        public UInt16 NumberOutputDataIndices;
        public UInt16 NumberFeatureButtonCaps;
        public UInt16 NumberFeatureValueCaps;
        public UInt16 NumberFeatureDataIndices;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HIDP_BUTTON_CAPS
    {
        public UInt16 Usage;
        public UInt16 UsagePage;
        public Byte IsAlias;
        public UInt16 BitField;
        public UInt16 LinkCollection;
        public UInt16 LinkUsage;
        public UInt16 LinkUsagePage;
        public Byte IsRange;
        public Byte IsStringRange;
        public Byte IsDesignatorRange;
        public Byte IsAbsolute;
        public UInt32 Reserved0, Reserved1, Reserved2, Reserved3, Reserved4, Reserved5, Reserved6, Reserved7, Reserved8, Reserved9;
        public UInt16 UsageMin;      // or Usage
        public UInt16 UsageMax;      // or Reserved
        public UInt16 StringMin;     // or StringIndex
        public UInt16 StringMax;     // or Reserved
        public UInt16 DesignatorMin; // or DesignatorIndex
        public UInt16 DesignatorMax; // or Reserved
        public UInt16 DataIndexMin;  // or DataIndex
        public UInt16 DataIndexMax;  // or Reserved
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HIDP_VALUE_CAPS
    {
        public UInt16 UsagePage;
        public Byte ReportID;
        public Byte IsAlias;
        public UInt16 BitField;
        public UInt16 LinkCollection;
        public UInt16 LinkUsage;
        public UInt16 LinkUsagePage;
        public Byte IsRange;
        public Byte IsStringRange;
        public Byte IsDesignatorRange;
        public Byte IsAbsolute;
        public Byte HasNull;
        public Byte Reserved;
        public UInt16 BitSize;
        public UInt16 ReportCount;
        public UInt16 Reserved2a, Reserved2b, Reserved2c, Reserved2d, Reserved2e;
        public UInt32 UnitsExp;
        public UInt32 Units;
        public Int32 LogicalMin;
        public Int32 LogicalMax;
        public Int32 PhysicalMin;
        public Int32 PhysicalMax;
        public UInt16 UsageMin;        // or UsageIndex
        public UInt16 UsageMax;        // or Reserved
        public UInt16 StringMin;       // or StringIndex
        public UInt16 StringMax;       // or Reserved
        public UInt16 DesignatorMin;   // or DesignatorIndex
        public UInt16 DesignatorMax;   // or Reserved
        public UInt16 DataIndexMin;    // or DataIndex
        public UInt16 DataIndexMax;    // or Reserved
    }

    // HIDP_REPORT_TYPE values
    public const int HidP_Input = 0;
    public const int HidP_Output = 1;
    public const int HidP_Feature = 2;
    
    [DllImport("hid.dll")]
    internal extern static UInt32 HidP_GetCaps(
            IntPtr PreparsedData,
            out HIDP_CAPS Capabilities);

    [DllImport("hid.dll")]
    internal extern static UInt32 HidP_GetButtonCaps(
            UInt32 ReportType,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] HIDP_BUTTON_CAPS[] ButtonCaps,
            ref UInt16 ButtonCapsLength,
            IntPtr PreparsedData);

    [DllImport("hid.dll")]
    internal extern static UInt32 HidP_GetValueCaps(
           UInt32 rportType,
           [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] HIDP_VALUE_CAPS[] ValueCaps,
           ref UInt16 ValueCapsLength,
           IntPtr PreparsedData);

    [DllImport("hid.dll")]
    internal extern static bool HidD_GetProductString(
            IntPtr HidDeviceObject,
            byte[] Buffer,
            uint BufferLength);

    [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern IntPtr SetupDiGetClassDevs(
            ref Guid ClassGuid,
            [MarshalAs(UnmanagedType.LPTStr)] string Enumerator,
            IntPtr hwndParent,
            UInt32 Flags);

    [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern Boolean SetupDiEnumDeviceInterfaces(
            IntPtr hDevInfo,
            ref SP_DEVINFO_DATA deviceInfoData,
            ref Guid interfaceClassGuid,
            UInt32 memberIndex,
            ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData);

    [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern Boolean SetupDiEnumDeviceInterfaces(
            IntPtr hDevInfo,
            IntPtr devInvo,
            ref Guid interfaceClassGuid,
            UInt32 memberIndex,
            ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData);

    [DllImport(@"setupapi.dll", SetLastError = true)]
    internal static extern Boolean SetupDiGetDeviceInterfaceDetail(
            IntPtr hDevInfo,
            ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
            IntPtr deviceInterfaceDetailData,
            UInt32 deviceInterfaceDetailDataSize,
            out UInt32 requiredSize,
            IntPtr deviceInfoData);

    [DllImport(@"setupapi.dll", SetLastError = true)]
    internal static extern Boolean SetupDiGetDeviceInterfaceDetail(
            IntPtr hDevInfo,
            ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
            ref SP_DEVICE_INTERFACE_DETAIL_DATA deviceInterfaceDetailData,
            UInt32 deviceInterfaceDetailDataSize,
            out UInt32 requiredSize,
            IntPtr deviceInfoData);

    [DllImport(@"setupapi.dll", SetLastError = true)]
    internal static extern Boolean SetupDiGetDeviceInterfaceDetail(
            IntPtr hDevInfo,
            ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
            ref SP_DEVICE_INTERFACE_DETAIL_DATA deviceInterfaceDetailData,
            UInt32 deviceInterfaceDetailDataSize,
            out UInt32 requiredSize,
            out SP_DEVINFO_DATA deviceInfoData);

    [DllImport(@"setupapi.dll", SetLastError = true)]
    internal static extern IntPtr SetupDiCreateDeviceInfoList(
            ref Guid ClassGuid,
            IntPtr hwndParent);

    [DllImport(@"setupapi.dll", SetLastError = true)]
    internal static extern Boolean SetupDiCreateDeviceInfo(
            IntPtr DeviceInfoSet,
            [MarshalAs(UnmanagedType.LPTStr)] string DeviceName,
            ref Guid ClassGuid,
            [MarshalAs(UnmanagedType.LPTStr)] string DeviceDescription,
            IntPtr hwndParent,
            UInt32 CreationFlags,
            IntPtr DevInfoData);

    [DllImport(@"setupapi.dll", SetLastError = true)]
    internal static extern Boolean SetupDiCreateDeviceInfo(
            IntPtr DeviceInfoSet,
            [MarshalAs(UnmanagedType.LPTStr)] string DeviceName,
            ref Guid ClassGuid,
            IntPtr DeviceDescription, // to allow pasing as null
            IntPtr hwndParent,
            UInt32 CreationFlags,
            IntPtr DevInfoData);


    [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern UInt16 SetupDiDestroyDeviceInfoList(IntPtr hDevInfo);

    [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern Boolean SetupDiRemoveDevice(
            IntPtr deviceInfoSet,
            ref SP_DEVINFO_DATA deviceInfoData);

    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    internal static extern IntPtr CreateFile(
            string fileName,
            [MarshalAs(UnmanagedType.U4)] UInt32 fileAccess,
            [MarshalAs(UnmanagedType.U4)] UInt32 fileShare,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] UInt32 creationDisposition,
            [MarshalAs(UnmanagedType.U4)] EFileAttributes flags,
            IntPtr template);

    // CreateFile access modes
    public const uint GENERIC_READ = 0x80000000;
    public const uint GENERIC_WRITE = 0x40000000;
    public const uint GENERIC_READ_WRITE = GENERIC_READ | GENERIC_WRITE;

    // CreateFile share modes
    public const uint SHARE_NONE = 0;
    public const uint SHARE_READ = 1;
    public const uint SHARE_WRITE = 2;
    public const uint SHARE_READ_WRITE = SHARE_READ | SHARE_WRITE;
    public const uint SHARE_DELETE = 4;

    // CreateFile dispositions
    public const uint CREATE_NEW = 1;
    public const uint CREATE_ALWAYS = 2;
    public const uint OPEN_EXISTING = 3;
    public const uint OPEN_ALWAYS = 4;
    public const uint TRUNCATE_EXISTING = 5;

    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    internal static extern bool WriteFile(
            IntPtr hFile,
            byte[] lpBuffer,
            UInt32 bytesToWrite,
            out UInt32 bytesWritten,
            IntPtr lpOverlapped);

    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    internal static extern bool WriteFile(
            IntPtr hFile,
            byte[] lpBuffer,
            UInt32 bytesToWrite,
            out UInt32 bytesWritten,
            ref System.Threading.NativeOverlapped lpOverlapped);

    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    internal static extern bool WriteFile(
            IntPtr hFile,
            byte[] lpBuffer,
            UInt32 bytesToWrite,
            IntPtr bytesWritten,
            ref System.Threading.NativeOverlapped lpOverlapped);

    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    internal unsafe static extern bool WriteFile(
            IntPtr hFile,
            IntPtr lpBuffer,
            Int32 bytesToWrite,
            IntPtr bytesWritten,
            System.Threading.NativeOverlapped* lpOverlapped);

    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    internal static extern bool ReadFile(
            IntPtr hFile,
            byte[] lpBuffer,
            UInt32 bytesToRead,
            out UInt32 bytesRead,
            ref System.Threading.NativeOverlapped lpOverlapped);

    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    internal static extern bool ReadFile(
            IntPtr hFile,
            byte[] lpBuffer,
            UInt32 bytesToRead,
            IntPtr bytesRead,
            ref System.Threading.NativeOverlapped lpOverlapped);

    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    internal static extern bool ReadFile(
            IntPtr hFile,
            IntPtr lpBuffer,
            UInt32 bytesToRead,
            IntPtr bytesRead,
            ref System.Threading.NativeOverlapped lpOverlapped);

    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    internal unsafe static extern bool ReadFile(
            IntPtr hFile,
            IntPtr lpBuffer,
            Int32 bytesToRead,
            IntPtr bytesRead,
            System.Threading.NativeOverlapped* lpOverlapped);

    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    internal static extern int FlushFileBuffers(
            IntPtr hFile);

    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    internal unsafe static extern int GetOverlappedResult(
            IntPtr hFile,
            System.Threading.NativeOverlapped* lpOverlapped,
            out UInt32 lpNumberOfBytesTransferred,
            Int32 bWait);

    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    internal static extern int CancelIo(
            IntPtr hFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CloseHandle(IntPtr hObject);
}
