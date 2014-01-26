
#include <math.h>
//#include <iostream>
//#include <mmintrin.h>
#include <emmintrin.h>
#include <smmintrin.h>



#include "FastFunctions.h"
#include "NativeMedian8bit.h"

/*
alpha : 0 - 256
*/
void ApplyColorPixelSSE(unsigned char * canvas,int color, int alpha)
{
    int invAlpha = 256 - alpha;


    __m128i mColorTimeAlpha = _mm_setr_epi32(color&0xffffff,0,0,0);
    mColorTimeAlpha = _mm_cvtepu8_epi16(mColorTimeAlpha);
    __m128i mColorTimeAlphaMull = _mm_set1_epi16((short)alpha);
    mColorTimeAlpha =  _mm_mullo_epi16(mColorTimeAlpha,mColorTimeAlphaMull);

    __m128i mMullInvAlpha = _mm_set1_epi16(invAlpha);
    mMullInvAlpha.m128i_i16[3] = 256;

    __m128i source = _mm_cvtsi32_si128(*((int*)canvas));

    //source = _mm_unpacklo_epi8(source, _mm_setzero_si128() );
    source = _mm_cvtepu8_epi16(source);

    __m128i tmp1  = _mm_mullo_epi16(source,mMullInvAlpha);    // source*invalpha
    tmp1          = _mm_adds_epu16(tmp1,mColorTimeAlpha);     // t

    tmp1          = _mm_srli_epi16(tmp1,8); 

    //source        = _mm_blend_epi16(tmp1,source,0x88);        // a,b,c,d  | e,f,g,h => a,b,c,h

    //source        = _mm_andnot_si128(mMaskAnd,source);        // mask alpha
    //tmp2          = _mm_and_si128(mMaskAnd,tmp2);             // mask colors
    //source        = _mm_or_si128(tmp2,source);                // 00XXXXXX | XX000000 = xxxxxxxx

    source        = _mm_packus_epi16(tmp1, tmp1);// _mm_setzero_si128() );         // pack

    *((int*)canvas) =  _mm_cvtsi128_si32(source);

}

/*
alpha : 0 - 256
*/
void ApplyColorPixelSSE(unsigned char * canvas,int count,int color, int alpha)
{
    int invAlpha = 256 - alpha;

    __m128i mColorTimeAlpha = _mm_setr_epi32(color&0xffffff,0,0,0);

    mColorTimeAlpha = _mm_cvtepu8_epi16(mColorTimeAlpha);
    __m128i mColorTimeAlphaMull = _mm_set1_epi16((short)alpha);
    mColorTimeAlpha =
        _mm_mullo_epi16(mColorTimeAlpha,mColorTimeAlphaMull);

    __m128i mMullInvAlpha = _mm_set1_epi16(invAlpha);
    mMullInvAlpha.m128i_i16[3] = 256;

    // __m128i mZero =  _mm_setzero_si128();
    int x =0;
    while(count > 0)
    {
        __m128i source = _mm_cvtsi32_si128(*(((int*)canvas)+x));

        //source = _mm_unpacklo_epi8(source, _mm_setzero_si128() );
        source = _mm_cvtepu8_epi16(source);

        __m128i tmp1  = _mm_mullo_epi16(source,mMullInvAlpha);    // source*invalpha
        tmp1          = _mm_adds_epu16(tmp1,mColorTimeAlpha);     // t

        tmp1          = _mm_srli_epi16(tmp1,8); 

        //source        = _mm_blend_epi16(tmp1,source,0x88);        // a,b,c,d  | e,f,g,h => a,b,c,h

        //source        = _mm_andnot_si128(mMaskAnd,source);        // mask alpha
        //tmp2          = _mm_and_si128(mMaskAnd,tmp2);             // mask colors
        //source        = _mm_or_si128(tmp2,source);                // 00XXXXXX | XX000000 = xxxxxxxx

        source        = _mm_packus_epi16(tmp1,tmp1);//mZero );         // pack

        *(((int*)canvas)+x) =  _mm_cvtsi128_si32(source);

        x++;
        count--;
    }

}


/*
alpha : 0 - 256
*/
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

void ApplyColorPixelSSE(unsigned char * canvas,int count,int r,int g, int b, int alpha)
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

    while(count > 0)
    {
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

        canvas += 4;
        count--;
    }
}

/*
alpha : 0 - 256
*/
void Apply2ColorPixelSSE(unsigned char * canvas,int r,int g, int b, int alpha)
{
    int invAlpha = 256 - alpha;
    __m128i mColorTimeAlpha = _mm_setr_epi16(b*alpha,g*alpha,r*alpha,0,b*alpha,g*alpha,r*alpha,0);
    __m128i mMullInvAlpha = _mm_setr_epi16(invAlpha,invAlpha,invAlpha,1,invAlpha,invAlpha,invAlpha,1);


    __m128i source = _mm_cvtsi64_si128(*((long long*)canvas));
    //if((len & 6) == 4)
    //_mm_prefetch(((char *)line)+128, _MM_HINT_T1 );
    //source = _mm_unpacklo_epi8(source, _mm_setzero_si128() );
    source = _mm_cvtepu8_epi16(source);

    __m128i tmp1  = _mm_mullo_epi16(source,mMullInvAlpha);    // source*invalpha
    tmp1          = _mm_adds_epu16(tmp1,mColorTimeAlpha);     // t

    __m128i tmp2  = tmp1;                                              // rychle deleni 255
    tmp2          = _mm_srli_epi16(tmp2,8);                   // >> 8
    source        = _mm_blend_epi16(tmp2,source,0x88);        // a,b,c,d  | e,f,g,h => a,b,c,h

    //source        = _mm_andnot_si128(mMaskAnd,source);        // mask alpha
    //tmp2          = _mm_and_si128(mMaskAnd,tmp2);             // mask colors
    //source        = _mm_or_si128(tmp2,source);                // 00XXXXXX | XX000000 = xxxxxxxx

    source        = _mm_packus_epi16(source, _mm_setzero_si128() );         // pack

    *((long long*)canvas) =  _mm_cvtsi128_si64(source);
}

void Apply2ColorPixelSSE(unsigned char * canvas,int count,int color, int alpha)
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
    while(count > 1)
    {
        __m128i source = _mm_cvtsi64_si128(*(((long long*)canvas)+x));

        //source = _mm_unpacklo_epi8(source, _mm_setzero_si128() );
        source = _mm_cvtepu8_epi16(source);

        __m128i tmp1  = _mm_mullo_epi16(source,mMullInvAlpha);    // source*invalpha
        tmp1          = _mm_adds_epu16(tmp1,mColorTimeAlpha);     // t

        tmp1          = _mm_srli_epi16(tmp1,8); 

        //source        = _mm_blend_epi16(tmp1,source,0x88);        // a,b,c,d  | e,f,g,h => a,b,c,h

        //source        = _mm_andnot_si128(mMaskAnd,source);        // mask alpha
        //tmp2          = _mm_and_si128(mMaskAnd,tmp2);             // mask colors
        //source        = _mm_or_si128(tmp2,source);                // 00XXXXXX | XX000000 = xxxxxxxx

        //source        = _mm_packus_epi16(source,source );// mZero );         // pack
        source        = _mm_packus_epi16(tmp1,tmp1 );// mZero );         // pack

        *(((long long*)canvas)+x) =  _mm_cvtsi128_si64(source);

        x++;//=8;
        //canvas += 8;
        count-=2;
    }

}


