#include <math.h>

//#include <mmintrin.h>
#include <emmintrin.h>
#include <smmintrin.h>
#include <immintrin.h>
#include "AlphaBlending.h"



void ApplyColorPixel(unsigned char * canvas, int color, int alpha256)
{
	//if(alpha> 0) alpha++;
	// convert alpha value from range 0-255 to 0-256
	//alpha = (alpha * 256) / 255;

	int invAlpha = 256 - alpha256;

	unsigned char * line = canvas;

	int r = (color >> 16) & 0xff;
	int g = (color >> 8) & 0xff;
	int b = color & 0xff;

	b *= alpha256;
	g *= alpha256;
	r *= alpha256;

	unsigned int tb = *line;
	unsigned int tg = line[1];
	unsigned int tr = line[2];


	tb = (b + (tb*invAlpha)) >> 8;
	tg = (g + (tg*invAlpha)) >> 8;
	tr = (r + (tr*invAlpha)) >> 8;

	/*tb = tb + (((b-tb)*alpha)>>8);
	tg=tg + (((g-tg)*alpha)>>8);
	tr=tr + (((r-tr)*alpha)>>8);*/

	*line = (unsigned char)tb;
	line[1] = (unsigned char)tg;
	line[2] = (unsigned char)tr;
}

//void ApplyColorPixel(unsigned char * canvas, int color, int alpha)
//{
//	//if(alpha> 0) alpha++;
//	// convert alpha value from range 0-255 to 0-256
//	alpha = (alpha * 256) / 255;
//
//	int invAlpha = 256 - alpha;
//
//	unsigned char * line = canvas;
//
//	int r = (color >> 16) & 0xff;
//	int g = (color >> 8) & 0xff;
//	int b = color & 0xff;
//
//	b *= alpha;
//	g *= alpha;
//	r *= alpha;
//
//	unsigned int tb = *line;
//	unsigned int tg = line[1];
//	unsigned int tr = line[2];
//
//
//	tb = (b + (tb*invAlpha)) >> 8;
//	tg = (g + (tg*invAlpha)) >> 8;
//	tr = (r + (tr*invAlpha)) >> 8;
//
//	/*tb = tb + (((b-tb)*alpha)>>8);
//	tg=tg + (((g-tg)*alpha)>>8);
//	tr=tr + (((r-tr)*alpha)>>8);*/
//
//	*line = (unsigned char)tb;
//	line[1] = (unsigned char)tg;
//	line[2] = (unsigned char)tr;
//}

void ApplyColorPixel(unsigned char * canvas, int len, int color, int alpha256)
{
	//if(alpha> 0) alpha++;
	// convert alpha value from range 0-255 to 0-256
	//alpha = (alpha * 256) / 255;

	int invAlpha = 256 - alpha256;

	unsigned char * line = canvas;

	int r = (color >> 16) & 0xff;
	int g = (color >> 8) & 0xff;
	int b = color & 0xff;

	b *= alpha256;
	g *= alpha256;
	r *= alpha256;

	while (len > 0)
	{
		unsigned int tb = *line;
		unsigned int tg = line[1];
		unsigned int tr = line[2];


		tb = (b + (tb*invAlpha)) >> 8;
		tg = (g + (tg*invAlpha)) >> 8;
		tr = (r + (tr*invAlpha)) >> 8;

		/*tb = tb + (((b-tb)*alpha)>>8);
		tg=tg + (((g-tg)*alpha)>>8);
		tr=tr + (((r-tr)*alpha)>>8);*/

		*line = (unsigned char)tb;
		line[1] = (unsigned char)tg;
		line[2] = (unsigned char)tr;

		len--;
		line += 4;
	}
}

void ApplyColorPixelFaster(unsigned char const * canvas, int len, const int color, const int alpha256)
{
	/*unsigned alpha = src >> 24;
	alpha += (alpha > 0);
*/
	unsigned int alphaNeg = 256 - alpha256;
	unsigned long long drb = color & 0xff00ff;
	unsigned int dg = color & 0x00ff00;
	drb = drb * alpha256;
	dg = dg * alpha256;

	unsigned int * line = (unsigned int *)canvas;

	while (len > 0)
	{
		unsigned int dest = *line;

		unsigned int srb = (dest) & 0xff00ff;
		unsigned int sg = (dest) & 0x00ff00;


		unsigned int orb = ((drb + (srb *alphaNeg)) >> 8) & 0xff00ff;
		unsigned int og = ((dg + (sg * alphaNeg)) >> 8) & 0x00ff00;


		*line = (unsigned int)orb + og;

		line++;
		len--;
	}

}



