using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using GenArt.AST;
using GenArt.Classes;
using GenArt.Core.AST;
using GenArt.Core.Classes;
using GenArt.Core.Classes.SWRenderLibrary;

namespace GenArt
{
    public partial class MainForm : Form
    {
        public static Settings Settings;
        private DnaDrawing currentDrawing;
        private DnaDrawing lastDrawing = null;
        private object DrawingLock = new object();

        private long lastErrorLevel = long.MaxValue;
        private long lastWorstErrorLevelDiff = 0;


        private long errorLevel = long.MaxValue;
        private int generation;
        private DnaDrawing guiDrawing;
        private bool isRunning;
        private DateTime lastRepaint = DateTime.MinValue;
        private int lastSelected;
        private TimeSpan repaintIntervall = new TimeSpan(0, 0, 0, 0, 0);
        private int repaintOnSelectedSteps = 3;
        private int selected;

        private Bitmap sourceBitmap;
        private CanvasBGRA sourceBitmapAsCanvas;
        private ImageEdges SourceBitmapEdges;

        DNARenderer _dnaRender;

        private Thread thread;

        // backbuffer do ktereho se provadi vykreslovani pred samotnym zobrazenim
        Bitmap backBuffer = new Bitmap(1, 1, PixelFormat.Format32bppPArgb);

        private int ZoomScale { get { return (int)nudZoom.Value; } }
        private int InitPopulation { get { return (int)nudPopulation.Value; } }
        private int EdgeThreshold { get { return (int)nudEdgeThreshold.Value; } }
        private int MaxPolygons { get { return (int)nudMaxPolygon.Value; } }



        public MainForm()
        {
            Tools.ClearPseudoRandom();
            
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.Opaque, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
 


            Text = FitnessCalculator.kk;
            
            Settings = new Settings();

            //Test();
            //TestSoftwareRenderPolygon();
            //TestBenchmark();

            

            InitImage(Bitmap.FromFile(Path.Combine(Application.StartupPath,GenArt.Properties.Resources.ml1)));

           
        }

        private void TestBenchmark()
        {
            const int CONST_Width = 1000;
            const int CONST_Height = 1000;


            DnaDrawing dna = new DnaDrawing(CONST_Width, CONST_Height);
            dna.Init();


            for (int i =0; i < 100; i++)
                dna.AddPolygon();

          
            byte [] canvasCorrect = new byte[CONST_Height * CONST_Width * 4];
            byte [] canvasTest = new byte[CONST_Height * CONST_Width * 4];


            Polygon polyCorrect = new Polygon(CONST_Width, CONST_Height);
            polyCorrect.SetStartBufferSize(CONST_Width, CONST_Height);
            Polygon polyTest = new Polygon(CONST_Width, CONST_Height);
            polyTest.SetStartBufferSize(CONST_Width, CONST_Height);

            //for (int index =0; index < dna.Polygons.Length; index++)
            {
                
                //polyCorrect.FillPolygonCorrectSlow(points, canvasCorrect, Color.Black);
                //for (int i = 0; i < 40000; i++)
                {
                    polyTest.FillPolygonBenchmark(canvasTest, Color.Black);
                }

              //  if (!polyCorrect.IsMinAreaDataEqual(polyTest))
                {
                //    polyCorrect.SaveMinAreaToFile("PolyCorrect.txt");
                //    polyTest.SaveMinAreaToFile("PolyTest.txt");

                  //  break;
                }

            }
            
        }