void Apply2ColorPixelSSE(unsigned char * canvas,int count, int r,int g, int b, int alpha)
{
    int invAlpha = 256 - alpha;
    __m128i mColorTimeAlpha = _mm_setr_epi16(b*alpha,g*alpha,r*alpha,0,b*alpha,g*alpha,r*alpha,0);
    __m128i mMullInvAlpha = _mm_setr_epi16(invAlpha,invAlpha,invAlpha,1,invAlpha,invAlpha,invAlpha,1);
    __m128i mZero = _mm_setzero_si128();
    while(count > 0)
    {
        __m128i source = _mm_cvtsi64_si128(*((long long*)canvas));
        //if((len & 6) == 4)
        //_mm_prefetch(((char *)line)+128, _MM_HINT_T1 );
        //source = _mm_unpacklo_epi8(source, _mm_setzero_si128() );
        source = _mm_cvtepu8_epi16(source);

        __m128i tmp1  = _mm_mullo_epi16(source,mMullInvAlpha);    // source*invalpha
        tmp1          = _mm_adds_epu16(tmp1,mColorTimeAlpha);     // t

        __m128i tmp2  = tmp1;                                              // rychle deleni 255
        tmp2          = _mm_srli_epi16(tmp2,8);                   // >> 8
        source        = _mm_blend_epi16(tmp2,source,0x88);        // a,b,c,d  | e,f,g,h => a,b,c,h

        //source        = _mm_andnot_si128(mMaskAnd,source);        // mask alpha
        //tmp2          = _mm_and_si128(mMaskAnd,tmp2);             // mask colors
        //source        = _mm_or_si128(tmp2,source);                // 00XXXXXX | XX000000 = xxxxxxxx

        source        = _mm_packus_epi16(source, mZero );         // pack

        *((long long*)canvas) =  _mm_cvtsi128_si64(source);

        canvas += 8;
        count-=2;
    }
}

/*
alpha : 0 - 256
*/
void Apply4ColorPixelSSE(unsigned char * canvas,int r,int g, int b, int alpha)
{
    int invAlpha = 256 - alpha;

    unsigned int tmpColor = (r*alpha)<<16 | b*alpha;

    __m128i mColorRBTimeAlpha = _mm_set1_epi32(tmpColor);

    //__m128i mColorRBTimeAlpha = _mm_setr_epi16(b*alpha,r*alpha,b*alpha,r*alpha,b*alpha,r*alpha,b*alpha,r*alpha);
    __m128i mColorGTimeAlpha = _mm_set1_epi16(g*alpha);


    __m128i mMullInvAlpha = _mm_set1_epi16(invAlpha);
    __m128i mMaskGAnd = _mm_setr_epi8(0,0xff,0,0,0,0xff,0,0,0,0xff,0,0,0,0xff,0,0);
    __m128i mMaskAAnd = _mm_setr_epi8(0,0,0,0xff,0,0,0,0xff,0,0,0,0xff,0,0,0,0xff);

    /*   __m128i mMaskGAnd = _mm_setr_epi16(0xff00,0,0xff00,0,0xff00,0,0xff00,0);
    __m128i mMaskAAnd = _mm_setr_epi16(0,0xff00,0,0xff00,0,0xff00,0,0xff00);*/

    __m128i sourceRB = _mm_load_si128((__m128i*)canvas); // load    ArgbArgbArgbArgb
    //_mm_prefetch((char *)canvas+16,_MM_HINT_T0);
    __m128i sourceG = _mm_and_si128(sourceRB,mMaskGAnd);  // masked  xxgxxxgxxxgxxxgx
    sourceRB = _mm_andnot_si128(mMaskGAnd,sourceRB);
    __m128i savealpha = _mm_and_si128(sourceRB,mMaskAAnd);
    sourceRB = _mm_andnot_si128(mMaskAAnd,sourceRB);


    sourceG = _mm_srli_epi16(sourceG,8);                 // shift for compute    xxxgxxxgxxxgxxxg
    // in source is now  xrxbxrxbxrxbxrxb
    sourceRB = _mm_mullo_epi16(sourceRB,mMullInvAlpha);    // sourceRB <= sourceRB*invAlpha
    sourceRB = _mm_adds_epu16(sourceRB,mColorRBTimeAlpha); // sourceRB <= sourceRB+(colorRB*Alpha)
    sourceRB = _mm_srli_epi16(sourceRB,8);                         // now in sourceRB is sourceRB/255
    sourceG = _mm_mullo_epi16(sourceG,mMullInvAlpha);    // sourceG <= sourceG*invAlpha
    sourceG = _mm_adds_epu16(sourceG,mColorGTimeAlpha); // sourceG <= sourceG+(colorG*Alpha)

    //sourceG = _mm_srli_epi16(sourceG,8);                           // now in sourceG is sourceG/255

    // now merge back xrxb.... xxxg.... into xrgb....

    //sourceG = _mm_slli_epi16(sourceG,8);
    //sourceG = _mm_and_si128(mMaskGAnd,sourceG);
    //sourceRB =_mm_or_si128(sourceRB,sourceG);         // now in sourceRB is xrgbxrgb....
    //sourceRB = _mm_or_si128(sourceRB,savealpha); // restore alpha now argb....


    sourceG = _mm_and_si128(mMaskGAnd,sourceG);
    __m128i tmp = _mm_or_si128(sourceRB,savealpha); // restore alpha now argb....
    sourceRB =_mm_or_si128(tmp,sourceG);         // now in sourceRB is xrgbxrgb....

    _mm_store_si128((__m128i*)canvas,sourceRB);
}

