namespace Universe.Bitmap
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    public class BitmapWriter
    {
        public static unsafe void Write(Bitmap2 bitmap, Stream output)
        {
            var bfh = new BITMAPFILEHEADER
            {
                bfType = 0x4D42,
                bfSize = sizeof (BITMAPFILEHEADER) + sizeof (BitmapInfoHeader) + bitmap.Stride*bitmap.Height,
                bfOffBits = sizeof (BITMAPFILEHEADER) + sizeof (BitmapInfoHeader)
            };

            var bih = new BitmapInfoHeader
            {
                biSize = sizeof (BitmapInfoHeader),
                biBitCount = (short) (bitmap.Format == PixelFormat2.Format32bppArgb ? 32 : 24),
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
                var ptr = &bfh;
                var array = new byte[sizeof (BITMAPFILEHEADER)];
                Marshal.Copy((IntPtr) ptr, array, 0, array.Length);
                output.Write(array, 0, array.Length);
            }

            {
                var ptr2 = &bih;
                var array = new byte[sizeof (BitmapInfoHeader)];
                Marshal.Copy((IntPtr) ptr2, array, 0, array.Length);
                output.Write(array, 0, array.Length);
            }

            bitmap.WritePixels(output);
        }
    }
}