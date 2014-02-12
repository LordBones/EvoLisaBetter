// ForDebuging.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <math.h>
#include <emmintrin.h>
#include <smmintrin.h>


void TestBlendColor128()
{
    // implementace michani barev podle vzorce
    // (color*alpha + (255-alpha)*source)/255
    // vzorec pro rychle deleni 255 = (value+1 + (value >> 8)) >> 8;

    unsigned char field[] = {13,14,15,255,13,14,15,255,13,14,15,255,13,14,15,255};



    int r = 7;
    int g = 8;
    int b = 254;
    int alpha = 255;
    int invAlpha = 255 - alpha;


    __m128i mColorRBTimeAlpha = _mm_setr_epi16(b*alpha,r*alpha,b*alpha,r*alpha,b*alpha,r*alpha,b*alpha,r*alpha);
    __m128i mColorGTimeAlpha = _mm_setr_epi16(g*alpha,0,g*alpha,0,g*alpha,0,g*alpha,0);


    __m128i mMullInvAlpha = _mm_set1_epi16(invAlpha);
    __m128i mMaskGAnd = _mm_setr_epi8(0,0xff,0,0,0,0xff,0,0,0,0xff,0,0,0,0xff,0,0);
    __m128i mMaskAAnd = _mm_setr_epi8(0,0,0,0xff,0,0,0,0xff,0,0,0,0xff,0,0,0,0xff);

    __m128i sourceRB = _mm_loadu_si128((__m128i*)field); // load    ArgbArgbArgbArgb
    __m128i sourceG = _mm_and_si128(sourceRB,mMaskGAnd);  // masked  xxgxxxgxxxgxxxgx
    sourceRB = _mm_andnot_si128(mMaskGAnd,sourceRB);
    __m128i savealpha = _mm_and_si128(sourceRB,mMaskAAnd);
    sourceRB = _mm_andnot_si128(mMaskAAnd,sourceRB);


    sourceG = _mm_srli_epi16(sourceG,8);                 // shift for compute    xxxgxxxgxxxgxxxg
    // in source is now  xrxbxrxbxrxbxrxb
    sourceRB = _mm_mullo_epi16(sourceRB,mMullInvAlpha);    // sourceRB <= sourceRB*invAlpha
    sourceG = _mm_mullo_epi16(sourceG,mMullInvAlpha);    // sourceG <= sourceG*invAlpha
    sourceRB = _mm_adds_epu16(sourceRB,mColorRBTimeAlpha); // sourceRB <= sourceRB+(colorRB*Alpha)
    sourceG = _mm_adds_epu16(sourceG,mColorGTimeAlpha); // sourceG <= sourceG+(colorG*Alpha)

    // fast div 255  = (t+1+(t>>8))>>8
    __m128i tmpRB = _mm_adds_epu16(sourceRB,_mm_set1_epi16(1));    // tmpRB = sourceRB +1
    __m128i tmpG = _mm_adds_epu16(sourceG,_mm_set1_epi16(1));      // tmpG = sourceG +1
    sourceRB = _mm_srli_epi16(sourceRB,8);                         // sourceRB = sourceRB >> 8
    sourceG = _mm_srli_epi16(sourceG,8);                           // sourceG = sourceG >> 8
    sourceRB = _mm_adds_epu16(sourceRB,tmpRB);                     // sourceRB = sourceRB + tmpsourceRB
    sourceG = _mm_adds_epu16(sourceG,tmpG);                        // sourceG = sourceG + tmpsourceG
    sourceRB = _mm_srli_epi16(sourceRB,8);                         // now in sourceRB is sourceRB/255
    sourceG = _mm_srli_epi16(sourceG,8);                           // now in sourceG is sourceG/255

    // now merge back xrxb.... xxxg.... into xrgb....
    sourceG = _mm_slli_epi16(sourceG,8);
    sourceRB =_mm_or_si128(sourceRB,sourceG);         // now in sourceRB is xrgbxrgb....
    sourceRB = _mm_or_si128(sourceRB,savealpha); // restore alpha now argb....
    _mm_storeu_si128((__m128i*)field,sourceRB);

}



void TestSumSquare()
{
    unsigned char field1[] = {2,3,4,250,2,3,4,250,2,3,4,250,2,3,4,250};
    unsigned char field2[] = {3,4,5,255,3,4,5,255,3,4,5,255,3,4,5,255};

    __m128i mMaskGAAnd = _mm_setr_epi8(0,0xff,0,0xff,0,0xff,0,0xff,0,0xff,0,0xff,0,0xff,0,0xff);
    __m128i mMaskEven = _mm_setr_epi16(0xffff,0,0xffff,0,0xffff,0,0xffff,0);
    __m128i mResult = _mm_setzero_si128();


    __m128i colors = _mm_loadu_si128((__m128i*)field1);
    __m128i colors2 = _mm_loadu_si128((__m128i*)field2);
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



    __int64 result = mResult.m128i_u32[0]+mResult.m128i_u32[1]+mResult.m128i_u32[2]+mResult.m128i_u32[3];



}

