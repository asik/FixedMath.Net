using System;
using System.Globalization;

namespace FixMath.NET {

    /// <summary>
    /// This is more or less a straight port of libfixmath (https://code.google.com/p/libfixmath/)
    /// It sort of works but I didn't spend much time on it.
    /// </summary>
    public partial struct Fix16 : IEquatable<Fix16>, IComparable<Fix16> {

        readonly int m_rawValue;
        static readonly Fix16[][] Fix16AtanCacheIndex;
        static readonly Fix16[] Fix16AtanCacheValue = new Fix16[4096];

        static Fix16() {
            Fix16AtanCacheIndex = new Fix16[2][];
            Fix16AtanCacheIndex[0] = new Fix16[4096];
            Fix16AtanCacheIndex[1] = new Fix16[4096];
        }

        public static readonly Fix16 FourDivPi = new Fix16(0x145F3);
        public static readonly Fix16 FourDivPi2 = new Fix16(0xFFFF9840);
        public static readonly Fix16 X4CorrectionComponent = new Fix16(0x399A);
        public static readonly Fix16 PiDiv4 = new Fix16(0x0000C90F);
        public static readonly Fix16 ThreePiDiv4 = new Fix16(0x00025B2F);

        public static readonly Fix16 MaxValue = new Fix16(int.MaxValue);
        public static readonly Fix16 MinValue = new Fix16(int.MinValue);
        public static readonly Fix16 Overflow = new Fix16(int.MinValue);

        public static readonly Fix16 Pi = new Fix16(205887);
        public static readonly Fix16 E = new Fix16(178145);
        public static readonly Fix16 One = new Fix16(0x00010000);
        public static readonly Fix16 Zero = new Fix16(0);

        public static explicit operator Fix16(int a) {
            return new Fix16(a * One.m_rawValue);
        }

        public static explicit operator float(Fix16 a) {
            return (float)a.m_rawValue / One.m_rawValue;
        }

        public static explicit operator double(Fix16 a) {
            return (double)a.m_rawValue / One.m_rawValue;
        }

        public static explicit operator decimal(Fix16 a) {
            return (decimal)a.m_rawValue / One.m_rawValue;
        }

        public static explicit operator int(Fix16 a) {
#if !FIXMATH_NO_ROUNDING
            return a.m_rawValue >> 16;
#else
            if (a.m_rawValue >= 0) {
                return (a.m_rawValue + (One.m_rawValue >> 1)) / One.m_rawValue;
            }
            return (a.m_rawValue - (One.m_rawValue >> 1)) / One.m_rawValue;
#endif
        }

        public static explicit operator Fix16(float a) {
            var temp = a * One.m_rawValue;
#if !FIXMATH_NO_ROUNDING
            temp += (temp >= 0) ? 0.5f : -0.5f;
#endif
            return new Fix16((int)temp);
        }

        public static explicit operator Fix16(double a) {
            var temp = a * One.m_rawValue;
#if !FIXMATH_NO_ROUNDING
            temp += (temp >= 0) ? 0.5f : -0.5f;
#endif
            return new Fix16((int)temp);
        }

        public static Fix16 Abs(Fix16 x) {
            // branchless implementation, see http://www.strchr.com/optimized_abs_function
            int mask = x.m_rawValue >> 31;
            return new Fix16((x.m_rawValue + mask) ^ mask);
        }

        public static Fix16 Floor(Fix16 x) {
            return new Fix16((int)((ulong)x.m_rawValue & 0xFFFF0000UL));
        }

        public static Fix16 Ceil(Fix16 x) {
            return new Fix16((int)
                (((ulong)x.m_rawValue & 0xFFFF0000UL) + (((ulong)x.m_rawValue & 0x0000FFFFUL) != 0UL ? (ulong)One.m_rawValue : 0UL)));
        }

        public static Fix16 Min(Fix16 x, Fix16 y) {
            return x.m_rawValue < y.m_rawValue ? x : y;
        }

        public static Fix16 Max(Fix16 x, Fix16 y) {
            return x.m_rawValue > y.m_rawValue ? x : y;
        }

        public static Fix16 Clamp(Fix16 x, Fix16 min, Fix16 max) {
            return Min(Max(x, min), max);
        }

        public static Fix16 operator +(Fix16 x, Fix16 y) {
#if FIXMATH_NO_OVERFLOW
            return new Fix16(x.m_rawValue + y.m_rawValue);
#else
            var sum = x.m_rawValue + y.m_rawValue;
            // Overflow can only happen if sign of a == sign of b, and then
            // it causes sign of sum != sign of a.
            if ((((x.m_rawValue ^ y.m_rawValue) & int.MinValue) == 0) && (((x.m_rawValue ^ sum) & 0x80000000) != 0))
                return Overflow;

            return new Fix16(sum);
#endif
        }

