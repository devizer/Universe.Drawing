using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils.DumpPalette
{
    using System.IO;

    class Program
    {
        static void Main(string[] args)
        {
            string file = args[0];

            byte[] bytes = new byte[54+1024];
            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (FileStream fs2 = new FileStream(file + ".cs", FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            using (StreamWriter wr = new StreamWriter(fs2))
            {
                BinaryReader rdr = new BinaryReader(fs);
                fs.Seek(54, SeekOrigin.Begin);
                wr.WriteLine("byte[] palette = new byte[1024] {");
                for (int i = 0; i < 256; i++)
                {
                    var b = fs.ReadByte();
                    var g = fs.ReadByte();
                    var r = fs.ReadByte();
                    var a = fs.ReadByte();

                    wr.WriteLine("  (byte) 0x{0:X2},  (byte) 0x{1:X2},  (byte) 0x{2:X2},  (byte) 0x{3:X2},  {4}", 
                        b,g,r,a,
                        i%8==0 ? " // " + i.ToString("X2") : ""
                        );

                }
                wr.WriteLine("}");
            }

        }
    }
}