/*
alpha : 0 - 256
*/
void Apply4ColorPixelSSE(unsigned char * canvas,int count,int r,int g, int b, int alpha)
{
    int invAlpha = 256 - alpha;

    unsigned int tmpColor = (r*alpha)<<16 | b*alpha;

    __m128i mColorRBTimeAlpha = _mm_set1_epi32(tmpColor);

    //__m128i mColorRBTimeAlpha = _mm_setr_epi16(b*alpha,r*alpha,b*alpha,r*alpha,b*alpha,r*alpha,b*alpha,r*alpha);
    __m128i mColorGTimeAlpha = _mm_set1_epi16(g*alpha);


    __m128i mMullInvAlpha = _mm_set1_epi16(invAlpha);
    __m128i mMaskGAnd = _mm_setr_epi8(0,0xff,0,0,0,0xff,0,0,0,0xff,0,0,0,0xff,0,0);
    __m128i mMaskAAnd = _mm_setr_epi8(0,0,0,0xff,0,0,0,0xff,0,0,0,0xff,0,0,0,0xff);
    while(count > 0)
    {
        /*   __m128i mMaskGAnd = _mm_setr_epi16(0xff00,0,0xff00,0,0xff00,0,0xff00,0);
        __m128i mMaskAAnd = _mm_setr_epi16(0,0xff00,0,0xff00,0,0xff00,0,0xff00);*/

        __m128i sourceRB = _mm_load_si128((__m128i*)canvas); // load    ArgbArgbArgbArgb
        //_mm_prefetch((char *)canvas+16,_MM_HINT_T0);
        __m128i sourceG = _mm_and_si128(sourceRB,mMaskGAnd);  // masked  xxgxxxgxxxgxxxgx
        sourceRB = _mm_andnot_si128(mMaskGAnd,sourceRB);
        __m128i savealpha = _mm_and_si128(sourceRB,mMaskAAnd);
        sourceRB = _mm_andnot_si128(mMaskAAnd,sourceRB);


        sourceG = _mm_srli_epi16(sourceG,8);                 // shift for compute    xxxgxxxgxxxgxxxg
        // in source is now  xrxbxrxbxrxbxrxb
        sourceRB = _mm_mullo_epi16(sourceRB,mMullInvAlpha);    // sourceRB <= sourceRB*invAlpha
        sourceRB = _mm_adds_epu16(sourceRB,mColorRBTimeAlpha); // sourceRB <= sourceRB+(colorRB*Alpha)
        sourceRB = _mm_srli_epi16(sourceRB,8);                         // now in sourceRB is sourceRB/255
        sourceG = _mm_mullo_epi16(sourceG,mMullInvAlpha);    // sourceG <= sourceG*invAlpha
        sourceG = _mm_adds_epu16(sourceG,mColorGTimeAlpha); // sourceG <= sourceG+(colorG*Alpha)

        //sourceG = _mm_srli_epi16(sourceG,8);                           // now in sourceG is sourceG/255

        // now merge back xrxb.... xxxg.... into xrgb....

        //sourceG = _mm_slli_epi16(sourceG,8);
        //sourceG = _mm_and_si128(mMaskGAnd,sourceG);
        //sourceRB =_mm_or_si128(sourceRB,sourceG);         // now in sourceRB is xrgbxrgb....
        //sourceRB = _mm_or_si128(sourceRB,savealpha); // restore alpha now argb....


        sourceG = _mm_and_si128(mMaskGAnd,sourceG);
        __m128i tmp = _mm_or_si128(sourceRB,savealpha); // restore alpha now argb....
        sourceRB =_mm_or_si128(tmp,sourceG);         // now in sourceRB is xrgbxrgb....

        _mm_store_si128((__m128i*)canvas,sourceRB);

        canvas+=16;
        count-=4;
    }
}

/*
alpha : 0 - 256
*/


void Apply4ColorPixelSSE(unsigned char * canvas,int count,int color, int alpha)
{
    int invAlpha = 256 - alpha;

    // unsigned int tmpColor = (r*alpha)<<16 | b*alpha;

    //  __m128i mColorTimeAlpha = _mm_set1_epi32(color&0xff00ff);

    //mColorTimeAlpha = _mm_cvtepu8_epi16(mColorTimeAlpha);
    //__m128i mColorTimeAlphaMull = _mm_set1_epi16((short)alpha);


    __m128i mColorRBTimeAlpha = _mm_set1_epi32(((unsigned int)(color&0xff00ff))*alpha);
    //mColorRBTimeAlpha = _mm_mullo_epi16(mColorRBTimeAlpha,mColorTimeAlphaMull);

    //__m128i mColorRBTimeAlpha = _mm_setr_epi16(b*alpha,r*alpha,b*alpha,r*alpha,b*alpha,r*alpha,b*alpha,r*alpha);
    __m128i mColorGTimeAlpha = _mm_set1_epi16(((color>>8)&0xff)*alpha);



    __m128i mMullInvAlpha = _mm_set1_epi16(invAlpha);
    __m128i mMullInvAlphaG = _mm_set1_epi32((256<<16)+invAlpha);

    //__m128i mMaskGAnd = _mm_set1_epi32(0x0000ff00);
    //__m128i mMaskAAnd = _mm_set1_epi32(0xff000000);
    __m128i mMaskGAAnd = _mm_set1_epi32(0xff00FF00);

    while(count > 0)
    {
        /*   __m128i mMaskGAnd = _mm_setr_epi16(0xff00,0,0xff00,0,0xff00,0,0xff00,0);
        __m128i mMaskAAnd = _mm_setr_epi16(0,0xff00,0,0xff00,0,0xff00,0,0xff00);*/

        __m128i sourceRB = _mm_load_si128((__m128i*)canvas); // load    ArgbArgbArgbArgb
        //_mm_prefetch((char *)canvas+16,_MM_HINT_T0);

        __m128i sourceG = sourceRB;



        //sourceG = _mm_and_si128(sourceG,mMaskGAnd);  // masked  xxgxxxgxxxgxxxgx
        sourceRB = _mm_andnot_si128(mMaskGAAnd,sourceRB);
        //__m128i savealpha = _mm_and_si128(sourceRB,mMaskAAnd);
        //sourceRB = _mm_andnot_si128(mMaskAAnd,sourceRB);


        sourceG = _mm_srli_epi16(sourceG,8);                 // shift for compute    xxxgxxxgxxxgxxxg
        // in source is now  xrxbxrxbxrxbxrxb
        sourceRB = _mm_mullo_epi16(sourceRB,mMullInvAlpha);    // sourceRB <= sourceRB*invAlpha
        sourceRB = _mm_adds_epu16(sourceRB,mColorRBTimeAlpha); // sourceRB <= sourceRB+(colorRB*Alpha)
        sourceRB = _mm_srli_epi16(sourceRB,8);                         // now in sourceRB is sourceRB/255
        sourceG = _mm_mullo_epi16(sourceG,mMullInvAlphaG);    // sourceG <= sourceG*invAlpha
        sourceG = _mm_adds_epu16(sourceG,mColorGTimeAlpha); // sourceG <= sourceG+(colorG*Alpha)

        //sourceG = _mm_srli_epi16(sourceG,8);                           // now in sourceG is sourceG/255

        // now merge back xrxb.... xxxg.... into xrgb....




        sourceG = _mm_and_si128(mMaskGAAnd,sourceG);

        //sourceRB = _mm_or_si128(sourceRB,savealpha); // restore alpha now argb....
        sourceRB =_mm_or_si128(sourceRB,sourceG);         // now in sourceRB is xrgbxrgb....

        _mm_store_si128((__m128i*)canvas,sourceRB);

        canvas+=16;
        count-=4;
    }
}

