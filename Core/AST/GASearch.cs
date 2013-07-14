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

namespace GenArt.Core.AST
{
    public class GASearch :IDisposable
    {
        private D3DDnaRender D3DRender = null;

        private DnaDrawing _currentBest;
        private long _currentBestFittness;
        private DnaDrawing _lastBest;
        private long _lastBestFittness;
        private long _lastWorstFitnessDiff;
        private int _generation = 0;
        private DnaDrawing [] _population;
        private Bitmap _destImg = new Bitmap(1,1);
        private byte [] _destImgByte = new byte[0];
        private byte [] _bitmapRaw = new byte[4];

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

        }

        private void CreateByteFieldFromDestImg()
        {
            this._destImgByte = new byte[_destImg.Width * _destImg.Height * 4];

            BitmapData bmdSRC = _destImg.LockBits(new Rectangle(0, 0, _destImg.Width, _destImg.Height), ImageLockMode.ReadOnly,
                                        PixelFormat.Format32bppPArgb);
            try
            {
                // zkopiruje data do bajtoveho pole
                // barvy se ukladaji rgba.
                unsafe
                {

                    byte * origPtr = (byte*)bmdSRC.Scan0.ToPointer();

                    for (int i = 0; i < this._destImgByte.Length; i++)
                    {
                        this._destImgByte[i] = *(origPtr + i);
                    }
                }
            }
            finally
            {
                _destImg.UnlockBits(bmdSRC);
            }
        }

        private Color GetColorByPolygonPoints(DnaPoint [] points)
        {
            int sumRed = 0;
            int sumGreen = 0;
            int sumBlue = 0;


            for (int index = 0; index < points.Length; index++)
            {
                int colorIndex = ((points[index].Y * this._destImg.Width) + points[index].X) << 2;
                sumRed += this._bitmapRaw[colorIndex];
                sumGreen += this._bitmapRaw[colorIndex+1];
                sumBlue += this._bitmapRaw[colorIndex+2];
            }

            return Color.FromArgb(255, sumRed / points.Length, sumGreen / points.Length, sumBlue / points.Length);
        }

        public void InitFirstPopulation(Bitmap destImg,byte [] bitmapRaw)
        {
            if (this.D3DRender != null)
            {
                this.D3DRender.Dispose();
                this.D3DRender = null;
            }

            this.D3DRender = new D3DDnaRender(destImg.Width, destImg.Height);

            this._generation = 0;
            this._destImg = destImg;
            this._bitmapRaw = bitmapRaw;
            this._currentBestFittness = long.MaxValue;

            CreateByteFieldFromDestImg();

            for (int i =0; i < this._population.Length; i++)
            {
                DnaDrawing dna = new DnaDrawing();

                for (int k =0; k < 10; k++)
                {
                    dna.AddPolygon(this._bitmapRaw,this._destImg.Width);
                
                }
                
                this._population[i] = dna;
            }

         
           
        }




        public void ExecuteGeneration()
        {
            long [] fittness = ComputeFittness();

            UpdateStatsByFittness(fittness);

            //GenerateNewPopulationBasic();
            //GenerateNewPopulationRoulete(fittness);
            GenerateNewPopulationByMutation(fittness);

            //MutatePopulation();

            this._generation++;
        }

        private long [] ComputeFittness()
        {
            long [] fittness = new long[this._population.Length];

            for (int index = 0; index < this._population.Length; index++)
            {
                //fittness[index] = FitnessCalculator.GetDrawingFitness2(this._population[index], this._destImg, Color.Black);
                //fittness[index] = FitnessCalculator.GetDrawingFitnessSoftware(this._population[index], this._destImg, this._destImgByte, Color.Black);
                fittness[index] = FitnessCalculator.GetDrawingFitnessSoftwareNative(this._population[index], this._destImg, this._destImgByte, Color.Black);
                //fittness[index] = FitnessCalculator.GetDrawingFitnessD3D(this.D3DRender, this._population[index], this._destImg, Color.Black);
                
            }

            return fittness;
        }

