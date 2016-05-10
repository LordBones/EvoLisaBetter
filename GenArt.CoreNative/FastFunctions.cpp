
#include <math.h>

//#include <mmintrin.h>
#include <emmintrin.h>
#include <smmintrin.h>



#include "FastFunctions.h"
#include "AlphaBlending.h"
#include "NativeMedian8bit.h"

#define SWAP(t,a,b) ( (t) = (a), (a) = (b), (b) = (t) )
#define SWAP2(a,b) ( (a) ^= (b), (b) ^= (a), (a) ^= (b) )




int __fastcall FastAbs2(int data)
{
    int topbitreplicated = data >> 31;
    return (data ^ topbitreplicated) - topbitreplicated;  
}

unsigned short int Fast255div(unsigned short int value)
{
    return (value+1 + (value >> 8)) >> 8;
}



void FastFunctions::RenderRectangle(unsigned char * canvas,int canvasWidth,const int x,const int y,const int width,const int height,const int color ,const int alpha)
{
    int rowStartIndex = y * canvasWidth + x * 4;

    //if(width > 127)
    //{
    //    for (int iy  = 0; iy < height; iy++)//, indexY += this._canvasWidth)
    //    {
    //        NewFastRowApplyColorSSE128(&canvas[rowStartIndex], width, color, alpha);

    //        rowStartIndex += canvasWidth;
    //    }
    //}
    //else
    {
		int alpha256 = ((color) >> 24) & 0xff;
		if (alpha256 == 0xff)  alpha256 += 1;

        canvas+=rowStartIndex;

        for (int iy  = 0; iy < height; iy++)//, indexY += this._canvasWidth)
        {
            AlphaBlending::NewFastRowApplyColorSSE256(canvas, width, color,alpha256);

            canvas += canvasWidth;
        }
    }
}

void FastFunctions::RenderTriangleByRanges(unsigned char * canvas, int canvasWidth, short int * ranges, int rlen,int startY, int color, int alpha)
{
    /*int rowStartIndex = (startY) * canvasWidth;
    //startY *= 2;
    for (int i = 0; i < rlen; i += 2)
    {
    int index = rowStartIndex + (ranges[i]) * 4;
    int count = ranges[i+1] -  ranges[i]+1;

    NewFastRowApplyColorSSE64(canvas+index, count, color, alpha);

    rowStartIndex += canvasWidth;
    }*/

	int alpha256 = ((color) >> 24) & 0xff;
	if (alpha256 == 0xff)  alpha256 += 1;

    canvas += (startY) * canvasWidth;
    //startY *= 2;
    for (int i = 0; i < rlen; i += 2)
    {
        int index = (ranges[i]) * 4;
        int count = ranges[i+1] -  ranges[i]+1;

        AlphaBlending::NewFastRowApplyColorSSE64(canvas+index, count, color,alpha256);

        canvas += canvasWidth;
    }

    /*canvas += (startY) * canvasWidth;

    for (int i = 0; i < rlen-4; i += 4)
    {
    int range = ranges[i];
    int index =  (range) * 4;
    //int index = rowStartIndex + (range+range+range+range);
    int count = ranges[i+1] -  range+1;

    NewFastRowApplyColorSSE64(canvas+index, count, color, alpha);

    canvas += canvasWidth;
    index = (ranges[i+2]) * 4;
    count = ranges[i+1+2] -  ranges[i+2]+1;

    NewFastRowApplyColorSSE64(canvas+index, count, color, alpha);

    canvas +=canvasWidth;

    }

    if(rlen&3 != 0)
    {
    int tmp = rlen-2;
    int index = (ranges[tmp]) * 4;
    int count = ranges[tmp+1] -  ranges[tmp]+1;

    NewFastRowApplyColorSSE64(canvas+index, count, color, alpha);

    }*/


}



