using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace GenArt.Classes
{
    public static class Tools
    {
        private static RandomNumberGenerator rng  = RandomNumberGenerator.Create();
        private static byte [] buff = new byte[800000];
        private static int buffIndex = 450000;
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

            return (int)(randValue % tmp);
            {
            //    randomCall++;
            //    return random.Next(min, max);
            }
        }

        public static int GetRandomNumber(int min, int max, int ignore)
        {
            if (!(min <= ignore && ignore < max)) return GetRandomNumber(min, max);

            int tmp = GetRandomNumber(min, max - 1);
            return (tmp >= ignore) ? tmp + 1 : tmp;
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