using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenArt.AST;
using GenArt.Classes;
using GenArt.Core.Classes;
using GenArt.Core.Classes.SWRenderLibrary;
using GenArtCoreNative;

namespace GenArt.Core.AST
{
    public class GASearch : IDisposable
    {
        public enum TypeRendering { software, softwareByRow, softwareByRowWithFitness };

        private DnaDrawing _currentBest;
        private long _currentBestFittness;
        private DnaDrawing _lastBest;
        private long _lastBestFittness;
        private long _lastWorstFitnessDiff;
        private int _generation = 0;
        private DnaDrawing [] _lastPopulation;
        private DnaDrawing [] _population;

        private byte [][] _populationCompressDNA;
        private byte [][] _lastPopulationCompressDNA;
        private byte [] _currentBestCompressDNA;
        private byte [] _lastBestCompressDNA;



        private CanvasBGRA _destCanvas = new CanvasBGRA(1, 1);

        private NativeFunctions _nativeFunc = new NativeFunctions();

        private ImageEdges _edgePoints = null;
        private ErrorMatrix _errorMatrix = new ErrorMatrix(1, 1);

        private int [] _rouleteTable = new int[0];
        private int [] _rankTable = new int[0];
        private long [] _diffFittness = new long[0];
        private long [] _fittness = new long[0];
        private float [] _similarity = new float[0];

        public int CONST_DynamicMutationGenInterval = 1000;
        private const byte CONST_MutationMaxRate = 255; // 255 means big mutation changes

        private byte _crLastMutationRate = 255;
        private int _crLastGenerationNumber = 0;
        private long _crLastBestFittness = 0;

        private int _popSize=  1;

        private TypeRendering _typeRendering = TypeRendering.software;




        #region CanvasForRender

        DNARenderer _dnaRender = new DNARenderer(1, 1);

        public long fillPixels
        {
            get { return _dnaRender.FillPixels; }
        }

        #endregion



        #region property


        public DnaDrawing CurrentBest
        {
            get { return _currentBest; }
        }

        public long CurrentBestFittness
        {
            get { return _currentBestFittness; }
        }

        public DnaDrawing LastBest
        {
            get { return _lastBest; }
        }

        public long LastBestFittness
        {
            get { return _lastBestFittness; }
            set { _lastBestFittness = value; }
        }

        public long LastWorstFittness
        {
            get { return _lastWorstFitnessDiff; }
        }

        public byte CurrMutateRate { get { return this._crLastMutationRate; } }

        public TypeRendering TypeRender
        {
            get { return _typeRendering; }
            set { _typeRendering = value; }
        }


        #endregion

        public ErrorMatrix ErrorMatrixCurrentClone() { return _errorMatrix.Clone(); }

        public GASearch(int popSize)
        {
            //if (popSize < 2)
            //    popSize = 2;

            _population = new DnaDrawing[popSize + 1];
            _lastPopulation = new DnaDrawing[popSize + 1];

            _populationCompressDNA = new byte [popSize + 1][];
            _lastPopulationCompressDNA = new byte [popSize + 1][];


            _rouleteTable = new int[popSize + 1];
            _rankTable = new int[popSize + 1];
            _diffFittness = new long[popSize + 1];
            _fittness = new long[popSize + 1];
            _similarity = new float[popSize + 1];
            _popSize = popSize;




        }

        private static ImageEdges CreateEdges(CanvasBGRA destImg, int EdgeThreshold)
        {
            EdgeDetector ed = new EdgeDetector(destImg);
            ed.DetectEdges(EdgeThreshold);

            ed.SaveEdgesAsBitmap("ImageEdges.bmp");
            ed.SaveBitmapHSL("bmpHSL_H.bmp", true, false, false);
            ed.SaveBitmapHSL("bmpHSL_S.bmp", false, true, false);
            ed.SaveBitmapHSL("bmpHSL_L.bmp", false, false, true);
            ed.SaveImageGreyscaleAsBitmap("ImageGreyscale.bmp");
            ed.SaveImageEdgeDirectionsAsBitmap("ImageEdgeDirection.bmp");

            return ed.GetAllEdgesPoints();

        }

