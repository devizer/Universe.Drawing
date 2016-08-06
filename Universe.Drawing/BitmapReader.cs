namespace Universe.Bitmap
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;

    public class BitmapReader
    {
        public static unsafe Bitmap2 Read(Stream input)
        {
            var arrFileHeader = StreamExtentions.Read(input, sizeof (BITMAPFILEHEADER),
                string.Format("BITMAP-FILE-HEADER structure with length {0} is expected",
                    sizeof (BITMAPFILEHEADER)));

            BITMAPFILEHEADER bfh;
            fixed (void* ptrFileHeader = arrFileHeader)
            {
                bfh = (BITMAPFILEHEADER) Marshal.PtrToStructure((IntPtr) ptrFileHeader, typeof (BITMAPFILEHEADER));
            }

            if (bfh.bfType != 0x4D42)
                throw new BitmapFormatException("Invalid bitmap. 'BM' magic header is expected");

            Debug.WriteLine(bfh);

            var arrInfoHeaderSize = StreamExtentions.Read(input, 4, "BITMAP-INFO-HEADER's header size is expected");
            var infoHeaderSize = BitConverter.ToInt32(arrInfoHeaderSize, 0);
            Debug.WriteLine("  BITMAP-INFO-HEADER's header size is " + infoHeaderSize);

            if (infoHeaderSize == sizeof (BitmapInfoHeader))
            {
                // v3
                var arrInfoHeader2 = StreamExtentions.Read(input, sizeof (BitmapInfoHeader) - 4, "BITMAP-INFO-HEADER, which length is 40, is expected");
                var arrInfoHeader = StreamExtentions.JoinBytes(arrInfoHeaderSize, arrInfoHeader2);
                BitmapInfoHeader infoHeader;
                fixed (void* ptrInfoHeader = arrInfoHeader)
                {
                    infoHeader = (BitmapInfoHeader) Marshal.PtrToStructure((IntPtr) ptrInfoHeader, typeof (BitmapInfoHeader));
                    Debug.WriteLine(infoHeader);
                }

                var sourceBpp = infoHeader.biBitCount;
                PixelFormat2 format;
                if (sourceBpp == 24)
                    format = PixelFormat2.Format24bppRgb;
                else if (sourceBpp == 32)
                    format = PixelFormat2.Format32bppArgb;
                else
                {
                    throw new NotSupportedException("Reader supports bitmaps with either 24 or 32 bits per pixel. Bitmap's BPP is " + sourceBpp);
                }

                var pos = sizeof (BITMAPFILEHEADER) + sizeof (BitmapInfoHeader);
                var paletteLength = bfh.bfOffBits - pos;
                if (paletteLength > 0)
                {
                    var arrPalette = StreamExtentions.Read(input, paletteLength,
                        "Bitmap palette (length=" + paletteLength + ") is expected at position = " + pos);

                    pos += paletteLength;
                }

                var ret = new Bitmap2(infoHeader.biWidth, infoHeader.biHeight, format);
                ReadPixels(input, pos, ret);
                return ret;
            }

            throw new NotSupportedException("Bitmaps with BITMAP-INFO-HEADER length!=40 (v3) are not supported. Header length is " + infoHeaderSize);

/*
CORE 12 	BITMAPCOREHEADER, 95/NT 3.1 и старше, CE 2.0/Mobile 5.0 и старше, Содержит только ширину, высоту и битность растра.
v3 	 40 	BITMAPINFOHEADER, 95/NT 3.1 и старше, CE 1.0/Mobile 5.0 и старше, Содержит ширину, высоту и битность растра, а также формат пикселей, информацию о цветовой таблице и разрешении.
v4 	 108 	BITMAPV4HEADER 	  95/NT 4.0 и старше  не поддерживается 	Отдельно выделены маски каналов, добавлена информация о цветовом пространстве и гамме.
v5 	 124 	BITMAPV5HEADER 	  98/2000 и старше 	  не поддерживается 	Добавлено указание предпочтительной стратегии отображения и поддержка профилей ICC.
*/
        }

        private static void ReadPixels(Stream stream, int pos, Bitmap2 bitmap)
        {
            var stride = bitmap.Stride;
            var buffer = new byte[stride];
            var len = stride*bitmap.Height;
            var p = bitmap.Scan0;
            for (var y = bitmap.Height - 1; y >= 0; y--)
            {
                StreamExtentions.Read(stream, buffer, "Pixels (" + stride + " bytes) of line " + y + " (stream offset = " + pos + ") is expected");
                Marshal.Copy(buffer, 0, p, stride);
                len -= stride;
                p += stride;
                pos += stride;
            }
        }
    }
}