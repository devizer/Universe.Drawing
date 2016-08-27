namespace Universe.Drawing
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

        public static BITMAPFILEHEADER BE2LE(BITMAPFILEHEADER bfh)
        {
            BITMAPFILEHEADER ret = bfh;
            if (BitConverter.IsLittleEndian) return ret;
            ret.bfOffBits = IntReverter.Int32(ret.bfOffBits);
            ret.bfSize = IntReverter.Int32(ret.bfSize);
            ret.bfType = IntReverter.Int16(ret.bfType);
            return ret;
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

        public static BitmapInfoHeader BE2LE(BitmapInfoHeader infoHeader)
        {
            var ret = infoHeader;
            if (BitConverter.IsLittleEndian) return ret;
            ret.biSize = IntReverter.Int32(ret.biSize);
            ret.biWidth = IntReverter.Int32(ret.biWidth);
            ret.biPlanes = IntReverter.Int16(ret.biPlanes);
            ret.biBitCount = IntReverter.Int16(ret.biBitCount);
            ret.biHeight = IntReverter.Int32(ret.biHeight);
            ret.biCompression = IntReverter.Int32(ret.biCompression);
            ret.biSizeImage = IntReverter.Int32(ret.biSizeImage);
            ret.biXPelsPerMeter = IntReverter.Int32(ret.biXPelsPerMeter);
            ret.biYPelsPerMeter = IntReverter.Int32(ret.biYPelsPerMeter);
            ret.biClrUsed = IntReverter.Int32(ret.biClrUsed);
            ret.biClrImportant = IntReverter.Int32(ret.biClrImportant);
            return ret;
        }


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