/*
alpha : 0 - 256
*/
void ApplyColorPixelSSE(unsigned char * canvas, int color, int alpha)
{
	int invAlpha = 256 - alpha;


	__m128i mColorTimeAlpha = _mm_setr_epi32(color & 0xffffff, 0, 0, 0);
	mColorTimeAlpha = _mm_cvtepu8_epi16(mColorTimeAlpha);
	__m128i mColorTimeAlphaMull = _mm_set1_epi16((short)alpha);
	mColorTimeAlpha = _mm_mullo_epi16(mColorTimeAlpha, mColorTimeAlphaMull);

	__m128i mMullInvAlpha = _mm_set1_epi16(invAlpha);
	mMullInvAlpha.m128i_i16[3] = 256;

	__m128i source = _mm_cvtsi32_si128(*((int*)canvas));

	//source = _mm_unpacklo_epi8(source, _mm_setzero_si128() );
	source = _mm_cvtepu8_epi16(source);

	__m128i tmp1 = _mm_mullo_epi16(source, mMullInvAlpha);    // source*invalpha
	tmp1 = _mm_adds_epu16(tmp1, mColorTimeAlpha);     // t

	tmp1 = _mm_srli_epi16(tmp1, 8);

	//source        = _mm_blend_epi16(tmp1,source,0x88);        // a,b,c,d  | e,f,g,h => a,b,c,h

	//source        = _mm_andnot_si128(mMaskAnd,source);        // mask alpha
	//tmp2          = _mm_and_si128(mMaskAnd,tmp2);             // mask colors
	//source        = _mm_or_si128(tmp2,source);                // 00XXXXXX | XX000000 = xxxxxxxx

	source = _mm_packus_epi16(tmp1, tmp1);// _mm_setzero_si128() );         // pack

	*((int*)canvas) = _mm_cvtsi128_si32(source);

}

/*
alpha : 0 - 256
*/
void ApplyColorPixelSSE(unsigned char * canvas, int count, int color, int alpha)
{
	int invAlpha = 256 - alpha;

	__m128i mColorTimeAlpha = _mm_setr_epi32(color & 0xffffff, 0, 0, 0);

	mColorTimeAlpha = _mm_cvtepu8_epi16(mColorTimeAlpha);
	__m128i mColorTimeAlphaMull = _mm_set1_epi16((short)alpha);
	mColorTimeAlpha =
		_mm_mullo_epi16(mColorTimeAlpha, mColorTimeAlphaMull);

	__m128i mMullInvAlpha = _mm_set1_epi16(invAlpha);
	//mMullInvAlpha.m128i_i16[3] = 256;
	_mm_insert_epi16(mMullInvAlpha, 256, 3);

	// __m128i mZero =  _mm_setzero_si128();
	int x = 0;
	while (count > 1)
	{
		__m128i source = _mm_cvtsi32_si128(*(((int*)canvas) + x));
		__m128i source2 = _mm_cvtsi32_si128(*(((int*)canvas) + x + 1));
		source = _mm_cvtepu8_epi16(source);
		source2 = _mm_cvtepu8_epi16(source2);

		__m128i tmp1 = _mm_mullo_epi16(source, mMullInvAlpha);    // source*invalpha
		tmp1 = _mm_adds_epu16(tmp1, mColorTimeAlpha);     // t

		tmp1 = _mm_srli_epi16(tmp1, 8);
		__m128i tmp2 = _mm_mullo_epi16(source2, mMullInvAlpha);    // source*invalpha
		tmp2 = _mm_adds_epu16(tmp2, mColorTimeAlpha);     // t

		tmp2 = _mm_srli_epi16(tmp2, 8);

		source = _mm_packus_epi16(tmp1, tmp1);//mZero );         // pack
		source2 = _mm_packus_epi16(tmp2, tmp2);//mZero );         // pack

		*(((int*)canvas) + x) = _mm_cvtsi128_si32(source);
		*(((int*)canvas) + x + 1) = _mm_cvtsi128_si32(source2);

		x += 2;
		count -= 2;
	}

	if (count > 0)
	{
		__m128i source = _mm_cvtsi32_si128(*(((int*)canvas) + x));
		source = _mm_cvtepu8_epi16(source);

		__m128i tmp1 = _mm_mullo_epi16(source, mMullInvAlpha);    // source*invalpha
		tmp1 = _mm_adds_epu16(tmp1, mColorTimeAlpha);     // t

		tmp1 = _mm_srli_epi16(tmp1, 8);

		source = _mm_packus_epi16(tmp1, tmp1);//mZero );         // pack

		*(((int*)canvas) + x) = _mm_cvtsi128_si32(source);

		x++;
		count--;
	}

}







