using System;
using System.Collections.Generic;
using System.Drawing;
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
            const int fieldlength = 4000;
            NativeFunctions native = new NativeFunctions();
            

            // oneline
            byte [] colorsCorrect = new byte[fieldlength];
            byte [] TestOldSSE64 = new byte[fieldlength];
            byte [] TestOldSSE128 = new byte[fieldlength];
            byte [] TestNewSSE = new byte[fieldlength];
            byte [] TestNewSSE64 = new byte[fieldlength];
            byte [] TestNewSSE128 = new byte[fieldlength];
            

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

                Array.Copy(colorsCorrect, TestOldSSE64, colorsCorrect.Length);
                Array.Copy(colorsCorrect, TestOldSSE128, colorsCorrect.Length);
                Array.Copy(colorsCorrect, TestNewSSE, colorsCorrect.Length);
                Array.Copy(colorsCorrect, TestNewSSE64, colorsCorrect.Length);
                Array.Copy(colorsCorrect, TestNewSSE128, colorsCorrect.Length);


                int r = rnd.Next(0, 256);
                int g = rnd.Next(0, 256);
                int b = rnd.Next(0, 256);
                int a = rnd.Next(0, 256);
                int color  = Color.FromArgb(a,r, g, b).ToArgb();
                int count = 33;
                native.RowApplyColor(colorsCorrect, 0, count, r, g, b, a);
                native.RowApplyColorSSE64(TestOldSSE64, 0, count, r, g, b, a);
                native.RowApplyColorSSE64(TestOldSSE128, 0, count, r, g, b, a);

                native.NewRowApplyColor(TestNewSSE, 0, count, color);
                native.NewRowApplyColor64(TestNewSSE64, 0, count, color);
                native.NewRowApplyColor128(TestNewSSE128, 0, count, color);


//                native.RowApplyColor(colorsCorrect, 0, 15, 128, 50, 90, 255);
 //               native.RowApplyColorSSE64(colorsTest, 0, 15, 128, 50, 90, 255);

                bool correct = true;
                correct &= CheckIfFieldsTheSame(colorsCorrect, TestOldSSE64,"Old RowAplyColorSSE64");
                correct &= CheckIfFieldsTheSame(colorsCorrect, TestOldSSE128, "Old RowAplyColorSSE128");
                correct &= CheckIfFieldsTheSame(colorsCorrect, TestNewSSE, "New RowAplyColorSSE");
                correct &= CheckIfFieldsTheSame(colorsCorrect, TestNewSSE64, "New RowAplyColorSSE64");
                correct &= CheckIfFieldsTheSame(colorsCorrect, TestNewSSE128, "New RowAplyColorSSE128");

                if (!correct) return false;
            }

            return true;
        }

        static bool CheckIfFieldsTheSame(byte [] field1, byte [] field2 , string msg)
        {
            for (int index = 0; index < field1.Length; index++)
            {
                if (field1[index] != field2[index])
                {
                    Console.WriteLine("["+msg+"] not working correct.");
                    return false;

                }
            }

            return true;
        }
    }
}
