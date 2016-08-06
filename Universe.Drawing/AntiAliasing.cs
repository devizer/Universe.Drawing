namespace Universe.Drawing
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public static class AntiAliasing
    {
        public static unsafe Bitmap2 SimpleUpScale(Bitmap2 input, int number)
        {
            if (number < 2 || number > 8)
                throw new ArgumentOutOfRangeException("number");

            var isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
            var bw = input.Width;
            var bh = input.Height;
            var format = input.Format;
            var bpp = format == PixelFormat2.Format32bppArgb ? 4 : 3;
            var output = new Bitmap2(bw*number, bh*number, format);
            var sw = Stopwatch.StartNew();
            var block32PerLine = output.Stride/32;
            for (var y = 0; y < bh; y++)
            {
                // Coping one row
                if (bpp == 3)
                {
                    var pSrc = (ThreeBytes*) (input.Scan0 + y*input.Stride);
                    var pDest = (ThreeBytes*) (output.Scan0 + y*number*output.Stride);
                    for (var x = 0; x < bw; x++)
                    {
                        var pixel = *(pSrc++);
                        for (var nx = 0; nx < number; nx++)
                            *(pDest++) = pixel;
                    }
                }
                else
                {
                    var pSrc = (Color2*) (input.Scan0 + y*input.Stride);
                    var pDest = (Color2*) (output.Scan0 + y*number*output.Stride);
                    for (var x = 0; x < bw; x++)
                    {
                        var pixel = *(pSrc++);
                        for (var nx = 0; nx < number; nx++)
                            *(pDest++) = pixel;
                    }
                }

                // coping 2nd, 3rd, 4th, etc
                for (var ny = 1; ny < number; ny++)
                {
                    var ptrSrc = (output.Scan0 + (y*number + 0)*output.Stride);
                    var ptrDest = ptrSrc + ny*output.Stride;

                    // copy 8 bytes a time
                    var p32Src = (long*) ptrSrc;
                    var p32Dest = (long*) ptrDest;
                    var numBytes = output.Stride;
                    for (; numBytes >= 8; numBytes -= 8)
                        *(p32Dest++) = *(p32Src++);

                    // copy 4 bytes a time
                    var pSrc = (int*) p32Src;
                    var pDest = (int*) p32Dest;
                    for (; numBytes >= 4; numBytes -= 4)
                        *(pDest++) = *(pSrc++);
                }
            }

            Console.WriteLine("Upscale *{0} AA {1}x{2}: {3} msec", number, input.Width, input.Height, sw.ElapsedMilliseconds);
            return output;
        }

        public static unsafe Bitmap2 SimpleUpScale_Old(Bitmap2 bitmap, int number)
        {
            if (number < 2 || number > 8)
                throw new ArgumentOutOfRangeException("number");

            var bw = bitmap.Width;
            var bh = bitmap.Height;
            var format = bitmap.Format;
            var bpp = format == PixelFormat2.Format32bppArgb ? 4 : 3;
            var bmp = new Bitmap2(bw*number, bh*number, format);
            var sw = Stopwatch.StartNew();
            for (var y = 0; y < bh; y++)
            {
                if (bpp == 3)
                {
                    for (var ny = 0; ny < number; ny++)
                    {
                        var pSrc = (ThreeBytes*) (bitmap.Scan0 + y*bitmap.Stride);
                        var pDest = (ThreeBytes*) (bmp.Scan0 + (y*number + ny)*bmp.Stride);
                        for (var x = 0; x < bw; x++)
                        {
                            var pixel = *(pSrc++);
                            for (var nx = 0; nx < number; nx++)
                                *(pDest++) = pixel;
                        }
                    }
                }
                else
                {
                    for (var ny = 0; ny < number; ny++)
                    {
                        var pSrc = (Color2*) (bitmap.Scan0 + y*bitmap.Stride);
                        var pDest = (Color2*) (bmp.Scan0 + (y*number + ny)*bmp.Stride);
                        for (var x = 0; x < bw; x++)
                        {
                            var pixel = *(pSrc++);
                            for (var nx = 0; nx < number; nx++)
                                *(pDest++) = pixel;
                        }
                    }
                }
            }

            Console.WriteLine("Upscale *{0} AA {1}x{2}: {3} msec", number, bitmap.Width, bitmap.Height, sw.ElapsedMilliseconds);
            return bmp;
        }


        public static unsafe Bitmap2 SimpleDownscale(Bitmap2 bitmap, int number, Bitmap2 output)
        {
            if (number < 2 || number > 8)
                throw new ArgumentOutOfRangeException("number");

            if (output.Width * number != bitmap.Width || output.Height * number != bitmap.Height)
                throw new ArgumentException("Output bitmap size should conform input bitmap size and AA scaling");

            if (output.Format != bitmap.Format)
                throw new ArgumentException("Output and input bitmaps should have same pixel format");

            var bw = bitmap.Width;
            var bh = bitmap.Height;
            if (bw % number != 0 || bh % number != 0)
                throw new ArgumentException("Please use the same number as per UpScale method");

            var format = bitmap.Format;
            var bpp = format == PixelFormat2.Format32bppArgb ? 4 : 3;
            // int[] sum = new int[bpp];
            int sumB, sumG, sumR, sumA;
            var width = bw / number;
            var height = bh / number;
            var divider = number * number;
            var strideSource = bitmap.Stride;
            var strideOutput = output.Stride;
            for (var y = 0; y < height; y++)
            {
                var pDest = (byte*)(output.Scan0 + y * strideOutput);
                var pSrc0 = (byte*)(bitmap.Scan0 + y * number * strideSource);
                for (var x = 0; x < width; x++)
                {
                    sumB = sumG = sumR = sumA = 0;
                    var pSrc = pSrc0 + x*number*bpp;

                    for (byte ny = 0; ny < number; ny++)
                    {
                        byte* pSrc2 = pSrc;
                        for (byte nx = 0; nx < number; nx++)
                        {
                            if (bpp == 3)
                            {
                                ThreeBytes pixel = *(ThreeBytes*)pSrc2;
                                sumB += pixel.B;
                                sumG += pixel.G;
                                sumR += pixel.R;
                                pSrc2 += 3;
                            }

                            else
                            {
                                Color2 pixel = *(Color2*)pSrc2;
                                sumB += pixel.B;
                                sumG += pixel.G;
                                sumR += pixel.R;
                                sumA += pixel.A;
                                pSrc2 += 4;
                            }
                        }
                        

                        pSrc += strideSource;
                    }

                    if (bpp == 3)
                    {
                        ThreeBytes pixel = new ThreeBytes()
                        {
                            B = (byte) (sumB/divider),
                            G = (byte) (sumG/divider),
                            R = (byte) (sumR/divider)
                        };

                        *(ThreeBytes*) pDest = pixel;
                        pDest += 3;
                    }
                    else
                    {
                        Color2 pixel = new Color2()
                        {
                            B = (byte) (sumB/divider),
                            G = (byte) (sumG/divider),
                            R = (byte) (sumR/divider),
                            A = (byte) (sumA/divider),
                        };

                        *(Color2*)pDest = pixel;
                        pDest += 4;
                    }
/*
                    *(pDest++) = (byte)(sumB / divider);
                    *(pDest++) = (byte)(sumG / divider);
                    *(pDest++) = (byte)(sumR / divider);
                    if (format == PixelFormat2.Format32bppArgb)
                        *(pDest++) = (byte)(sumA / divider);
*/
                }
            }

            return output;
        }

        public static unsafe Bitmap2 SimpleDownscale_Old(Bitmap2 bitmap, int number, Bitmap2 output)
        {
            if (number < 2 || number > 8)
                throw new ArgumentOutOfRangeException("number");

            if (output.Width * number != bitmap.Width || output.Height * number != bitmap.Height)
                throw new ArgumentException("Output bitmap size should conform input bitmap size and AA scaling");

            if (output.Format != bitmap.Format)
                throw new ArgumentException("Output and input bitmaps should have same pixel format");

            var bw = bitmap.Width;
            var bh = bitmap.Height;
            if (bw % number != 0 || bh % number != 0)
                throw new ArgumentException("Please use the same number as per UpScale method");

            var format = bitmap.Format;
            var bpp = format == PixelFormat2.Format32bppArgb ? 4 : 3;
            // int[] sum = new int[bpp];
            int sumB, sumG, sumR, sumA;
            var width = bw / number;
            var height = bh / number;
            var divider = number * number;
            for (var y = 0; y < height; y++)
            {
                var pDest = (byte*)(output.Scan0 + y * output.Stride);
                for (var x = 0; x < width; x++)
                {
                    sumB = sumG = sumR = sumA = 0;
                    var pSrc = (byte*)(bitmap.Scan0 + y * number * bitmap.Stride + x * number * bpp);

                    for (var ny = 0; ny < number; ny++)
                    {
                        var pSrc2 = pSrc;
                        for (var nx = 0; nx < number; nx++)
                        {
                            sumB += *(pSrc2++);
                            sumG += *(pSrc2++);
                            sumR += *(pSrc2++);
                            if (format == PixelFormat2.Format32bppArgb) sumA += *(pSrc2++);
                        }

                        pSrc += bitmap.Stride;
                    }

                    *(pDest++) = (byte)(sumB / divider);
                    *(pDest++) = (byte)(sumG / divider);
                    *(pDest++) = (byte)(sumR / divider);
                    if (format == PixelFormat2.Format32bppArgb)
                        *(pDest++) = (byte)(sumA / divider);
                }
            }

            return output;
        }

        public static Bitmap2 SimpleDownscale(Bitmap2 bitmap, int number)
        {
            if (number < 2 || number > 8)
                throw new ArgumentOutOfRangeException("number");

            if (bitmap.Width%number != 0 || bitmap.Height%number != 0)
                throw new ArgumentException("Please use the same number as per UpScale method");

            var output = new Bitmap2(bitmap.Width/number, bitmap.Height/number, bitmap.Format);
            SimpleDownscale(bitmap, number, output);
            return output;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct ThreeBytes
        {
            public byte B;
            public byte G;
            public byte R;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct ThirtyTwoBytes
        {
            public readonly long P1;
            public readonly long P2;
            public readonly long P3;
            public readonly long P4;
        }
    }
}