using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenArt.AST;
using GenArt.Classes;
using GenArt.Core.AST;

namespace GenArt.Core.Classes
{
    public class PolygonColorPredict
    {
        #region Get color By polygon

        /// <summary>
        /// finalni vypocet barvy
        /// velikost odchykly prumerne barvy od skutecnych urcuje velikost alpha kanalu
        /// </summary>
        /// <param name="avgDiff"></param>
        /// <param name="sumRed"></param>
        /// <param name="sumGreen"></param>
        /// <param name="sumBlue"></param>
        /// <returns></returns>
        private static Color Helper_AvgColorByAlpha(int avgDiff, int sumRed, int sumGreen, int sumBlue)
        {
            int alpha = 255 - avgDiff;
            alpha = 5 + (250 * alpha) / 255;

            return Color.FromArgb(alpha, sumRed, sumGreen, sumBlue);
        }

        /// <summary>
        /// vypocet indexu stredoveho bodu polygonu
        /// </summary>
        /// <param name="points"></param>
        /// <param name="_rawDestImage"></param>
        /// <returns></returns>
        private static int Helper_PolygonMiddlePointIndex(DnaPoint[] points, CanvasARGB _rawDestImage)
        {
            int middleX = 0, middleY = 0;

            for (int index = 0; index < points.Length; index++)
            {
                middleX += points[index].X;
                middleY += points[index].Y;
            }

            middleX /= points.Length;
            middleY /= points.Length;

            return ((middleY * _rawDestImage.WidthPixel) + middleX) << 2;
        }

        /// <summary>
        /// suma vrcholu uprostred hran polygonu
        /// </summary>
        /// <param name="points"></param>
        /// <param name="_rawDestImage"></param>
        /// <param name="computeSumDiff"></param>
        /// <param name="diffRed"></param>
        /// <param name="diffGreen"></param>
        /// <param name="diffBlue"></param>
        /// <param name="sumRed"></param>
        /// <param name="sumGreen"></param>
        /// <param name="sumBlue"></param>
        private static void Helper_PolygonMiddleEdgeColorSum(DnaPoint[] points, CanvasARGB _rawDestImage,
            bool computeSumDiff, int diffRed, int diffGreen, int diffBlue,
            ref int sumRed, ref int sumGreen, ref int sumBlue)
        {
            byte [] imageData = _rawDestImage.Data;

            int x = (points[0].X + points[points.Length - 1].X) / 2;
            int y = (points[0].Y + points[points.Length - 1].Y) / 2;


            int colorIndex = ((y * _rawDestImage.WidthPixel) + x) << 2;
            if (computeSumDiff)
            {
                sumBlue += Tools.fastAbs(diffBlue- imageData[colorIndex]);
                sumGreen += Tools.fastAbs(diffGreen-imageData[colorIndex + 1]);
                sumRed += Tools.fastAbs(diffRed-imageData[colorIndex + 2]);
            }
            else
            {
                sumBlue += imageData[colorIndex];
                sumGreen += imageData[colorIndex + 1];
                sumRed += imageData[colorIndex + 2];
            }


            for (int index = 1; index < points.Length; index++)
            {
                x = (points[index].X + points[index - 1].X) / 2;
                y = (points[index].Y + points[index - 1].Y) / 2;


                colorIndex = ((y * _rawDestImage.WidthPixel) + x) << 2;

                if (computeSumDiff)
                {
                    sumBlue += Tools.fastAbs(diffBlue - imageData[colorIndex]);
                    sumGreen += Tools.fastAbs(diffGreen - imageData[colorIndex + 1]);
                    sumRed += Tools.fastAbs(diffRed - imageData[colorIndex + 2]);
                }
                else
                {
                    sumBlue += imageData[colorIndex];
                    sumGreen += imageData[colorIndex + 1];
                    sumRed += imageData[colorIndex + 2];
                }
            }
        }

