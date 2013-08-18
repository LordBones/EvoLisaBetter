using System.Collections.Generic;
using System.Xml.Serialization;
using GenArt.Classes;
using System;
using System.Drawing;
using GenArt.Core.Classes;

namespace GenArt.AST
{
    public class DnaDrawing
    {
        public DnaPolygon [] Polygons;// { get; set; }
        private short _maxWidth = 0;
        private short _maxHeight = 0;

        public DnaBrush BackGround; 

        public bool IsDirty { get; private set; }

        public short Width { get{return _maxWidth;} }
        public short Height { get{return _maxHeight;} }


        public int PointCount
        {
            get
            {
                int pointCount = 0;
                for (int index = 0; index < Polygons.Length;index++ )
                    pointCount += Polygons[index].Points.Length;

                return pointCount;
            }
        }

        public long GetSumSize
        {
            get
            {
                long sum = 0;
                for (int i = 0; i < this.Polygons.Length; i++)
                {
                    sum += this.Polygons[i].GetPixelSizePolygon();
                }
                return sum;
            }
        }

        public DnaDrawing(short maxWidth, short maxHeight)
        {
            Polygons = new DnaPolygon[0];
            BackGround = new DnaBrush(255, 0, 0, 0);
            _maxHeight = maxHeight;
            _maxWidth = maxWidth;
        }

        public void SetDirty()
        {
            IsDirty = true;
        }

        public void Init()
        {
            Polygons = new DnaPolygon[Settings.ActivePolygonsMin];
            BackGround = new DnaBrush(255, 0, 0, 0);
            BackGround.InitRandomWithoutAlpha();

            for (int i = 0; i < Settings.ActivePolygonsMin; i++)
                AddPolygon();

            SetDirty();
        }

        public bool HasSomePolygonBadAngles()
        {
            for (int index = 0; index < this.Polygons.Length; index++)
            {
                if (!this.Polygons[index].IsNotSmallAngles(this.Polygons[index].Points))
                {
                    return true;
                }
            }
            return false;
        }

        public DnaDrawing Clone()
        {
            var drawing = new DnaDrawing(this._maxWidth,this._maxHeight);
            drawing.Polygons = new DnaPolygon[Polygons.Length];
            drawing.BackGround = BackGround;

            for (int index = 0; index < Polygons.Length; index++)
                drawing.Polygons[index] = Polygons[index].Clone();

            return drawing;
        }


        public void Mutate(CanvasBGRA destImage = null)
        {
            if (Tools.WillMutate(Settings.ActiveRemovePolygonMutationRate))
                RemovePolygon();
            
            else if (Tools.WillMutate(Settings.ActiveAddPolygonMutationRate))
                AddPolygon(destImage);

            else if (Tools.WillMutate(Settings.ActiveMovePolygonMutationRate))
                SwapPolygon();
            
            
            {
                for (int index = 0; index < Polygons.Length; index++)
                    Polygons[index].Mutate(this, destImage);
            }

        }

        public void MutateBetter(CanvasBGRA destImage = null, ImageEdges edgePoints = null)
        {
            /// k mutaci pozadi dochazi pouze jednou 
            if (Tools.GetRandomNumber(0, 10) == 9)
                BackGround.MutateRGBOldWithoutAlpha(this);

             do
             {
                 int mutateChange = Tools.GetRandomNumber(0, 1001);



                 if (mutateChange < 100)
                 {
                     if (Settings.ActivePolygonsMax <= this.Polygons.Length)
                         RemovePolygon();
                     AddPolygon(destImage, edgePoints);
                 }
                 else if (mutateChange < 200)
                     RemovePolygon();
                 else if (mutateChange < 300)
                     SwapPolygon2();
               

                 else
                 {
                     while (!this.IsDirty)
                     {
                         if (Polygons.Length == 0)
                             break;

                         //for (int index = 0; index < Polygons.Length; index++)
                         //    Polygons[index].Mutate(this,destImage, edgePoints);

                         if (Tools.GetRandomNumber(0, 2) >= 1) 
                         {
                             int index = Tools.GetRandomNumber(0, Polygons.Length);
                             Polygons[index].Mutate(this, destImage, edgePoints);
                         }
                         else
                         {
                             int tindex = Tools.GetRandomNumber(0, Polygons.Length);
                             DnaBrush brush = Polygons[tindex].Brush;
                             brush.MutateRGBOld(this);
                             Polygons[tindex].Brush = brush;
                             //Polygons[tindex].Brush.MutateRGBOld(this);
                         }
                     }
                 }

                

             } while (Tools.GetRandomNumber(1,11) <= 5);



        }

