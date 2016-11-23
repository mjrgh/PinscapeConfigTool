using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

// IncBuildNumber - auto build number generator tool
//
// This is meant to be used as a pre-build step in Visual Studio projects.  It uses
// two files: a flat text file containing simply a build number as a decimal string,
// and a program source file (C, C++, C#) containing a static data definition for
// the build number, flagged with a comment, like this:
//
//     const char *build_number = "1"; // #BuildNumber    <-- C or C++
//     String BuildNumber = "1";       // #BuildNumber    <-- C#
//
// The key is a "//" comment containing the string "#BuildNumber" on the same line
// as a variable initializer expression containing a string with digits.  We find the
// build number itself by searching for the pattern "(\d+)" (the quotes are part of the
// pattern), so the build number string must be the first string on the line containing
// only digits.  It's okay to have other initializer expressions and other strings on
// the same line, as long as none of the strings contain nothing but digits, but to
// avoid fragility it's best to keep the line to a single initializer.
//
namespace IncBuildNumber
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: IncBuildNumber <flat text file.txt> <source file.{c,cpp,cs}>");
                return;
            }

            // read the text file containing the version number, parsing the number as an int,
            // and bump it up by one
            int buildNum = int.Parse(File.ReadAllText(args[0])) + 1;

            // rewrite the text file with the new verison number
            File.WriteAllText(args[0], buildNum.ToString());

            // load the source file, patch the version number string initializer, 
            // and rewrite the file
            File.WriteAllText(
                args[1],
                Regex.Replace(
                    File.ReadAllText(args[1]),
                    @"(?m)""\d+""(.*#BuildNumber)",
                    new MatchEvaluator(m => "\"" + buildNum + "\"" + m.Groups[1].Value)
                )
            );
        }
    }
}
