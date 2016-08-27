namespace Universe.Drawing
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography.X509Certificates;

    public partial class BitmapWriter
    {
        public static unsafe void Write(Bitmap2 bitmap, Stream output)
        {
            var bfh = new BITMAPFILEHEADER
            {
                bfType = 0x4D42,
                bfSize = sizeof(BITMAPFILEHEADER) + sizeof(BitmapInfoHeader) + bitmap.Stride * bitmap.Height,
                bfOffBits = sizeof(BITMAPFILEHEADER) + sizeof(BitmapInfoHeader)
            };

            var bih = new BitmapInfoHeader
            {
                biSize = sizeof(BitmapInfoHeader),
                biBitCount = (short)(bitmap.Format == PixelFormat2.Format32bppArgb ? 32 : 24),
                biCompression = 0,
                biPlanes = 1,
                biWidth = bitmap.Width,
                biHeight = bitmap.Height,
                biSizeImage = 0,
                biXPelsPerMeter = bitmap.PpmX,
                biYPelsPerMeter = bitmap.PpiY,
                biClrUsed = 0,
                biClrImportant = 0
            };

            {
                var copy = BITMAPFILEHEADER.BE2LE(bfh);
                var ptr = &copy;
                var array = new byte[sizeof(BITMAPFILEHEADER)];
                Marshal.Copy((IntPtr)ptr, array, 0, array.Length);
                output.Write(array, 0, array.Length);
            }

            {
                var copy = BitmapInfoHeader.BE2LE(bih);
                var ptr2 = &copy;
                var array = new byte[sizeof(BitmapInfoHeader)];
                Marshal.Copy((IntPtr)ptr2, array, 0, array.Length);
                output.Write(array, 0, array.Length);
            }

            bitmap.WritePixels(output);
        }


        public static unsafe void WriteGrayScale8Bpp(Bitmap2 bitmap, Stream output)
        {
            WriteGrayScale8Bpp(bitmap, output, GrayScaleFlavour.Human);
        }
        
        public static unsafe void WriteGrayScale8Bpp(Bitmap2 bitmap, Stream output, GrayScaleFlavour flavour)
        {
            Stopwatch sw = Stopwatch.StartNew();
            const int paletteLength = 4*256;
            var bfh = new BITMAPFILEHEADER
            {
                bfType = 0x4D42,
                bfSize = sizeof (BITMAPFILEHEADER) + sizeof (BitmapInfoHeader) + bitmap.Stride*bitmap.Height,
                bfOffBits = sizeof (BITMAPFILEHEADER) + sizeof (BitmapInfoHeader) + paletteLength
            };

            var bih = new BitmapInfoHeader
            {
                biSize = sizeof (BitmapInfoHeader),
                biBitCount = (short) 8,
                biCompression = 0,
                biPlanes = 1,
                biWidth = bitmap.Width,
                biHeight = bitmap.Height,
                biSizeImage = 0,
                biXPelsPerMeter = bitmap.PpmX,
                biYPelsPerMeter = bitmap.PpiY,
                biClrUsed = 256,
                biClrImportant = 0
            };

            {
                var copy = BITMAPFILEHEADER.BE2LE(bfh);
                var ptr = &copy;
                var array = new byte[sizeof(BITMAPFILEHEADER)];
                Marshal.Copy((IntPtr) ptr, array, 0, array.Length);
                output.Write(array, 0, array.Length);
            }

            {
                var copy = BitmapInfoHeader.BE2LE(bih);
                var ptr2 = &copy;
                var array = new byte[sizeof (BitmapInfoHeader)];
                Marshal.Copy((IntPtr) ptr2, array, 0, array.Length);
                output.Write(array, 0, array.Length);
            }

            byte[] palette = new byte[paletteLength];
            int pos = 0;
            int posSepia = 0;
            for (int i = 0; i <= 255; i++)
            {
                if (flavour != GrayScaleFlavour.Sepia)
                {
                    byte b = (byte) i;
                    palette[pos++] = b;
                    palette[pos++] = b;
                    palette[pos++] = b;
                    palette[pos++] = 255;
                }
                else
                {
                    palette[pos++] = _sepia[posSepia++];
                    palette[pos++] = _sepia[posSepia++];
                    palette[pos++] = _sepia[posSepia++];
                    palette[pos++] = 255;
                }
            }
            output.Write(palette, 0, paletteLength);


            if (bitmap.Scan0 == IntPtr.Zero) Bitmap2.ThrowODE();
            var width = bitmap.Width;
            var height = bitmap.Height;
            var format = bitmap.Format;
            var stride = width%4 != 0 ? ((width >> 2) + 1) << 2 : width;
            var buffer = new byte[stride];
            for (int x = 0; x < 4 && x < width; x++) buffer[width - 1 - x] = 255;
            var ptrSrcLine = bitmap.Scan0;
            for (int y = 0; y < height; y++)
            {
                byte* ptrSrcPixel = (byte*) ptrSrcLine;
                for (int x = 0; x < width; x++)
                {
                    byte b = *(ptrSrcPixel++);
                    byte g = *(ptrSrcPixel++);
                    byte r = *(ptrSrcPixel++);
                    if (format == PixelFormat2.Format32bppArgb) ptrSrcPixel++;
                    byte pixel;
                    if (flavour == GrayScaleFlavour.Human)
                        pixel = (byte)((2989 * (int)r + 5870 * (int)g + 1140 * (int)b) / 10000);
                    else if (flavour == GrayScaleFlavour.Mathematical)
                        pixel = (byte) ((((int) b) + ((int) g) + ((int) r))/3);
                    else
                        pixel = FindSepiaIndex(r, g, b);

                    buffer[x] = pixel;
                }

                output.Write(buffer, 0, stride);
                ptrSrcLine += bitmap.Stride;
            }

            Console.WriteLine("Write 8 bit " + flavour + " of " + bitmap.Width + "x" + bitmap.Height + ": " + sw.Elapsed);

        }



    }

    public enum GrayScaleFlavour
    {
        Human,
        Mathematical,
        Sepia,
    }
}