        public static Fix16 operator -(Fix16 x, Fix16 y) {
#if FIXMATH_NO_OVERFLOW
            return new Fix16(x.m_rawValue - y.m_rawValue);
#else
            var diff = x.m_rawValue - y.m_rawValue;
            // Overflow can only happen if sign of a != sign of b, and then
            // it causes sign of sum != sign of a.
            if ((((x.m_rawValue ^ y.m_rawValue) & int.MinValue) != 0) && (((x.m_rawValue ^ diff) & 0x80000000) != 0))
                return Overflow;

            return new Fix16(diff);
#endif
        }

        public static Fix16 SAdd(Fix16 a, Fix16 b) {
            var result = a + b;

            if (result == Overflow) {
                return (a > Zero) ? MaxValue : MinValue;
            }
            return result;
        }

        public static Fix16 SSub(Fix16 a, Fix16 b) {
            var result = a - b;

            if (result == Overflow) {
                return (a > Zero) ? MaxValue : MinValue;
            }

            return result;
        }

        // Since this is .NET, we can assume 64-bit arithmetic is supported
        public static Fix16 operator *(Fix16 x, Fix16 y) {

            var product = (long)x.m_rawValue * y.m_rawValue;

#if !FIXMATH_NO_OVERFLOW
            // The upper 17 bits should all be the same (the sign).
            var upper = (uint)(product >> 47);
#endif

            if (product < 0) {
#if !FIXMATH_NO_OVERFLOW
                if (~upper != 0)
                    return Overflow;
#endif

#if !FIXMATH_NO_ROUNDING
                // This adjustment is required in order to round -1/2 correctly
                product--;
#endif
            }
            else {
#if !FIXMATH_NO_OVERFLOW
                if (upper != 0)
                    return Overflow;
#endif
            }

#if FIXMATH_NO_ROUNDING
            return new Fix16((int)(product >> 16));
#else
            var result = product >> 16;
            result += (product & 0x8000) >> 15;

            return new Fix16((int)result);
#endif
        }

        public static Fix16 SMul(Fix16 a, Fix16 b) {
            var result = a * b;

            if (result == Overflow) {
                return (a >= Zero) == (b >= Zero) ? 
                    MaxValue : 
                    MinValue;
            }

            return result;
        }

        static byte Clz(uint x) {
            byte result = 0;
            if (x == 0) { return 32; }
            while ((x & 0xF0000000) == 0) { result += 4; x <<= 4; }
            while ((x & 0x80000000) == 0) { result += 1; x <<= 1; }
            return result;
        }

        public static Fix16 operator /(Fix16 x, Fix16 y) {
            // This uses a hardware 32/32 bit division multiple times, until we have
            // computed all the bits in (a<<17)/b. Usually this takes 1-3 iterations.
            var a = x.m_rawValue;
            var b = y.m_rawValue;

            if (b == 0) {
                return MinValue;
            }

            var remainder = (uint)((a >= 0) ? a : (-a));
            var divider = (uint)((b >= 0) ? b : (-b));
            var quotient = 0U;
            var bitPos = 17;

            // Kick-start the division a bit.
            // This improves speed in the worst-case scenarios where N and D are large
            // It gets a lower estimate for the result by N/(D >> 17 + 1).
            if ((divider & 0xFFF00000) != 0) {
                var shiftedDiv = ((divider >> 17) + 1);
                quotient = remainder / shiftedDiv;
                remainder -= (uint)(((ulong)quotient * divider) >> 17);
            }

            // If the divider is divisible by 2^n, take advantage of it.
            while ((divider & 0xF) == 0 && bitPos >= 4) {
                divider >>= 4;
                bitPos -= 4;
            }

            while (remainder != 0 && bitPos >= 0) {
                // Shift remainder as much as we can without overflowing
                int shift = Clz(remainder);
                if (shift > bitPos) shift = bitPos;
                remainder <<= shift;
                bitPos -= shift;

                var div = remainder / divider;
                remainder = remainder % divider;
                quotient += div << bitPos;

#if !FIXMATH_NO_OVERFLOW
                if ((div & ~(0xFFFFFFFF >> bitPos)) != 0) {
                    return Overflow;
                }
#endif

                remainder <<= 1;
                bitPos--;
            }

#if !FIXMATH_NO_ROUNDING
            // Quotient is always positive so rounding is easy
            quotient++;
#endif

            var result = (int)(quotient >> 1);

            // Figure out the sign of the result
            if (((a ^ b) & 0x80000000) != 0) {
#if !FIXMATH_NO_OVERFLOW
                if (result == MinValue.m_rawValue) {
                    return Overflow;
                }
#endif
                result = -result;
            }

            return new Fix16(result);
        }