        private void UpdateStatsByFittness(long [] fittness)
        {
            long bestFittness = long.MaxValue;
            long WorstFittness = 0;
            int bestIndex = -1;
            for (int index = 0; index < this._population.Length; index++)
            {
                if (fittness[index] > WorstFittness)
                {
                    WorstFittness = fittness[index];
                }

                if (fittness[index] < bestFittness)
                {
                    bestFittness = fittness[index];
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

        private void GenerateNewPopulationByMutation(long[] fittness)
        {
            int maxNormalizeValue = fittness.Length * 1000;
            //int [] rouleteTable = RouletteTableNormalize(fittness,maxNormalizeValue);
            int [] rouleteTable = RouletteTableNormalizeBetter(fittness, maxNormalizeValue);


            DnaDrawing [] newPopulation = new DnaDrawing[this._population.Length];

            //newPopulation[0] = this._currentBest.Clone();
            newPopulation[0] = this._lastBest.Clone();

            for (int index = 1; index < this._population.Length; index++)
            {
                int indexParent1 = Tools.GetRandomNumber(0, maxNormalizeValue);
                indexParent1 = RouletteVheelParrentIndex(indexParent1, rouleteTable);

                newPopulation[index] = this._population[indexParent1].Clone();

                while (!newPopulation[index].IsDirty)
                    newPopulation[index].MutateBetter();

            }

            this._population = newPopulation;
        }

        private void GenerateNewPopulationRoulete(long[] fittness)
        {
            int maxNormalizeValue = fittness.Length * 100000;
            //int [] rouleteTable = RouletteTableNormalize(fittness,maxNormalizeValue);
            int [] rouleteTable = RouletteTableNormalizeBetter(fittness, maxNormalizeValue);
            
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
                indexParent1 = RouletteVheelParrentIndex(indexParent1, rouleteTable);


                int indexParent2 = indexParent1;

                while (indexParent1 == indexParent2)
                    indexParent2 = RouletteVheelParrentIndex(Tools.GetRandomNumber(0, maxNormalizeValue), rouleteTable);

                //newPopulation[index] = CrossoverBasic(this._population[indexParent1], this._population[indexParent2]);
                //newPopulation[index] = CrossoverOnePoint(this._population[indexParent1], this._population[indexParent2]);
                newPopulation[index] = CrossoverOnePointUniqueCheck(this._population[indexParent1], this._population[indexParent2]);
            }

            this._population = newPopulation;
        }

        private int[] RouletteTableNormalize(long[] fittness, int maxNormalizeValue)
        {
            
            long sumFittness = 0;
            for (int index = 0; index < fittness.Length; index++)
                sumFittness += fittness[index];

            int [] rouleteTable = new int[fittness.Length];

            int lastRouleteValue = 0;
            for (int index = 0; index < fittness.Length; index++)
            {
                int tmp = (int)((fittness[index] / (float)sumFittness) * maxNormalizeValue);
                rouleteTable[index] = lastRouleteValue + tmp;
                lastRouleteValue = lastRouleteValue + tmp;
            }

            return rouleteTable;
        }

        private int[] RouletteTableNormalizeBetter(long[] fittness, int maxNormalizeValue)
        {
            long [] diffFittness = new long[fittness.Length];

            long fittnessMax = 0;

            for (int index = 0; index < fittness.Length; index++)
                if (fittnessMax < fittness[index]) fittnessMax = fittness[index];

            for (int index = 0; index < fittness.Length; index++)
                diffFittness[index] = ((fittnessMax - fittness[index]+100) << 2) ;

            long sumFittness = 0;
            for (int index = 0; index < diffFittness.Length; index++)
                sumFittness += diffFittness[index];

            int [] rouleteTable = new int[fittness.Length];

            int lastRouleteValue = 0;
            for (int index = 0; index < diffFittness.Length; index++)
            {
                int tmp = (int)((diffFittness[index] / (float)sumFittness) * maxNormalizeValue);
                rouleteTable[index] = lastRouleteValue + tmp;
                lastRouleteValue = lastRouleteValue + tmp;
            }

            return rouleteTable;
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

        private DnaDrawing CrossoverOnePointUniqueCheck(DnaDrawing parent1, DnaDrawing parent2)
        {
            //double crossLine = Tools.GetRandomNumber(1,9)*0.1d;

            double crossLine = 0.7;


            int countCrossGenP1 = (int)(parent1.Polygons.Length * crossLine);
            int countCrossGenP2 = (int)(parent2.Polygons.Length * (crossLine));
            int newDnaSize = countCrossGenP1 + (parent2.Polygons.Length - countCrossGenP2 * 1);

            DnaDrawing result = new DnaDrawing();

            DnaPolygon [] polygons = new DnaPolygon[newDnaSize];
            int polygonsIndex = 0;

            int maxIndex = Math.Max(parent1.Polygons.Length, parent2.Polygons.Length);

            HashSet<uint> uniqIds = new HashSet<uint>();

            for (int index = 0; index < countCrossGenP1; index++)
            {
                DnaPolygon poly = parent1.Polygons[index].Clone();

                if (!uniqIds.Contains(poly.UniqueID))
                {
                    polygons[polygonsIndex++] = parent1.Polygons[index].Clone();
                    uniqIds.Add(poly.UniqueID);
                }
            }



            for (int index = countCrossGenP2; index < parent2.Polygons.Length; index++)
            {
                DnaPolygon poly = parent2.Polygons[index].Clone();

                if (!uniqIds.Contains(poly.UniqueID))
                {
                    polygons[polygonsIndex++] = parent2.Polygons[index].Clone();
                    uniqIds.Add(poly.UniqueID);
                }
            }

            if (polygonsIndex < polygons.Length)
            {
                DnaPolygon [] tmp = new DnaPolygon[polygonsIndex];
                Array.Copy(polygons, tmp, tmp.Length);
                polygons = tmp;
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
                this._population[index].Mutate(this._bitmapRaw, this._destImg.Width);

                this._population[index].SetDirty();
            }
        }

        public void Dispose()
        {
            if (this.D3DRender != null)
            {
                this.D3DRender.Dispose();
                this.D3DRender = null;
            }
        }
    }
}
