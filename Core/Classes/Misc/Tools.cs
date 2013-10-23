using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace GenArt.Classes
{
    public static class Tools
    {
        private static RandomNumberGenerator rng  = RandomNumberGenerator.Create();
        private static byte [] buff = new byte[800000];
        private static int buffIndex = 4500000;
        private static  Random random = new Random(0);

        public static readonly int MaxPolygons = 250;
        public static long randomCall = 0;

        public static void ClearPseudoRandom() { random = new Random(0); random.NextBytes(buff); buffIndex = 0; randomCall = 0; }

        

        public static int GetRandomNumber(int min, int max)
        {
            //int dif = max - min;
            //if (dif < 10000)
            //{
            //    int newVal = random.Next(255,((dif+1)<<8)-1) >> 8;
            //    return min + newVal;
            //}
            //else

            if (buffIndex >= buff.Length)
            {
                random.NextBytes(buff);
                //rng.GetBytes(buff);
                buffIndex = 0;
            }

            randomCall++;

            uint randValue = (uint)((buff[buffIndex] << 24) + (buff[buffIndex + 1] << 16) + (buff[buffIndex + 2] << 8) + buff[buffIndex + 3]);
            buffIndex += 4;

            uint tmp = (uint)(max - min);

            return (int)min+(int)(randValue % tmp);
            {
            //    randomCall++;
            //    return random.Next(min, max);
            }
        }

        public static double GetRandomNumberDouble()
        {
            if (buffIndex >= buff.Length)
            {
                random.NextBytes(buff);
                //rng.GetBytes(buff);
                buffIndex = 0;
            }

            randomCall++;

            uint randValue = (uint)((buff[buffIndex] << 24) + (buff[buffIndex + 1] << 16) + (buff[buffIndex + 2] << 8) + buff[buffIndex + 3]);
            buffIndex += 4;

            
            return  (1.0/uint.MaxValue)*randValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int GetRandomNumberNoLinear_MinMoreOften(int maxValue, byte mutationRate)
        {
            double rnd = GetRandomNumberDouble();
            double power = 4 - 3*(mutationRate/255.0);

            return (int)(maxValue- Math.Pow(rnd, 1.0 / power) * maxValue); 
        }

        public static int GetRandomNumberNoLinear_MinMoreOften(int value, int leftMinValue, int rightMaxValue, byte mutationRate)
        {
            int leftDiff = value - leftMinValue + 1;
            int rightDiff = rightMaxValue - value + 1;
            int randomMark = (Tools.GetRandomNumber(0, 2) == 0) ? 1 : -1;

            int mutationMax =  (randomMark >= 0) ? rightDiff : leftDiff;
            int tmp = Tools.GetRandomNumberNoLinear_MinMoreOften(mutationMax, mutationRate) * randomMark;

            return value + tmp;
        }

        public static int GetRandomNumber(int min, int max, int ignore)
        {
            if (!(min <= ignore && ignore < max)) return GetRandomNumber(min, max);

            int tmp = GetRandomNumber(min, max - 1);
            return (tmp >= ignore) ? tmp + 1 : tmp;
        }

        public static int GetRandomChangeValue(int oldValue, int min, int max)
        {
            if (!(oldValue >= min && oldValue <= max))
                throw new NotImplementedException();

            int leftDelta = oldValue - min;
            int rightDelta = max - oldValue + 1;

            int diff = GetRandomNumber(0, leftDelta + rightDelta);

            return oldValue + diff - leftDelta;
        }

        public static int GetRandomChangeValue(int oldValue, int min, int max, byte mutationRate)
        {
            if (!(oldValue >= min && oldValue <= max))
                throw new NotImplementedException();

            int leftDelta = oldValue - min;
            int rightDelta = max - oldValue + 1;

            leftDelta = GetValueByMutationRate(leftDelta, 0, mutationRate);
            rightDelta = GetValueByMutationRate(rightDelta, 1, mutationRate);

           

            int diff = GetRandomNumber(0, leftDelta + rightDelta);

            return oldValue + diff - leftDelta;
        }

        public static int GetRandomChangeValueGuaranted(int oldValue, int min, int max, byte mutationRate)
        {
            if (!(oldValue >= min && oldValue <= max))
                throw new NotImplementedException();

            int leftDelta = oldValue - min;
            int rightDelta = max - oldValue + 1;

            leftDelta = GetValueByMutationRate(leftDelta, 0, mutationRate);
            rightDelta = GetValueByMutationRate(rightDelta, 1, mutationRate);

            // nutne pokud je pouze jeden prvek a je soucasne v ignore
            if (leftDelta + rightDelta == 1)
                return oldValue;

            int diff = GetRandomNumber(0, leftDelta + rightDelta, leftDelta);

            return oldValue + diff - leftDelta;
        }

        public static int GetValueByMutationRate(int value, int minValue, byte mutationRate)
        {
            return Math.Max(minValue, ((mutationRate + 1) * value) / (256));
        }
        

        //public static int GetRandomNumber(int min, int max)
        //{
        //    return GetRandomNumber2(min, max);
        //}

        //public static int GetRandomNumber(int min, int max, int ignore)
        //{
        //    if (!(min <= ignore && ignore < max)) return GetRandomNumber2(min, max);

        //    int tmp = GetRandomNumber2(min, max - 1);
        //    return (tmp >= ignore) ? tmp + 1 : tmp;
        //}

        public static void swap<T>(ref T p1, ref T p2)
        {
            T tmp =  p1;
            p1 = p2;
            p2 = tmp;
        }

        public static int MaxWidth = 200;
        public static int MaxHeight = 200;

        public static bool WillMutate(int mutationRate)
        {
            return GetRandomNumber(0, mutationRate) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int fastAbs(int value)
        {
            int topbitreplicated = value >> 31;
            return (value ^ topbitreplicated) - topbitreplicated;
        }
    }
}