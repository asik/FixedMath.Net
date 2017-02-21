using System;
using Xunit;

namespace FixMath.NET
{
    public class Fix16Test
    {
        readonly int[] m_testcases = new[] {
          // Small numbers
          0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
          -1, -2, -3, -4, -5, -6, -7, -8, -9, -10,
  
          // Integer numbers
          0x10000, -0x10000, 0x20000, -0x20000, 0x30000, -0x30000,
          0x40000, -0x40000, 0x50000, -0x50000, 0x60000, -0x60000,
  
          // Fractions (1/2, 1/4, 1/8)
          0x8000, -0x8000, 0x4000, -0x4000, 0x2000, -0x2000,
  
          // Problematic carry
          0xFFFF, -0xFFFF, 0x1FFFF, -0x1FFFF, 0x3FFFF, -0x3FFFF,
  
          // Smallest and largest values
          int.MaxValue, int.MinValue,
  
          // Large random numbers
          831858892, 574794913, 2147272293, -469161054, -961611615,
          1841960234, 1992698389, 520485404, 560523116, -2094993050,
          -876897543, -67813629, 2146227091, 509861939, -1073573657,
  
          // Small random numbers
          -14985, 30520, -83587, 41129, 42137, 58537, -2259, 84142,
          -28283, 90914, 19865, 33191, 81844, -66273, -63215, -44459,
          -11326, 84295, 47515, -39324,
  
          // Tiny random numbers
          -171, -359, 491, 844, 158, -413, -422, -737, -575, -330,
          -376, 435, -311, 116, 715, -1024, -487, 59, 724, 993
        };

        static Fix16 Delta(Fix16 a, Fix16 b) {
            return a > b ? a - b : b - a;
        }

        #if !FIXMATH_NO_ROUNDING
        static readonly Fix16 MaxDelta = Fix16.FromRaw(1);
        #else
        static readonly Fix16 MaxDelta = Fix16.FromRaw(0);
#endif

        #region Basic Multiplication

        [Fact]
        public void BasicMultiplicationPosPos() {
            MultiplicationTest(5, 5, 25);
        }

        [Fact]
        public void BasicMultiplicationNegPos() {
            MultiplicationTest(-5, 5, -25);
        }

        [Fact]
        public void BasicMultiplicationNegNeg() {
            MultiplicationTest(-5, -5, 25);
        }

        [Fact]
        public void BasicMultiplicationPosNeg() {
            MultiplicationTest(5, -5, -25);
        }

        static void MultiplicationTest(int v1, int v2, int expected) {
            var a1 = (Fix16)v1;
            var a2 = (Fix16)v2;
            var expectedF = (Fix16)expected;
            var actual = a1 * a2;
            Assert.Equal(expectedF, actual);
        }

        static void MultiplicationTestRaw(int v1, int v2, int expected) {
            var a1 = Fix16.FromRaw(v1);
            var a2 = Fix16.FromRaw(v2);
            var expectedF = Fix16.FromRaw(expected);
            var actual = a1 * a2;
            Assert.Equal(expectedF, actual);
        }
        #endregion

#if !FIXMATH_NO_ROUNDING
        [Fact]
        public void MultiplicationRoundingCornerCases() {
            MultiplicationTestRaw(2, 0x8000, 1);
            MultiplicationTestRaw(-2, 0x8000,-1);
            MultiplicationTestRaw(3, 0x8000, 2);
            MultiplicationTestRaw(-3, 0x8000, -2);
            MultiplicationTestRaw(2, 0x7FFF, 1);
            MultiplicationTestRaw(-2, 0x7FFF, -1);
            MultiplicationTestRaw(2, 0x8001, 1);
            MultiplicationTestRaw(-2, 0x8001, -1);
        }
#endif

        [Fact]
        public void MultiplicationTestCases() {
            RunAllTestCases((f1, f2) => f1 * f2, (d1, d2) => d1 * d2, "*");
        }