void TestSumABS()
{
    unsigned char field1[] = {2,3,4,250,2,3,4,250,2,3,4,250,2,3,4,250};
    unsigned char field2[] = {3,4,5,255,3,4,5,255,3,4,5,255,3,4,5,255};

    __int64 result = 0;

    __m128i mMaskGAAnd = _mm_set1_epi16(0xff00);
    __m128i mMaskEven = _mm_set1_epi32(0xffff);
    __m128i mResult = _mm_setzero_si128();

    int c = 1000;
    int count = 16;
    int index = 0;
    while(count > 15)
    {

        __m128i colors = _mm_loadu_si128((__m128i*)field1+index);
        __m128i colors2 = _mm_loadu_si128((__m128i*)field2+index);
        /*_mm_prefetch((char *)curr+16,_MM_HINT_T0);
        _mm_prefetch((char *)orig+16,_MM_HINT_T0);*/

        __m128i tmp1 = _mm_min_epu8(colors,colors2);
        __m128i tmp2 = _mm_max_epu8(colors,colors2);
        tmp1 = _mm_subs_epu8(tmp2,tmp1);



        tmp2 = _mm_and_si128(tmp1,mMaskGAAnd);  // masked  xxgxxxgxxxgxxxgx
        tmp2 = _mm_and_si128(tmp2,mMaskEven);
        tmp2 =  _mm_srli_epi16(tmp2,8);
        tmp1 = _mm_andnot_si128(mMaskGAAnd,tmp1);

        //tmp2 = _mm_add_epi16(tmp2,tmp1);
        tmp2 = _mm_hadds_epi16(tmp2,tmp1);
        tmp2 = _mm_hadds_epi16(tmp2,_mm_setzero_si128());
        tmp2 = _mm_cvtepu16_epi32(tmp2);

        mResult = _mm_add_epi32(tmp2,mResult);

        /*__m128i tmp3 = _mm_and_si128(tmp1,mMaskEven);
        mResult = _mm_add_epi32(tmp3,mResult);
        tmp1 = _mm_srli_epi32(tmp1,16);
        mResult = _mm_add_epi32(tmp1,mResult);

        tmp3 = _mm_and_si128(tmp2,mMaskEven);
        mResult = _mm_add_epi32(tmp3,mResult);*/



        c--;
        if(c == 0)
        {
            result += mResult.m128i_u32[0]+mResult.m128i_u32[1]+mResult.m128i_u32[2]+mResult.m128i_u32[3];
            mResult = _mm_setzero_si128();
            c = 1000;
        }
        //      result += tmp1.m128i_u16[0]+tmp1.m128i_u16[2]+tmp1.m128i_u16[3]+tmp1.m128i_u16[4]+tmp1.m128i_u16[5]+
        //         tmp1.m128i_u16[6]+tmp1.m128i_u16[7]+tmp2.m128i_u16[0]+tmp2.m128i_u16[4];

        count-=16;
        index+=16;

    }

    result += mResult.m128i_u32[0]+mResult.m128i_u32[1]+mResult.m128i_u32[2]+mResult.m128i_u32[3];


    while(count > 3)
    {
        int br = field1[index] - field2[index];
        int bg = field1[index+1] - field2[index+1];
        int bb = field1[index+2] - field2[index+2];

        result += labs(br)+labs(bg) + labs(bb);

        count-=4;
        index+=4;
        //curr+=4;
        //orig+=4;
        //  index += 4;
    }


    //return result;


}

