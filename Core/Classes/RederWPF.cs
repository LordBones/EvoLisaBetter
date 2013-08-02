using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GenArt.AST;

namespace GenArt.Core.Classes
{
    static class RenderWPF
    {
        private static  DrawingVisual dv = new DrawingVisual();
        private static RenderTargetBitmap rtb  = new RenderTargetBitmap(1, 1, 0, 0, System.Windows.Media.PixelFormats.Pbgra32);
        private static byte [] result = new byte[0];
        

        //Render a Drawing
        public static byte[] Render(DnaDrawing drawing, int scale, Brush background, int width, int height)
        {
            if(rtb.PixelWidth != width || rtb.PixelHeight != height)
            {
                result = new byte[width * height * 4];
                rtb = new RenderTargetBitmap(width, height, 0, 0, System.Windows.Media.PixelFormats.Pbgra32);
                //BitmapSource bs = 
                RenderOptions.SetBitmapScalingMode(rtb, BitmapScalingMode.LowQuality);
                //DrawingVisual dv = new DrawingVisual();
                RenderOptions.SetEdgeMode(rtb,EdgeMode.Aliased);

                
            }
            
            
            using (DrawingContext ctx = dv.RenderOpen())
            {
                ctx.DrawRectangle(background, null, new System.Windows.Rect(0, 0, width, height));

                //StreamGeometry sg = new StreamGeometry();
                //StreamGeometryContext sgc = sg.Open();
                //sgc.BeginFigure(new Point(10,10), true, true);
                //sgc.LineTo(new Point(20, 10), false, false);
                //sgc.LineTo(new Point(20, 20), false, false);
                //sgc.LineTo(new Point(10, 20), false, false);
                
               
                //sgc.Close();
                //sg.Freeze();

                //Brush brush = new SolidColorBrush(Colors.Blue);
                //brush.Freeze();
                //ctx.DrawGeometry(brush, null, sg);


                
                foreach (var item in drawing.Polygons)
                {
                    Point [] points = GetPoints(item.Points);


                    StreamGeometry sg = new StreamGeometry(); 
                    StreamGeometryContext sgc = sg.Open();
                    
                    sgc.BeginFigure(points[0], true, true);

                    for (int index = 1; index < points.Length; index++)
                    {
                        sgc.LineTo(points[index], false, false);
                    }

                    sgc.Close();
                    sg.Freeze();
                    
                    //Brush brush = new SolidColorBrush(Color.FromArgb(item.Brush.Alpha, item.Brush.Red, item.Brush.Green, item.Brush.Blue));
                    //brush.Freeze();
                    //ctx.DrawGeometry(brush, null, sg);
                    ctx.DrawGeometry(item.Brush.brushWPF, null, sg);

                    
                }

            }
            
            rtb.Render(dv);
            //byte [] result = new byte[width * height * 4];
            rtb.CopyPixels(result, width * 4, 0);

            return result;
            //System.Media.Imaging.
            // g.Clear(Color.Black);

            //foreach (DnaPolygon polygon in drawing.Polygons)
            //   Render(polygon, g, scale);
        }



        private static Point [] GetPoints(DnaPoint [] points)
        {
            Point [] result = new Point[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                result[i] = new Point(points[i].X, points[i].Y);
            }

            return result;
        }
    }
}
