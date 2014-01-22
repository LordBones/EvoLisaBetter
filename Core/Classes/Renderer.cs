using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using GenArt.AST;
using GenArt.Core.AST;
using GenArt.Core.Classes;

namespace GenArt.Classes
{
    public static class Renderer
    {
       
        //Render a Drawing
        public static void Render(DnaDrawing drawing,Graphics g,int scale)
        {
            g.Clear(drawing.BackGround.BrushColor);
            //g.Clear(background);

            for (int index = 0;index < drawing.Polygons.Length;index++)
            {

                using (Brush brush = new SolidBrush(drawing.Polygons[index].Brush.BrushColor))
                {
                    if (drawing.Polygons[index] is DnaPolygon)
                    {
                        Point[] points = GetGdiPoints(drawing.Polygons[index].Points, scale);
                        g.FillPolygon(brush, points);
                    }
                    else if (drawing.Polygons[index] is DnaTriangleStrip)
                    {
                         int count = drawing.Polygons[index].Points.Length-3;
                         for (int i = 0; i <= count; i++)
                         {
                             Point[] points = GetGdiPointsTriangle(drawing.Polygons[index].Points,i, scale);
                             g.FillPolygon(brush, points);
                         }
                    }

                    else if (drawing.Polygons[index] is DnaRectangle)
                    {
                        DnaRectangle rectangle = (DnaRectangle)drawing.Polygons[index];
                        g.FillRectangle(brush, rectangle.StartPoint.X * scale, rectangle.StartPoint.Y * scale,
                            rectangle.Width * scale, rectangle.Height * scale);
                    }
                    else if (drawing.Polygons[index] is DnaElipse)
                    {
                        DnaElipse elipse = (DnaElipse)drawing.Polygons[index];
                        g.FillEllipse(brush, elipse.StartPoint.X * scale, elipse.StartPoint.Y * scale,
                            elipse.Width * scale, elipse.Height * scale);
                    }
                }
            }
               
        }

        //Render a Drawing
        public static void RenderErrorMatrix(ErrorMatrix errorMatrix, Graphics g, int scale)
        {
            int maxError = (int)errorMatrix.Matrix.Max(x => x);

            g.Clear(Color.Black);

            for (int matrixY = 0; matrixY <  errorMatrix.MatrixHeight; matrixY++)
            {
                for (int matrixX = 0; matrixX < errorMatrix.MatrixWidth; matrixX++)
                {
                    //int originalTileHeight =  errorMatrix.InputPixelHeight - matrixY * ErrorMatrix.CONST_TileSize;
                    //if (originalTileHeight >= CONST_TileSize) originalTileHeight = CONST_TileSize;
                    //int originalTileWidth = this._inputPixelWidth - matrixX * CONST_TileSize;
                    //if (originalTileWidth >= CONST_TileSize) originalTileWidth = CONST_TileSize;

                    //int r = (errorMatrix.Matrix[matrixY*errorMatrix.MatrixWidth+matrixX]*255)/maxError;
                    int r = errorMatrix.Matrix[matrixY * errorMatrix.MatrixWidth + matrixX];
                    //r *= 8;
                    r = (r > 255 )? 255 : r;
                    using (Brush brush = new SolidBrush(Color.FromArgb(255, r, 0, 255-r)))
                    {
                        //int indexTileStart = matrixY * errorMatrix.InputPixelWidth + matrixX * ErrorMatrix.CONST_TileSize;

                        Rectangle rec = new Rectangle(
                            matrixX * errorMatrix.CONST_TileSize * scale, matrixY * errorMatrix.CONST_TileSize * scale,
                           errorMatrix.CONST_TileSize * scale, errorMatrix.CONST_TileSize * scale);
                        g.FillRectangle(brush, rec);
                        g.DrawRectangle(new Pen(Color.White), rec);


                    }
                }
            }
        }

        public static void RenderWire(DnaDrawing drawing, Graphics g, int scale)
        {
            g.Clear(drawing.BackGround.BrushColor);
            //g.Clear(background);

            for (int index = 0; index < drawing.Polygons.Length; index++)
            {
                //drawing.Polygons[index].Brush.BrushColor;
                Color c = Color.FromArgb(255,
                    drawing.Polygons[index].Brush.Red,
                    drawing.Polygons[index].Brush.Green,
                    drawing.Polygons[index].Brush.Blue
                    );

                using (Pen pen = new Pen(c))
                {
                    if (drawing.Polygons[index] is DnaPolygon)
                    {
                            Point[] points = GetGdiPoints(drawing.Polygons[index].Points, scale);

                            g.DrawPolygon(pen, points);


                            using (Brush b = new SolidBrush(Color.White))
                            {
                                for (int pi = 0; pi < points.Length; pi++)
                                {
                                    g.FillEllipse(b, points[pi].X - 1 * scale, points[pi].Y - 1 * scale, 3 * scale, 3 * scale);
                                }
                            }
                    }
                    else if (drawing.Polygons[index] is DnaTriangleStrip)
                    {
                         int count = drawing.Polygons[index].Points.Length-3;
                         for (int i = 0; i <= count; i++)
                         {
                             Point[] points = GetGdiPointsTriangle(drawing.Polygons[index].Points, i, scale);

                             g.DrawPolygon(pen, points);

                             using (Brush b = new SolidBrush(Color.White))
                             {
                                 for (int pi = 0; pi < points.Length; pi++)
                                 {
                                     g.FillEllipse(b, points[pi].X - 1 * scale, points[pi].Y - 1 * scale, 3 * scale, 3 * scale);
                                 }
                             }
                         }
                    }
                    else if (drawing.Polygons[index] is DnaRectangle)
                    {
                        DnaRectangle rectangle = (DnaRectangle)drawing.Polygons[index];
                        g.DrawRectangle(pen, rectangle.StartPoint.X * scale, rectangle.StartPoint.Y * scale,
                            rectangle.Width * scale, rectangle.Height * scale);
                    }
                    else if (drawing.Polygons[index] is DnaElipse)
                    {
                        DnaElipse elipse = (DnaElipse)drawing.Polygons[index];
                        g.DrawEllipse(pen, elipse.StartPoint.X * scale, elipse.StartPoint.Y * scale,
                            elipse.Width * scale, elipse.Height * scale);
                    }
                }

                
            }
            //Render(polygon, g, scale);
        }

       
        //Convert a list of DnaPoint to a list of System.Drawing.Point's
        private static Point[] GetGdiPoints(IList<DnaPoint> points,int scale)
        {
            Point[] pts = new Point[points.Count];
            int i = 0;
            foreach (DnaPoint pt in points)
            {
                pts[i++] = new Point(pt.X * scale, pt.Y * scale);
            }
            return pts;
        }

        private static Point[] GetGdiPointsTriangle(IList<DnaPoint> points, int index, int scale)
        {
            Point[] pts = new Point[3];

            for(int i = 0;i<3;i++)
            {
                pts[i] = new Point(points[index].X * scale, points[index].Y * scale);
                index++;
            }

            return pts;
        }

        //Convert a DnaBrush to a System.Drawing.Brush
        private static Brush GetGdiBrush(DnaBrush b)
        {
            return new SolidBrush(Color.FromArgb(b.Alpha, b.Red, b.Green, b.Blue));
        }

        
    }
}