/*
alpha : 0 - 256
*/
void Apply4ColorPixelSSE2(unsigned char * canvas,int count,int color, int alpha)
{
    int invAlpha = 256 - alpha;

    __m128i mColorTimeAlpha = _mm_set1_epi32(color&0xffffff);

    mColorTimeAlpha = _mm_cvtepu8_epi16(mColorTimeAlpha);
    __m128i mColorTimeAlphaMull = _mm_set1_epi16((short)alpha);
    mColorTimeAlpha = _mm_mullo_epi16(mColorTimeAlpha,mColorTimeAlphaMull);

    __m128i mMullInvAlpha = _mm_set1_epi16(invAlpha);
    __m128i mZero = _mm_setzero_si128();
    __m128i mAlphaMask = _mm_set1_epi32(0xff000000); 

    while(count > 3)
    {
        __m128i source = _mm_load_si128((__m128i*)canvas); // load    ArgbArgbArgbArgb
        __m128i sourceOrig = source;
        sourceOrig = _mm_and_si128(sourceOrig,mAlphaMask);
        source = _mm_andnot_si128(mAlphaMask,source);

        //_mm_prefetch((char *)canvas+16,_MM_HINT_T0);
        __m128i sourceLow = source; _mm_setzero_si128();
        //sourceLow = _mm_shuffle_epi32((__m128d)sourceLow,(__m128d)source,_MM_SHUFFLE2(0,1));

        sourceLow         = _mm_slli_si128(sourceLow,8); 
        sourceLow         = _mm_srli_si128(sourceLow,8); 
        source            = _mm_srli_si128(source,8);




        //source = _mm_unpacklo_epi8(source, _mm_setzero_si128() );
        source = _mm_cvtepu8_epi16(source);
        sourceLow = _mm_cvtepu8_epi16(sourceLow);

        //__m128i sourceLowOrig = sourceLow;
        //__m128i sourceOrig = source;

        source  = _mm_mullo_epi16(source,mMullInvAlpha);    // source*invalpha
        sourceLow  = _mm_mullo_epi16(sourceLow,mMullInvAlpha);    // source*invalpha

        source          = _mm_adds_epu16(source,mColorTimeAlpha);     // t
        sourceLow          = _mm_adds_epu16(sourceLow,mColorTimeAlpha);     // t

        source          = _mm_srli_epi16(source,8); 
        sourceLow       = _mm_srli_epi16(sourceLow,8); 



        source        = _mm_packus_epi16(source, mZero );         // pack
        sourceLow     = _mm_packus_epi16(sourceLow, mZero );         // pack

        source        = _mm_slli_si128(source,8);
        source        = _mm_or_si128(source,sourceLow);

        source = _mm_or_si128(source,sourceOrig);

        _mm_store_si128((__m128i*)canvas,source);

        canvas += 16;
        count-=4;
    }


}

void FastFunctions::NewFastRowApplyColorSSE(unsigned char * canvas, int countPixel, int color , int alpha)
{
    alpha = (alpha*256)/255;

    unsigned char * line = canvas;

    //if(len > 0)
    {
        ApplyColorPixelSSE(line,countPixel,color,alpha);

        //len -= 1;
        //line+=4;
    }

    /*while(len > 0)
    {
    ApplyColorPixelSSE(line,color,alpha);

    len -= 1;
    line+=4;
    }*/
}

void FastFunctions::NewFastRowApplyColorSSE64(unsigned char * canvas, int countPixel, int color , int alpha255)
{
    // convert alpha value from range 0-255 to 0-256

    int alpha = (alpha255*256)/255;


    unsigned char * line = canvas;

    if(countPixel < 8)
    {
        ApplyColorPixelSSE(line,countPixel,color,alpha);
    }
    else
    {
        // fix bad align
        // move if address is 0xc || 0x4
        unsigned int tmp = ((((unsigned int)line) & 0xf)/4)&1; 

        if((tmp != 0) //&& countPixel > 0
            )
        {
            ApplyColorPixelSSE(line,color,alpha);

            countPixel -= 1;
            line+=4;
        }

        //if(countPixel > 1)
        {
            int tmpLen = countPixel - (countPixel&1);
            Apply2ColorPixelSSE(line,tmpLen, color,alpha);

            countPixel -= tmpLen;
            line+=tmpLen*4;
        }


        if(countPixel > 0)
        {
            //ApplyColorPixelSSE(line,len,r,g,b,alpha);
            ApplyColorPixelSSE(line,color,alpha);
        }
    }

    /*while(len > 0)
    {
    ApplyColorPixelSSE(line,r,g,b,alpha);

    len -= 1;
    line+=4;
    }*/
}

void FastFunctions::NewFastRowApplyColorSSE64(unsigned char * canvas, int countPixel, int color )
{
    NewFastRowApplyColorSSE64(canvas,countPixel,color,(((color) >> 24) & 0xff));
}

void FastFunctions::NewFastRowApplyColorSSE128(unsigned char * canvas, int countPixel, int color , int alpha)
{
    // convert alpha value from range 0-255 to 0-256

    alpha = (alpha*256)/255;

    unsigned char * line = canvas;

    unsigned int tmp = (((unsigned int)line) & 0xf )/4; 
    tmp = 4-tmp;
    if(countPixel > 3 && tmp > 0)
    {



        ApplyColorPixelSSE(line,tmp,color,alpha);
        countPixel -= tmp;
        line+=tmp*4;



        /*if(tmp == 0x4)
        {
        ApplyColorPixelSSE(line,3,color,alpha);
        countPixel -= 3;
        line+=12;
        }
        else if(tmp == 0x8)
        {
        ApplyColorPixelSSE(line,2,color,alpha);
        countPixel -= 2;
        line+=8;
        }
        else if(tmp == 0xc)
        {
        ApplyColorPixelSSE(line,color,alpha);

        countPixel -= 1;
        line+=4;
        }*/
    }

    /* while( len > 0)
    {
    unsigned int tmp = ((unsigned int)line) & 0xf; 
    if((tmp == 0xc || tmp == 0x4 || tmp == 0x8) )
    {
    ApplyColorPixelSSE(line,r,g,b,alpha);

    len -= 1;
    line+=4;
    }
    else
    {
    break;
    }
    }*/

    if(countPixel > 63)
    {
        int tmpLen = countPixel - (countPixel&3);
        //Apply4ColorPixelSSE(line,r,g,b,alpha);
        Apply4ColorPixelSSE(line,tmpLen,color,alpha);
        countPixel -= tmpLen;

        line+=tmpLen*4;
    }

    /*if(countPixel > 1)
    {
    int tmpLen = countPixel - (countPixel&1);
    Apply2ColorPixelSSE(line,tmpLen, color,alpha);

    countPixel -= tmpLen;
    line+=tmpLen*4;
    }*/


    if(countPixel > 0)
    {
        ApplyColorPixelSSE(line,countPixel,color,alpha);
    }
}


void FastFunctions::FastRowApplyColor(unsigned char * canvas, int len, int r , int g, int b, int alpha)
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

