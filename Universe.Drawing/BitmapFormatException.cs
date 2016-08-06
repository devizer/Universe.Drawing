namespace Universe.Bitmap
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class BitmapFormatException : Exception
    {
        public BitmapFormatException()
        {
        }

        public BitmapFormatException(string message) : base(message)
        {
        }

        public BitmapFormatException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BitmapFormatException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}