        bool IsTrinagleInterleaving(DnaPoint [] tri,DnaPoint [] tri2 )
        {
            int startX = int.MaxValue;
            int startY = int.MaxValue;
            int endX = 0;
            int endY = 0;

            int startX2 = int.MaxValue;
            int startY2 = int.MaxValue;
            int endX2 = 0;
            int endY2 = 0;

            for (int index = 0; index < 3; index++)
            {
                DnaPoint p = tri[0];
                DnaPoint p2 = tri2[0];

                if (p.X < startX) startX = p.X;
                if (p.Y < startY) startY = p.Y;
                if (p.X > endX) endX = p.X;
                if (p.Y > endY) endY = p.Y;

                if (p2.X < startX2) startX2 = p2.X;
                if (p2.Y < startY2) startY2 = p2.Y;
                if (p2.X > endX2) endX2 = p2.X;
                if (p2.Y > endY2) endY2 = p2.Y;

            }

            if ((startY <= startY2 && endY <= startY2) ||
               (startY >= endY2 && endY >= endY2) ||
                (startX <= startX2 && endX <= startX2) ||
               (startY >= endX2 && endX >= endX2)
                )
            {
                return false;
            }

            return true;
        }
        static int c = 0;
        public void SwapPolygon2()
        {
             
            if (Polygons.Length < 2)
                return;

            //int index = Tools.GetRandomNumber(0, Polygons.Length - 1);
            //int index2 = Tools.GetRandomNumber(0, Polygons.Length - 1);

            //DnaPolygon poly = Polygons[index];
            //Polygons[index] = Polygons[index2];
            //Polygons[index] = poly;

            int index = Tools.GetRandomNumber(0, Polygons.Length);
            bool swapUp = Tools.GetRandomNumber(0, 2) < 1;

            if (swapUp && index + 1 >= Polygons.Length) swapUp = false;
            else if (!swapUp && index == 0) swapUp = true;

            DnaPolygon poly = Polygons[index];

            if (swapUp)
            {
                int tmpIndex = index-1;
                while (tmpIndex >= 0 && !IsTrinagleInterleaving(Polygons[tmpIndex].Points, Polygons[index].Points))
                 tmpIndex--;

                if (tmpIndex < 0)
                {
                    tmpIndex = index+1;
                    while (tmpIndex < Polygons.Length && !IsTrinagleInterleaving(Polygons[tmpIndex].Points, Polygons[index].Points))
                        tmpIndex++;

                    // nema smysl prohazovat dva polygony nikde se neprekryvaji
                    if (tmpIndex >= Polygons.Length)
                    { c++;return;}
                }

                Polygons[index] = Polygons[tmpIndex];
                Polygons[tmpIndex] = poly;
            }
            else
            {
                int tmpIndex = index + 1;
                while (tmpIndex < Polygons.Length && !IsTrinagleInterleaving(Polygons[tmpIndex].Points, Polygons[index].Points))
                    tmpIndex++;

                if (tmpIndex >= Polygons.Length)
                {
                    tmpIndex = index - 1;
                    while (tmpIndex >=0 && !IsTrinagleInterleaving(Polygons[tmpIndex].Points, Polygons[index].Points))
                        tmpIndex--;

                    // nema smysl prohazovat dva polygony nikde se neprekryvaji
                    if (tmpIndex < 0)
                    {c++; return;}
                }


                Polygons[index] = Polygons[tmpIndex];
                Polygons[tmpIndex] = poly;

            }


             
          
            {

                SetDirty();
            }
            Console.WriteLine(c);
        }

