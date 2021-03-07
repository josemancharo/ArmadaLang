using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmadaLang.Compiler
{
    public class ArmadaFileLoader
    {
        public static StreamReader GetTextReaderForFile(string file)
        {
            var bytes = File.ReadAllBytes(file);
            var stream = new MemoryStream(bytes);
            var reader = new StreamReader(stream);
            return reader;
        }
    }
}