void FastFunctions::RenderTriangle(unsigned char * canvas, int canvasWidth,int canvasHeight, 
                                   short int px0,short int py0,short int px1,short int py1,short int px2,short int py2, int color)
{
    int v0x,v1x,v2x,v0y,v1y,v2y;

    v0x = px1 - px0;
    v0y = py1 - py0;

    v1x = px2 - px1;
    v1y = py2 - py1;

    v2x = px0 - px2;
    v2y = py0 - py2;

	int alpha256 = ((color) >> 24) & 0xff;
	if (alpha256 == 0xff)  alpha256 += 1;

   
    int invAlpha = 256 - alpha256;

    int b = ((color)&0xff) * alpha256;
    int g = ((color>>8)&0xff) * alpha256;
    int r = ((color>>16)&0xff) * alpha256;
    // process all points
    int rowIndex = 0;

    int sz1 =  (0 - px0) * v0y - (0 - py0) * v0x;
    int sz2 =  (0 - px1) * v1y - (0 - py1) * v1x;
    int sz3 =  (0 - px2) * v2y - (0 - py2) * v2x;
    for (int y = 0; y < canvasHeight; y++)
    {
        int z1 = sz1;
        int z2 = sz2;
        int z3 = sz3;


        int currIndex = rowIndex;
        for (int x = 0; x < canvasWidth; x+=4)
        {
            if((((z1^z2)|(z1^z3))>>31) == 0)


                //if ((z1 * z2 > 0) && (z1 * z3 > 0))
            {
                //int index = (canvas.Width * y) + x * 4;


                //ApplyColorPixelSSE(canvas+currIndex,color,alpha);


                int tb = canvas[currIndex];
                int tg = canvas[currIndex + 1];
                int tr = canvas[currIndex + 2];


                canvas[currIndex] = (unsigned char)((b + (tb * invAlpha)) >> 8);
                canvas[currIndex+1] = (unsigned char)((g + (tg * invAlpha)) >> 8);
                canvas[currIndex+2] = (unsigned char)((r + (tr * invAlpha)) >> 8);



            }

            z1 += v0y;
            z2 += v1y;
            z3 += v2y;
            currIndex += 4;
        }

        sz1 -= v0x;
        sz2 -= v1x;
        sz3 -= v2x;

        rowIndex += canvasWidth;

    }
}

