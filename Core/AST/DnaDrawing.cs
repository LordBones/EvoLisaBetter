using System.Collections.Generic;
using System.Xml.Serialization;
using GenArt.Classes;
using System;
using System.Drawing;
using GenArt.Core.Classes;
using GenArt.Core.AST;

namespace GenArt.AST
{
    public class DnaDrawing
    {
        public DnaPrimitive [] Polygons;// { get; set; }
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
                    pointCount += Polygons[index].GetCountPoints();

                return pointCount;
            }
        }

        public DnaDrawing(short maxWidth, short maxHeight)
        {
            Polygons = new DnaPrimitive[0];
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
            Polygons = new DnaPrimitive[Settings.ActivePolygonsMin];
            BackGround = new DnaBrush(255, 0, 0, 0);
            BackGround.InitRandomWithoutAlpha();

            for (int i = 0; i < Settings.ActivePolygonsMin; i++)
                AddPolygon(255,null);

            SetDirty();
        }

        public DnaDrawing Clone()
        {
            var drawing = new DnaDrawing(this._maxWidth,this._maxHeight);
            drawing.Polygons = new DnaPrimitive[Polygons.Length];
            drawing.BackGround = BackGround;

            for (int index = 0; index < Polygons.Length; index++)
                drawing.Polygons[index] = (DnaPrimitive)Polygons[index].Clone();

            return drawing;
        }


        public void Mutate(CanvasBGRA destImage = null)
        {
            if (Tools.WillMutate(Settings.ActiveRemovePolygonMutationRate))
                RemovePolygon(null);
            
            else if (Tools.WillMutate(Settings.ActiveAddPolygonMutationRate))
                AddPolygon(0,null,destImage);

            else if (Tools.WillMutate(Settings.ActiveMovePolygonMutationRate))
                SwapPolygon();
            
            
            {
                for (int index = 0; index < Polygons.Length; index++)
                    Polygons[index].Mutate(0,this, destImage);
            }

        }

        public void MutateBetter(byte mutationRate, ErrorMatrix errorMatrix, CanvasBGRA destImage = null, ImageEdges edgePoints = null)
        {
            /// k mutaci pozadi dochazi pouze jednou 
            //if (Tools.GetRandomNumber(0, 10) == 9)
            //    BackGround.MutateRGBOldWithoutAlpha(this);

            do
            {
                this.IsDirty = false;
                //int mutateChange = Tools.GetRandomNumber(0, 1001);


                 
                if (Tools.GetRandomNumber(0, 1001) < 101)
                {
                    //if (Settings.ActivePolygonsMax <= this.Polygons.Length)
                    //    RemovePolygon();
                    if (Settings.ActivePolygonsMax > this.Polygons.Length)
                    {
                        //AddPolygon(mutationRate, errorMatrix, destImage, edgePoints);  

                        //int tmp = Tools.GetRandomNumber(0, 3);
                        //if (tmp == 0)        AddPolygon(mutationRate, errorMatrix, destImage, edgePoints);  
                        //else if (tmp == 1) AddElipse(mutationRate, errorMatrix, destImage, edgePoints);
                        //else AddRectangle(mutationRate, errorMatrix, destImage, edgePoints);

                        int tmp = Tools.GetRandomNumber(0, 3);
                        if (tmp == 0) AddPolygon(mutationRate, errorMatrix, null, edgePoints);
                        else if (tmp == 1) AddElipse(mutationRate, errorMatrix, null, edgePoints);
                        else AddRectangle(mutationRate, errorMatrix, null, edgePoints);

                        //if (Tools.GetRandomNumber(0, 3) < 1)
                        //    AddPolygon(mutationRate, errorMatrix, destImage, edgePoints);
                        //if (Tools.GetRandomNumber(0, 3) < 1)
                        //{
                        //    AddElipse(mutationRate, errorMatrix, destImage, edgePoints);
                        //}
                        //    if (Tools.GetRandomNumber(0, 3) < 1)
                        //{
                        //    AddRectangle(mutationRate, errorMatrix, destImage, edgePoints);
                        //}

                        // break;
                    }
                }

                if (Tools.GetRandomNumber(0, 1001) < 101)
                {
                    RemovePolygon(errorMatrix);
                    //RemovePolygon(errorMatrix);
                    //break;

                }
                if (Tools.GetRandomNumber(0, 1001) < 101)
                {
                    //SwapPolygon();
                    SwapPolygon2();
                    //break;

                }

                if (Tools.GetRandomNumber(0, 1001) < 101)
                {
                    RandomExchangeElipseRectangle();
                    //break;
                }




                if (Polygons.Length > 0)
                {
                    if (Tools.GetRandomNumber(0, 1001) < 101)
                    {
                        //int index = GetRNDIndexPolygonBySize(this.Polygons);
                        //int index = GetRNDIndexPolygonByLive(this.Polygons);

                        //int ? tmpIndex = GetRNDPolygonIndexOnlyPoints(errorMatrix);
                        //if (!tmpIndex.HasValue) throw new NotImplementedException("sem se to nesmi dostat.");

                        //int index = tmpIndex.Value;

                        int index = Tools.GetRandomNumber(0, Polygons.Length);

                        //if (Tools.GetRandomNumber(0, 2) == 3)
                        //    Polygons[index].MutateTranspozite(this, destImage);
                        //else
                        //{
                        Polygons[index].Mutate(mutationRate, this, destImage, edgePoints);

                        //Color nearColor = Color.Black;

                        //if (Polygons[index] is DnaPolygon)
                        //{
                        //    nearColor = PolygonColorPredict.GetColorBy_PC_MEP_MEOPAM_MP_AlphaDiff(Polygons[index].Points, destImage);
                        //}
                        //else if (Polygons[index] is DnaRectangle)
                        //{
                        //    nearColor = PolygonColorPredict.GetColorBy_PC_MEP_MEOPAM_MP_AlphaDiff((DnaRectangle)Polygons[index], destImage);
                        //}

                        //byte alpha = Polygons[index].Brush.Alpha;
                        //Polygons[index].Brush.SetByColor(nearColor);
                        //Polygons[index].Brush.Alpha = alpha;
                        //}
                    }

                    if (!this.IsDirty ||
                       (this.IsDirty && Tools.GetRandomNumber(0, 1001) < 101))
                    {
                        int ? tmpIndex = GetRNDPolygonIndexOnlyPoints(errorMatrix);
                        if (!tmpIndex.HasValue) throw new NotImplementedException("sem se to nesmi dostat.");

                        int tindex = tmpIndex.Value;
                        //int tindex = Tools.GetRandomNumber(0, Polygons.Length);
                        Polygons[tindex].Brush.MutateRGBOldnew(mutationRate, this);
                        //SortOnePolygonByAlpa(tindex);

                        //Polygons[tindex].Brush.MutateRGBOld(this);
                    }
                }

                } while (false);
            //} while (Tools.GetRandomNumber(1, 101) <= 25);
        }

        public void MutateBetter2(byte mutationRate, ErrorMatrix errorMatrix, CanvasBGRA destImage = null, ImageEdges edgePoints = null)
        {
            /// k mutaci pozadi dochazi pouze jednou 
            //if (Tools.GetRandomNumber(0, 10) == 9)
            //    BackGround.MutateRGBOldWithoutAlpha(this);

            do
            {
                this.IsDirty = false;
                int mutateChange = Tools.GetRandomNumber(0, 1001);



                if (mutateChange < 50)
                {
                    //if (Settings.ActivePolygonsMax <= this.Polygons.Length)
                    //    RemovePolygon();
                    if (Settings.ActivePolygonsMax > this.Polygons.Length)
                    {
                        if (Tools.GetRandomNumber(0, 2) < 1)
                            AddPolygon(mutationRate, errorMatrix, destImage, edgePoints);
                        else
                        {
                            AddRectangle(mutationRate, errorMatrix, destImage, edgePoints);
                        }
                        continue;
                    }
                }
                if (mutateChange < 100)
                {
                    RemovePolygon(errorMatrix);
                    //RemovePolygon(errorMatrix);
                    continue;
                }
                if (mutateChange < 150)
                {
                    SwapPolygon();
                    //if (SwapPolygon2())
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
                    if (mutateChange < 650)
                    {
                        //int index = GetRNDIndexPolygonBySize(this.Polygons);
                        //int index = GetRNDIndexPolygonByLive(this.Polygons);

                        //int ? tmpIndex = GetRNDPolygonIndexOnlyPoints(errorMatrix);
                        //if (!tmpIndex.HasValue) throw new NotImplementedException("sem se to nesmi dostat.");

                        //int index = tmpIndex.Value;

                        int index = Tools.GetRandomNumber(0, Polygons.Length);

                        //if (Tools.GetRandomNumber(0, 2) == 3)
                        //    Polygons[index].MutateTranspozite(this, destImage);
                        //else
                        //{
                        Polygons[index].Mutate(mutationRate, this, destImage, edgePoints);

                        //Color nearColor = Color.Black;

                        //if (Polygons[index] is DnaPolygon)
                        //{
                        //    nearColor = PolygonColorPredict.GetColorBy_PC_MEP_MEOPAM_MP_AlphaDiff(Polygons[index].Points, destImage);
                        //}
                        //else if (Polygons[index] is DnaRectangle)
                        //{
                        //    nearColor = PolygonColorPredict.GetColorBy_PC_MEP_MEOPAM_MP_AlphaDiff((DnaRectangle)Polygons[index], destImage);
                        //}

                        //byte alpha = Polygons[index].Brush.Alpha;
                        //Polygons[index].Brush.SetByColor(nearColor);
                        //Polygons[index].Brush.Alpha = alpha;
                        //}

                    }
                    else
                    {
                        int ? tmpIndex = GetRNDPolygonIndex(errorMatrix);
                        if (!tmpIndex.HasValue) throw new NotImplementedException("sem se to nesmi dostat.");

                        int tindex = tmpIndex.Value;
                        //int tindex = Tools.GetRandomNumber(0, Polygons.Length);
                        Polygons[tindex].Brush.MutateRGBOld(mutationRate, this);

                        //Polygons[tindex].Brush.MutateRGBOld(this);
                    }
                }

            } while (false);
            // while (Tools.GetRandomNumber(1, 11) <= 5);
        }

       
        bool IsPrimitiveInterleaving(DnaPrimitive who, DnaPrimitive interWith)
        {
            DnaPolygon poly = who as DnaPolygon;
            if (poly != null)
            {
                DnaPoint [] points = poly._Points;
                if(interWith.IsPointInside(points[0]) || 
                   interWith.IsPointInside(points[1]) ||
                   interWith.IsPointInside(points[2]) ||
                   interWith.IsLineCrossed(points[0],points[1]) ||
                   interWith.IsLineCrossed(points[1],points[2]) || 
                   interWith.IsLineCrossed(points[2],points[0]))
                    return true;
                else
                return false;
            }

            DnaRectangle rec = who as DnaRectangle;
            if (rec != null)
            {
                DnaPoint [] points = rec.Points;

                if (interWith.IsLineCrossed(points[0], new DnaPoint(points[1].X, points[0].Y)) ||
                    interWith.IsLineCrossed(points[0], new DnaPoint(points[0].X, points[1].Y)) ||
                    interWith.IsLineCrossed(points[1], new DnaPoint(points[1].X, points[0].Y)) ||
                    interWith.IsLineCrossed(points[1], new DnaPoint(points[0].X, points[1].Y)))

                    return true;
                else
                    return false;
            }

            DnaElipse eli = who as DnaElipse;
            if (eli != null)
            {
                if (interWith is DnaRectangle || interWith is DnaPolygon)
                {
                    return IsPrimitiveInterleaving(interWith, who);
                }

                DnaElipse intEli = who as DnaElipse;
                if (intEli != null)
                {
                    DnaPoint startPoint = eli.StartPoint;
                    DnaPoint endPoint = eli.EndPoint;

                    if (interWith.IsPointInside(startPoint) ||
                    interWith.IsPointInside(new DnaPoint(startPoint.X, endPoint.Y)) ||
                    interWith.IsPointInside(new DnaPoint(endPoint.X, startPoint.Y)) ||
                    interWith.IsPointInside(endPoint))

                    return true;
                else
                    return false;
                }
            }

            return true;

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

            DnaPrimitive poly = Polygons[index];

            if (swapUp)
            {
                int tmpIndex = index-1;
                while (tmpIndex >= 0 && !IsPrimitiveInterleaving(Polygons[tmpIndex], Polygons[index]))
                 tmpIndex--;

                if (tmpIndex < 0)
                {
                    tmpIndex = index+1;
                    while (tmpIndex < Polygons.Length && !IsPrimitiveInterleaving(Polygons[tmpIndex], Polygons[index]))
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
                while (tmpIndex < Polygons.Length && !IsPrimitiveInterleaving(Polygons[tmpIndex], Polygons[index]))
                    tmpIndex++;

                if (tmpIndex >= Polygons.Length)
                {
                    tmpIndex = index - 1;
                    while (tmpIndex >= 0 && !IsPrimitiveInterleaving(Polygons[tmpIndex], Polygons[index]))
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

            DnaPrimitive poly = Polygons[index];

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

        private void RandomExchangeElipseRectangle()
        {
            if (Polygons.Length == 0)
                return;

            int index = Tools.GetRandomNumber(0, Polygons.Length);

            DnaPrimitive primitive = null;
            int primitiveIndex = 0;

            // find primitive for change
            int tmp = index;
            // down
            while (tmp >= 0)
            {
                if (Polygons[tmp] is DnaElipse || Polygons[tmp] is DnaRectangle)
                {
                    primitive = Polygons[tmp];
                    primitiveIndex = tmp;
                    break;
                }

                tmp--;
            }

            if (primitive == null)
            {
                index++;
                while (index < Polygons.Length)
                {
                    if (Polygons[index] is DnaElipse || Polygons[index] is DnaRectangle)
                    {
                        primitive = Polygons[index];
                        primitiveIndex = index;
                        break;
                    }

                    index++;
                }
            }

            if (primitive != null)
            {
                DnaElipse elipse = primitive as DnaElipse;
                if (elipse != null)
                {
                    DnaRectangle rec = new DnaRectangle();
                    rec.Brush = elipse.Brush;
                    rec.StartPoint = elipse.StartPoint;
                    rec.EndPoint = elipse.EndPoint;

                    Polygons[primitiveIndex] = rec;

                    SetDirty();
                    return;
                }

                DnaRectangle rectangle = primitive as DnaRectangle;

                if (rectangle != null)
                {
                    if (rectangle.Width > 4 && rectangle.Height > 4)
                    {
                        DnaElipse tmpelipse = new DnaElipse();
                        tmpelipse.Brush = rectangle.Brush;
                        tmpelipse.StartPoint = rectangle.StartPoint;
                        tmpelipse.Width = rectangle.Width;
                        tmpelipse.Height = rectangle.Height;


                        Polygons[primitiveIndex] = tmpelipse;

                        SetDirty();
                        return;
                    }
                }
            }

        }

        public void RemovePolygon(ErrorMatrix errorMatrix)
        {
            if (Polygons.Length > Settings.ActivePolygonsMin)
            {
                //int index = GetRNDIndexPolygonBySize(this.Polygons);
                //int index = GetRNDIndexPolygonByLive(this.Polygons);

                int ? tmpIndex = GetRNDPolygonIndexOnlyPoints(errorMatrix);
                //int ? tmpIndex = GetRNDPolygonIndex(errorMatrix);
                if (!tmpIndex.HasValue) return;

                int index = tmpIndex.Value;
                //int index = Tools.GetRandomNumber(0, Polygons.Length);

                DnaPrimitive [] polygons = new DnaPrimitive[Polygons.Length -1];

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

        


        public void AddPolygon(byte mutationRate, ErrorMatrix errorMatrix, CanvasBGRA _rawDestImage = null, ImageEdges edgePoints = null)
        {
            if (Polygons.Length < Settings.ActivePolygonsMax )
            {
                if (PointCount < Settings.ActivePointsMax + Settings.ActivePointsPerPolygonMin)
                {
                    var newPolygon = new DnaPolygon();
                    newPolygon.Init(mutationRate,errorMatrix, edgePoints);
                    //newPolygon.Init(null);

                    if (_rawDestImage != null)
                    {
                        //Color nearColor = GetColorByPolygonPoints(newPolygon.Points, _rawDestImage, width);
                        Color nearColor = 
                            //PolygonColorPredict.GetColorBy_PointsColor_MiddleEdgePoints_MiddlePoint_AlphaDiff(newPolygon.Points, _rawDestImage);
                            //PolygonColorPredict.GetColorBy_PointsColor_MiddlePoint_AlphaDiff(newPolygon.Points, _rawDestImage);
                            //PolygonColorPredict.GetColorBy_PointsColor_MiddleEdgePoints_MiddlePoint_AlphaDiff(newPolygon.Points, _rawDestImage);
                            PolygonColorPredict.GetColorBy_PC_MEP_MEOPAM_MP_AlphaDiff(newPolygon._Points, _rawDestImage);

                        newPolygon.Brush.SetByColor(nearColor);
                        //newPolygon.Brush.InitRandom();
                    }
                    else
                    {
                        newPolygon.Brush.InitRandom();
                    }

                    
                    //List<DnaPrimitive> polygons = new List<DnaPrimitive>(Polygons);
                    //if (polygons.Count == 0)
                    //    polygons.Add(newPolygon);
                    //else
                    //{
                    //    int index = Tools.GetRandomNumber(0, Polygons.Length); 
                    //    polygons.Insert(index, newPolygon);
                    //}
                    //this.Polygons = polygons.ToArray();

                    DnaPrimitive [] polygons = new DnaPrimitive[Polygons.Length + 1];
                    Array.Copy(Polygons, polygons, Polygons.Length);

                    polygons[polygons.Length - 1] = newPolygon;
                    Polygons = polygons;
                    //SortOnePolygonByAlpa(polygons.Length - 1);

                    SetDirty();
                }
            }
        }

        public void AddRectangle(byte mutationRate, ErrorMatrix errorMatrix, CanvasBGRA _rawDestImage = null, ImageEdges edgePoints = null)
        {
            if (Polygons.Length < Settings.ActivePolygonsMax)
            {
                if (PointCount < Settings.ActivePointsMax + Settings.ActivePointsPerPolygonMin)
                {
                    var newRectangle = new DnaRectangle();
                    newRectangle.Init(mutationRate,errorMatrix, edgePoints);
                    

                    if (_rawDestImage != null)
                    {
                        //Color nearColor = GetColorByPolygonPoints(newPolygon.Points, _rawDestImage, width);
                        Color nearColor = 
                            //PolygonColorPredict.GetColorBy_PointsColor_MiddleEdgePoints_MiddlePoint_AlphaDiff(newPolygon.Points, _rawDestImage);
                            //PolygonColorPredict.GetColorBy_PointsColor_MiddlePoint_AlphaDiff(newPolygon.Points, _rawDestImage);
                            //PolygonColorPredict.GetColorBy_PointsColor_MiddleEdgePoints_MiddlePoint_AlphaDiff(newPolygon.Points, _rawDestImage);
                            PolygonColorPredict.GetColorBy_PC_MEP_MEOPAM_MP_AlphaDiff(newRectangle, _rawDestImage);

                        newRectangle.Brush.SetByColor(nearColor);
                        //newPolygon.Brush.InitRandom();
                    }
                    else
                    {
                        newRectangle.Brush.InitRandom();
                    }


                    //List<DnaPrimitive> polygons = new List<DnaPrimitive>(Polygons);
                    //if (polygons.Count == 0)
                    //    polygons.Add(newRectangle);
                    //else
                    //{
                    //    int index = Tools.GetRandomNumber(0, Polygons.Length);
                    //    polygons.Insert(index, newRectangle);
                    //}
                    //this.Polygons = polygons.ToArray();


                    DnaPrimitive [] polygons = new DnaPrimitive[Polygons.Length + 1];
                    Array.Copy(Polygons, polygons, Polygons.Length);

                    polygons[polygons.Length - 1] = newRectangle;
                    Polygons = polygons;

                    SetDirty();
                }
            }
        }

        public void AddElipse(byte mutationRate, ErrorMatrix errorMatrix, CanvasBGRA _rawDestImage = null, ImageEdges edgePoints = null)
        {
            if (Polygons.Length < Settings.ActivePolygonsMax)
            {
                if (PointCount < Settings.ActivePointsMax + Settings.ActivePointsPerPolygonMin)
                {
                    var newElipse = new DnaElipse();
                    newElipse.Init(mutationRate, errorMatrix, edgePoints);


                    if (_rawDestImage != null)
                    {
                        //Color nearColor = GetColorByPolygonPoints(newPolygon.Points, _rawDestImage, width);
                        Color nearColor = 
                            //PolygonColorPredict.GetColorBy_PointsColor_MiddleEdgePoints_MiddlePoint_AlphaDiff(newPolygon.Points, _rawDestImage);
                            //PolygonColorPredict.GetColorBy_PointsColor_MiddlePoint_AlphaDiff(newPolygon.Points, _rawDestImage);
                            //PolygonColorPredict.GetColorBy_PointsColor_MiddleEdgePoints_MiddlePoint_AlphaDiff(newPolygon.Points, _rawDestImage);
                            PolygonColorPredict.GetColorBy_PC_MEP_MEOPAM_MP_AlphaDiff(newElipse, _rawDestImage);

                        newElipse.Brush.SetByColor(nearColor);
                        //newPolygon.Brush.InitRandom();
                    }
                    else
                    {
                        newElipse.Brush.InitRandom();
                    }

                
                    //List<DnaPrimitive> polygons = new List<DnaPrimitive>(Polygons);
                    //if (polygons.Count == 0)
                    //    polygons.Add(newElipse);
                    //else
                    //{
                    //    int index = Tools.GetRandomNumber(0, Polygons.Length);
                    //    polygons.Insert(index, newElipse);
                    //}
                    //this.Polygons = polygons.ToArray();


                    DnaPrimitive [] polygons = new DnaPrimitive[Polygons.Length + 1];
                    Array.Copy(Polygons, polygons, Polygons.Length);

                    polygons[polygons.Length - 1] = newElipse;
                    Polygons = polygons;

                    SetDirty();
                }
            }
        }

        
        private int ? GetRNDPolygonIndex(ErrorMatrix errorMatrix)
        {
            throw new NotImplementedException();

            if (this.Polygons.Length == 1)
            {
                return 0;
            }
            else if (this.Polygons.Length > 1)
            {
                //return Tools.GetRandomNumber(0, Polygons.Length);
                 
                
                List<int> polygonsId = new List<int>();

                do
                {

                    int matrixIndex = errorMatrix.GetRNDMatrixRouleteIndex();
                    Rectangle tileArea = errorMatrix.GetTileByErrorMatrixIndex(matrixIndex);

                    for (int index = 0; index < this.Polygons.Length; index++)
                    {
                        DnaPrimitive polygon = this.Polygons[index];
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
                                if (GraphicFunctions.LineIntersect(polygon.Points[0], polygon.Points[1], tPointLeftTop, tPointRightDown) ||
                                GraphicFunctions.LineIntersect(polygon.Points[0], polygon.Points[1], tPointLeftDown, tPointRightTop))
                                {
                                    polygonsId.Add(index);
                                }
                                else if (GraphicFunctions.LineIntersect(polygon.Points[1], polygon.Points[2], tPointLeftTop, tPointRightDown) ||
                                GraphicFunctions.LineIntersect(polygon.Points[1], polygon.Points[2], tPointLeftDown, tPointRightTop))
                                {
                                    polygonsId.Add(index);
                                }
                                else if (GraphicFunctions.LineIntersect(polygon.Points[2], polygon.Points[0], tPointLeftTop, tPointRightDown) ||
                                GraphicFunctions.LineIntersect(polygon.Points[2], polygon.Points[0], tPointLeftDown, tPointRightTop))
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
                    DnaRectangle rec = new DnaRectangle((short) tileArea.X, (short) tileArea.Y,(short) tileArea.Width,(short) tileArea.Height);
                    
                    for (int index = 0; index < this.Polygons.Length; index++)
                    {
                        DnaPrimitive polygon = this.Polygons[index];
                        if(IsPrimitiveInterleaving(rec, polygon)) polygonsId.Add(index);

                        //if ( IsPointInRectangle(tileArea, polygon.Points[0])) polygonsId.Add(index);
                        //else if (IsPointInRectangle(tileArea, polygon.Points[1])) polygonsId.Add(index);
                        //else if (IsPointInRectangle(tileArea, polygon.Points[2])) polygonsId.Add(index);
                       

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