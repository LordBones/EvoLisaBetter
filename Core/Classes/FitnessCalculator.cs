using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using GenArt.AST;
using GenArt.Core.Classes;

namespace GenArt.Classes
{
    public static class FitnessCalculator
    {
        public static string kk = Assembly.GetEntryAssembly().GetName().Version.ToString();

        private static Bitmap rbmp = new Bitmap(1, 1, PixelFormat.Format32bppPArgb);
        private static Graphics g = Graphics.FromImage(rbmp);

        public static long GetDrawingFitnessD3D(D3DDnaRender d3drender, DnaDrawing newDrawing, Bitmap sourceBitmap, Color background)
        {
            long error = 0;// Tools.GetRandomNumber(100, 10000);

            //using (Bitmap bmp = D3DRender.Render(newDrawing, background, Tools.MaxWidth, Tools.MaxHeight))
            //{
            //    error = ComputeFittnessBasicD3D(bmp, sourceBitmap);
            //    // error = (long)(newDrawing.Polygons.Count + newDrawing.PointCount);
            //}

            byte [] bmp = d3drender.Render(newDrawing, background);
            {
                  error = ComputeFittnessBasic(bmp, sourceBitmap);
            }

            return error + ((newDrawing.PointCount + 1));

        }

        public static long GetDrawingFitnessD3D(DnaDrawing newDrawing, Bitmap sourceBitmap, Color background)
        {
            long error = 0;// Tools.GetRandomNumber(100, 10000);

            //using (Bitmap bmp = D3DRender.Render(newDrawing, background, Tools.MaxWidth, Tools.MaxHeight))
            //{
            //    error = ComputeFittnessBasicD3D(bmp, sourceBitmap);
            //    // error = (long)(newDrawing.Polygons.Count + newDrawing.PointCount);
            //}

            byte [] bmp = D3DRender.Render(newDrawing, background, Tools.MaxWidth, Tools.MaxHeight);
            {
              //  error = ComputeFittnessBasicD3D(bmp, sourceBitmap);
                // error = (long)(newDrawing.Polygons.Count + newDrawing.PointCount);
            }


            //return (long)((error * ttt) + 1);
            return error + ((newDrawing.PointCount + 1));

        }

        public static long GetDrawingFitness2(DnaDrawing newDrawing, Bitmap sourceBitmap, Color background)
        {
            long error = 0;

            if (rbmp.Width != Tools.MaxWidth || rbmp.Height != Tools.MaxHeight)
            {
                //g.Dispose();
                rbmp.Dispose();

                rbmp = new Bitmap(Tools.MaxWidth, Tools.MaxHeight, PixelFormat.Format32bppPArgb);
                g = Graphics.FromImage(rbmp);

                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighSpeed;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;

            }


            Renderer.Render(newDrawing, g, 1, background);

            //rbmp.Save("testGDI.bmp", ImageFormat.Bmp);
            error = ComputeFittnessBasic(rbmp, sourceBitmap);


            return error + ((newDrawing.PointCount + 1));

        }

        public static long GetDrawingFitnessWPF(DnaDrawing newDrawing, Bitmap sourceBitmap, Color background)
        {
            long error = 0;


            byte [] resultBmp = RenderWPF.Render(newDrawing,1,new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(background.R,background.G, background.B)),
                sourceBitmap.Width,sourceBitmap.Height);


            //Bitmap bmp = new Bitmap(sourceBitmap.Width, sourceBitmap.Height, PixelFormat.Format32bppPArgb);
            //var lockBmp = bmp.LockBits(new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);

            //unsafe
            //{
            //    byte * ll = (byte*)lockBmp.Scan0.ToPointer();

            //    for (int index = 0; index < resultBmp.Length; index++)
            //    {
            //        *(ll + index) = resultBmp[index];
            //    }
            //}

            //bmp.UnlockBits(lockBmp);
            //bmp.Save("test.png", ImageFormat.Png);

