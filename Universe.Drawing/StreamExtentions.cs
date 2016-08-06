namespace Universe.Bitmap
{
    using System.IO;

    internal class StreamExtentions
    {
        public static byte[] Read(Stream stream, int numberBytes, string context)
        {
            var ret = new byte[numberBytes];
            Read(stream, ret, context);
            return ret;
        }

        public static void Read(Stream stream, byte[] fullBuffer, string context)
        {
            var total = 0;
            var numberBytes = fullBuffer.Length;
            while (total < numberBytes)
            {
                var n = stream.Read(fullBuffer, total, numberBytes - total);
                if (n <= 0)
                    throw new BitmapFormatException("Invalid bitmap. " + context);

                total += n;
            }
        }

        public static byte[] JoinBytes(params byte[][] arrays)
        {
            var length = arrays[0].Length;
            for (var i = 1; i < arrays.Length; i++)
                length += arrays[i].Length;

            var p = 0;
            var ret = new byte[length];
            for (var i = 0; i < arrays.Length; i++)
            {
                var arr = arrays[i];
                var l = arr.Length;
                for (var j = 0; j < l; j++)
                    ret[p++] = arr[j];
            }

            return ret;
        }
    }
}