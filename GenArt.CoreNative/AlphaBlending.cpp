#include <math.h>

//#include <mmintrin.h>
#include <emmintrin.h>
#include <smmintrin.h>

#include "AlphaBlending.h"



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
    while(count > 3)
    {
        
        __m128i source =  _mm_cvtsi64_si128(*(((long long*)canvas)+x));
        __m128i source2 = _mm_cvtsi64_si128(*(((long long*)canvas)+x+1));

        //__m128i source =  _mm_loadu_si128((__m128i*)(canvas+x*8));
        //__m128i source2 =  _mm_loadu_si128((__m128i*)(canvas+x*8));
        //_mm_ctv
        //source2  = _mm_srli_si128(source2,8); 

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
        //_mm_storel_pi((__m64*)(canvas+x*8),(__m128)source);
        //_mm_storel_epi64((__m64*)(canvas+x*8),(__m128)source
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

        source  = _mm_mullo_epi16(source,mMullInvAlpha);    // source*invalpha
        source          = _mm_adds_epu16(source,mColorTimeAlpha);     // t

        source          = _mm_srli_epi16(source,8); 

        source        = _mm_packus_epi16(source,source );// mZero );         // pack

        *(((long long*)canvas)+x) = source.m128i_u64[0];// _mm_cvtsi128_si64(source);
    }

}

void Apply2ColorPixelSSE2(unsigned char * canvas,int count,int color, int alpha)
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
        __m128i source = _mm_cvtsi64_si128(*((long long*)(canvas+x)));

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

        *((long long*)(canvas+x)) =  _mm_cvtsi128_si64(source);

        x+=8;
        //canvas += 8;
        count-=2;
    }

}

