using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;
using GenArt.AST;
using GenArt.Classes;
using GenArt.Core.Classes;
using GenArt.Core.Classes.SWRenderLibrary;

namespace GenArt
{
    static class Program
    {
        private static void TestBenchmark()
        {
            DnaDrawing dna = new DnaDrawing();
            dna.Init();
            
            for (int i =0; i < 100; i++)
                dna.AddPolygon();

            const int CONST_Width = 200;
            const int CONST_Height = 200;

            byte [] canvasCorrect = new byte[CONST_Height * CONST_Width * 4];


            //byte [] canvasTest = new byte[CONST_Height * CONST_Width * 4];

            CanvasBGRA canvasTest = new CanvasBGRA(CONST_Width, CONST_Height);

            SWTriangle triangleTest = new SWTriangle(CONST_Width, CONST_Height);

            Polygon polyCorrect = new Polygon(CONST_Width, CONST_Height);
            polyCorrect.SetStartBufferSize(CONST_Width, CONST_Height);
            Polygon polyTest = new Polygon(CONST_Width, CONST_Height);
            polyTest.SetStartBufferSize(CONST_Width, CONST_Height);

             for (int i = 0; i < 100000; i++)
             {
            for (int index =0; index < dna.Polygons.Length; index++)
            {

                //polyCorrect.FillPolygonCorrectSlow(points, canvasCorrect, Color.Black);
                
                    //polyTest.FillPolygonBenchmark(canvasTest, Color.Black);
                    triangleTest.RenderTriangle(dna.Polygons[index].Points, canvasTest, Color.PaleGreen);
                

                //  if (!polyCorrect.IsMinAreaDataEqual(polyTest))
                {
                    //    polyCorrect.SaveMinAreaToFile("PolyCorrect.txt");
                    //    polyTest.SaveMinAreaToFile("PolyTest.txt");

                    //  break;
                }

            }
             }

        }

        private static void TestSoftwareRenderPolygon()
        {
            Settings.ActivePointsMax = 1500;
            Settings.ActivePolygonsMax = 255;
            const int CONST_Width = 200;
            const int CONST_Height = 200;

            Tools.MaxWidth = CONST_Width;
            Tools.MaxHeight = CONST_Height;

            byte [] canvasCorrect = null;
            CanvasBGRA canvasTest = null;

            SWTriangle triangleTest = new SWTriangle(CONST_Width, CONST_Height);

            Polygon polyCorrect = new Polygon(CONST_Width, CONST_Height);
            polyCorrect.SetStartBufferSize(CONST_Width, CONST_Height);
            Polygon polyTest = new Polygon(CONST_Width, CONST_Height);
            polyTest.SetStartBufferSize(CONST_Width, CONST_Height);

            for (int repeat = 0; repeat < 1000; repeat++)
            {
                DnaDrawing dna = new DnaDrawing();
                dna.Init();

                for (int i =0; i < 2; i++)
                    dna.AddPolygon();


                for (int i =0; i < dna.Polygons.Length; i++)
                    dna.Polygons[i].Brush.InitRandom();




                canvasCorrect = new byte[CONST_Height * CONST_Width * 4];
                canvasTest = new CanvasBGRA(CONST_Width, CONST_Height);


                //Bitmap rbmp = new Bitmap(Tools.MaxWidth, Tools.MaxHeight, PixelFormat.Format32bppPArgb);
                //Graphics g = Graphics.FromImage(rbmp);

                //g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                //g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                //g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighSpeed;
                //g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
                //g.Clear(Color.Black);
                
                bool end = false;
                for (int index =0; index < dna.Polygons.Length; index++)
                {
                    Point [] points = SoftwareRender.GetGdiPoints(dna.Polygons[index].Points, 1);
                    polyCorrect.FillPolygonCorrectSlow(points, canvasCorrect, dna.Polygons[index].Brush.BrushColor);

                    triangleTest.RenderTriangle(
                        dna.Polygons[index].Points, canvasTest, dna.Polygons[index].Brush.BrushColor);

                    //polyTest.FillPolygon(points, canvasTest, dna.Polygons[index].Brush.BrushColor);

                    //using (Brush brush = new SolidBrush(dna.Polygons[index].Brush.BrushColor))
                    //{
                        
                    //    Point[] gdipoints = GetGdiPoints(dna.Polygons[index].Points, 1);
                        
                    //    g.FillPolygon(brush, gdipoints);
                    //}
                    

                    bool canvasEqual = true;
                    for (int ieq = 0; ieq < canvasTest.Data.Length; ieq++)
                    {
                        if (canvasCorrect[ieq] != canvasTest.Data[ieq])
                        {
                            canvasEqual = false;
                            break;
                        }
                    }

                       /* if (//!polyTest.IsMinAreaDataEqual(polyTest) 
                            //|| 
                            !canvasEqual)
                        {
                            polyCorrect.SaveMinAreaToFile("PolyCorrect.txt");
                            polyTest.SaveMinAreaToFile("PolyTest.txt");

                            Console.Out.WriteLine("Test Fail");
                            end = true;
                            break;
                        }*/

                }

                //rbmp.Save("canvasTestswGDI.bmp");
                //if (end)
                    break;
            }



            

            Bitmap bmp = new Bitmap(CONST_Width, CONST_Height, PixelFormat.Format32bppPArgb);
            var lockBmp = bmp.LockBits(new Rectangle(0, 0, CONST_Width, CONST_Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);

            unsafe
            {
                byte * ll = (byte*)lockBmp.Scan0.ToPointer();

                for (int index = 0; index < canvasTest.Data.Length; index++)
                {
                    *(ll + index) = canvasTest.Data[index];
                }
            }

            bmp.UnlockBits(lockBmp);
            bmp.Save("canvasTestsw.bmp", ImageFormat.Bmp);

            bmp = new Bitmap(CONST_Width, CONST_Height, PixelFormat.Format32bppPArgb);
            var lockBmp2 = bmp.LockBits(new Rectangle(0, 0, CONST_Width, CONST_Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);

            unsafe
            {
                byte * ll = (byte*)lockBmp2.Scan0.ToPointer();

                for (int index = 0; index < canvasCorrect.Length; index++)
                {
                    *(ll + index) = canvasCorrect[index];
                }
            }

            bmp.UnlockBits(lockBmp);
            bmp.Save("canvasTestswCorrect.bmp", ImageFormat.Bmp);

        }

        private static Point[] GetGdiPoints(IList<DnaPoint> points, int scale)
        {
            Point[] pts = new Point[points.Count];
            int i = 0;
            foreach (DnaPoint pt in points)
            {
                pts[i++] = new Point(pt.X * scale, pt.Y * scale);
            }
            return pts;
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string [] args)
        {


            if (args.Length == 0)
            {
                MainForm tf = new MainForm();
                tf.Show();
                tf.Activate();
                Application.Run(tf);


                //Application.EnableVisualStyles();
                //Application.SetCompatibleTextRenderingDefault(false);
                //Application.Run(new MainForm());
            }
            else
            {
                
                if (args[0] == "bench")
                {

                    TimeSpan ts = Process.GetCurrentProcess().UserProcessorTime;

                    TestBenchmark();

                    ts = Process.GetCurrentProcess().UserProcessorTime - ts;

                    Console.Out.WriteLine("time: {0}.{1:d3}", (int)ts.TotalSeconds, ts.Milliseconds);
                    
                }
                else if (args[0] == "test")
                {
                    TestSoftwareRenderPolygon();
                }
                else
                {
                    Console.Out.WriteLine("test");
                }
            }
        }
    }
}