void FastFunctions::RenderTriangleNew(unsigned char * canvas, int canvasWidth,int canvasHeight, 
                                      short int px0,short int py0,short int px1,short int py1,short int px2,short int py2, int color)
{
    int rgba = color;
	int alpha256 = ((color) >> 24) & 0xff;
	if (alpha256 == 0xff)  alpha256 += 1;


    int v0x,v1x,v2x,v0y,v1y,v2y,v0c,v1c,v2c;

    v0x = px1 - px0;
    v0y = py1 - py0;

    v1x = px2 - px1;
    v1y = py2 - py1;

    v2x = px0 - px2;
    v2y = py0 - py2;

    int tmp = 0;
    tmp = v0x;v0x=v0y;v0y=tmp;
    tmp = v1x;v1x=v1y;v1y=tmp;
    tmp = v2x;v2x=v2y;v2y=tmp;

    /*Tools.swap<int>(ref v0x, ref v0y);
    Tools.swap<int>(ref v1x, ref v1y);
    Tools.swap<int>(ref v2x, ref v2y);*/

    /*if (v0x < 0) { v0x = -v0x; } else { v0y = -v0y; }
    if (v1x < 0) { v1x = -v1x; } else { v1y = -v1y; }
    if (v2x < 0) { v2x = -v2x; } else { v2y = -v2y; }
    */
    v0x = -v0x;
    v1x = -v1x;
    v2x = -v2x;


    v0c = -(v0x*px0+v0y*py0);
    v1c = -(v1x*px1+v1y*py1);
    v2c = -(v2x*px2+v2y*py2);


    // process all points
    int minY = (py0 < py1) ? py0 : py1;
    minY = (minY < py2) ? minY : py2;
    int maxY = (py0 > py1) ? py0 : py1;
    maxY = (maxY > py2) ? maxY : py2;

    int rowIndex = minY * canvasWidth;

    for (int y = minY; y <= maxY; y++)
    {
        // compute ax+by+c =0, v(a,b) , u(k,l)=A-B, u(k,l) => v(l,-k)
        //int tmpx0 = (v0x == 0)? px0 : (-v0y * y - v0c) / v0x;
        //int tmpx1 = (v1x == 0) ? px2 : (-v1y * y - v1c) / v1x;
        //int tmpx2 = (v2x == 0) ? px2 : (-v2y * y - v2c) / v2x;

        int start = 0;
        int end = 0;

        int isCrossLine0 = (py0 == py1)? -1 : (y - py0) * (py1 - y);
        //int isCrossLine1 = (py1 == py2) ? -1 : (y - py1) * (py2 - y);
        //int isCrossLine2 = (py2 == py0) ? -1 : (y - py2) * (py0 - y);

        if (isCrossLine0 >= 0)
        {

            int isCrossLine1 = (py1 == py2
                ||
                (y == py1 &&  py1 > minY && py1<maxY  )
                ) ? -1 : (y - py1) * (py2 - y);

            if (isCrossLine1 >= 0)
            {
                int tmpx0 = (v0x == 0) ? px0 : (-v0y * y - v0c) / v0x;
                int tmpx1 = (v1x == 0) ? px2 : (-v1y * y - v1c) / v1x;

                start = tmpx0;
                end = tmpx1;
            }
            else
            {
                int tmpx0 = (v0x == 0) ? px0 : (-v0y * y - v0c) / v0x;
                int tmpx2 = (v2x == 0) ? px2 : (-v2y * y - v2c) / v2x;

                start = tmpx0;
                end = tmpx2;

            }
        }
        else
        {
            int tmpx1 = (v1x == 0) ? px2 : (-v1y * y - v1c) / v1x;
            int tmpx2 = (v2x == 0) ? px2 : (-v2y * y - v2c) / v2x;
            start = tmpx1;
            end = tmpx2;
        }

        if (start > end)
        {
            tmp = start;start=end;end=tmp;
            //Tools.swap<int>(ref start, ref end);
        }



        int currIndex = rowIndex+start*4;

        AlphaBlending::NewFastRowApplyColorSSE64(canvas+currIndex,end- start + 1,rgba,alpha256);

        rowIndex += canvasWidth;
    }
}