        public void InitFirstPopulation(Bitmap destImg, int EdgeTreshold)
        {
            DnaDrawing.RecycleClear();

            this._generation = 0;
            this._destCanvas = CanvasBGRA.CreateCanvasFromBitmap(destImg);
            this._destCanvas.ReduceNoiseMedian();
            this._destCanvas.ReduceNoiseMedian(true);

            //this._destCanvas.ReduceNoiseMedian();
            //this._destCanvas.ReduceNoiseMedian();
            //this._destCanvas.ReduceNoiseMedian();
            //this._destCanvas.ReduceNoiseMedian();
            //this._destCanvas.ReduceNoiseMedian();
            //this._destCanvas.ReduceNoiseMedian();
            //this._destCanvas.ReduceNoiseMedian();
            CanvasBGRA.CreateBitmpaFromCanvas(this._destCanvas).Save("ImageMedianNoise.bmp");

            this._currentBestFittness = long.MaxValue;
            this._lastBestFittness = long.MaxValue;

            _errorMatrix = new ErrorMatrix(this._destCanvas.WidthPixel, this._destCanvas.HeightPixel);

            _dnaRender = new DNARenderer(_destCanvas.WidthPixel, _destCanvas.HeightPixel);
            _dnaRender.DestCanvas = this._destCanvas;

            //this._edgePoints = CreateEdges(this._destCanvas, EdgeTreshold);
            //this._destCanvas.EasyColorReduction();

            //DnaBrush backGround = new DnaBrush(255, 0, 0, 0);

            DnaBrush backGround = ComputeBackgroundColor(this._destCanvas);


            for (int i =0; i < this._population.Length; i++)
            {
                DnaDrawing dna = new DnaDrawing(this._destCanvas.WidthPixel, this._destCanvas.HeightPixel);
                //dna.BackGround.InitRandomWithoutAlpha();
                dna.BackGround = backGround;
                //dna.BackGround.InitRandomWithoutAlpha();

                for (int k =0; k < 10; k++)
                {
                    //dna.AddRectangle(null, this._destCanvas, this._edgePoints);
                    //dna.MutationAddPolygon(255, null, this._destCanvas, this._edgePoints);

                }

                this._population[i] = dna;
            }

            this._lastBest = this._population[_population.Length - 1].Clone();
            this._currentBest = this._lastBest.Clone();
            this._fittness[_population.Length - 1] = long.MaxValue;

            // nezbytne aby doslo k vypoctu novych fittness
            ComputeFittness();

            _crLastMutationRate = 255;
            _crLastGenerationNumber = 0;
            _crLastBestFittness = long.MaxValue;

            // compress version dna
            Helper_InitPoluplation_CompressDNA();
        }

        private static DnaBrush ComputeBackgroundColor(CanvasBGRA image)
        {
            Median8bit mr = new Median8bit();
            Median8bit mg = new Median8bit();
            Median8bit mb = new Median8bit();

            for (int index = 0; index < image.Data.Length; index += 4)
            {
                mb.InsertData(image.Data[index]);
                mg.InsertData(image.Data[index + 1]);
                mr.InsertData(image.Data[index + 2]);
            }

            return new DnaBrush(255, (int)mr.Median, (int)mg.Median, (int)mb.Median);

        }


        public void ExecuteGeneration()
        {
            GenerateNewPopulationByMutation();
            //GenerateNewPopulationRoulete();

            ComputeFittness();
            UpdateStatsByFittness();

            //GenerateNewPopulationBasic();
         
            this._generation++;
        }

        public void ExecuteGenerationPure()
        {
            Helper_GenerateNewPopulation_CompressDNA();
            
            Helper_ComputeFittness_CompressDNA();
            Helper_UpdateStatsByFittness_CompressDNA();

            this._generation++;
        }


        private void ComputeFittness()
        {
            DNARenderer.RenderType renderType = DNARenderer.RenderType.Software;

            if (_typeRendering == GASearch.TypeRendering.software) renderType = DNARenderer.RenderType.Software;
            else if (_typeRendering == GASearch.TypeRendering.softwareByRow) renderType = DNARenderer.RenderType.SoftwareByRows;
            else if (_typeRendering == GASearch.TypeRendering.softwareByRowWithFitness) renderType = DNARenderer.RenderType.SoftwareByRowsWithFittness;


            for (int index = 0; index < this._popSize; index++)
            {
                _dnaRender.RenderDNA(this._population[index], renderType);

                //long fittness = FitnessCalculator.ComputeFittness_Basic(_destCanvas.Data, _dnaRender.Canvas.Data,1// this._generation%10+1);
                //);
                //long fittness = FitnessCalculator.ComputeFittness_Basic2(_destCanvas.Data, _dnaRender.Canvas.Data);
                //long fittness = FitnessCalculator.ComputeFittness_2d3(_destCanvas.Data, _dnaRender.Canvas.Data, _destCanvas.Width);

                //long fittness = _nativeFunc.ComputeFittness_2d(_destCanvas.Data, _dnaRender.Canvas.Data, _dnaRender.Canvas.Width);
                //long fittness = _nativeFunc.ComputeFittness_2d_2x2(_destCanvas.Data, _dnaRender.Canvas.Data, _dnaRender.Canvas.Width);

                //long fittness = FitnessCalculator.ComputeFittness_BasicAdvance(_destCanvas.Data, _dnaRender.Canvas.Data);
                //long fittness = _nativeFunc.ComputeFittnessTile(_destCanvas.Data, _dnaRender.Canvas.Data, _dnaRender.Canvas.WidthPixel);
                //long fittness = _nativeFunc.ComputeFittnessAdvance(_destCanvas.Data, _dnaRender.Canvas.Data);

                long fittness = 0;
                if (_typeRendering == TypeRendering.softwareByRowWithFitness)
                    fittness = _dnaRender.Fittness;
                else
                    fittness = _nativeFunc.ComputeFittnessABSSSE(_destCanvas.Data, _dnaRender.Canvas.Data);
                //fittness = FitnessCalculator.ComputeFittnessLine_SumABS(_destCanvas.Data, _dnaRender.Canvas.Data);

                long bloat = this._population[index].PointCount;

                _fittness[index] = fittness + bloat ;

                //fittness[index] = FitnessCalculator.GetDrawingFitness2(this._population[index], this._destImg, Color.Black);
                //_fittness[index] = FitnessCalculator.GetDrawingFitnessSoftware(this._population[index], this._destCanvas, Color.Black);
                //fittness[index] = FitnessCalculator.GetDrawingFitnessSoftwareNative(this._population[index], this._destImg, this._destImgByte, Color.Black);
                //fittness[index] = FitnessCalculator.GetDrawingFitnessWPF(this._population[index], this._destCanvas, Color.Black);    
            }
        }

