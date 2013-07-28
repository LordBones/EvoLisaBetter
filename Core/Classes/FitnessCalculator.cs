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
            error = ComputeFittnessBasic2(rbmp, sourceBitmap);
          
            return (long)(error) + ((newDrawing.PointCount + 1)*(newDrawing.PointCount + 1));

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
            
            softwareRender.Render(newDrawing, drawCanvas, sourceBitmap.Width, 1, background);

            error = ComputeFittnessBasic(drawCanvas, sourceBitmap);
            //error = ComputeFittnessAdvance(drawCanvas, sourceBitmap);

            //double sizeError = GetErrorByPolygonArea(sourceBitmap.Width, sourceBitmap.Height, newDrawing);

            //return (long)(error*sizeError) + ((newDrawing.PointCount + 1) * (newDrawing.PointCount + 1));

            //double koef = 0.0;
            //koef = (newDrawing.Polygons.Length < 100) ? 1.0 : 1.0+(newDrawing.Polygons.Length-100) / 100.0;  

            return (long)(error ) + ((newDrawing.PointCount + 1) * (newDrawing.PointCount + 1));

        }

        public static long GetDrawingFitnessSoftwareNative(DnaDrawing newDrawing, Bitmap sourceBitmap, byte[] sourceBitmapByte, Color background)
        {
            long error = 0;

            if (drawCanvas.Length != sourceBitmap.Width * sourceBitmap.Height * 4)
            {
                drawCanvas = new byte[sourceBitmap.Width * sourceBitmap.Height * 4];

                softwareRender = new SoftwareRender(sourceBitmap.Width, sourceBitmap.Height);
            }


            
            softwareRender.Render(newDrawing, drawCanvas, sourceBitmap.Width, 1, background);

            //error = ComputeFittnessBasic(drawCanvas, sourceBitmapByte);

            GenArtCoreNative.Class1 nc = new GenArtCoreNative.Class1();
            error = nc.ComputeFittness(drawCanvas, sourceBitmapByte, drawCanvas.Length);

            double sizeError = GetErrorByPolygonArea(sourceBitmap.Width, sourceBitmap.Height, newDrawing);

            //error = ComputeFittnessBasic(drawCanvas, sourceBitmap);
            return (int)(error*sizeError) + ((newDrawing.PointCount + 1) * (newDrawing.PointCount + 1));

        }

        private static double GetErrorByPolygonArea(int bitmapWidth, int bitmapHeight, DnaDrawing dna)
        {
            long sizeCanvas = bitmapHeight * bitmapWidth;
            long sumPolySize = dna.GetSumSize;

            if (sumPolySize > sizeCanvas*2)
            {
                return 1;//((sumPolySize - sizeCanvas)) / (double)sizeCanvas;
            }

            return 1;
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

        private static long ComputeFittnessBasic2(byte [] current, Bitmap orig)
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

                        int step = 0;
                        long partresult = 0;
                        while (currentPtr < totalLength)
                        {
                            int br = *(currentPtr) - *(origPtr);
                            int bg = *(currentPtr + 1) - *(origPtr + 1);
                            int bb = *(currentPtr + 2) - *(origPtr + 2);

                            long tmpres = Tools.fastAbs(br) + Tools.fastAbs(bg) + Tools.fastAbs(bb);

                            partresult += tmpres;
                        
                          
                            currentPtr += 4;
                            origPtr += 4;

                            step++;
                            if (step >= Tools.MaxWidth)
                            {
                                step = 0;
                                result += partresult * partresult;
                                partresult = 0;
                               
                            }
                        }

                            result += partresult * partresult;
                
                    }
                }
            }
            finally
            {
                orig.UnlockBits(bmdSRC);
            }

            return result;
        }

        private static long ComputeFittnessBasic(byte[] current, Bitmap orig)
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

                        int step = 0;
                        long partresult = 0;
                        while (currentPtr < totalLength)
                        {
                            int br = Tools.fastAbs(*(currentPtr) - *(origPtr));
                            int bg = Tools.fastAbs(*(currentPtr + 1) - *(origPtr + 1));
                            int bb = Tools.fastAbs(*(currentPtr + 2) - *(origPtr + 2));

                            //br = (br > 64) ? br << 2 : br;
                            //bg = (bg > 64) ? bg << 2 : bg;
                            //bb = (bb > 64) ? bb << 2 : bb;


                            long tmpres = br + bg + bb;

                            partresult += tmpres;


                            currentPtr += 4;
                            origPtr += 4;

                            step++;
                            if (step >= Tools.MaxWidth)
                            {
                                step = 0;
                                result += partresult;// *partresult;
                                partresult = 0;

                            }
                        }

                        result += partresult * partresult;

                    }
                }
            }
            finally
            {
                orig.UnlockBits(bmdSRC);
            }

            return result;
        }

        private static long ComputeFittnessAdvance(byte[] current, Bitmap orig)
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
                            int br = Tools.fastAbs(* (currentPtr) - *(origPtr));
                            int bg = Tools.fastAbs( * (currentPtr + 1) - *(origPtr + 1));
                            int bb = Tools.fastAbs( * (currentPtr + 2) - *(origPtr + 2));


                            long tmpres = (br < bg)?  bg : br;
                            tmpres = (tmpres < bb) ? bb : tmpres;

                            result += tmpres*3;
                        
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

                        long tmpres = Tools.fastAbs(br) + Tools.fastAbs(bg) + Tools.fastAbs(bb);
                        //int tmpres = (int)Math.Sqrt(( br * br + bg * bg + bb * bb)/3)*3;
                        //result += tmpres;
                        result += tmpres;
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

        private static long ComputeFittnessBasic2(Bitmap current, Bitmap orig)
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
                    byte * currentPtr = (byte*)bmd1.Scan0.ToPointer();
                    byte * origPtr = (byte*)bmdSRC.Scan0.ToPointer();

                    byte * totalLength = origPtr + (Tools.MaxWidth * Tools.MaxHeight * 4);
                    int index = 0;

                    long last = 0;
                    long partres = 0;
                    int step = 0;
                    while (origPtr < totalLength)
                    {
                        //int br = *(currentPtr) ^ *(origPtr);
                        //int bg = *(currentPtr + 1) ^ *(origPtr + 1);
                        //int bb = *(currentPtr + 2) ^ *(origPtr + 2);

                        //result += br + bg + bb;

                        int br = currentPtr[0] - origPtr[0];
                        int bg = currentPtr[1] - origPtr[1];
                        int bb = currentPtr[2] - origPtr[2];

                        long tmpres = Tools.fastAbs(br) + Tools.fastAbs(bg) + Tools.fastAbs(bb);
                        //int tmpres = (int)Math.Sqrt(( br * br + bg * bg + bb * bb)/3)*3;
                        //result += tmpres;
                        partres += tmpres;
                        currentPtr += 4;
                        origPtr += 4;

                        step++;
                        if (step >= Tools.MaxWidth)
                        {
                            step = 0;
                            result += partres << 4;
                            partres = 0;
                        }
                        
                    }

                    result += partres << 4;
                    
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