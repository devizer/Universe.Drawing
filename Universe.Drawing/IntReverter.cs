using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Universe.Drawing
{
    class IntReverter
    {
        public static short Int16(short value)
        {
            return (short)
                ((value & 0xFF) << 8
                 | (value & 0xFF00) >> 8);

        }

        public static int Int32(int value)
        {
            return (int)
                ((value & 0x000000FF) << 24
                 | (value & 0x0000FF00) << 8
                 | (value & 0x00FF0000) >> 8
                 | (value & 0xFF000000) >> 24);
        }
    }
}
