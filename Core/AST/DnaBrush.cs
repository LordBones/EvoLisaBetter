using GenArt.Classes;
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
            Red = (byte)Tools.GetRandomNumber(0, 255);
            Green = (byte)Tools.GetRandomNumber(0, 255);
            Blue = (byte)Tools.GetRandomNumber(0, 255);
            Alpha = (byte)Tools.GetRandomNumber(1, 254);

            //Red = 255;
            //Green = 0;
            //Blue = 0;
            //Alpha = 255;

            //Alpha = 255;

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

        public bool Mutate(DnaDrawing drawing)
        {
            if (Tools.WillMutate(Settings.ActiveRedMutationRate))
            {
                int colorPart = Tools.GetRandomNumber(1<<8, 4<<8)>>8;

                if (colorPart == 1)
                {

                    //Red = (byte)Tools.GetRandomNumber(Settings.ActiveRedRangeMin, Settings.ActiveRedRangeMax);
                    //Red = (byte)((Tools.GetRandomNumber(Settings.ActiveRedRangeMin, Settings.ActiveRedRangeMax) + Red) / 2);
                    int tmp = Tools.GetRandomNumber(1, 11) - 5 + Red;

                    if (tmp < 0) Red = 0;
                    else if (tmp > 255) Red = 255;
                    else Red = (byte)tmp;
                }
                else if (colorPart == 2)
                {
                    //Green = (byte)Tools.GetRandomNumber(Settings.ActiveGreenRangeMin, Settings.ActiveGreenRangeMax);
                    //Green = (byte)((Tools.GetRandomNumber(Settings.ActiveGreenRangeMin, Settings.ActiveGreenRangeMax)+Green)/2);

                    int tmp = Tools.GetRandomNumber(1, 11) - 5 + Green;

                    if (tmp < 0) Green = 0;
                    else if (tmp > 255) Green = 255;
                    else Green = (byte)tmp;
                }
                else if (colorPart == 3)
                {
                    //Blue = (byte)Tools.GetRandomNumber(Settings.ActiveBlueRangeMin, Settings.ActiveBlueRangeMax);
                    //Blue = (byte)((Tools.GetRandomNumber(Settings.ActiveBlueRangeMin, Settings.ActiveBlueRangeMax) + Blue)/2);

                    int tmp = Tools.GetRandomNumber(1, 11) - 5 + Blue;

                    if (tmp < 0) Blue = 0;
                    else if (tmp > 255) Blue = 255;
                    else Blue = (byte)tmp;
                }
                else if (colorPart == 4)
                {

                    //Alpha = (byte)Tools.GetRandomNumber(Settings.ActiveAlphaRangeMin, Settings.ActiveAlphaRangeMax);
                    //Alpha = (byte)((Tools.GetRandomNumber(Settings.ActiveAlphaRangeMin, Settings.ActiveAlphaRangeMax) + Alpha)/2);

                    int tmp = Tools.GetRandomNumber(1, 11) - 5 + Alpha;

                    if (tmp < 0) Alpha = 0;
                    else if (tmp > 255) Alpha = 255;
                    else Alpha = (byte)tmp;
                }
                
                drawing.SetDirty();

                return true;
            }

            return false;
        }


        

        public bool MutateByHSL(DnaDrawing drawing)
        {
            bool wasMutate = false;
            //HSLColor hslct = new HSLColor(Red, Green, Blue);
            

            if (  Tools.GetRandomNumber(0, 1000000) < 250000)
            {
                HSLColor hslc = new HSLColor(Red, Green, Blue);

                int tmp = Tools.GetRandomNumber(0,20,10);
                int hue = (int)hslc.Hue;
                hslc.Hue = Math.Max(Math.Min(hue + tmp-10, 255), 5);

                Color color = (Color)hslc;
                Red = color.R;
                Green = color.G;
                Blue = color.B;
                wasMutate = true;
            }
            
            if ( Tools.GetRandomNumber(0, 1000000) < 250000)
            {
                HSLColor hslc = new HSLColor(Red, Green, Blue);

                int tmp = Tools.GetRandomNumber(0, 20, 10);
                int sat = (int)hslc.Saturation;
                hslc.Saturation = Math.Max(Math.Min(sat + tmp - 10, 255), 5);


                Color color = (Color)hslc;
                Red = color.R;
                Green = color.G;
                Blue = color.B;
                wasMutate = true;
            }
            if (Tools.GetRandomNumber(0, 1000000) < 250000)
            {
                HSLColor hslc = new HSLColor(Red, Green, Blue);

                int tmp = Tools.GetRandomNumber(0, 20, 10);
                int lum = (int)hslc.Luminosity;
                hslc.Luminosity = Math.Max(Math.Min(lum + tmp - 10, 255), 5);

                Color color = (Color)hslc;
                Red = color.R;
                Green = color.G;
                Blue = color.B;
                wasMutate = true;
            }

            if (Tools.GetRandomNumber(0, 1000000) < 250000 || !wasMutate)
            {
                Alpha = (byte)Math.Max(Math.Min(Alpha + Tools.GetRandomNumber(0, 20, 10) - 10, 255), 5); 

            }

            drawing.SetDirty();

            return true;
        }

        public bool MutateByHSLRandom(DnaDrawing drawing)
        {
            bool wasMutate = false;
            if (Tools.GetRandomNumber(0, 1000000) < 250000)
            {
                HSLColor hslc = new HSLColor(Red, Green, Blue);
                hslc.Hue = Tools.GetRandomNumber(5, 255, (int)hslc.Hue);

                Color color = (Color)hslc;
                Red = color.R;
                Green = color.G;
                Blue = color.B;
                wasMutate = true;
            }
            if (Tools.GetRandomNumber(0, 1000000) < 250000)
            {
                HSLColor hslc = new HSLColor(Red, Green, Blue);
                hslc.Luminosity = Tools.GetRandomNumber(5, 255, (int)hslc.Luminosity);

                Color color = (Color)hslc;
                Red = color.R;
                Green = color.G;
                Blue = color.B;
                wasMutate = true;
            }
            if (Tools.GetRandomNumber(0, 1000000) < 250000)
            {
                HSLColor hslc = new HSLColor(Red, Green, Blue);
                hslc.Saturation = Tools.GetRandomNumber(5, 255, (int)hslc.Saturation);

                Color color = (Color)hslc;
                Red = color.R;
                Green = color.G;
                Blue = color.B;
                wasMutate = true;
            }
            if (Tools.GetRandomNumber(0, 1000000) < 250000 || !wasMutate)
            {
                Alpha = (byte)Tools.GetRandomNumber(5, 255, Alpha);

            }

            drawing.SetDirty();

            return true;
        }
    }
}