void Apply2ColorPixelSSE_aligned(unsigned char const * canvas, int count, const int color, const int alpha)
{
	int invAlpha = 256 - alpha;

	__m128i mColorTimeAlpha = _mm_set1_epi32(color & 0xffffff);

	mColorTimeAlpha = _mm_cvtepu8_epi16(mColorTimeAlpha);
	__m128i mColorTimeAlphaMull = _mm_set1_epi16((short)alpha);
	mColorTimeAlpha = _mm_mullo_epi16(mColorTimeAlpha, mColorTimeAlphaMull);

	__m128i mMullInvAlpha = _mm_set1_epi16(invAlpha);
	/*__m128i mMullInvAlpha = _mm_set_epi16(256,invAlpha, invAlpha, invAlpha,256, invAlpha, invAlpha, invAlpha);*/

	_mm_insert_epi16(mMullInvAlpha, 256, 3);
	_mm_insert_epi16(mMullInvAlpha, 256, 7);
	//mMullInvAlpha.m128i_i16[3] = 256;
	//mMullInvAlpha.m128i_i16[7] = 256;

	//__m128i mZero = _mm_setzero_si128();
	int x = 0;
	while (count > 3)
	{
		if (((unsigned int)canvas & 0x0f) != 0) return;

		__m128i source = _mm_loadu_si128((__m128i*)(canvas + x));
		__m128i source2 = _mm_unpackhi_epi8(source, _mm_setzero_si128());// _mm_srli_si128(source, 8);

		//__m128i source =  _mm_cvtsi64_si128(*(((long long*)canvas)+x));
		//__m128i source2 = _mm_cvtsi64_si128(*(((long long*)canvas)+x+1));

		//_mm_ctv

		source = _mm_unpacklo_epi8(source, _mm_setzero_si128());
		//source2 = _mm_unpacklo_epi8(source2,_mm_setzero_si128());

		//source = _mm_cvtepu8_epi16(source);
		//source2 = _mm_cvtepu8_epi16(source2);

		source = _mm_mullo_epi16(source, mMullInvAlpha);    // source*invalpha
		source2 = _mm_mullo_epi16(source2, mMullInvAlpha);    // source*invalpha
		source = _mm_adds_epu16(source, mColorTimeAlpha);     // t
		source2 = _mm_adds_epu16(source2, mColorTimeAlpha);     // t
		source = _mm_srli_epi16(source, 8);
		source2 = _mm_srli_epi16(source2, 8);

		//source        = _mm_packus_epi16(source,source );// mZero );        
		//source2        = _mm_packus_epi16(source2,source2 );// mZero );     

		source = _mm_packus_epi16(source, source2);
		//source = _mm_unpacklo_epi64(source, source2);

		_mm_store_si128((__m128i*)(canvas + x), source);
		//_mm_stream_si128((__m128i*)(canvas + x * 8), source);
		//*((((long long*)canvas)+x)) =  source.m128i_u64[0];// _mm_cvtsi128_si64(source);
		//*((((long long*)canvas)+x+1)) = source2.m128i_u64[0]; //_mm_cvtsi128_si64(source2);

		x += 16;
		//canvas += 16;
		//canvas += 8;
		count -= 4;
	}

	if (count > 1)
	{
		__m128i source = _mm_cvtsi64_si128(*(((long long*)(canvas + x))));

		//source = _mm_unpacklo_epi8(source, _mm_setzero_si128() );
		source = _mm_cvtepu8_epi16(source);

		source = _mm_mullo_epi16(source, mMullInvAlpha);    // source*invalpha
		source = _mm_adds_epu16(source, mColorTimeAlpha);     // t

		source = _mm_srli_epi16(source, 8);

		source = _mm_packus_epi16(source, source);// mZero );         // pack

		*((long long*)(canvas + x)) = source.m128i_u64[0];// _mm_cvtsi128_si64(source);
	}

}

