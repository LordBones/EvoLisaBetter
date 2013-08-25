// ForDebuging.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <emmintrin.h>

int _tmain(int argc, _TCHAR* argv[])
{
    unsigned char field[] = {13,14,15,255};

    int r = 7;
    int g = 8;
    int b = 9;
    int alpha = 256;

      __m128i nNull = _mm_cvtsi32_si128(0);
      __m128i mColor = _mm_setr_epi16(b,g,r,0,0,0,0,0);
      __m128i mMullAlpha = _mm_setr_epi16(alpha,alpha,alpha,1,1,1,1,1);
      __m128i mDivShr8 = _mm_setr_epi16(8,8,8,0,0,0,0,0);
      __m128i mMaskAnd = _mm_setr_epi16(0xffff,0xffff,0xffff,0,0,0,0,0);



    __m128i source = _mm_cvtsi32_si128(*((int*)field));
              source = _mm_unpacklo_epi8(source, nNull );

              __m128i tmp1 = _mm_sub_epi16(mColor,source);
              tmp1 = _mm_and_si128(tmp1,mMaskAnd);
              tmp1 = _mm_mullo_epi16(tmp1,mMullAlpha);
              tmp1 = _mm_srai_epi16(tmp1,8);
              source = _mm_add_epi16(tmp1,source);
              source = _mm_packus_epi16(source, nNull );
            

              *((int*)field) =  _mm_cvtsi128_si32(source);

	return 0;
}