        private void TestSoftwareRenderPolygon()
        {
            const int CONST_Width = 200;
            const int CONST_Height = 200;

            DnaDrawing dna = new DnaDrawing(CONST_Width,CONST_Height);
            dna.Init();

            for (int i =0; i < 100; i++)
                dna.AddPolygon();

            

            byte [] canvasCorrect = new byte[CONST_Height*CONST_Width*4];
            byte [] canvasTest = new byte[CONST_Height*CONST_Width*4];

            
            Polygon polyCorrect = new Polygon(CONST_Width, CONST_Height);
            polyCorrect.SetStartBufferSize(200, 200);
            Polygon polyTest = new Polygon(CONST_Width, CONST_Height);
            polyTest.SetStartBufferSize(200, 200);

            for(int index =0;index < dna.Polygons.Length;index++)
            {
               Point [] points = SoftwareRender.GetGdiPoints(dna.Polygons[index].Points,1);
               polyCorrect.FillPolygonCorrectSlow(points,canvasCorrect,Color.Black);
               polyTest.FillPolygon(points, canvasTest, Color.Black);

               if (!polyCorrect.IsMinAreaDataEqual(polyTest))
               {
                   polyCorrect.SaveMinAreaToFile("PolyCorrect.txt");
                   polyTest.SaveMinAreaToFile("PolyTest.txt");

                   break;
               }
           
            }
            
        }

       

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void StartEvolutionNew()
        {
            lastDrawing = null;
            lastErrorLevel = 0;
            lastWorstErrorLevelDiff = 0;
            errorLevel = long.MaxValue;
            selected = 0;
            currentDrawing = null;
            Settings.ActivePolygonsMax = MaxPolygons;
            Settings.ActivePointsMax = MaxPolygons * 3;
            generation = 0;


            Tools.ClearPseudoRandom();
            GASearch gaSearch = new GASearch(InitPopulation);
            gaSearch.InitFirstPopulation(sourceBitmap, EdgeThreshold);

            while (isRunning)
            {
                //if (generation > 14000) break;

                gaSearch.ExecuteGeneration();

                generation++;

                lastDrawing = gaSearch.LastBest;
                lastErrorLevel = gaSearch.LastBestFittness;
                lastWorstErrorLevelDiff = gaSearch.LastWorstFittness;

                if(gaSearch.CurrentBestFittness < errorLevel)
                {
                    selected++;

                    lock (DrawingLock)
                    {
                        currentDrawing = gaSearch.CurrentBest;
                        errorLevel = gaSearch.CurrentBestFittness;
                    }
                }              
            }
        }

        

        //covnerts the source image to a Color[,] for faster lookup
        private Bitmap ConvertImageIntoPARGB()
        {
           
            var sourceImage = picPattern.Image as Bitmap;

            if (sourceImage == null)
                throw new NotSupportedException("A source image of Bitmap format must be provided");

            Bitmap sourceBitmap = new Bitmap(Tools.MaxWidth, Tools.MaxHeight, PixelFormat.Format32bppPArgb);

            using (Graphics g = Graphics.FromImage(sourceBitmap))
            {
                g.DrawImage(sourceImage, new Rectangle(0,0,Tools.MaxWidth, Tools.MaxHeight), 0, 0, Tools.MaxWidth, Tools.MaxHeight,  GraphicsUnit.Pixel);
            }

            return sourceBitmap;
        }


        private void btnStart_Click(object sender, EventArgs e)
        {
            if (isRunning)
                Stop();
            else
                Start();
        }

        private void Start()
        {
            btnStart.Text = "Stop";
            isRunning = true;
            tmrRedraw.Enabled = true;

            if (thread != null)
                KillThread();

            //thread = new Thread(StartEvolution)
            //             {
            //                 IsBackground = true,
            //                 Priority = ThreadPriority.AboveNormal
            //             };

            thread = new Thread(StartEvolutionNew)
            {
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal
            };

            thread.Start();
        }

        private void KillThread()
        {
            if (thread != null)
            {
                thread.Abort();
            }
            thread = null;
        }

        private void Stop()
        {
            if (isRunning)
                KillThread();

            btnStart.Text = "Start";
            isRunning = false;
            tmrRedraw.Enabled = false;
            this.Text = Tools.randomCall.ToString();

            if(guiDrawing != null)
                StatRefresh(guiDrawing);

        }

        DateTime last = DateTime.Now;
        long lastGenetation = 0;
        
