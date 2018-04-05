using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;

namespace FixMath.NET
{
    public class Fix64Tests
    {

        long[] m_testCases = new[] {
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
            - 171,
            -359, 491, 844, 158, -413, -422, -737, -575, -330,
            -376, 435, -311, 116, 715, -1024, -487, 59, 724, 993
        };

        [Fact]
        public void Precision()
        {
            Assert.Equal(0.00000000023283064365386962890625m, Fix64.Precision);
        }

        [Fact]
        public void LongToFix64AndBack()
        {
            var sources = new[] { long.MinValue, int.MinValue - 1L, int.MinValue, -1L, 0L, 1L, int.MaxValue, int.MaxValue + 1L, long.MaxValue };
            var expecteds = new[] { 0L, int.MaxValue, int.MinValue, -1L, 0L, 1L, int.MaxValue, int.MinValue, -1L };
            for (int i = 0; i < sources.Length; ++i)
            {
                var expected = expecteds[i];
                var f = (Fix64)sources[i];
                var actual = (long)f;
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void DoubleToFix64AndBack()
        {
            var sources = new[] {
                (double)int.MinValue,
                -(double)Math.PI,
                -(double)Math.E,
                -1.0,
                -0.0,
                0.0,
                1.0,
                (double)Math.PI,
                (double)Math.E,
                (double)int.MaxValue
            };

            foreach (var value in sources)
            {
                AreEqualWithinPrecision(value, (double)(Fix64)value);
            }
        }

        static void AreEqualWithinPrecision(decimal value1, decimal value2)
        {
            Assert.True(Math.Abs(value2 - value1) < Fix64.Precision);
        }

        static void AreEqualWithinPrecision(double value1, double value2)
        {
            Assert.True(Math.Abs(value2 - value1) < (double)Fix64.Precision);
        }

        [Fact]
        public void DecimalToFix64AndBack()
        {

            Assert.Equal(Fix64.MaxValue, (Fix64)(decimal)Fix64.MaxValue);
            Assert.Equal(Fix64.MinValue, (Fix64)(decimal)Fix64.MinValue);

            var sources = new[] {
                int.MinValue,
                -(decimal)Math.PI,
                -(decimal)Math.E,
                -1.0m,
                -0.0m,
                0.0m,
                1.0m,
                (decimal)Math.PI,
                (decimal)Math.E,
                int.MaxValue
            };

            foreach (var value in sources)
            {
                AreEqualWithinPrecision(value, (decimal)(Fix64)value);
            }
        }

        [Fact]
        public void Addition()
        {
            var terms1 = new[] { Fix64.MinValue, (Fix64)(-1), Fix64.Zero, Fix64.One, Fix64.MaxValue };
            var terms2 = new[] { (Fix64)(-1), (Fix64)2, (Fix64)(-1.5m), (Fix64)(-2), Fix64.One };
            var expecteds = new[] { Fix64.MinValue, Fix64.One, (Fix64)(-1.5m), (Fix64)(-1), Fix64.MaxValue };
            for (int i = 0; i < terms1.Length; ++i)
            {
                var actual = terms1[i] + terms2[i];
                var expected = expecteds[i];
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Substraction()
        {
            var terms1 = new[] { Fix64.MinValue, (Fix64)(-1), Fix64.Zero, Fix64.One, Fix64.MaxValue };
            var terms2 = new[] { Fix64.One, (Fix64)(-2), (Fix64)(1.5m), (Fix64)(2), (Fix64)(-1) };
            var expecteds = new[] { Fix64.MinValue, Fix64.One, (Fix64)(-1.5m), (Fix64)(-1), Fix64.MaxValue };
            for (int i = 0; i < terms1.Length; ++i)
            {
                var actual = terms1[i] - terms2[i];
                var expected = expecteds[i];
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void BasicMultiplication()
        {
            var term1s = new[] { 0m, 1m, -1m, 5m, -5m, 0.5m, -0.5m, -1.0m };
            var term2s = new[] { 16m, 16m, 16m, 16m, 16m, 16m, 16m, -1.0m };
            var expecteds = new[] { 0L, 16, -16, 80, -80, 8, -8, 1 };
            for (int i = 0; i < term1s.Length; ++i)
            {
                var expected = expecteds[i];
                var actual = (long)((Fix64)term1s[i] * (Fix64)term2s[i]);
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void MultiplicationTestCases()
        {
            var sw = new Stopwatch();
            int failures = 0;
            for (int i = 0; i < m_testCases.Length; ++i)
            {
                for (int j = 0; j < m_testCases.Length; ++j)
                {
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
                    if (Math.Abs(actualM - expected) > maxDelta)
                    {
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
            Assert.True(failures < 1);
        }


        static void Ignore<T>(T value) { }

        [Fact]
        public void DivisionTestCases()
        {
            var sw = new Stopwatch();
            int failures = 0;
            for (int i = 0; i < m_testCases.Length; ++i)
            {
                for (int j = 0; j < m_testCases.Length; ++j)
                {
                    var x = Fix64.FromRaw(m_testCases[i]);
                    var y = Fix64.FromRaw(m_testCases[j]);
                    var xM = (decimal)x;
                    var yM = (decimal)y;

                    if (m_testCases[j] == 0)
                    {
                        Assert.Throws<DivideByZeroException>(() => Ignore(x / y));
                    }
                    else
                    {
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
                        if (Math.Abs(actualM - expected) > maxDelta)
                        {
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
            Console.WriteLine("{0} total, {1} per division", sw.ElapsedMilliseconds, (double)sw.Elapsed.Milliseconds / (m_testCases.Length * m_testCases.Length));
            Assert.True(failures < 1);
        }



        [Fact]
        public void Sign()
        {
            var sources = new[] { Fix64.MinValue, (Fix64)(-1), Fix64.Zero, Fix64.One, Fix64.MaxValue };
            var expecteds = new[] { -1, -1, 0, 1, 1 };
            for (int i = 0; i < sources.Length; ++i)
            {
                var actual = Fix64.Sign(sources[i]);
                var expected = expecteds[i];
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Abs()
        {
            Assert.Equal(Fix64.MaxValue, Fix64.Abs(Fix64.MinValue));
            var sources = new[] { -1, 0, 1, int.MaxValue };
            var expecteds = new[] { 1, 0, 1, int.MaxValue };
            for (int i = 0; i < sources.Length; ++i)
            {
                var actual = Fix64.Abs((Fix64)sources[i]);
                var expected = (Fix64)expecteds[i];
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void FastAbs()
        {
            Assert.Equal(Fix64.MinValue, Fix64.FastAbs(Fix64.MinValue));
            var sources = new[] { -1, 0, 1, int.MaxValue };
            var expecteds = new[] { 1, 0, 1, int.MaxValue };
            for (int i = 0; i < sources.Length; ++i)
            {
                var actual = Fix64.FastAbs((Fix64)sources[i]);
                var expected = (Fix64)expecteds[i];
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Floor()
        {
            var sources = new[] { -5.1m, -1, 0, 1, 5.1m };
            var expecteds = new[] { -6m, -1, 0, 1, 5m };
            for (int i = 0; i < sources.Length; ++i)
            {
                var actual = (decimal)Fix64.Floor((Fix64)sources[i]);
                var expected = expecteds[i];
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Ceiling()
        {
            var sources = new[] { -5.1m, -1, 0, 1, 5.1m };
            var expecteds = new[] { -5m, -1, 0, 1, 6m };
            for (int i = 0; i < sources.Length; ++i)
            {
                var actual = (decimal)Fix64.Ceiling((Fix64)sources[i]);
                var expected = expecteds[i];
                Assert.Equal(expected, actual);
            }

            Assert.Equal(Fix64.MaxValue, Fix64.Ceiling(Fix64.MaxValue));
        }

        [Fact]
        public void Round()
        {
            var sources = new[] { -5.5m, -5.1m, -4.5m, -4.4m, -1, 0, 1, 4.5m, 4.6m, 5.4m, 5.5m };
            var expecteds = new[] { -6m, -5m, -4m, -4m, -1, 0, 1, 4m, 5m, 5m, 6m };
            for (int i = 0; i < sources.Length; ++i)
            {
                var actual = (decimal)Fix64.Round((Fix64)sources[i]);
                var expected = expecteds[i];
                Assert.Equal(expected, actual);
            }
            Assert.Equal(Fix64.MaxValue, Fix64.Round(Fix64.MaxValue));
        }


        [Fact]
        public void Sqrt()
        {
            for (int i = 0; i < m_testCases.Length; ++i)
            {
                var f = Fix64.FromRaw(m_testCases[i]);
                if (Fix64.Sign(f) < 0)
                {
                    Assert.Throws<ArgumentOutOfRangeException>(() => Fix64.Sqrt(f));
                }
                else
                {
                    var expected = Math.Sqrt((double)f);
                    var actual = (double)Fix64.Sqrt(f);
                    var delta = (decimal)Math.Abs(expected - actual);
                    Assert.True(delta <= Fix64.Precision);
                }
            }
        }

        [Fact]
        public void Log2()
        {
            double maxDelta = (double)(Fix64.Precision * 4);

            for (int j = 0; j < m_testCases.Length; ++j)
            {
                var b = Fix64.FromRaw(m_testCases[j]);

                if (b <= Fix64.Zero)
                {
                    Assert.Throws<ArgumentOutOfRangeException>(() => Fix64.Log2(b));
                }
                else
                {
                    var expected = Math.Log((double)b) / Math.Log(2);
                    var actual = (double)Fix64.Log2(b);
                    var delta = Math.Abs(expected - actual);

                    Assert.True(delta <= maxDelta, string.Format("Ln({0}) = expected {1} but got {2}", b, expected, actual));
                }
            }
        }

        [Fact]
        public void Ln()
        {
            double maxDelta = 0.00000001;

            for (int j = 0; j < m_testCases.Length; ++j)
            {
                var b = Fix64.FromRaw(m_testCases[j]);

                if (b <= Fix64.Zero)
                {
                    Assert.Throws<ArgumentOutOfRangeException>(() => Fix64.Ln(b));
                }
                else
                {
                    var expected = Math.Log((double)b);
                    var actual = (double)Fix64.Ln(b);
                    var delta = Math.Abs(expected - actual);

                    Assert.True(delta <= maxDelta, string.Format("Ln({0}) = expected {1} but got {2}", b, expected, actual));
                }
            }
        }

        [Fact]
        public void Pow2()
        {
            double maxDelta = 0.0000001;
            for (int i = 0; i < m_testCases.Length; ++i)
            {
                var e = Fix64.FromRaw(m_testCases[i]);

                var expected = Math.Min(Math.Pow(2, (double)e), (double)Fix64.MaxValue);
                var actual = (double)Fix64.Pow2(e);
                var delta = Math.Abs(expected - actual);

                Assert.True(delta <= maxDelta, string.Format("Pow2({0}) = expected {1} but got {2}", e, expected, actual));
            }
        }

        [Fact]
        public void Pow()
        {
            for (int i = 0; i < m_testCases.Length; ++i)
            {
                var b = Fix64.FromRaw(m_testCases[i]);

                for (int j = 0; j < m_testCases.Length; ++j)
                {
                    var e = Fix64.FromRaw(m_testCases[j]);

                    if (b == Fix64.Zero && e < Fix64.Zero)
                    {
                        Assert.Throws<DivideByZeroException>(() => Fix64.Pow(b, e));
                    }
                    else if (b < Fix64.Zero && e != Fix64.Zero)
                    {
                        Assert.Throws<ArgumentOutOfRangeException>(() => Fix64.Pow(b, e));
                    }
                    else
                    {
                        var expected = e == Fix64.Zero ? 1 : b == Fix64.Zero ? 0 : Math.Min(Math.Pow((double)b, (double)e), (double)Fix64.MaxValue);

                        // Absolute precision deteriorates with large result values, take this into account
                        // Similarly, large exponents reduce precision, even if result is small.
                        double maxDelta = Math.Abs((double)e) > 100000000 ? 0.5 : expected > 100000000 ? 10 : expected > 1000 ? 0.5 : 0.00001;

                        var actual = (double)Fix64.Pow(b, e);
                        var delta = Math.Abs(expected - actual);

                        Assert.True(delta <= maxDelta, string.Format("Pow({0}, {1}) = expected {2} but got {3}", b, e, expected, actual));
                    }
                }
            }
        }

        [Fact]
        public void Modulus()
        {
            var deltas = new List<decimal>();
            foreach (var operand1 in m_testCases)
            {
                foreach (var operand2 in m_testCases)
                {
                    var f1 = Fix64.FromRaw(operand1);
                    var f2 = Fix64.FromRaw(operand2);

                    if (operand2 == 0)
                    {
                        Assert.Throws<DivideByZeroException>(() => Ignore(f1 / f2));
                    }
                    else
                    {
                        var d1 = (decimal)f1;
                        var d2 = (decimal)f2;
                        var actual = (decimal)(f1 % f2);
                        var expected = d1 % d2;
                        var delta = Math.Abs(expected - actual);
                        deltas.Add(delta);
                        Assert.True(delta <= 60 * Fix64.Precision, string.Format("{0} % {1} = expected {2} but got {3}", f1, f2, expected, actual));
                    }
                }
            }
            Console.WriteLine("Max error: {0} ({1} times precision)", deltas.Max(), deltas.Max() / Fix64.Precision);
            Console.WriteLine("Average precision: {0} ({1} times precision)", deltas.Average(), deltas.Average() / Fix64.Precision);
            Console.WriteLine("failed: {0}%", deltas.Count(d => d > Fix64.Precision) * 100.0 / deltas.Count);
        }

        //[Fact]
        //public void SinBenchmark()
        //{
        //    var deltas = new List<double>();

        //    var swf = new Stopwatch();
        //    var swd = new Stopwatch();

        //    // Restricting the range to from 0 to Pi/2
        //    for (var angle = 0.0; angle <= 2 * Math.PI; angle += 0.000004)
        //    {
        //        var f = (Fix64)angle;
        //        swf.Start();
        //        var actualF = Fix64.Sin(f);
        //        swf.Stop();
        //        var actual = (double)actualF;
        //        swd.Start();
        //        var expectedD = Math.Sin(angle);
        //        swd.Stop();
        //        var expected = (double)expectedD;
        //        var delta = Math.Abs(expected - actual);
        //        deltas.Add(delta);
        //    }
        //    Console.WriteLine("Max error: {0} ({1} times precision)", deltas.Max(), deltas.Max() / (double)Fix64.Precision);
        //    Console.WriteLine("Average precision: {0} ({1} times precision)", deltas.Average(), deltas.Average() / (double)Fix64.Precision);
        //    Console.WriteLine("Fix64.Sin time = {0}ms, Math.Sin time = {1}ms", swf.ElapsedMilliseconds, swd.ElapsedMilliseconds);
        //}

        [Fact]
        public void Sin()
        {
            Assert.True(Fix64.Sin(Fix64.Zero) == Fix64.Zero);

            Assert.True(Fix64.Sin(Fix64.PiOver2) == Fix64.One);
            Assert.True(Fix64.Sin(Fix64.Pi) == Fix64.Zero);
            Assert.True(Fix64.Sin(Fix64.Pi + Fix64.PiOver2) == -Fix64.One);
            Assert.True(Fix64.Sin(Fix64.PiTimes2) == Fix64.Zero);

            Assert.True(Fix64.Sin(-Fix64.PiOver2) == -Fix64.One);
            Assert.True(Fix64.Sin(-Fix64.Pi) == Fix64.Zero);
            Assert.True(Fix64.Sin(-Fix64.Pi - Fix64.PiOver2) == Fix64.One);
            Assert.True(Fix64.Sin(-Fix64.PiTimes2) == Fix64.Zero);


            for (double angle = -2 * Math.PI; angle <= 2 * Math.PI; angle += 0.0001)
            {
                var f = (Fix64)angle;
                var actualF = Fix64.Sin(f);
                var expected = (decimal)Math.Sin(angle);
                var delta = Math.Abs(expected - (decimal)actualF);
                Assert.True(delta <= 3 * Fix64.Precision, string.Format("Sin({0}): expected {1} but got {2}", angle, expected, actualF));
            }

            var deltas = new List<decimal>();
            foreach (var val in m_testCases)
            {
                var f = Fix64.FromRaw(val);
                var actualF = Fix64.Sin(f);
                var expected = (decimal)Math.Sin((double)f);
                var delta = Math.Abs(expected - (decimal)actualF);
                Assert.True(delta <= 0.0000001M, string.Format("Sin({0}): expected {1} but got {2}", f, expected, actualF));
            }
        }

        [Fact]
        public void FastSin()
        {
            for (double angle = -2 * Math.PI; angle <= 2 * Math.PI; angle += 0.0001)
            {
                var f = (Fix64)angle;
                var actualF = Fix64.FastSin(f);
                var expected = (decimal)Math.Sin(angle);
                var delta = Math.Abs(expected - (decimal)actualF);
                Assert.True(delta <= 50000 * Fix64.Precision, string.Format("Sin({0}): expected {1} but got {2}", angle, expected, actualF));
            }

            foreach (var val in m_testCases)
            {
                var f = Fix64.FromRaw(val);
                var actualF = Fix64.FastSin(f);
                var expected = (decimal)Math.Sin((double)f);
                var delta = Math.Abs(expected - (decimal)actualF);
                Assert.True(delta <= 0.01M, string.Format("Sin({0}): expected {1} but got {2}", f, expected, actualF));
            }
        }

        [Fact]
        public void Acos()
        {
            var maxDelta = 0.00000001m;
            var deltas = new List<decimal>();

            Assert.Equal(Fix64.Zero, Fix64.Acos(Fix64.One));
            Assert.Equal(Fix64.PiOver2, Fix64.Acos(Fix64.Zero));
            Assert.Equal(Fix64.Pi, Fix64.Acos(-Fix64.One));

            // Precision
            for (var x = -1.0; x < 1.0; x += 0.001)
            {
                var xf = (Fix64)x;
                var actual = (decimal)Fix64.Acos(xf);
                var expected = (decimal)Math.Acos((double)xf);
                var delta = Math.Abs(actual - expected);
                deltas.Add(delta);
                Assert.True(delta <= maxDelta, string.Format("Precision: Acos({0}): expected {1} but got {2}", xf, expected, actual));
            }

            for (int i = 0; i < m_testCases.Length; ++i)
            {
                var b = Fix64.FromRaw(m_testCases[i]);

                if (b < -Fix64.One || b > Fix64.One)
                {
                    Assert.Throws<ArgumentOutOfRangeException>(() => Fix64.Acos(b));
                }
                else
                {
                    var expected = (decimal)Math.Acos((double)b);
                    var actual = (decimal)Fix64.Acos(b);
                    var delta = Math.Abs(expected - actual);
                    deltas.Add(delta);
                    Assert.True(delta <= maxDelta, string.Format("Acos({0}) = expected {1} but got {2}", b, expected, actual));
                }
            }
            Console.WriteLine("Max error: {0} ({1} times precision)", deltas.Max(), deltas.Max() / Fix64.Precision);
            Console.WriteLine("Average precision: {0} ({1} times precision)", deltas.Average(), deltas.Average() / Fix64.Precision);
        }

        [Fact]
        public void Cos()
        {
            Assert.True(Fix64.Cos(Fix64.Zero) == Fix64.One);

            Assert.True(Fix64.Cos(Fix64.PiOver2) == Fix64.Zero);
            Assert.True(Fix64.Cos(Fix64.Pi) == -Fix64.One);
            Assert.True(Fix64.Cos(Fix64.Pi + Fix64.PiOver2) == Fix64.Zero);
            Assert.True(Fix64.Cos(Fix64.PiTimes2) == Fix64.One);

            Assert.True(Fix64.Cos(-Fix64.PiOver2) == -Fix64.Zero);
            Assert.True(Fix64.Cos(-Fix64.Pi) == -Fix64.One);
            Assert.True(Fix64.Cos(-Fix64.Pi - Fix64.PiOver2) == Fix64.Zero);
            Assert.True(Fix64.Cos(-Fix64.PiTimes2) == Fix64.One);


            for (double angle = -2 * Math.PI; angle <= 2 * Math.PI; angle += 0.0001)
            {
                var f = (Fix64)angle;
                var actualF = Fix64.Cos(f);
                var expected = (decimal)Math.Cos(angle);
                var delta = Math.Abs(expected - (decimal)actualF);
                Assert.True(delta <= 3 * Fix64.Precision, string.Format("Cos({0}): expected {1} but got {2}", angle, expected, actualF));
            }

            foreach (var val in m_testCases)
            {
                var f = Fix64.FromRaw(val);
                var actualF = Fix64.Cos(f);
                var expected = (decimal)Math.Cos((double)f);
                var delta = Math.Abs(expected - (decimal)actualF);
                Assert.True(delta <= 0.0000001M, string.Format("Cos({0}): expected {1} but got {2}", f, expected, actualF));
            }
        }

        [Fact]
        public void FastCos()
        {
            for (double angle = -2 * Math.PI; angle <= 2 * Math.PI; angle += 0.0001)
            {
                var f = (Fix64)angle;
                var actualF = Fix64.FastCos(f);
                var expected = (decimal)Math.Cos(angle);
                var delta = Math.Abs(expected - (decimal)actualF);
                Assert.True(delta <= 50000 * Fix64.Precision, string.Format("Cos({0}): expected {1} but got {2}", angle, expected, actualF));
            }

            foreach (var val in m_testCases)
            {
                var f = Fix64.FromRaw(val);
                var actualF = Fix64.FastCos(f);
                var expected = (decimal)Math.Cos((double)f);
                var delta = Math.Abs(expected - (decimal)actualF);
                Assert.True(delta <= 0.01M, string.Format("Cos({0}): expected {1} but got {2}", f, expected, actualF));
            }
        }

        [Fact]
        public void Tan()
        {
            Assert.True(Fix64.Tan(Fix64.Zero) == Fix64.Zero);
            Assert.True(Fix64.Tan(Fix64.Pi) == Fix64.Zero);
            Assert.True(Fix64.Tan(-Fix64.Pi) == Fix64.Zero);

            Assert.True(Fix64.Tan(Fix64.PiOver2 - (Fix64)0.001) > Fix64.Zero);
            Assert.True(Fix64.Tan(Fix64.PiOver2 + (Fix64)0.001) < Fix64.Zero);
            Assert.True(Fix64.Tan(-Fix64.PiOver2 - (Fix64)0.001) > Fix64.Zero);
            Assert.True(Fix64.Tan(-Fix64.PiOver2 + (Fix64)0.001) < Fix64.Zero);

            for (double angle = 0;/*-2 * Math.PI;*/ angle <= 2 * Math.PI; angle += 0.0001)
            {
                var f = (Fix64)angle;
                var actualF = Fix64.Tan(f);
                var expected = (decimal)Math.Tan(angle);
                Assert.Equal(actualF > Fix64.Zero, expected > 0);
                //TODO figure out a real way to test this function
            }

            //foreach (var val in m_testCases) {
            //    var f = (Fix64)val;
            //    var actualF = Fix64.Tan(f);
            //    var expected = (decimal)Math.Tan((double)f);
            //    var delta = Math.Abs(expected - (decimal)actualF);
            //    Assert.True(delta <= 0.01, string.Format("Tan({0}): expected {1} but got {2}", f, expected, actualF));
            //}
        }

        [Fact]
        public void Atan()
        {
            var maxDelta = 0.00000001m;
            var deltas = new List<decimal>();

            Assert.Equal(Fix64.Zero, Fix64.Atan(Fix64.Zero));

            // Precision
            for (var x = -1.0; x < 1.0; x += 0.0001)
            {
                var xf = (Fix64)x;
                var actual = (decimal)Fix64.Atan(xf);
                var expected = (decimal)Math.Atan((double)xf);
                var delta = Math.Abs(actual - expected);
                deltas.Add(delta);
                Assert.True(delta <= maxDelta, string.Format("Precision: Atan({0}): expected {1} but got {2}", xf, expected, actual));
            }

            // Scalability and edge cases
            foreach (var x in m_testCases)
            {
                var xf = (Fix64)x;
                var actual = (decimal)Fix64.Atan(xf);
                var expected = (decimal)Math.Atan((double)xf);
                var delta = Math.Abs(actual - expected);
                deltas.Add(delta);
                Assert.True(delta <= maxDelta, string.Format("Scalability: Atan({0}): expected {1} but got {2}", xf, expected, actual));
            }
            Console.WriteLine("Max error: {0} ({1} times precision)", deltas.Max(), deltas.Max() / Fix64.Precision);
            Console.WriteLine("Average precision: {0} ({1} times precision)", deltas.Average(), deltas.Average() / Fix64.Precision);
        }
        //[Fact]
        public void AtanBenchmark()
        {
            var deltas = new List<decimal>();

            var swf = new Stopwatch();
            var swd = new Stopwatch();

            for (var x = -1.0; x < 1.0; x += 0.001)
            {
                for (int k = 0; k < 1000; ++k)
                {
                    var xf = (Fix64)x;
                    swf.Start();
                    var actualF = Fix64.Atan(xf);
                    swf.Stop();
                    swd.Start();
                    var expected = Math.Atan((double)xf);
                    swd.Stop();
                    deltas.Add(Math.Abs((decimal)actualF - (decimal)expected));
                }
            }
            Console.WriteLine("Max error: {0} ({1} times precision)", deltas.Max(), deltas.Max() / Fix64.Precision);
            Console.WriteLine("Average precision: {0} ({1} times precision)", deltas.Average(), deltas.Average() / Fix64.Precision);
            Console.WriteLine("Fix64.Atan time = {0}ms, Math.Atan time = {1}ms", swf.ElapsedMilliseconds, swd.ElapsedMilliseconds);
        }

        [Fact]
        public void Atan2()
        {
            var deltas = new List<decimal>();
            // Identities
            Assert.Equal(Fix64.Atan2(Fix64.Zero, -Fix64.One), Fix64.Pi);
            Assert.Equal(Fix64.Atan2(Fix64.Zero, Fix64.Zero), Fix64.Zero);
            Assert.Equal(Fix64.Atan2(Fix64.Zero, Fix64.One), Fix64.Zero);
            Assert.Equal(Fix64.Atan2(Fix64.One, Fix64.Zero), Fix64.PiOver2);
            Assert.Equal(Fix64.Atan2(-Fix64.One, Fix64.Zero), -Fix64.PiOver2);

            // Precision
            for (var y = -1.0; y < 1.0; y += 0.01)
            {
                for (var x = -1.0; x < 1.0; x += 0.01)
                {
                    var yf = (Fix64)y;
                    var xf = (Fix64)x;
                    var actual = Fix64.Atan2(yf, xf);
                    var expected = (decimal)Math.Atan2((double)yf, (double)xf);
                    var delta = Math.Abs((decimal)actual - expected);
                    deltas.Add(delta);
                    Assert.True(delta <= 0.005M, string.Format("Precision: Atan2({0}, {1}): expected {2} but got {3}", yf, xf, expected, actual));
                }
            }

            // Scalability and edge cases
            foreach (var y in m_testCases)
            {
                foreach (var x in m_testCases)
                {
                    var yf = (Fix64)y;
                    var xf = (Fix64)x;
                    var actual = (decimal)Fix64.Atan2(yf, xf);
                    var expected = (decimal)Math.Atan2((double)yf, (double)xf);
                    var delta = Math.Abs(actual - expected);
                    deltas.Add(delta);
                    Assert.True(delta <= 0.005M, string.Format("Scalability: Atan2({0}, {1}): expected {2} but got {3}", yf, xf, expected, actual));
                }
            }
            Console.WriteLine("Max error: {0} ({1} times precision)", deltas.Max(), deltas.Max() / Fix64.Precision);
            Console.WriteLine("Average precision: {0} ({1} times precision)", deltas.Average(), deltas.Average() / Fix64.Precision);
        }


        //[Fact]
        public void Atan2Benchmark()
        {
            var deltas = new List<decimal>();

            var swf = new Stopwatch();
            var swd = new Stopwatch();

            foreach (var y in m_testCases)
            {
                foreach (var x in m_testCases)
                {
                    for (int k = 0; k < 1000; ++k)
                    {
                        var yf = (Fix64)y;
                        var xf = (Fix64)x;
                        swf.Start();
                        var actualF = Fix64.Atan2(yf, xf);
                        swf.Stop();
                        swd.Start();
                        var expected = Math.Atan2((double)yf, (double)xf);
                        swd.Stop();
                        deltas.Add(Math.Abs((decimal)actualF - (decimal)expected));
                    }
                }
            }
            Console.WriteLine("Max error: {0} ({1} times precision)", deltas.Max(), deltas.Max() / Fix64.Precision);
            Console.WriteLine("Average precision: {0} ({1} times precision)", deltas.Average(), deltas.Average() / Fix64.Precision);
            Console.WriteLine("Fix64.Atan2 time = {0}ms, Math.Atan2 time = {1}ms", swf.ElapsedMilliseconds, swd.ElapsedMilliseconds);
        }

        [Fact]
        public void Negation()
        {
            foreach (var operand1 in m_testCases)
            {
                var f = Fix64.FromRaw(operand1);
                if (f == Fix64.MinValue)
                {
                    Assert.Equal(-f, Fix64.MaxValue);
                }
                else
                {
                    var expected = -((decimal)f);
                    var actual = (decimal)(-f);
                    Assert.Equal(expected, actual);
                }
            }
        }

        [Fact]
        public void EqualsTests()
        {
            foreach (var op1 in m_testCases)
            {
                foreach (var op2 in m_testCases)
                {
                    var d1 = (decimal)op1;
                    var d2 = (decimal)op2;
                    Assert.True(op1.Equals(op2) == d1.Equals(d2));
                }
            }
        }

        [Fact]
        public void EqualityAndInequalityOperators()
        {
            var sources = m_testCases.Select(Fix64.FromRaw).ToList();
            foreach (var op1 in sources)
            {
                foreach (var op2 in sources)
                {
                    var d1 = (double)op1;
                    var d2 = (double)op2;
                    Assert.True((op1 == op2) == (d1 == d2));
                    Assert.True((op1 != op2) == (d1 != d2));
                    Assert.False((op1 == op2) && (op1 != op2));
                }
            }
        }

        [Fact]
        public void CompareTo()
        {
            var nums = m_testCases.Select(Fix64.FromRaw).ToArray();
            var numsDecimal = nums.Select(t => (decimal)t).ToArray();
            Array.Sort(nums);
            Array.Sort(numsDecimal);
            Assert.True(nums.Select(t => (decimal)t).SequenceEqual(numsDecimal));
        }
    }
}
