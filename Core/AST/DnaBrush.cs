﻿using GenArt.Classes;
using System;
using System.Drawing;
using GenArt.Core.Classes;

namespace GenArt.AST
{
    [Serializable]
    public struct DnaBrush
    {
        public byte Red;
        public byte Green;
        public byte Blue;
        public byte Alpha;

        public Color BrushColor
        {
            get
            {
                return Color.FromArgb(Alpha
                    , Red, Green, Blue);
            }
        }
        public System.Windows.Media.Brush brushWPF { get { return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(Alpha, Red, Green, Blue)); } }


        public DnaBrush(int alpha, int red, int green, int blue)
        {
            Red = (byte)red;
            Green = (byte)green;
            Blue = (byte)blue;
            Alpha = (byte)alpha;

        }

       
        public void InitRandom()
        {
            Red = (byte)Tools.GetRandomNumber(0, 256);
            Green = (byte)Tools.GetRandomNumber(0, 256);
            Blue = (byte)Tools.GetRandomNumber(0, 256);
            Alpha = (byte)Tools.GetRandomNumber(1, 256);

       
        }

        public void InitRandomWithoutAlpha()
        {
            Red = (byte)Tools.GetRandomNumber(0, 256);
            Green = (byte)Tools.GetRandomNumber(0, 256);
            Blue = (byte)Tools.GetRandomNumber(0, 256);
        }

        public void SetByColor(Color color)
        {
            this.Alpha = color.A;
            this.Blue = color.B;
            this.Green = color.G;
            this.Red = color.R;
        }
     
        private DnaBrush Clone()
        {
            return new DnaBrush
                       {
                           Alpha = Alpha,
                           Blue = Blue,
                           Green = Green,
                           Red = Red,
                       };
        }

       

        public bool MutateRGBOld(byte mutationRate, DnaDrawing drawing)
        {
            

            int colorPart =  Tools.GetRandomNumber(1, 5);

        
                if (colorPart == 1)
                {
                    int value = Tools.GetRandomChangeValue(Red, 0, 255, mutationRate);
                    Red = (byte)Math.Max(Math.Min(value, 255), 0);
                }
                else if (colorPart == 2)
                {
                    int value = Tools.GetRandomChangeValue(Green, 0, 255, mutationRate);
                    Green = (byte)Math.Max(Math.Min(value, 255), 0);
                }
                else if (colorPart == 3)
                {
                    int value = Tools.GetRandomChangeValue(Blue, 0, 255, mutationRate);
                    Blue = (byte)Math.Max(Math.Min(value, 255), 0);
                }
                else if (colorPart >= 4)
                {
                    int value = Tools.GetRandomChangeValue(Alpha, 5, 255, mutationRate);
                    Alpha = (byte)Math.Max(Math.Min(value, 255), 5);

                    //Alpha = (byte)Math.Max(Math.Min(Alpha + Tools.GetRandomNumber(0, 20, 10) - 10, 255), 5);
                    //Alpha = (byte)Tools.GetRandomNumber(5, 256, Alpha);
                }
                
                drawing.SetDirty();

                return true;
            
        }

        public bool MutateRGBOld3(byte mutationRate, DnaDrawing drawing)
        {
            int mutationMax = Math.Max(2, ((mutationRate + 1) * 255) / (256));
            int mutationMiddle = mutationMax / 2;


            int colorPart =  Tools.GetRandomNumber(1, 5);

            int tmp = Tools.GetRandomNumber(0, mutationMax, mutationMiddle) - mutationMiddle;

                if (colorPart == 1)
                {
                    Red = (byte)Math.Max(Math.Min(Red + tmp, 255), 0);
                }
                else if (colorPart == 2)
                {
                    Green = (byte)Math.Max(Math.Min(Green + tmp, 255), 0);
                }
                else if (colorPart == 3)
                {
                    Blue = (byte)Math.Max(Math.Min(Blue + tmp, 255), 0);
                }
                else if (colorPart >= 4)
                {
                    tmp = Tools.GetRandomNumber(0, mutationMax, mutationMiddle) - mutationMiddle;
                    Alpha = (byte)Math.Max(Math.Min(Alpha + tmp, 255), 5);

                    //Alpha = (byte)Math.Max(Math.Min(Alpha + Tools.GetRandomNumber(0, 20, 10) - 10, 255), 5);
                    //Alpha = (byte)Tools.GetRandomNumber(5, 256, Alpha);
                }
                
                drawing.SetDirty();

                return true;
            
        }