void FastFunctions::FastRowApplyColorSSE64(unsigned char * canvas, int len, int r , int g, int b, int alpha)
{
    // convert alpha value from range 0-255 to 0-256
    alpha = (alpha*256)/255;

    unsigned char * line = canvas;


    // fix bad align
    unsigned int tmp = ((unsigned int)line) & 0xf; 

    if((tmp == 0xc || tmp == 0x4) && len > 0)
    {
        ApplyColorPixelSSE(line,r,g,b,alpha);

        len -= 1;
        line+=4;
    }

    if(len > 1)
    {
        int tmpLen = len - (len&1);
        Apply2ColorPixelSSE(line,tmpLen, r,g,b,alpha);

        len -= tmpLen;
        line+=tmpLen*4;
    }


    if(len > 0)
    {
        //ApplyColorPixelSSE(line,len,r,g,b,alpha);
        ApplyColorPixelSSE(line,r,g,b,alpha);
    }


    /*while(len > 0)
    {
    ApplyColorPixelSSE(line,r,g,b,alpha);

    len -= 1;
    line+=4;
    }*/


}


void FastFunctions::FastRowApplyColorSSE128(unsigned char * canvas, int len, int r , int g, int b, int alpha)
{
    alpha = (alpha*256)/255;

    // implementace (color*alpha + (255-alpha)*source)/255 



    unsigned char * line = canvas;

    if(len > 3)
    {
        unsigned int tmp = ((unsigned int)line) & 0xf; 
        if(tmp == 0x4)
        {
            ApplyColorPixelSSE(line,3,r,g,b,alpha);
            len -= 3;
            line+=12;
        }
        else if(tmp == 0x8)
        {
            ApplyColorPixelSSE(line,2,r,g,b,alpha);
            len -= 2;
            line+=8;
        }
        else if(tmp == 0xc)
        {
            ApplyColorPixelSSE(line,r,g,b,alpha);
            len -= 1;
            line+=4;
        }
    }

    /* while( len > 0)
    {
    unsigned int tmp = ((unsigned int)line) & 0xf; 
    if((tmp == 0xc || tmp == 0x4 || tmp == 0x8) )
    {
    ApplyColorPixelSSE(line,r,g,b,alpha);

    len -= 1;
    line+=4;
    }
    else
    {
    break;
    }
    }*/

    if(len > 3)
    {
        int tmpLen = len - (len&3);
        //Apply4ColorPixelSSE(line,r,g,b,alpha);
        Apply4ColorPixelSSE(line,tmpLen,r,g,b,alpha);
        len -= tmpLen;

        line+=tmpLen*4;
    }


    if(len > 0)
    {
        ApplyColorPixelSSE(line,len,r,g,b,alpha);
    }
}


int __fastcall FastAbs2(int data)
{
    int topbitreplicated = data >> 31;
    return (data ^ topbitreplicated) - topbitreplicated;  
}

unsigned short int Fast255div(unsigned short int value)
{
    return (value+1 + (value >> 8)) >> 8;
}

void FastFunctions::FastRowsApplyColor(unsigned char * canvas, int canvasWidth, short int * ranges, int rlen, int rangeStartY, int r , int g, int b, int alpha)
{
    //if(alpha> 0) alpha++;
    // convert alpha value from range 0-255 to 0-256
    alpha = (alpha*256)/255;

    int invAlpha = 256-alpha;
    int rowStartIndex = (rangeStartY) * canvasWidth;

    short * end = ranges+rlen;

    //int indexY = minY * this._canvasWidth;
    //for (int y  = 0; y < rlen; y+=2)//, indexY += this._canvasWidth)
    while(ranges < end)
    {
        int index = rowStartIndex + (*ranges) * 4;

        //
        int len = ranges[1] - *ranges+1; 
        unsigned char * line = canvas+index;

        while(len > 0)
        {
            unsigned int tb = *line;
            unsigned int tg = line[1];
            unsigned int tr = line[2];


            tb = (b*alpha + (tb*invAlpha))>>8;
            tg=(g*alpha + (tg*invAlpha))>>8;
            tr=(r*alpha + (tr*invAlpha))>>8;

            /*tb = tb + (((b-tb)*alpha)>>8);
            tg=tg + (((g-tg)*alpha)>>8);
            tr=tr + (((r-tr)*alpha)>>8);*/

            *line = (unsigned char)tb;
            line[1] = (unsigned char)tg;
            line[2] = (unsigned char)tr;


            len -= 1;
            line+=4;
        }

        rowStartIndex += canvasWidth;
        ranges+=2;
    }
}





void FastFunctions::FastRowsApplyColorSSE64(unsigned char * canvas, int canvasWidth, short int * ranges, int rlen, int rangeStartY, int r,int g, int b, int alpha)
{
    // convert alpha value from range 0-255 to 0-256
    alpha = (alpha*256)/255;

    // implementace (color*alpha + (255-alpha)*source)/255 

    int rowStartIndex = (rangeStartY) * canvasWidth;

    short * end = ranges+rlen;



    //int indexY = minY * this._canvasWidth;
    //for (int y  = 0; y < rlen; y+=2)//, indexY += this._canvasWidth)
    while(ranges < end)
    {
        int index = rowStartIndex + (*ranges) * 4;

        int len = ranges[1] - *ranges+1; 
        unsigned char * line = canvas+index;

        // fix bad align
        unsigned int tmp = ((unsigned int)line) & 0xf; 

        if((tmp == 0xc || tmp == 0x4) && len > 0)
        {
            ApplyColorPixelSSE(line,r,g,b,alpha);

            len -= 1;
            line+=4;
        }

        while(len > 1)
        {
            Apply2ColorPixelSSE(line,r,g,b,alpha);

            len -= 2;
            line+=8;
        }

        while(len > 0)
        {
            ApplyColorPixelSSE(line,r,g,b,alpha);

            len -= 1;
            line+=4;
        }

        rowStartIndex += canvasWidth;
        ranges+=2;
    }

}

void FastFunctions::FastRowsApplyColorSSE128(unsigned char * canvas, int canvasWidth, short int * ranges, int rlen, int rangeStartY, int r,int g, int b, int alpha)
{
    alpha = (alpha*256)/255;

    // implementace (color*alpha + (255-alpha)*source)/255 

    int rowStartIndex = (rangeStartY) * canvasWidth;
    short * end = ranges+rlen;

    //int indexY = minY * this._canvasWidth;
    //for (int y  = 0; y < rlen; y+=2)//, indexY += this._canvasWidth)
    while(ranges < end)
    {
        int index = rowStartIndex + (*ranges) * 4;

        int len = ranges[1] - *ranges+1; 
        unsigned char * line = canvas+index;

        while( len > 0)
        {
            unsigned int tmp = ((unsigned int)line) & 0xf; 
            if((tmp == 0xc || tmp == 0x4 || tmp == 0x8) )
            {
                ApplyColorPixelSSE(line,r,g,b,alpha);

                len -= 1;
                line+=4;
            }
            else
            {
                break;
            }
        }

        while(len > 3)
        {
            Apply4ColorPixelSSE(line,r,g,b,alpha);

            len -= 4;
            line+=16;
        }

        while(len > 0)
        {
            ApplyColorPixelSSE(line,r,g,b,alpha);

            len -= 1;
            line+=4;
        }

        rowStartIndex += canvasWidth;
        ranges+=2;
    }

}

