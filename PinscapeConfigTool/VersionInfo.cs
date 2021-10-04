using System;

namespace PinscapeConfigTool
{
    class VersionInfo
    {
        public static string BuildNumber = "4036"; // #BuildNumber
        public static DateTime BuildDate = new DateTime(2000, 1, 1).AddDays(
            System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Build);
    }
}
