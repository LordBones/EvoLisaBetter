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
using GenArt.Core.Classes.Misc;

namespace GenArt.Core.AST
{
    public class GASearch : IDisposable
    {
        public enum TypeRendering { software, softwareByRow, softwareByRowWithFitness, softwareByRowWithFitnessParallel };

        private const int CONST_CompressDNA_GenSize = 4 + 6 + 1;

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

        private ObjectRecyclerTS<DnaPolygon> _RecyclePool_Polygon = new ObjectRecyclerTS<DnaPolygon>(100);
        private ObjectRecyclerTS<DnaRectangle> _RecyclePool_Rectangle = new ObjectRecyclerTS<DnaRectangle>(100);



        private CanvasARGB _destCanvas = new CanvasARGB(1, 1);
        private AreaSizeVO<short> _canvasArea;

        private NativeFunctions _nativeFunc = new NativeFunctions();

        private ImageEdges _edgePoints = null;
        private ErrorMatrix _errorMatrix = new ErrorMatrix(1, 1);

       
        private int [] _rankTable = new int[0];
        private Tuple<int, long>[] _lookupFittness_RankTable = new Tuple<int, long>[0];
        private Tuple<int, long>[] _lookupFittness_PopSize = new Tuple<int, long>[0];
        private int _maxRankNormalizeValue = 0;

        private long [] _fittness = new long[0];
       

        public int CONST_DynamicMutationGenInterval = 256;
        private const byte CONST_MutationMaxRate = 2; // 255 means big mutation changes

        private byte _crLastMutationRate = 255;
        private int _crLastGenerationNumber = 0;
        private long _crLastBestFittness = 0;

        private int _popSize=  1;
        private int _countElite = 2;

        private TypeRendering _typeRendering = TypeRendering.software;
        private DnaBrush _background;

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

            int elitismAndBestCount = 1 + this._countElite;

            _population = new DnaDrawing[popSize + elitismAndBestCount];
            _lastPopulation = new DnaDrawing[popSize + elitismAndBestCount];

            _populationCompressDNA = new byte [popSize + elitismAndBestCount][];
            _lastPopulationCompressDNA = new byte [popSize + elitismAndBestCount][];


           
            _rankTable = new int[popSize + elitismAndBestCount];
            _fittness = new long[popSize + elitismAndBestCount];
            _popSize = popSize;

            _lookupFittness_PopSize = new Tuple<int, long>[_popSize];
            _lookupFittness_RankTable = new Tuple<int, long>[_rankTable.Length];




        }

        private static ImageEdges CreateEdges(CanvasARGB destImg, int EdgeThreshold)
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
            this._destCanvas = CanvasARGB.CreateCanvasFromBitmap(destImg);
            this._canvasArea = new AreaSizeVO<short>(_destCanvas.WidthPixel, _destCanvas.HeightPixel);
            //this._destCanvas.ReduceNoiseMedian();
            //this._destCanvas.ReduceNoiseMedian(true);

            //CanvasARGB.CreateBitmpaFromCanvas(this._destCanvas).Save("ImageMedianNoise.bmp");


            this._currentBestFittness = long.MaxValue;
            this._lastBestFittness = long.MaxValue;

            _errorMatrix = new ErrorMatrix(this._destCanvas.WidthPixel, this._destCanvas.HeightPixel);

            _dnaRender = new DNARenderer(_destCanvas.WidthPixel, _destCanvas.HeightPixel);
            _dnaRender.DestCanvas = this._destCanvas;
            

            this._edgePoints = CreateEdges(this._destCanvas, EdgeTreshold);
            //this._destCanvas.EasyColorReduction();

            //DnaBrush backGround = new DnaBrush(255, 0, 0, 0);

            DnaBrush backGround = ComputeBackgroundColor(this._destCanvas);
            _background = backGround;

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

            this._lastBest = new DnaDrawing(this._destCanvas.WidthPixel, this._destCanvas.HeightPixel);
            this._currentBest = this._lastBest.Clone();

            for(int i = this._popSize;i < this._population.Length;i++)
                this._fittness[i] = long.MaxValue;

           
                ComputeFittness();


