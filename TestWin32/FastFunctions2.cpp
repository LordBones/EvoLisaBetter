#include "stdafx.h"
#include "FastFunctions2.h"


FastFunctions2::FastFunctions2(void)
{
}


FastFunctions2::~FastFunctions2(void)
{
}

void FastFunctions2::FastRowApplyColor(unsigned char * canvas, int from, int to, int colorABRrem, int colorAGRrem, int colorARRrem, int colorRem)
{
    //unsigned int * ptrColor = (unsigned int *)(canvas+from);

	//int count = (to - from)>>2;

	// while(count>=0)
 //   {
 //       
	//	unsigned int Color = ptrColor[count];
	//	unsigned int res = Color&0xff000000;

	//	res |= ApplyColor((Color>>16)&0xFF, colorARRrem, colorRem) << 16; // r
	//	unsigned int G = (Color>>8)&0xFF;
	//	res |= ApplyColor(G, colorAGRrem, colorRem)<<8;
	//	unsigned int B = Color&0xFF;
	//	

 //       res |= ((colorABRrem + colorRem * B)>> 16);// ApplyColor(B, colorABRrem, colorRem);
 //       
 //       

	//	ptrColor[count] = res; // (Color&0xff000000) | (R<<16) | (G << 8) | B;

	//	
	//	count--;
 //       
 //   }


    /*
    while(from <= to)
    {
        int index = from;
         canvas[index] = (unsigned char)FastFunctions::ApplyColor(canvas[index], colorABRrem, colorRem);
                    canvas[index + 1] = (unsigned char)FastFunctions::ApplyColor(canvas[index + 1], colorAGRrem, colorRem);
                    canvas[index + 2] = (unsigned char)FastFunctions::ApplyColor(canvas[index + 2], colorARRrem, colorRem);

                    from += 4;
    }*/

    while(from <= to)
    {
        int index = from;
        //((axrem + rem * colorChanel) >> 16)
        unsigned int b = canvas[index];
        unsigned int g = canvas[index+1];
        unsigned int r = canvas[index+2];
        b*=colorRem;
        g*=colorRem;
        r*=colorRem;

        b+=colorABRrem;
        g+=colorAGRrem;
        r+=colorARRrem;

        b>>=16;
        g>>=16;
        r>>=16;

        canvas[index] = (unsigned char)b;
        canvas[index+1] = (unsigned char)g;
        canvas[index+2] = (unsigned char)r;
  
        from += 4;
    }




   /* canvas = canvas + from;

    while(from <= to)
    {
         *canvas = ApplyColor(*canvas, colorABRrem, colorRem);
                    canvas[1] = ApplyColor(canvas[1], colorAGRrem, colorRem);
                    canvas[2] = ApplyColor(canvas[2], colorARRrem, colorRem);

                    from += 4;
                    canvas +=4;

    }*/
}