        public void SwapPolygon()
        {
            if (Polygons.Length < 2)
                return;

            //int index = Tools.GetRandomNumber(0, Polygons.Length - 1);
            //int index2 = Tools.GetRandomNumber(0, Polygons.Length - 1);

            //DnaPolygon poly = Polygons[index];
            //Polygons[index] = Polygons[index2];
            //Polygons[index] = poly;

            int index = Tools.GetRandomNumber(0, Polygons.Length);
            bool swapUp = Tools.GetRandomNumber(0,2) < 1;

            if (swapUp && index + 1 >= Polygons.Length) swapUp = false;
            else if (!swapUp && index == 0) swapUp = true;

            DnaPolygon poly = Polygons[index];

            if (swapUp)
            {
                Polygons[index] = Polygons[index + 1];
                Polygons[index + 1] = poly;
            }
            else
            {
                Polygons[index] = Polygons[index - 1];
                Polygons[index - 1] = poly;
           
            }

            

            //int indexfrom = Tools.GetRandomNumber(0, Polygons.Length-1);
            //int indexTo = Tools.GetRandomNumber(0, Polygons.Length - 1);
            //if (indexTo != indexfrom)
            //{
            //    if (indexfrom > indexTo)
            //    {
            //        indexTo++;
            //        DnaPolygon tmpPolygon = Polygons[indexfrom];

            //        for (int i = indexfrom; i > indexTo; i--)
            //            Polygons[i] = Polygons[i - 1];

            //        Polygons[indexTo] = tmpPolygon;
            //    }
            //    else if (indexfrom < indexTo)
            //    {
            //        indexTo++;
            //        DnaPolygon tmpPolygon = Polygons[indexfrom];

            //        for (int i = indexfrom; i < indexTo; i++)
            //            Polygons[i] = Polygons[i + 1];

            //        Polygons[indexTo] = tmpPolygon;
            //    }
            {

                SetDirty();
            }
        }

        public void RemovePolygon()
        {
            if (Polygons.Length > Settings.ActivePolygonsMin)
            {
                int index = Tools.GetRandomNumber(0, Polygons.Length);

                DnaPolygon [] polygons = new DnaPolygon[Polygons.Length -1];

                //if (index > 0)
                //    Array.Copy(Polygons, polygons, index);
                for (int i = 0; i < index; i++)
                    polygons[i] = Polygons[i];

                for (int i = index; i < polygons.Length; i++)
                    polygons[i] = Polygons[i+1];

                Polygons = polygons;
                SetDirty();
            }
        }

        public static Color GetColorByPolygonPointsBasic(DnaPoint[] points,byte [] _rawDestImage, int width)
        {
            int sumRed = 0;
            int sumGreen = 0;
            int sumBlue = 0;


            for (int index = 0; index < points.Length; index++)
            {
                int colorIndex = ((points[index].Y * width) + points[index].X) << 2;
                sumBlue += _rawDestImage[colorIndex];
                sumGreen += _rawDestImage[colorIndex + 1];
                sumRed += _rawDestImage[colorIndex + 2];
            }

            return Color.FromArgb(64, sumRed / points.Length, sumGreen / points.Length, sumBlue / points.Length);
        }

        public static Color GetColorByPolygonPointsWithAphaDiff(DnaPoint[] points, byte[] _rawDestImage, int width)
        {
            int sumRed = 0;
            int sumGreen = 0;
            int sumBlue = 0;


            for (int index = 0; index < points.Length; index++)
            {
                int colorIndex = ((points[index].Y * width) + points[index].X) << 2;
                sumBlue += _rawDestImage[colorIndex];
                sumGreen += _rawDestImage[colorIndex + 1];
                sumRed += _rawDestImage[colorIndex + 2];
            }

            sumBlue /= points.Length;
            sumGreen /= points.Length;
            sumRed /= points.Length;

            int sumDiffRed = 0;
            int sumDiffGreen = 0;
            int sumDiffBlue = 0;


            for (int index = 0; index < points.Length; index++)
            {
                int colorIndex = ((points[index].Y * width) + points[index].X) << 2;
                sumDiffBlue += Tools.fastAbs(sumRed - _rawDestImage[colorIndex]);
                sumDiffGreen += Tools.fastAbs(sumGreen - _rawDestImage[colorIndex + 1]);
                sumDiffRed += Tools.fastAbs(sumBlue -  _rawDestImage[colorIndex + 2]);
            }

            sumDiffBlue /= points.Length;
            sumDiffGreen /= points.Length;
            sumDiffRed /= points.Length;
          
            //int avgSumDiff = (sumDiffBlue + sumDiffRed + sumDiffGreen) / 3;
            int avgSumDiff = (int)Math.Sqrt( (sumDiffBlue * sumDiffBlue + sumDiffRed * sumDiffRed + sumDiffGreen * sumDiffGreen) / 3);

            int alpha = 254 - (Math.Min(avgSumDiff, 127) << 1);

            return Color.FromArgb(alpha, sumRed, sumGreen , sumBlue );
        }

        

