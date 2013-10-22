
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenArt.Core.Classes.SWRenderLibrary
{
    public static class SWHelpers
    {
        public static void RowApplyBlendColorSafeARGB(byte[] data, int startIndex, int endIndex, int r, int g, int b, int alpha)
        {
            SWHelpers.RowApplyBlendColorSafeARGB(data, startIndex, endIndex,
                Color.FromArgb(alpha, r, g, b).ToArgb());
        }

        public static void RowApplyBlendColorSafeARGB(byte[] data, int startIndex, int endIndex, int color)
        {
            int r = (byte)((((uint)color) >> 16) & 0xff);
            int g = (byte)((((uint)color) >> 8) & 0xff);
            int b = (byte)((((uint)color)) & 0xff);
            int alpha = (byte)((((uint)color) >> 24) & 0xff);

            alpha = (alpha * 256) / 255;

            int invAlpha = 256 - alpha;

            int cb = b * alpha;
            int cg = g * alpha;
            int cr = r * alpha;

            while (startIndex <= endIndex)
            {
                int tb = data[startIndex];
                int tg = data[startIndex + 1];
                int tr = data[startIndex + 2];


                tb = (cb + (tb * invAlpha)) >> 8;
                tg = (cg + (tg * invAlpha)) >> 8;
                tr = (cr + (tr * invAlpha)) >> 8;

                /*tb = tb + (((b-tb)*alpha)>>8);
                tg=tg + (((g-tg)*alpha)>>8);
                tr=tr + (((r-tr)*alpha)>>8);*/

                data[startIndex] = (byte)tb;
                data[startIndex + 1] = (byte)tg;
                data[startIndex + 2] = (byte)tr;



                startIndex += 4;
            }

        }
    }
}