void FastFunctions::RenderTriangleNewOptimize(unsigned char * const canvas,const int canvasWidth,const int canvasHeight, 
                                              short int px0,short int py0,short int px1,short int py1,short int px2,short int py2, const int color)
{

    //int alpha255 = (color >> 24) & 0xff;
	int alpha256 = ((color) >> 24) & 0xff;
	if (alpha256 == 0xff)  alpha256 += 1;
    

    if (py0 > py1)
    {
        short tmp;
        SWAP(tmp,py0,py1);
        SWAP(tmp,px0,px1);
    }
    if (py1 > py2)
    {

        short tmp;
        SWAP(tmp,py1,py2);
        SWAP(tmp,px1,px2);
    }

    if (py0 > py1)
    {
        short tmp;
        SWAP(tmp,py0,py1);
        SWAP(tmp,px0,px1);
    }

    // compute vector and ax+by+c, compute vector (a,b) a coeficient c
    int v01x,v20x,v01y,v20y,v01c,v20c;

    //v01x = px1 - px0;
    //v01y = py1 - py0;

    //v20x = px0 - px2;
    //v20y = py0 - py2;

    //Tools.swap<int>(ref v01x, ref v01y);
    //Tools.swap<int>(ref v20x, ref v20y);

    v01x = -(py1 - py0);
    v01y = px1 - px0;

    v20x = -(py0 - py2);
    v20y = px0 - px2;

    v01c = -(v01x * px0 + v01y * py0);
    v20c = -(v20x * px2 + v20y * py2);

    int middleY = py1;
    int rowIndex = py0 * canvasWidth;

    int tmpNominal0 = (-v01y * py0 - v01c);
    int tmpNominal2 = (-v20y * py0 - v20c);

    // fill first half
    for (int y = py0; y < middleY; y++)
    {
        //int tmpx0 =  (-v01y * y - v01c) / v01x;
        //int tmpx2 =  (-v20y * y - v20c) / v20x;

        //int tmpx0 =  tmpNominal0 / v01x;
        //int tmpx2 =  tmpNominal2 / v20x;

        int tmpx0 =  tmpNominal0*2 / v01x ;
        tmpx0 = tmpx0/2 + (tmpx0&1);
        int tmpx2 = tmpNominal2*2 / v20x;
        tmpx2 = tmpx2/2 + (tmpx2&1);

        tmpNominal0 += -v01y;
        tmpNominal2 += -v20y;


        int start = tmpx0;

        int end = tmpx2;

        if (start > end)
        {
            int tmp;
            SWAP(tmp,start,end);
        }

        //if (end >= canvas.WidthPixel)
        //{
        //    int kkk = 454;
        //    throw new Exception();
        //    continue;
        //}


        int currIndex = rowIndex + start * 4;

        AlphaBlending::NewFastRowApplyColorSSE64(canvas+currIndex,end- start + 1,color,alpha256);

        rowIndex += canvasWidth;
    }

    //int v12x = px2 - px1;
    //int v12y = py2 - py1;
    //Tools.swap<int>(ref v12x, ref v12y);
    int v12x = py2 - py1;
    int v12y = px2 - px1;

    v12x = -v12x;
    int v12c = -(v12x * px1 + v12y * py1);

    //rowIndex = middleY * canvasWidth;

    // osetreni specialniho pripadu kdy prostredni bod je v jedne lajne s spodnim
    if (middleY == py2)
    {
        int start = px1;
        int end = px2;

        if (start > end)
        {
            int tmp;
            SWAP(tmp,start,end);
        }

        int currIndex = rowIndex + start * 4;

        AlphaBlending::NewFastRowApplyColorSSE64(canvas+currIndex,end- start + 1,color,alpha256);

        rowIndex += canvasWidth;

        middleY++;
    }

    int tmpNominal1 = (-v12y * middleY - v12c);
    tmpNominal2 = (-v20y * middleY - v20c);

    /*int tmp = (-v20y * middleY - v20c) / v20x;

    if (px1 > tmp ) 
    {
    int tmp = 0;
    SWAP(tmp,tmpNominal1,tmpNominal2);
    SWAP(tmp,v20x,v12x);
    SWAP(tmp,v20y,v12y);

    }*/

    // fill first half
    for (int y = middleY; y <= py2; y++)
    {
        //int tmpx1 =  tmpNominal1 / v12x;
        //int tmpx2 = tmpNominal2 / v20x;

        int tmpx1 =  tmpNominal1*2 / v12x ;
        tmpx1 = tmpx1/2 + (tmpx1&1);
        int tmpx2 = tmpNominal2*2 / v20x;
        tmpx2 = tmpx2/2 + (tmpx2&1);

        tmpNominal1 += -v12y;
        tmpNominal2 += -v20y;


        int start = tmpx1;
        int end = tmpx2;

        if (start > end) 
        {
            int tmp;
            SWAP(tmp,start,end);
        }

        //if (end >= canvas.WidthPixel)
        //{

        //    int kkk = 454;
        //    throw new Exception();
        //    continue;
        //}


        int currIndex = rowIndex + start * 4;
        AlphaBlending::NewFastRowApplyColorSSE64(canvas+currIndex,end- start + 1,color,alpha256);

        rowIndex += canvasWidth;
    }
}


