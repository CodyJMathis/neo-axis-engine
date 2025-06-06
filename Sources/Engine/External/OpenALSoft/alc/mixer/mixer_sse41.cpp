/**
 * OpenAL cross platform audio library
 * Copyright (C) 2014 by Timothy Arceri <t_arceri@yahoo.com.au>.
 * This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU Library General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 * You should have received a copy of the GNU Library General Public
 *  License along with this library; if not, write to the
 *  Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 * Or go to http://www.gnu.org/copyleft/lgpl.html
 */

#include "config.h"
#ifdef HAVE_SSE41

#include <xmmintrin.h>
#include <emmintrin.h>
#include <smmintrin.h>

#include "alu.h"
#include "defs.h"


template<>
const ALfloat *Resample_<LerpTag,SSE4Tag>(const InterpState*, const ALfloat *RESTRICT src,
    ALsizei frac, ALint increment, ALfloat *RESTRICT dst, ALsizei dstlen)
{
    const __m128i increment4{_mm_set1_epi32(increment*4)};
    const __m128 fracOne4{_mm_set1_ps(1.0f/FRACTIONONE)};
    const __m128i fracMask4{_mm_set1_epi32(FRACTIONMASK)};

    ASSUME(frac >= 0);
    ASSUME(increment > 0);
    ASSUME(dstlen >= 0);

    alignas(16) ALsizei pos_[4], frac_[4];
    InitiatePositionArrays(frac, increment, frac_, pos_, 4);
    __m128i frac4{_mm_setr_epi32(frac_[0], frac_[1], frac_[2], frac_[3])};
    __m128i pos4{_mm_setr_epi32(pos_[0], pos_[1], pos_[2], pos_[3])};

    const ALsizei todo{dstlen & ~3};
    for(ALsizei i{0};i < todo;i += 4)
    {
        const int pos0{_mm_extract_epi32(pos4, 0)};
        const int pos1{_mm_extract_epi32(pos4, 1)};
        const int pos2{_mm_extract_epi32(pos4, 2)};
        const int pos3{_mm_extract_epi32(pos4, 3)};
        const __m128 val1{_mm_setr_ps(src[pos0  ], src[pos1  ], src[pos2  ], src[pos3  ])};
        const __m128 val2{_mm_setr_ps(src[pos0+1], src[pos1+1], src[pos2+1], src[pos3+1])};

        /* val1 + (val2-val1)*mu */
        const __m128 r0{_mm_sub_ps(val2, val1)};
        const __m128 mu{_mm_mul_ps(_mm_cvtepi32_ps(frac4), fracOne4)};
        const __m128 out{_mm_add_ps(val1, _mm_mul_ps(mu, r0))};

        _mm_store_ps(&dst[i], out);

        frac4 = _mm_add_epi32(frac4, increment4);
        pos4 = _mm_add_epi32(pos4, _mm_srli_epi32(frac4, FRACTIONBITS));
        frac4 = _mm_and_si128(frac4, fracMask4);
    }

    /* NOTE: These four elements represent the position *after* the last four
     * samples, so the lowest element is the next position to resample.
     */
    ALsizei pos{_mm_cvtsi128_si32(pos4)};
    frac = _mm_cvtsi128_si32(frac4);

    for(ALsizei i{todo};i < dstlen;++i)
    {
        dst[i] = lerp(src[pos], src[pos+1], frac * (1.0f/FRACTIONONE));

        frac += increment;
        pos  += frac>>FRACTIONBITS;
        frac &= FRACTIONMASK;
    }
    return dst;
}

#endif