void Apply2ColorPixelSSE_aligned3(unsigned char * canvas, int count, int color, int alpha)
{
	int invAlpha = 256 - alpha;

	__m128i mColorTimeAlpha = _mm_set1_epi32(color & 0xffffff);

	mColorTimeAlpha = _mm_cvtepu8_epi16(mColorTimeAlpha);
	__m128i mColorTimeAlphaMull = _mm_set1_epi16((short)alpha);
	mColorTimeAlpha = _mm_mullo_epi16(mColorTimeAlpha, mColorTimeAlphaMull);

	__m128i mMullInvAlpha = _mm_set1_epi16(invAlpha);
	/*__m128i mMullInvAlpha = _mm_set_epi16(256,invAlpha, invAlpha, invAlpha,256, invAlpha, invAlpha, invAlpha);*/

	_mm_insert_epi16(mMullInvAlpha, 256, 3);
	_mm_insert_epi16(mMullInvAlpha, 256, 7);
	//mMullInvAlpha.m128i_i16[3] = 256;
	//mMullInvAlpha.m128i_i16[7] = 256;

	//__m128i mZero = _mm_setzero_si128();
	int x = 0;
	while (count > 3)
	{
		if (((unsigned int)canvas & 0x0f) != 0) return;

		__m128i source = _mm_loadu_si128((__m128i*)(canvas + x));
		__m128i source2 = _mm_unpackhi_epi8(source, _mm_setzero_si128());// _mm_srli_si128(source, 8);

																		 //__m128i source =  _mm_cvtsi64_si128(*(((long long*)canvas)+x));
																		 //__m128i source2 = _mm_cvtsi64_si128(*(((long long*)canvas)+x+1));

																		 //_mm_ctv

		source = _mm_unpacklo_epi8(source, _mm_setzero_si128());
		//source2 = _mm_unpacklo_epi8(source2,_mm_setzero_si128());

		//source = _mm_cvtepu8_epi16(source);
		//source2 = _mm_cvtepu8_epi16(source2);

		source = _mm_mullo_epi16(source, mMullInvAlpha);    // source*invalpha
		source2 = _mm_mullo_epi16(source2, mMullInvAlpha);    // source*invalpha
		source = _mm_adds_epu16(source, mColorTimeAlpha);     // t
		source2 = _mm_adds_epu16(source2, mColorTimeAlpha);     // t
		source = _mm_srli_epi16(source, 8);
		source2 = _mm_srli_epi16(source2, 8);

		//source        = _mm_packus_epi16(source,source );// mZero );        
		//source2        = _mm_packus_epi16(source2,source2 );// mZero );     

		source = _mm_packus_epi16(source, source2);
		//source = _mm_unpacklo_epi64(source, source2);

		_mm_store_si128((__m128i*)(canvas + x), source);
		//_mm_stream_si128((__m128i*)(canvas + x * 8), source);
		//*((((long long*)canvas)+x)) =  source.m128i_u64[0];// _mm_cvtsi128_si64(source);
		//*((((long long*)canvas)+x+1)) = source2.m128i_u64[0]; //_mm_cvtsi128_si64(source2);

		x += 16;
		//canvas += 16;
		//canvas += 8;
		count -= 4;
	}

	if (count > 1)
	{
		__m128i source = _mm_cvtsi64_si128(*(((long long*)(canvas + x))));

		//source = _mm_unpacklo_epi8(source, _mm_setzero_si128() );
		source = _mm_cvtepu8_epi16(source);

		source = _mm_mullo_epi16(source, mMullInvAlpha);    // source*invalpha
		source = _mm_adds_epu16(source, mColorTimeAlpha);     // t

		source = _mm_srli_epi16(source, 8);

		source = _mm_packus_epi16(source, source);// mZero );         // pack

		*((long long*)(canvas + x)) = source.m128i_u64[0];// _mm_cvtsi128_si64(source);
	}

}