void FastFunctions::FastRowsApplyColorSSE128_test(unsigned char * canvas, int canvasWidth, short int * ranges, int rlen, int rangeStartY, int r,int g, int b, int alpha)
{
    alpha = (alpha*256)/255;

    int rowStartIndex = (rangeStartY) * canvasWidth;

    short * end = ranges+rlen;

    int invAlpha = 256-alpha;

    //int indexY = minY * this._canvasWidth;
    //for (int y  = 0; y < rlen; y+=2)//, indexY += this._canvasWidth)
    while(ranges < end)
    {

        int index = rowStartIndex + (*ranges) * 4;

        //
        int len = ranges[1] - *ranges+1; 
        unsigned char * line = canvas+index;

        while( len > 0)
        {
            unsigned int tmp = ((unsigned int)line) & 0xf; 
            if((tmp == 0xc || tmp == 0x4 || tmp == 0x8) )
            {
                ApplyColorPixelSSE(line,r,g,b,alpha);

                len -= 1;
                line+=4;
            }
            else
            {
                break;
            }
        }


        while(len > 3)
        {
            Apply4ColorPixelSSE(line,r,g,b,alpha);

            len -= 4;
            line+=16;
        }


        while(len > 0)
        {
            ApplyColorPixelSSE(line,r,g,b,alpha);

            len -= 1;
            line+=4;
        }

        rowStartIndex += canvasWidth;
        ranges+=2;
    }

}


void FastFunctions::RenderRectangle(unsigned char * canvas,int canvasWidth, int x,int y, int width, int height, int color , int alpha)
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
        canvas+=rowStartIndex;

        for (int iy  = 0; iy < height; iy++)//, indexY += this._canvasWidth)
        {
            NewFastRowApplyColorSSE64(canvas, width, color, alpha);

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

    canvas += (startY) * canvasWidth;
    //startY *= 2;
    for (int i = 0; i < rlen; i += 2)
    {
        int index = (ranges[i]) * 4;
        int count = ranges[i+1] -  ranges[i]+1;

        NewFastRowApplyColorSSE64(canvas+index, count, color, alpha);

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
                                   short int px0,short int py0,short int px1,short int py1,short int px2,short int py2, int color, int alpha)
{
    int v0x,v1x,v2x,v0y,v1y,v2y;

    v0x = px1 - px0;
    v0y = py1 - py0;

    v1x = px2 - px1;
    v1y = py2 - py1;

    v2x = px0 - px2;
    v2y = py0 - py2;


    alpha = (alpha * 256) / 255;

    int invAlpha = 256 - alpha;

    int b = ((color)&0xff) * alpha;
    int g = ((color>>8)&0xff) * alpha;
    int r = ((color>>16)&0xff) * alpha;
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
                                      short int px0,short int py0,short int px1,short int py1,short int px2,short int py2, int color, int alpha255)
{

    //alpha = (alpha * 256) / 255;



    int rgba = color;

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

            int isCrossLine1 = (py1 == py2) ? -1 : (y - py1) * (py2 - y);

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

        NewFastRowApplyColorSSE64(canvas+currIndex,end- start + 1,rgba,alpha255);



        /*while (start <= end )// && start < canvas.WidthPixel)
        {
        int tb = canvas.Data[currIndex];
        int tg = canvas.Data[currIndex + 1];
        int tr = canvas.Data[currIndex + 2];


        canvas.Data[currIndex] = (byte)((b + (tb * invAlpha)) >> 8);
        canvas.Data[currIndex + 1] = (byte)((g + (tg * invAlpha)) >> 8);
        canvas.Data[currIndex + 2] = (byte)((r + (tr * invAlpha)) >> 8);

        start ++;
        currIndex+= 4;
        }*/

        rowIndex += canvasWidth;
    }
}