        /// <summary>
        /// suma vrcholu polygonu
        /// </summary>
        /// <param name="points"></param>
        /// <param name="_rawDestImage"></param>
        /// <param name="computeSumDiff"></param>
        /// <param name="diffRed"></param>
        /// <param name="diffGreen"></param>
        /// <param name="diffBlue"></param>
        /// <param name="sumRed"></param>
        /// <param name="sumGreen"></param>
        /// <param name="sumBlue"></param>
        private static void Helper_PolygonPointsColorSum(DnaPoint[] points, CanvasARGB _rawDestImage,
            bool computeSumDiff, int diffRed, int diffGreen, int diffBlue,
            ref int sumRed, ref int sumGreen, ref int sumBlue)
        {
            byte [] imageData = _rawDestImage.Data;



            for (int index = 0; index < points.Length; index++)
            {
                int colorIndex = ((points[index].Y * _rawDestImage.WidthPixel) + points[index].X) << 2;

                if (computeSumDiff)
                {
                    sumBlue += Tools.fastAbs(diffBlue - imageData[colorIndex]);
                    sumGreen += Tools.fastAbs(diffGreen - imageData[colorIndex + 1]);
                    sumRed += Tools.fastAbs(diffRed - imageData[colorIndex + 2]);
                }
                else
                {
                    sumBlue += imageData[colorIndex];
                    sumGreen += imageData[colorIndex + 1];
                    sumRed += imageData[colorIndex + 2];
                }
            }
        }

        /// <summary>
        /// suma vrcholu polygonu
        /// </summary>
        /// <param name="points"></param>
        /// <param name="_rawDestImage"></param>
        /// <param name="computeSumDiff"></param>
        /// <param name="diffRed"></param>
        /// <param name="diffGreen"></param>
        /// <param name="diffBlue"></param>
        /// <param name="sumRed"></param>
        /// <param name="sumGreen"></param>
        /// <param name="sumBlue"></param>
        private static void Helper_PolygonMiddlePointsBetweenPointAndMiddle_ColorSum
            (DnaPoint[] points, CanvasARGB _rawDestImage, int indexMiddlePoint,
            bool computeSumDiff, int diffRed, int diffGreen, int diffBlue,
            ref int sumRed, ref int sumGreen, ref int sumBlue)
        {
            byte [] imageData = _rawDestImage.Data;

            int middleX = (indexMiddlePoint % _rawDestImage.Width) / 4;
            int middleY = (indexMiddlePoint / _rawDestImage.Width);


            for (int index = 0; index < points.Length; index++)
            {
                int tmpX = (points[index].X + middleX) / 2;
                int tmpY = (points[index].Y + middleY) / 2;

                int colorIndex = ((tmpY * _rawDestImage.WidthPixel) + tmpX) << 2;

                if (computeSumDiff)
                {
                    sumBlue += Tools.fastAbs(diffBlue - imageData[colorIndex]);
                    sumGreen += Tools.fastAbs(diffGreen - imageData[colorIndex + 1]);
                    sumRed += Tools.fastAbs(diffRed - imageData[colorIndex + 2]);
                }
                else
                {
                    sumBlue += imageData[colorIndex];
                    sumGreen += imageData[colorIndex + 1];
                    sumRed += imageData[colorIndex + 2];
                }
            }
        }

        /// <summary>
        /// pricteni stredove barvy
        /// </summary>
        /// <param name="indexPoint"></param>
        /// <param name="_rawDestImage"></param>
        /// <param name="computeSumDiff"></param>
        /// <param name="diffRed"></param>
        /// <param name="diffGreen"></param>
        /// <param name="diffBlue"></param>
        /// <param name="sumRed"></param>
        /// <param name="sumGreen"></param>
        /// <param name="sumBlue"></param>
        private static void Helper_PolygonMiddlePointColorAdd(int indexPoint, CanvasARGB _rawDestImage,
            bool computeSumDiff, int diffRed, int diffGreen, int diffBlue,
            ref int sumRed, ref int sumGreen, ref int sumBlue)
        {
            byte [] imageData = _rawDestImage.Data;

            if (computeSumDiff)
            {
                sumBlue += Tools.fastAbs(diffBlue - imageData[indexPoint]);
                sumGreen += Tools.fastAbs(diffGreen - imageData[indexPoint + 1]);
                sumRed += Tools.fastAbs(diffRed - imageData[indexPoint + 2]);
            }
            else
            {
                sumBlue += imageData[indexPoint];
                sumGreen += imageData[indexPoint + 1];
                sumRed += imageData[indexPoint + 2];
            }
        }



