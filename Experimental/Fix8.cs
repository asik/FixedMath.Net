using System;
using System.Globalization;

namespace FixMath.NET {

    /// <summary>
    /// Represents a Q3.4 fixed-point number.
    /// </summary>
    /// <remarks>
    /// I wrote this type essentially as a stepping stone towards writing Fix64, 
    /// so while it works, it is not very optimized and lacks some operations.
    /// </remarks>
    struct Fix8 : IEquatable<Fix8>, IComparable<Fix8> {
        readonly sbyte m_rawValue;
        /// <summary>
        /// Represents the value 1 in Q3.4 format.
        /// </summary>
        public static readonly Fix8 One = new Fix8(1 << 4);
        /// <summary>
        /// Minimum value, this is -8.
        /// </summary>
        public static readonly Fix8 MinValue = new Fix8(sbyte.MinValue);
        /// <summary>
        /// Maximum value, this is 7.9375.
        /// </summary>
        public static readonly Fix8 MaxValue = new Fix8(sbyte.MaxValue);
        // Precision of this type is 2^-4
        public const decimal Precision = 0.0625m;
        /// <summary>
        /// The value of Pi
        /// </summary>
        public static readonly Fix8 Pi = (Fix8)3.1415926535897932384626433833M;
        public static readonly Fix8 PiOver2 = (Fix8)1.5707963267948966192313216916M;
        public static readonly Fix8 PiInv = (Fix8)0.3183098861837906715377675267M;
        public static readonly Fix8 PiOver2Inv = (Fix8)0.6366197723675813430755350535M;

        /// <summary>
        /// Returns the absolute value of a Fix8 number.
        /// Note: Abs(Fix8.MinValue) == Fix8.MaxValue.
        /// </summary>
        public static Fix8 Abs(Fix8 value) {
            if (value.m_rawValue == sbyte.MinValue) {
                return MaxValue;
            }

            // branchless implementation, see http://www.strchr.com/optimized_abs_function
            var mask = (sbyte)(value.m_rawValue >> 7);
            return new Fix8((sbyte)((value.m_rawValue + mask) ^ mask));
        }

        /// <summary>
        /// Returns the largest integer less than or equal to the specified number.
        /// </summary>
        public static Fix8 Floor(Fix8 value) {
            // Just zero out the decimal part
            return new Fix8((sbyte)(value.m_rawValue & 0xF0));
        }

        /// <summary>
        /// Returns the smallest integral value that is greater than or equal to the specified number.
        /// </summary>
        public static Fix8 Ceiling(Fix8 value) {
            var hasDecimalPart = (value.m_rawValue & 0x0F) != 0;
            return hasDecimalPart ? Floor(value) + One : value;
        }

        /// <summary>
        /// Rounds a value to the nearest integral value.
        /// If the value is halfway between an even and an uneven value, returns the even value.
        /// </summary>
        public static Fix8 Round(Fix8 value) {
            var decimalPart = (sbyte)(value.m_rawValue & 0x0F);
            var integralPart = Floor(value);
            if (decimalPart < 0x08) {
                return integralPart;
            }
            if (decimalPart > 0x08) {
                return integralPart + One;
            }
            // if number is halfway between two values, round to the nearest even number
            // this is the method used by System.Math.Round().
            return (integralPart.m_rawValue & (1 << 4)) == 0
                       ? integralPart
                       : integralPart + One;
        }

        /// <summary>
        /// Returns a number indicating the sign of a Fix8 number.
        /// Returns 1 if the value is positive, 0 if is 0, and -1 if it is negative.
        /// </summary>
        public static int Sign(Fix8 value) {
            return
                value.m_rawValue < 0 ? -1 :
                value.m_rawValue > 0 ? 1 :
                0;
        }

        /// <summary>
        /// Builds a Fix8 directly from a value, without shifting it.
        /// </summary>
        public static Fix8 FromRaw(sbyte rawValue) {
            return new Fix8(rawValue);
        }

        public static explicit operator decimal(Fix8 value) {
            return (decimal)value.m_rawValue / One.m_rawValue;
        }

        public static explicit operator Fix8(decimal value) {
            var nearestExact = Math.Round(value * 16m);
            return new Fix8((sbyte)(nearestExact));
        }

        public static Fix8 operator +(Fix8 x, Fix8 y) {
            var xl = x.m_rawValue;
            var yl = y.m_rawValue;
            var sum = (sbyte)(xl + yl);
            // if signs of operands are equal and signs of sum and x are different
            if (((~(xl ^ yl) & (xl ^ sum)) & sbyte.MinValue) != 0) {
                sum = xl > 0 ? sbyte.MaxValue : sbyte.MinValue;
            }
            return new Fix8(sum);
        }

