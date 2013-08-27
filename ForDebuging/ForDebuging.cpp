// ForDebuging.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <emmintrin.h>
#include <smmintrin.h>


int _tmain(int argc, _TCHAR* argv[])
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


	return 0;
}