        public static Color GetColorByPolygonMiddle(DnaPoint[] points, byte[] _rawDestImage, int width)
        {
            int middleX = 0;
            int middleY = 0;

            for (int index = 0; index < points.Length; index++)
            {
                middleX += points[index].X;
                middleY += points[index].Y;
            }

            middleX /= points.Length;
            middleY /= points.Length;

            int middleColorIndex = ((middleY * width) + middleX) << 2;

            int sumRed = 0;
            int sumGreen = 0;
            int sumBlue = 0;

            sumBlue += _rawDestImage[middleColorIndex];
            sumGreen += _rawDestImage[middleColorIndex + 1];
            sumRed += _rawDestImage[middleColorIndex + 2];
            
            int alpha = 0;
            alpha = Tools.GetRandomNumber(1, 256);

            return Color.FromArgb(alpha, sumRed, sumGreen, sumBlue);
        }


        public static Color GetColorByPolygonPoints(DnaPoint[] points, byte[] _rawDestImage, int width)
        {
            int middleX = 0;
            int middleY = 0;

            for (int index = 0; index < points.Length; index++)
            {
                middleX += points[index].X;
                middleY += points[index].Y;
            }

            middleX /= points.Length;
            middleY /= points.Length;

            int middleColorIndex = ((middleY * width) + middleX) << 2;

            int sumRed = 0;
            int sumGreen = 0;
            int sumBlue = 0;

            sumBlue += _rawDestImage[middleColorIndex];
            sumGreen += _rawDestImage[middleColorIndex + 1];
            sumRed += _rawDestImage[middleColorIndex + 2];


            for (int index = 0; index < points.Length; index++)
            {
                int colorIndex = ((points[index].Y * width) + points[index].X) << 2;
                sumBlue += _rawDestImage[colorIndex];
                sumGreen += _rawDestImage[colorIndex + 1];
                sumRed += _rawDestImage[colorIndex + 2];
            }

            sumBlue /= points.Length + 1;
            sumGreen /= points.Length + 1;
            sumRed /= points.Length + 1;

            int sumDiffRed = 0;
            int sumDiffGreen = 0;
            int sumDiffBlue = 0;

            sumDiffBlue = Tools.fastAbs(sumBlue - _rawDestImage[middleColorIndex]);
            sumDiffGreen = Tools.fastAbs(sumGreen - _rawDestImage[middleColorIndex + 1]);
            sumDiffRed = Tools.fastAbs(sumRed - _rawDestImage[middleColorIndex + 2]);


            for (int index = 0; index < points.Length; index++)
            {
                int colorIndex = ((points[index].Y * width) + points[index].X) << 2;
                sumDiffBlue = Tools.fastAbs(sumBlue - _rawDestImage[colorIndex]);
                sumDiffGreen = Tools.fastAbs(sumGreen - _rawDestImage[colorIndex + 1]);
                sumDiffRed = Tools.fastAbs(sumRed - _rawDestImage[colorIndex + 2]);
            }

            sumDiffBlue = sumDiffBlue / (points.Length + 1);
            sumDiffGreen = sumDiffGreen / (points.Length + 1);
            sumDiffRed = sumDiffRed / (points.Length + 1);

            int avgSumDiff = (sumDiffBlue + sumDiffRed + sumDiffGreen) / 3;
            //int avgSumDiff = (int)Math.Sqrt((sumDiffBlue * sumDiffBlue + sumDiffRed * sumDiffRed + sumDiffGreen * sumDiffGreen) / 3);

            int alpha = 254 - (Math.Min(avgSumDiff, 127) << 1);
            alpha = 5 + (249 * alpha) / 254;

            return Color.FromArgb(alpha, sumRed, sumGreen, sumBlue);
        }

