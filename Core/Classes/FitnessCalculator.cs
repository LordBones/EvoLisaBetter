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
using System.Diagnostics;

namespace GenArt.Classes
{
    public static class FitnessCalculator
    {
        #region new fittness methods

        public static long ComputeFittness_Basic(byte[] current, byte[] orig, int increment)
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

                index += 4*increment;
            }



            return result*increment;
        }

        #endregion



        public static string kk = Assembly.GetEntryAssembly().GetName().Version.ToString();

        public static long GetDrawingFitnessWPF(DnaDrawing newDrawing, CanvasARGB sourceImage, Color background)
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

            error = ComputeFittnessLine_SumABS(resultBmp, sourceImage.Data);

            //return error;
            return error + ((newDrawing.PointCount + 1) *(newDrawing.PointCount + 1));

        }

        private static long ComputeFittness_SumABS(byte[] current, Bitmap orig)
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

        /// <summary>
        /// spocita fittnesss pro dany jeden radek obrazku
        /// </summary>
        /// <param name="currentLine"></param>
        /// <param name="orig"></param>
        /// <param name="origStartIndex"></param>
        /// <returns></returns>
        public static long ComputeFittnessLine_SumSquare(byte[] currentLine, byte[] orig, int origStartIndex)
        {
            long result = 0;
            int len = currentLine.Length;
           
            int index = 0;
            while (index < currentLine.Length)
            {
                int br = (currentLine[index] - orig[origStartIndex+index]);
                int bg = (currentLine[index + 1] - orig[origStartIndex+index + 1]);
                int bb = (currentLine[index + 2] - orig[origStartIndex+index + 2]);

                long tmpres = br*br + bg*bg + bb*bb;

                result += tmpres;

                index += 4;
            }

            return result;
        }

        

        /// <summary>
        /// spocita fittnesss pro dany jeden radek obrazku
        /// </summary>
        /// <param name="current"></param>
        /// <param name="orig"></param>
        /// <param name="origStartIndex"></param>
        /// <returns></returns>
        public static long ComputeFittnessLine_SumSquare(byte[] current, byte[] orig)
        {
            long result = 0;

            int index = 0;
            while (index < current.Length)
            {
                int br = (current[index] - orig[ index]);
                int bg = (current[index + 1] - orig[ index + 1]);
                int bb = (current[index + 2] - orig[ index + 2]);

                long tmpres = br * br + bg * bg + bb * bb;

                result += tmpres;

                index += 4;
            }

            return result;
        }

        /// <summary>
        /// spocita fittnesss pro dany jeden radek obrazku
        /// </summary>
        /// <param name="current"></param>
        /// <param name="orig"></param>
        /// <param name="origStartIndex"></param>
        /// <returns></returns>
        public static long ComputeFittnessLine_OnlyCorrectHit2(byte[] current, byte[] orig)
        {
            long result = 0;

            int lastColor = 0;
            {
                int br = Tools.fastAbs(current[0] - orig[0]);
                int bg = Tools.fastAbs(current[ 1] - orig[1]);
                int bb = Tools.fastAbs(current[ 2] - orig[2]);
                result += (br + bg + bb) * (br + bg + bb);
            }

            int index = 4;
            int indexLast = 0;
            while (index < current.Length)
            {
                int betweenbr = (current[index] - current[indexLast])
                                - (orig[index] - orig[indexLast]);
                int betweenbg = (current[index+1] - current[indexLast+1])
                                - (orig[index+1] - orig[indexLast+1]);

                int betweenbb = (current[index + 2] - current[indexLast + 2])
                                - (orig[index + 2] - orig[indexLast + 2]);

                int tmp = Tools.fastAbs(betweenbr) + Tools.fastAbs(betweenbg) + Tools.fastAbs(betweenbb);
                result += tmp * tmp;


                int br = Tools.fastAbs(current[index] - orig[index]);
                int bg = Tools.fastAbs(current[index + 1] - orig[index + 1]);
                int bb = Tools.fastAbs(current[index + 2] - orig[index + 2]);
                int color = br + bg + bb;


                int col2 = color ;
                //int abr = Tools.fastAbs(br-lastbr);
                //int abg = Tools.fastAbs(bg - lastbg);
                //int abb = Tools.fastAbs(bb - lastbb);


                //long tmpres = br*br+bg*bg+bb*bb+ (abr)* (abr) + (abg) * (abg) + (abb) * (abb);
                //long tmpres = (color) * (color) + Tools.fastAbs(color - lastColor);// + (abr+ abg + abb);
                long tmpres = col2*col2;// + (abr+ abg + abb);

                //lastColor = color;
                
                
                result +=tmpres;

                index += 4;
                indexLast += 4;
            }

            return result;
        }


        /// <summary>
        /// spocita fittnesss pro dany jeden radek obrazku
        /// </summary>
        /// <param name="current"></param>
        /// <param name="orig"></param>
        /// <param name="origStartIndex"></param>
        /// <returns></returns>
        public static long ComputeFittnessLine_OnlyCorrectHit3(byte[] current, byte[] orig)
        {
            long result = 0;

            int lastColor = 0;
            int countMinInRow = 0;
            int countPx = 0;
            long tmpRowRes = 0;
            int index = 0;
            while (index < current.Length)
            {
                int br = Tools.fastAbs(current[index] - orig[index]);
                int bg = Tools.fastAbs(current[index + 1] - orig[index + 1]);
                int bb = Tools.fastAbs(current[index + 2] - orig[index + 2]);
                int color = br + bg + bb;
                int col2 = color;
                //int abr = Tools.fastAbs(br-lastbr);
                //int abg = Tools.fastAbs(bg - lastbg);
                //int abb = Tools.fastAbs(bb - lastbb);


                //long tmpres = br*br+bg*bg+bb*bb+ (abr)* (abr) + (abg) * (abg) + (abb) * (abb);
                //long tmpres = (color) * (color) + Tools.fastAbs(color - lastColor);// + (abr+ abg + abb);
                long tmpres = col2 * col2;// + (abr+ abg + abb);

                lastColor = color;

                //if ((col2 >> 5) == 0)
                //    countMinInRow++;

                result += tmpres;
               // tmpRowRes += tmpres;

                index += 4;
                countPx++;

                //if(countPx >= 16 )
                //{
                //    if (countMinInRow == 0)
                //        result += tmpRowRes;
                //    else
                //        result += tmpRowRes * 4;

                //    countPx = 0;
                //    tmpRowRes = 0;
                //    countMinInRow = 0;
                //}
            }

            return result;
        }


        private static int[] _multipleConst = new int[] { 1, 1, 1, 4, 1, 1, 1 };
        /// <summary>
        /// spocita fittnesss pro dany jeden radek obrazku
        /// </summary>
        /// <param name="current"></param>
        /// <param name="orig"></param>
        /// <param name="origStartIndex"></param>
        /// <returns></returns>
        public static long ComputeFittnessLine_OnlyCorrectHit(byte[] current, byte[] orig)
        {
            long result = 0;

            //int lastColor = 0;
            int multiIndex = 0;
            
            int index = 0;
            while (index < current.Length)
            {
                int br = Tools.fastAbs(current[index] - orig[index]);
                int bg = Tools.fastAbs(current[index + 1] - orig[index + 1]);
                int bb = Tools.fastAbs(current[index + 2] - orig[index + 2]);
                int color = br + bg + bb;
                int col2 = color  *_multipleConst[multiIndex];
               
                long tmpres = col2 * col2;// + (abr+ abg + abb);

                //lastColor = color;
                result += tmpres ;
                // tmpRowRes += tmpres;

                index += 4;
                multiIndex++;

                if (_multipleConst.Length <= multiIndex)
                    multiIndex = 0;
            }

            return result;
        }



        public static long ComputeFittnessOneChanell_SumAbs(byte[] current, byte[] orig, int width)
        {
            long result = 0;

            for(int i =0;i< current.Length;i++)
            {
                int br = (current[i] - orig[i]);
                long tmpres = 
                    //Tools.fastAbs(br) + 
                    Tools.fastAbs(br);
                result += tmpres;

            }
            
            return result;
        }

        

       


        /// <summary>
        /// spocita fittnesss pro dany jeden radek obrazku
        /// </summary>
        /// <param name="currentLine"></param>
        /// <param name="orig"></param>
        /// <param name="origStartIndex"></param>
        /// <returns></returns>
        public static long ComputeFittnessLine_SumABS(byte[] currentLine, byte[] orig, int origStartIndex)
        {
            long result = 0;

            int index = 0;
            while (index < currentLine.Length)
            {
                int br = (currentLine[index] - orig[origStartIndex + index]);
                int bg = (currentLine[index + 1] - orig[origStartIndex + index + 1]);
                int bb = (currentLine[index + 2] - orig[origStartIndex + index + 2]);

                long tmpres = Tools.fastAbs(br) + Tools.fastAbs(bg) + Tools.fastAbs(bb);

                result += tmpres;

                index += 4;
            }

            return result;
        }

        /// <summary>
        /// spocita fittnesss pro dany jeden radek obrazku
        /// </summary>
        /// <param name="current"></param>
        /// <param name="orig"></param>
        /// <param name="origStartIndex"></param>
        /// <returns></returns>
        public static long ComputeFittnessLine_SumABS(byte[] current, byte[] orig)
        {
            long result = 0;

            int index = 0;
            while (index < current.Length)
            {
                int br = (current[index] - orig[index]);
                int bg = (current[index + 1] - orig[index + 1]);
                int bb = (current[index + 2] - orig[index + 2]);
               
              
                long tmpres = Tools.fastAbs(br) + Tools.fastAbs(bg) + Tools.fastAbs(bb);
                
                result += tmpres;

                index += 4;
            }
       
            return result;
        }

        /// <summary>
        /// spocita fittnesss pro dany jeden radek obrazku
        /// </summary>
        /// <param name="current"></param>
        /// <param name="orig"></param>
        /// <param name="origStartIndex"></param>
        /// <returns></returns>
        public static long ComputeFittnessLine_SumABSPerRow(byte[] current, byte[] orig, int pixelWidht)
        {
            float[] diff = new float[5];
            float[] coef = new float[] { 0.1f, 0.2f, 0.4f,0.2f, 0.1f };

            int countRows = current.Length / (pixelWidht*4);

            int rowIndex = 0;
            long result = 0;

            double res = 0.0;

            for(int r = 0;r< countRows;r++)
            {
                Array.Clear(diff, 0, diff.Length);
                int col = rowIndex;
                for (int index = 0; index < pixelWidht - 4;index++ )
                {
                    // shift
                    for(int i = 1;i< diff.Length;i++)
                    {
                        diff[i-1] = diff[i];
                    }

                    int br = (current[col] - orig[col]);
                    col++;
                    int bg = (current[col ] - orig[col]);
                    col++;
                    int bb = (current[col] - orig[col]);

                    diff[diff.Length-1] = Tools.fastAbs(br)+Tools.fastAbs(bg)+Tools.fastAbs(bb);
                    

                    for (int i = 0; i < diff.Length;i++ )
                    {
                        res += diff[i] * coef[i];
                    }

                    col += 2;
                    //col += 4;
                }

                rowIndex += pixelWidht * 4;
            }

            return (long)res;
           
 //           return result;
        }

        public static long ComputeFittness_Median(byte[] current, byte[] orig)
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


            long result = 0;
            result += (long)(medB.TotalSum + medB.SumStdDev );
            result += (long)(medG.TotalSum + medG.SumStdDev );
            result += (long)(medR.TotalSum + medR.SumStdDev );

            return result;
        }

        public static long ComputeFittness_Basic2(byte[] current, byte[] orig)
        {
            long result = 0;

            int lastDiffBr = 0;
            int lastDiffBg = 0;
            int lastDiffBb = 0;

            int index = 0;
            while (index < orig.Length)
            {
                int br = Tools.fastAbs(current[index] - orig[index]);
                int bg = Tools.fastAbs(current[index + 1] - orig[index + 1]);
                int bb = Tools.fastAbs(current[index + 2] - orig[index + 2]);

                long tmpres = br + bg + bb;
                tmpres += Tools.fastAbs(br - lastDiffBr);
                tmpres += Tools.fastAbs(bg - lastDiffBg);
                tmpres += Tools.fastAbs(bb- lastDiffBb);

                lastDiffBb = bb;
                lastDiffBg = bg;
                lastDiffBr = br;

                result += tmpres;

                index += 4;
            }

            return result;
        }

       
    }
}