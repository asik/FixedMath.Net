using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FixMath.NET {

    public struct Fix64 {
        readonly long m_rawValue;

        public static readonly Fix64 MaxValue = new Fix64(long.MaxValue);
        public static readonly Fix64 MinValue = new Fix64(long.MinValue);
        public static readonly Fix64 One = new Fix64(1L << 32);
        public static readonly Fix64 Zero = new Fix64();

        public static int Sign(Fix64 value) {
            return value.m_rawValue < 0 ? -1 : 1;
        }

        /// <summary>
        /// Adds x and y. Performs saturating addition, i.e. in case of overflow, 
        /// rounds to MinValue or MaxValue depending on sign of operands.
        /// </summary>
        public static Fix64 operator +(Fix64 x, Fix64 y) {
            // Overflow can only happen if sign of a == sign of b, and then
            // it causes sign of sum != sign of a.
            var xl = x.m_rawValue;
            var yl = y.m_rawValue;
            var sum = xl + yl;
            if (((xl ^ yl) & long.MinValue) == 0 && ((sum ^ xl) & long.MinValue) != 0) {
                return xl > 0 ? MaxValue : MinValue;
            }
            return new Fix64(sum);
        }

        /// <summary>
        /// Subtracts y from x. Performs saturating substraction, i.e. in case of overflow, 
        /// rounds to MinValue or MaxValue depending on sign of operands.
        /// </summary>
        public static Fix64 operator -(Fix64 x, Fix64 y) {
            // Overflow can only happen if sign of a != sign of b, and then
            // it causes sign of sum != sign of a.
            var xl = x.m_rawValue;
            var yl = y.m_rawValue;
            var diff = xl - yl;
            if (((xl ^ yl) & long.MinValue) != 0 && ((diff ^ xl) & long.MinValue) != 0) {
                return xl > 0 ? MaxValue : MinValue;
            }
            return new Fix64(diff);
        }

        static long AddOverflowHelper(long x, long y, ref bool overflow) {
            var sum = x + y;
            // x + y overflows if sign(x) ^ sign(y) != sign(sum)
            overflow |= ((x ^ y ^ sum) & long.MinValue) != 0;
            return sum;
        }

        public static Fix64 operator *(Fix64 x, Fix64 y) {

            var xl = x.m_rawValue;
            var yl = y.m_rawValue;

            var xlo = (ulong)(xl & 0x00000000FFFFFFFF);
            var xhi = xl >> 32;
            var ylo = (ulong)(yl & 0x00000000FFFFFFFF);
            var yhi = yl >> 32;

            var lolo = xlo * ylo;
            var lohi = (long)xlo * yhi;
            var hilo = xhi * (long)ylo;
            var hihi = xhi * yhi;

            var loResult = lolo >> 32;
            var midResult1 = lohi;
            var midResult2 = hilo;
            var hiResult = hihi << 32;

            bool overflow = false;
            var sum = AddOverflowHelper((long)loResult, midResult1, ref overflow);
            sum = AddOverflowHelper(sum, midResult2, ref overflow);
            sum = AddOverflowHelper(sum, hiResult, ref overflow);
            //sbyte sum = (sbyte)((sbyte)loResult + midResult1 + midResult2 + hiResult);

            bool opSignsEqual = ((xl ^ yl) & long.MinValue) == 0;

            // if signs of operands are equal and sign of result is negative,
            // then multiplication overflowed positively
            // the reverse is also true
            if (opSignsEqual) {
                if (sum < 0 || (overflow && xl > 0)) {
                    return MaxValue;
                }
            }
            else {
                if (sum > 0) {
                    return MinValue;
                }
            }

            // if the top 32 bits of hihi (unused in the result) are neither all 0s or 1s,
            // then this means the result overflowed.
            var topCarry = hihi >> 32;
            if (topCarry != 0 && topCarry != -1 /*&& xl != -17 && yl != -17*/) {
                return opSignsEqual ? MaxValue : MinValue; 
            }

            // If signs differ, both operands' magnitudes are greater than 1,
            // and the result is greater than the negative operand, then there was negative overflow.
            if (!opSignsEqual) {
                long posOp, negOp;
                if (xl > yl) {
                    posOp = xl;
                    negOp = yl;
                }
                else {
                    posOp = yl;
                    negOp = xl;
                }
                if (sum > negOp && negOp < -(1 << 32) && posOp > (1 << 32)) {
                    return MinValue;
                }
            }

            return new Fix64(sum);


            //var xl = x.m_rawValue;
            //var yl = y.m_rawValue;

            ////if (xl == 0L || yl == 0L) {
            ////    return new Fix64();
            ////}

            //var xlow = (ulong)xl & 0x00000000FFFFFFFF;
            //var xhigh = xl >> 32;
            //var ylow = (ulong)yl & 0x00000000FFFFFFFF;
            //var yhigh = yl >> 32;

            //var lowlow = xlow * ylow;
            //var lowhigh = (long)xlow * yhigh;
            //var highlow = xhigh * (long)ylow;
            //var highhigh = xhigh * yhigh;

            //var loResult = lowlow >> 32;
            //var midResult1 = lowhigh;
            //var midResult2 = highlow;
            //var hiResult = highhigh << 32;

            //var finalResult = (long)loResult + midResult1 + midResult2 + hiResult;

            //var carryBitsHigh = (int)(highhigh >> 32);
            ////if (highhigh < 0) {
            ////    if (~carryBitsHigh != 0) {
            ////        return MinValue;
            ////    }
            ////}
            ////else {
            ////    if (carryBitsHigh != 0) {
            ////        return MaxValue;
            ////    }
            ////}
            //if (carryBitsHigh != 0 && carryBitsHigh != -1) {
            //    return ((xl ^ yl) & long.MinValue) == 0 ? MaxValue : MinValue;
            //}

            //// overflow detection
            //if (((xl ^ yl) & long.MinValue) == 0) {
            //    // if signs of operands are equal but result is negative
            //    if (finalResult < 0) {
            //        return MaxValue;
            //    }
            //}
            //else {
            //    if (finalResult > 0 || (finalResult == 0 && (xl != 0 && yl != 0))) {
            //        // if signs of operands are different but result is positive
            //        // if result == 0, signs can be different yet there's no overflow, e.g. 0 * -1

            //        // if signs of operands are different, result is 0 and neither operand is 0
            //        // this is a special case of negative overflow
            //        return MinValue;
            //    }
            //}


            //return new Fix64(finalResult);

            // Very slow but correct implementation, basically just delegating the work to System.Decimal
            // System.Decimal is itself implemented in software using integers
            //var xD = (decimal)x;
            //var yD = (decimal)y;
            //var resultD = xD * yD;
            //return
            //    resultD >= (decimal)MaxValue ? MaxValue :
            //    resultD <= (decimal)MinValue ? MinValue :
            //    (Fix64)resultD;
        }

        public static explicit operator Fix64(long value) {
            return new Fix64(value * One.m_rawValue);
        }
        public static explicit operator long(Fix64 value) {
            return value.m_rawValue >> 32;
        }
        public static explicit operator Fix64(float value) {
            var temp = value * One.m_rawValue;
            temp += (temp >= 0) ? 0.5f : -0.5f;
            return new Fix64((long)temp);
        }
        public static explicit operator float(Fix64 value) {
            return (float)value.m_rawValue / One.m_rawValue;
        }
        public static explicit operator Fix64(double value) {
            var temp = value * One.m_rawValue;
            temp += (temp >= 0) ? 0.5 : -0.5;
            return new Fix64((long)temp);
        }
        public static explicit operator double(Fix64 value) {
            return (double)value.m_rawValue / One.m_rawValue;
        }
        public static explicit operator Fix64(decimal value) {
            var temp = value * One.m_rawValue;
            temp += (temp >= 0) ? 0.5m : -0.5m;
            return new Fix64((long)temp);
        }
        public static explicit operator decimal(Fix64 value) {
            return (decimal)value.m_rawValue / One.m_rawValue;
        }

        public override string ToString() {
            return ((decimal)this).ToString(CultureInfo.InvariantCulture);
        }

        public static Fix64 FromRaw(long rawValue) {
            return new Fix64(rawValue);
        }

        Fix64(long rawValue) {
            m_rawValue = rawValue;
        }
    }
}