        public static Fix16 SDiv(Fix16 inArg0, Fix16 inArg1) {
            var result = inArg0 / inArg1;

            if (result == Overflow) {
                return (inArg0 >= Zero) == (inArg1 >= Zero) ? MaxValue : MinValue;
            }

            return result;
        }


        public static Fix16 Sqrt(Fix16 x) {
            var inValue = x.m_rawValue;
            var neg = (inValue < 0);
            var num = (uint)(neg ? -inValue : inValue);
            var result = 0U;

            // Many numbers will be less than 15, so
            // this gives a good balance between time spent
            // in if vs. time spent in the while loop
            // when searching for the starting value.
            uint bit = (num & 0xFFF00000) != 0 ?
                (uint)1 << 30 :
                (uint)1 << 18;

            while (bit > num) {
                bit >>= 2;
            }

            // The main part is executed twice, in order to avoid
            // using 64 bit values in computations.
            for (var n = 0; n < 2; n++) {
                // First we get the top 24 bits of the answer.
                while (bit != 0) {
                    if (num >= result + bit) {
                        num -= result + bit;
                        result = (result >> 1) + bit;
                    }
                    else {
                        result = (result >> 1);
                    }
                    bit >>= 2;
                }

                if (n == 0) {
                    // Then process it again to get the lowest 8 bits.
                    if (num > 65535) {
                        // The remainder 'num' is too large to be shifted left
                        // by 16, so we have to add 1 to result manually and
                        // adjust 'num' accordingly.
                        // num = a - (result + 0.5)^2
                        //       = num + result^2 - (result + 0.5)^2
                        //       = num - result - 0.5
                        num -= result;
                        num = (num << 16) - 0x8000;
                        result = (result << 16) + 0x8000;
                    }
                    else {
                        num <<= 16;
                        result <<= 16;
                    }

                    // (sizeof(basetype) / 2) - 2? Hence 30 for long, 6 for short and 2 for byte.
                    bit = 1 << 14;
                }
            }

#if !FIXMATH_NO_ROUNDING
            // Finally, if next bit would have been 1, round the result upwards.
            if (num > result) {
                result++;
            }
#endif

            return new Fix16((int)(neg ? -result : result));
        }

        /// <summary>
        /// Fast version of sin that only works for inAngle between -Pi and Pi.
        /// </summary>
        /// <param name="inAngle">Must be comprised between -Pi and Pi</param>
        /// <returns></returns>
        public static Fix16 SinParabola(Fix16 inAngle) {
            // On 0->PI, sin looks like x² that is :
            // - centered on PI/2,
            // - equals 1 on PI/2,
            // - equals 0 on 0 and PI
            // that means :  4/PI * x  - 4/PI² * x²
            // Use abs(x) to handle (-PI) -> 0 zone.
            var retval = (FourDivPi * inAngle) + (FourDivPi2 * inAngle * Abs(inAngle));
            // At this point, retval equals sin(inAngle) on important points ( -PI, -PI/2, 0, PI/2, PI),
            // but is not very precise between these points
            // So improve its precision by adding some x^4 component to retval
            retval += X4CorrectionComponent * (new Fix16((retval * Abs(retval)).m_rawValue - retval.m_rawValue));
            return retval;
        }

        /// <summary>
        /// Implemented as if FIXMATH_SIN_LUT was always defined.
        /// </summary>
        public static Fix16 Sin(Fix16 inAngle) {
            var tempAngle = inAngle % (Pi << 1);

            if (tempAngle < Zero)
                tempAngle += Pi << 1;

            if (tempAngle >= Pi) {
                tempAngle -= Pi;
                if (tempAngle >= (Pi >> 1))
                    tempAngle = Pi - tempAngle;
                return -(tempAngle.m_rawValue >= SinLut.Length ?
                    One :
                    new Fix16(SinLut[tempAngle.m_rawValue]));
            }
            if (tempAngle >= (Pi >> 1))
                tempAngle = Pi - tempAngle;
            return tempAngle.m_rawValue >= SinLut.Length ?
                       One :
                       new Fix16(SinLut[tempAngle.m_rawValue]);
        }

