using CollectionUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
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
                    "Il s'agit de l'outil de commande Pinscape (PinscapeCmd), qui vous permet d'envoyer des \n "
                    + "séquences de commandes vers vos unités de contrôleur Pinscape. Ce programme est \n"
                    + "conçu comme un utilitaire de ligne de commande pour le rendre pratique à utiliser sous Windows \n"
                    + "scripts shell de commande (fichiers .CMD ou .BAT), pour vous aider à automatiser les tâches. \n"
                    + "\n"
                    + "Pour utiliser cet outil à partir de la ligne de commande, écrivez PinscapeCmd (le programme \n "
                    + "nom), puis suivez-le sur la même ligne avec un ou plusieurs des éléments suivants \n"
                    + "éléments: \n"
                    + "\n"
                    + "Unit=n : diriger les éléments qui suivent vers le numéro d'unité spécifié. 'n'\n"
                    + "est le numéro d'unité Pinscape que vous avez sélectionné dans la configuration, généralement 1 pour\n"
                    + "la première unité, 2 pour la seconde, etc. Si vous en avez plus d'une\n"
                    + "Unité Pinscape, vous DEVEZ l'inclure avant toute autre commande à\n"
                    + "dites au programme à quelle unité vous vous adressez. Si vous n'avez que\n"
                    + "une unité Pinscape dans votre système, ce n'est pas nécessaire, car il y a\n"
                    + "pas d'ambiguïté sur l'unité que vous souhaitez utiliser dans ce cas.\n"
                    + "\n"
                    + "NightMode = status: active ou désactive le mode nuit (remplacez 'state' par ON ou\n"
                    + "OFF selon l'état que vous souhaitez engager).\n"
                    + "\n"
                    + "TVON=mode : activer ou désactiver le relais TV, ou le pulser. 'mode' peut être activé sur\n"
                    + "activer le relais, OFF pour le désactiver ou PULSE pour impulser le relais pendant\n"
                    + "un moment (allumé puis éteint), de la même manière que le contrôleur pulse normalement\n"
                    + "au démarrage du système pour allumer les téléviseurs. Si aucun mode n'est fourni,\n"
                    + "c'est la même chose que TVON = PULSE. Cette commande ne fonctionne que si la fonction TV ON\n"
                    + "est activé. Cela n'affecte que le relais; il n'envoie aucune commande IR.\n"
                    + "Pour ce faire, utilisez SendIR = n. N"
                    + "\n"
                    + "SendIR=n : transmettre la commande de télécommande IR #n, en utilisant la commande\n"
                    + "numérotation dans l'outil de configuration.\n"
                    + "\n"
                    + "Silencieux: ne faites pas de pause avant la fin du programme. Le programme attend normalement\n"
                    + "pour que vous appuyiez sur une touche avant de quitter lorsque vous l'exécutez à partir de Windows\n"
                    + "bureau, pour vous permettre de voir tous les messages affichés avant la console\n"
                    + "La fenêtre se ferme. Si vous exécutez le programme via un\n"
                    + "processus automatisé, tel qu'un script de commande ou un raccourci de démarrage Windows, utilisez\n"
                    + "cette option pour que le programme se termine immédiatement une fois terminé."
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
                    devices.Count == 0 ? "Aucune unité Pinscape n'est présente dans votre système" :
                    "Plusieurs unités Pinscape sont présentes dans votre système, vous devez donc spécifier\n"
                    + "celui auquel vous voulez vous adresser. Écrivez \"unit=n\", où 'n' est le Pinscape\n"
                    + "numéro d'unité de l'unité à laquelle vous vous adressez, avant tout autre élément du\n"
                    + "liste de commandes.");

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
                            throw new Exception("Le numéro d'unité spécifié par \"" + a + "\" n'est pas present.\n"
                                + (devices.Count == 1 ? "La seule unité actuellement présente est #" : "Les unités suivantes sont actuellement présentes: ")
                                + devices.Select(d => d.PinscapeUnitNo).SerialJoin()
                                + ".");
                    }
                    else if (al == "unit")
                    {
                        throw new Exception("l' \"unité\" d'argument n'a pas le numéro d'unité. Écrire cela\n"
                        + "par \"unit=n\", où 'n' est le numéro d'unité Pinscape que vous souhaitez adresser.");
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
                                + "optins valide.\n"
                                + "Veuillez spécifier NightMode = ON ou NightMode = OFF.");
                    }
                    else if (al == "nightmode")
                    {
                        throw new Exception("La commande \"NightMode\" nécessite un paramètre ON ou OFF. Écrire\n"
                            + "comme ceci NightMode=ON ou NightMode=OFF.");
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
                            throw new Exception("Le mode TV ON ne fait pas partie des options valides. Veuillez préciser\n"
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
                            throw new Exception("Une erreur s'est produite lors de l'obtention du nombre d'emplacements infrarouges sur votre Pinscape\n"
                                + "unité. Vous avez peut-être installé un micrologiciel plus ancien qui ne prend pas en charge les télécommandes IR.");

                        // validate the slot number
                        if (n < 1 || n > buf[0])
                            throw new Exception("Le numéro d'emplacement de commande IR dans \"" + a + "\" n'est pas dans le "
                                + "Plage valide\n"
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
                        throw new Exception("La commande \"SendIR\" est incomplète. Écrivez ceci comme SendIR=n,\n"
                            + "où n est l'emplacement de commande IR (en utilisant la numérotation dans la configuration de l'appareil).");
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
                System.Console.Write("\n[ Appuyez sur une touche pour quitter (utilisez l'option SILENCIEUX pour sauter la prochaine fois) ]");
                System.Console.ReadKey();
            }
        }
    }
}