        public static Fix8 operator -(Fix8 x, Fix8 y) {
            var xl = x.m_rawValue;
            var yl = y.m_rawValue;
            var diff = (sbyte)(xl - yl);
            // if signs of operands are different and signs of sum and x are different
            if ((((xl ^ yl) & (xl ^ diff)) & sbyte.MinValue) != 0) {
                diff = xl < 0 ? sbyte.MinValue : sbyte.MaxValue;
            }
            return new Fix8(diff);
        }

        static sbyte AddOverflowHelper(sbyte x, sbyte y, ref bool overflow) {
            var sum = (sbyte)(x + y);
            // x + y overflows if sign(x) ^ sign(y) != sign(sum)
            overflow |= ((x ^ y ^ sum) & sbyte.MinValue) != 0;
            return sum;
        }

        /// <summary>
        /// Multiplies two Fix8 numbers.
        /// Deals with overflow by saturation.
        /// </summary>
        public static Fix8 operator *(Fix8 x, Fix8 y) {
            // Using the cross-multiplication algorithm, for learning purposes.
            // It would be both trivial and much faster to use an Int16, but this technique
            // won't work for a Fix64, since there's no Int128 or equivalent (and BigInteger is too slow).

            sbyte xl = x.m_rawValue;
            sbyte yl = y.m_rawValue;

            byte xlo = (byte)(xl & 0x0F);
            sbyte xhi = (sbyte)(xl >> 4);
            byte ylo = (byte)(yl & 0x0F);
            sbyte yhi = (sbyte)(yl >> 4);

            byte lolo = (byte)(xlo * ylo);
            sbyte lohi = (sbyte)((sbyte)xlo * yhi);
            sbyte hilo = (sbyte)(xhi * (sbyte)ylo);
            sbyte hihi = (sbyte)(xhi * yhi);

            byte loResult = (byte)(lolo >> 4);
            sbyte midResult1 = lohi;
            sbyte midResult2 = hilo;
            sbyte hiResult = (sbyte)(hihi << 4);

            bool overflow = false;
            sbyte sum = AddOverflowHelper((sbyte)loResult, midResult1, ref overflow);
            sum = AddOverflowHelper(sum, midResult2, ref overflow);
            sum = AddOverflowHelper(sum, hiResult, ref overflow);
            //sbyte sum = (sbyte)((sbyte)loResult + midResult1 + midResult2 + hiResult);

            bool opSignsEqual = ((xl ^ yl) & sbyte.MinValue) == 0;

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
                // If signs differ, both operands' magnitudes are greater than 1,
                // and the result is greater than the negative operand, then there was negative overflow.
                sbyte posOp, negOp;
                if (xl > yl) {
                    posOp = xl;
                    negOp = yl;
                }
                else {
                    posOp = yl;
                    negOp = xl;
                }
                if (sum > negOp && negOp < -(1 << 4) && posOp > (1 << 4)) {
                    return MinValue;
                }
            }

            // if the top 4 bits of hihi (unused in the result) are neither all 0s nor 1s,
            // then this means the result overflowed.
            sbyte topCarry = (sbyte)(hihi >> 4);
            // -17 (-1.0625) is a problematic value which never causes overflow but messes up the carry bits
            if (topCarry != 0 && topCarry != -1 && xl != -17 && yl != -17) {
                return opSignsEqual ? MaxValue : MinValue;
            }

            // Round up if necessary, but don't overflow
            var lowCarry = (byte)(lolo << 4);
            if (lowCarry >= 0x80 && sum < sbyte.MaxValue) {
                ++sum;
            }

            return new Fix8(sum);
        }

        static int CountLeadingZeroes(byte x) {
            int result = 0;
            if (x == 0) { return 8; }
            while ((x & 0xF0) == 0) { result += 4; x <<= 4; }
            while ((x & 0x80) == 0) { result += 1; x <<= 1; }
            return result;
        }

        public static Fix8 operator /(Fix8 x, Fix8 y) {
            var xl = x.m_rawValue;
            var yl = y.m_rawValue;

            if (yl == 0) {
                throw new DivideByZeroException();
            }

            var remainder = (byte)(xl >= 0 ? xl : -xl);
            var divider = (byte)(yl >= 0 ? yl : -yl);
            var quotient = (byte)0;
            var bitPos = 5;


            // If the divider is divisible by 2^n, take advantage of it.
            while ((divider & 0xF) == 0 && bitPos >= 4) {
                divider >>= 4;
                bitPos -= 4;
            }

            while (remainder != 0 && bitPos >= 0) {
                int shift = CountLeadingZeroes(remainder);
                if (shift > bitPos) {
                    shift = bitPos;
                }
                remainder <<= shift;
                bitPos -= shift;

                var div = (byte)(remainder / divider);
                remainder = (byte)(remainder % divider);
                quotient += (byte)(div << bitPos);

                if ((div & ~(0xFF >> bitPos)) != 0) {
                    return ((xl ^ yl) & sbyte.MinValue) == 0 ? MaxValue : MinValue;
                }

                remainder <<= 1;
                --bitPos;
            }

            // rounding
            ++quotient;
            var result = (sbyte)(quotient >> 1);
            if (((xl ^ yl) & sbyte.MinValue) != 0) {
                result = (sbyte)-result;
            }

            return new Fix8(result);
        }

