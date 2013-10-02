
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
      _mm_prefetch((char *)canvas+16,_MM_HINT_T0);
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


 int FastFunctions::ApplyColor(int colorChanel, int axrem, int rem)
{
    return ((axrem + rem * colorChanel) >> 16);
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


        len -= 4;
        line+=4;
    }
}

void FastFunctions::FastRowApplyColorSSE64(unsigned char * canvas, int len, int r , int g, int b, int alpha)
{
    // convert alpha value from range 0-255 to 0-256
    alpha = (alpha*256)/255;

    int invAlpha = 256-alpha;

    unsigned char * line = canvas;

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

    if(len > 0)
    {
        ApplyColorPixelSSE(line,r,g,b,alpha);

        len -= 1;
        line+=4;
    }


}


void FastFunctions::FastRowApplyColorSSE128(unsigned char * canvas, int len, int r , int g, int b, int alpha)
{
    alpha = (alpha*256)/255;

    // implementace (color*alpha + (255-alpha)*source)/255 

    int invAlpha = 256-alpha;

    unsigned char * line = canvas;

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

      int invAlpha = 256-alpha;

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

      int invAlpha = 256-alpha;

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
        //_mm_stream_si128((__m128i *)M, f);
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
            result += (medB.ValueSum() + medB.SumStdDev());
            result += (medG.ValueSum() + medG.SumStdDev());
            result += (medR.ValueSum() + medR.SumStdDev());

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

    __m128i mMaskGAAnd = _mm_setr_epi8(0,0xff,0,0xff,0,0xff,0,0xff,0,0xff,0,0xff,0,0xff,0,0xff);
    __m128i mMaskEven = _mm_setr_epi16(0xffff,0,0xffff,0,0xffff,0,0xffff,0);
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

