
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Universe.Drawing
{
    using System.Drawing.Drawing2D;

    partial class Graphics2
    {
        
        
        public void FillEllipse_Old(
            float xCenter, float yCenter,
            float xRadius, float yRadius,
            Color2 color)
        {

            var bitmap = WorkingBitmap;
            float dx = 1f / (ScaleAA * Scale.H);
            float dy = 1f / (ScaleAA * Scale.V);
            float xRadius2 = xRadius * xRadius;
            float yRadius2 = yRadius * yRadius;

            if (xRadius > yRadius)
            {
                for (float x = -xRadius; x < xRadius; x += dx)
                {
                    float y1 = (float) Math.Sqrt((1f - (x*x/xRadius2))*yRadius2);
                    float y2 = -y1;
                    DrawDraftLine(bitmap, x, y1, x, y2, color);
                }
            }
            else
            {
                for (float y = -yRadius; y < xRadius; y += dy)
                {
                    float x1 = (float) Math.Sqrt((1f - (y * y / yRadius2)) * xRadius2);
                    var x2 = -x1;
                    DrawDraftLine(bitmap, x1, y, x2, y, color);
                }
            }
        }

        private void DrawDraftLine(Bitmap2 bitmap, float x1, float y1, float x2, float y2, Color2 color)
        {
            int bw = bitmap.Width, bh = bitmap.Height;
            var from = GetTransformed(x1, y1);
            var to = GetTransformed(x2, y2);
            var lx = to.H - from.H;
            var ly = to.V - from.V;
            int count = (int) (Math.Sqrt(lx*lx + ly*ly) + 1);
            var floatCount = (float) count;
            for (int i = 0; i < count; i++)
            {
                var x = from.H + lx * i / floatCount;
                var y = from.V + ly * i / floatCount;
                int xi = (int) (x+0.5f);
                int yi = (int) (y+0.5f);
                if (xi >= 0 && yi >= 0 && xi < bw && yi < bh)
                    bitmap[xi, yi] = color;
            }
        }
    }
}