            _crLastMutationRate = 255;
            _crLastGenerationNumber = 0;
            _crLastBestFittness = long.MaxValue/2;

            // compress version dna
            Helper_InitPoluplation_CompressDNA();


            RankTableFill(this._rankTable, 10000, 1.5, out _maxRankNormalizeValue);

        }

        private static DnaBrush ComputeBackgroundColor(CanvasARGB image)
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
                
                long fittness = 0;
                if (_typeRendering == TypeRendering.softwareByRowWithFitness)
                    fittness = _dnaRender.Fittness;
                else
                    //fittness = _nativeFunc.ComputeFittnessABSSSE_ARGB(_destCanvas.Data, _dnaRender.Canvas.Data);
                //fittness = FitnessCalculator.ComputeFittnessLine_SumABS(_destCanvas.Data, _dnaRender.Canvas.Data);
                fittness = FitnessCalculator.ComputeFittnessLine_SumABSPerRow(_destCanvas.Data, _dnaRender.Canvas.Data,_destCanvas.WidthPixel);

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
            RankTableFill(this._rankTable, 10000, 2.0, out maxNormalizeValue);
            
            Tools.swap<DnaDrawing[]>(ref this._population, ref this._lastPopulation);
            
            this._population[this._population.Length - 1] = null;

            byte currMutatioRate = //(byte)(((this._generation % CONST_DynamicMutationGenInterval) > CONST_DynamicMutationGenInterval / 2) ? 255 : 64);
             GetCurrentMutationRate();

            List<Tuple<int, long>> lookupFittnes = new List<Tuple<int, long>>(_rankTable.Length);
            for (int i = 0; i < _rankTable.Length; i++)
            {
                lookupFittnes.Add(new Tuple<int, long>(i, this._fittness[i]));
            }

            lookupFittnes = lookupFittnes.OrderByDescending(x => x.Item2).ToList();


            for (int index = 0; index < _popSize; index++)
            {
                //int indexParent1 = Tools.GetRandomNumber(0, maxNormalizeValue + 1);
                //indexParent1 = RouletteVheelParrentIndex(indexParent1, this._rouleteTable);

                int indexParent1 = Tools.GetRandomNumber(0, maxNormalizeValue + 1);
                indexParent1 = RankVheelParrentIndex(indexParent1, this._rankTable);
                
                //int indexParent1 = this._fittness.Length - 1;




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

                DnaDrawing dna = this._lastPopulation[lookupFittnes[indexParent1].Item1].Clone();
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
            RankTableFill(this._rankTable, 10000,2.0, out maxNormalizeValue);
           
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

       

        /// <summary>
        /// // implementace vzorce
        // 2-sp+(2*(sp-1)*((pos-1)/(n-1)  
        // pos = 1 nejmensi fittness, sp = <1.0,2.0>  1.0 - linearni

        /// </summary>
        /// <param name="rankTable"></param>
        /// <param name="rankBaseValue"></param>
        /// <param name="sp"></param>
        /// <param name="MaxValueRankTable"></param>
        private static void RankTableFill( int[] rankTable, int rankBaseValue, double sp, out int MaxValueRankTable)
        {
            
            // implementace vzorce
            // 2-sp+(2*(sp-1)*((pos-1)/(n-1)  
            // pos = 1 nejmensi fittness, sp = <1.0,2.0>  1.0 - linearni

            int lastValue = 0;
            for (int i = 0; i < rankTable.Length; i++)
            {
                double currentRank = 2.0 - sp + (2 * (sp - 1.0) * ((i) / ((double)rankTable.Length - 1)));

                rankTable[i] = lastValue + (int)(currentRank * rankBaseValue);
                lastValue = rankTable[i];
            }


            MaxValueRankTable = rankTable[rankTable.Length - 1] + rankBaseValue;
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
            else if (_typeRendering == GASearch.TypeRendering.softwareByRowWithFitnessParallel) renderType = DNARenderer.RenderType.SoftwareByRowsWithFittnessParallel;


            for (int index = 0; index < this._popSize; index++)
            {
                DnaDrawing dDrawing= Helper_TranslateDNA(this._populationCompressDNA[index]);
                int countPoint = dDrawing.PointCount;
                

                
                
                //_dnaRender.RenderDNA_CompressDNA(this._populationCompressDNA[index], renderType);

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
                if (_typeRendering == TypeRendering.softwareByRowWithFitness || _typeRendering == TypeRendering.softwareByRowWithFitnessParallel)
                {
                    _dnaRender.RenderDNA(dDrawing, renderType);
                    fittness = _dnaRender.Fittness;
                }
                else
                {
                    _dnaRender.RenderDNA(dDrawing, renderType);
                    //fittness = FitnessCalculator.ComputeFittnessLine_OnlyCorrectHit(_destCanvas.Data, _dnaRender.Canvas.Data);
                    fittness = _nativeFunc.ComputeFittnessSquareSSE_ARGB(_destCanvas.Data, _dnaRender.Canvas.Data);
                    //fittness = _nativeFunc.ComputeFittnessAdvance_ARGB(_destCanvas.Data, _dnaRender.Canvas.Data);

                    //fittness = _nativeFunc.ComputeFittnessABSSSE_ARGB(_destCanvas.Data, _dnaRender.Canvas.Data);
                    //fittness = FitnessCalculator.ComputeFittnessLine_SumSquare(_destCanvas.Data, _dnaRender.Canvas.Data);
                }

                for (int i = 0; i < dDrawing.Polygons.Length; i++)
                {
                    DnaPolygon pd = dDrawing.Polygons[i] as DnaPolygon;
                    if (pd != null)
                    {
                        _RecyclePool_Polygon.PutForRecycle(pd);
                        continue;
                    }

                    DnaRectangle dr = dDrawing.Polygons[i] as DnaRectangle;
                    if (dr != null)
                        _RecyclePool_Rectangle.PutForRecycle(dr);
                }

                fittness = fittness;//  (long)(Math.Sqrt(fittness)*100.0);

                //double areaSize = Helper_DnaSumAreaTriangles(this._populationCompressDNA[index]) /(double) this._destCanvas.CountPixels;

                //if (areaSize > 2.0) fittness = (long)(fittness * (areaSize-1));// ((long)(fittness*(1.0+(areaSize-1.0)*0.5));

                long bloat = countPoint;
                if (bloat == 0) bloat = 1;

                double kkkk = (bloat * 0.01 + 1);

                _fittness[index] = (long)(fittness + bloat );
                    //+ bloat*bloat)  ;

                //fittness[index] = FitnessCalculator.GetDrawingFitness2(this._population[index], this._destImg, Color.Black);
                //_fittness[index] = FitnessCalculator.GetDrawingFitnessSoftware(this._population[index], this._destCanvas, Color.Black);
                //fittness[index] = FitnessCalculator.GetDrawingFitnessSoftwareNative(this._population[index], this._destImg, this._destImgByte, Color.Black);
                //fittness[index] = FitnessCalculator.GetDrawingFitnessWPF(this._population[index], this._destCanvas, Color.Black);    
            }
        }

       

        private long Helper_DnaSumAreaTriangles(byte[] dna)
        {
            if (dna.Length == 0) return 0;
            if (Settings.ActivePolygonsMax < dna[0])  return Settings.ActivePolygonsMax;


            long sum = 0;
            int countTriangles = 0;
            int index = 1;

            int maxWidth = this._destCanvas.WidthPixel - 1;
            int maxHeight = this._destCanvas.HeightPixel - 1;

            while (index+CONST_CompressDNA_GenSize < dna.Length && countTriangles < Settings.ActivePolygonsMax )
            {
                int x = ((dna[index + 5]) * maxWidth) / 255;
                int y = ((dna[index + 6]) * maxHeight) / 255;

                int x2 = ((dna[index + 7]) * maxWidth) / 255;
                int y2 = ((dna[index + 8]) * maxHeight) / 255;

                int x3 = ((dna[index + 9]) * maxWidth) / 255;
                int y3 = ((dna[index + 10]) * maxHeight) / 255;


                sum += (int) GraphicFunctions.TriangleAreaSize(x, y, x2, y2, x3, y3);
                
                countTriangles++;
                index += CONST_CompressDNA_GenSize;
            }

            return sum;
        }




        private void Helper_UpdateStatsByFittness_CompressDNA()
        {
            int populationLastIndex = this._populationCompressDNA.Length - 1;

            long bestFittness = long.MaxValue;
            long WorstFittness = 0;
            int bestIndex = -1;
            for (int index = 0; index < _popSize; index++)
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

            // pridani nejlepsiho do kolekce
            _fittness[_popSize] = this._currentBestFittness;
            _populationCompressDNA[_popSize] = this._currentBestCompressDNA;

            

            _lastWorstFitnessDiff = WorstFittness - this._lastBestFittness;


        }

        private void Helper_GenerateNewPopulation_CompressDNA()
        {

            // add elites
            Tuple<int, long>[] lookupFittnes2 = _lookupFittness_PopSize;
            for (int i = 0; i < _popSize; i++)
            {
                lookupFittnes2[i] = new Tuple<int, long>(i, this._fittness[i]);
            }

            lookupFittnes2 = lookupFittnes2.OrderByDescending(x => x.Item2).ToArray();
            int startIndex = _popSize + 1;
            for (int i = 0; i < this._countElite; i++)
            {
                _lastPopulationCompressDNA[startIndex + i] = _populationCompressDNA[lookupFittnes2[i].Item1];
                _fittness[startIndex + i] = _fittness[lookupFittnes2[i].Item1];
            }

            Tools.swap<byte [][]>(ref this._populationCompressDNA,ref this._lastPopulationCompressDNA);

            byte currMutatioRate = 4;//(byte)(((this._generation % CONST_DynamicMutationGenInterval) > CONST_DynamicMutationGenInterval / 2) ? 255 : 64);
             //GetCurrentMutationRate();

           Tuple<int, long>[] lookupFittnes = _lookupFittness_RankTable;
            for(int i =0;i<_rankTable.Length;i++)  
            {
                lookupFittnes[i] =new Tuple<int, long>(i, this._fittness[i]);
            }

            lookupFittnes = lookupFittnes.OrderByDescending(x => x.Item2).ToArray();

            for (int index = 0; index < _popSize; index++)
            {
                int indexParent1 = Tools.GetRandomNumber(0, _maxRankNormalizeValue + 1);
                indexParent1 = RankVheelParrentIndex(indexParent1, this._rankTable);
                //indexParent1 = lookupFittnes[lookupFittnes.Count()-2].Item1;
                 
                DnaDrawing dDrawing = Helper_TranslateDNA(this._lastPopulationCompressDNA[lookupFittnes[indexParent1].Item1]);


                /* byte[] dna;
                 //indexParent1 = 0;
                 if (Tools.GetRandomNumber(0, 2)==0)
                 {
                     //dna = Helper_CrossoverOnePoint_CompressDNA3(this._lastPopulationCompressDNA[lookupFittnes[indexParent1].Item1], this._lastPopulationCompressDNA[lookupFittnes[indexParent2].Item1]);
                     dna = Helper_CrossoverOnePoint_CompressDNA3(this._lastPopulationCompressDNA[lookupFittnes[indexParent1].Item1], this._lastPopulationCompressDNA[lookupFittnes[indexParent1].Item1]);
                     //dna = Helper_CrossoverOnePoint_CompressDNA3(dna, dna);
                     //byte []  dna = Helper_CrossoverUniform_CompressDNA(this._lastPopulationCompressDNA[indexParent1], this._lastPopulationCompressDNA[indexParent2]);
                 }
                 else
                 {
                     int count = this._lastPopulationCompressDNA[lookupFittnes[indexParent1].Item1][0] * CONST_CompressDNA_GenSize + 1;
                     dna = new byte[this._lastPopulationCompressDNA[lookupFittnes[indexParent1].Item1].Length];
                     Buffer.BlockCopy(this._lastPopulationCompressDNA[lookupFittnes[indexParent1].Item1], 0, dna, 0, count);

                 }
                 */

                if (dDrawing.Polygons.Length > 1)
                {

                    int inde = Tools.GetRandomNumber(0, dDrawing.Polygons.Length - 1);

                    int inde2 = inde + 1;

                    //inde2 = Helper_GetNearestInterleaveIndex(inde, dDrawing.Polygons);

                    DnaPrimitive dp = dDrawing.Polygons[inde];
                    
                    DnaPrimitive dp2 = dDrawing.Polygons[inde2];
                    dDrawing.Polygons[inde] = dp2;
                    dDrawing.Polygons[inde2] = dp;


                }



                if (Tools.GetRandomNumber(0, 2) == 0)
                {
                    dDrawing.MutateNew(currMutatioRate, this._destCanvas,
                        _edgePoints);
                    //null);
                }
                else
                {
                    dDrawing.MutateNew(currMutatioRate, this._destCanvas,
                    null);
                }

                //if (Tools.GetRandomNumber(0, 3) == 0)
                //{
                //    Helper_NoisyMutate(dDrawing, this._canvasArea, 10);
                //}

                this._populationCompressDNA[index] = Helper_EncodeToDna(dDrawing);

               // Helper_Mutate_CompressDna_old(this._populationCompressDNA[index]);

                for (int i = 0; i < dDrawing.Polygons.Length; i++)
                {
                    DnaPolygon pd = dDrawing.Polygons[i] as DnaPolygon;
                    if (pd != null)
                    {
                        _RecyclePool_Polygon.PutForRecycle(pd);
                        continue;
                    }

                    DnaRectangle dr = dDrawing.Polygons[i] as DnaRectangle;
                    if (dr != null)
                        _RecyclePool_Rectangle.PutForRecycle(dr);
                }
            }




        }

        private static void Helper_NoisyMutate(DnaDrawing dna, AreaSizeVO<short> area, short noiseSize)
        {
            for(int i = 0;i < dna.Polygons.Length;i++)
            {

               //if( Tools.GetRandomNumber(0,5) == 0) continue;

                DnaRectangle rect = dna.Polygons[i] as DnaRectangle;
                if(rect != null)
                {
                    DnaPoint tmp;
                    tmp = rect.StartPoint;
                    Tools.MutatePointByRadial(ref tmp.X, ref tmp.Y, area,255,noiseSize);
                    rect.StartPoint = tmp;

                    tmp = rect.EndPoint;
                    Tools.MutatePointByRadial(ref tmp.X, ref tmp.Y, area, 255, noiseSize);
                    rect.EndPoint = tmp;

                    rect.RepairOrderAxis();
                }

                DnaPolygon poly = dna.Polygons[i] as DnaPolygon;
                if (poly != null)
                {
                    for(int p =0; p < poly.Points.Length;p++)
                    {
                        DnaPoint tmp;
                        tmp = poly.Points[p];
                        Tools.MutatePointByRadial(ref tmp.X, ref tmp.Y, area, 255, noiseSize);
                        poly.Points[p] = tmp;
                    }
                }
            }
        }

        private static int Helper_GetNearestInterleaveIndex(int index, DnaPrimitive [] prims)
        {
            int indexBestUp = index ;
            for (int i = indexBestUp+1; i < prims.Length; i++)
            {
                if (DnaDrawing.IsPrimitiveInterleaving(prims[index], prims[i]))
                {
                    indexBestUp = i;
                    break;
                }
            }
            
            int indexBestDown = index ;
            for (int i = indexBestDown-1; i > 0; i--)
            {
                if (DnaDrawing.IsPrimitiveInterleaving(prims[index], prims[i]))
                {
                    indexBestDown = i;
                    break;
                }
            }

            if(indexBestUp == index && indexBestDown == index)
            {
                return index;
            }
            else if(indexBestUp != index && indexBestDown == index)
            {
                return indexBestUp;
            }
            else if (indexBestUp == index && indexBestDown != index)
            {
                return indexBestDown;
            }
            else
            {
                if(indexBestUp-index > index - indexBestDown)
                {
                    return indexBestDown;
                }
                else
                {
                    return indexBestUp;
                }
            }

        }

        private byte [] Helper_CrossoverOnePoint_CompressDNA(byte [] parent1, byte [] parent2)
        {
            double crossLine = Tools.GetRandomNumber(1,900)*0.001d;
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

        private byte[] Helper_CrossoverOnePoint_CompressDNA2(byte[] parent1, byte[] parent2)
        {
            if (parent1.Length != parent2.Length) throw new NotSupportedException();

            int crossLine = Tools.GetRandomNumber(1, parent1.Length);
            
            if (Tools.GetRandomNumber(0, 2) == 0)
                Tools.swap<byte[]>(ref parent1, ref parent2);

            
            
            byte[] dnaResult = new byte[parent1.Length];
            dnaResult[0] = parent1[0];
            int length = parent1.Length;
            

            Buffer.BlockCopy(parent1, 1, dnaResult, length - crossLine, crossLine);

            Buffer.BlockCopy(parent2, crossLine, dnaResult, 1, length - crossLine);

            return dnaResult;
        }

        /// <summary>
        /// special
        /// </summary>
        /// <param name="parent1"></param>
        /// <param name="parent2"></param>
        /// <returns></returns>
        private byte[] Helper_CrossoverOnePoint_CompressDNA3(byte[] parent1, byte[] parent2)
        {
            if (parent1.Length != parent2.Length) throw new NotSupportedException();

            int p1Len = parent1[0] * CONST_CompressDNA_GenSize ;
            int p2Len = parent2[0] * CONST_CompressDNA_GenSize ;

            if (p1Len < p2Len)
            {
                Tools.swap<byte[]>(ref parent1, ref parent2);
                Tools.swap<int>(ref p1Len, ref p2Len);
            }





            int crossLine = 0;
            if(parent2[0] != 0)
                crossLine=   Tools.GetRandomNumber(0, parent2[0])*CONST_CompressDNA_GenSize;



            byte[] dnaResult = new byte[parent1.Length];
            dnaResult[0] = parent1[0];


            int length = parent1.Length;

            int p2partCount = (p2Len - crossLine);

            Buffer.BlockCopy(parent2,1+ crossLine, dnaResult, 1 , p2partCount);
               
            Buffer.BlockCopy(parent1, 1, dnaResult, 1+p2partCount, p1Len - (p2partCount));

            

            return dnaResult;
        }


        

        private void Helper_Mutate_CompressDna_old(byte [] dna)
        {
            double probability = 0.01;

            double gens = dna.Length * probability;

            int countGens = 0;

            while(gens > 0.0)
            {
                if(gens>=1.0)
                {
                    countGens += 1;
                    gens -= 1.0;
                }
                else
                {
                    if( Tools.GetRandomNumberDouble() < gens)
                    {
                        countGens++;
                        
                    }

                    break;
                }
            }

            if (dna.Length > 0)
            {


                int countGenes = countGens;// Tools.GetRandomNumber(1, 11);

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
            result.BackGround = _background;

            int maxWidth = this._destCanvas.WidthPixel-1;
            int maxHeight = this._destCanvas.HeightPixel-1;

            List<DnaPrimitive> primitives = new List<DnaPrimitive>();

            if (dna.Length > 0)
            {
                
                int index = 0;

                while (index < dna.Length)
                {
                    if (primitives.Count >= Settings.ActivePolygonsMax) break;

                    while (index < dna.Length && (dna[index]&3) > 1) index++;

                    if (index >= dna.Length) break;

                    byte typeElement = (byte)(dna[index]&3);
                    if (typeElement == 0) // triangle
                    {
                        if (index + 16 < dna.Length)
                        {
                            DnaPolygon newPoly = _RecyclePool_Polygon.GetNewOrRecycle();
                            newPoly.Brush.SetByColor(dna[index + 1], dna[index + 2], dna[index + 3], dna[index + 4]);

                            index += 5;

                            DnaPoint[] points = new DnaPoint[3];

                            short x = (short)(ReadShort(dna, index)% maxWidth);
                            x = (short)Math.Max(Math.Min(x, maxWidth), 0);
                            short y = (short)(ReadShort(dna, index+2)%maxHeight);
                            y = (short)Math.Max(Math.Min(y, maxHeight), 0);
                            points[0] = new DnaPoint(x,y);
                            index += 4;

                             x = (short)(ReadShort(dna, index) % maxWidth);
                            x = (short)Math.Max(Math.Min(x, maxWidth), 0);
                             y = (short)(ReadShort(dna, index + 2) % maxHeight);
                            y = (short)Math.Max(Math.Min(y, maxHeight), 0);
                            points[1] = new DnaPoint(x, y);
                            index += 4;

                             x = (short)(ReadShort(dna, index) % maxWidth);
                            x = (short)Math.Max(Math.Min(x, maxWidth), 0);
                             y = (short)(ReadShort(dna, index + 2) % maxHeight);
                            y = (short)Math.Max(Math.Min(y, maxHeight), 0);
                            points[2] = new DnaPoint(x, y);
                            index += 4;
                            
                            newPoly._Points = points;
                            primitives.Add(newPoly);
                        }
                        else
                        {
                            break; 
                        }
                    }
                    else if (typeElement == 1)
                    {
                        if (index + 12 < dna.Length)
                        {

                            DnaRectangle newRec = _RecyclePool_Rectangle.GetNewOrRecycle();
                            newRec.Brush.SetByColor(dna[index + 1], dna[index + 2], dna[index + 3], dna[index + 4]);

                            index += 5;

                            short x = (short)(ReadShort(dna, index) % maxWidth);
                            x = (short)Math.Max(Math.Min(x, maxWidth), 0);
                            short y = (short)(ReadShort(dna, index + 2) % maxHeight);
                            y = (short)Math.Max(Math.Min(y, maxHeight), 0);
                            newRec.StartPoint = new DnaPoint(x, y);
                            index += 4;

                             x = (short)(ReadShort(dna, index) % maxWidth);
                            x = (short)Math.Max(Math.Min(x, maxWidth), 0);
                             y = (short)(ReadShort(dna, index + 2) % maxHeight);
                            y = (short)Math.Max(Math.Min(y, maxHeight), 0);
                            newRec.EndPoint = new DnaPoint(x, y);
                            index += 4;

                            newRec.RepairOrderAxis();

                            primitives.Add(newRec);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

            }

            result.Polygons = primitives.ToArray();
            return result;
        }

        private byte[] Helper_EncodeToDna(DnaDrawing dna )
        {
            List<byte> result = new List<byte>();

            for(int i = 0;i < dna.Polygons.Length;i++)
            {
                DnaPolygon poly = dna.Polygons[i] as DnaPolygon;
                if(poly != null)
                {
                    result.Add(0);
                    result.Add(poly.Brush.Alpha);
                    result.Add(poly.Brush.Red);
                    result.Add(poly.Brush.Green);
                    result.Add(poly.Brush.Blue);

                    SaveShort_Append(result, poly._Points[0].X);
                    SaveShort_Append(result, poly._Points[0].Y);

                    SaveShort_Append(result, poly._Points[1].X);
                    SaveShort_Append(result, poly._Points[1].Y);

                    SaveShort_Append(result, poly._Points[2].X);
                    SaveShort_Append(result, poly._Points[2].Y);
                }

                DnaRectangle rec = dna.Polygons[i] as DnaRectangle;
                if (rec != null)
                {
                    result.Add(1);
                    result.Add(rec.Brush.Alpha);
                    result.Add(rec.Brush.Red);
                    result.Add(rec.Brush.Green);
                    result.Add(rec.Brush.Blue);

                    SaveShort_Append(result, rec.StartPoint.X);
                    SaveShort_Append(result, rec.StartPoint.Y);

                    SaveShort_Append(result, rec.EndPoint.X);
                    SaveShort_Append(result, rec.EndPoint.Y);
                }
            }

            return result.ToArray();
        }

        private static short ReadShort(byte [] data, int index)
        {
            return (short)((data[index] << 8) | data[index + 1]);
        }

        private static void SaveShort(byte[] data, int index,  short input)
        {
            data[index] = (byte)(input >> 8);
            data[index+1] = (byte)(input & 0xff );
        }

        private static void SaveShort_Append(List<byte> data, short input)
        {
            data.Add((byte)(input >> 8));
             data.Add((byte)(input & 0xff));
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