        public bool MutateRGBOld2(DnaDrawing drawing)
        {

            int colorPart = Tools.GetRandomNumber(1, 5);

            int tmp = Tools.GetRandomNumber(0, 160, 80) - 80;

            if (colorPart == 1)
            {
                Red = (byte)Tools.GetRandomNumber(0, 256, Red);
            }
            else if (colorPart == 2)
            {
                Green = (byte)Tools.GetRandomNumber(0, 256, Green);
            }
            else if (colorPart == 3)
            {
                Blue = (byte)Tools.GetRandomNumber(0, 256, Blue);
            }
            else if (colorPart >= 4)
            {
                Alpha = (byte)Tools.GetRandomNumber(5, 256, Alpha);
                
                //Alpha = (byte)Math.Max(Math.Min(Alpha + Tools.GetRandomNumber(0, 20, 10) - 10, 255), 5);
                //Alpha = (byte)Tools.GetRandomNumber(5, 256, Alpha);
            }

            drawing.SetDirty();

            return true;

        }


        public bool MutateRGBOldWithoutAlpha(DnaDrawing drawing)
        {

            int colorPart = Tools.GetRandomNumber(1, 4);

            int tmp = Tools.GetRandomNumber(0, 10, 5) - 5;

            if (colorPart == 1)
            {
                Red = (byte)Math.Max(Math.Min(Red + tmp , 255), 0);
            }
            else if (colorPart == 2)
            {
                Green = (byte)Math.Max(Math.Min(Green + tmp , 255), 0);
            }
            else if (colorPart == 3)
            {
                Blue = (byte)Math.Max(Math.Min(Blue + tmp , 255), 0);
            }

            drawing.SetDirty();

            return true;

        }


        public bool MutateByRGB(DnaDrawing drawing)
        {
            bool wasMutate = false;
            //HSLColor hslct = new HSLColor(Red, Green, Blue);


            if (Tools.GetRandomNumber(0, 101) < 25)
            {
                int tmp = Tools.GetRandomNumber(0, 20, 10);
                
                Red = (byte)Math.Max(Math.Min(Red + tmp - 10, 255), 5);
                wasMutate = true;
            }

            if (Tools.GetRandomNumber(0, 101) < 25)
            {
                int tmp = Tools.GetRandomNumber(0, 20, 10);
                Green = (byte)Math.Max(Math.Min(Green + tmp - 10, 255), 5);

                wasMutate = true;
            }
            if (Tools.GetRandomNumber(0, 101) < 25)
            {
                int tmp = Tools.GetRandomNumber(0, 20, 10);
                Blue = (byte)Math.Max(Math.Min(Blue + tmp - 10, 255), 5);

                wasMutate = true;
            }

            if (Tools.GetRandomNumber(1, 101) <= 25 || !wasMutate)
            {
                int tmp = Tools.GetRandomNumber(0, 20, 10);
                Alpha = (byte)Math.Max(Math.Min(Alpha + tmp - 10, 255), 5);
                //Alpha = (byte)Math.Max(Math.Min(Alpha + Tools.GetRandomNumber(0, 20, 10) - 10, 255), 5);
                //Alpha = (byte)Tools.GetRandomNumber(5, 255,Alpha);

            }

            drawing.SetDirty();

            return true;
        }
    }
}