        private void UpdateStatsByFittness()
        {
            long bestFittness = long.MaxValue;
            long WorstFittness = 0;
            int bestIndex = -1;
            for (int index = 0; index < this._popSize; index++)
            {
                if (_fittness[index] > WorstFittness)
                {
                    WorstFittness = this._fittness[index];
                }

                if (this._fittness[index] < bestFittness )
                {
                    bestFittness = this._fittness[index];
                    bestIndex = index;
                }
            }

            if (bestFittness <= this._currentBestFittness)
            {
                this._currentBestFittness = bestFittness;
                this._currentBest = this._population[bestIndex].Clone(); // klon nutny kvuli recyklaci primitiv

                //ComputeCurrentBestErrorMatrix();
            }

            this._lastBest = this._population[bestIndex].Clone(); // klon nutny kvuli recyklaci primitiv
            this._lastBestFittness = bestFittness;

            //ComputeCurrentBestErrorMatrix(this.LastBest);

            // aplikovani pridani nejlepsiho do kolekce
            if (_generation % 1 == 5)
            {
                _fittness[this._population.Length - 1] = this._currentBestFittness;
                _population[this._population.Length - 1] = this._currentBest.Clone(); // klon nutny kvuli recyklaci primitiv
            }
            else
            {
                _fittness[this._population.Length - 1] = this._lastBestFittness;
                _population[this._population.Length - 1] = this._lastBest.Clone(); // klon nutny kvuli recyklaci primitiv
            }

            _lastWorstFitnessDiff = WorstFittness - this._lastBestFittness;


        }

        private void ComputeCurrentBestErrorMatrix()
        {
            _dnaRender.RenderDNA(this._currentBest, DNARenderer.RenderType.Software);

            _errorMatrix.ComputeErrorMatrix(this._destCanvas, _dnaRender.Canvas);

        }

        private void ComputeCurrentBestErrorMatrix(DnaDrawing dna)
        {
            _dnaRender.RenderDNA(dna, DNARenderer.RenderType.Software);

            _errorMatrix.ComputeErrorMatrix(this._destCanvas, _dnaRender.Canvas);

        }

        private void ComputeSimilarity()
        {
            HashSet<int> allIds = new HashSet<int>();
            HashSet<int> theSameIds = new HashSet<int>();

            int maxLength = _population.Max(x => x.Polygons.Length);

            for (int index = 0; index < _population.Length; index++)
            {
                DnaPrimitive [] polygons = _population[index].Polygons;
                for (int polygonIndex = 0; polygonIndex < polygons.Length; polygonIndex++)
                {
                    bool wasAdd = allIds.Add(polygons[polygonIndex].UniqueId);

                    // if polygon exist safe him
                    if (!wasAdd)
                        theSameIds.Add(polygons[polygonIndex].UniqueId);
                }
            }

            // compute similarity
            for (int index = 0; index < _population.Length; index++)
            {
                DnaPrimitive [] polygons = _population[index].Polygons;
                int countSame =0;

                for (int polygonIndex = 0; polygonIndex < polygons.Length; polygonIndex++)
                {
                    if (theSameIds.Contains(polygons[polygonIndex].UniqueId))
                        countSame++;
                }

                _similarity[index] = countSame / ((float)polygons.Length + 0.1f * (maxLength - polygons.Length));
            }

            //if (allIds.Count != theSameIds.Count)
            //{
            //    int i = 0;
            //    allIds.Clear();
            //}
        }


        private byte GetCurrentMutationRate()
        {
            //int positionInMutationRate = (this._generation % CONST_DynamicMutationGenInterval);
            int positionInMutationRate =  CONST_DynamicMutationGenInterval - 1 - (this._generation % CONST_DynamicMutationGenInterval);


            byte mutatioRate =  (byte)((positionInMutationRate * CONST_MutationMaxRate) / CONST_DynamicMutationGenInterval);

            return mutatioRate;
        }

        private byte GetCurrentMutationRate2()
        {
            if (_generation - _crLastGenerationNumber >= CONST_DynamicMutationGenInterval / 1000)
            {
                if (this._currentBestFittness < (this._crLastBestFittness))// - this._crLastBestFittness / 1000))
                {
                    _crLastBestFittness = this._currentBestFittness;
                    _crLastGenerationNumber = _generation;
                }
                else
                {
                    if (this._crLastMutationRate == 0) this._crLastMutationRate = 255;
                    else
                    {
                        //if (this._currentBestFittness == this._crLastBestFittness)
                        //    this._crLastMutationRate >>= 1;
                        //else
                        this._crLastMutationRate--;

                    }

                    _crLastBestFittness = this._currentBestFittness;
                    _crLastGenerationNumber = _generation;
                }

            }

            return this._crLastMutationRate;
        }

        private void GenerateNewPopulationBasic()
        {
            DnaDrawing [] newPopulation = new DnaDrawing[_popSize + 1];

            newPopulation[this._population.Length] = this._currentBest.Clone();

            for (int index = 1; index < this._population.Length; index++)
            {
                int indexParent1 = Tools.GetRandomNumber(0, this._population.Length);
                int indexParent2 = indexParent1;

                while (indexParent1 == indexParent2)
                    indexParent2 = Tools.GetRandomNumber(0, this._population.Length);

                newPopulation[index] = CrossoverBasic(this._population[indexParent1], this._population[indexParent2]);
            }

            this._population = newPopulation;
        }

