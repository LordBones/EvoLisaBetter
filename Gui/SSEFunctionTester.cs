using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenArtCoreNative;

namespace GenArt
{
    public static class SSEFunctionTester
    {
        public static bool  ApplyRowColor()
        {
            NativeFunctions native = new NativeFunctions();
            

            // oneline
            byte [] colorsCorrect = new byte[32];
            byte [] colorsTest = new byte[32];

            Random rnd  = new Random(
                (int)DateTime.Now.Ticks);
                //0);
            for (int counter = 0; counter < 1000; counter++)
            {

                for (int index = 0; index < colorsCorrect.Length; index += 4)
                {
                    colorsCorrect[index] = (byte)rnd.Next(0, 256);
                    colorsCorrect[index + 1] = (byte)rnd.Next(0, 256);
                    colorsCorrect[index + 2] = (byte)rnd.Next(0, 256);
                    colorsCorrect[index + 3] = 255;
                }

                Array.Copy(colorsCorrect, colorsTest, colorsCorrect.Length);

                int r = rnd.Next(0, 256);
                int g = rnd.Next(0, 256);
                int b = rnd.Next(0, 256);
                int a = rnd.Next(0, 256);

                native.RowApplyColor(colorsCorrect, 0, 15, r, g, b, a);
                native.RowApplyColorSSE64(colorsTest, 0, 15, r, g, b, a);


//                native.RowApplyColor(colorsCorrect, 0, 15, 128, 50, 90, 255);
 //               native.RowApplyColorSSE64(colorsTest, 0, 15, 128, 50, 90, 255);

                for (int index = 0; index < colorsCorrect.Length; index ++)
                {
                    if (colorsCorrect[index] != colorsTest[index])
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
