using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Universe.Bitmap
{
    using System.Diagnostics;
    using System.Net.Configuration;
    using System.Runtime.InteropServices;

    public static class AntiAliasing
    {

        unsafe public static Bitmap2 SimpleUpScale(Bitmap2 input, int number)
        {
            if (number < 2 || number > 8)
                throw new ArgumentOutOfRangeException("number");

            bool isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
            var bw = input.Width;
            var bh = input.Height;
            var format = input.Format;
            int bpp = format == PixelFormat2.Format32bppArgb ? 4 : 3;
            Bitmap2 output = new Bitmap2(bw * number, bh * number, format);
            Stopwatch sw = Stopwatch.StartNew();
            int block32PerLine = output.Stride/32;
            for (int y = 0; y < bh; y++)
            {
                // Coping one row
                if (bpp == 3)
                {
                    ThreeBytes* pSrc = (ThreeBytes*)(input.Scan0 + y * input.Stride);
                    ThreeBytes* pDest = (ThreeBytes*)(output.Scan0 + y * number * output.Stride);
                    for (int x = 0; x < bw; x++)
                    {
                        ThreeBytes pixel = *(pSrc++);
                        for (int nx = 0; nx < number; nx++)
                            *(pDest++) = pixel;
                    }
                }
                else
                {
                    Color2* pSrc = (Color2*)(input.Scan0 + y * input.Stride);
                    Color2* pDest = (Color2*)(output.Scan0 + y * number * output.Stride);
                    for (int x = 0; x < bw; x++)
                    {
                        Color2 pixel = *(pSrc++);
                        for (int nx = 0; nx < number; nx++)
                            *(pDest++) = pixel;
                    }
                }

                // coping 2nd, 3rd, 4th, etc
                for (int ny = 1; ny < number; ny++)
                {
                    IntPtr ptrSrc = (output.Scan0 + (y * number + 0) * output.Stride);
                    IntPtr ptrDest = ptrSrc + ny * output.Stride;

                    // copy 8 bytes a time
                    long* p32Src = (long*)ptrSrc;
                    long* p32Dest = (long*)ptrDest;
                    int numBytes = output.Stride;
                    for(; numBytes >= 8; numBytes-=8)
                        *(p32Dest++) = *(p32Src++);

                    // copy 4 bytes a time
                    int* pSrc = (int*) p32Src;
                    int* pDest = (int*) p32Dest;
                    for (; numBytes >= 4; numBytes -= 4)
                        *(pDest++) = *(pSrc++);
                }
            }

            Console.WriteLine("Upscale *{0} AA {1}x{2}: {3} msec", number, input.Width, input.Height, sw.ElapsedMilliseconds);
            return output;
        }

        unsafe public static Bitmap2 SimpleUpScale_Old(Bitmap2 bitmap, int number)
        {
            if (number < 2 || number > 8)
                throw new ArgumentOutOfRangeException("number");

            var bw = bitmap.Width;
            var bh = bitmap.Height;
            var format = bitmap.Format;
            int bpp = format == PixelFormat2.Format32bppArgb ? 4 : 3;
            Bitmap2 bmp = new Bitmap2(bw * number, bh * number, format);
            Stopwatch sw = Stopwatch.StartNew();
            for (int y = 0; y < bh; y++)
            {
                if (bpp == 3)
                {
                    for (int ny = 0; ny < number; ny++)
                    {
                        ThreeBytes* pSrc = (ThreeBytes*)(bitmap.Scan0 + y * bitmap.Stride);
                        ThreeBytes* pDest = (ThreeBytes*)(bmp.Scan0 + (y * number + ny) * bmp.Stride);
                        for (int x = 0; x < bw; x++)
                        {
                            ThreeBytes pixel = *(pSrc++);
                            for (int nx = 0; nx < number; nx++)
                                *(pDest++) = pixel;
                        }
                    }
                }
                else
                {
                    for (int ny = 0; ny < number; ny++)
                    {
                        Color2* pSrc = (Color2*)(bitmap.Scan0 + y * bitmap.Stride);
                        Color2* pDest = (Color2*)(bmp.Scan0 + (y * number + ny) * bmp.Stride);
                        for (int x = 0; x < bw; x++)
                        {
                            Color2 pixel = *(pSrc++);
                            for (int nx = 0; nx < number; nx++)
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
            int bpp = format == PixelFormat2.Format32bppArgb ? 4 : 3;
            // int[] sum = new int[bpp];
            int sumB, sumG, sumR, sumA;
            var width = bw / number;
            var height = bh / number;
            int divider = number * number;
            for (int y = 0; y < height; y++)
            {
                byte* pDest = (byte*)(output.Scan0 + y * output.Stride);
                for (int x = 0; x < width; x++)
                {
                    sumB = sumG = sumR = sumA = 0;
                    byte* pSrc = (byte*)(bitmap.Scan0 + y * number * bitmap.Stride + x * number * bpp);

                    for (int ny = 0; ny < number; ny++)
                    {
                        byte* pSrc2 = pSrc;
                        for (int nx = 0; nx < number; nx++)
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

            Bitmap2 output = new Bitmap2(bitmap.Width/number, bitmap.Height/number, bitmap.Format);
            SimpleDownscale(bitmap, number, output);
            return output;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ThreeBytes
        {
            public byte B;
            public byte G;
            public byte R;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ThirtyTwoBytes
        {
            public long P1;
            public long P2;
            public long P3;
            public long P4;
        }



    }
}