        private void tmrRedraw_Tick(object sender, EventArgs e)
        {


            if (currentDrawing == null)
                return;

            int polygons = 0;
            int points = 0;

            lock (DrawingLock)
            {
                polygons = currentDrawing.Polygons.Length;
                points = currentDrawing.PointCount;

            }

            double avg = 0;
            if (polygons != 0)
                avg = points/polygons;

            toolStripStatusLabelFitness.Text = errorLevel.ToString();
            toolStripStatusLabelGeneration.Text = generation.ToString();
            toolStripStatusLabelSelected.Text = selected.ToString();
            toolStripStatusLabelPoints.Text = points.ToString();
            toolStripStatusLabelPolygons.Text = polygons.ToString();
            toolStripStatusLabelAvgPoints.Text = avg.ToString();

            
            


            bool shouldRepaint = false;
            if (repaintIntervall.Ticks > 0)
                if (lastRepaint < DateTime.Now - repaintIntervall)
                    shouldRepaint = true;

            if (repaintOnSelectedSteps > 0)
                //if (lastSelected + repaintOnSelectedSteps < selected)
                    shouldRepaint = true;

            if (shouldRepaint)
            {
                lock (DrawingLock)
                {
                    if (chbLastGen.Checked)
                    {
                        guiDrawing = lastDrawing.Clone();
                    }
                    else
                    {
                        guiDrawing = currentDrawing.Clone();
                    }
                }
                //if (this.chbShowProgress.Checked)
                {
                    pnlCanvas.Invalidate();
                }

                lastRepaint = DateTime.Now;
                lastSelected = selected;
            }

            StatRefresh(guiDrawing);
         
            double speed = (generation - lastGenetation) / (DateTime.Now - last).TotalSeconds;

            this.Text = "speed: " + string.Format("{0:######.000}", speed) +
                " gen/s   Last Fittness: " + this.lastErrorLevel +
                " Worst Fittness Diff: " + string.Format("{0:####.000 }%", (this.lastWorstErrorLevelDiff / (this.lastErrorLevel/100.0d)) );
                //" Bad Angle: "+ guiDrawing.HasSomePolygonBadAngles();
            last = DateTime.Now;
            lastGenetation = generation;
        }

        private void pnlCanvas_Paint(object sender, PaintEventArgs e)
        {


            //if (isRunning && !this.chbShowProgress.Checked)
            //    return;
            e.Graphics.Clear(Color.Black);

            if (ZoomScale * picPattern.Width != backBuffer.Width ||
                ZoomScale * picPattern.Height != backBuffer.Height)
            {
                backBuffer.Dispose();
                backBuffer = new Bitmap(ZoomScale * picPattern.Width, ZoomScale * picPattern.Height,
                                            PixelFormat.Format32bppPArgb);
            }
           
            using (Graphics backGraphics = Graphics.FromImage(backBuffer)) 
            {
                e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighSpeed;
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;

                backGraphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                backGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                backGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                 backGraphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighSpeed;
                 backGraphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;

                 if ((guiDrawing != null) && chbShowResult.Checked)
                 {
                     backGraphics.Clear(Color.Black);
                     Renderer.Render(guiDrawing, backGraphics, ZoomScale);
                     e.Graphics.DrawImage(backBuffer, 0, 0);

                 }

                 if (chbShowEdges.Checked)
                 {
                     DnaPoint [] edgePoints = SourceBitmapEdges.EdgePoints;
                     for (int index = 0; index < edgePoints.Length; index++)
                     {
                         DnaPoint point = edgePoints[index];
                         e.Graphics.FillRectangle(new SolidBrush(Color.White),
                             point.X * ZoomScale, point.Y * ZoomScale, 1 * ZoomScale, 1 * ZoomScale);
                     }
                 }

            }
        }

        private void InitImage(Image img)
        {
            Stop();

            picPattern.Image = img;
            
            Tools.MaxHeight = picPattern.Height;
            Tools.MaxWidth = picPattern.Width;

            sourceBitmap = ConvertImageIntoPARGB();
            sourceBitmapAsCanvas = CanvasBGRA.CreateCanvasFromBitmap(sourceBitmap);
            UpdateSourceBitmapEdges();

            _dnaRender = new DNARenderer(Tools.MaxWidth, Tools.MaxHeight);

            SetCanvasSize();

            splitContainer1.SplitterDistance = picPattern.Width + 30;
            pnlCanvas.Invalidate();
        }

        private void UpdateSourceBitmapEdges()
        {
            EdgeDetector edgeDetector = new EdgeDetector(CanvasBGRA.CreateCanvasFromBitmap(sourceBitmap));
            edgeDetector.DetectEdges(EdgeThreshold);
            SourceBitmapEdges = edgeDetector.GetAllEdgesPoints();
        }

