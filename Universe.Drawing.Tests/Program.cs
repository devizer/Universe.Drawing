using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Universe.Drawing.Tests
{
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Runtime.Remoting.Metadata.W3cXsd2001;

    using TinyGZip;

    class Program
    {
        static void Main(string[] args)
        {
            PrepareDownscleProfiler();
            Run4();
            return;
            Run1();
            Run2();
            Run3();
        }

        private static void PrepareDownscleProfiler()
        {
            {
                Bitmap2 bmp = new Bitmap2(1, 1, PixelFormat2.Format24bppRgb);
                AntiAliasing.SimpleDownscale(AntiAliasing.SimpleUpScale(bmp, 2), 2, bmp);
                AntiAliasing.SimpleDownscale(AntiAliasing.SimpleUpScale(bmp, 3), 3, bmp);
            }

            Bitmap2 bmp2;
            using (FileStream fs = new FileStream("photo24bpp.bmp", FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                bmp2 = BitmapReader.Read(fs);
            }

            int scale =
                Environment.OSVersion.Platform == PlatformID.Win32NT
                    ? 8
                    : 3;

            var working = AntiAliasing.SimpleUpScale(bmp2, scale);
            ProfileDownscale(bmp2, working, scale);

        }

        private static void ProfileDownscale(Bitmap2 plain, Bitmap2 aa, int number)
        {
            int n = 5;
            long sum = 0;
            for (int i = 0; i < n; i++)
            {
                Stopwatch sw = Stopwatch.StartNew();
                AntiAliasing.SimpleDownscale(aa, number, plain);
                var elapsed = sw.ElapsedMilliseconds;
                sum += elapsed;
                Console.WriteLine("Downscale {0}x{1} by *{2}: {3} msec", aa.Width, aa.Height, number, elapsed);
            }

            Console.WriteLine("Average is {0} msec", sum /n );
        }


        private static void Run4()
        {
            var aaMax = Environment.OSVersion.Platform == PlatformID.Win32NT ? 8 : 6;
            for(int w = 8; w>=0; w--)
            for (int aaScale = aaMax; aaScale >= 1; aaScale--)
            {
                Bitmap2 bmp;
                using (FileStream fs = new FileStream("photo24bpp.bmp", FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    bmp = BitmapReader.Read(fs);
                }

                string name = "Ellipse-AA" + aaScale + "-" + w + "W.bmp";
                Console.WriteLine(name);
                string nameDraft = "Ellipse-Draft-AA" + aaScale + "-" + w + "W.bmp";
                using (Graphics2 g = new Graphics2(bmp, aaScale))
                {
                    const int r = 400;
                    var xCenter = bmp.Width / 2f;
                    var yCenter = bmp.Height / 2f;
                    for (int rotation = 30; rotation <= 150; rotation += 60)
                    {
                        g.Reset()
                            .SetCenter(xCenter, yCenter)
                            .SetRotation(rotation)
                            .DrawEllipse(0, 0, 600, 150, 0, 360, new Color2(43, 107, 39), w);
                    }

                    for (int angle = 0; angle < 360; angle += 60)
                    {
                        var localX = xCenter + 1.78f*r*Math.Cos(angle/360f*2f*Math.PI);
                        var localY = yCenter + 1.78f*r*Math.Sin(angle/360f*2f*Math.PI);

                        for (int rotation = 0; rotation <= 90; rotation += 90)
                        {
                            g.Reset()
                                .SetCenter((float)localX, (float)localY)
                                .SetRotation(rotation + angle)
                                .DrawEllipse(0, 0, r, 111, 0, 360, new Color2(43, 107, 39), w);
                        }

                        
                    }

                    int marginH = 130;
                    int marginV = 99;
                    for (int dx = -1; dx <= 1; dx += 2)
                        for (int dy = -1; dy <= 1; dy += 2)
                        {
                            float x = bmp.Width / 2f + (bmp.Width / 2f - marginH) * dx;
                            float y = bmp.Height / 2f + (bmp.Height / 2f - marginV) * dy;
                            for (int rotation = 45; rotation <= 135; rotation += 90)
                            {
                                g.Reset()
                                    .SetCenter(x, y)
                                    .SetRotation(rotation)
                                    // .DrawEllipse(0, 0, marginV / 1.4142f, marginV / 1.4142f/2, 0, 360, new Color2(43, 38, 107), w);
                                    .DrawEllipse(0, 0, marginV / 1.4142f, marginV / 1.4142f/2, 0, 360, new Color2(255,255,255), w);

                            }

                            for (int rotation = 45; rotation <= 135; rotation += 90)
                            {
                                g.Reset()
                                    .SetCenter(x, y)
                                    .SetRotation(rotation)
                                    .FillEllipse(0, 0, marginV / 1.4142f - w / 2f + 0.5f, marginV / 1.4142f / 2 - w / 2f + 0.5f, new Color2(43, 38, 107));
                                    

                            }

                        }

                    using(FileStream fs = new FileStream(nameDraft, FileMode.Create, FileAccess.Write))
                        BitmapWriter.Write(g.WorkingBitmap, fs);
                }

                using (FileStream fs = new FileStream(name, FileMode.Create, FileAccess.Write))
                    BitmapWriter.Write(bmp, fs);

                // return;

            }
        }

        private static void Run3()
        {
            Bitmap b = new Bitmap("photo-jpeg.jpg");
            MemoryStream mem = new MemoryStream();
            b.Save(mem, ImageFormat.Bmp);
            mem.Position = 0;

            Bitmap2 bmp = BitmapReader.Read(mem);
            DrawSomething(bmp);

            using (FileStream fs = new FileStream("photo-jpeg-transformed.bmp", FileMode.Create, FileAccess.Write))
                BitmapWriter.Write(bmp, fs);

            using (FileStream fs = new FileStream("photo-jpeg-transformed.bmp.gz", FileMode.Create, FileAccess.Write))
            using (var gzipped = GZipExtentions.CreateFastestCompressor(fs))
                BitmapWriter.Write(bmp, gzipped);

        }

        private static void Run2()
        {
            Bitmap2 bmp;
            using (FileStream fs = new FileStream("photo24bpp.bmp", FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                bmp = BitmapReader.Read(fs);
            }

            DrawSomething(bmp);
            using (FileStream fs = new FileStream("photo24bpp-transformed.bmp", FileMode.Create, FileAccess.Write))
                BitmapWriter.Write(bmp, fs);

            using (FileStream fs = new FileStream("photo24bpp-transformed.bmp.gz", FileMode.Create, FileAccess.Write))
            using (var gzipped = GZipExtentions.CreateFastestCompressor(fs))
                BitmapWriter.Write(bmp, gzipped);

        }

        static void Run1()
        {
            foreach(var format in new[] {PixelFormat2.Format24bppRgb, PixelFormat2.Format32bppArgb} )
            {
                for (int dx = 1; dx <= 4; dx++)
                {
                    Bitmap2 bmp = new Bitmap2(801+dx, 601+dx, format);
                    Console.WriteLine("Width: {0}, Format: {1}", bmp.Width, format);

                    Clear(bmp, new Color2(200, 220, 255));
                    DrawSomething(bmp);
                    using (FileStream fs = new FileStream("1st-" + bmp.Width + "W-" + format + ".bmp", FileMode.Create, FileAccess.Write))
                        BitmapWriter.Write(bmp, fs);

                    if (bmp.Width%2 == 0 && bmp.Height%2 == 0)
                    {
                        var bmpAA = AntiAliasing.SimpleDownscale(bmp, 2);
                        using (FileStream fs = new FileStream("1st-" + bmp.Width + "W-" + format + "-AA.bmp", FileMode.Create, FileAccess.Write))
                            BitmapWriter.Write(bmpAA, fs);
                        
                    }

                    using (FileStream fs = new FileStream("1st-" + bmp.Width + "W-" + format + ".bmp.gz", FileMode.Create, FileAccess.Write))
                    using (var gzipped = GZipExtentions.CreateFastestCompressor(fs))
                        BitmapWriter.Write(bmp, gzipped);

                    Graphics2 g = new Graphics2(bmp);
                    var bmp3 = AntiAliasing.SimpleUpScale(bmp, 5);
                    using (FileStream fs = new FileStream("1st-" + bmp.Width + "W-" + format + "-x5.bmp", FileMode.Create, FileAccess.Write))
                        BitmapWriter.Write(bmp3, fs);
                }
            }

        }

        private static void DrawSomething(Bitmap2 bmp)
        {
            FillRect(bmp, 13, 43, 321, 321, new Color2(58, 68, 255));
            DrawEllipse(bmp, 322, 166, 200, 40, 0, 270, new Color2(58, 200, 88));
        }

        static void DrawEllipse(Bitmap2 bitmap, float xCenter, float yCenter, float xRadius, float yRadius, float angleStart, float angleEnd, Color2 color)
        {
            int bw = bitmap.Width, bh = bitmap.Height;
            var maxRadius = Math.Max(xRadius, yRadius);
            double length2PiR = 2*Math.PI*maxRadius;
            float step = (float) (1/length2PiR);
            int count = (int) (length2PiR*1.01 + 1);
            float a0 = (float) (angleStart/2/Math.PI);
            float aStep = (float) (((angleEnd - angleStart)/360*2*Math.PI)/count);
            for (int i = 0; i <= count; i++)
            {
                float angle = a0 + i*aStep;
                var x = xCenter + Math.Cos(angle) * xRadius;
                var y = yCenter + Math.Sin(angle) * yRadius;
                int xi = (int) x, yi = (int) y;
                if (xi >= 0 && yi >= 0 && xi < bw && yi < bh)
                    bitmap[xi,yi] = color;
            }
        }

        static void FillRect(Bitmap2 bitmap, int left, int top, int width, int height, Color2 color)
        {
            int bw = bitmap.Width, bh = bitmap.Height;
            for (int y = top, ny = 0; y < bh && ny < height; y++, ny++)
                for (int x = left, nx = 0; x < bw && nx < width; x++, nx++)
                {
                    if (x>=0 && y>=0)
                        bitmap[x, y] = color;
                }
        }

        static void Clear(Bitmap2 bitmap, Color2 color)
        {
            int bw = bitmap.Width, bh = bitmap.Height;
            for(int y=0; y<bh; y++)
                bitmap.ClearHorizontalLine(y, color);
        }
    }
}
