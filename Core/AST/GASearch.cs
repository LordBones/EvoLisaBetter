﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenArt.AST;
using GenArt.Classes;
using GenArt.Core.Classes;

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


        private ImageEdges _edgePoints = null;

        private int [] _rouleteTable = new int[0];
        private long [] _diffFittness = new long[0];
        private long [] _fittness = new long[0];


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

        public GASearch(int popSize)
        {
            //if (popSize < 2)
            //    popSize = 2;

             _population = new DnaDrawing[popSize];
             _lastPopulation = new DnaDrawing[popSize];
             _rouleteTable = new int[popSize];
             _diffFittness = new long[popSize];
             _fittness = new long[popSize];


        }


        private Color GetColorByPolygonPoints(DnaPoint [] points)
        {
            int sumRed = 0;
            int sumGreen = 0;
            int sumBlue = 0;

            byte [] canvasData = this._destCanvas.Data;

            for (int index = 0; index < points.Length; index++)
            {
                int colorIndex = ((points[index].Y * this._destCanvas.WidthPixel) + points[index].X) << 2;
                sumRed += canvasData[colorIndex];
                sumGreen += canvasData[colorIndex + 1];
                sumBlue += canvasData[colorIndex + 2];
            }

            return Color.FromArgb(255, sumRed / points.Length, sumGreen / points.Length, sumBlue / points.Length);
        }

        private static ImageEdges CreateEdges(CanvasBGRA destImg)
        {
            EdgeDetector ed = new EdgeDetector(destImg);
            ed.DetectEdges();
            
            ed.SaveEdgesAsBitmap("ImageEdges.bmp");
            ed.SaveBitmapHSL("bmpHSL_H.bmp", true, false, false);
            ed.SaveBitmapHSL("bmpHSL_S.bmp", false, true, false);
            ed.SaveBitmapHSL("bmpHSL_L.bmp", false, false, true);

            return ed.GetAllEdgesPoints();

        }

        public void InitFirstPopulation(Bitmap destImg)
        {
            this._generation = 0;
            this._destCanvas = CanvasBGRA.CreateCanvasFromBitmap(destImg);

            this._currentBestFittness = long.MaxValue;

            this._edgePoints = CreateEdges(this._destCanvas);

            this._destCanvas.EasyColorReduction();

           

            for (int i =0; i < this._population.Length; i++)
            {
                DnaDrawing dna = new DnaDrawing();

                for (int k =0; k < 10; k++)
                {
                    dna.AddPolygon(this._destCanvas, this._edgePoints);

                }

                this._population[i] = dna;
            }
        }


        public void ExecuteGeneration()
        {
            ComputeFittness();

            UpdateStatsByFittness();

            //GenerateNewPopulationBasic();
            //GenerateNewPopulationRoulete();
            GenerateNewPopulationByMutation();

            //MutatePopulation();

            this._generation++;
        }

        private void ComputeFittness()
        {
            

            for (int index = 0; index < this._population.Length; index++)
            {
                //fittness[index] = FitnessCalculator.GetDrawingFitness2(this._population[index], this._destImg, Color.Black);
                _fittness[index] = FitnessCalculator.GetDrawingFitnessSoftware(this._population[index], this._destCanvas, Color.Black);
                //fittness[index] = FitnessCalculator.GetDrawingFitnessSoftwareNative(this._population[index], this._destImg, this._destImgByte, Color.Black);
                //fittness[index] = FitnessCalculator.GetDrawingFitnessWPF(this._population[index], this._destCanvas, Color.Black);
                
            }
        }

        private void UpdateStatsByFittness()
        {
            long bestFittness = long.MaxValue;
            long WorstFittness = 0;
            int bestIndex = -1;
            for (int index = 0; index < this._population.Length; index++)
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
            }

            this._lastBest = this._population[bestIndex];
            this._lastBestFittness = bestFittness;

            _lastWorstFitnessDiff = WorstFittness - this._lastBestFittness;
        }

        private void GenerateNewPopulationBasic()
        {
            DnaDrawing [] newPopulation = new DnaDrawing[this._population.Length];

            newPopulation[0] = this._currentBest.Clone();

            for (int index = 1; index < this._population.Length; index++)
            {
                int indexParent1 = Tools.GetRandomNumber(0, this._population.Length - 1);
                int indexParent2 = indexParent1;

                while(indexParent1 == indexParent2)
                    indexParent2 = Tools.GetRandomNumber(0, this._population.Length - 1);

                newPopulation[index] = CrossoverBasic(this._population[indexParent1],this._population[indexParent2]);
            }

            this._population = newPopulation;
        }

        private void GenerateNewPopulationByMutation()
        {
            int maxNormalizeValue = this._fittness.Length * 1000000;
            //int [] rouleteTable = RouletteTableNormalize(fittness,maxNormalizeValue);
            RouletteTableNormalizeBetter(this._fittness, this._rouleteTable, this._diffFittness, maxNormalizeValue);

            DnaDrawing [] tmpPolulation = this._population;
            this._population = this._lastPopulation;
            this._lastPopulation = tmpPolulation;

            
                DnaDrawing dnaElite = this._currentBest.Clone();
                //newPopulation[0] = this._currentBest.Clone();
                //newPopulation[0] = this._lastBest.Clone();

                while (!dnaElite.IsDirty)
                    dnaElite.MutateBetter(this._destCanvas, _edgePoints);


                //DnaDrawing [] newPopulation = this._population;
                this._population[0] = dnaElite;
            

            for (int index = 1; index < this._population.Length; index++)
            {
                int indexParent1 = Tools.GetRandomNumber(0, maxNormalizeValue);
                indexParent1 = RouletteVheelParrentIndex(indexParent1, this._rouleteTable);

                DnaDrawing dna = this._lastPopulation[indexParent1].Clone();

                while (!dna.IsDirty)
                    dna.MutateBetter(this._destCanvas, _edgePoints);

                this._population[index] = dna;

            }

            //this._population = newPopulation;
        }

        private void GenerateNewPopulationRoulete()
        {
            int maxNormalizeValue = this._fittness.Length * 100000;
            //int [] rouleteTable = RouletteTableNormalize(fittness,maxNormalizeValue);
            RouletteTableNormalizeBetter(this._fittness, this._rouleteTable, this._diffFittness, maxNormalizeValue);
            
            DnaDrawing [] newPopulation = new DnaDrawing[this._population.Length];

            int index = 0;

            newPopulation[0] = this._currentBest.Clone();
            //newPopulation[0] = this._lastBest.Clone();
            index++;
            //if ((this._generation & 0x1f) == 0)
            //{
            //    newPopulation[0] = this._currentBest.Clone();
            //    index++;
            //}

            //int tmpEliteIndex = Tools.GetRandomNumber(0, maxNormalizeValue);
            //tmpEliteIndex = RouletteVheelParrentIndex(tmpEliteIndex, rouleteTable);
            //newPopulation[0] = this._population[tmpEliteIndex].Clone();

            for (; index < this._population.Length; index++)
            {
                int indexParent1 = Tools.GetRandomNumber(0, maxNormalizeValue);
                indexParent1 = RouletteVheelParrentIndex(indexParent1, this._rouleteTable);


                int indexParent2 = indexParent1;

                while (indexParent1 == indexParent2)
                    indexParent2 = RouletteVheelParrentIndex(Tools.GetRandomNumber(0, maxNormalizeValue), this._rouleteTable);

                //newPopulation[index] = CrossoverBasic(this._population[indexParent1], this._population[indexParent2]);
                newPopulation[index] = CrossoverOnePoint(this._population[indexParent1], this._population[indexParent2]);
            }

            this._population = newPopulation;
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

            for (int index = 0; index < fittness.Length; index++)
                if (fittnessMax < fittness[index]) fittnessMax = fittness[index];

            long sumFittness = 0;
            for (int index = 0; index < fittness.Length; index++)
            {
                long diffFit  = ((fittnessMax - fittness[index] + 100) << 2);
                sumFittness += diffFit;
                diffFittness[index] = diffFit;
            }

           
            int lastRouleteValue = 0;
            for (int index = 0; index < diffFittness.Length; index++)
            {
                int tmp = (int)((diffFittness[index] / (float)sumFittness) * maxNormalizeValue);
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
            DnaDrawing result = new DnaDrawing();

            List<DnaPolygon> polygons = new List<DnaPolygon>();
            int maxIndex = Math.Max(parent1.Polygons.Length,parent2.Polygons.Length);
            for (int index = 0; index < maxIndex; index++)
            {
                DnaDrawing tmp = (Tools.GetRandomNumber(1,1000) > 500)? parent1 : parent2;

                if (index < tmp.Polygons.Length)
                {
                    polygons.Add(tmp.Polygons[index].Clone());
                }
            }

            result.Polygons = polygons.ToArray();

            return result;
        }

        private DnaDrawing CrossoverOnePoint(DnaDrawing parent1, DnaDrawing parent2)
        {
            //double crossLine = Tools.GetRandomNumber(1,9)*0.1d;

            double crossLine = 0.7;


            int countCrossGenP1 = (int)(parent1.Polygons.Length * crossLine);
            int countCrossGenP2 = (int)(parent2.Polygons.Length * (crossLine));
            int newDnaSize = countCrossGenP1 + (parent2.Polygons.Length- countCrossGenP2*1);

            DnaDrawing result = new DnaDrawing();

            DnaPolygon [] polygons = new DnaPolygon[newDnaSize];
            int polygonsIndex = 0;

            int maxIndex = Math.Max(parent1.Polygons.Length, parent2.Polygons.Length);

            for (int index = 0; index < countCrossGenP1; index++)
            {
                polygons[polygonsIndex++] = parent1.Polygons[index].Clone();   
            }

            

            for (int index = countCrossGenP2; index < parent2.Polygons.Length; index++)
            {
                polygons[polygonsIndex++] =parent2.Polygons[index].Clone();
            }

            result.Polygons = polygons;

            return result;
        }

        private DnaDrawing CrossoverOnePoint2(DnaDrawing parent1, DnaDrawing parent2)
        {
            //double crossLine = Tools.GetRandomNumber(1,9)*0.1d;

            double crossLine = 0.3;


            DnaDrawing result = new DnaDrawing();

            List<DnaPolygon> polygons = new List<DnaPolygon>();
            int maxIndex = Math.Max(parent2.Polygons.Length, parent2.Polygons.Length);

            int countCrossGen = (int)(parent2.Polygons.Count() * crossLine);

            for (int index = 0; index < countCrossGen; index++)
            {
                polygons.Add(parent2.Polygons[index].Clone());
            }

            countCrossGen = (int)(parent1.Polygons.Count() * (crossLine));

            for (int index = countCrossGen; index < parent1.Polygons.Length; index++)
            {
                polygons.Add(parent1.Polygons[index].Clone());
            }

            result.Polygons = polygons.ToArray();

            return result;
        }

        private void MutatePopulation()
        {
            for (int index = 0; index < this._population.Length; index++)
            {
                while(!this._population[index].IsDirty)
                    this._population[index].Mutate(this._destCanvas);

                this._population[index].SetDirty();
            }
        }

        public void Dispose()
        {
           
        }
    }
}
