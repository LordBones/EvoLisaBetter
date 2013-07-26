using System.Collections.Generic;
using System.Xml.Serialization;
using GenArt.Classes;
using System;
using System.Drawing;

namespace GenArt.AST
{
    [Serializable]
    public class DnaDrawing
    {
        public DnaPolygon [] Polygons;// { get; set; }

        [XmlIgnore]
        public bool IsDirty { get; private set; }

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

        public DnaDrawing()
        {
            Polygons = new DnaPolygon[0];
        }

        public void SetDirty()
        {
            IsDirty = true;
        }

        public void Init()
        {
            Polygons = new DnaPolygon[Settings.ActivePolygonsMin];

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
            var drawing = new DnaDrawing();
            drawing.Polygons = new DnaPolygon[Polygons.Length];

            for (int index = 0; index < Polygons.Length; index++)
                drawing.Polygons[index] = Polygons[index].Clone();

            return drawing;
        }


        public void Mutate(byte [] _rawDestImage = null, int width = 0)
        {
            if (Tools.WillMutate(Settings.ActiveRemovePolygonMutationRate))
                RemovePolygon();
            
            else if (Tools.WillMutate(Settings.ActiveAddPolygonMutationRate))
                AddPolygon(_rawDestImage,width);

            else if (Tools.WillMutate(Settings.ActiveMovePolygonMutationRate))
                SwapPolygon();
            
            
            {
                for (int index = 0; index < Polygons.Length; index++)
                    Polygons[index].Mutate(this, _rawDestImage, width);
            }

        }

        public void MutateBetter(byte[] _rawDestImage = null, int width = 0, DnaPoint [] edgePoints = null)
        {
            int mutateChange = Tools.GetRandomNumber(0, 1000);

            if(mutateChange <100)
            AddPolygon(_rawDestImage, width,edgePoints);
            else if (mutateChange < 400)
                RemovePolygon();
            else if (mutateChange < 600)
                SwapPolygon();

            else
            {
                while (!this.IsDirty)
                {
                    for (int index = 0; index < Polygons.Length; index++)
                        Polygons[index].Mutate(this, _rawDestImage, width, edgePoints);

                    if (Polygons.Length == 0)
                        break;
                }
            }
            
           

        }

        public void SwapPolygon()
        {
            if (Polygons.Length < 1)
                return;

            int index = Tools.GetRandomNumber(0, Polygons.Length - 1);
            int index2 = Tools.GetRandomNumber(0, Polygons.Length - 1);

            DnaPolygon poly = Polygons[index];
            Polygons[index] = Polygons[index2];
            Polygons[index] = poly;


            

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

        public static Color GetColorByPolygonPoints2(DnaPoint[] points,byte [] _rawDestImage, int width)
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

        public static Color GetColorByPolygonPoints3(DnaPoint[] points, byte[] _rawDestImage, int width)
        {
            int sumRed = 0;
            int sumGreen = 0;
            int sumBlue = 0;


            for (int index = 0; index < points.Length; index++)
            {
                int colorIndex = ((points[index].Y * width) + points[index].X) << 2;
                sumRed += _rawDestImage[colorIndex];
                sumGreen += _rawDestImage[colorIndex + 1];
                sumBlue += _rawDestImage[colorIndex + 2];
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
                sumDiffRed += Tools.fastAbs(sumRed - _rawDestImage[colorIndex]);
                sumDiffGreen += Tools.fastAbs(sumGreen - _rawDestImage[colorIndex + 1]);
                sumDiffBlue += Tools.fastAbs(sumBlue -  _rawDestImage[colorIndex + 2]);
            }

            sumDiffBlue /= points.Length;
            sumDiffGreen /= points.Length;
            sumDiffRed /= points.Length;
          
            //int avgSumDiff = (sumDiffBlue + sumDiffRed + sumDiffGreen) / 3;
            int avgSumDiff = (int)Math.Sqrt( (sumDiffBlue * sumDiffBlue + sumDiffRed * sumDiffRed + sumDiffGreen * sumDiffGreen) / 3);

            int alpha = 254 - (Math.Min(avgSumDiff, 127) << 1);

            return Color.FromArgb(alpha, sumRed, sumGreen , sumBlue );
        }

        public static Color GetColorByPolygonPoints4(DnaPoint[] points, byte[] _rawDestImage, int width)
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
                int tmp = sumBlue - _rawDestImage[colorIndex];
                sumDiffBlue += tmp*tmp;
                tmp = sumGreen - _rawDestImage[colorIndex+1];
                sumDiffGreen += tmp*tmp;
                tmp = sumRed - _rawDestImage[colorIndex+2];
                sumDiffRed += tmp * tmp;
            }