        void RunAllTestCases(Func<Fix16, Fix16, Fix16> fix16Op, Func<double, double, double> doubleOp, string opChar) {

            for (var i = 0; i < m_testcases.Length; i++) {
                for (var j = 0; j < m_testcases.Length; j++) {
                    var a = Fix16.FromRaw(m_testcases[i]);
                    var b = Fix16.FromRaw(m_testcases[j]);
                    var result = fix16Op(a, b);

                    var fa = (double)a;
                    var fb = (double)b;
                    var fresult = (Fix16)doubleOp(fa, fb);

                    var max = (double)(Fix16.MaxValue);
                    var min = (double)(Fix16.MinValue);

                    if (Delta(fresult, result) > MaxDelta) {
                        if (doubleOp(fa, fb) > max || doubleOp(fa, fb) < min) {
#if !FIXMATH_NO_OVERFLOW
                            Assert.Equal(result, Fix16.Overflow);
                            //failures++;
#endif
                            // Legitimate overflow
                            continue;
                        }
                        Assert.True(false, string.Format("{0} {1} {2} = {3}\n{4} {1} {5} = {6}", a, opChar, b, result, fa, fb, fresult));
                    }
                }
            }
        }


        [Fact]
        public void BasicDivisionPosPos() {
            DivisionTest(15, 5, 3);
        }
        [Fact]
        public void BasicDivisionNegPos() {
            DivisionTest(-15, 5, -3);
        }
        [Fact]
        public void BasicDivisionPosNeg() {
            DivisionTest(15, -5, -3);
        }
        [Fact]
        public void BasicDivisionNegNeg() {
            DivisionTest(-15, -5, 3);
        }

        static void DivisionTest(int v1, int v2, int expected) {
            var a1 = (Fix16)v1;
            var a2 = (Fix16)v2;
            var expectedF = (Fix16)expected;
            var actual = a1 / a2;
            Assert.Equal(expectedF, actual);
        }

        static void DivisionTestRaw(int v1, int v2, int expected) {
            var a1 = Fix16.FromRaw(v1);
            var a2 = Fix16.FromRaw(v2);
            var expectedF = Fix16.FromRaw(expected);
            var actual = a1 / a2;
            Assert.Equal(expectedF, actual);
        }

        static void DivisionTestRaw(int v1, Fix16 v2, int expected) {
            var a1 = Fix16.FromRaw(v1);
            var expectedF = Fix16.FromRaw(expected);
            var actual = a1 / v2;
            Assert.Equal(expectedF, actual );
        }

#if !FIXMATH_NO_ROUNDING
        [Fact]
        public void DivisionRoundingCornerCases() {
            DivisionTestRaw(0, 10, 0);
            DivisionTestRaw(1, (Fix16)(2), 1);
            DivisionTestRaw(-1, (Fix16)(2), -1);
            DivisionTestRaw(1, (Fix16)(-2), -1);
            DivisionTestRaw(-1, (Fix16)(-2), 1);
            DivisionTestRaw(3, (Fix16)(2), 2);
            DivisionTestRaw(-3, (Fix16)(2), -2);
            DivisionTestRaw(3, (Fix16)(-2), -2);
            DivisionTestRaw(-3, (Fix16)(-2), 2);
            DivisionTestRaw(2, 0x7FFF, 4);
            DivisionTestRaw(-2, 0x7FFF, -4);
            DivisionTestRaw(2, 0x8001, 4);
            DivisionTestRaw(-2, 0x8001, -4);
        }
#endif

        [Fact]
        public void DivisionTestCases() {
            RunAllTestCases((f1, f2) => f1 / f2, (d1, d2) => d1 / d2, "/");
        }

        [Fact]
        public void AdditionTestCases() {
            RunAllTestCases((f1, f2) => f1 + f2, (d1, d2) => d1 + d2, "+");
        }

        [Fact]
        public void SubstractionTestCases() {
            RunAllTestCases((f1, f2) => f1 - f2, (d1, d2) => d1 - d2, "-");
        }

