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
    public class GASearch :IDisposable
    {
        private DnaDrawing _currentBest;
        private long _currentBestFittness;
        private DnaDrawing _lastBest;
        private long _lastBestFittness;
        private long _lastWorstFitnessDiff;
        private int _generation = 0;
        private DnaDrawing [] _lastPopulation;
        private DnaDrawing [] _population;

        private CanvasBGRA _destCanvas = new CanvasBGRA(1,1);

        private NativeFunctions _nativeFunc = new NativeFunctions();

        private ImageEdges _edgePoints = null;
        private ErrorMatrix _errorMatrix = new ErrorMatrix(1,1);

        private int [] _rouleteTable = new int[0];
        private long [] _diffFittness = new long[0];
        private long [] _fittness = new long[0];
        private float [] _similarity = new float[0];

        public int CONST_DynamicMutationGenInterval = 1000;
        private const int CONST_MutationMaxRate = 255; // 255 means big mutation changes


        private int _popSize=  1;

        #region CanvasForRender

        DNARenderer _dnaRender = new DNARenderer(1,1); 

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

        #endregion

        public ErrorMatrix ErrorMatrixCurrentClone() { return _errorMatrix.Clone(); }

        public GASearch(int popSize)
        {
            //if (popSize < 2)
            //    popSize = 2;

             _population = new DnaDrawing[popSize+1];
             _lastPopulation = new DnaDrawing[popSize+1];
             _rouleteTable = new int[popSize+1];
             _diffFittness = new long[popSize+1];
             _fittness = new long[popSize+1];
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
            this._generation = 0;
            this._destCanvas = CanvasBGRA.CreateCanvasFromBitmap(destImg);
            this._destCanvas.ReduceNoiseMedian();
            CanvasBGRA.CreateBitmpaFromCanvas(this._destCanvas).Save("ImageMedianNoise.bmp");

            this._currentBestFittness = long.MaxValue;
            this._lastBestFittness = long.MaxValue;

            _errorMatrix = new ErrorMatrix(this._destCanvas.WidthPixel, this._destCanvas.HeightPixel);

            _dnaRender = new DNARenderer(_destCanvas.WidthPixel, _destCanvas.HeightPixel);

            this._edgePoints = CreateEdges(this._destCanvas, EdgeTreshold);
            //this._destCanvas.EasyColorReduction();

            DnaBrush backGround = ComputeBackgroundColor(this._destCanvas);

            for (int i =0; i < this._population.Length; i++)
            {
                DnaDrawing dna = new DnaDrawing(this._destCanvas.WidthPixel, this._destCanvas.HeightPixel);
                //dna.BackGround.InitRandomWithoutAlpha();
                dna.BackGround = backGround;

                for (int k =0; k < 10; k++)
                {
                    //dna.AddRectangle(null, this._destCanvas, this._edgePoints);
                    dna.AddPolygon(255, null, this._destCanvas, this._edgePoints);

                }

                this._population[i] = dna;
            }

            this._lastBest = this._population[_population.Length-1].Clone();
            this._currentBest = this._lastBest.Clone();

            // nezbytne aby doslo k vypoctu novych fittness
            ComputeFittness();
        }

        private static DnaBrush ComputeBackgroundColor(CanvasBGRA image)
        {
            Median8bit mr = new Median8bit();
            Median8bit mg = new Median8bit();
            Median8bit mb = new Median8bit();

            for (int index = 0; index < image.Data.Length; index += 4)
            {
                mb.InsertData(image.Data[index]);
                mg.InsertData(image.Data[index+1]);
                mr.InsertData(image.Data[index+2]);
            }

            return new DnaBrush(255, (int)mr.Median, (int)mg.Median, (int)mb.Median);

        }


        public void ExecuteGeneration()
        {
            GenerateNewPopulationByMutation();

            ComputeFittness();

            UpdateStatsByFittness();

            //GenerateNewPopulationBasic();
            //GenerateNewPopulationRoulete();
            

            //MutatePopulation();

            this._generation++;
        }

        private void ComputeFittness()
        {
            for (int index = 0; index < this._population.Length-1; index++)
            {
                _dnaRender.RenderDNA(this._population[index], DNARenderer.RenderType.SoftwareTriangle);

                //long fittness = FitnessCalculator.ComputeFittness_Basic(_destCanvas.Data, _dnaRender.Canvas.Data);
                //long fittness = FitnessCalculator.ComputeFittness_BasicAdvance(_destCanvas.Data, _dnaRender.Canvas.Data);
                //long fittness = _nativeFunc.ComputeFittnessAdvance(_destCanvas.Data, _dnaRender.Canvas.Data);
                long fittness = _nativeFunc.ComputeFittness(_destCanvas.Data, _dnaRender.Canvas.Data);

                long bloat = this._population[index].Polygons.Length;

                _fittness[index] = fittness + bloat;

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
            for (int index = 0; index < this._population.Length-1; index++)
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

            if (bestFittness < this._currentBestFittness)
            {
                this._currentBestFittness = bestFittness;
                this._currentBest = this._population[bestIndex];

                ComputeCurrentBestErrorMatrix();
            }

            this._lastBest = this._population[bestIndex];
            this._lastBestFittness = bestFittness;

            // aplikovani pridani nejlepsiho do kolekce
            if (_generation % 1 == 0) 
            {
                _fittness[this._population.Length - 1] = this._currentBestFittness;
                _population[this._population.Length - 1] = this._currentBest;
            }
            else
            {
                _fittness[this._population.Length - 1] = this._lastBestFittness;
                _population[this._population.Length - 1] = this._lastBest;
            }

            _lastWorstFitnessDiff = WorstFittness - this._lastBestFittness;

            
        }

        private void ComputeCurrentBestErrorMatrix()
        {
            _dnaRender.RenderDNA(this._currentBest, DNARenderer.RenderType.SoftwareTriangle);

            _errorMatrix.ComputeErrorMatrix(this._destCanvas, _dnaRender.Canvas);
      
        }

        private void ComputeCurrentBestErrorMatrix(DnaDrawing dna)
        {
            _dnaRender.RenderDNA(dna, DNARenderer.RenderType.SoftwareTriangle);

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

                _similarity[index] = countSame / ((float)polygons.Length+ 0.1f*(maxLength-polygons.Length));
            }

            //if (allIds.Count != theSameIds.Count)
            //{
            //    int i = 0;
            //    allIds.Clear();
            //}
        }


        private byte GetCurrentMutationRate()
        {
            int positionInMutationRate =  CONST_DynamicMutationGenInterval-1 - ( this._generation % CONST_DynamicMutationGenInterval);

            byte mutatioRate =  (byte)((positionInMutationRate * CONST_MutationMaxRate)/ CONST_DynamicMutationGenInterval);

            return mutatioRate;
        }

        private void GenerateNewPopulationBasic()
        {
            DnaDrawing [] newPopulation = new DnaDrawing[_popSize+1];

            newPopulation[this._population.Length] = this._currentBest.Clone();

            for (int index = 1; index < this._population.Length; index++)
            {
                int indexParent1 = Tools.GetRandomNumber(0, this._population.Length);
                int indexParent2 = indexParent1;

                while(indexParent1 == indexParent2)
                    indexParent2 = Tools.GetRandomNumber(0, this._population.Length);

                newPopulation[index] = CrossoverBasic(this._population[indexParent1],this._population[indexParent2]);
            }

            this._population = newPopulation;
        }

        private void GenerateNewPopulationByMutation()
        {
            int maxNormalizeValue = this._fittness.Length * 100000;
            //int [] rouleteTable = RouletteTableNormalize(fittness,maxNormalizeValue);
            ComputeSimilarity();
            //RouletteTableNormalizeBetter(this._fittness, this._rouleteTable, this._diffFittness,  maxNormalizeValue);
            //RouletteTableNormalizeBetterWithSimilarity2(this._fittness, this._rouleteTable, this._diffFittness, this._similarity, maxNormalizeValue);

            DnaDrawing [] tmpPolulation = this._population;
            this._population = this._lastPopulation;
            this._lastPopulation = tmpPolulation;

            byte currMutatioRate = GetCurrentMutationRate();

            for (int index = 0; index < _popSize; index++)
            {
                int indexParent1 = Tools.GetRandomNumber(0, maxNormalizeValue + 1);
                 indexParent1 = RouletteVheelParrentIndex(indexParent1, this._rouleteTable);

                

                int tmpindexParent1 = Tools.GetRandomNumber(0, this._fittness.Length);
                indexParent1 = tmpindexParent1;
                int tmpindexParent2 = Tools.GetRandomNumber(0, this._fittness.Length);
                //if (this._fittness[tmpindexParent1]/((1.0-this._similarity[tmpindexParent1]+1.0)) > 
                //    this._fittness[tmpindexParent2]/((1.0-this._similarity[tmpindexParent2]+1.0)) )
                //    indexParent1 = tmpindexParent2;

                if (this._fittness[tmpindexParent1]  >
                   this._fittness[tmpindexParent2] )
                    indexParent1 = tmpindexParent2;

                //tmpindexParent2 = Tools.GetRandomNumber(0, this._fittness.Length);

                //if (this._fittness[tmpindexParent1] >
                //  this._fittness[tmpindexParent2])
                //    indexParent1 = tmpindexParent2;

                DnaDrawing dna = this._lastPopulation[indexParent1].Clone();
                //ComputeCurrentBestErrorMatrix(dna);
                while (!dna.IsDirty)
                    dna.MutateBetter(currMutatioRate, this._errorMatrix, this._destCanvas,
                        null//_edgePoints
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
            RouletteTableNormalizeBetter(this._fittness, this._rouleteTable, this._diffFittness,  maxNormalizeValue);

            DnaDrawing [] tmpPolulation = this._population;
            this._population = this._lastPopulation;
            this._lastPopulation = tmpPolulation;

           
            for (int index = 0; index < _popSize; index++)
            {
                int indexParent1 = Tools.GetRandomNumber(0, maxNormalizeValue+1);
                indexParent1 = RouletteVheelParrentIndex(indexParent1, this._rouleteTable);


                int indexParent2 = indexParent1;

                while (indexParent1 == indexParent2)
                    indexParent2 = RouletteVheelParrentIndex(Tools.GetRandomNumber(0, maxNormalizeValue+1), this._rouleteTable);

                //newPopulation[index] = CrossoverBasic(this._population[indexParent1], this._population[indexParent2]);

                DnaDrawing  dna = CrossoverOnePoint(this._lastPopulation[indexParent1], this._lastPopulation[indexParent2]);

                while (!dna.IsDirty)
                    dna.MutateBetter(0,this._errorMatrix,this._destCanvas, _edgePoints);

                this._population[index] = dna;
            }

           

            
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

        private static void RouletteTableNormalizeBetter(long[] fittness,int [] rouleteTable, long []diffFittness, int maxNormalizeValue)
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
            (long[] fittness, int[] rouleteTable, long[] diffFittness, float [] similarity, int maxNormalizeValue)
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

                float koef = (1.0f - similarity[index])*1.5f + 1.0f;
                long diffFit = (long)(((fittnessMax - fittness[index]) + minDiffFit) * koef) ;
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

        private int RouletteVheelParrentIndex(int value, int [] rouletteTable)
        {
            for (int index = 0; index < rouletteTable.Length; index++)
            {
                if (rouletteTable[index] > value)
                    return index;
            }

            return rouletteTable.Length - 1;
        }

        private DnaDrawing CrossoverBasic(DnaDrawing parent1, DnaDrawing parent2)
        {
            DnaDrawing result = new DnaDrawing(this._destCanvas.WidthPixel, this._destCanvas.HeightPixel);

            List<DnaPrimitive> polygons = new List<DnaPrimitive>();
            int maxIndex = Math.Max(parent1.Polygons.Length,parent2.Polygons.Length);
            for (int index = 0; index < maxIndex; index++)
            {
                DnaDrawing tmp = (Tools.GetRandomNumber(1,1000) > 500)? parent1 : parent2;

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
            int newDnaSize = countCrossGenP1 + (parent2.Polygons.Length- countCrossGenP2*1);

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
