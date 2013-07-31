using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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

        private byte [] sourceColors;
        private Bitmap sourceBitmap;
        private Color Background = Color.Black;

        private Thread thread;

        public MainForm()
        {
           
            

            InitializeComponent();
            Text = FitnessCalculator.kk;
            Settings = Serializer.DeserializeSettings();
            if (Settings == null)
                Settings = new Settings();

            //Test();
            //TestSoftwareRenderPolygon();
            //TestBenchmark();

            InitImage();

           
        }

        private void TestBenchmark()
        {
            DnaDrawing dna = GetNewInitializedDrawing();

            for (int i =0; i < 100; i++)
                dna.AddPolygon();

            const int CONST_Width = 1000;
            const int CONST_Height = 1000;

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
            DnaDrawing dna = GetNewInitializedDrawing();

            for (int i =0; i < 100; i++)
                dna.AddPolygon();

            const int CONST_Width = 200;
            const int CONST_Height = 200;

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

        private static DnaDrawing GetNewInitializedDrawing()
        {
            var drawing = new DnaDrawing();
            drawing.Init();
            return drawing;
        }


        private void StartEvolutionNew()
        {
            SetupSourceColorMatrix();
            EdgeDetector ed = new EdgeDetector(sourceBitmap);
            ed.DetectEdges();
            DnaPoint[] tmpEdgePoints = ed.GetAllEdgesPoints();
            ed.SaveEdgesAsBitmap("ImageEdges.bmp");
            ed.SaveBitmapHSL("bmpHSL_H.bmp", true, false, false);
            ed.SaveBitmapHSL("bmpHSL_S.bmp", false, true, false);
            ed.SaveBitmapHSL("bmpHSL_L.bmp", false, false, true);


            GASearch gaSearch = new GASearch(10);
            gaSearch.InitFirstPopulation(sourceBitmap, sourceColors, tmpEdgePoints);

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
                    
                    currentDrawing = gaSearch.CurrentBest;
                    errorLevel = gaSearch.CurrentBestFittness;
                }              
            }
        }

        private void StartEvolution()
        {
            SetupSourceColorMatrix();
            if (currentDrawing == null)
                currentDrawing = GetNewInitializedDrawing();
            lastSelected = 0;

            DnaDrawing newDrawing = null;

            while (isRunning)
            {
                if (generation == 4000) break;

                
                    if (currentDrawing.Polygons.Length == 0)
                    {
                        for (int i =0; i < 100;i++ )
                            currentDrawing.AddPolygon();
                    }

                    //if(newDrawing == null)
                        newDrawing = currentDrawing.Clone();
                
                while(!newDrawing.IsDirty)
                    newDrawing.Mutate();

                if (newDrawing.IsDirty)
                {
                    generation++;

                    //long newErrorLevel = FitnessCalculator.GetDrawingFitnessWPF(newDrawing, sourceBitmap,Background);
                    long newErrorLevel = FitnessCalculator.GetDrawingFitness2(newDrawing, sourceBitmap, Background);
                    //long newErrorLevel = FitnessCalculator.GetDrawingFitnessSoftware(newDrawing, sourceBitmap, Background);
                    //long newErrorLevel = FitnessCalculator.GetDrawingFitness(newDrawing, sourceColors);

                    if (newErrorLevel < errorLevel)
                    {
                        selected++;
                        //lock (currentDrawing)
                        {
                            currentDrawing = newDrawing;
                        }
                        errorLevel = newErrorLevel;
                    }

                    //newDrawing = null;
                }
                //else, discard new drawing
            }
        }

        //covnerts the source image to a Color[,] for faster lookup
        private void SetupSourceColorMatrix()
        {
            sourceColors = new byte[Tools.MaxWidth*Tools.MaxHeight*4];
            int colorIndex = 0;

            var sourceImage = picPattern.Image as Bitmap;

            if (sourceImage == null)
                throw new NotSupportedException("A source image of Bitmap format must be provided");

            sourceBitmap = new Bitmap(Tools.MaxWidth, Tools.MaxHeight, PixelFormat.Format32bppPArgb);

            using (Graphics g = Graphics.FromImage(sourceBitmap))
            {
                g.DrawImage(sourceImage, new Rectangle(0,0,Tools.MaxWidth, Tools.MaxHeight), 0, 0, Tools.MaxWidth, Tools.MaxHeight,  GraphicsUnit.Pixel);
            }

            long sumR = 0;
            long sumG =0;
            long sumB = 0;

            for (int y = 0; y < Tools.MaxHeight; y++)
            {
                for (int x = 0; x < Tools.MaxWidth; x++)
                {
                    
                    Color c = sourceImage.GetPixel(x, y);
                    sumR += c.R;
                    sumG += c.G;
                    sumB += c.B;
                    
                    sourceColors[colorIndex] = c.B;
                    
                    colorIndex++;
                    sourceColors[colorIndex] = c.G;
                    colorIndex++;
                    sourceColors[colorIndex] = c.R;
                    colorIndex++;
                    colorIndex++;

                }
            }

            long countPixels = Tools.MaxWidth* Tools.MaxHeight;

            Background = Color.FromArgb((int)(sumR / countPixels), (int)(sumG / countPixels), (int)(sumB / countPixels));
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
        }

        DateTime last = DateTime.Now;
        long lastGenetation = 0;

        private void tmrRedraw_Tick(object sender, EventArgs e)
        {
            if (currentDrawing == null)
                return;

            int polygons = currentDrawing.Polygons.Length;
            int points = currentDrawing.PointCount;
            double avg = 0;
            if (polygons != 0)
                avg = points/polygons;

            toolStripStatusLabelFitness.Text = errorLevel.ToString();
            toolStripStatusLabelGeneration.Text = generation.ToString();
            toolStripStatusLabelSelected.Text = selected.ToString();
            toolStripStatusLabelPoints.Text = points.ToString();
            toolStripStatusLabelPolygons.Text = polygons.ToString();
            toolStripStatusLabelAvgPoints.Text = avg.ToString();

            tsslFittnessError.Text =  string.Format("{0:G}", (errorLevel/(double)(Tools.MaxHeight * Tools.MaxWidth * 3 * 255)) *100);

            


            bool shouldRepaint = false;
            if (repaintIntervall.Ticks > 0)
                if (lastRepaint < DateTime.Now - repaintIntervall)
                    shouldRepaint = true;

            if (repaintOnSelectedSteps > 0)
                //if (lastSelected + repaintOnSelectedSteps < selected)
                    shouldRepaint = true;

            if (shouldRepaint)
            {
                lock (currentDrawing)
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

            if (guiDrawing == null)
            {
                e.Graphics.Clear(Background);
                return;
            }


            using (
                var backBuffer = new Bitmap(trackBarScale.Value*picPattern.Width, trackBarScale.Value*picPattern.Height,
                                            PixelFormat.Format32bppPArgb))
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
                
                Renderer.Render(guiDrawing, backGraphics, trackBarScale.Value,Background);

                e.Graphics.DrawImage(backBuffer, 0, 0);
            }
        }

        private void InitImage()
        {
            Stop();


            picPattern.Image = Bitmap.FromFile(GenArt.Properties.Resources.ml1);

            Tools.MaxHeight = picPattern.Height;
            Tools.MaxWidth = picPattern.Width;

            SetCanvasSize();

            splitContainer1.SplitterDistance = picPattern.Width + 30;
        }

        private void OpenImage()
        {
            Stop();

            string fileName = FileUtil.GetOpenFileName(FileUtil.ImgExtension);
            if (string.IsNullOrEmpty(fileName))
                return;

            picPattern.Image = Image.FromFile(fileName);

            Tools.MaxHeight = picPattern.Height;
            Tools.MaxWidth = picPattern.Width;

            SetCanvasSize();

            splitContainer1.SplitterDistance = picPattern.Width + 30;
        }

        private void SetCanvasSize()
        {
            pnlCanvas.Height = trackBarScale.Value*picPattern.Height;
            pnlCanvas.Width = trackBarScale.Value*picPattern.Width;

            pnlCanvas.Invalidate();
            lastRepaint = DateTime.Now;
        }

        private void OpenDNA()
        {
            Stop();

            DnaDrawing drawing = Serializer.DeserializeDnaDrawing(FileUtil.GetOpenFileName(FileUtil.DnaExtension));
            if (drawing != null)
            {
                if (currentDrawing == null)
                    currentDrawing = GetNewInitializedDrawing();

                lock (currentDrawing)
                {
                    currentDrawing = drawing;
                    guiDrawing = currentDrawing.Clone();
                }
                pnlCanvas.Invalidate();
                lastRepaint = DateTime.Now;
            }
        }

        private void SaveDNA()
        {
            string fileName = FileUtil.GetSaveFileName(FileUtil.DnaExtension);
            if (string.IsNullOrEmpty(fileName) == false && currentDrawing != null)
            {
                DnaDrawing clone = null;
                lock (currentDrawing)
                {
                    clone = currentDrawing.Clone();
                }
                if (clone != null)
                    Serializer.Serialize(clone, fileName);
            }
        }

       

        private void sourceImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenImage();
        }

        private void dNAToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenDNA();
        }

        private void dNAToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SaveDNA();
        }

        private void trackBarScale_Scroll(object sender, EventArgs e)
        {
            SetCanvasSize();
        }

        
    }
}