        static void SquareRootTestRaw(int x, int expected) {
            var xf = Fix16.FromRaw(x);
            var expectedF = Fix16.FromRaw(expected);
            var actual = Fix16.Sqrt(xf);
            Assert.Equal(expectedF, actual);
        }

#if !FIXMATH_NO_ROUNDING
        [Fact]
        public void SquareRootRoundingCornerCases() {
            SquareRootTestRaw(214748302, 3751499);
            SquareRootTestRaw(214748303, 3751499);
            SquareRootTestRaw(214748359, 3751499);
            SquareRootTestRaw(214748360, 3751500);
        }
#endif

        [Fact]
        public void SinParabola() {
            for (var angle = -Fix16.Pi; angle <= Fix16.Pi; angle += Fix16.Pi / (Fix16)16) {
                var actual = (double)Fix16.SinParabola(angle);
                var expected = Math.Sin((double)angle);
                var diff = Math.Abs(actual - expected);
                Assert.True(diff < 0.002);
            }
        }

        [Fact]
        public void Sin() {
            for (var angle = (Fix16)(-100); angle <= (Fix16)100; angle += Fix16.One / (Fix16)10) {
                var actual = (double)Fix16.Sin(angle);
                var expected = Math.Sin((double)angle);
                var diff = Math.Abs(actual - expected);
                Assert.True(diff < 0.00025);
            }
        }

        [Fact]
        public void Cos() {
            for (var angle = (Fix16)(-100); angle <= (Fix16)100; angle += Fix16.One / (Fix16)10) {
                var actual = (double)Fix16.Cos(angle);
                var expected = Math.Cos((double)angle);
                var diff = Math.Abs(actual - expected);
                Assert.True(diff < 0.00025);
            }
        }

        [Fact]
        public void Tan() {
            for (var angle = (Fix16)(-100); angle <= (Fix16)100; angle += Fix16.One / (Fix16)10) {
                var actual = (double)Fix16.Tan(angle);
                var expected = Math.Tan((double)angle);
                var relativeDiff = Math.Abs(actual - expected) / Math.Abs(expected);
                Assert.True(relativeDiff < 1);
            }
        }

        [Fact]
        public void Atan2() {
            for (var angleX = (Fix16)(-50); angleX <= (Fix16)50; angleX += Fix16.One / (Fix16)9) {
                for (var angleY = (Fix16)(-50); angleY <= (Fix16)50; angleY += Fix16.One / (Fix16)7) {
                    var actual = (double)Fix16.Atan2(angleX, angleY);
                    var expected = Math.Atan2((double)angleX, (double)angleY);
                    var diff = Math.Abs(actual - expected);
                    Assert.True(diff < 0.015);
                }
            }
        }

        [Fact]
        public void SubstractionSign() {
            for (int i = -1; i <= 1; ++i) {
                for (int j = -1; j <= 1; ++j) {
                    var a1 = (Fix16)i;
                    var a2 = (Fix16)j;
                    var actual = (int)(a1 - a2);
                    var expected = i - j;
                    Assert.Equal(expected, actual);
                }
            }
        }

        [Fact]
        public void AdditionSign() {
            for (int i = -1; i <= 1; ++i) {
                for (int j = -1; j <= 1; ++j) {
                    var a1 = (Fix16)i;
                    var a2 = (Fix16)j;
                    var actual = (int)(a1 + a2);
                    var expected = i + j;
                    Assert.Equal(expected, actual);
                }
            }
        }

        //[Fact]
        public void Sqrt() {
            for (int i = (int.MaxValue / 2); i <= int.MaxValue; i += 21) {
                var f = Fix16.FromRaw(i);
                var expected = Math.Sqrt((double)f);
                var actual = (double)Fix16.Sqrt(f);
                var delta = Math.Abs(expected - actual);
                Assert.True(delta <= 0.0625);
            }
        }
    }
}
