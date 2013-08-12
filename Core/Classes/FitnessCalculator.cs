using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using GenArt.AST;
using GenArt.Core.Classes;
using GenArtCoreNative;

namespace GenArt.Classes
{
    public static class FitnessCalculator
    {
        #region new fittness methods

        public static long ComputeFittness_Basic(byte[] current, byte[] orig)
        {
            long result = 0;

            int index = 0;
            while (index < orig.Length)
            {
                int br = Tools.fastAbs(current[index] - orig[index]);
                int bg = Tools.fastAbs(current[index + 1] - orig[index + 1]);
                int bb = Tools.fastAbs(current[index + 2] - orig[index + 2]);

                long tmpres = br + bg + bb;

                result += tmpres;

                index += 4;
            }



            return result;
        }

        #endregion



        public static string kk = Assembly.GetEntryAssembly().GetName().Version.ToString();

        private static Bitmap rbmp = new Bitmap(1, 1, PixelFormat.Format32bppPArgb);
        private static Graphics g = Graphics.FromImage(rbmp);





        public static long GetDrawingFitness2(DnaDrawing newDrawing, CanvasBGRA sourceImage, Color background)
        {
            long error = 0;

            if (rbmp.Width != sourceImage.WidthPixel || rbmp.Height != sourceImage.HeightPixel)
            {
                //g.Dispose();
                rbmp.Dispose();

                rbmp = new Bitmap(sourceImage.WidthPixel, sourceImage.HeightPixel, PixelFormat.Format32bppPArgb);
                g = Graphics.FromImage(rbmp);

                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighSpeed;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;

            }


            Renderer.Render(newDrawing, g, 1);

            //rbmp.Save("testGDI.bmp", ImageFormat.Bmp);
            error = ComputeFittnessBasic(sourceImage.Data, rbmp);

            return (long)(error) + ((newDrawing.PointCount + 1) * (newDrawing.PointCount + 1));

        }

        public static long GetDrawingFitnessWPF(DnaDrawing newDrawing, CanvasBGRA sourceImage, Color background)
        {
            long error = 0;


            byte [] resultBmp = RenderWPF.Render(newDrawing, 1, new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(background.R, background.G, background.B)),
                sourceImage.WidthPixel, sourceImage.HeightPixel);


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

            error = ComputeFittnessBasic(resultBmp, sourceImage.Data);

            //return error;
            return error + ((newDrawing.PointCount + 1) *(newDrawing.PointCount + 1));

        }

        static CanvasBGRA drawCanvas = new CanvasBGRA(1, 1);
        static SoftwareRender softwareRender;
        static NativeFunctions nativeFunc = new NativeFunctions();

        public static long GetDrawingFitnessSoftware(DnaDrawing newDrawing, CanvasBGRA sourceImage, Color background)
        {


            if (drawCanvas.Data.Length != sourceImage.Data.Length)
            {
                drawCanvas = new CanvasBGRA(sourceImage.WidthPixel, sourceImage.HeightPixel);

                softwareRender = new SoftwareRender(sourceImage.WidthPixel, sourceImage.HeightPixel);
            }

            softwareRender.Render(newDrawing, drawCanvas, 1, background);

            long error = 0;

            //error = ComputeFittnessBasic(drawCanvas, sourceBitmap);
            //error = ComputeFittnessBasic(drawCanvas.Data, sourceImage.Data);
            error = nativeFunc.ComputeFittness(drawCanvas.Data, sourceImage.Data);
            //error = ComputeFittnessAdvance(drawCanvas, sourceBitmap);

            //double sizeError = GetErrorByPolygonArea(sourceBitmap.Width, sourceBitmap.Height, newDrawing);

            //return (long)(error*sizeError) + ((newDrawing.PointCount + 1) * (newDrawing.PointCount + 1));

            //double koef = 0.0;
            //koef = (newDrawing.Polygons.Length < 100) ? 1.0 : 1.0+(newDrawing.Polygons.Length-100) / 100.0;  

            return (long)(error) + ((newDrawing.PointCount + 1) * (newDrawing.PointCount + 1));

        }

        public static long GetDrawingFitnessSoftwareNative(DnaDrawing newDrawing, CanvasBGRA sourceImage, Color background)
        {

            if (drawCanvas.Data.Length != sourceImage.Data.Length)
            {
                drawCanvas = new CanvasBGRA(sourceImage.WidthPixel, sourceImage.HeightPixel);

                softwareRender = new SoftwareRender(sourceImage.WidthPixel, sourceImage.HeightPixel);
            }

            softwareRender.Render(newDrawing, drawCanvas, 1, background);

            long error = 0;
            //error = ComputeFittnessBasic(drawCanvas, sourceBitmapByte);

            GenArtCoreNative.NativeFunctions nc = new GenArtCoreNative.NativeFunctions();
            error = nc.ComputeFittness(drawCanvas.Data, sourceImage.Data);

            //error = ComputeFittnessBasic(drawCanvas, sourceBitmap);
            return (int)(error) + ((newDrawing.PointCount + 1) * (newDrawing.PointCount + 1));

        }

        private static double GetErrorByPolygonArea(int bitmapWidth, int bitmapHeight, DnaDrawing dna)
        {
            long sizeCanvas = bitmapHeight * bitmapWidth;
            long sumPolySize = dna.GetSumSize;

            if (sumPolySize > sizeCanvas * 2)
            {
                return 1;//((sumPolySize - sizeCanvas)) / (double)sizeCanvas;
            }

            return 1;
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

        private static long ComputeFittnessBasic(byte[] current, byte[] orig)
        {
            long result = 0;

            int index = 0;
            while (index < orig.Length)
            {
                int br = Tools.fastAbs(current[index] - orig[index]);
                int bg = Tools.fastAbs(current[index + 1] - orig[index + 1]);
                int bb = Tools.fastAbs(current[index + 2] - orig[index + 2]);

                long tmpres = br + bg + bb;

                result += tmpres;

                index += 4;
            }

            

            return result;
        }

        public static long ComputeFittness_BasicAdvance(byte[] current, byte[] orig)
        {
            Median8bit medR = new Median8bit();
            Median8bit medG = new Median8bit();
            Median8bit medB = new Median8bit();


            int index = 0;
            while (index < orig.Length)
            {
                medB.InsertData((byte)Tools.fastAbs(current[index] - orig[index]));
                medG.InsertData((byte)Tools.fastAbs(current[index + 1] - orig[index + 1]));
                medR.InsertData((byte)Tools.fastAbs(current[index + 2] - orig[index + 2]));
                index += 4;
            }


            double ChDiff_AvgB = medB.Median;
            double ChDiff_AvgG = medG.Median;
            double ChDiff_AvgR = medR.Median;


            // spocteni prumerne std odchylky
            index = 0;

            double sumStdDevB = 0;
            double sumStdDevG = 0;
            double sumStdDevR = 0;
            while (index < orig.Length)
            {
                sumStdDevB += Math.Abs(Tools.fastAbs(current[index] - orig[index]) - ChDiff_AvgB);
                sumStdDevG += Math.Abs(Tools.fastAbs(current[index + 1] - orig[index + 1]) - ChDiff_AvgG);
                sumStdDevR += Math.Abs(Tools.fastAbs(current[index + 2] - orig[index + 2]) - ChDiff_AvgR);
                index += 4;
            }

            long result = 0;
            result += (long)(medB.TotalSum + ChDiff_AvgB * 2);
            result += (long)(medG.TotalSum + ChDiff_AvgG * 2)*7;
            result += (long)(medR.TotalSum + ChDiff_AvgR * 2)*2;
          


            return result;
        }
    }
}