        public static Color GetColorBy_PointsColor(DnaPoint[] points, CanvasARGB _rawDestImage)
        {
            int sumRed = 0, sumGreen = 0, sumBlue = 0;

            Helper_PolygonPointsColorSum(points, _rawDestImage, false, 0, 0, 0, ref sumRed, ref sumGreen, ref sumBlue);

            return Color.FromArgb(64, sumRed / points.Length, sumGreen / points.Length, sumBlue / points.Length);
        }

        public static Color GetColorBy_PointsColor_AphaDiff(DnaPoint[] points, CanvasARGB _rawDestImage)
        {
            int sumRed = 0, sumGreen = 0, sumBlue = 0;

            Helper_PolygonPointsColorSum(points, _rawDestImage, false, 0, 0, 0, ref sumRed, ref sumGreen, ref sumBlue);

            sumBlue /= points.Length;
            sumGreen /= points.Length;
            sumRed /= points.Length;

            int sumDiffRed = 0, sumDiffGreen = 0, sumDiffBlue = 0;

            Helper_PolygonPointsColorSum(points, _rawDestImage, true, sumRed, sumGreen, sumBlue,
                ref sumDiffRed, ref sumDiffGreen, ref sumDiffBlue);

            sumDiffBlue /= points.Length;
            sumDiffGreen /= points.Length;
            sumDiffRed /= points.Length;

            int avgSumDiff = (sumDiffBlue + sumDiffRed + sumDiffGreen) / 3;

            return Helper_AvgColorByAlpha(avgSumDiff, sumRed, sumGreen, sumBlue);
        }



        public static Color GetColorBy_MiddlePoint(DnaPoint[] points, CanvasARGB _rawDestImage)
        {
            byte [] imageData = _rawDestImage.Data;
            int middleColorIndex = Helper_PolygonMiddlePointIndex(points, _rawDestImage);

            int sumRed = imageData[middleColorIndex + 2];
            int sumGreen = imageData[middleColorIndex + 1];
            int sumBlue = imageData[middleColorIndex];

            int alpha = 255;
            //alpha = Tools.GetRandomNumber(1, 256);

            return Color.FromArgb(alpha, sumRed, sumGreen, sumBlue);
        }


        public static Color GetColorBy_PointsColor_MiddlePoint_AlphaDiff
            (DnaPoint[] points, CanvasARGB _rawDestImage)
        {
            byte [] imageData = _rawDestImage.Data;
            int middleColorIndex = Helper_PolygonMiddlePointIndex(points, _rawDestImage);

            int sumRed = 0, sumGreen = 0, sumBlue = 0;

            Helper_PolygonMiddlePointColorAdd(middleColorIndex, _rawDestImage, false, 0, 0, 0, ref sumRed, ref sumGreen, ref sumBlue);
            Helper_PolygonPointsColorSum(points, _rawDestImage, false, 0, 0, 0, ref sumRed, ref sumGreen, ref sumBlue);

            sumBlue /= points.Length + 1;
            sumGreen /= points.Length + 1;
            sumRed /= points.Length + 1;

            int sumDiffRed = 0, sumDiffGreen = 0, sumDiffBlue = 0;

            Helper_PolygonMiddlePointColorAdd(middleColorIndex, _rawDestImage, true, sumRed, sumGreen, sumBlue, ref sumDiffRed, ref sumDiffGreen, ref sumDiffBlue);
            Helper_PolygonPointsColorSum(points, _rawDestImage, true, sumRed, sumGreen, sumBlue, ref sumDiffRed, ref sumDiffGreen, ref sumDiffBlue);

            sumDiffBlue /= points.Length + 1;
            sumDiffGreen /= points.Length + 1;
            sumDiffRed /= points.Length + 1;

            int avgSumDiff = (sumDiffBlue + sumDiffRed + sumDiffGreen) / 3;
     
            return Helper_AvgColorByAlpha(avgSumDiff, sumRed, sumGreen, sumBlue);
         }

