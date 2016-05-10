using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GenArtCoreNative;
using System.Drawing;

namespace UnitTest
{
    [TestClass]
    public class NativeWrapeTest
    {
        NativeFunctions nativeF = new NativeFunctions();

        [TestMethod]
        public void ClearFieldByColor()
        {
            byte[] fieldFinal = new byte[100];

            for (int i = 0; i < fieldFinal.Length; i += 4)
            {
                fieldFinal[i] = 50;
                fieldFinal[i + 1] = 40;
                fieldFinal[i + 2] = 30;
                fieldFinal[i + 3] = 20;
            } 

            byte [] field = new byte[100];

            nativeF.ClearFieldByColor(field, Color.FromArgb(20, 30, 40, 50).ToArgb());

            for(int i = 0;i<field.Length;i++)
            {
                if (field[i] != fieldFinal[i])
                    Assert.Fail("first test");
            }

            Array.Clear(field, 0, field.Length);
            Array.Clear(fieldFinal, 0, fieldFinal.Length);

            for (int i = 40; i < 60; i += 4)
            {
                fieldFinal[i] = 50;
                fieldFinal[i + 1] = 40;
                fieldFinal[i + 2] = 30;
                fieldFinal[i + 3] = 20;
            } 

            nativeF.ClearFieldByColorInt(field, 40,5, Color.FromArgb(20, 30, 40, 50).ToArgb());

            for (int i = 0; i < field.Length; i++)
            {
                if (field[i] != fieldFinal[i])
                    Assert.Fail("second test");
            }

           

        }
    }
}
