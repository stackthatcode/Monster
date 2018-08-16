using System.IO;

namespace Monster.ConsoleApp.TestJson
{
    public class TestLoader
    {
        public static string GimmeJson(string fileName)
        {
            return File.ReadAllText($@".\TestJson\{fileName}");
        }
    }
}