void FillSSEInt32(unsigned long * M, long Fill, unsigned int CountFill)
{
    __m128i f;
	
	unsigned int index = 0;

    unsigned int tmpCount = 4-((((unsigned int)M)&0xf)/4);
    if(tmpCount < 4 && tmpCount < CountFill)
    {
        switch (tmpCount)
        {
        case 0x3: {*M = Fill;M[1] = Fill;M[2] = Fill;break;}
        case 0x2: {*M = Fill;M[1] = Fill;break;}
        case 0x1: {*M = Fill;break;}
        }

        index+=tmpCount;
        CountFill -= tmpCount;
    }


    f = _mm_set1_epi32(Fill);

    
    while (CountFill >= 16)
    { 
        _mm_store_si128((__m128i *)(M+index), f);
        _mm_store_si128((__m128i *)(M+index+4), f);
        _mm_store_si128((__m128i *)(M+index+8), f);
        _mm_store_si128((__m128i *)(M+index+12), f);
        //_mm_store_si128((__m128i *)(M+index+16), f);
        //_mm_store_si128((__m128i *)(M+index+20), f);
        //_mm_store_si128((__m128i *)(M+index+24), f);
        //_mm_store_si128((__m128i *)(M+index+28), f);
        //_mm_store_si128((__m128i *)M2, f);
        //_mm_stream_si128((__m128i *)M, f);
        //_mm_stream_si128((__m128i *)(M+4), f);
        //M += 16;

        index += 16;
        CountFill -= 16;
    }

    while (CountFill >= 4)
    {
        _mm_store_si128((__m128i *)(M+index), f);
        //_mm_stream_si128((__m128i *)M, f);
        
		index += 4;
        CountFill -= 4;
    }

    /*while (CountFill >= 4)
    {
        M[0] = Fill;
        M[1] = Fill;
        M[2] = Fill;
        M[3] = Fill;

        //_mm_storeu_si128((__m128i *)M, f);
        //_mm_stream_si128((__m128i *)M, f);
        M += 4;
        CountFill -= 4;
    }*/

    
    if(CountFill > 0)
    {
        switch (CountFill )
        {
        case 0x3: {M[index] = Fill;M[index+1] = Fill;M[index+2] = Fill;break;}
        case 0x2: {M[index] = Fill;M[index+1] = Fill;break;}
        case 0x1: {M[index] = Fill;break;}
        }
    }
}

void FillSSEInt32test(unsigned long * M, long Fill, unsigned int CountFill)
{
    __m128i f;

    // Fix mis-alignment.
    /*if (((unsigned int)M) & 0xf)
    {
    unsigned int tmp = ((unsigned int)M) & 0xf; 

    switch (tmp)
    {
    case 0x4: if (CountFill >= 1) { *M++ = Fill; CountFill--; }
    case 0x8: if (CountFill >= 1) { *M++ = Fill; CountFill--; }
    case 0xc: if (CountFill >= 1) { *M++ = Fill; CountFill--; }
    }
    }*/

    /*int tmpCount = 4-((((unsigned int)M)&0xf)/4);
    if(tmpCount < 4 && tmpCount < CountFill)
    {
        switch (tmpCount)
        {
        case 0x3: {*M = Fill;M[1] = Fill;M[2] = Fill;break;}
        case 0x2: {*M = Fill;M[1] = Fill;break;}
        case 0x1: {*M = Fill;break;}
        }

        M+=tmpCount;
        CountFill -= tmpCount;
    }*/


    f = _mm_set1_epi32(Fill);

    /*int index = 0;
    while (CountFill >= 16)
    { 
        _mm_store_si128((__m128i *)(M+index), f);
        _mm_store_si128((__m128i *)(M+index+4), f);
        _mm_store_si128((__m128i *)(M+index+8), f);
        _mm_store_si128((__m128i *)(M+index+12), f);
        //_mm_store_si128((__m128i *)(M+index+16), f);
        //_mm_store_si128((__m128i *)(M+index+20), f);
        //_mm_store_si128((__m128i *)(M+index+24), f);
        //_mm_store_si128((__m128i *)(M+index+28), f);
        //_mm_store_si128((__m128i *)M2, f);
        //_mm_stream_si128((__m128i *)M, f);
        //_mm_stream_si128((__m128i *)(M+4), f);
        //M += 16;

        index += 16;
        CountFill -= 16;
    }

    M += index;*/



    /*while (CountFill >= 4)
    {
        _mm_storeu_si128((__m128i *)M, f);
        //_mm_stream_si128((__m128i *)M, f);
        M += 4;
        CountFill -= 4;
    }*/

    while (CountFill >= 4)
    {
        //M[0] = Fill;
        //M[1] = Fill;
        //M[2] = Fill;
        //M[3] = Fill;

        _mm_storeu_si128((__m128i *)M, f);
        //_mm_stream_si128((__m128i *)M, f);
        M += 4;
        CountFill -= 4;
    }

    
    if(CountFill > 0)
    {
        switch (CountFill )
        {
        case 0x3: {*M = Fill;M[1] = Fill;M[2] = Fill;break;}
        case 0x2: {*M = Fill;M[1] = Fill;break;}
        case 0x1: {*M = Fill;break;}
        }
    }
}

