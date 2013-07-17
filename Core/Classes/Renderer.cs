using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

using GenArt.AST;

namespace GenArt.Classes
{
    public static class Renderer
    {
       
        //Render a Drawing
        public static void Render(DnaDrawing drawing,Graphics g,int scale, Color background)
        {
            g.Clear(Color.Black);
            //g.Clear(background);

            for (int index = 0;index < drawing.Polygons.Length;index++)
            {

                using (Brush brush = new SolidBrush(drawing.Polygons[index].Brush.BrushColor))
                {
                    //var tmpPoints = polygon.ClonePoints();
                    //tmpPoints.Add(tmpPoints[0]);

                    //Point[] points = GetGdiPoints(tmpPoints, scale);
                    Point[] points = GetGdiPoints(drawing.Polygons[index].Points, scale);
                    //g.DrawLines(new Pen(brush), points);
                    g.FillPolygon(brush, points);
                    
                    //g.DrawPolygon(new Pen(polygon.Brush.Brush), points);

                    //g.FillClosedCurve(polygon.Brush.Brush, points);

                }
            }
                //Render(polygon, g, scale);
        }

        //Render a polygon
        private static void Render(DnaPolygon polygon, Graphics g, int scale)
        {
            using (Brush brush = new SolidBrush(polygon.Brush.BrushColor))
            {

                //var tmpPoints = polygon.ClonePoints();
                //tmpPoints.Add(tmpPoints[0]);

                //Point[] points = GetGdiPoints(tmpPoints, scale);
                Point[] points = GetGdiPoints(polygon.Points, scale);
                //g.DrawLines(new Pen(polygon.Brush.Brush), points);
                //Point[] points = GetGdiPoints(polygon.Points, scale);
                g.FillPolygon(brush, points);
                // g.DrawPolygon(new Pen(polygon.Brush.Brush), points);

                //g.FillClosedCurve(polygon.Brush.Brush, points);

            }
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

        //Convert a DnaBrush to a System.Drawing.Brush
        private static Brush GetGdiBrush(DnaBrush b)
        {
            return new SolidBrush(Color.FromArgb(b.Alpha, b.Red, b.Green, b.Blue));
        }

        
    }
}