void Apply2ColorPixelSSE2(unsigned char * canvas, int count, int color, int alpha)
{
	int invAlpha = 256 - alpha;

	__m128i mColorTimeAlpha = _mm_set1_epi32(color & 0xffffff);

	mColorTimeAlpha = _mm_cvtepu8_epi16(mColorTimeAlpha);
	__m128i mColorTimeAlphaMull = _mm_set1_epi16((short)alpha);
	mColorTimeAlpha = _mm_mullo_epi16(mColorTimeAlpha, mColorTimeAlphaMull);

	__m128i mMullInvAlpha = _mm_set1_epi16(invAlpha);

	mMullInvAlpha.m128i_i16[3] = 256;
	mMullInvAlpha.m128i_i16[7] = 256;

	//__m128i mZero = _mm_setzero_si128();
	int x = 0;
	while (count > 1)
	{
		__m128i source = _mm_cvtsi64_si128(*((long long*)(canvas + x)));

		//source = _mm_unpacklo_epi8(source, _mm_setzero_si128() );
		source = _mm_cvtepu8_epi16(source);

		__m128i tmp1 = _mm_mullo_epi16(source, mMullInvAlpha);    // source*invalpha
		tmp1 = _mm_adds_epu16(tmp1, mColorTimeAlpha);     // t

		tmp1 = _mm_srli_epi16(tmp1, 8);

		//source        = _mm_blend_epi16(tmp1,source,0x88);        // a,b,c,d  | e,f,g,h => a,b,c,h

		//source        = _mm_andnot_si128(mMaskAnd,source);        // mask alpha
		//tmp2          = _mm_and_si128(mMaskAnd,tmp2);             // mask colors
		//source        = _mm_or_si128(tmp2,source);                // 00XXXXXX | XX000000 = xxxxxxxx

		//source        = _mm_packus_epi16(source,source );// mZero );         // pack
		source = _mm_packus_epi16(tmp1, tmp1);// mZero );         // pack

		*((long long*)(canvas + x)) = _mm_cvtsi128_si64(source);

		x += 8;
		//canvas += 8;
		count -= 2;
	}

}








/*
alpha : 0 - 256
*/


void Apply4ColorPixelSSE(unsigned char * canvas, int count, int color, int alpha)
{
	int invAlpha = 256 - alpha;

	__m128i mColorRBTimeAlpha = _mm_set1_epi32(((unsigned int)(color & 0xff00ff))*alpha);
	__m128i mColorGTimeAlpha = _mm_set1_epi16(((color >> 8) & 0xff)*alpha);

	__m128i mMullInvAlpha = _mm_set1_epi16(invAlpha);
	__m128i mMullInvAlphaG = _mm_set1_epi32((256 << 16) + invAlpha);

	__m128i mMaskGAAnd = _mm_set1_epi32(0xff00FF00);

	while (count > 0)
	{
		
		__m128i sourceRB = _mm_loadu_si128((__m128i*)canvas); // load    ArgbArgbArgbArgb
		//_mm_prefetch((char *)canvas+16,_MM_HINT_T0);

		__m128i sourceG = sourceRB;
		sourceRB = _mm_andnot_si128(mMaskGAAnd, sourceRB);
		sourceG = _mm_srli_epi16(sourceG, 8);                 // shift for compute    xxxgxxxgxxxgxxxg
		// in source is now  xrxbxrxbxrxbxrxb
		sourceRB = _mm_mullo_epi16(sourceRB, mMullInvAlpha);    // sourceRB <= sourceRB*invAlpha
		sourceRB = _mm_adds_epu16(sourceRB, mColorRBTimeAlpha); // sourceRB <= sourceRB+(colorRB*Alpha)
		sourceRB = _mm_srli_epi16(sourceRB, 8);                         // now in sourceRB is sourceRB/255
		sourceG = _mm_mullo_epi16(sourceG, mMullInvAlphaG);    // sourceG <= sourceG*invAlpha
		sourceG = _mm_adds_epu16(sourceG, mColorGTimeAlpha); // sourceG <= sourceG+(colorG*Alpha)

		//sourceG = _mm_srli_epi16(sourceG,8);                           // now in sourceG is sourceG/255

		// now merge back xrxb.... xxxg.... into xrgb....

		sourceG = _mm_and_si128(mMaskGAAnd, sourceG);

		sourceRB = _mm_or_si128(sourceRB, sourceG);         // now in sourceRB is xrgbxrgb....

		_mm_storeu_si128((__m128i*)canvas, sourceRB);

		canvas += 16;
		count -= 4;
	}
}

