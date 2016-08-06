namespace Universe.Bitmap
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;

    public enum PixelFormat2
    {
        Format24bppRgb = 137224,
        Format32bppArgb = 2498570
    }

    public class Bitmap2 : IDisposable
    {
        public PixelFormat2 Format;
        public int Height;

        public int PpmX;
        public int PpmY;
        public IntPtr Scan0;
        public int Stride;
        public int Width;

        public Bitmap2()
        {
            PpmX = PpmX = 11811;
        }

        public Bitmap2(int width, int height, PixelFormat2 format)
        {
            Width = width;
            Height = height;
            Format = format;
            Stride = GetStride(width, format);

            Scan0 = Marshal.AllocHGlobal(Stride*height);
        }

        public int PpiX
        {
            get { return (int) ((PpmX*127L + 2500)/5000); }
            set { PpmX = (int) ((value*5000L + 64)/127); }
        }

        public int PpiY
        {
            get { return (int) ((PpmY*127L + 2500)/5000); }
            set { PpmY = (int) ((value*5000L + 64)/127); }
        }

        public unsafe Color2 this[int x, int y]
        {
            get
            {
                if (Scan0 == IntPtr.Zero) ThrowODE();
                var ptr = Scan0 + Stride*(Height - y - 1) + (Format == PixelFormat2.Format32bppArgb ? x*4 : x*3);
                var p = (byte*) ptr;
                var b = *(p++);
                var g = *(p++);
                var r = *(p++);
                var a = Format == PixelFormat2.Format32bppArgb ? *p : byte.MaxValue;
                return new Color2 {R = r, G = g, B = b, A = a};
            }
            set
            {
                if (Scan0 == IntPtr.Zero) ThrowODE();
                var ptr = Scan0 + Stride*(Height - y - 1) + (Format == PixelFormat2.Format32bppArgb ? x*4 : x*3);
                var p = (byte*) ptr;
                *(p++) = value.B;
                *(p++) = value.G;
                *(p++) = value.R;
                if (Format == PixelFormat2.Format32bppArgb)
                    *p = value.A;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

/*
        public Bitmap2(Stream source)
        {
            var BitmapReader.Read(source);

        }
*/

        public BitmapData2 Lock()
        {
            if (Scan0 == IntPtr.Zero) ThrowODE();
            return new BitmapData2(Width, Height, Format, Stride, this);
        }

        public unsafe void ClearHorizontalLine(int y, Color2 color)
        {
            if (Scan0 == IntPtr.Zero) ThrowODE();
            var ptr = Scan0 + Stride*(Height - y - 1);
            var p = (byte*) ptr;
            for (var x = 0; x < Width; x++)
            {
                *(p++) = color.B;
                *(p++) = color.G;
                *(p++) = color.R;
                if (Format == PixelFormat2.Format32bppArgb)
                    *(p++) = color.A;
            }
        }

        internal void WritePixels(Stream stream)
        {
            if (Scan0 == IntPtr.Zero) ThrowODE();
            var buffer = new byte[8192];
            var len = Stride*Height;
            var p = Scan0;
            while (len > 0)
            {
                var n = Math.Min(len, buffer.Length);
                Marshal.Copy(p, buffer, 0, n);
                stream.Write(buffer, 0, n);
                len -= n;
                p += n;
            }
        }

        internal void ReadPixels(Stream stream)
        {
            var buffer = new byte[8192];
            var len = Stride*Height;
            var p = Scan0;
            while (len > 0)
            {
                var n = stream.Read(buffer, 0, Math.Min(len, Stride));
                Marshal.Copy(buffer, 0, p, n);
                len -= n;
                p += n;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            var copy = Interlocked.Exchange(ref Scan0, IntPtr.Zero);
            if (copy != IntPtr.Zero)
                Marshal.FreeHGlobal(copy);

            if (disposing)
            {
            }
        }

        ~Bitmap2()
        {
            Dispose(false);
        }

        private static int GetStride(int width, PixelFormat2 format)
        {
            var s = width*(format == PixelFormat2.Format32bppArgb ? 4 : 3);
            if (s%4 != 0) s = ((s >> 2) + 1) << 2;
            return s;
        }

        private static void ThrowODE()
        {
            var name = typeof (Bitmap2).Name;
            throw new ObjectDisposedException(name + " Disposed. Memory released", name);
        }
    }

    public class BitmapData2
    {
        private readonly Bitmap2 Bitmap;

        public BitmapData2(int width, int height, PixelFormat2 format, int stride, Bitmap2 bitmap)
        {
            Width = width;
            Height = height;
            Format = format;
            Stride = stride;
            Bitmap = bitmap;
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public PixelFormat2 Format { get; private set; }
        public int Stride { get; private set; }

        public IntPtr Scan0
        {
            get { return Bitmap.Scan0; }
        }

        public IntPtr GetPointerByLine(int y)
        {
            return Scan0 + Stride*y;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Color2
    {
        public byte B;
        public byte G;
        public byte R;
        public byte A;

        public Color2(byte r, byte g, byte b, byte a)
        {
            B = b;
            G = g;
            R = r;
            A = a;
        }

        public Color2(byte r, byte g, byte b)
        {
            B = b;
            G = g;
            R = r;
            A = 255;
        }
    }
}