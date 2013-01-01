using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace FixMath.NET {

    [TestFixture]
    class Fix64Tests {

        readonly long[] m_testCases = new[] {
            // Small numbers
            0L, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
            -1, -2, -3, -4, -5, -6, -7, -8, -9, -10,
  
            // Integer numbers
            0x100000000, -0x100000000, 0x200000000, -0x200000000, 0x300000000, -0x300000000,
            0x400000000, -0x400000000, 0x500000000, -0x500000000, 0x600000000, -0x600000000,
  
            // Fractions (1/2, 1/4, 1/8)
            0x80000000, -0x80000000, 0x40000000, -0x40000000, 0x20000000, -0x20000000,
  
            // Problematic carry
            0xFFFFFFFF, -0xFFFFFFFF, 0x1FFFFFFFF, -0x1FFFFFFFF, 0x3FFFFFFFF, -0x3FFFFFFFF,
  
            // Smallest and largest values
            long.MaxValue, long.MinValue,
  
            // Large random numbers
            6791302811978701836, -8192141831180282065, 6222617001063736300, -7871200276881732034,
            8249382838880205112, -7679310892959748444, 7708113189940799513, -5281862979887936768,
            8220231180772321456, -5204203381295869580, 6860614387764479339, -9080626825133349457,
            6658610233456189347, -6558014273345705245, 6700571222183426493,
  
            // Small random numbers
            -436730658, -2259913246, 329347474, 2565801981, 3398143698, 137497017, 1060347500,
            -3457686027, 1923669753, 2891618613, 2418874813, 2899594950, 2265950765, -1962365447,
            3077934393
  
            // Tiny random numbers
            -171, -359, 491, 844, 158, -413, -422, -737, -575, -330,
            -376, 435, -311, 116, 715, -1024, -487, 59, 724, 993
        };

        [Test]
        public void LongToFix64AndBack() {
            var sources = new[] {   long.MinValue, int.MinValue - 1L,   int.MinValue, -1L, 0L, 1L, int.MaxValue, int.MaxValue + 1L, long.MaxValue };
            var expecteds = new[] { 0L,            int.MaxValue,        int.MinValue, -1L, 0L, 1L, int.MaxValue, int.MinValue,      -1L };
            for (int i = 0; i < sources.Length; ++i) {
                var expected = expecteds[i];
                var f = (Fix64)sources[i];
                var actual = (long)f;
                Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void DoubleToFix64AndBack() {
            var sources = new[] { int.MinValue, -1.0, -0.0, 0.0, 1.0, int.MaxValue };
            var expecteds = new[] { int.MinValue, -1.0, 0.0, 0.0, 1.0, int.MaxValue };
            for (int i = 0; i < sources.Length; ++i) {
                var expected = expecteds[i];
                var f = (Fix64)sources[i];
                var actual = (double)f;
                Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void DecimalToFix64AndBack() {
            var sources = new[] { int.MinValue, -123456789.123456789m, -1.0m, -0.0m, 0.0m, 1.0m, 123456789.123456789m, int.MaxValue };
            var expecteds = new[] { int.MinValue, -123456789.123456789m, -1.0m, 0.0m, 0.0m, 1.0m, 123456789.123456789m, int.MaxValue };
            for (int i = 0; i < sources.Length; ++i) {
                var expected = expecteds[i];
                var f = (Fix64)sources[i];
                var actual = (decimal)f;
                actual = decimal.Round(actual, 9); // we're supposed to have 9 significant decimal places
                Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void Addition() {
            var terms1 = new[] { Fix64.MinValue, (Fix64)(-1), Fix64.Zero, Fix64.One, Fix64.MaxValue };
            var terms2 = new[] { (Fix64)(-1), (Fix64)2, (Fix64)(-1.5m), (Fix64)(-2), Fix64.One };
            var expecteds = new[] { Fix64.MinValue, Fix64.One, (Fix64)(-1.5m), (Fix64)(-1), Fix64.MaxValue };
            for (int i = 0; i < terms1.Length; ++i) {
                var actual = terms1[i] + terms2[i];
                var expected = expecteds[i];
                Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void Substraction() {
            var terms1 = new[] { Fix64.MinValue, (Fix64)(-1), Fix64.Zero, Fix64.One, Fix64.MaxValue };
            var terms2 = new[] { Fix64.One, (Fix64)(-2), (Fix64)(1.5m), (Fix64)(2), (Fix64)(-1) };
            var expecteds = new[] { Fix64.MinValue, Fix64.One, (Fix64)(-1.5m), (Fix64)(-1), Fix64.MaxValue };
            for (int i = 0; i < terms1.Length; ++i) {
                var actual = terms1[i] - terms2[i];
                var expected = expecteds[i];
                Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void BasicMultiplication() {
            var term1s = new[] { 0m, 1m, -1m, 5m, -5m, 0.5m, -0.5m, -1.0m };
            var term2s = new[] { 16m, 16m, 16m, 16m, 16m, 16m, 16m, -1.0m };
            var expecteds = new[] { 0L, 16, -16, 80, -80, 8, -8, 1 };
            for (int i = 0; i < term1s.Length; ++i) {
                var expected = expecteds[i];
                var actual = (long)((Fix64)term1s[i] * (Fix64)term2s[i]);
                Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void MultiplicationTestCases() {
            var sw = new Stopwatch();
            int failures = 0;
            for (int i = 0; i < m_testCases.Length; ++i) {
                for (int j = 0; j < m_testCases.Length; ++j) {
                    var x = Fix64.FromRaw(m_testCases[i]);
                    var y = Fix64.FromRaw(m_testCases[j]);
                    var xM = (decimal)x;
                    var yM = (decimal)y;
                    var expected = xM * yM;
                    expected =
                        expected > (decimal)Fix64.MaxValue
                            ? (decimal)Fix64.MaxValue
                            : expected < (decimal)Fix64.MinValue
                                  ? (decimal)Fix64.MinValue
                                  : expected;
                    sw.Start();
                    var actual = x * y;
                    sw.Stop();
                    var actualM = (decimal)actual;
                    var maxDelta = (decimal)Fix64.FromRaw(1);
                    if (Math.Abs(actualM - expected) > maxDelta) {
                        Console.WriteLine("Failed for FromRaw({0}) * FromRaw({1}): expected {2} but got {3}",
                            m_testCases[i],
                            m_testCases[j],
                            (Fix64)expected,
                            actualM);
                        ++failures;
                    }
                }
            }
            Console.WriteLine("{0} total, {1} per multiplication", sw.ElapsedMilliseconds, (double)sw.Elapsed.Milliseconds / (m_testCases.Length * m_testCases.Length));
            Assert.Less(failures, 1);
        }


        static void Ignore<T>(T value) { }

        [Test]
        public void DivisionTestCases() {
            var sw = new Stopwatch();
            int failures = 0;
            for (int i = 0; i < m_testCases.Length; ++i) {
                for (int j = 0; j < m_testCases.Length; ++j) {
                    var x = Fix64.FromRaw(m_testCases[i]);
                    var y = Fix64.FromRaw(m_testCases[j]);
                    var xM = (decimal)x;
                    var yM = (decimal)y;

                    if (m_testCases[j] == 0) {
                        Assert.Throws<DivideByZeroException>(() => Ignore(x / y));
                    }
                    else {
                        var expected = xM / yM;
                        expected =
                            expected > (decimal)Fix64.MaxValue
                                ? (decimal)Fix64.MaxValue
                                : expected < (decimal)Fix64.MinValue
                                      ? (decimal)Fix64.MinValue
                                      : expected;
                        sw.Start();
                        var actual = x / y;
                        sw.Stop();
                        var actualM = (decimal)actual;
                        var maxDelta = (decimal)Fix64.FromRaw(1);
                        if (Math.Abs(actualM - expected) > maxDelta) {
                            Console.WriteLine("Failed for FromRaw({0}) / FromRaw({1}): expected {2} but got {3}",
                                m_testCases[i],
                                m_testCases[j],
                                (Fix64)expected,
                                actualM);
                            ++failures;
                        }
                    }
                }
            }
            Console.WriteLine("{0} total, {1} per multiplication", sw.ElapsedMilliseconds, (double)sw.Elapsed.Milliseconds / (m_testCases.Length * m_testCases.Length));
            Assert.Less(failures, 1);
        }



        [Test]
        public void Sign() {
            var sources = new[] { Fix64.MinValue, (Fix64)(-1), Fix64.Zero, Fix64.One, Fix64.MaxValue };
            var expecteds = new[] { -1, -1, 0, 1, 1 };
            for (int i = 0; i < sources.Length; ++i) {
                var actual = Fix64.Sign(sources[i]);
                var expected = expecteds[i];
                Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void Abs() {
            Assert.Throws<OverflowException>(() => Fix64.Abs(Fix64.MinValue));
            var sources = new[] { -1, 0, 1, int.MaxValue };
            var expecteds = new[] { 1, 0, 1, int.MaxValue };
            for (int i = 0; i < sources.Length; ++i) {
                var actual = Fix64.Abs((Fix64)sources[i]);
                var expected = (Fix64)expecteds[i];
                Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void Floor() {
            var sources = new[] { -5.1m, -1, 0, 1, 5.1m };
            var expecteds = new[] { -6m, -1, 0, 1, 5m };
            for (int i = 0; i < sources.Length; ++i) {
                var actual = (decimal)Fix64.Floor((Fix64)sources[i]);
                var expected = expecteds[i];
                Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void Ceiling() {
            var sources = new[] { -5.1m, -1, 0, 1, 5.1m };
            var expecteds = new[] { -5m, -1, 0, 1, 6m };
            for (int i = 0; i < sources.Length; ++i) {
                var actual = (decimal)Fix64.Ceiling((Fix64)sources[i]);
                var expected = expecteds[i];
                Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void Round() {
            var sources = new[] { -5.5m, -5.1m, -4.5m, -4.4m, -1, 0, 1, 4.5m, 4.6m, 5.4m, 5.5m };
            var expecteds = new[] { -6m, -5m, -4m, -4m, -1, 0, 1, 4m, 5m, 5m, 6m };
            for (int i = 0; i < sources.Length; ++i) {
                var actual = (decimal)Fix64.Round((Fix64)sources[i]);
                var expected = expecteds[i];
                Assert.AreEqual(expected, actual);
            }
        }
    }
}