        private void GenerateNewPopulationByMutation()
        {
            int maxNormalizeValue = this._fittness.Length * 100000;
            //int [] rouleteTable = RouletteTableNormalize(fittness,maxNormalizeValue);
            //ComputeSimilarity();
            //RouletteTableNormalizeBetter(this._fittness, this._rouleteTable, this._diffFittness,  maxNormalizeValue);
            //RouletteTableNormalizeBetterWithSimilarity2(this._fittness, this._rouleteTable, this._diffFittness, this._similarity, maxNormalizeValue);
            RankTableFill2(this._fittness, this._rankTable, out maxNormalizeValue);

            Tools.swap<DnaDrawing[]>(ref this._population, ref this._lastPopulation);
            
            this._population[this._population.Length - 1] = null;

            byte currMutatioRate = //(byte)(((this._generation % CONST_DynamicMutationGenInterval) > CONST_DynamicMutationGenInterval / 2) ? 255 : 64);
             GetCurrentMutationRate();

            for (int index = 0; index < _popSize; index++)
            {
                //int indexParent1 = Tools.GetRandomNumber(0, maxNormalizeValue + 1);
                //indexParent1 = RouletteVheelParrentIndex(indexParent1, this._rouleteTable);

                //int indexParent1 = Tools.GetRandomNumber(0, maxNormalizeValue + 1);
                //indexParent1 = RankVheelParrentIndex(indexParent1, this._rankTable);
                int indexParent1 = this._fittness.Length - 1;




                //int tmpindexParent1 = Tools.GetRandomNumber(0, this._fittness.Length);
                //int indexParent1 = tmpindexParent1;
                //int tmpindexParent2 = Tools.GetRandomNumber(0, this._fittness.Length);


                //if (this._fittness[tmpindexParent1] >
                //   this._fittness[tmpindexParent2])
                //    indexParent1 = tmpindexParent2;

                //tmpindexParent2 = Tools.GetRandomNumber(0, this._fittness.Length);

                //if (this._fittness[tmpindexParent1] >
                //  this._fittness[tmpindexParent2])
                //    indexParent1 = tmpindexParent2;


                if (this._population[index] != null)
                {
                    DnaDrawing.RecyclePrimitive(this._population[index].Polygons);
                    this._population[index] = null;
                }

                DnaDrawing dna = this._lastPopulation[indexParent1].Clone();
                //ComputeCurrentBestErrorMatrix(dna);
                while (!dna.IsDirty)
                    dna.MutateBetter(currMutatioRate,
                        null,//this._errorMatrix,
                        this._destCanvas,
                        _edgePoints
                        );

                
                
                this._population[index] = dna;
            }



            //this._population = newPopulation;
        }

        private void GenerateNewPopulationRoulete()
        {
            int maxNormalizeValue = this._fittness.Length * 100000;
            //int [] rouleteTable = RouletteTableNormalize(fittness,maxNormalizeValue);
            //RouletteTableNormalize(this._fittness, this._rouleteTable, maxNormalizeValue);
            //RouletteTableNormalizeBetter(this._fittness, this._rouleteTable, this._diffFittness, maxNormalizeValue);
            RankTableFill2(this._fittness, this._rankTable, out maxNormalizeValue);

            Tools.swap<DnaDrawing[]>(ref this._population, ref this._lastPopulation);
           
            byte currMutatioRate = //(byte)(((this._generation % CONST_DynamicMutationGenInterval) > CONST_DynamicMutationGenInterval / 2) ? 255 : 64);
             GetCurrentMutationRate();

            for (int index = 0; index < _popSize; index++)
            {
                int indexParent1 = Tools.GetRandomNumber(0, maxNormalizeValue + 1);
                indexParent1 = RankVheelParrentIndex(indexParent1, this._rankTable);


                int indexParent2 = indexParent1;

                while (indexParent1 == indexParent2) 
                {
                    int tmp = Tools.GetRandomNumber(0, maxNormalizeValue + 1);
                    indexParent2 = RankVheelParrentIndex(tmp, this._rankTable);
                }

                //newPopulation[index] = CrossoverBasic(this._population[indexParent1], this._population[indexParent2]);

                DnaDrawing  dna = CrossoverOnePoint(this._lastPopulation[indexParent1], this._lastPopulation[indexParent2]);

                while (!dna.IsDirty)
                    dna.MutateBetter(currMutatioRate, this._errorMatrix, this._destCanvas, _edgePoints);

                if (this._population[index] != null)
                    DnaDrawing.RecyclePrimitive(this._population[index].Polygons);

                this._population[index] = dna;

            }




        }

        private class compare : IComparer<long>
        {
            public compare()
            {

            }

            #region IComparer<long> Members

            public int Compare(long x, long y)
            {
                if (x == y) return 0;
                if (x > y) return -1;

                return 1;
            }

            #endregion
        }

        private static void RankTableFill(long[] fittness, int[] rankTable, out int MaxValueRankTable)
        {
            int nextRankValue = 100;
            for (int i = 0; i < rankTable.Length; i++)
            {
                rankTable[i] = nextRankValue;
                nextRankValue += nextRankValue / 2;
            }

            MaxValueRankTable = nextRankValue;
        }

