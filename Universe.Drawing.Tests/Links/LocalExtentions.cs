using System;
using System.Diagnostics;
using System.IO;

namespace Universe.Drawing.Tests.Links
{
    class LocalExtentions
    {
        public static FileStream CreateCompressedFile(string fullPath)
        {
            File.WriteAllBytes(fullPath, new byte[0]);

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                ProcessStartInfo si = new ProcessStartInfo("compact", "/c \"" + fullPath + "\"");
                si.UseShellExecute = false;
                si.CreateNoWindow = true;
                using (var p = Process.Start(si))
                {
                    p.WaitForExit();
                }
            }

            FileStream fs = new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite, 16384);
            return fs;
        }

    }
}