        /// <summary>
        /// Returns the square root of a specified number.
        /// Throws an ArgumentException if the number is negative.
        /// </summary>
        public static Fix8 Sqrt(Fix8 x) {
            var xl = x.m_rawValue;
            if (xl < 0) {
                // We cannot represent NaN, and Sqrt is undefined for x < 0. 
                // So we just throw an exception.
                throw new ArgumentOutOfRangeException("Negative value passed to Sqrt", "x");
            }

            var num = (byte)xl;
            var result = (byte)0;

            // second-to-top bit
            byte bit = 1 << 6;

            while (bit > num) {
                bit >>= 2;
            }

            // The main part is executed twice, in order to avoid
            // using 16 bit values in computations.
            for (var n = 0; n < 2; n++) {
                // First we get the top 6 bits of the answer.
                while (bit != 0) {
                    if (num >= result + bit) {
                        num -= (byte)(result + bit);
                        result = (byte)((result >> 1) + bit);
                    }
                    else {
                        result = (byte)(result >> 1);
                    }
                    bit >>= 2;
                }

                if (n == 0) {
                    // Then process it again to get the lowest (sizeof(basetype) / 4) bits.
                    if (num > (1 << 4) - 1) {
                        // The remainder 'num' is too large to be shifted left
                        // by 16, so we have to add 1 to result manually and
                        // adjust 'num' accordingly.
                        // num = a - (result + 0.5)^2
                        //       = num + result^2 - (result + 0.5)^2
                        //       = num - result - 0.5
                        num -= result;
                        num = (byte)((num << 4) - 0x8);
                        result = (byte)((result << 4) + 0x8);
                    }
                    else {
                        num <<= 4;
                        result <<= 4;
                    }

                    // shift = (sizeof(basetype) / 2) - 2, Hence 30 for long, 6 for short and 2 for byte.
                    bit = 1 << 2;
                }
            }
            // Finally, if next bit would have been 1, round the result upwards.
            if (num > result) {
                ++result;
            }
            return new Fix8((sbyte)result);
        }


        /// <summary>
        /// If it worked, this method would return the Sine of x
        /// But it doesn't, so don't use it
        /// </summary>
        public static Fix8 Sin(Fix8 x) {
            // Using Taylor series http://dotancohen.com/eng/taylor-sine.php

            //TODO currently working for 0 <= x <= 1.5625, check what's going on outside that range

            // First, constrain value to range (-pi/2) - (pi/2)
            // since we'll need to compute up to value^5 and MaxValue ~= 8
            var shift = (((int)Floor(x * PiOver2Inv) + 1) / 2) % 2 == 1;

            var source = (sbyte)(x.m_rawValue % PiOver2.m_rawValue);
            if (shift) {
                source += (sbyte)(source < 0 ? Pi.m_rawValue : -(Pi.m_rawValue));
            }
            var sourceF = new Fix8(source);

            // Calculate sin(source) using Taylor series
            var sourceSq = sourceF * sourceF;
            var result = source;
            sourceF = sourceF * sourceSq; // source^3
            result -= (sbyte)(sourceF.m_rawValue / 6); // 3!
            sourceF = sourceF * sourceSq; // source^5
            result += (sbyte)(sourceF.m_rawValue / 120); // 5!

            if (shift) {
                result = (sbyte)(-result);
            }

            return new Fix8(result);
        }



        public static Fix8 operator %(Fix8 x, Fix8 y) {
            return new Fix8((sbyte)(x.m_rawValue % y.m_rawValue));
        }



        public bool Equals(Fix8 other) {
            return m_rawValue == other.m_rawValue;
        }

        public int CompareTo(Fix8 other) {
            return m_rawValue.CompareTo(other.m_rawValue);
        }

        public override int GetHashCode() {
            return m_rawValue;
        }

        public override bool Equals(object obj) {
            var fix8 = obj as Fix8?;
            return fix8.HasValue && fix8.Value.m_rawValue == m_rawValue;
        }

        public override string ToString() {
            return ((decimal)this).ToString(CultureInfo.InvariantCulture);
        }

        Fix8(sbyte value) {
            m_rawValue = value;
        }
    }
}