            sumDiffBlue = (int)Math.Sqrt(sumDiffBlue /points.Length);
            sumDiffGreen = (int)Math.Sqrt(sumDiffGreen / points.Length);
            sumDiffRed = (int)Math.Sqrt(sumDiffRed / points.Length);

            //int avgSumDiff = (sumDiffBlue + sumDiffRed + sumDiffGreen) / 3;
            int avgSumDiff = (int)Math.Sqrt((sumDiffBlue * sumDiffBlue + sumDiffRed * sumDiffRed + sumDiffGreen * sumDiffGreen) / 3);

            int alpha = 254 - (Math.Min(avgSumDiff, 127) << 1);

            return Color.FromArgb(alpha, sumRed, sumGreen, sumBlue);
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
            alpha = Tools.GetRandomNumber(1, 254);

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

            sumBlue /= points.Length+1;
            sumGreen /= points.Length+1;
            sumRed /= points.Length+1;

            int sumDiffRed = 0;
            int sumDiffGreen = 0;
            int sumDiffBlue = 0;

            int tmpMiddle = sumBlue - _rawDestImage[middleColorIndex];
            sumDiffBlue += tmpMiddle * tmpMiddle;
            tmpMiddle = sumGreen - _rawDestImage[middleColorIndex + 1];
            sumDiffGreen += tmpMiddle * tmpMiddle;
            tmpMiddle = sumRed - _rawDestImage[middleColorIndex + 2];
            sumDiffRed += tmpMiddle * tmpMiddle;


            for (int index = 0; index < points.Length; index++)
            {
                int colorIndex = ((points[index].Y * width) + points[index].X) << 2;
                int tmp = sumBlue - _rawDestImage[colorIndex];
                sumDiffBlue += tmp * tmp;
                tmp = sumGreen - _rawDestImage[colorIndex + 1];
                sumDiffGreen += tmp * tmp;
                tmp = sumRed - _rawDestImage[colorIndex + 2];
                sumDiffRed += tmp * tmp;
            }

            sumDiffBlue = (int)Math.Sqrt(sumDiffBlue / (points.Length+1));
            sumDiffGreen = (int)Math.Sqrt(sumDiffGreen / (points.Length + 1));
            sumDiffRed = (int)Math.Sqrt(sumDiffRed / (points.Length + 1));

            //int avgSumDiff = (sumDiffBlue + sumDiffRed + sumDiffGreen) / 3;
            int avgSumDiff = (int)Math.Sqrt((sumDiffBlue * sumDiffBlue + sumDiffRed * sumDiffRed + sumDiffGreen * sumDiffGreen) / 3);

            int alpha = 254 - (Math.Min(avgSumDiff, 127) << 1);
            alpha = 5+(32*alpha)/254;

            return Color.FromArgb(alpha, sumRed, sumGreen, sumBlue);
        }


        public void AddPolygon(byte [] _rawDestImage = null, int width =0, DnaPoint [] edgePoints = null)
        {
            if (Polygons.Length < Settings.ActivePolygonsMax )
            {
                if (PointCount < Settings.ActivePointsMax + Settings.ActivePointsPerPolygonMin)
                {
                    var newPolygon = new DnaPolygon();
                    newPolygon.Init(edgePoints);

                    if (_rawDestImage != null)
                    {
                        Color nearColor = GetColorByPolygonMiddle(newPolygon.Points, _rawDestImage, width);
                        newPolygon.Brush.SetByColor(nearColor);
                    }
                    //else
                    //{
                    //    int i = 0;
                    //    int b = 5;
                    //    i += b;
                    //}

                    //newPolygon.InitTestPolygon();

                    //int index = Tools.GetRandomNumber(0, Polygons.Count);

                    //Polygons.Insert(index, newPolygon);

                    DnaPolygon [] polygons = new DnaPolygon[Polygons.Length + 1];
                    Array.Copy(Polygons, polygons, Polygons.Length);

                    //for (int index = 0; index < Polygons.Length; index++)
                    //    polygons[index] = Polygons[index];

                    polygons[polygons.Length - 1] = newPolygon;

                    Polygons = polygons;

                    SetDirty();
                }
            }
        }
    }
}