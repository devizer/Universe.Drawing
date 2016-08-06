namespace Universe.Bitmap
{
    using System.IO;

    internal class StreamExtentions
    {
        public static byte[] Read(Stream stream, int numberBytes, string context)
        {
            byte[] ret = new byte[numberBytes];
            Read(stream, ret, context);
            return ret;
        }

        public static void Read(Stream stream, byte[] fullBuffer, string context)
        {
            int total = 0;
            var numberBytes = fullBuffer.Length;
            while (total < numberBytes)
            {
                int n = stream.Read(fullBuffer, total, numberBytes - total);
                if (n <= 0)
                    throw new BitmapFormatException("Invalid bitmap. " + context);

                total += n;
            }
        }

        public static byte[] JoinBytes(params byte[][] arrays)
        {
            int length = arrays[0].Length;
            for (int i = 1; i < arrays.Length; i++)
                length += arrays[i].Length;

            int p = 0;
            byte[] ret = new byte[length];
            for (int i = 0; i < arrays.Length; i++)
            {
                var arr = arrays[i];
                int l = arr.Length;
                for (int j = 0; j < l; j++)
                    ret[p++] = arr[j];
            }

            return ret;
        }
    }
}