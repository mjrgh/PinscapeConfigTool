using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace NightMode
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(String[] args)
        {
            // check for arguments - NightMode <PinscapeUnitNumber> ON|OFF
            if (args.Length == 2)
            {
                int unit;
                if (int.TryParse(args[0], out unit))
                {
                    String a = args[1].ToLower();
                    if (a == "on")
                    {
                        NightMode(unit, true);
                        return;
                    }
                    else if (a == "off")
                    {
                        NightMode(unit, false);
                        return;
                    }
                }
                Usage();
                return;
            }
            else if (args.Length != 0)
            {
                Usage();
                return;
            }


            // run the UI
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        static void NightMode(int unit, bool on)
        {
            // Get the device list
            List<DeviceInfo> list = DeviceInfo.FindDevices();

            // Look up the unit by Pinscape unit number.  Match on the raw unit number
            // or the DOF-adjusted unit number.  DOF adds 50 to the Pinscape unit number
            // (our unit 1 -> DOF 51, our unit 2 -> DOF 52, etc).
            DeviceInfo dev = list.Find(d => d.PinscapeUnitNo == unit || d.PinscapeUnitNo + 50 == unit);

            // If we didn't find a match, try looking it up by LedWiz unit number instead
            if (dev == null)
                dev = list.Find(d => d.LedWizUnitNo == unit);

            // if we didn't find a unit, return failure
            if (dev == null)
            {
                MessageBox.Show("Le numéro d'unité sélectionné (" + unit + ") n'a pas été trouvé.");
                return;
            }

            // send the NightMode request
            if (!dev.SpecialRequest(8, new byte[] { (byte)(on ? 1 : 0) }))
                MessageBox.Show("Une erreur s'est produite lors de l'envoi de la requête NightMode au KL25Z.");
        }

        static void Usage()
        {
            MessageBox.Show("Usage: NightMode <PinscapeUnitNumber> ON|OFF\r\n\r\n"
                + "Par exemple, \"NightMode 1 ON\" active le mode nuit sur l'unité Pinscape # 1.\r\n\r\n"
                + "Exécuter sans arguments pour sélectionner de manière interactive.");
        }
    }
}