        public static Color GetColorBy_MiddleEdgePoints_MiddlePoint_AlphaDiff(DnaPoint[] points, CanvasARGB _rawDestImage)
        {
            byte [] destImageData = _rawDestImage.Data;

            int middleColorIndex = Helper_PolygonMiddlePointIndex(points, _rawDestImage); 

            int sumRed = 0, sumGreen = 0, sumBlue = 0;

            Helper_PolygonMiddleEdgeColorSum(points, _rawDestImage, false, 0, 0, 0, ref sumRed, ref sumGreen, ref sumBlue);
            Helper_PolygonMiddlePointColorAdd(middleColorIndex, _rawDestImage, false, 0, 0, 0, ref sumRed, ref sumGreen, ref sumBlue);


            sumBlue /= points.Length + 1;
            sumGreen /= points.Length + 1;
            sumRed /= points.Length + 1;

            int sumDiffRed = 0, sumDiffGreen = 0, sumDiffBlue = 0;

            Helper_PolygonMiddleEdgeColorSum(points, _rawDestImage, true, sumRed, sumGreen, sumBlue, ref sumDiffRed, ref sumDiffGreen, ref sumDiffBlue);
            Helper_PolygonMiddlePointColorAdd(middleColorIndex, _rawDestImage, true, sumRed, sumGreen, sumBlue, ref sumDiffRed, ref sumDiffGreen, ref sumDiffBlue);
            
            sumDiffBlue /= points.Length + 1;
            sumDiffGreen /= points.Length + 1;
            sumDiffRed /= points.Length + 1;

            int avgSumDiff = (sumDiffBlue + sumDiffRed + sumDiffGreen) / 3;

            return Helper_AvgColorByAlpha(avgSumDiff, sumRed, sumGreen, sumBlue);
        }

        public static Color GetColorBy_PointsColor_MiddleEdgePoints_MiddlePoint_AlphaDiff(DnaPoint[] points, CanvasARGB _rawDestImage)
        {
            byte [] destImageData = _rawDestImage.Data;

            int middleColorIndex = Helper_PolygonMiddlePointIndex(points, _rawDestImage);

            int sumRed = 0, sumGreen = 0, sumBlue = 0;

            Helper_PolygonMiddleEdgeColorSum(points, _rawDestImage, false, 0, 0, 0, ref sumRed, ref sumGreen, ref sumBlue);
            Helper_PolygonMiddlePointColorAdd(middleColorIndex, _rawDestImage, false, 0, 0, 0, ref sumRed, ref sumGreen, ref sumBlue);
            Helper_PolygonPointsColorSum(points, _rawDestImage, false, 0, 0, 0, ref sumRed, ref sumGreen, ref sumBlue);


            sumBlue /= points.Length*2 + 1;
            sumGreen /= points.Length*2 + 1;
            sumRed /= points.Length*2 + 1;

            int sumDiffRed = 0, sumDiffGreen = 0, sumDiffBlue = 0;

            Helper_PolygonMiddleEdgeColorSum(points, _rawDestImage, true, sumRed, sumGreen, sumBlue, ref sumDiffRed, ref sumDiffGreen, ref sumDiffBlue);
            Helper_PolygonMiddlePointColorAdd(middleColorIndex, _rawDestImage, true, sumRed, sumGreen, sumBlue, ref sumDiffRed, ref sumDiffGreen, ref sumDiffBlue);
            Helper_PolygonPointsColorSum(points, _rawDestImage, true, sumRed, sumGreen, sumBlue, ref sumDiffRed, ref sumDiffGreen, ref sumDiffBlue);


            sumDiffBlue /= points.Length*2 + 1;
            sumDiffGreen /= points.Length*2 + 1;
            sumDiffRed /= points.Length*2 + 1;

            int avgSumDiff = (sumDiffBlue + sumDiffRed + sumDiffGreen) / 3;

            return Helper_AvgColorByAlpha(avgSumDiff, sumRed, sumGreen, sumBlue);
        }