void FastFunctions::ClearFieldByColorInt(unsigned char * curr, int lengthPixel, int color)
{
    //for(int i =0;i<10000;i++)
    {
        FillSSEInt32((unsigned long *)curr, color,lengthPixel);
		
    }
    //std::fill((unsigned int *)curr,((unsigned int *)curr)+length/4,  color);
}






void FastFunctions::RenderOneRow(int * listRowsForApply, int countRows, unsigned char * canvas)
{
    ClearFieldByColorInt(canvas
        +listRowsForApply[0]*4
        ,listRowsForApply[1],listRowsForApply[2]);

    int end  = countRows*3+3;
    //int index = 3;
    for(int index = 3;index < end;index+=3)
    {
        int color = listRowsForApply[index + 2];
		int alpha256 = ((color) >> 24) & 0xff;
		if (alpha256 == 0xff)  alpha256 += 1;


        AlphaBlending::NewFastRowApplyColorSSE64(canvas+
            listRowsForApply[index] * 4, listRowsForApply[index + 1], color,alpha256);

        //index +=3;
    }
}




__int64 FastFunctions::computeFittnessWithStdDev_ARGB(unsigned char * curr, unsigned char * orig, int length)
{
    NativeMedian8Bit medR = NativeMedian8Bit();
    NativeMedian8Bit medG = NativeMedian8Bit();
    NativeMedian8Bit medB = NativeMedian8Bit();


    //int index = 0;
    //while (index < length)
    for(int index = 0;index < length;index+=4)
    {
        medR.InsertData((unsigned char)labs(curr[index] - orig[index]));
        medG.InsertData((unsigned char)labs(curr[index + 1] - orig[index + 1]));
        medB.InsertData((unsigned char)labs(curr[index + 2] - orig[index + 2]));

        /*  int tmp = (labs(curr[index] - orig[index])>>2) +
        (labs(curr[index+1] - orig[index+1])>>2) +
        (labs(curr[index+2] - orig[index+2])>>2) ;
        medB.InsertData(tmp);*/




        //  index += 4;
    }


    __int64 result = 0;
    result += (medR.ValueSum() + medR.SumStdDev()+
        medG.ValueSum() + medG.SumStdDev()+
        medB.ValueSum() + medB.SumStdDev());


    //result += (medB.Median()) + medB.SumStdDev());

    // result += (medG.ValueSum() + medG.SumStdDev());
    // result += (medR.ValueSum() + medR.SumStdDev());

    return result;


}

__int64 FastFunctions::computeFittnessSumSquare_ARGB(unsigned char * curr, unsigned char * orig, int length)
{

    __int64 result = 0;


    //int index = 0;
    //while (index < length)
    for(int index = 0;index < length;index+=4)
    {
        int br = curr[index] - orig[index];
        int bg = curr[index + 1] - orig[index + 1];
        int bb = curr[index + 2] - orig[index + 2];

        result += br*br+bg*bg + bb*bb;

        //  index += 4;
    }

    return result;
}