        private static void RankTableFill2(long[] fittness, int[] rankTable, out int MaxValueRankTable)
        {
            int rankbasevalue = 10000;
            double sp = 1.9;
            // implementace vzorce
            // 2-sp+(2*(sp-1)*((pos-1)/(n-1)  
            // pos = 1 nejmensi fittness, sp = <1.0,2.0>  1.0 - linearni

            int lastValue = 0;
            for (int i = 0; i < rankTable.Length; i++)
            {
                double rnd = (i) / ((double)rankTable.Length - 1);

                double currentRank = Math.Pow(rnd, 2);

                rankTable[i] = lastValue + (int)(currentRank * rankbasevalue);
                lastValue = rankTable[i];
            }


            MaxValueRankTable = rankTable[rankTable.Length - 1] + rankbasevalue;
        }

        private static void RankTableFill3(long[] fittness, int[] rankTable, out int MaxValueRankTable)
        {
            int rankbasevalue = 10000;
            double sp = 1.9;
            // implementace vzorce
            // 2-sp+(2*(sp-1)*((pos-1)/(n-1)  
            // pos = 1 nejmensi fittness, sp = <1.0,2.0>  1.0 - linearni

            int lastValue = 0;
            for (int i = 0; i < rankTable.Length; i++)
            {
                double currentRank = 2.0 - sp + (2 * (sp - 1.0) * ((i) / ((double)rankTable.Length - 1)));

                rankTable[i] = lastValue + (int)(currentRank * rankbasevalue);
                lastValue = rankTable[i];
            }


            MaxValueRankTable = rankTable[rankTable.Length - 1] + rankbasevalue;
        }

        private static void RouletteTableNormalize(long[] fittness, int[] rouleteTable, int maxNormalizeValue)
        {

            long sumFittness = 0;
            for (int index = 0; index < fittness.Length; index++)
                sumFittness += fittness[index];

            int lastRouleteValue = 0;
            for (int index = 0; index < fittness.Length; index++)
            {
                int tmp = (int)((fittness[index] / (float)sumFittness) * maxNormalizeValue);
                rouleteTable[index] = lastRouleteValue + tmp;
                lastRouleteValue = lastRouleteValue + tmp;
            }
        }

        private static void RouletteTableNormalizeBetter(long[] fittness, int[] rouleteTable, long[] diffFittness, int maxNormalizeValue)
        {
            long fittnessMax = 0;
            long fittnessMin = long.MaxValue;

            for (int index = 0; index < fittness.Length; index++)
            {
                if (fittnessMax < fittness[index]) fittnessMax = fittness[index];
                if (fittnessMin > fittness[index]) fittnessMin = fittness[index];
            }

            long sumFittness = 0;
            long minDiffFit = fittnessMax - fittnessMin;
            for (int index = 0; index < fittness.Length; index++)
            {
                long diffFit = (fittnessMax - fittness[index] + minDiffFit);
                sumFittness += diffFit;
                diffFittness[index] = diffFit;
            }


            int lastRouleteValue = 0;
            for (int index = 0; index < diffFittness.Length; index++)
            {
                int tmp = (int)(((long)diffFittness[index] * maxNormalizeValue) / sumFittness);
                rouleteTable[index] = lastRouleteValue + tmp;
                lastRouleteValue = lastRouleteValue + tmp;
            }
        }

        private static void RouletteTableNormalizeBetterWithSimilarity
            (long[] fittness, int[] rouleteTable, long[] diffFittness, float[] similarity, int maxNormalizeValue)
        {
            long fittnessMax = 0;
            long fittnessMin = long.MaxValue;

            for (int index = 0; index < fittness.Length; index++)
            {
                if (fittnessMax < fittness[index]) fittnessMax = fittness[index];
                if (fittnessMin > fittness[index]) fittnessMin = fittness[index];
            }

            long sumFittness = 0;
            long minDiffFit = 0;//(fittnessMax - fittnessMin)/8;
            if (minDiffFit == 0) minDiffFit = 1;

            for (int index = 0; index < fittness.Length; index++)
            {
                // similarity 1.0 very similar, 0.0 very different  
                // koef multiple increase for more different. Min is 1.0;

                float koef = (1.0f - similarity[index]) * 1.5f + 1.0f;
                long diffFit = (long)(((fittnessMax - fittness[index]) + minDiffFit) * koef);
                sumFittness += diffFit;
                diffFittness[index] = diffFit;
            }


            int lastRouleteValue = 0;
            for (int index = 0; index < diffFittness.Length; index++)
            {
                int tmp = (int)(((long)diffFittness[index] * maxNormalizeValue) / sumFittness);
                rouleteTable[index] = lastRouleteValue + tmp;
                lastRouleteValue = lastRouleteValue + tmp;
            }
        }

        private static void RouletteTableNormalizeBetterWithSimilarity2
            (long[] fittness, int[] rouleteTable, long[] diffFittness, float[] similarity, int maxNormalizeValue)
        {
            long fittnessMax = 0;
            long fittnessMin = long.MaxValue;

            for (int index = 0; index < fittness.Length; index++)
            {
                if (fittnessMax < fittness[index]) fittnessMax = fittness[index];
                if (fittnessMin > fittness[index]) fittnessMin = fittness[index];
            }

            long sumFittness = 0;
            long minDiffFit = 1;//fittnessMin;

            for (int index = 0; index < fittness.Length; index++)
            {
                // similarity 1.0 very similar, 0.0 very different  
                // koef multiple increase for more different. Min is 1.0;

                float koef = (1.0f - similarity[index]) * 1.5f + 1.0f;
                long diffFit = (long)(((fittnessMax - fittness[index]) + minDiffFit) * koef);
                sumFittness += diffFit;
                diffFittness[index] = diffFit;
            }

            if (sumFittness == 0) minDiffFit = 1;

            int lastRouleteValue = 0;
            for (int index = 0; index < diffFittness.Length; index++)
            {
                int tmp = (int)(((long)diffFittness[index] * maxNormalizeValue) / sumFittness);
                rouleteTable[index] = lastRouleteValue + tmp;
                lastRouleteValue = lastRouleteValue + tmp;
            }
        }