            error = ComputeFittnessBasic(resultBmp,sourceBitmap);

            //return error;
            return error + ((newDrawing.PointCount + 1));

        }

        static byte [] drawCanvas = new byte[0];
        static SoftwareRender softwareRender;

        public static long GetDrawingFitnessSoftware(DnaDrawing newDrawing, Bitmap sourceBitmap,byte [] sourceBitmapByte, Color background)
        {
            long error = 0;

            if (drawCanvas.Length != sourceBitmap.Width * sourceBitmap.Height * 4)
            {
                drawCanvas = new byte[sourceBitmap.Width * sourceBitmap.Height * 4];

                softwareRender = new SoftwareRender(sourceBitmap.Width, sourceBitmap.Height);
            }

            
            //SoftwareRender.Render(newDrawing, drawCanvas, sourceBitmap.Width, 1, background);

            softwareRender.Render(newDrawing, drawCanvas, sourceBitmap.Width, 1, background);


            //Bitmap bmp = new Bitmap(sourceBitmap.Width, sourceBitmap.Height, PixelFormat.Format32bppPArgb);
            //var lockBmp = bmp.LockBits(new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);

            //unsafe
            //{
            //    byte * ll = (byte*)lockBmp.Scan0.ToPointer();

            //    for (int index = 0; index < drawCanvas.Length; index++)
            //    {
            //        *(ll + index) = drawCanvas[index];
            //    }
            //}

            //bmp.UnlockBits(lockBmp);
            //bmp.Save("testsw.bmp", ImageFormat.Bmp);
             
            
            //error = ComputeFittnessBasic(drawCanvas, sourceBitmapByte);

            GenArtCoreNative.Class1 nc = new GenArtCoreNative.Class1();
            error = nc.ComputeFittness(drawCanvas, sourceBitmapByte, drawCanvas.Length);

            //error = ComputeFittnessBasic(drawCanvas, sourceBitmap);
            //return error;
            return error + ((newDrawing.PointCount + 1) * (newDrawing.PointCount + 1));

        }

        private static long ComputeFittnessBasic(byte[] current, byte[] orig)
        {
            long result = 0;
            int index = 0;
            //while (index < current.Length)
            //{
            //    int br =  current[index] - orig[index];
            //    int bg = current[index+1] - orig[index+1];
            //    int bb = current[index+2] - orig[index+2];

            //    int tmpres = (int)Math.Sqrt((br * br + bg * bg + bb * bb) / 3) * 3;
            //    result += tmpres;

            //    index += 4;
            //}

            while (index < current.Length)
            {
                int br = Math.Abs(current[index] - orig[index]);
                int bg = Math.Abs(current[index + 1] - orig[index + 1]);
                int bb = Math.Abs(current[index + 2] - orig[index + 2]);

                int tmpres = br + bg + bb;
                result += tmpres;

                index += 4;
            }

            return result;
        }

        private static long ComputeFittnessBasic(byte [] current, Bitmap orig)
        {
            long result = 0;

            BitmapData bmdSRC = orig.LockBits(new Rectangle(0, 0, orig.Width, orig.Height), ImageLockMode.ReadOnly,
                                        PixelFormat.Format32bppPArgb);
            try
            {
                
                unsafe
                {
                    fixed (byte * tmpcurrentPtr = current)
                    {
                        byte * currentPtr = tmpcurrentPtr;
                        byte * origPtr = (byte*)bmdSRC.Scan0.ToPointer();

                        //int totalLength = (Tools.MaxWidth * Tools.MaxHeight * 4);
                        byte * totalLength = currentPtr + (Tools.MaxWidth * Tools.MaxHeight * 4);
                        int index = 0;

                        while (currentPtr < totalLength)
                        {
                            int br = *(currentPtr) - *(origPtr);
                            int bg = *(currentPtr + 1) - *(origPtr + 1);
                            int bb = *(currentPtr + 2) - *(origPtr + 2);

                            int tmpres = (int)Math.Sqrt((br * br + bg * bg + bb * bb) / 3) * 3;
                            result += tmpres;
                        
                            //result += br * br + bg * bg + bb * bb;
                            //result += tmp*tmp;
                            //int br = *(currentPtr) ^ *(origPtr);
                            //int bg = *(currentPtr + 1) ^ *(origPtr + 1);
                            //int bb = *(currentPtr +2) ^ *(origPtr + 2);

                            //result += br  +  bg +  bb;


                            //index += 4;
                            currentPtr += 4;
                            origPtr += 4;
                        }
                    }
                }
            }
            finally
            {
                orig.UnlockBits(bmdSRC);
            }

            return result;
        }