__int64 FastFunctions::computeFittnessSumSquareASM_ARGB( unsigned char* curr, unsigned char* orig, int count )
{
    __int64 result = 0;

    __m128i mMaskGAAnd = _mm_set1_epi16(0xff00);
    __m128i mMaskEven = _mm_set1_epi32(0xffff);
    __m128i mResult = _mm_setzero_si128();

    int c = 1000;

    while(count > 15)
    {

        __m128i colors = _mm_loadu_si128((__m128i*)curr);
        __m128i colors2 = _mm_loadu_si128((__m128i*)orig);
        /*_mm_prefetch((char *)curr+16,_MM_HINT_T0);
        _mm_prefetch((char *)orig+16,_MM_HINT_T0);*/

        __m128i tmp1 = _mm_min_epu8(colors,colors2);
        __m128i tmp2 = _mm_max_epu8(colors,colors2);
        tmp1 = _mm_subs_epu8(tmp2,tmp1);

        tmp2 = _mm_and_si128(tmp1,mMaskGAAnd);  // masked  xxgxxxgxxxgxxxgx
        tmp1 = _mm_andnot_si128(mMaskGAAnd,tmp1);

        tmp2 =  _mm_srli_epi16(tmp2,8);
        tmp1 = _mm_mullo_epi16(tmp1,tmp1);
        tmp2 = _mm_mullo_epi16(tmp2,tmp2);

        __m128i tmp3 = _mm_and_si128(tmp1,mMaskEven);
        mResult = _mm_add_epi32(tmp3,mResult);
        tmp1 = _mm_srli_epi32(tmp1,16);
        mResult = _mm_add_epi32(tmp1,mResult);

        tmp3 = _mm_and_si128(tmp2,mMaskEven);
        mResult = _mm_add_epi32(tmp3,mResult);



        c--;
        if(c == 0)
        {
            result += _mm_extract_epi32(mResult,0)+ _mm_extract_epi32(mResult,1)+
				_mm_extract_epi32(mResult,2)+ _mm_extract_epi32(mResult,3);
			
            mResult = _mm_setzero_si128();
            c = 1000;
        }
        //      result += tmp1.m128i_u16[0]+tmp1.m128i_u16[2]+tmp1.m128i_u16[3]+tmp1.m128i_u16[4]+tmp1.m128i_u16[5]+
        //         tmp1.m128i_u16[6]+tmp1.m128i_u16[7]+tmp2.m128i_u16[0]+tmp2.m128i_u16[4];

        count-=16;
        curr+=16;
        orig+=16;

    }

	result += _mm_extract_epi32(mResult, 0) + _mm_extract_epi32(mResult, 1) +
		_mm_extract_epi32(mResult, 2) + _mm_extract_epi32(mResult, 3);

    //result += mResult.m128i_u32[0]+mResult.m128i_u32[1]+mResult.m128i_u32[2]+mResult.m128i_u32[3];


    while(count > 3)
    {
        int br = curr[0] - orig[0];
        int bg = curr[1] - orig[1];
        int bb = curr[2] - orig[2];

        result += br*br+bg*bg + bb*bb;

        count-=4;
        curr+=4;
        orig+=4;
        //  index += 4;
    }


    return result;

}


__int64 FastFunctions::computeFittnessSumABS_ARGB(unsigned char * curr, unsigned char * orig, int length)
{

    __int64 result = 0;


    //int index = 0;
    //while (index < length)
    for(int index = 0;index < length;index+=4)
    {
        int br = curr[index] - orig[index];
        int bg = curr[index + 1] - orig[index + 1];
        int bb = curr[index + 2] - orig[index + 2];
        //int ba = curr[index + 3] - orig[index + 3];

        result += labs(br)+labs(bg) + labs(bb);// + labs(ba);

        //  index += 4;
    }

    return result;
}

