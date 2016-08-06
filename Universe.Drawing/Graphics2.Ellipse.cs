﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Universe.Bitmap
{
    using System.Diagnostics;
    using System.Diagnostics.PerformanceData;

    partial class Graphics2
    {
        void DrawDraftEllipse(Bitmap2 bitmap, float xCenter, float yCenter, float xRadius, float yRadius, float angleStart, float angleEnd, Color2 color)
        {
            
            int bw = bitmap.Width, bh = bitmap.Height;
            var maxRadius = Math.Max(xRadius, yRadius);
            double length2PiR = 2 * Math.PI * maxRadius;
            float step = (float)(1 / length2PiR);
            int count = (int)(length2PiR * 1.01 + 1);
            float a0 = (float)(angleStart / 2 / Math.PI);
            float aStep = (float)(((angleEnd - angleStart) / 360 * 2 * Math.PI) / count);
            for (int i = 0; i <= count; i++)
            {
                float angle = a0 + i * aStep;
                var x = xCenter + Math.Cos(angle) * xRadius;
                var y = yCenter + Math.Sin(angle) * yRadius;
                var transformed = GetTransformed((float) x, (float) y);
                int xi = (int)transformed.H, yi = (int)transformed.V;
                if (xi >= 0 && yi >= 0 && xi < bw && yi < bh)
                    bitmap[xi, yi] = color;
            }
        }

        public Graphics2 DrawEllipse(float xCenter, float yCenter, float xRadius, float yRadius, float angleStart, float angleEnd, Color2 color, float width)
        {
            if (WorkingBitmap == null) throw new ObjectDisposedException("WorkingBitmap");

            if (Math.Abs(width) < 1e-6)
                DrawDraftEllipse(WorkingBitmap, xCenter, yCenter, xRadius, yRadius, angleStart, angleEnd, color);
            else
            {
                var wall = ScaleAA*width;
                int count = (int) wall;
                if (count < 1) count = 1;
                // 1: 0, 
                // 2: -0.5 & +0.5
                // 3: -1, 0, 1
                // 4: -1.5, -0.5, +0.5, +1.5 
                for (var i = 0; i < count; i++)
                {
                    float offset = (i - (count - 1) / 2f) / ScaleAA;
                    Debug.WriteLine("Count={0} Offset={1}", count, offset);
                    DrawDraftEllipse(WorkingBitmap, 
                        xCenter, yCenter, 
                        xRadius + offset, yRadius + offset, 
                        angleStart, angleEnd, 
                        color);
                }

            }
            return this;
        }

    }
}