        public static Color GetColorBy_PC_MEP_MEOPAM_MP_AlphaDiff(DnaPoint[] points, CanvasARGB _rawDestImage)
        {
            byte [] destImageData = _rawDestImage.Data;

            int middleColorIndex = Helper_PolygonMiddlePointIndex(points, _rawDestImage);

            int sumRed = 0, sumGreen = 0, sumBlue = 0;

            Helper_PolygonMiddleEdgeColorSum(points, _rawDestImage, false, 0, 0, 0, ref sumRed, ref sumGreen, ref sumBlue);
            Helper_PolygonMiddlePointColorAdd(middleColorIndex, _rawDestImage, false, 0, 0, 0, ref sumRed, ref sumGreen, ref sumBlue);
            Helper_PolygonPointsColorSum(points, _rawDestImage, false, 0, 0, 0, ref sumRed, ref sumGreen, ref sumBlue);
            Helper_PolygonMiddlePointsBetweenPointAndMiddle_ColorSum(points, _rawDestImage,middleColorIndex, false, 0, 0, 0, ref sumRed, ref sumGreen, ref sumBlue);
            

            sumBlue /= points.Length * 3 + 1;
            sumGreen /= points.Length * 3 + 1;
            sumRed /= points.Length * 3 + 1;

            int sumDiffRed = 0, sumDiffGreen = 0, sumDiffBlue = 0;

            Helper_PolygonMiddleEdgeColorSum(points, _rawDestImage, true, sumRed, sumGreen, sumBlue, ref sumDiffRed, ref sumDiffGreen, ref sumDiffBlue);
            Helper_PolygonMiddlePointColorAdd(middleColorIndex, _rawDestImage, true, sumRed, sumGreen, sumBlue, ref sumDiffRed, ref sumDiffGreen, ref sumDiffBlue);
            Helper_PolygonPointsColorSum(points, _rawDestImage, true, sumRed, sumGreen, sumBlue, ref sumDiffRed, ref sumDiffGreen, ref sumDiffBlue);
            Helper_PolygonMiddlePointsBetweenPointAndMiddle_ColorSum(points, _rawDestImage,middleColorIndex, true, sumRed, sumGreen, sumBlue, ref sumDiffRed, ref sumDiffGreen, ref sumDiffBlue);
            

            sumDiffBlue /= points.Length * 3 + 1;
            sumDiffGreen /= points.Length * 3 + 1;
            sumDiffRed /= points.Length * 3 + 1;

            int avgSumDiff = (sumDiffBlue + sumDiffRed + sumDiffGreen) / 3;

            return Helper_AvgColorByAlpha(avgSumDiff, sumRed, sumGreen, sumBlue);
        }


        public static Color GetColorBy_PC_MEP_MEOPAM_MP_AlphaDiff(DnaRectangle rectangle, CanvasARGB _rawDestImage)
        {
            DnaPoint [] points = new DnaPoint[4];
            points[0] = rectangle.StartPoint;
            points[1] = rectangle.StartPoint;
            points[1].X = rectangle.EndPoint.X;
            points[2] = rectangle.EndPoint;
            points[3] = rectangle.StartPoint;
            points[3].Y = rectangle.EndPoint.Y;
            


            byte [] destImageData = _rawDestImage.Data;

            int middleColorIndex = Helper_PolygonMiddlePointIndex(points, _rawDestImage);

            int sumRed = 0, sumGreen = 0, sumBlue = 0;

            Helper_PolygonMiddleEdgeColorSum(points, _rawDestImage, false, 0, 0, 0, ref sumRed, ref sumGreen, ref sumBlue);
            Helper_PolygonMiddlePointColorAdd(middleColorIndex, _rawDestImage, false, 0, 0, 0, ref sumRed, ref sumGreen, ref sumBlue);
            Helper_PolygonPointsColorSum(points, _rawDestImage, false, 0, 0, 0, ref sumRed, ref sumGreen, ref sumBlue);
            Helper_PolygonMiddlePointsBetweenPointAndMiddle_ColorSum(points, _rawDestImage, middleColorIndex, false, 0, 0, 0, ref sumRed, ref sumGreen, ref sumBlue);


            sumBlue /= points.Length * 3 + 1;
            sumGreen /= points.Length * 3 + 1;
            sumRed /= points.Length * 3 + 1;

            int sumDiffRed = 0, sumDiffGreen = 0, sumDiffBlue = 0;

            Helper_PolygonMiddleEdgeColorSum(points, _rawDestImage, true, sumRed, sumGreen, sumBlue, ref sumDiffRed, ref sumDiffGreen, ref sumDiffBlue);
            Helper_PolygonMiddlePointColorAdd(middleColorIndex, _rawDestImage, true, sumRed, sumGreen, sumBlue, ref sumDiffRed, ref sumDiffGreen, ref sumDiffBlue);
            Helper_PolygonPointsColorSum(points, _rawDestImage, true, sumRed, sumGreen, sumBlue, ref sumDiffRed, ref sumDiffGreen, ref sumDiffBlue);
            Helper_PolygonMiddlePointsBetweenPointAndMiddle_ColorSum(points, _rawDestImage, middleColorIndex, true, sumRed, sumGreen, sumBlue, ref sumDiffRed, ref sumDiffGreen, ref sumDiffBlue);


            sumDiffBlue /= points.Length * 3 + 1;
            sumDiffGreen /= points.Length * 3 + 1;
            sumDiffRed /= points.Length * 3 + 1;

            int avgSumDiff = (sumDiffBlue + sumDiffRed + sumDiffGreen) / 3;

            return Helper_AvgColorByAlpha(avgSumDiff, sumRed, sumGreen, sumBlue);
        }