        public static Color GetColorByPolygonPointsMiddleEdgePoints(DnaPoint[] points, CanvasBGRA destImage)
        {
            byte [] destImageData = destImage.Data;

            int middleX = 0;
            int middleY = 0;

            for (int index = 0; index < points.Length; index++)
            {
                middleX += points[index].X;
                middleY += points[index].Y;
            }

            middleX /= points.Length;
            middleY /= points.Length;

            int middleColorIndex = ((middleY * destImage.WidthPixel) + middleX) << 2;

            int sumRed = 0;
            int sumGreen = 0;
            int sumBlue = 0;

            sumBlue += destImageData[middleColorIndex]*points.Length;
            sumGreen += destImageData[middleColorIndex + 1] * points.Length;
            sumRed += destImageData[middleColorIndex + 2] * points.Length;


            int x = (points[0].X + points[points.Length - 1].X) / 2;
            int y = (points[0].Y + points[points.Length - 1].Y) / 2;


            int colorIndex = ((y *destImage.WidthPixel) + x) << 2;
            sumBlue += destImageData[colorIndex];
            sumGreen += destImageData[colorIndex + 1];
            sumRed += destImageData[colorIndex + 2];
           

            for (int index = 1; index < points.Length; index++)
            {
                x = (points[index].X + points[index - 1].X)/2;
                y = (points[index].Y + points[index - 1].Y)/2;


                colorIndex = ((y * destImage.WidthPixel) + x) << 2;
                sumBlue += destImageData[colorIndex];
                sumGreen += destImageData[colorIndex + 1];
                sumRed += destImageData[colorIndex + 2];
            }



            sumBlue /= points.Length + points.Length;
            sumGreen /= points.Length + points.Length;
            sumRed /= points.Length + points.Length;

            int sumDiffRed = 0;
            int sumDiffGreen = 0;
            int sumDiffBlue = 0;

            sumDiffBlue = Tools.fastAbs(sumBlue - destImageData[middleColorIndex]) * points.Length;
            sumDiffGreen = Tools.fastAbs(sumGreen - destImageData[middleColorIndex + 1]) * points.Length;
            sumDiffRed = Tools.fastAbs(sumRed - destImageData[middleColorIndex + 2]) * points.Length;


            x = (points[0].X + points[points.Length - 1].X) / 2;
            y = (points[0].Y + points[points.Length - 1].Y) / 2;

            colorIndex = ((y * destImage.WidthPixel) + x) << 2;
            sumDiffBlue = Tools.fastAbs(sumBlue - destImageData[colorIndex]);
            sumDiffGreen = Tools.fastAbs(sumGreen - destImageData[colorIndex + 1]);
            sumDiffRed = Tools.fastAbs(sumRed - destImageData[colorIndex + 2]);

            for (int index = 1; index < points.Length; index++)
            {
                x = (points[index].X + points[index - 1].X) / 2;
                y = (points[index].Y + points[index - 1].Y) / 2;

                colorIndex = ((y * destImage.WidthPixel) + x) << 2;
                sumDiffBlue = Tools.fastAbs(sumBlue - destImageData[colorIndex]);
                sumDiffGreen = Tools.fastAbs(sumGreen - destImageData[colorIndex + 1]);
                sumDiffRed = Tools.fastAbs(sumRed - destImageData[colorIndex + 2]);
            }

            sumDiffBlue = sumDiffBlue / (points.Length + points.Length);
            sumDiffGreen = sumDiffGreen / (points.Length + points.Length);
            sumDiffRed = sumDiffRed / (points.Length + points.Length);

            int avgSumDiff = (sumDiffBlue + sumDiffRed + sumDiffGreen) / 3;
            //int avgSumDiff = (int)Math.Sqrt((sumDiffBlue * sumDiffBlue + sumDiffRed * sumDiffRed + sumDiffGreen * sumDiffGreen) / 3);

            int alpha = 254 - (Math.Min(avgSumDiff, 127) << 1);
            alpha = 5 + (128
                * alpha) / 254;

            return Color.FromArgb(alpha, sumRed, sumGreen, sumBlue);
        }


        public void AddPolygon(CanvasBGRA _rawDestImage = null, ImageEdges edgePoints = null)
        {
            if (Polygons.Length < Settings.ActivePolygonsMax )
            {
                if (PointCount < Settings.ActivePointsMax + Settings.ActivePointsPerPolygonMin)
                {
                    var newPolygon = new DnaPolygon();
                    newPolygon.Init(edgePoints);
                    //newPolygon.Init(null);

                    if (_rawDestImage != null)
                    {
                        //Color nearColor = GetColorByPolygonPoints(newPolygon.Points, _rawDestImage, width);
                        Color nearColor = GetColorByPolygonPointsMiddleEdgePoints(newPolygon.Points, _rawDestImage);

                        newPolygon.Brush.SetByColor(nearColor);
                        //newPolygon.Brush.InitRandom();
                    }

                    //int index = Tools.GetRandomNumber(0, Polygons.Count);

                    //Polygons.Insert(index, newPolygon);

                    DnaPolygon [] polygons = new DnaPolygon[Polygons.Length + 1];
                    Array.Copy(Polygons, polygons, Polygons.Length);

                    polygons[polygons.Length - 1] = newPolygon;


                    

                    Polygons = polygons;

                    SetDirty();
                }
            }
        }
    }
}