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
        private static readonly Random random = new Random(0);

        public static readonly int MaxPolygons = 250;

        public static int GetRandomNumber2(int min, int max)
        {
            if (buffIndex >= buff.Length)
            {
                rng.GetBytes(buff);
                buffIndex = 0;
            }

            long randValue = (((long)buff[buffIndex] << 24) + (buff[buffIndex + 1] << 16) + (buff[buffIndex + 2] << 8) + buff[buffIndex+3]);
          
            long delta = ((max-1) + 0xffffffff) - (min + (0xffffffff));

            long newDelta =(long)((randValue / (float)uint.MaxValue) * delta);
            newDelta = min + newDelta;


            buffIndex += 4;

            if (newDelta > max) return max;
            if (newDelta < min) return min;

            return (int)newDelta;
            
        }
        public static int GetRandomNumber(int min, int max)
        {
            //int dif = max - min;
            //if (dif < 10000)
            //{
            //    int newVal = random.Next(255,((dif+1)<<8)-1) >> 8;
            //    return min + newVal;
            //}
            //else
            {
                return random.Next(min, max);
            }
        }

        public static int GetRandomNumber(int min, int max, int ignore)
        {
            if (!(min <= ignore && ignore <= max)) return GetRandomNumber(min, max);

            int tmp = GetRandomNumber(min, max - 1);
            return (tmp >= ignore)? tmp+1 : tmp;
        }

        public static short GetRandomNumber(short min, short max)
        {
            return (short)random.Next(min, max);
        }

        //public static int GetRandomNumber(int min, int max)
        //{
        //    return GetRandomNumber2(min, max);
        //}

        //public static int GetRandomNumber(int min, int max, int ignore)
        //{
        //    if (!(min <= ignore && ignore <= max)) return GetRandomNumber2(min, max);

        //    int tmp = GetRandomNumber2(min, max - 1);
        //    return (tmp >= ignore)? tmp+1 : tmp;
        //}

        //public static short GetRandomNumber(short min, short max)
        //{
        //    return (short)GetRandomNumber2(min, max);
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