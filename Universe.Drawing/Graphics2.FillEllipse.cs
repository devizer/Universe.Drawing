using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Universe.Drawing
{
    partial class Graphics2
    {

        public void FillEllipse(
            float xCenter, float yCenter,
            float xRadius, float yRadius,
            Color2 color)
        {
            EllipseLines lines = new EllipseLines();
            
            var bitmap = WorkingBitmap;
            float dx = 1f/(ScaleAA*Scale.H);
            float dy = 1f/(ScaleAA*Scale.V);
            float xRadius2 = xRadius*xRadius;
            float yRadius2 = yRadius*yRadius;

            if (xRadius > yRadius)
            {
                for (float x = -xRadius; x < xRadius; x += dx)
                {
                    float y1 = (float) Math.Sqrt((1f - (x*x/xRadius2))*yRadius2);
                    float y2 = -y1;
                    CalcDraftLine(lines, x, y1, x, y2);
/*
                    var p1 = GetTransformed(x, y1);
                    var p2 = GetTransformed(x, y2);
                    lines.AddLine((int)(p1.H + 0.5f), (int)(p1.V + 0.5f));
                    lines.AddLine((int)(p2.H + 0.5f), (int)(p2.V + 0.5f));
*/
                }
            }
            else
            {
                for (float y = -yRadius; y < yRadius; y += dy)
                {
                    float x1 = (float) Math.Sqrt((1f - (y*y/yRadius2))*xRadius2);
                    var x2 = -x1;
                    CalcDraftLine(lines, x1, y, x2, y);
/*
                    var p1 = GetTransformed(x1, y);
                    var p2 = GetTransformed(x2, y);
                    lines.AddLine((int)(p1.H + 0.5f), (int)(p1.V + 0.5f));
                    lines.AddLine((int)(p2.H + 0.5f), (int)(p2.V + 0.5f));
*/

                }
            }

#if DEBUG
            var ordered = lines.Lines.OrderBy(x => x.Key).ToArray();
#endif
            foreach (KeyValuePair<int, MinMax> line in lines.Lines)
            {
                var y = line.Key;
                if (y<0 || y>=bitmap.Height) continue;
                var x1 = Math.Max(0,line.Value.Min);
                var x2 = Math.Min(bitmap.Width, line.Value.Max);
                for (int x = x1; x <= x2; x++)
                    bitmap[x, y] = color;
            }
        }

        private void CalcDraftLine(EllipseLines lines, float x1, float y1, float x2, float y2)
        {
            var from = GetTransformed(x1, y1);
            var to = GetTransformed(x2, y2);
            var lx = to.H - from.H;
            var ly = to.V - from.V;
            int count = (int)(Math.Sqrt(lx * lx + ly * ly) + 1);
            var floatCount = (float)count;
            for (int i = 0; i < count; i++)
            {
                var x = from.H + lx * i / floatCount;
                var y = from.V + ly * i / floatCount;
                int xi = (int)(x + 0.5f);
                int yi = (int)(y + 0.5f);
                lines.AddLine(xi, yi);
            }
        }


        private struct MinMax
        {
            public int Min;
            public int Max;

            public override string ToString()
            {
                return string.Format("{{ {0} ... {1} }}", Min, Max);
            }
        }

        private class EllipseLines
        {
            // key: Y. Value: X1, X2
            public Dictionary<int, MinMax> Lines = new Dictionary<int, MinMax>();

            public void AddLine(int x, int y)
            {
                MinMax minmax;
                if (!Lines.TryGetValue(y, out minmax))
                {
                    Lines[y] = new MinMax() {Min = x, Max = x};
                }
                else
                {
                    minmax.Min = Math.Min(minmax.Min, x);
                    minmax.Max = Math.Max(minmax.Max, x);
                    Lines[y] = minmax;
                }
            }
        }
    }
}