        private void OpenImage()
        {
            string fileName = FileUtil.GetOpenFileName(FileUtil.ImgExtension);
            if (string.IsNullOrEmpty(fileName))
                return;

            InitImage(Image.FromFile(fileName));
        }

        private void SetCanvasSize()
        {
            pnlCanvas.Height = ZoomScale * picPattern.Height;
            pnlCanvas.Width = ZoomScale * picPattern.Width;

            pnlCanvas.Invalidate();
            lastRepaint = DateTime.Now;
        }

        

        private void SaveDNA()
        {
            string fileName = FileUtil.GetSaveFileName(FileUtil.DnaExtension);
            if (string.IsNullOrEmpty(fileName) == false && currentDrawing != null)
            {
                lock (DrawingLock)
                {
                    if (currentDrawing != null)
                    {
                        SaveDNAAsSVG(currentDrawing, CanvasBGRA.CreateCanvasFromBitmap(this.sourceBitmap), fileName);
                    }
                }
               
            }
        }

        static private void SaveDNAAsSVG(DnaDrawing dna, CanvasBGRA sourceImage, string fileName)
        {
            DNARenderer _dnaRender = new DNARenderer(dna.Width, dna.Height);
            _dnaRender.RenderDNA(dna, DNARenderer.RenderType.SoftwareTriangle);
            MatchStatistics ms = new MatchStatistics();
            ms.ComputeImageMatchStatMed(sourceImage, _dnaRender.Canvas);

            string line1 = string.Format("Med: {0:###.000} / {1:###.000}  Sum: {2:###.000} / {3:###.000}  (med/stdev)",
                (ms.Diff_MedB + ms.Diff_MedG + ms.Diff_MedR) / 3,
                (ms.Diff_MedStdDevB + ms.Diff_MedStdDevG + ms.Diff_MedStdDevR) / 3,
                (ms.Diff_MedB + ms.Diff_MedG + ms.Diff_MedR),
                (ms.Diff_MedStdDevB + ms.Diff_MedStdDevG + ms.Diff_MedStdDevR)); 
            // 12 mezer
            string line2 = string.Format("R  : {0:###.000} / {1:####.000}", ms.Diff_MedR, ms.Diff_MedStdDevR);
            string line3 = string.Format("G  : {0:###.000} / {1:####.000}", ms.Diff_MedG, ms.Diff_MedStdDevG);
            string line4 = string.Format("B  : {0:###.000} / {1:####.000}", ms.Diff_MedB, ms.Diff_MedStdDevB);

             
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<?xml version=\"1.0\" standalone=\"no\"?>");
            sb.AppendLine("<!DOCTYPE svg PUBLIC \"-//W3C//DTD SVG 1.1//EN\" \"http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd\">");
            sb.AppendLine();
         
            sb.AppendLine("<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\">");
            sb.AppendLine("<g font-family=\"courier\" font-size=\"16\" >");
            sb.AppendLine("<text x=\"5\" y=\"20\" fill=\"black\"><tspan xml:space=\"preserve\">" + line1 + "</tspan></text>");
            sb.AppendLine("<text x=\"5\" y=\"40\" fill=\"black\"><tspan xml:space=\"preserve\">" + line2 + "</tspan></text>");
            sb.AppendLine("<text x=\"5\" y=\"60\" fill=\"black\"><tspan xml:space=\"preserve\">" + line3 + "</tspan></text>");
            sb.AppendLine("<text x=\"5\" y=\"80\" fill=\"black\"><tspan xml:space=\"preserve\">" + line4 + "</tspan></text>");
            sb.AppendLine("</g>");
            // addbackground
            sb.AppendLine("<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\" y=\"90\" >");
            sb.AppendFormat("<rect x=\"{0}\" y=\"{1}\" width=\"{2}\" height=\"{3}\"" +
                " style=\"fill:rgb({4},{5},{6});fill-opacity:{7};\"/>",
                0, 0, dna.Width, dna.Height, dna.BackGround.Red, dna.BackGround.Green, dna.BackGround.Blue,
                (dna.BackGround.Alpha / 255.0).ToString("0.###", CultureInfo.InvariantCulture));
            sb.AppendLine();