        private int RouletteVheelParrentIndex(int value, int[] rouletteTable)
        {
            for (int index = 0; index < rouletteTable.Length; index++)
            {
                if (rouletteTable[index] > value)
                    return index;
            }

            return rouletteTable.Length - 1;
        }

        private int RankVheelParrentIndex(int value, int[] rankTable)
        {
            for (int index = 0; index < rankTable.Length; index++)
            {
                if (rankTable[index] > value)
                    return index;
            }

            return rankTable.Length - 1;
        }

        private DnaDrawing CrossoverBasic(DnaDrawing parent1, DnaDrawing parent2)
        {
            DnaDrawing result = new DnaDrawing(this._destCanvas.WidthPixel, this._destCanvas.HeightPixel);

            List<DnaPrimitive> polygons = new List<DnaPrimitive>();
            int maxIndex = Math.Max(parent1.Polygons.Length, parent2.Polygons.Length);
            for (int index = 0; index < maxIndex; index++)
            {
                DnaDrawing tmp = (Tools.GetRandomNumber(1, 1000) > 500) ? parent1 : parent2;

                if (index < tmp.Polygons.Length)
                {
                    polygons.Add((DnaPrimitive)tmp.Polygons[index].Clone());
                }
            }

            result.Polygons = polygons.ToArray();

            return result;
        }

        private DnaDrawing CrossoverOnePoint(DnaDrawing parent1, DnaDrawing parent2)
        {
            //double crossLine = Tools.GetRandomNumber(1,9)*0.1d;

            double crossLine = 0.3;


            int countCrossGenP1 = (int)(parent1.Polygons.Length * crossLine);
            int countCrossGenP2 = (int)(parent2.Polygons.Length * (crossLine));
            int newDnaSize = countCrossGenP1 + (parent2.Polygons.Length - countCrossGenP2 * 1);

            DnaDrawing result = new DnaDrawing(this._destCanvas.WidthPixel, this._destCanvas.HeightPixel);

            DnaPrimitive [] polygons = new DnaPrimitive[newDnaSize];
            int polygonsIndex = 0;

            int maxIndex = Math.Max(parent1.Polygons.Length, parent2.Polygons.Length);

            for (int index = 0; index < countCrossGenP1; index++)
            {
                polygons[polygonsIndex++] = (DnaPrimitive)parent1.Polygons[index].Clone();
            }



            for (int index = countCrossGenP2; index < parent2.Polygons.Length; index++)
            {
                polygons[polygonsIndex++] = (DnaPrimitive)parent2.Polygons[index].Clone();
            }

            result.Polygons = polygons;

            return result;
        }

        private DnaDrawing CrossoverOnePoint2(DnaDrawing parent1, DnaDrawing parent2)
        {
            //double crossLine = Tools.GetRandomNumber(1,9)*0.1d;

            double crossLine = 0.3;


            DnaDrawing result = new DnaDrawing(this._destCanvas.WidthPixel, this._destCanvas.HeightPixel);

            List<DnaPrimitive> polygons = new List<DnaPrimitive>();
            int maxIndex = Math.Max(parent2.Polygons.Length, parent2.Polygons.Length);

            int countCrossGen = (int)(parent2.Polygons.Count() * crossLine);

            for (int index = 0; index < countCrossGen; index++)
            {
                polygons.Add((DnaPrimitive)parent2.Polygons[index].Clone());
            }

            countCrossGen = (int)(parent1.Polygons.Count() * (crossLine));

            for (int index = countCrossGen; index < parent1.Polygons.Length; index++)
            {
                polygons.Add((DnaPrimitive)parent1.Polygons[index].Clone());
            }

            result.Polygons = polygons.ToArray();

            return result;
        }

        #region dna compressed genetic algorithm

        private void Helper_InitPoluplation_CompressDNA()
        {
            for (int i = 0; i < _populationCompressDNA.Length; i++)
            {
                byte [] dna = new byte[2048];
                for (int di = 0; di < dna.Length; di++)
                {
                    dna[di] = (byte)Tools.GetRandomNumber(0, 256);
                }

                _populationCompressDNA[i] = dna;
            }
        }