void Apply4ColorPixelSSE_Aligned(unsigned char * canvas, int count, int color, int alpha)
{
	int invAlpha = 256 - alpha;

	__m128i mColorRBTimeAlpha = _mm_set1_epi32(((unsigned int)(color & 0xff00ff))*alpha);
	__m128i mColorGTimeAlpha = _mm_set1_epi16(((color >> 8) & 0xff)*alpha);

	__m128i mMullInvAlpha = _mm_set1_epi16(invAlpha);
	__m128i mMullInvAlphaG = _mm_set1_epi32((256 << 16) + invAlpha);

	__m128i mMaskGAAnd = _mm_set1_epi32(0xff00FF00);

	while (count > 0)
	{

		__m128i sourceRB = _mm_load_si128((__m128i*)canvas); // load    ArgbArgbArgbArgb
															  //_mm_prefetch((char *)canvas+16,_MM_HINT_T0);

		__m128i sourceG = sourceRB;
		sourceRB = _mm_andnot_si128(mMaskGAAnd, sourceRB);
		sourceG = _mm_srli_epi16(sourceG, 8);                 // shift for compute    xxxgxxxgxxxgxxxg
															  // in source is now  xrxbxrxbxrxbxrxb
		sourceRB = _mm_mullo_epi16(sourceRB, mMullInvAlpha);    // sourceRB <= sourceRB*invAlpha
		sourceRB = _mm_adds_epu16(sourceRB, mColorRBTimeAlpha); // sourceRB <= sourceRB+(colorRB*Alpha)
		sourceRB = _mm_srli_epi16(sourceRB, 8);                         // now in sourceRB is sourceRB/255
		sourceG = _mm_mullo_epi16(sourceG, mMullInvAlphaG);    // sourceG <= sourceG*invAlpha
		sourceG = _mm_adds_epu16(sourceG, mColorGTimeAlpha); // sourceG <= sourceG+(colorG*Alpha)

															 //sourceG = _mm_srli_epi16(sourceG,8);                           // now in sourceG is sourceG/255

															 // now merge back xrxb.... xxxg.... into xrgb....

		sourceG = _mm_and_si128(mMaskGAAnd, sourceG);

		sourceRB = _mm_or_si128(sourceRB, sourceG);         // now in sourceRB is xrgbxrgb....

		_mm_store_si128((__m128i*)canvas, sourceRB);

		canvas += 16;
		count -= 4;
	}
}

/*
alpha : 0 - 256
*/
void Apply8ColorPixelSSE(unsigned char * canvas, int count, int color, int alpha)
{
	int invAlpha = 256 - alpha;

	// unsigned int tmpColor = (r*alpha)<<16 | b*alpha;

	__m128i mColorRBTimeAlpha = _mm_set1_epi32(((unsigned int)(color & 0xff00ff))*alpha);
	//mColorRBTimeAlpha = _mm_mullo_epi16(mColorRBTimeAlpha,mColorTimeAlphaMull);

	__m128i mColorGTimeAlpha = _mm_set1_epi16(((color >> 8) & 0xff)*alpha);



	__m128i mMullInvAlpha = _mm_set1_epi16(invAlpha);
	//__m128i mMullInvAlphaG = _mm_set1_epi16( invAlpha);

	//__m128i mMaskGAnd = _mm_set1_epi32(0x0000ff00);
	//__m128i mMaskAAnd = _mm_set1_epi32(0xff000000);
	__m128i mMaskGAAnd = _mm_set1_epi32(0xff00FF00);
	__m128i mMaskGAnd = _mm_set1_epi32(0x0000FF00);

	__m128i testSuffle = _mm_setr_epi8(1, 0x80, 0x80, 0x80, 5, 0x80, 0x80, 0x80, 9, 0x80, 0x80, 0x80, 13, 0x80, 0x80, 0x80);
	__m128i testSuffle2 = _mm_setr_epi8(0x80, 0x80, 1, 0x80, 0x80, 0x80, 5, 0x80, 0x80, 0x80, 9, 0x80, 0x80, 0x80, 13, 0x80);


	while (count > 7)
	{
		__m128i sourceRB = _mm_loadu_si128((__m128i*)canvas); // load    ArgbArgbArgbArgb
		__m128i sourceRB2 = _mm_loadu_si128(((__m128i*)canvas)+1); // load    ArgbArgbArgbArgb

		//_mm_shuffle_epi8(sourceRB, testSuffle);

		//__m128i tmpTG = _mm_srli_epi32(_mm_and_si128(mMaskGAnd,sourceRB),8);
		__m128i tmpTG = _mm_shuffle_epi8(sourceRB, testSuffle);
		//__m128i tmpTG2 =  _mm_slli_epi32(_mm_and_si128(mMaskGAnd, sourceRB2), 8);
		__m128i tmpTG2 = _mm_shuffle_epi8(sourceRB2, testSuffle2);

		__m128i sourceGG = _mm_or_si128(tmpTG, tmpTG2);

		sourceRB = _mm_andnot_si128(mMaskGAAnd, sourceRB);
		sourceRB2 = _mm_andnot_si128(mMaskGAAnd, sourceRB2);

		// now rb and rb2 has   xRxB, xRxB, ...
		// source GG      has   xG2xG, xG2xG, xG2xG, ...

		sourceRB2 = _mm_mullo_epi16(sourceRB2, mMullInvAlpha);    // sourceRB <= sourceRB*invAlpha
		sourceRB2 = _mm_adds_epu16(sourceRB2, mColorRBTimeAlpha); // sourceRB <= sourceRB+(colorRB*Alpha)
		sourceRB2 = _mm_srli_epi16(sourceRB2, 8);                         // now in sourceRB is sourceRB/255

		sourceRB = _mm_mullo_epi16(sourceRB, mMullInvAlpha);    // sourceRB <= sourceRB*invAlpha
		sourceRB = _mm_adds_epu16(sourceRB, mColorRBTimeAlpha); // sourceRB <= sourceRB+(colorRB*Alpha)
		sourceRB = _mm_srli_epi16(sourceRB, 8);                         // now in sourceRB is sourceRB/255
		sourceGG = _mm_mullo_epi16(sourceGG, mMullInvAlpha);    // sourceG <= sourceG*invAlpha
		sourceGG = _mm_adds_epu16(sourceGG, mColorGTimeAlpha); // sourceG <= sourceG+(colorG*Alpha)

															 //sourceG = _mm_srli_epi16(sourceG,8);                           // now in sourceG is sourceG/255

															 // now merge back xrxb.... xxxg.... into xrgb....


		//sourceGG = _mm_srli_epi16(sourceGG, 8);

		tmpTG = _mm_and_si128(mMaskGAnd, sourceGG);
		tmpTG2 = _mm_and_si128(mMaskGAnd, _mm_srli_epi32( sourceGG,16));
		sourceRB = _mm_or_si128(sourceRB, tmpTG);         // now in sourceRB is xrgbxrgb....
		sourceRB2 = _mm_or_si128(sourceRB2, tmpTG2);         // now in sourceRB is xrgbxrgb....

		_mm_storeu_si128((__m128i*)canvas, sourceRB);
		_mm_storeu_si128(((__m128i*)canvas)+1, sourceRB2);

		canvas += 32;
		count -= 8;
	}

}

