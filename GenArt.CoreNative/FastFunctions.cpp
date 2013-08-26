
#include <math.h>
//#include <iostream>
#include <mmintrin.h>
#include <emmintrin.h>


#include "FastFunctions.h"
#include "NativeMedian8bit.h"


 int FastFunctions::ApplyColor(int colorChanel, int axrem, int rem)
{
    return ((axrem + rem * colorChanel) >> 16);
}

void FastFunctions::FastRowApplyColor(unsigned char * canvas, int len, int colorABRrem, int colorAGRrem, int colorARRrem, int colorRem)
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

      int invAlpha = 255-alpha;
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

			  //((axrem + rem * colorChanel) >> 16)

			  unsigned int tb = *line;
			  unsigned int tg = line[1];
			  unsigned int tr = line[2];

			  //tb = tb + ((b-tb)*alpha)/255;
			  //tg=tg + ((g-tg)*alpha)/255;
			  //tr=tr + ((r-tr)*alpha)/255;

              /*tb = (b*alpha + (tb*invAlpha))/255;
			  tg=(g*alpha + (tg*invAlpha))/255;
			  tr=(r*alpha + (tr*invAlpha))/255;
*/
               tb = Fast255div(b*alpha + (tb*invAlpha));
			  tg=Fast255div(g*alpha + (tg*invAlpha));
			  tr=Fast255div(r*alpha + (tr*invAlpha));

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

  

  void FastFunctions::FastRowsApplyColor2(unsigned char * canvas, int canvasWidth, short int * ranges, int rlen, int rangeStartY, int r,int g, int b, int alpha)
  {

     // pFastRowsApplyColor2(canvas,canvasWidth,ranges,rlen,rangeStartY,colorABRrem,colorAGRrem,colorARRrem,colorRem);

	

      // implementace (color*alpha + (255-alpha)*source)/255 
      // implementace deleni 255  v sse s pomoci tohoto vzorce (value+1 + (value >> 8)) >> 8;
      
	  int rowStartIndex = (rangeStartY) * canvasWidth;

	  short * end = ranges+rlen;

      int invAlpha = 255-alpha;
      
      // __m128i nNull = _mm_cvtsi32_si128(0);
      __m128i mColorTimeAlpha = _mm_setr_epi16(b*alpha,g*alpha,r*alpha,0,b*alpha,g*alpha,r*alpha,0);

      __m128i mMullInvAlpha = _mm_setr_epi16(invAlpha,invAlpha,invAlpha,1,invAlpha,invAlpha,invAlpha,1);
      __m128i mMaskAnd = _mm_setr_epi16(0xffff,0xffff,0xffff,0,0xffff,0xffff,0xffff,0);


      

	  //int indexY = minY * this._canvasWidth;
	  //for (int y  = 0; y < rlen; y+=2)//, indexY += this._canvasWidth)
	  while(ranges < end)
	  {


		  int index = rowStartIndex + (*ranges) * 4;

		  //
		  int len = ranges[1] - *ranges+1; 
		  unsigned char * line = canvas+index;
		  
          
          while(len > 1)
          {
              __m128i source = _mm_cvtsi64_si128(*((long long*)line));

              source = _mm_unpacklo_epi8(source, _mm_setzero_si128() );
              __m128i tmp1  = _mm_mullo_epi16(source,mMullInvAlpha);    // source*invalpha
              tmp1          = _mm_adds_epu16(tmp1,mColorTimeAlpha);     // t

                                                                        // rychle deleni 255
              __m128i tmp2  = _mm_adds_epu16(tmp1,_mm_set1_epi16(1));   // t + 1
              tmp1          = _mm_srli_epi16(tmp1,8);                   //t >> 8
              tmp2          = _mm_adds_epu16(tmp1,tmp2);                //(t+1)+(t >> 8)
              tmp2          = _mm_srli_epi16(tmp2,8);                   //((t+1)+(t >> 8)) >> 8
              source        = _mm_andnot_si128(mMaskAnd,source);        // mask alpha
              tmp2          = _mm_and_si128(mMaskAnd,tmp2);             // mask colors
              source        = _mm_or_si128(tmp2,source);                // 00XXXXXX | XX000000 = xxxxxxxx

              source        = _mm_packus_epi16(source, _mm_setzero_si128() );         // pack

              *((long long*)line) =  _mm_cvtsi128_si64(source);


			  len -= 2;
			  line+=8;
          }


		  while(len > 0)
		  {
              __m128i source = _mm_cvtsi32_si128(*((int*)line));

              source = _mm_unpacklo_epi8(source, _mm_setzero_si128() );
              __m128i tmp1  = _mm_mullo_epi16(source,mMullInvAlpha);    // source*invalpha
              tmp1          = _mm_adds_epu16(tmp1,mColorTimeAlpha);     // t

                                                                        // rychle deleni 255
              __m128i tmp2  = _mm_adds_epu16(tmp1,_mm_set1_epi16(1));   // t + 1
              tmp1          = _mm_srli_epi16(tmp1,8);                   //t >> 8
              tmp2          = _mm_adds_epu16(tmp1,tmp2);                //(t+1)+(t >> 8)
              tmp2          = _mm_srli_epi16(tmp2,8);                   //((t+1)+(t >> 8)) >> 8
              source        = _mm_andnot_si128(mMaskAnd,source);        // mask alpha
              tmp2          = _mm_and_si128(mMaskAnd,tmp2);             // mask colors
              source        = _mm_or_si128(tmp2,source);                // 00XXXXXX | XX000000 = xxxxxxxx

              source        = _mm_packus_epi16(source, _mm_setzero_si128() );         // pack

              *((int*)line) =  _mm_cvtsi128_si32(source);


			  len -= 1;
			  line+=4;
		  }

		  rowStartIndex += canvasWidth;
		  ranges+=2;
	  }

  }

  

  static unsigned short alphaMMXmul_const1[4] = {256,256,256,256};	      