            foreach (var item in dna.Polygons)
            {
                StringBuilder sbPoints = new StringBuilder();
                sbPoints.AppendFormat("{0},{1} ", item.Points[0].X,item.Points[0].Y);
                sbPoints.AppendFormat("{0},{1} ", item.Points[1].X,item.Points[1].Y);
                sbPoints.AppendFormat("{0},{1} ", item.Points[2].X,item.Points[2].Y);

                sb.AppendFormat("<polygon points=\"{0}\" style=\"fill:rgb({1},{2},{3});fill-opacity:{4};\"/>",
                    sbPoints.ToString(), item.Brush.Red,item.Brush.Green, item.Brush.Blue,
                    (item.Brush.Alpha / 255.0).ToString("0.###",CultureInfo.InvariantCulture));
                sb.AppendLine();

            }
            sb.AppendLine("</svg>");
            sb.AppendLine("</svg>");


            byte [] text = UTF8Encoding.UTF8.GetBytes(sb.ToString());

            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                fs.Write(text, 0, text.Length);
            }

        }

        private void StatRefresh(DnaDrawing guiDrawing)
        {
            _dnaRender.RenderDNA(guiDrawing, DNARenderer.RenderType.SoftwareTriangle);
            MatchStatistics ms = new MatchStatistics();
            ms.ComputeImageMatchStatMed(sourceBitmapAsCanvas, _dnaRender.Canvas);

            // avg 

            //tsslFittnessError.Text = string.Format("Error (avg/stdev)  sum: {0:###.000} / {1:###.000}" +
            //    "       avg: {2:###.000} / {3:###.000}" +
            //    "       R: {4:###.000} / {5:####.000}," +
            //    "       G: {6:####.000} / {7:####.000}," +
            //    "       B: {8:####.000} / {9:####.000}",

            //    (ms.Diff_AvgB + ms.Diff_AvgG + ms.Diff_AvgR),
            //    (ms.Diff_AvgStdDevB + ms.Diff_AvgStdDevG + ms.Diff_AvgStdDevR),
            //    (ms.Diff_AvgB + ms.Diff_AvgG + ms.Diff_AvgR) / 3,
            //    (ms.Diff_AvgStdDevB + ms.Diff_AvgStdDevG + ms.Diff_AvgStdDevR) / 3,
            //    ms.Diff_AvgR, ms.Diff_AvgStdDevR, ms.Diff_AvgG, ms.Diff_AvgStdDevG,
            //    ms.Diff_AvgB, ms.Diff_AvgStdDevB);


            tsslFittnessError.Text = string.Format("Error (Med/stdev)  sum: {0:###} / {1:###.000}" +
    "       avg: {2:###.000} / {3:###.000}" +
    "       R: {4:000} / {5:000.000}," +
    "       G: {6:000} / {7:000.000}," +
    "       B: {8:000} / {9:000.000}",

    (ms.Diff_MedB + ms.Diff_MedG + ms.Diff_MedR),
    (ms.Diff_MedStdDevB + ms.Diff_MedStdDevG + ms.Diff_MedStdDevR),
    (ms.Diff_MedB + ms.Diff_MedG + ms.Diff_MedR) / 3,
    (ms.Diff_MedStdDevB + ms.Diff_MedStdDevG + ms.Diff_MedStdDevR) / 3,
    ms.Diff_MedR, ms.Diff_MedStdDevR, ms.Diff_MedG, ms.Diff_MedStdDevG,
    ms.Diff_MedB, ms.Diff_MedStdDevB);

        }

        

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            OpenImage();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            SaveDNA();
        }

        private void nudZoom_ValueChanged(object sender, EventArgs e)
        {
            SetCanvasSize();
        }

        private void nudEdgeThreshold_ValueChanged(object sender, EventArgs e)
        {
            UpdateSourceBitmapEdges();
            pnlCanvas.Invalidate();
        }

        private void chbShowResult_CheckedChanged(object sender, EventArgs e)
        {
            pnlCanvas.Invalidate();
        }

        private void chbShowEdges_CheckedChanged(object sender, EventArgs e)
        {
            pnlCanvas.Invalidate();
        }

        
    }
}