void AlphaBlending::NewFastRowApplyColor(unsigned char * canvas, int countPixel, int color)
{
	int alpha256 = ((color) >> 24) & 0xff;
	if (alpha256 == 0xff)  alpha256 += 1;

	ApplyColorPixelFaster(canvas, countPixel, color, alpha256);

}

void AlphaBlending::NewFastRowApplyColorSSE(unsigned char * canvas, int countPixel, int color)
{
	int alpha256 = ((color) >> 24) & 0xff;
	if (alpha256 == 0xff)  alpha256 += 1;


	unsigned char * line = canvas;

	//if(len > 0)
	{
		ApplyColorPixelSSE(line, countPixel, color, alpha256);

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

void AlphaBlending::NewFastRowApplyColorSSE64(unsigned char * canvas, int countPixel, int color)
{
	int alpha256 = ((color) >> 24) & 0xff;
	if (alpha256 == 0xff)  alpha256 += 1;// : alpha256;
	NewFastRowApplyColorSSE64(canvas, countPixel, color, alpha256);

	//NewFastRowApplyColorSSE64(canvas, countPixel, color, ((((color) >> 24) & 0xff) * 256) / 255);

	//NewFastRowApplyColorSSE64(canvas, countPixel, color, color);
}

void AlphaBlending::NewFastRowApplyColorSSE64Empty(unsigned char * canvas, int countPixel, int color)
{
	return;
	//NewFastRowApplyColorSSE64(canvas, countPixel, color, ((((color) >> 24) & 0xff) * 256) / 255);

}

void AlphaBlending::NewFastRowApplyColorSSE64(unsigned char * canvas, int countPixel, int color, int alpha256)
{
	// convert alpha value from range 0-255 to 0-256

	int alpha = alpha256;// ((((color) >> 24) & 0xff)*256)/255;


	unsigned char * line = canvas;

	if (countPixel < 8)
	{
		//ApplyColorPixel(line, countPixel, color, alpha);
		ApplyColorPixelFaster(line, countPixel, color, alpha);
		//ApplyColorPixelSSE(line,countPixel,color,alpha);
	}
	//return;
	else
	{
		// fix bad align
		// move if address is 0xc || 0x4
		unsigned int tmp = ((((unsigned int)line) & 0xf) / 4);

		if ((tmp != 0) //&& countPixel > 0
			)
		{
			tmp = 4 - tmp;
			ApplyColorPixelFaster(line, tmp, color, alpha);
			//ApplyColorPixelSSE(line,tmp,color,alpha);

			countPixel -= tmp;
			line += 4 * tmp;

			//countPixel -= 1;
			//line += 4;
		}

		//if(countPixel > 1)
		{
			int tmpLen = countPixel - (countPixel & 1);
			Apply2ColorPixelSSE_aligned(line, tmpLen, color, alpha);
			//ApplyColorPixel(line,tmpLen, color, alpha);

			countPixel -= tmpLen;
			line += tmpLen * 4;
		}


		if (countPixel > 0)
		{

			ApplyColorPixelFaster(line, 1, color, alpha);
			//ApplyColorPixelSSE(line,color,alpha);
		}
	}

}

void AlphaBlending::NewFastRowApplyColorSSE128(unsigned char * canvas, int countPixel, int color)
{
	int alpha256 = ((color) >> 24) & 0xff;
	if (alpha256 == 0xff)  alpha256 += 1;

	NewFastRowApplyColorSSE128(canvas, countPixel, color, alpha256);
}
void AlphaBlending::NewFastRowApplyColorSSE128(unsigned char * canvas, int countPixel, int color, int alpha256)
{
	// convert alpha value from range 0-255 to 0-256

	unsigned char * line = canvas;

	if (countPixel < 16)
	{
		//ApplyColorPixelSSE(line,countPixel,color,alpha);
		ApplyColorPixelFaster(line, countPixel, color, alpha256);
		//NewFastRowApplyColorSSE64(line, countPixel, color, alpha256);
	}
	else
	{
		/*unsigned int tmp = (((unsigned int)line) & 0xf) / 4;
		tmp = 4 - tmp;
		//if (tmp > 0)
		{
			ApplyColorPixelFaster(line, tmp, color, alpha256);
			//ApplyColorPixelSSE(line,tmp,color,alpha256);
			countPixel -= tmp;
			line += tmp * 4;
		}*/

		//if(countPixel > 3)
		{
			int tmpLen = countPixel  & (~3);
			//Apply4ColorPixelSSE(line,r,g,b,alpha);
			/*if(((((unsigned int)line) & 0xf) / 4) == 0)
			Apply4ColorPixelSSE_Aligned(line, tmpLen, color, alpha256);
			else*/
				Apply4ColorPixelSSE(line, tmpLen, color, alpha256);
			countPixel -= tmpLen;

			line += tmpLen * 4;
		}

		/*if(countPixel > 1)
		{
		int tmpLen = countPixel - (countPixel&1);
		Apply2ColorPixelSSE(line,tmpLen, color,alpha);

		countPixel -= tmpLen;
		line+=tmpLen*4;
		}*/


		if (countPixel > 0)
		{
			ApplyColorPixelFaster(line, countPixel, color, alpha256);
			//ApplyColorPixelSSE(line,countPixel,color,alpha256);
		}
	}
}

void AlphaBlending::NewFastRowApplyColorSSE256(unsigned char * canvas, int countPixel, int color)
{
	int alpha256 = ((color) >> 24) & 0xff;
	if (alpha256 == 0xff)  alpha256 += 1;

	NewFastRowApplyColorSSE256(canvas, countPixel, color, alpha256);
}
void AlphaBlending::NewFastRowApplyColorSSE256(unsigned char * canvas, int countPixel, int color, int alpha256)
{
	// convert alpha value from range 0-255 to 0-256


	unsigned char * line = canvas;

	if (countPixel < 8)
	{
		//ApplyColorPixelSSE(line,countPixel,color,alpha);
		ApplyColorPixelFaster(line, countPixel, color, alpha256);
		//NewFastRowApplyColorSSE64(line, countPixel, color, alpha256);
	}
	else
	{
		/*unsigned int tmp = (((unsigned int)line) & 0xf) / 4;
		tmp = 4 - tmp;
		//if (tmp > 0)
		{


			ApplyColorPixelFaster(line, tmp, color, alpha256);
			//ApplyColorPixelSSE(line,tmp,color,alpha256);
			countPixel -= tmp;
			line += tmp * 4;
		}*/


		//if(countPixel > 3)
		//{
			int tmpLen = countPixel & (~7);

			Apply8ColorPixelSSE(line, tmpLen, color, alpha256);
			countPixel -= tmpLen;

			line += tmpLen * 4;
		//}

		if (countPixel > 0)
		{
			ApplyColorPixelFaster(line, countPixel, color, alpha256);
			//ApplyColorPixelSSE(line,countPixel,color,alpha256);
		}
	}
}






