__int64 FastFunctions::computeFittnessSumABSASM_ARGB( unsigned char* curr, unsigned char* orig, int count )
{
    __int64 result = 0;

    //__m128i mMaskGAAnd = _mm_set1_epi16(0xff00);
    //__m128i mMaskEven = _mm_set1_epi32(0xffff);
    __m128i mResult = _mm_setzero_si128();
    __m128i mResult2 = _mm_setzero_si128();
    //__m128i mzero = _mm_setzero_si128();
    __m128i mMask = _mm_set1_epi32(0xffffff);


    int c = 1000;

    /*while(count > 15)
    {
    __m128i colors = _mm_loadu_si128((__m128i*)curr);
    __m128i colors2 = _mm_loadu_si128((__m128i*)orig);
    _mm_prefetch((char *)curr+16,_MM_HINT_T0);
    _mm_prefetch((char *)orig+16,_MM_HINT_T0);

    colors = _mm_and_si128(colors,mMask);
    colors2 = _mm_and_si128(colors2,mMask);
    __m128i tmp1 = _mm_sad_epu8(colors,colors2);
    mResult = _mm_add_epi64(mResult,tmp1);

    count-=16;
    curr+=16;
    orig+=16;

    }*/

    while(count > 31)
    {
        __m128i colors = _mm_loadu_si128((__m128i*)curr);
        __m128i colors2 = _mm_loadu_si128((__m128i*)orig);
        __m128i colors3 = _mm_loadu_si128((__m128i*)(curr+16));
        __m128i colors4 = _mm_loadu_si128((__m128i*)(orig+16));
        //_mm_prefetch((char *)curr+32,_MM_HINT_T0);
        //_mm_prefetch((char *)orig+32,_MM_HINT_T0);

        colors = _mm_and_si128(colors,mMask);
        colors2 = _mm_and_si128(colors2,mMask);


        colors3 = _mm_and_si128(colors3,mMask);
        colors4 = _mm_and_si128(colors4,mMask);
        __m128i tmp1 = _mm_sad_epu8(colors,colors2);
        __m128i tmp2 = _mm_sad_epu8(colors3,colors4);
        mResult = _mm_add_epi64(mResult,tmp1);
        mResult2 = _mm_add_epi64(mResult2,tmp2);

        count-=32;
        curr+=32;
        orig+=32;

    }

    result += mResult.m128i_u64[0]+mResult.m128i_u64[1];//+mResult.m128i_u32[1]+mResult.m128i_u32[2]+mResult.m128i_u32[3];
    result += mResult2.m128i_u64[0]+mResult2.m128i_u64[1];

    while(count > 3)
    {
        int br = curr[0] - orig[0];
        int bg = curr[1] - orig[1];
        int bb = curr[2] - orig[2];

        result += labs(br)+labs(bg) + labs(bb);

        count-=4;
        curr+=4;
        orig+=4;
        //  index += 4;
    }


    return result;

}

__int64 FastFunctions::computeFittnessSumABSASM( unsigned char* curr, unsigned char* orig, int count )
{
    __int64 result = 0;

    __m128i mResult = _mm_setzero_si128();
    __m128i mResult2 = _mm_setzero_si128();
    

    int c = 1000;

    while(count > 31)
    {
        __m128i colors = _mm_loadu_si128((__m128i*)curr);
        __m128i colors2 = _mm_loadu_si128((__m128i*)orig);
        __m128i colors3 = _mm_loadu_si128((__m128i*)(curr+16));
        __m128i colors4 = _mm_loadu_si128((__m128i*)(orig+16));
        //_mm_prefetch((char *)curr+32,_MM_HINT_T0);
        //_mm_prefetch((char *)orig+32,_MM_HINT_T0);

        __m128i tmp1 = _mm_sad_epu8(colors,colors2);
        __m128i tmp2 = _mm_sad_epu8(colors3,colors4);
        mResult = _mm_add_epi64(mResult,tmp1);
        mResult2 = _mm_add_epi64(mResult2,tmp2);

        count-=32;
        curr+=32;
        orig+=32;

    }

    result += mResult.m128i_u64[0]+mResult.m128i_u64[1];//+mResult.m128i_u32[1]+mResult.m128i_u32[2]+mResult.m128i_u32[3];
    result += mResult2.m128i_u64[0]+mResult2.m128i_u64[1];

    while(count > 0)
    {
        int b = curr[0] - orig[0];
        
        result += labs(b);

        count--;
        curr++;
        orig++;
    }


    return result;

}


