namespace Universe.Bitmap
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BITMAPFILEHEADER
    {
        public short bfType;
        public int bfSize;
        public short bfReserved1;
        public short bfReserved2;
        public int bfOffBits;

        public override string ToString()
        {
            return string.Format("BITMAPFILEHEADER:{5}BfType: {0},{5}BfSize: {1},{5}BfReserved1: {2},{5}BfReserved2: {3},{5}BfOffBits: {4}", bfType,
                bfSize, bfReserved1, bfReserved2, bfOffBits,
                Environment.NewLine + "     ");
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BITMAPCOREHEADER
    {
        public uint bcSize;
        public ushort bcWidth;
        public ushort bcHeight;
        public ushort bcPlanes;
        public ushort bcBitCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BitmapInfoHeader
    {
        public int biSize;
        public int biWidth;
        public int biHeight;
        public short biPlanes; // 1
        public short biBitCount; //24|32
        public int biCompression; // 0: BMP
        public int biSizeImage; //0 for uncompressed
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public int biClrUsed;
        public int biClrImportant;

        public override string ToString()
        {
            return
                string.Format(
                    "Bitmap-Info-Header:{11}BiSize: {0},{11}BiWidth: {1},{11}BiHeight: {2},{11}BiPlanes: {3},{11}BiBitCount: {4},{11}BiCompression: {5},{11}BiSizeImage: {6},{11}BiXPelsPerMeter: {7},{11}BiYPelsPerMeter: {8},{11}BiClrUsed: {9},{11}BiClrImportant: {10}",
                    biSize, biWidth, biHeight, biPlanes, biBitCount, biCompression, biSizeImage, biXPelsPerMeter, biYPelsPerMeter, biClrUsed,
                    biClrImportant,
                    Environment.NewLine + "     ");
        }

/*
        public BitmapInfoHeader(Stream stream)
        {
            this = new BitmapInfoHeader();
            this.Read(stream);
        }


        public unsafe void Read(Stream stream)
        {
            var array = new byte[sizeof(BitmapInfoHeader)];
            stream.Read(array, 0, array.Length);
            fixed (byte* pData = array)
                this = *(BitmapInfoHeader*)pData;
        }

        public unsafe void Write(Stream stream)
        {
            var array = new byte[sizeof(BitmapInfoHeader)];
            fixed (BitmapInfoHeader* ptr = &this)
                Marshal.Copy((IntPtr)ptr, array, 0, sizeof(BitmapInfoHeader));

            stream.Write(array, 0, sizeof(BitmapInfoHeader));
        }
*/
    }
}