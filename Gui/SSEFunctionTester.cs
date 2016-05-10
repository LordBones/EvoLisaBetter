using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenArtCoreNative;
using System.Diagnostics;
using GenArt.Core.Classes.Misc;

namespace GenArt
{
    public static class SSEFunctionTester
    {
        


        public static bool ApplyRowColor()
        {
            StopWatchExt sw = new StopWatchExt();

            const int fieldlength = 32000;
            NativeFunctions native = new NativeFunctions();


            long ticksColorsCorrect_sum = 0;
            long ticksTestNewSSECorrect_sum = 0;
            long ticksTestNewSSE64Correct_sum = 0;
            long ticksTestNewSSE128Correct_sum = 0;
            long ticksTestNewSSE256Correct_sum = 0;


            // oneline
            byte[] colorsCorrect = new byte[fieldlength];
            byte[] TestNewSSE = new byte[fieldlength];
            byte[] TestNewSSE64 = new byte[fieldlength];
            byte[] TestNewSSE128 = new byte[fieldlength];
            byte[] TestNewSSE256 = new byte[fieldlength];


            Random rnd = new Random(
                (int)DateTime.Now.Ticks);
            //0);
            int CONST_Counter = 1000;

            for (int counter = 0; counter < CONST_Counter; counter++)
            {

                for (int index = 0; index < colorsCorrect.Length; index += 4)
                {
                    colorsCorrect[index] = (byte)rnd.Next(0, 256);
                    colorsCorrect[index + 1] = (byte)rnd.Next(0, 256);
                    colorsCorrect[index + 2] = (byte)rnd.Next(0, 256);
                    colorsCorrect[index + 3] = 255;
                }

                Array.Copy(colorsCorrect, TestNewSSE, colorsCorrect.Length);
                Array.Copy(colorsCorrect, TestNewSSE64, colorsCorrect.Length);
                Array.Copy(colorsCorrect, TestNewSSE128, colorsCorrect.Length);
                Array.Copy(colorsCorrect, TestNewSSE256, colorsCorrect.Length);


                int r = rnd.Next(0, 256);
                int g = rnd.Next(0, 256);
                int b = rnd.Next(0, 256);
                int a = rnd.Next(0, 256);
                int color = Color.FromArgb(a, r, g, b).ToArgb();
                int count = 64;

                int startIndex = 8;

                sw.ResetStart();
                native.NewRowApplyColor(TestNewSSE, startIndex, count, color);
                ticksTestNewSSECorrect_sum += sw.StopTicks();

                sw.ResetStart();
                native.NewRowApplyColor64(TestNewSSE64, startIndex, count, color);
                ticksTestNewSSE64Correct_sum += sw.StopTicks();

                sw.ResetStart();
                native.NewRowApplyColor128(TestNewSSE128, startIndex, count, color);
                ticksTestNewSSE128Correct_sum += sw.StopTicks();

                sw.ResetStart();
                native.NewRowApplyColor256(TestNewSSE256, startIndex, count, color);
                ticksTestNewSSE256Correct_sum += sw.StopTicks();

                sw.ResetStart();
                native.NewRowApplyColorPure(colorsCorrect, startIndex, count, color);
                ticksColorsCorrect_sum += sw.StopTicks();

                //               native.RowApplyColorSSE64(colorsTest, 0, 15, 128, 50, 90, 255);


                bool correct = true;
                //correct &= CheckIfFieldsTheSame(colorsCorrect, TestOldSSE64,"Old RowAplyColorSSE64");
                //correct &= CheckIfFieldsTheSame(colorsCorrect, TestOldSSE128, "Old RowAplyColorSSE128");
                correct &= CheckIfFieldsTheSame(colorsCorrect, TestNewSSE, "New RowAplyColorSSE");
                correct &= CheckIfFieldsTheSame(colorsCorrect, TestNewSSE64, "New RowAplyColorSSE64");
                correct &= CheckIfFieldsTheSame(colorsCorrect, TestNewSSE128, "New RowAplyColorSSE128");
                correct &= CheckIfFieldsTheSame(colorsCorrect, TestNewSSE256, "New RowAplyColorSSE256");

                if (!correct)
                {
                    FieldDebugWrite(colorsCorrect);
                    FieldDebugWrite(TestNewSSE);

                    return false;
                }
            }

            Console.WriteLine("New RowApplyColorCorrect: {0} avgTics", ticksColorsCorrect_sum / (double)CONST_Counter);
            Console.WriteLine("New RowApplyColorSSE: {0} avgTics", ticksTestNewSSECorrect_sum / (double)CONST_Counter);
            Console.WriteLine("New RowApplyColorSSE64: {0} avgTics", ticksTestNewSSE64Correct_sum / (double)CONST_Counter);
            Console.WriteLine("New RowApplyColorSSE128: {0} avgTics", ticksTestNewSSE128Correct_sum / (double)CONST_Counter);
            Console.WriteLine("New RowApplyColorSSE256: {0} avgTics", ticksTestNewSSE256Correct_sum / (double)CONST_Counter);

            return true;
        }

        static bool CheckIfFieldsTheSame(byte[] field1, byte[] field2, string msg)
        {
            for (int index = 0; index + 3 < field1.Length; index += 4)
            {
                if (field1[index] != field2[index] || field1[index + 1] != field2[index + 1] || field1[index + 2] != field2[index + 2])
                {
                    Console.WriteLine("[" + msg + "] not working correct.");
                    return false;

                }
            }

            return true;
        }

        static void FieldDebugWrite(byte[] field1)
        {
            for (int i = 0; i < field1.Length; i++)
            {
                Trace.Write(field1[i].ToString() + ",");
            }
            Trace.WriteLine("-");
        }
    }
}
