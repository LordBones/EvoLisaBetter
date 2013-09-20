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
                AddPolygon(null);

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
                RemovePolygon(null);
            
            else if (Tools.WillMutate(Settings.ActiveAddPolygonMutationRate))
                AddPolygon(null,destImage);

            else if (Tools.WillMutate(Settings.ActiveMovePolygonMutationRate))
                SwapPolygon();
            
            
            {
                for (int index = 0; index < Polygons.Length; index++)
                    Polygons[index].Mutate(this, destImage);
            }

        }

        public void MutateBetter(ErrorMatrix errorMatrix, CanvasBGRA destImage = null, ImageEdges edgePoints = null)
        {
            /// k mutaci pozadi dochazi pouze jednou 
            //if (Tools.GetRandomNumber(0, 10) == 9)
            //    BackGround.MutateRGBOldWithoutAlpha(this);

            do
            {
                this.IsDirty = false;
                int mutateChange = Tools.GetRandomNumber(0, 1001);



                if (mutateChange < 200)
                {
                    //if (Settings.ActivePolygonsMax <= this.Polygons.Length)
                    //    RemovePolygon();
                    if (Settings.ActivePolygonsMax > this.Polygons.Length)
                    {
                        AddPolygon(errorMatrix, destImage, edgePoints);
                        continue;
                    }
                }
                if (mutateChange < 400)
                {
                    RemovePolygon(errorMatrix);
                    continue;
                }
                if (mutateChange < 600)
                {
                    //SwapPolygon();
                    if (SwapPolygon2())
                        continue;
                }

                // else

                while (!this.IsDirty)
                {
                    if (Polygons.Length == 0)
                        break;

                    //for (int index = 0; index < Polygons.Length; index++)
                    //    Polygons[index].Mutate(this,destImage, edgePoints);

                    //if (Tools.GetRandomNumber(0, 2) >= 1)
                    if (mutateChange < 800)
                    {
                        //int index = GetRNDIndexPolygonBySize(this.Polygons);
                        //int index = GetRNDIndexPolygonByLive(this.Polygons);

                        int ? tmpIndex = GetRNDPolygonIndexOnlyPoints(errorMatrix);
                        if (!tmpIndex.HasValue) throw new NotImplementedException("sem se to nesmi dostat.");

                        int index = tmpIndex.Value;

                        //int index = Tools.GetRandomNumber(0, Polygons.Length);
                        Polygons[index].Mutate(this, destImage, edgePoints);
                   
                    }
                    else
                    {
                        int ? tmpIndex = GetRNDPolygonIndex(errorMatrix); 
                        if (!tmpIndex.HasValue) throw new NotImplementedException("sem se to nesmi dostat.");

                        int tindex = tmpIndex.Value;
                        //int tindex = Tools.GetRandomNumber(0, Polygons.Length);
                        DnaBrush brush = Polygons[tindex].Brush;
                        brush.MutateRGBOld(this);
                        Polygons[tindex].Brush = brush;
                       
                        //Polygons[tindex].Brush.MutateRGBOld(this);
                    }
                }

            } while (false);
            // while (Tools.GetRandomNumber(1, 11) <= 5);
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
                DnaPoint p = tri[index];
                DnaPoint p2 = tri2[index];

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
               (startX >= endX2 && endX >= endX2)
                )
            {
                return false;
            }

            return true;
        }

        bool IsTrinagleInterleaving2(DnaPoint[] tri, DnaPoint[] tri2)
        {
            if ( GraphicFunctions.IsPointInTriangle(tri[0], tri[1], tri[2], tri2[1]) ||
                 GraphicFunctions.IsPointInTriangle(tri[0], tri[1], tri[2], tri2[1]) ||
                 GraphicFunctions.IsPointInTriangle(tri[0], tri[1], tri[2], tri2[1]))
            {
                return true;
            }

            return false;

        }


        static int c = 0;
        public bool SwapPolygon2()
        {
             
            if (Polygons.Length < 2)
                return  false;

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
                while (tmpIndex >= 0 && !IsTrinagleInterleaving2(Polygons[tmpIndex].Points, Polygons[index].Points))
                 tmpIndex--;

                if (tmpIndex < 0)
                {
                    tmpIndex = index+1;
                    while (tmpIndex < Polygons.Length && !IsTrinagleInterleaving2(Polygons[tmpIndex].Points, Polygons[index].Points))
                        tmpIndex++;

                    // nema smysl prohazovat dva polygony nikde se neprekryvaji
                    if (tmpIndex >= Polygons.Length)
                    { c++;
                        return false;}
                }

                Polygons[index] = Polygons[tmpIndex];
                Polygons[tmpIndex] = poly;
            }
            else
            {
                int tmpIndex = index + 1;
                while (tmpIndex < Polygons.Length && !IsTrinagleInterleaving2(Polygons[tmpIndex].Points, Polygons[index].Points))
                    tmpIndex++;

                if (tmpIndex >= Polygons.Length)
                {
                    tmpIndex = index - 1;
                    while (tmpIndex >=0 && !IsTrinagleInterleaving2(Polygons[tmpIndex].Points, Polygons[index].Points))
                        tmpIndex--;

                    // nema smysl prohazovat dva polygony nikde se neprekryvaji
                    if (tmpIndex < 0)
                    {c++; 
                        return false;}
                }


                Polygons[index] = Polygons[tmpIndex];
                Polygons[tmpIndex] = poly;

            }


             
          
            {

                SetDirty();
            }

            return true;
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

        public void RemovePolygon(ErrorMatrix errorMatrix)
        {
            if (Polygons.Length > Settings.ActivePolygonsMin)
            {
                //int index = GetRNDIndexPolygonBySize(this.Polygons);
                //int index = GetRNDIndexPolygonByLive(this.Polygons);

                //int ? tmpIndex = GetRNDPolygonIndex(errorMatrix);
                //if (!tmpIndex.HasValue) return;

                //int index = tmpIndex.Value;
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

       


        public void AddPolygon(ErrorMatrix errorMatrix, CanvasBGRA _rawDestImage = null, ImageEdges edgePoints = null)
        {
            if (Polygons.Length < Settings.ActivePolygonsMax )
            {
                if (PointCount < Settings.ActivePointsMax + Settings.ActivePointsPerPolygonMin)
                {
                    var newPolygon = new DnaPolygon();
                    newPolygon.Init(errorMatrix, edgePoints);
                    //newPolygon.Init(null);

                    if (_rawDestImage != null)
                    {
                        //Color nearColor = GetColorByPolygonPoints(newPolygon.Points, _rawDestImage, width);
                        Color nearColor = 
                            //PolygonColorPredict.GetColorBy_PointsColor_MiddleEdgePoints_MiddlePoint_AlphaDiff(newPolygon.Points, _rawDestImage);
                            //PolygonColorPredict.GetColorBy_PointsColor_MiddlePoint_AlphaDiff(newPolygon.Points, _rawDestImage);
                            //PolygonColorPredict.GetColorBy_PointsColor_MiddleEdgePoints_MiddlePoint_AlphaDiff(newPolygon.Points, _rawDestImage);
                            PolygonColorPredict.GetColorBy_PC_MEP_MEOPAM_MP_AlphaDiff(newPolygon.Points, _rawDestImage);

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

        private static int GetRNDIndexPolygonBySize(DnaPolygon[] polygons)
        {
            long [] polygonSizes = new long[polygons.Length];

            for (int index = 0; index < polygons.Length; index++)
                polygonSizes[index] = polygons[index].GetPixelSizePolygon();

            long polySizeMax = 0;
            long polySizeMin = long.MaxValue;

            for (int index = 0; index < polygonSizes.Length; index++)
            {
                if (polySizeMax < polygonSizes[index]) polySizeMax = polygonSizes[index];
                if (polySizeMin > polygonSizes[index]) polySizeMin = polygonSizes[index];
            }


            long sumSizes = 0;
            long minDiffSize = (polySizeMax - polySizeMin)/4;
            for (int index = 0; index < polygonSizes.Length; index++)
            {
                long diffFit = (polySizeMax - polygonSizes[index] + minDiffSize);
                sumSizes += diffFit;
            }

            long [] polygonRullete = new long[polygons.Length];

            int lastRouleteValue = 0;
            const int maxNormalizeValue = 1000000;
            for (int index = 0; index < polygonSizes.Length; index++)
            {
                long diffSize = (polySizeMax - polygonSizes[index] + minDiffSize);

                int tmp = (int)(((long)diffSize * maxNormalizeValue) / sumSizes);
                polygonRullete[index] = lastRouleteValue + tmp;
                lastRouleteValue = lastRouleteValue + tmp;
            }

            int rndIndex = Tools.GetRandomNumber(0, maxNormalizeValue + 1);

            for (int index = 0; index < polygonRullete.Length; index++)
            {
                if (polygonRullete[index] > rndIndex)
                    return index;
            }

            return polygonRullete.Length - 1;
        }

        
        private int ? GetRNDPolygonIndex(ErrorMatrix errorMatrix)
        {
            

            if (this.Polygons.Length == 1)
            {
                return 0;
            }
            else if (this.Polygons.Length > 1)
            {
                List<int> polygonsId = new List<int>();

                do
                {

                    int matrixIndex = errorMatrix.GetRNDMatrixRouleteIndex();
                    Rectangle tileArea = errorMatrix.GetTileByErrorMatrixIndex(matrixIndex);

                    for (int index = 0; index < this.Polygons.Length; index++)
                    {
                        DnaPolygon polygon = this.Polygons[index];
                        if (IsPointInRectangle(tileArea, polygon.Points[0])) polygonsId.Add(index);
                        else if (IsPointInRectangle(tileArea, polygon.Points[1])) polygonsId.Add(index);
                        else if (IsPointInRectangle(tileArea, polygon.Points[2])) polygonsId.Add(index);
                        else
                        {
                            DnaPoint tPointLeftTop = new DnaPoint((short)tileArea.X, (short)tileArea.Y);
                            DnaPoint tPointRightTop = new DnaPoint((short)(tileArea.X + tileArea.Width - 1), (short)tileArea.Y);
                            DnaPoint tPointLeftDown = new DnaPoint((short)tileArea.X, (short)(tileArea.Y + tileArea.Height - 1));
                            DnaPoint tPointRightDown = new DnaPoint((short)(tileArea.X + tileArea.Width - 1), (short)(tileArea.Y + tileArea.Height - 1));
                            

                            // test if tile is inside triangle
                            if (GraphicFunctions.IsPointInTriangle(
                                polygon.Points[0], polygon.Points[1], polygon.Points[2],
                                tPointLeftTop) &&

                                GraphicFunctions.IsPointInTriangle(
                                polygon.Points[0], polygon.Points[1], polygon.Points[2],
                                tPointRightTop) &&

                                GraphicFunctions.IsPointInTriangle(
                                polygon.Points[0], polygon.Points[1], polygon.Points[2],
                                tPointLeftDown) &&

                                GraphicFunctions.IsPointInTriangle(
                                polygon.Points[0], polygon.Points[1], polygon.Points[2],
                                tPointRightDown)
                                )
                            {
                                polygonsId.Add(index);
                            }
                            else
                            {
                                // test if some edge cross tile
                                if (DnaPolygon.LineIntersect(polygon.Points[0], polygon.Points[1], tPointLeftTop, tPointRightDown) ||
                                DnaPolygon.LineIntersect(polygon.Points[0], polygon.Points[1], tPointLeftDown, tPointRightTop))
                                {
                                    polygonsId.Add(index);
                                }
                                else if (DnaPolygon.LineIntersect(polygon.Points[1], polygon.Points[2], tPointLeftTop, tPointRightDown) ||
                                DnaPolygon.LineIntersect(polygon.Points[1], polygon.Points[2], tPointLeftDown, tPointRightTop))
                                {
                                    polygonsId.Add(index);
                                }
                                else if (DnaPolygon.LineIntersect(polygon.Points[2], polygon.Points[0], tPointLeftTop, tPointRightDown) ||
                                DnaPolygon.LineIntersect(polygon.Points[2], polygon.Points[0], tPointLeftDown, tPointRightTop))
                                {
                                    polygonsId.Add(index);
                                }
                            }

                        }

                    }
                } while (polygonsId.Count == 0);

                int polygonIndex = Tools.GetRandomNumber(0, polygonsId.Count);
                return polygonsId[polygonIndex];
            }

            // if no pygons in dna
            return null;
        }

        private int? GetRNDPolygonIndexOnlyPoints(ErrorMatrix errorMatrix)
        {


            if (this.Polygons.Length == 1)
            {
                return 0;
            }
            else if (this.Polygons.Length > 1)
            {
                List<int> polygonsId = new List<int>();

                do
                {

                    int matrixIndex = errorMatrix.GetRNDMatrixRouleteIndex();
                    Rectangle tileArea = errorMatrix.GetTileByErrorMatrixIndex(matrixIndex);

                    for (int index = 0; index < this.Polygons.Length; index++)
                    {
                        DnaPolygon polygon = this.Polygons[index];
                        if (IsPointInRectangle(tileArea, polygon.Points[0])) polygonsId.Add(index);
                        else if (IsPointInRectangle(tileArea, polygon.Points[1])) polygonsId.Add(index);
                        else if (IsPointInRectangle(tileArea, polygon.Points[2])) polygonsId.Add(index);
                       

                    }
                } while (polygonsId.Count == 0);

                int polygonIndex = Tools.GetRandomNumber(0, polygonsId.Count);
                return polygonsId[polygonIndex];
            }

            // if no pygons in dna
            return null;
        }

        private static bool IsPointInRectangle(Rectangle area, DnaPoint point)
        {
            int diffX = area.X - point.X;
            int diffY = area.Y - point.Y;

            return diffX >= 0 && diffX < area.Width &&
               diffY >= 0 && diffY < area.Height;

        }
        

    }


}