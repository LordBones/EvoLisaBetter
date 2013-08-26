// ForDebuging.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <emmintrin.h>

int _tmain(int argc, _TCHAR* argv[])
{
    // implementace michani barev podle vzorce
    // (color*alpha + (255-alpha)*source)/255
    // vzorec pro rychle deleni 255 = (value+1 + (value >> 8)) >> 8;

    unsigned char field[] = {13,14,15,255};



    int r = 7;
    int g = 8;
    int b = 254;
    int alpha = 100;
    int invAlpha = 255 - alpha;

      __m128i nNull = _mm_cvtsi32_si128(0);
      __m128i mColorTimeAlpha = _mm_setr_epi16(b*alpha,g*alpha,r*alpha,0,b*alpha,g*alpha,r*alpha,0);

      __m128i mMullInvAlpha = _mm_setr_epi16(invAlpha,invAlpha,invAlpha,1,invAlpha,invAlpha,invAlpha,1);
      __m128i mMaskAnd = _mm_setr_epi16(0xffff,0xffff,0xffff,0,0xffff,0xffff,0xffff,0);



      __m128i source = _mm_cvtsi32_si128(*((int*)field));
      source = _mm_unpacklo_epi8(source, nNull );
      __m128i tmp1 = _mm_mullo_epi16(source,mMullInvAlpha); // source*invalpha
      tmp1 = _mm_adds_epu16(tmp1,mColorTimeAlpha);   // t

      // rychle deleni 255
      __m128i tmp2 = _mm_adds_epu16(tmp1,_mm_set1_epi16(1));   // t + 1
      tmp1 = _mm_srli_epi16(tmp1,8);  //t >> 8
      tmp2 = _mm_adds_epu16(tmp1,tmp2); //(t+1)+(t >> 8)
      tmp2 = _mm_srli_epi16(tmp2,8);  //((t+1)+(t >> 8)) >> 8
      source = _mm_andnot_si128(mMaskAnd,source); // mask alpha
      tmp2 = _mm_and_si128(mMaskAnd,tmp2); // mask colors
      source = _mm_or_si128(tmp2,source); // 00XXXXXX | XX000000 = xxxxxxxx

      source = _mm_packus_epi16(source, nNull ); // pack


      *((int*)field) =  _mm_cvtsi128_si32(source);

	return 0;
}