static unsigned alphaMMXmul_0[2] = {1,1};				      
									      
void MixAlphaMMX32(void *dest,const void *src,unsigned len,unsigned opacity) {
									      
/*__asm {									      
	mov		edi,dest					      
	mov		ebx,src						      
	mov		ecx,len						      
	mov		edx,opacity					      
									      
	movzx   eax,dl                                                        
   	movq    mm7,[alphaMMXmul_const1]				      
									      
	shl     eax,16							      
	add     eax,edx							      
   	mov     [alphaMMXmul_0],eax 					      
   	mov     [alphaMMXmul_0 + 4],eax					      
   	movq    mm6,[alphaMMXmul_0]	; mm6(X) = alpha (4 words)	      
   	pxor    mm5,mm5							      
   	psubusw mm7,mm6			; mm7(Y) = 256 – alpha (4 words)      
									      
ALIGN 16								      
MixAlphaMMX32_MainLoop: 						      
									      
 movd    	mm0,[edi]	; mm0(A) = 0 0 0 0 | 0 Ra Ga Ba	 	      
 add     	edi,4							      
 movd    	mm1,[ebx]	; mm1(B) = 0 0 0 0 | 0 Rb Gb Bb	  	      
 add     	ebx,4							      
 punpcklbw 	mm0,mm5		; mm0 = 0 0 0 Ra | 0 Ga 0 Ba		      
 punpcklbw 	mm1,mm5		; mm1 = 0 0 0 Rb | 0 Gb 0 Bb		      
 pmullw  	mm0,mm6		; mm0 = 0 Ra*X | Ga*X Ba*X		      
 pmullw  	mm1,mm7		; mm1 = 0 Rb*Y | Gb*Y Bb*Y 		      
 paddusw 	mm0,mm1		; mm0 = 0 Ra*X+Rb*y | Ga*X+Gb*y Ba*X+Bb*Y     
 psrlw		mm0,8		; mm0 = 0 0 0 Rc | 0 Gc 0 Bc		      
 packuswb 	mm0,mm0		; mm0 = 0 0 0 0 | 0 Rc Gc Bc		      
 movd    	[edi-4],mm0						      
 dec		ecx							      
 jnz   		MixAlphaMMX32_MainLoop 					      
 emms									      
}*/									      
}	

  void FillSSEInt32(unsigned long * M, long Fill, unsigned int Count)
{
    __m128i f;

    // Fix mis-alignment.
    if (((unsigned int)M) & 0xf)
    {
		unsigned int tmp = ((unsigned int)M) & 0xf; 
		
        switch (tmp)
        {
            case 0x4: if (Count >= 1) { *M++ = Fill; Count--; }
            case 0x8: if (Count >= 1) { *M++ = Fill; Count--; }
            case 0xc: if (Count >= 1) { *M++ = Fill; Count--; }
        }
    }

    f.m128i_i32[0] = Fill;
    f.m128i_i32[1] = Fill;
    f.m128i_i32[2] = Fill;
    f.m128i_i32[3] = Fill;

	while (Count >= 4)
    {
        _mm_store_si128((__m128i *)M, f);
        M += 4;
        Count -= 4;
	}

	// Fill remaining LONGs.
    switch (Count & 0x3)
    {
        case 0x3: *M++ = Fill;
        case 0x2: *M++ = Fill;
        case 0x1: *M++ = Fill;
    }
}

void FastFunctions::ClearFieldByColor(unsigned char * curr, int length, int color)
{
	FillSSEInt32((unsigned long *)curr, color,length/4);
	//std::fill((unsigned int *)curr,((unsigned int *)curr)+length/4,  color);
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
                medB.InsertData(labs(curr[index] - orig[index]));
                medG.InsertData(labs(curr[index + 1] - orig[index + 1]));
                medR.InsertData(labs(curr[index + 2] - orig[index + 2]));
              //  index += 4;
            }


            __int64 result = 0;
            result += (medB.ValueSum() + medB.SumStdDev()*2);
            result += (medG.ValueSum() + medG.SumStdDev() * 2);
            result += (medR.ValueSum() + medR.SumStdDev() * 2);

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

unsigned __int64 FastFunctions::computeFittnessSumSquareASM( unsigned char* p1, unsigned char* p2, int count )
{
    int high32 = 0;

	/*_asm
	{
		push	ebx
		push	esi
		push	edi

		mov		esi, p1
		mov		edi, p2
		xor		eax, eax
again:
		dec		count
		js		done

		movzx	ebx, byte ptr [esi]
		movzx	edx, byte ptr [edi]
		sub		edx, ebx
		imul	edx, edx

		movzx	ebx, byte ptr [esi+1]
		movzx	ecx, byte ptr [edi+1]
		sub		ebx, ecx
		imul	ebx, ebx
		add		edx, ebx

		movzx	ebx, byte ptr [esi+2]
		movzx	ecx, byte ptr [edi+2]
		sub		ebx, ecx
		imul	ebx, ebx
		add		edx, ebx

		add		esi, 4
		add		edi, 4

		add		eax, edx
		jnc		again

		inc		high32
		jmp		again
done:
		mov		edx, high32

		pop		edi
		pop		esi
		pop		ebx
	}*/

    return 0;

}