        private void Helper_ComputeFittness_CompressDNA()
        {
            DNARenderer.RenderType renderType = DNARenderer.RenderType.Software;

            if (_typeRendering == GASearch.TypeRendering.software) renderType = DNARenderer.RenderType.Software;
            else if (_typeRendering == GASearch.TypeRendering.softwareByRow) renderType = DNARenderer.RenderType.SoftwareByRows;
            else if (_typeRendering == GASearch.TypeRendering.softwareByRowWithFitness) renderType = DNARenderer.RenderType.SoftwareByRowsWithFittness;


            for (int index = 0; index < this._popSize; index++)
            {
                _dnaRender.RenderDNA_CompressDNA(this._populationCompressDNA[index], renderType);

                //long fittness = FitnessCalculator.ComputeFittness_Basic(_destCanvas.Data, _dnaRender.Canvas.Data,1// this._generation%10+1);
                //);
                //long fittness = FitnessCalculator.ComputeFittness_Basic2(_destCanvas.Data, _dnaRender.Canvas.Data);
                //long fittness = FitnessCalculator.ComputeFittness_2d3(_destCanvas.Data, _dnaRender.Canvas.Data, _destCanvas.Width);

                //long fittness = _nativeFunc.ComputeFittness_2d(_destCanvas.Data, _dnaRender.Canvas.Data, _dnaRender.Canvas.Width);
                //long fittness = _nativeFunc.ComputeFittness_2d_2x2(_destCanvas.Data, _dnaRender.Canvas.Data, _dnaRender.Canvas.Width);

                //long fittness = FitnessCalculator.ComputeFittness_BasicAdvance(_destCanvas.Data, _dnaRender.Canvas.Data);
                //long fittness = _nativeFunc.ComputeFittnessTile(_destCanvas.Data, _dnaRender.Canvas.Data, _dnaRender.Canvas.WidthPixel);
                //long fittness = _nativeFunc.ComputeFittnessAdvance(_destCanvas.Data, _dnaRender.Canvas.Data);

                long fittness = 0;
                if (_typeRendering == TypeRendering.softwareByRowWithFitness)
                    fittness = _dnaRender.Fittness;
                else
                    fittness = _nativeFunc.ComputeFittnessSquareSSE(_destCanvas.Data, _dnaRender.Canvas.Data);
                //fittness = FitnessCalculator.ComputeFittnessLine_SumSquare(_destCanvas.Data, _dnaRender.Canvas.Data);

                long bloat = Helper_DnaCountPoints(this._populationCompressDNA[index]);

                _fittness[index] = fittness + bloat * bloat + this._populationCompressDNA[index].Length;

                //fittness[index] = FitnessCalculator.GetDrawingFitness2(this._population[index], this._destImg, Color.Black);
                //_fittness[index] = FitnessCalculator.GetDrawingFitnessSoftware(this._population[index], this._destCanvas, Color.Black);
                //fittness[index] = FitnessCalculator.GetDrawingFitnessSoftwareNative(this._population[index], this._destImg, this._destImgByte, Color.Black);
                //fittness[index] = FitnessCalculator.GetDrawingFitnessWPF(this._population[index], this._destCanvas, Color.Black);    
            }
        }

        private int Helper_DnaCountPoints(byte [] dna)
        {
            int countPoints = 0;

            int index = 0;
            while (index < dna.Length)
            {
                if (//(dna[index] & 3) < 1 &&
                    //primitives.Count < Settings.ActivePolygonsMax &&
                    index + 4 + 6 + 1 <= dna.Length)
                {
                    countPoints += 3;

                    index += 4 + 6 + 1;
                }
                else
                {
                    index++;
                }

            }

            return countPoints;
        }

        private void Helper_UpdateStatsByFittness_CompressDNA()
        {
            int populationLastIndex = this._populationCompressDNA.Length - 1;

            long bestFittness = long.MaxValue;
            long WorstFittness = 0;
            int bestIndex = -1;
            for (int index = 0; index < populationLastIndex; index++)
            {
                if (_fittness[index] > WorstFittness)
                {
                    WorstFittness = this._fittness[index];
                }

                if (this._fittness[index] < bestFittness)
                {
                    bestFittness = this._fittness[index];
                    bestIndex = index;
                }
            }

            if (bestFittness <= this._currentBestFittness)
            {
                this._currentBestFittness = bestFittness;
                this._currentBestCompressDNA = this._populationCompressDNA[bestIndex];
                this._currentBest = Helper_TranslateDNA(this._currentBestCompressDNA);

            }

            this._lastBestCompressDNA = this._populationCompressDNA[bestIndex]; 
            this._lastBestFittness = bestFittness;
            this._lastBest = Helper_TranslateDNA(this._lastBestCompressDNA);

            //ComputeCurrentBestErrorMatrix(this.LastBest);

            // aplikovani pridani nejlepsiho do kolekce
            if (_generation % 1 == 5)
            {
                _fittness[populationLastIndex] = this._currentBestFittness;
                _populationCompressDNA[populationLastIndex] = this._currentBestCompressDNA;
            }
            else
            {
                _fittness[populationLastIndex] = this._lastBestFittness;
                _populationCompressDNA[populationLastIndex] = this._lastBestCompressDNA; 
            }

            _lastWorstFitnessDiff = WorstFittness - this._lastBestFittness;


        }

