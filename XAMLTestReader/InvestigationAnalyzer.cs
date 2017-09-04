using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XAMLTestReader
{
    public class InvestigationAnalyzer
    {
        public static List<string> GetFailingTests(string path)
        {
            List<string> tests = new List<string>();

            foreach (string img in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
            {
                Console.WriteLine(img);
            }

            return null;
        }
    }
}