        public static Color GetColorBy_PC_MEP_MEOPAM_MP_AlphaDiff(DnaElipse elipse, CanvasARGB _rawDestImage)
        {
            DnaPoint [] points = new DnaPoint[4];
            points[0] = elipse.StartPoint;
            points[0].X = (short)(elipse.StartPoint.X + elipse.Width/2);
            points[1] = elipse.StartPoint;
            points[1].Y = (short)(elipse.StartPoint.Y + elipse.Height / 2); ;
            points[2].X = points[0].X;
            points[2].Y = (short)(elipse.StartPoint.Y + elipse.Height - 1);
            points[3].X = (short)(elipse.StartPoint.X + elipse.Width-1);
            points[3].Y = points[1].Y;
            



            byte [] destImageData = _rawDestImage.Data;

            int middleColorIndex = Helper_PolygonMiddlePointIndex(points, _rawDestImage);

            int sumRed = 0, sumGreen = 0, sumBlue = 0;

            Helper_PolygonMiddleEdgeColorSum(points, _rawDestImage, false, 0, 0, 0, ref sumRed, ref sumGreen, ref sumBlue);
            Helper_PolygonMiddlePointColorAdd(middleColorIndex, _rawDestImage, false, 0, 0, 0, ref sumRed, ref sumGreen, ref sumBlue);
            Helper_PolygonPointsColorSum(points, _rawDestImage, false, 0, 0, 0, ref sumRed, ref sumGreen, ref sumBlue);
            Helper_PolygonMiddlePointsBetweenPointAndMiddle_ColorSum(points, _rawDestImage, middleColorIndex, false, 0, 0, 0, ref sumRed, ref sumGreen, ref sumBlue);


            sumBlue /= points.Length * 3 + 1;
            sumGreen /= points.Length * 3 + 1;
            sumRed /= points.Length * 3 + 1;

            int sumDiffRed = 0, sumDiffGreen = 0, sumDiffBlue = 0;

            Helper_PolygonMiddleEdgeColorSum(points, _rawDestImage, true, sumRed, sumGreen, sumBlue, ref sumDiffRed, ref sumDiffGreen, ref sumDiffBlue);
            Helper_PolygonMiddlePointColorAdd(middleColorIndex, _rawDestImage, true, sumRed, sumGreen, sumBlue, ref sumDiffRed, ref sumDiffGreen, ref sumDiffBlue);
            Helper_PolygonPointsColorSum(points, _rawDestImage, true, sumRed, sumGreen, sumBlue, ref sumDiffRed, ref sumDiffGreen, ref sumDiffBlue);
            Helper_PolygonMiddlePointsBetweenPointAndMiddle_ColorSum(points, _rawDestImage, middleColorIndex, true, sumRed, sumGreen, sumBlue, ref sumDiffRed, ref sumDiffGreen, ref sumDiffBlue);


            sumDiffBlue /= points.Length * 3 + 1;
            sumDiffGreen /= points.Length * 3 + 1;
            sumDiffRed /= points.Length * 3 + 1;

            int avgSumDiff = (sumDiffBlue + sumDiffRed + sumDiffGreen) / 3;

            return Helper_AvgColorByAlpha(avgSumDiff, sumRed, sumGreen, sumBlue);
        }
        #endregion
    }
}
