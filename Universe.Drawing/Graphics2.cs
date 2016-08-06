namespace Universe.Bitmap
{
    using System;
    using System.Threading;

    public partial class Graphics2 : IDisposable
    {
        public readonly Bitmap2 Bitmap;
        private bool _HasRotation;


        private float _Rotation;

        // cache
        private float _RotationCos = 1;
        private float _RotationSin;
        public FloatPoint Center;
        public FloatPoint Scale;
        public int ScaleAA = 1;
        public Bitmap2 WorkingBitmap;

        protected Graphics2()
        {
            Center = new FloatPoint {H = 0, V = 0};
            Scale = new FloatPoint {H = 1, V = 1};
            Rotation = 0;
            ScaleAA = 1;
        }

        public Graphics2(Bitmap2 bitmap) : this(bitmap, 1)
        {
            Bitmap = bitmap;
            WorkingBitmap = bitmap;
            ScaleAA = 1;
        }

        public Graphics2(Bitmap2 bitmap, int scaleAA) : this()
        {
            ScaleAA = scaleAA;
            Bitmap = bitmap;
            WorkingBitmap =
                scaleAA == 1
                    ? bitmap
                    : AntiAliasing.SimpleUpScale(bitmap, scaleAA);
        }

        public float Rotation
        {
            get { return _Rotation; }
            set
            {
                _Rotation = value;
                var r = _Rotation*2*Math.PI/360f;
                _RotationCos = (float) Math.Cos(r);
                _RotationSin = (float) Math.Sin(r);
                _HasRotation = Math.Abs(_Rotation) > 1e-6;
            }
        }

        public void Dispose()
        {
            if (ScaleAA != 1)
            {
                var copy = Interlocked.Exchange(ref WorkingBitmap, null);
                if (copy != null)
                {
                    AntiAliasing.SimpleDownscale(copy, ScaleAA, Bitmap);
                    copy.Dispose();
                }
            }
        }

        public Graphics2 SetRotation(float rotation)
        {
            Rotation = rotation;
            return this;
        }

        public Graphics2 SetCenter(FloatPoint center)
        {
            Center = center;
            return this;
        }

        public Graphics2 SetCenter(float xCenter, float yCenter)
        {
            Center = new FloatPoint {H = xCenter, V = yCenter};
            return this;
        }

        public Graphics2 SetScale(FloatPoint scale)
        {
            Scale = scale;
            return this;
        }

        public Graphics2 Reset()
        {
            Rotation = 0;
            Scale = new FloatPoint {H = 1, V = 1};
            Center = new FloatPoint {H = 0, V = 0};
            return this;
        }

        private FloatPoint GetTransformed(FloatPoint arg)
        {
            return GetTransformed(arg.H, arg.V);
        }

        private FloatPoint GetTransformed(float x, float y)
        {
            if (_HasRotation)
            {
                var x1 = x*_RotationCos - y*_RotationSin;
                y = x*_RotationSin + y*_RotationCos;
                x = x1;
            }

            x = (x + Center.H)*Scale.H*ScaleAA;
            y = (y + Center.V)*Scale.V*ScaleAA;
            return new FloatPoint {H = x, V = y};
        }
    }

    public struct FloatPoint
    {
        public float H;
        public float V;
    }
}