void FillSSEInt32(unsigned long * M, long Fill, unsigned int CountFill)
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

    int tmpCount = 4-((((unsigned int)M)&0xf)/4);
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
    }


    f = _mm_set1_epi32(Fill);

    int index = 0;
    while (CountFill >= 16)
    { 
        _mm_store_si128((__m128i *)(M+index), f);
        _mm_store_si128((__m128i *)(M+index+4), f);
        _mm_store_si128((__m128i *)(M+index+8), f);
        _mm_store_si128((__m128i *)(M+index+12), f);
        /*_mm_store_si128((__m128i *)(M+index+16), f);
        _mm_store_si128((__m128i *)(M+index+20), f);
        _mm_store_si128((__m128i *)(M+index+24), f);
        _mm_store_si128((__m128i *)(M+index+28), f);*/
        //_mm_store_si128((__m128i *)M2, f);
        //_mm_stream_si128((__m128i *)M, f);
        //_mm_stream_si128((__m128i *)(M+4), f);
        //M += 16;

        index += 16;
        CountFill -= 16;
    }

    M += index;
    


    while (CountFill >= 4)
    {
        _mm_store_si128((__m128i *)M, f);
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

void FastFunctions::ClearFieldByColor(unsigned char * curr, int lengthPixel, int color)
{
    FillSSEInt32((unsigned long *)curr, color,lengthPixel);
    //std::fill((unsigned int *)curr,((unsigned int *)curr)+length/4,  color);
}

void FastFunctions::RenderOneRow(int * listRowsForApply, int countRows, unsigned char * canvas)
{
    ClearFieldByColor(canvas+listRowsForApply[0]*4,listRowsForApply[1],listRowsForApply[2]);

    int index = 3;
    for(int rows = 0;rows < countRows;rows++)
    {
        int color = listRowsForApply[index + 2];
        int alpha = (((color)>>24)&0xff);
        NewFastRowApplyColorSSE64(canvas+
            listRowsForApply[index] * 4, listRowsForApply[index + 1], color, alpha);

        index +=3;
    }
}

__int64 FastFunctions::computeFittnessTile(unsigned char * curr, unsigned char * orig, int length, int widthPixel)
{
    // NativeMedian8Bit medR = NativeMedian8Bit();
    // NativeMedian8Bit medG = NativeMedian8Bit();
    NativeMedian8Bit medB = NativeMedian8Bit();
    widthPixel*=10;
    __int64 result = 0;

    int pixelOnRow = 0;
    for(int index = 0;index < length;index+=4)
    {
        if(pixelOnRow >= widthPixel)
        {
            result += (medB.ValueSum() + medB.SumStdDev());
            medB.Clear();
            pixelOnRow = 0;
        }

        medB.InsertData(labs(curr[index] - orig[index]));
        medB.InsertData(labs(curr[index + 1] - orig[index + 1]));
        medB.InsertData(labs(curr[index + 2] - orig[index + 2]));

        pixelOnRow++;


        /*  int tmp = (labs(curr[index] - orig[index])>>2) +
        (labs(curr[index+1] - orig[index+1])>>2) +
        (labs(curr[index+2] - orig[index+2])>>2) ;
        medB.InsertData(tmp);*/




        //  index += 4;
    }


    result += (medB.ValueSum() + medB.SumStdDev());


    //result += (medB.Median()) + medB.SumStdDev());

    // result += (medG.ValueSum() + medG.SumStdDev());
    // result += (medR.ValueSum() + medR.SumStdDev());

    return result;
}

__int64 FastFunctions::computeFittness_2d(unsigned char * current, unsigned char * orig, int length, int width)
{
    __int64 result = 0;
    int height = (length / (width));

    int rowIndexUp = 0;
    int rowIndex = width;
    int rowIndexDown = width*2;

    for (int y = 1; y < height - 1; y+=2)
    {
        int tmpRowIndexUp = rowIndexUp+8;
        int tmpRowIndex = rowIndex + 8;
        int tmpRowIndexDown = rowIndexDown + 8;

        int lastBr = current[tmpRowIndex-8] - orig[tmpRowIndex-8];lastBr*=lastBr;
        int lastBg = current[tmpRowIndex-8+1] - orig[tmpRowIndex-8+1];lastBg*=lastBg;
        int lastBb = current[tmpRowIndex-8+2] - orig[tmpRowIndex-8+2];lastBb*=lastBb;

        int lastBrDown = current[tmpRowIndexDown-8] - orig[tmpRowIndexDown-8];lastBrDown*=lastBrDown;
        int lastBgDown = current[tmpRowIndexDown-8+1] - orig[tmpRowIndexDown-8+1];lastBgDown*=lastBgDown;
        int lastBbDown = current[tmpRowIndexDown-8+2] - orig[tmpRowIndexDown-8+2];lastBbDown*=lastBbDown;

        for (int x = 8; x < width; x += 8)
        {
            /*if(x+16<width)
            {
            _mm_prefetch((char *)current+tmpRowIndexUp+16,_MM_HINT_T0);
            _mm_prefetch((char *)orig+tmpRowIndexUp+16,_MM_HINT_T0);
            _mm_prefetch((char *)current+tmpRowIndex+16,_MM_HINT_T0);
            _mm_prefetch((char *)orig+tmpRowIndex+16,_MM_HINT_T0);
            _mm_prefetch((char *)current+tmpRowIndexDown+16,_MM_HINT_T0);
            _mm_prefetch((char *)orig+tmpRowIndexDown+16,_MM_HINT_T0);
            }*/
            // compute 2d fittness

            int br = current[tmpRowIndex] - orig[tmpRowIndex];
            int bg = current[tmpRowIndex + 1] - orig[tmpRowIndex + 1];
            int bb = current[tmpRowIndex + 2] - orig[tmpRowIndex + 2];
            br *= br;
            bg *= bg;
            bb *= bb;

            int br2 = current[tmpRowIndex-4] - orig[tmpRowIndex-4];
            int bg2 = current[tmpRowIndex-4 + 1] - orig[tmpRowIndex-4 + 1];
            int bb2 = current[tmpRowIndex-4 + 2] - orig[tmpRowIndex-4 + 2];
            br2 *= br2;
            bg2 *= bg2;
            bb2 *= bb2;

            int br3 = current[tmpRowIndexDown] - orig[tmpRowIndexDown];
            int bg3 = current[tmpRowIndexDown + 1] - orig[tmpRowIndexDown + 1];
            int bb3 = current[tmpRowIndexDown + 2] - orig[tmpRowIndexDown + 2];
            br3 *= br3;
            bg3 *= bg3;
            bb3 *= bb3;

            int br4 = current[tmpRowIndexDown - 4] - orig[tmpRowIndexDown - 4];
            int bg4 = current[tmpRowIndexDown - 4 + 1] - orig[tmpRowIndexDown - 4 + 1];
            int bb4 = current[tmpRowIndexDown - 4 + 2] - orig[tmpRowIndexDown - 4 + 2];

            br4 *= br4;
            bg4 *= bg4;
            bb4 *= bb4;



            const int fixMul = 1;
            const int fixMul2 = 1;
            result += (br + bb + bg + 
                br2 + bb2 + bg2 +
                br3 + bb3 + bg3 +
                br4 + bb4 + bg4)*fixMul ;

            result += (labs(br - br4)+labs(bg - bg4)+labs(bb - bb4))*fixMul2;
            result += (labs(br2 - br3) + labs(bg2 - bg3) + labs(bb2 - bb3))*fixMul2;

            int tmpbr = current[tmpRowIndexUp] - orig[tmpRowIndexUp];
            int tmpbg = current[tmpRowIndexUp+1] - orig[tmpRowIndexUp+1];
            int tmpbb = current[tmpRowIndexUp+2] - orig[tmpRowIndexUp+2];
            result += (labs(br - tmpbr*tmpbr)+labs(bg - tmpbg*tmpbg)+labs(bb - tmpbb*tmpbb))*fixMul2;

            tmpbr = current[tmpRowIndexUp-4] - orig[tmpRowIndexUp-4];
            tmpbg = current[tmpRowIndexUp-4+1] - orig[tmpRowIndexUp-4+1];
            tmpbb = current[tmpRowIndexUp-4+2] - orig[tmpRowIndexUp-4+2];
            result += (labs(br2 - tmpbr*tmpbr)+labs(bg2 - tmpbg*tmpbg)+labs(bb2 - tmpbb*tmpbb))*fixMul2;

            result += (labs(br2 - lastBr)+labs(bg2 - lastBg)+labs(bb2 - lastBb))*fixMul2;
            result += (labs(br4 - lastBrDown)+labs(bg4 - lastBgDown)+labs(bb4 - lastBbDown))*fixMul2;

            lastBr = br2;
            lastBg = bg2;
            lastBb = bb2;

            lastBrDown = br4;
            lastBgDown = bg4;
            lastBbDown = bb4;

            tmpRowIndex += 8;
            tmpRowIndexDown += 8;

        }

        rowIndex += width*2;
        rowIndexDown += width*2;
    }


    return result;



}

//__int64 FastFunctions::computeFittness_2d(unsigned char * current, unsigned char * orig, int length, int width)
//{
//    __int64 result = 0;
//    int height = (length / (width));
//
//    int rowIndex = 0;
//    int rowIndexDown = width;
//
//    for (int y = 0; y < height - 1; y++)
//    {
//        int tmpRowIndex = rowIndex + 4;
//        int tmpRowIndexDown = rowIndexDown + 4;
//
//        int brLast = current[tmpRowIndex - 4] - orig[tmpRowIndex - 4];
//        int bgLast = current[tmpRowIndex - 4+1] - orig[tmpRowIndex - 4+1];
//        int bbLast = current[tmpRowIndex - 4+2] - orig[tmpRowIndex - 4+2];
//        brLast*=brLast;
//        bgLast *= bgLast;
//        bbLast *= bbLast;
//
//        for (int x = 4; x < width; x += 4)
//        {
//            // compute 2d fittness
//
//            int br = current[tmpRowIndex] - orig[tmpRowIndex];
//            int bg = current[tmpRowIndex + 1] - orig[tmpRowIndex + 1];
//            int bb = current[tmpRowIndex + 2] - orig[tmpRowIndex + 2];
//            br *= br;
//            bg *= bg;
//            bb *= bb;
//
//            const int fixMul = 1;
//            const int fixMul2 = 2;
//            result += (br + bb + bg)*fixMul;
//
//            result += (labs(br - brLast)+labs(bg - bgLast) + labs(bb - bbLast))*fixMul2;
//           
//            brLast = br;
//            bgLast = bg;
//            bbLast = bb;
//
//            int tmp = current[tmpRowIndexDown] - orig[tmpRowIndexDown];
//            result += labs(br - tmp*tmp)*fixMul2;
//            tmp = current[tmpRowIndexDown  + 1] - orig[tmpRowIndexDown + 1];
//            result += labs(bg - tmp*tmp)*fixMul2;
//            tmp = current[tmpRowIndexDown + 2] - orig[tmpRowIndexDown + 2];
//            result += labs(bb - tmp*tmp)*fixMul2;
//
//            tmpRowIndex += 4;
//            tmpRowIndexDown += 4;
//
//        }
//
//        rowIndex += width;
//        rowIndexDown += width;
//    }
//
//
//    return result;
//
//
//}


__int64 FastFunctions::computeFittness_2d_2x2(unsigned char * current, unsigned char * orig, int length, int width)
{
    __int64 result = 0;
    int height = (length / (width));

    int rowIndex = 0;
    int rowIndexDown = width;

    for (int y = 0; y < height - 1; y+=2)
    {
        int tmpRowIndex = rowIndex + 4;
        int tmpRowIndexDown = rowIndexDown + 4;

        for (int x = 4; x < width; x += 8)
        {
            // compute 2d fittness

            int br = current[tmpRowIndex] - orig[tmpRowIndex];
            int bg = current[tmpRowIndex + 1] - orig[tmpRowIndex + 1];
            int bb = current[tmpRowIndex + 2] - orig[tmpRowIndex + 2];
            br *= br;
            bg *= bg;
            bb *= bb;

            int br2 = current[tmpRowIndex-4] - orig[tmpRowIndex-4];
            int bg2 = current[tmpRowIndex-4 + 1] - orig[tmpRowIndex-4 + 1];
            int bb2 = current[tmpRowIndex-4 + 2] - orig[tmpRowIndex-4 + 2];
            br2 *= br2;
            bg2 *= bg2;
            bb2 *= bb2;

            int br3 = current[tmpRowIndexDown] - orig[tmpRowIndexDown];
            int bg3 = current[tmpRowIndexDown + 1] - orig[tmpRowIndexDown + 1];
            int bb3 = current[tmpRowIndexDown + 2] - orig[tmpRowIndexDown + 2];
            br3 *= br3;
            bg3 *= bg3;
            bb3 *= bb3;

            int br4 = current[tmpRowIndexDown - 4] - orig[tmpRowIndexDown - 4];
            int bg4 = current[tmpRowIndexDown - 4 + 1] - orig[tmpRowIndexDown - 4 + 1];
            int bb4 = current[tmpRowIndexDown - 4 + 2] - orig[tmpRowIndexDown - 4 + 2];
            br4 *= br4;
            bg4 *= bg4;
            bb4 *= bb4;

            const int fixMul = 1;
            const int fixMul2 = 2;
            result += br*fixMul + bb*fixMul + bg*fixMul + 
                br2*fixMul + bb2*fixMul + bg2*fixMul +
                br3*fixMul + bb3*fixMul + bg3*fixMul +
                br4*fixMul + bb4*fixMul + bg4*fixMul ;

            int tmp = labs(br - br2); result += tmp*fixMul2;
            tmp = labs(bg - bg2); result += tmp*fixMul2;
            tmp = labs(bb - bb2); result += tmp*fixMul2;

            tmp = labs(br4 - br2);  result += tmp*fixMul2;
            tmp = labs(bg4 - bg2);  result += tmp*fixMul2;
            tmp = labs(bb4 - bb2);  result += tmp*fixMul2;

            tmp = labs(br4 - br3);  result += tmp*fixMul2;
            tmp = labs(bg4 - bg3);  result += tmp*fixMul2;
            tmp = labs(bb4 - bb3);  result += tmp*fixMul2;

            tmp = labs(br - br3); result += tmp*fixMul2;
            tmp = labs(bg - bg3); result += tmp*fixMul2;
            tmp = labs(bb - bb3); result += tmp*fixMul2;


            tmpRowIndex += 8;
            tmpRowIndexDown += 8;

        }

        rowIndex += width*2;
        rowIndexDown += width*2;
    }


    return result;


}


__int64 FastFunctions::computeFittnessWithStdDev(unsigned char * curr, unsigned char * orig, int length)
{
    NativeMedian8Bit medR = NativeMedian8Bit();
    NativeMedian8Bit medG = NativeMedian8Bit();
    NativeMedian8Bit medB = NativeMedian8Bit();


    //int index = 0;
    //while (index < length)
    for(int index = 0;index < length;index+=4)
    {
        medR.InsertData(labs(curr[index] - orig[index]));
        medG.InsertData(labs(curr[index + 1] - orig[index + 1]));
        medB.InsertData(labs(curr[index + 2] - orig[index + 2]));

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

__int64 FastFunctions::computeFittnessSumSquare(unsigned char * curr, unsigned char * orig, int length)
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

unsigned __int64 FastFunctions::computeFittnessSumSquareASM( unsigned char* curr, unsigned char* orig, int count )
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
        _mm_prefetch((char *)curr+16,_MM_HINT_T0);
        _mm_prefetch((char *)orig+16,_MM_HINT_T0);

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
            result += mResult.m128i_u32[0]+mResult.m128i_u32[1]+mResult.m128i_u32[2]+mResult.m128i_u32[3];
            mResult = _mm_setzero_si128();
            c = 1000;
        }
        //      result += tmp1.m128i_u16[0]+tmp1.m128i_u16[2]+tmp1.m128i_u16[3]+tmp1.m128i_u16[4]+tmp1.m128i_u16[5]+
        //         tmp1.m128i_u16[6]+tmp1.m128i_u16[7]+tmp2.m128i_u16[0]+tmp2.m128i_u16[4];

        count-=16;
        curr+=16;
        orig+=16;

    }

    result += mResult.m128i_u32[0]+mResult.m128i_u32[1]+mResult.m128i_u32[2]+mResult.m128i_u32[3];


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