        private static long FittnessBasicTest(byte[] current, Bitmap orig)
        {
            long result = 0;

            BitmapData bmdSRC = orig.LockBits(new Rectangle(0, 0, orig.Width, orig.Height), ImageLockMode.ReadOnly,
                                        PixelFormat.Format32bppPArgb);
            try
            {

                unsafe
                {
                    fixed (byte * tmpcurrentPtr = current)
                    {
                        byte * currentPtr = tmpcurrentPtr;
                        byte * origPtr = (byte*)bmdSRC.Scan0.ToPointer();

                        //int totalLength = (Tools.MaxWidth * Tools.MaxHeight * 4);
                        byte * totalLength = currentPtr + (Tools.MaxWidth * Tools.MaxHeight * 4);
                        int index = 0;

                        byte * totalLength2 = totalLength - 16;

                        while (currentPtr < totalLength2)
                        {
                                result += Tools.fastAbs(*(currentPtr)- *(origPtr));//r
                                result += Tools.fastAbs(*(currentPtr + 1) - *(origPtr + 1));//g
                                result += Tools.fastAbs(*(currentPtr + 2) - *(origPtr + 2));//b

                                //currentPtr += 4;
                                //origPtr += 4;

                                result += Tools.fastAbs(*(currentPtr + 4) - *(origPtr + 4));//r
                                result += Tools.fastAbs(*(currentPtr + 5) - *(origPtr + 5));//g
                                result += Tools.fastAbs(*(currentPtr + 6) - *(origPtr + 6));//b

                                //currentPtr += 8;
                                //origPtr += 8;

                                result += Tools.fastAbs(*(currentPtr + 8) - *(origPtr + 8));//r
                                result += Tools.fastAbs(*(currentPtr + 9) - *(origPtr + 9));//g
                                result += Tools.fastAbs(*(currentPtr + 10) - *(origPtr + 10));//b

                                //currentPtr += 12;
                                //origPtr += 12;

                                result += Tools.fastAbs(*(currentPtr + 12) - *(origPtr + 12));//r
                                result += Tools.fastAbs(*(currentPtr + 13) - *(origPtr + 13));//g
                                result += Tools.fastAbs(*(currentPtr + 14) - *(origPtr + 24));//b



                                currentPtr += 16;
                                origPtr += 16;

                            
                        }

                        while (currentPtr < totalLength)
                        {
                            result += GetColorFittness(*(currentPtr), *(origPtr));//r
                            result += GetColorFittness(*(currentPtr + 1), *(origPtr + 1));//g
                            result += GetColorFittness(*(currentPtr + 2), *(origPtr + 2));//b

                            currentPtr += 4;
                            origPtr += 4;
                        }

                    }
                }
            }
            finally
            {
                orig.UnlockBits(bmdSRC);
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetColorFittness(byte oldC, byte newC)
        {
            return Tools.fastAbs(newC - oldC);
        }

        private static long FittnessBasicTest2(byte[] current, Bitmap orig)
        {
            long result = 0;

            BitmapData bmdSRC = orig.LockBits(new Rectangle(0, 0, orig.Width, orig.Height), ImageLockMode.ReadOnly,
                                        PixelFormat.Format32bppPArgb);
            try
            {

                unsafe
                {
                    fixed (byte * tmpcurrentPtr = current)
                    {
                        uint * currentPtr = (uint*)tmpcurrentPtr;
                        uint * origPtr = (uint*)bmdSRC.Scan0.ToPointer();

                        //int totalLength = (Tools.MaxWidth * Tools.MaxHeight * 4);
                        uint * totalLength = currentPtr + (Tools.MaxWidth * Tools.MaxHeight);
                        uint index = 0;

                        while (currentPtr < totalLength)
                        {
                            uint origC = *(origPtr);
                            uint currC = *(currentPtr);

                         

                            int br = ((int)((origC >> 16) & 0xff)) - ((int)((currC >> 16) & 0xff));
                            int bg = ((int)((origC >> 8) & 0xff)) - ((int)((currC >> 8) & 0xff));
                            int bb = ((int)((origC >> 0) & 0xff)) - ((int)((currC >> 0) & 0xff));

                            result += Tools.fastAbs(br) + Tools.fastAbs(bg) + Tools.fastAbs(bb);

                           

                            //int br = *(currentPtr) ^ *(origPtr);
                            //int bg = *(currentPtr + 1) ^ *(origPtr + 1);
                            //int bb = *(currentPtr + 2) ^ *(origPtr + 2);

                            //result += br + bg + bb;


                            //index += 4;
                            currentPtr+=1;
                            origPtr+=1;
                        }
                    }
                }
            }
            finally
            {
                orig.UnlockBits(bmdSRC);
            }

            return result;
        }

        
        private static long ComputeFittnessBasic(Bitmap current, Bitmap orig)
        {
            long result = 0;
           
            BitmapData bmd1 = current.LockBits(new Rectangle(0, 0, current.Width, orig.Height), ImageLockMode.ReadOnly,
                                         PixelFormat.Format32bppPArgb);

            BitmapData bmdSRC = orig.LockBits(new Rectangle(0, 0, orig.Width, orig.Height), ImageLockMode.ReadOnly,
                                        PixelFormat.Format32bppPArgb);
            try
            {
                unsafe
                {
                    byte * currentPtr = (byte *)bmd1.Scan0.ToPointer();
                    byte * origPtr = (byte*)bmdSRC.Scan0.ToPointer();

                    byte * totalLength = origPtr + (Tools.MaxWidth * Tools.MaxHeight * 4);
                    int index = 0;

                    long last = 0;
                    while (origPtr < totalLength)
                    {
                        //int br = *(currentPtr) ^ *(origPtr);
                        //int bg = *(currentPtr + 1) ^ *(origPtr + 1);
                        //int bb = *(currentPtr + 2) ^ *(origPtr + 2);

                        //result += br + bg + bb;

                        int br = currentPtr[0] - origPtr[0];
                        int bg = currentPtr[1] - origPtr[1];
                        int bb = currentPtr[2] - origPtr[2];

                        //int brabs = Tools.fastAbs(br);
                        //int bgabs = Tools.fastAbs(bg);
                        //int bbabs = Tools.fastAbs(bb);

                        //long tmpres =  fastAbs(brabs - bgabs) + fastAbs(brabs - bbabs) + fastAbs(bgabs - bbabs) + brabs + bgabs+ bbabs;

                        //long tmpres = Tools.fastAbs(br) + Tools.fastAbs(bg) + Tools.fastAbs(bb);
                        int tmpres = (int)Math.Sqrt(( br * br + bg * bg + bb * bb)/3)*3;
                        result += tmpres;
                        //result += ((tmpres - last < 0)? (last - tmpres) :(tmpres - last)) + tmpres;
                        //result += (tmpres - last) * (tmpres - last) + tmpres;
                        //last = tmpres;
                        //index += 4;
                        currentPtr += 4;
                        origPtr += 4;
                    }
                }
            }
            finally
            {
                rbmp.UnlockBits(bmd1);
                orig.UnlockBits(bmdSRC);
            }

            return result;
        }  
    }
}