//void ApplyColorPixelSSE(unsigned char * canvas,int r,int g, int b, int alpha)
//  {
//      int invAlpha = 256 - alpha;
//
//       unsigned long long tmpColor = ((unsigned long long)(r*alpha)<<32) | ((unsigned long long)(g*alpha)<<16) | (b*alpha);
//       __m128i mColorTimeAlpha = _mm_setzero_si128();
//       mColorTimeAlpha.m128i_u64[0] = tmpColor;
//       mColorTimeAlpha.m128i_u64[1] = tmpColor;
//
//      __m128i mColorTimeAlpha2 = _mm_setr_epi16(b*alpha,g*alpha,r*alpha,0,b*alpha,g*alpha,r*alpha,0);
//
//      __m128i mMullInvAlpha = _mm_set1_epi16(invAlpha);
//      __m128i source = _mm_cvtsi32_si128(*((int*)canvas));
//
//      //source = _mm_unpacklo_epi8(source, _mm_setzero_si128() );
//      source = _mm_cvtepu8_epi16(source);
//
//      __m128i tmp1  = _mm_mullo_epi16(source,mMullInvAlpha);    // source*invalpha
//      tmp1          = _mm_adds_epu16(tmp1,mColorTimeAlpha);     // t
//
//      tmp1          = _mm_srli_epi16(tmp1,8); 
//
//      source        = _mm_blend_epi16(tmp1,source,0x88);        // a,b,c,d  | e,f,g,h => a,b,c,h
//
//      //source        = _mm_andnot_si128(mMaskAnd,source);        // mask alpha
//      //tmp2          = _mm_and_si128(mMaskAnd,tmp2);             // mask colors
//      //source        = _mm_or_si128(tmp2,source);                // 00XXXXXX | XX000000 = xxxxxxxx
//
//      source        = _mm_packus_epi16(source, _mm_setzero_si128() );         // pack
//
//      *((int*)canvas) =  _mm_cvtsi128_si32(source);
//
//  }

void Apply4ColorPixelSSE2(unsigned char * canvas,int count,int color, int alpha)
{

    int invAlpha = 256 - alpha;

    __m128i mColorTimeAlpha = _mm_set1_epi32(color&0xffffff);

    mColorTimeAlpha = _mm_cvtepu8_epi16(mColorTimeAlpha);
    __m128i mColorTimeAlphaMull = _mm_set1_epi16((short)alpha);
    mColorTimeAlpha = _mm_mullo_epi16(mColorTimeAlpha,mColorTimeAlphaMull);

    __m128i mMullInvAlpha = _mm_set1_epi16(invAlpha);

    mMullInvAlpha.m128i_i16[3] = 256;
    mMullInvAlpha.m128i_i16[7] = 256;

    //__m128i mZero = _mm_setzero_si128();
    int x=0;
    while(count > 3)
    {

        //__m128i source =  _mm_cvtsi64_si128(*(((long long*)canvas)+x));
        //__m128i source2 = _mm_cvtsi64_si128(*(((long long*)canvas)+x+1));

        __m128i source =  _mm_loadu_si128((__m128i*)(canvas+x*8));
        __m128i source2 = _mm_loadu_si128((__m128i*)(canvas+x*8));
        //_mm_ctv
        source2  = _mm_srli_si128(source2,64); 

        source = _mm_unpacklo_epi8(source,_mm_setzero_si128());
        source2 = _mm_unpacklo_epi8(source2,_mm_setzero_si128());

        //source = _mm_cvtepu8_epi16(source);
        //source2 = _mm_cvtepu8_epi16(source2);

        source  = _mm_mullo_epi16(source,mMullInvAlpha);    // source*invalpha
        source2  = _mm_mullo_epi16(source2,mMullInvAlpha);    // source*invalpha
        source  = _mm_adds_epu16(source,mColorTimeAlpha);     // t
        source2  = _mm_adds_epu16(source2,mColorTimeAlpha);     // t
        source  = _mm_srli_epi16(source,8); 
        source2  = _mm_srli_epi16(source2,8); 

        source        = _mm_packus_epi16(source,source );// mZero );         // pack
        source2        = _mm_packus_epi16(source2,source2 );// mZero );         // pack

        *((((long long*)canvas)+x)) =  source.m128i_u64[0];// _mm_cvtsi128_si64(source);
        *((((long long*)canvas)+x+1)) = source2.m128i_u64[0]; //_mm_cvtsi128_si64(source2);

        x+=2;
        //canvas += 8;
        count-=4;
    }

    if(count > 1)
    {
        __m128i source = _mm_cvtsi64_si128(*(((long long*)canvas)+x));

        //source = _mm_unpacklo_epi8(source, _mm_setzero_si128() );
        source = _mm_cvtepu8_epi16(source);

        __m128i tmp1  = _mm_mullo_epi16(source,mMullInvAlpha);    // source*invalpha
        tmp1          = _mm_adds_epu16(tmp1,mColorTimeAlpha);     // t

        tmp1          = _mm_srli_epi16(tmp1,8); 

        source        = _mm_packus_epi16(tmp1,tmp1 );// mZero );         // pack

        *(((long long*)canvas)+x) = source.m128i_u64[0];// _mm_cvtsi128_si64(source);
    }



}