void Apply8ChanelColorSSE(unsigned char * canvas,int count,unsigned char color, int alpha256)
{
    int invAlpha = 256 - alpha256;

    __m128i mColorTimeAlpha = _mm_set1_epi16(color*alpha256);
    __m128i mMullInvAlpha = _mm_set1_epi16(invAlpha);

    //__m128i mZero = _mm_setzero_si128();
    int x=0;
    while(count > 15)
    {
        
        __m128i source =  _mm_cvtsi64_si128(*(((long long*)canvas)+x));
        __m128i source2 = _mm_cvtsi64_si128(*(((long long*)canvas)+x+1));

        //__m128i source =  _mm_loadu_si128((__m128i*)(canvas+x*8));
        //__m128i source2 =  _mm_loadu_si128((__m128i*)(canvas+x*8));
        //_mm_ctv
        //source2  = _mm_srli_si128(source2,8); 

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
        //_mm_storel_pi((__m64*)(canvas+x*8),(__m128)source);
        //_mm_storel_epi64((__m64*)(canvas+x*8),(__m128)source
        *((((long long*)canvas)+x)) =  source.m128i_u64[0];// _mm_cvtsi128_si64(source);
        *((((long long*)canvas)+x+1)) = source2.m128i_u64[0]; //_mm_cvtsi128_si64(source2);

        x+=2;
        //canvas += 8;
        count-=16;
    }

    while(count > 7)
    {
        __m128i source = _mm_cvtsi64_si128(*(((long long*)canvas)+x));

        //source = _mm_unpacklo_epi8(source, _mm_setzero_si128() );
        source = _mm_cvtepu8_epi16(source);

        source  = _mm_mullo_epi16(source,mMullInvAlpha);    // source*invalpha
        source          = _mm_adds_epu16(source,mColorTimeAlpha);     // t

        source          = _mm_srli_epi16(source,8); 

        source        = _mm_packus_epi16(source,source );// mZero );         // pack

        *(((long long*)canvas)+x) = source.m128i_u64[0];// _mm_cvtsi128_si64(source);
        canvas+=8;
        count-=8;
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

void AlphaBlending::NewFastRowApplyColorSSE(unsigned char * canvas, int countPixel, int color )
{
    int alpha = ((((color) >> 24) & 0xff)*256)/255;

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

void AlphaBlending::NewFastRowApplyColorSSE64(unsigned char * canvas, int countPixel, int color )
{
    
    NewFastRowApplyColorSSE64(canvas,countPixel,color,((((color) >> 24) & 0xff)*256)/255);
    
}
void AlphaBlending::NewFastRowApplyColorSSE64(unsigned char * canvas, int countPixel, int color, int alpha256 )
{
    // convert alpha value from range 0-255 to 0-256
   
    int alpha = alpha256;// ((((color) >> 24) & 0xff)*256)/255;


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

}

void AlphaBlending::NewFastRowApplyColorSSE128(unsigned char * canvas, int countPixel, int color )
{
    NewFastRowApplyColorSSE128(canvas,countPixel,color,((((color) >> 24) & 0xff)*256)/255);
}
void AlphaBlending::NewFastRowApplyColorSSE128(unsigned char * canvas, int countPixel, int color, int alpha256 )
{
    // convert alpha value from range 0-255 to 0-256


    unsigned char * line = canvas;

    if(countPixel < 64)
    {
        //ApplyColorPixelSSE(line,countPixel,color,alpha);
        NewFastRowApplyColorSSE64(line,countPixel,color,alpha256);
    }
    else
    {
        unsigned int tmp = (((unsigned int)line) & 0xf )/4; 
        tmp = 4-tmp;
        if(tmp > 0)
        {



            ApplyColorPixelSSE(line,tmp,color,alpha256);
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

        //if(countPixel > 3)
        {
            int tmpLen = countPixel - (countPixel&3);
            //Apply4ColorPixelSSE(line,r,g,b,alpha);
            Apply4ColorPixelSSE(line,tmpLen,color,alpha256);
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
            ApplyColorPixelSSE(line,countPixel,color,alpha256);
        } 
    }
}

void AlphaBlending::FastChanelRowApplyColor(unsigned char * canvas, int count, unsigned char color, int alpha256)
{    
    int invAlpha = 256-alpha256;

    int c = alpha256*color;
    
    while(count > 3)
    {
        unsigned int tc = *canvas;
        unsigned int tc2 = canvas[1];
        unsigned int tc3 = canvas[2];
        unsigned int tc4 = canvas[3];

        
        tc = (c + (tc*invAlpha))>>8;
        tc2 = (c + (tc2*invAlpha))>>8;
        tc3 = (c + (tc3*invAlpha))>>8;
        tc4 = (c + (tc4*invAlpha))>>8;
        
        *canvas = (unsigned char)tc;
        canvas[1] = (unsigned char)tc2;
        canvas[2] = (unsigned char)tc3;
        canvas[3] = (unsigned char)tc4;
        
        count -=4;
        canvas+=4;
    }

    

    while(count > 0)
    {
        unsigned int tc = *canvas;
        
        tc = (c + (tc*invAlpha))>>8;
        
        *canvas = (unsigned char)tc;
        
        count --;
        canvas++;
    }
}

void AlphaBlending::FastChanelRowApplyColor8SSE(unsigned char * canvas, int count, unsigned char color, int alpha256)
{    
     // convert alpha value from range 0-255 to 0-256
   
    int alpha = alpha256;// ((((color) >> 24) & 0xff)*256)/255;


    unsigned char * line = canvas;

    if(count < 16)
    {
        FastChanelRowApplyColor(line,count,color,alpha256);
    }
    else
    {
        // fix bad align
        // move if address is 0xc || 0x4
        /*unsigned int tmp = ((((unsigned int)line) & 0xf)/4)&1; 

        if((tmp != 0) //&& countPixel > 0
            )
        {
            ApplyColorPixelSSE(line,color,alpha);

            countPixel -= 1;
            line+=4;
        }*/

        //if(countPixel > 1)
        {
            int tmpLen = count - (count&7);
            Apply8ChanelColorSSE(line,tmpLen, color,alpha256);

            count-= tmpLen;
            line+=tmpLen;
        }


        if(count > 0)
        {
            FastChanelRowApplyColor(line,count,color,alpha256);
        }
    }

}
   


void AlphaBlending::FastRowApplyColor(unsigned char * canvas, int len, int r , int g, int b, int alpha)
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

void AlphaBlending::FastRowApplyColorSSE64(unsigned char * canvas, int len, int r , int g, int b, int alpha)
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


void AlphaBlending::FastRowApplyColorSSE128(unsigned char * canvas, int len, int r , int g, int b, int alpha)
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

void AlphaBlending::FastRowsApplyColor(unsigned char * canvas, int canvasWidth, short int * ranges, int rlen, int rangeStartY, int r , int g, int b, int alpha)
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





void AlphaBlending::FastRowsApplyColorSSE64(unsigned char * canvas, int canvasWidth, short int * ranges, int rlen, int rangeStartY, int r,int g, int b, int alpha)
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

void AlphaBlending::FastRowsApplyColorSSE128(unsigned char * canvas, int canvasWidth, short int * ranges, int rlen, int rangeStartY, int r,int g, int b, int alpha)
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

void AlphaBlending::FastRowsApplyColorSSE128_test(unsigned char * canvas, int canvasWidth, short int * ranges, int rlen, int rangeStartY, int r,int g, int b, int alpha)
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