        public static Fix16 Cos(Fix16 inAngle) {
            return Sin(new Fix16(inAngle.m_rawValue + (Pi.m_rawValue >> 1)));
        }

        public static Fix16 Tan(Fix16 inAngle) {
            return SDiv(Sin(inAngle), Cos(inAngle));
        }

        public static Fix16 Asin(Fix16 x) {
            if (x > One || x < -One) {
                return Zero;
            }

            var rv = One - (x * x);
            rv = x / Sqrt(rv);
            rv = Atan(rv);
            return rv;
        }


        public static Fix16 Atan2(Fix16 inY, Fix16 inX) {
            // This code is based on http://en.wikipedia.org/wiki/User:Msiddalingaiah/Ideas#Fast_arc_tangent
            var hash = (uint)(inX.m_rawValue ^ inY.m_rawValue);
            hash ^= hash >> 20;
            hash &= 0x0FFF;
            if ((Fix16AtanCacheIndex[0][hash] == inX) && (Fix16AtanCacheIndex[1][hash] == inY)) {
                return Fix16AtanCacheValue[hash];
            }

            var absInY = Abs(inY);
            Fix16 angle;
            if (inX >= Zero) {
                var r = (inX - absInY) / (inX + absInY);
                var r3 = r * r * r;
                angle = (new Fix16(0x00003240) * r3) - (new Fix16(0x0000FB50) * r) + PiDiv4;
            }
            else {
                var r = (inX + absInY) / (absInY - inX);
                var r3 = r * r * r;
                angle = (new Fix16(0x00003240) * r3)
                        - (new Fix16(0x0000FB50) * r)
                        + ThreePiDiv4;
            }
            if (inY < Zero) {
                angle = -angle;
            }

            Fix16AtanCacheIndex[0][hash] = inX;
            Fix16AtanCacheIndex[1][hash] = inY;
            Fix16AtanCacheValue[hash] = angle;

            return angle;
        }

        public static Fix16 Atan(Fix16 x) {
            return Atan2(x, One);
        }

        public static Fix16 Acos(Fix16 x) {
            return new Fix16((Pi.m_rawValue >> 1) - Asin(x).m_rawValue);
        }

        public static Fix16 operator %(Fix16 x, Fix16 y) {
            return new Fix16(x.m_rawValue % y.m_rawValue);
        }

        public static Fix16 operator >>(Fix16 x, int shift) {
            return new Fix16(x.m_rawValue >> shift);
        }

        public static Fix16 operator <<(Fix16 x, int shift) {
            return new Fix16(x.m_rawValue << shift);
        }

        public static Fix16 operator -(Fix16 x) {
            return new Fix16(-x.m_rawValue);
        }

        public static bool operator >(Fix16 x, Fix16 y) {
            return x.m_rawValue > y.m_rawValue;
        }

        public static bool operator <(Fix16 x, Fix16 y) {
            return x.m_rawValue < y.m_rawValue;
        }

        public static bool operator >=(Fix16 x, Fix16 y) {
            return x.m_rawValue >= y.m_rawValue;
        }

        public static bool operator <=(Fix16 x, Fix16 y) {
            return x.m_rawValue <= y.m_rawValue;
        }

        public static bool operator ==(Fix16 x, Fix16 y) {
            return x.m_rawValue == y.m_rawValue;
        }

        public static bool operator !=(Fix16 x, Fix16 y) {
            return x.m_rawValue != y.m_rawValue;
        }

        public static Fix16 operator ++(Fix16 x) {
            return x + One;
        }

        public static Fix16 operator --(Fix16 x) {
            return x - One;
        }

        public static Fix16 FromRaw(int i) {
            return new Fix16(i);
        }

        Fix16(int rawValue) {
            m_rawValue = rawValue;
        }

        Fix16(uint rawValue) {
            m_rawValue = (int)rawValue;
        }

        public override string ToString() {
            // Using Decimal.ToString() instead of float or double because decimal is 
            // also implemented in software. This guarantees a consistent string representation.
            return ((decimal)this).ToString(CultureInfo.InvariantCulture);
        }

        public bool Equals(Fix16 other) {
            return m_rawValue == other.m_rawValue;
        }

        public int CompareTo(Fix16 other) {
            return m_rawValue.CompareTo(other.m_rawValue);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }
            return obj is Fix16 && Equals((Fix16)obj);
        }

        public override int GetHashCode() {
            return m_rawValue;
        }
    }
}
