using System;

namespace Monster.ConsoleApp
{
    public class CommandLineFuncs
    {
        public static bool Confirm(string confirmMsg)
        {
            Console.WriteLine(
                Environment.NewLine + confirmMsg + Environment.NewLine + "Please type 'YES' to proceed");
            var input = Console.ReadLine();
            return input.Trim() == "YES";
        }
    }
}