void ApplyColorPixelSSE(unsigned char * canvas,int r,int g, int b, int alpha)
{
    int invAlpha = 256 - alpha;

    // unsigned long long tmpColor = ((unsigned long long)(r*alpha)<<32) | ((unsigned long long)(g*alpha)<<16) | (b*alpha);

    //__m128i mColorTimeAlpha = _mm_setzero_si128();
    ///mColorTimeAlpha.m128i_u64[0] = tmpColor;
    //mColorTimeAlpha.m128i_u64[1] = tmpColor;
    //__m128i mColorTimeAlpha;
    //mColorTimeAlpha.m128i_u16[0] = b*alpha;
    //mColorTimeAlpha.m128i_u16[1] = g*alpha;
    //mColorTimeAlpha.m128i_u16[2] = r*alpha;


    __m128i mColorTimeAlpha = _mm_setr_epi16(b*alpha,g*alpha,r*alpha,0,0,0,0,0);

    __m128i mMullInvAlpha = _mm_set1_epi16(invAlpha);
    __m128i source = _mm_cvtsi32_si128(*((int*)canvas));

    //source = _mm_unpacklo_epi8(source, _mm_setzero_si128() );
    source = _mm_cvtepu8_epi16(source);

    __m128i tmp1  = _mm_mullo_epi16(source,mMullInvAlpha);    // source*invalpha
    tmp1          = _mm_adds_epu16(tmp1,mColorTimeAlpha);     // t

    tmp1          = _mm_srli_epi16(tmp1,8); 

    source        = _mm_blend_epi16(tmp1,source,0x88);        // a,b,c,d  | e,f,g,h => a,b,c,h

    //source        = _mm_andnot_si128(mMaskAnd,source);        // mask alpha
    //tmp2          = _mm_and_si128(mMaskAnd,tmp2);             // mask colors
    //source        = _mm_or_si128(tmp2,source);                // 00XXXXXX | XX000000 = xxxxxxxx

    source        = _mm_packus_epi16(source, _mm_setzero_si128() );         // pack

    *((int*)canvas) =  _mm_cvtsi128_si32(source);

}

void ApplyColorPixel(unsigned char * canvas,int r,int g, int b, int alpha)
{


    int invAlpha = 256-alpha;

    unsigned char * line = canvas;

    b *= alpha;
    g *= alpha;
    r *= alpha;


    unsigned int tb = *line;
    unsigned int tg = line[1];
    unsigned int tr = line[2];


    tb = (b + (tb*invAlpha))>>8;
    tg=(g + (tg*invAlpha))>>8;
    tr=(r + (tr*invAlpha))>>8;

    /*tb = tb + (((b-tb)*alpha)>>8);
    tg=tg + (((g-tg)*alpha)>>8);
    tr=tr + (((r-tr)*alpha)>>8);*/

    *line = (unsigned char)tb;
    line[1] = (unsigned char)tg;
    line[2] = (unsigned char)tr;
}

void FastRowApplyColor(unsigned char * canvas, int len, int r , int g, int b, int alpha)
{
    //if(alpha> 0) alpha++;
    // convert alpha value from range 0-255 to 0-256
    alpha = (alpha*256)/255;

    int invAlpha = 256-alpha;

    unsigned char * line = canvas;

    b *= alpha;
    g *= alpha;
    r *= alpha;

    while(len > 0)
    {
        unsigned int tb = *line;
        unsigned int tg = line[1];
        unsigned int tr = line[2];


        tb = (b + (tb*invAlpha))>>8;
        tg=(g + (tg*invAlpha))>>8;
        tr=(r + (tr*invAlpha))>>8;

        /*tb = tb + (((b-tb)*alpha)>>8);
        tg=tg + (((g-tg)*alpha)>>8);
        tr=tr + (((r-tr)*alpha)>>8);*/

        *line = (unsigned char)tb;
        line[1] = (unsigned char)tg;
        line[2] = (unsigned char)tr;


        len --;
        line+=4;
    }
}


int _tmain(int argc, _TCHAR* argv[])
{
    TestSumABS();
    TestSumSquare();

    unsigned char field1[] = {255,3,4,255,2,3,4,255,2,3,4,255,2,3,4,255};
    unsigned char fieldtest[] = {255,3,4,255,2,3,4,255,2,3,4,255,2,3,4,255};

    //ApplyColorPixelSSE(field1,50,100,150,255);
    int r = 255;
    int g = 0;
    int b = 254;
    int a  = 128;
    int color = (r<<16)+(g<<8)+b;

    //ApplyColorPixel(field1,255,0,254,a);
    //ApplyColorPixelSSE(fieldtest,255,0,254,a);
    FastRowApplyColor(field1,4,r,g,b,a);

    Apply4ColorPixelSSE2(fieldtest,4,color, a);

    bool equal = true;
    for(int i = 0;i < 16;i++)
    {
        if(field1[i] != fieldtest[i])
        {
            equal = false;
            break;
        }
    }

    int c = 0;


    return 0;
}

