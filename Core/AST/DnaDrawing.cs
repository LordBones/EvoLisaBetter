﻿using System.Collections.Generic;
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

        public static Color GetColorByPolygonPointsMiddleEdgePoints(DnaPoint[] points, byte[] _rawDestImage, int width)
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

            sumBlue += _rawDestImage[middleColorIndex]*points.Length;
            sumGreen += _rawDestImage[middleColorIndex + 1] * points.Length;
            sumRed += _rawDestImage[middleColorIndex + 2] * points.Length;


            int x = (points[0].X + points[points.Length - 1].X) / 2;
            int y = (points[0].Y + points[points.Length - 1].Y) / 2;


            int colorIndex = ((y * width) + x) << 2;
            sumBlue += _rawDestImage[colorIndex];
            sumGreen += _rawDestImage[colorIndex + 1];
            sumRed += _rawDestImage[colorIndex + 2];
           

            for (int index = 1; index < points.Length; index++)
            {
                x = (points[index].X + points[index - 1].X)/2;
                y = (points[index].Y + points[index - 1].Y)/2;


                colorIndex = ((y * width) + x) << 2;
                sumBlue += _rawDestImage[colorIndex];
                sumGreen += _rawDestImage[colorIndex + 1];
                sumRed += _rawDestImage[colorIndex + 2];
            }



            sumBlue /= points.Length + points.Length;
            sumGreen /= points.Length + points.Length;
            sumRed /= points.Length + points.Length;

            int sumDiffRed = 0;
            int sumDiffGreen = 0;
            int sumDiffBlue = 0;

            sumDiffBlue = Tools.fastAbs(sumBlue - _rawDestImage[middleColorIndex]) * points.Length;
            sumDiffGreen = Tools.fastAbs(sumGreen - _rawDestImage[middleColorIndex + 1]) * points.Length;
            sumDiffRed = Tools.fastAbs(sumRed - _rawDestImage[middleColorIndex + 2]) * points.Length;


            x = (points[0].X + points[points.Length - 1].X) / 2;
            y = (points[0].Y + points[points.Length - 1].Y) / 2;

            colorIndex = ((y * width) + x) << 2;
            sumDiffBlue = Tools.fastAbs(sumBlue - _rawDestImage[colorIndex]);
            sumDiffGreen = Tools.fastAbs(sumGreen - _rawDestImage[colorIndex + 1]);
            sumDiffRed = Tools.fastAbs(sumRed - _rawDestImage[colorIndex + 2]);

            for (int index = 1; index < points.Length; index++)
            {
                x = (points[index].X + points[index - 1].X) / 2;
                y = (points[index].Y + points[index - 1].Y) / 2;

                colorIndex = ((y * width) + x) << 2;
                sumDiffBlue = Tools.fastAbs(sumBlue - _rawDestImage[colorIndex]);
                sumDiffGreen = Tools.fastAbs(sumGreen - _rawDestImage[colorIndex + 1]);
                sumDiffRed = Tools.fastAbs(sumRed - _rawDestImage[colorIndex + 2]);
            }

            sumDiffBlue = sumDiffBlue / (points.Length + points.Length);
            sumDiffGreen = sumDiffGreen / (points.Length + points.Length);
            sumDiffRed = sumDiffRed / (points.Length + points.Length);

            int avgSumDiff = (sumDiffBlue + sumDiffRed + sumDiffGreen) / 3;
            //int avgSumDiff = (int)Math.Sqrt((sumDiffBlue * sumDiffBlue + sumDiffRed * sumDiffRed + sumDiffGreen * sumDiffGreen) / 3);

            int alpha = 254 - (Math.Min(avgSumDiff, 127) << 1);
            alpha = 5 + (249
                * alpha) / 254;

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
                        //Color nearColor = GetColorByPolygonPoints(newPolygon.Points, _rawDestImage, width);
                        Color nearColor = GetColorByPolygonPointsMiddleEdgePoints(newPolygon.Points, _rawDestImage, width);

                        newPolygon.Brush.SetByColor(nearColor);
                    }

                    //newPolygon.InitTestPolygon();

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