        private void Helper_GenerateNewPopulation_CompressDNA()
        {
            int maxNormalizeValue = this._fittness.Length * 100000;
            //int [] rouleteTable = RouletteTableNormalize(fittness,maxNormalizeValue);
            //RouletteTableNormalize(this._fittness, this._rouleteTable, maxNormalizeValue);
            //RouletteTableNormalizeBetter(this._fittness, this._rouleteTable, this._diffFittness, maxNormalizeValue);
            RankTableFill2(this._fittness, this._rankTable, out maxNormalizeValue);

            Tools.swap<byte [][]>(ref this._populationCompressDNA,ref this._lastPopulationCompressDNA);
           
            byte currMutatioRate = //(byte)(((this._generation % CONST_DynamicMutationGenInterval) > CONST_DynamicMutationGenInterval / 2) ? 255 : 64);
             GetCurrentMutationRate();

            for (int index = 0; index < _popSize; index++)
            {
                int indexParent1 = Tools.GetRandomNumber(0, maxNormalizeValue + 1);
                indexParent1 = RankVheelParrentIndex(indexParent1, this._rankTable);


                int indexParent2 = indexParent1;

                while (indexParent1 == indexParent2)
                {
                    int tmp = Tools.GetRandomNumber(0, maxNormalizeValue + 1);
                    indexParent2 = RankVheelParrentIndex(tmp, this._rankTable);
                }

                byte []  dna = Helper_CrossoverOnePoint_CompressDNA(this._lastPopulationCompressDNA[indexParent1], this._lastPopulationCompressDNA[indexParent2]);
                //byte []  dna = Helper_CrossoverUniform_CompressDNA(this._lastPopulationCompressDNA[indexParent1], this._lastPopulationCompressDNA[indexParent2]);
               
                if(Tools.GetRandomNumber(0,100) < 10 )
                    Helper_Mutate_CompressDna(ref dna);
                
                this._populationCompressDNA[index] = dna;

            }




        }

        private byte [] Helper_CrossoverOnePoint_CompressDNA(byte [] parent1, byte [] parent2)
        {
            double crossLine = Tools.GetRandomNumber(1,90)*0.01d;
            //double crossLine2 = Tools.GetRandomNumber(1, 9) * 0.1d;

            //double crossLine = 0.3;

            if (Tools.GetRandomNumber(0, 2) == 0)
                Tools.swap<byte[]>(ref parent1, ref parent2);

            int countCrossGenP1 = (int)(parent1.Length * crossLine);
            int countCrossGenP2 = (int)(parent2.Length * (crossLine));
            int newDnaSize = countCrossGenP1 + (parent2.Length - countCrossGenP2);

            byte [] dnaResult = new byte[newDnaSize];

            int dnaIndex = 0;

            int maxIndex = Math.Max(parent1.Length, parent2.Length);

            for (int index = 0; index < countCrossGenP1; index++)
            {
                dnaResult[dnaIndex++] = parent1[index];
            }

            for (int index = countCrossGenP2; index < parent2.Length; index++)
            {
                dnaResult[dnaIndex++] = parent2[index];
            }

            return dnaResult;
        }

        private byte[] Helper_CrossoverUniform_CompressDNA(byte[] parent1, byte[] parent2)
        {
            //double crossLine = Tools.GetRandomNumber(1,9)*0.1d;

            byte [] dnaResult = new byte[parent1.Length];

            for (int i = 0; i < parent1.Length; i++)
            {
                dnaResult[i] = (Tools.GetRandomNumber(0, 2) == 0) ?
                    parent1[i] : parent2[i]; 
            }

            return dnaResult;
        }

        private void Helper_Mutate_CompressDna(ref byte [] dna)
        {
            if (dna.Length > 0)
            {
                int countGenes = Tools.GetRandomNumber(1, 11);

                for (int i = 0; i < countGenes; i++)
                {
                    int index = Tools.GetRandomNumber(0, dna.Length);
                    //dna[index] = (byte)Tools.GetRandomNumber(0, 256);
                    dna[index] = (byte)Tools.GetRandomNumberNoLinear_MinMoreOften(dna[index], 0, 255,64);
                }
            }

            
        }

        private DnaDrawing Helper_TranslateDNA(byte [] dna)
        {
            DnaDrawing result = new DnaDrawing(this._destCanvas.WidthPixel, this._destCanvas.HeightPixel);

            int maxWidht = this._destCanvas.WidthPixel-1;
            int maxHeight = this._destCanvas.HeightPixel-1;


            List<DnaPrimitive> primitives = new List<DnaPrimitive>();
            int index = 0;
            while (index < dna.Length)
            {
                if (//(dna[index]&3) < 1 && 
                    primitives.Count < Settings.ActivePolygonsMax &&
                    index + 4 + 6 + 1 <= dna.Length)
                {
                    DnaPolygon triangle = new DnaPolygon();
                    triangle.Brush.SetByColor(dna[index + 1], dna[index + 2], dna[index + 3], dna[index + 4]);

                    DnaPoint [] points = new DnaPoint[3];

                    int x = (( dna[index + 5]) * maxWidht) / 255;
                    int y = (( dna[index + 6]) * maxHeight) / 255;

                    points[0] = new DnaPoint((short)x, (short)y);

                    x = (( dna[index + 7]) * maxWidht) / 255;
                    y = (( dna[index + 8]) * maxHeight) / 255;

                    points[1] = new DnaPoint((short)x, (short)y);

                    x = (( dna[index + 9]) * maxWidht) / 255;
                    y = (( dna[index + 10]) * maxHeight) / 255;

                    points[2] = new DnaPoint((short)x, (short)y);

                    triangle._Points = points;

                    primitives.Add(triangle);

                    index +=4 + 6 + 1;
                }
                else
                {
                    index++;
                }

            }

            result.Polygons = primitives.ToArray();
            return result;
        }

        #endregion

        //private void MutatePopulation()
        //{
        //    for (int index = 0; index < this._population.Length; index++)
        //    {
        //        while(!this._population[index].IsDirty)
        //            this._population[index].Mutate(this._destCanvas);

        //        this._population[index].SetDirty();
        //    }
        //}

        public void Dispose()
        {

        }


    }
}
