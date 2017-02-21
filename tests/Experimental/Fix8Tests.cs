using System;
using System.Diagnostics;
using Xunit;

namespace FixMath.NET
{

    class Fix8Tests {

        [Fact]
        public void DecimalToFix8AndBack() {
            var sources = new[] {   -8m, -7.99m, -7.98m, -7.97m, -7.96m,   -7.95m,  -0.0m, 0.0m, 7.87m,  7.88m,  7.89m,  7.90m,  7.91m,  7.92m,    7.93m, 7.94m,     7.95m };
            var expecteds = new[] { -8m, -8m,    -8m,    -8m,    -7.9375m, -7.9375m, 0.0m, 0.0m, 7.875m, 7.875m, 7.875m, 7.875m, 7.9375m, 7.9375m, 7.9375m, 7.9375m, 7.9375m };
            int failed = 0;
            for (var i = 0; i < sources.Length; ++i) {
                var expected = expecteds[i];
                var f = (Fix8)sources[i];
                var actual = (decimal)f;
                if (expected != actual) {
                    Console.WriteLine("Failed conversion from decimal and back: expected {0} but got {1}", expected, actual);
                    ++failed;
                }
            }
            Assert.Equal(0, failed);
        }

        [Fact]
        public void Addition() {
            // Testing every possible combination of terms
            // now that's what I call coverage
            //var sw = new Stopwatch();
            //for (var k = 0; k < 100; ++k ) {
            for (int i = sbyte.MinValue; i <= sbyte.MaxValue; ++i) {
                for (int j = sbyte.MinValue; j <= sbyte.MaxValue; ++j) {
                    var op1 = Fix8.FromRaw((sbyte)i);
                    var op2 = Fix8.FromRaw((sbyte)j);
                    var expected = (decimal)op1 + (decimal)op2;
                    expected =
                       expected > (decimal)Fix8.MaxValue ? (decimal)Fix8.MaxValue :
                       expected < (decimal)Fix8.MinValue ? (decimal)Fix8.MinValue :
                       expected;
                    //sw.Start();
                    var actual = op1 + op2;
                    //sw.Stop();
                    var actualM = (decimal)actual;
                    if (expected != actualM) {
                        Console.WriteLine("Failed for {0} + {1}: expected {2} but got {3}", op1, op2, expected, actual);
                    }
                    Assert.Equal(expected, actualM);
                }
            }
            //}
            //Console.WriteLine("Fix8: {0} total, {1} per addition", sw.ElapsedMilliseconds, sw.Elapsed.Milliseconds / (100 * 65536.0));
        }

        [Fact]
        public void Subtraction() {
            // Testing every possible combination of terms
            // now that's what I call coverage
            //var sw = new Stopwatch();
            //for (var k = 0; k < 100; ++k) {
            for (int i = sbyte.MinValue; i <= sbyte.MaxValue; ++i) {
                for (int j = sbyte.MinValue; j <= sbyte.MaxValue; ++j) {
                    var op1 = Fix8.FromRaw((sbyte)i);
                    var op2 = Fix8.FromRaw((sbyte)j);
                    var expected = (decimal)op1 - (decimal)op2;
                    expected =
                        expected > (decimal)Fix8.MaxValue
                            ? (decimal)Fix8.MaxValue
                            : expected < (decimal)Fix8.MinValue
                                  ? (decimal)Fix8.MinValue
                                  : expected;
                    //sw.Start();
                    var actual = op1 - op2;
                    //sw.Stop();
                    var actualM = (decimal)actual;
                    if (expected != actualM) {
                        Console.WriteLine("Failed for {0} - {1}: expected {2} but got {3}", op1, op2, expected, actual);
                    }
                    Assert.Equal(expected, actualM);
                }
            }
            //}
            //Console.WriteLine("Fix8: {0} total, {1} per substraction", sw.ElapsedMilliseconds, sw.Elapsed.Milliseconds / (100 * 65536.0));
        }

        [Fact]
        public void Multiplication() {
            int failed = 0;
            var sw = new Stopwatch();
            var swf = new Stopwatch();
            var swd = new Stopwatch();
            //for (int k = 0; k < 100; ++k) {
            for (int i = sbyte.MinValue; i <= sbyte.MaxValue; ++i) {
                var op1 = Fix8.FromRaw((sbyte)i);

                for (int j = sbyte.MinValue; j <= sbyte.MaxValue; ++j) {

                    var op2 = Fix8.FromRaw((sbyte)j);

                    var op1d = (double)(decimal)op1;
                    var op2d = (double)(decimal)op2;
                    swd.Start();
                    var resultd = op1d * op2d;
                    swd.Stop();

                    var op1f = (float)(decimal)op1;
                    var op2f = (float)(decimal)op2;
                    swf.Start();
                    var resultf = op1f * op2f;
                    swf.Stop();

                    sw.Start();
                    var actualF = op1 * op2;
                    sw.Stop();
                    var actual = (decimal)(actualF);
                    var expected = (decimal)op1 * (decimal)op2;
                    expected =
                        expected > (decimal)Fix8.MaxValue
                            ? (decimal)Fix8.MaxValue
                            : expected < (decimal)Fix8.MinValue
                                  ? (decimal)Fix8.MinValue
                                  : expected;
                    var expectedRounded = Math.Round(expected * 16m) * 0.0625m;
                    var delta = Math.Abs(expected - actual);

                    // Fix8 correctly rounds within half of its precision, but doesn't use the banker's algorithm
                    // like Math.Round(). 
                    if (delta > (0.0625m / 2.0m)) {
                        Console.WriteLine("Failed {0} * {1} : expected {2} but got {3}", op1, op2, expectedRounded, actual);
                        ++failed;
                    }

                    if (double.IsNaN(resultd) || float.IsNaN(resultf)) {
                        Console.WriteLine("");
                    }
                    //Assert.Equal(expected, actual);
                }
            }
            //}
            //Console.WriteLine("Fix8: {0} total, {1} per multiplication", sw.ElapsedMilliseconds, sw.Elapsed.Milliseconds / (100*65536.0));
            //Console.WriteLine("Double: {0} total, {1} per multiplication", swd.ElapsedMilliseconds, swd.Elapsed.Milliseconds / (100*65536.0));
            //Console.WriteLine("Float: {0} total, {1} per multiplication", swf.ElapsedMilliseconds, swf.Elapsed.Milliseconds / (100*65536.0));
            Assert.Equal(0, failed);
        }

        static void Ignore<T>(T value) {}

        [Fact]
        public void Division() {
            int failed = 0;
            //var sw = new Stopwatch();
            //var swf = new Stopwatch();
            //var swd = new Stopwatch();
            //for (int k = 0; k < 100; ++k) {
                for (int i = sbyte.MinValue; i <= sbyte.MaxValue; ++i) {
                    var op1 = Fix8.FromRaw((sbyte)i);
                    for (int j = sbyte.MinValue; j <= sbyte.MaxValue; ++j) {
                        var op2 = Fix8.FromRaw((sbyte)j);
                        if (j == 0) {
                            Assert.Throws<DivideByZeroException>(() => Ignore(op1 / op2));
                        }
                        else {
                            //var op1d = (double)(decimal)op1;
                            //var op2d = (double)(decimal)op2;
                            //swd.Start();
                            //var resultd = op1d / op2d;
                            //swd.Stop();

                            //var op1f = (float)(decimal)op1;
                            //var op2f = (float)(decimal)op2;
                            //swf.Start();
                            //var resultf = op1f / op2f;
                            //swf.Stop();

                            //sw.Start();
                            var actualF = op1 / op2;
                            //sw.Stop();
                            var actual = (decimal)(actualF);
                            var expected = (decimal)op1 / (decimal)op2;
                            expected =
                                expected > (decimal)Fix8.MaxValue
                                    ? (decimal)Fix8.MaxValue
                                    : expected < (decimal)Fix8.MinValue
                                          ? (decimal)Fix8.MinValue
                                          : expected;
                            var expectedRounded = Math.Round(expected * 16m) * 0.0625m;
                            var delta = Math.Abs(expected - actual);
                            if (delta > (0.0625m / 2m)) {
                                Console.WriteLine("Failed {0} / {1} : expected {2} but got {3}", op1, op2, expectedRounded, actual);
                                ++failed;
                            }

                            // This is just to prevent the optimizer from removing the double and float operations
                            //if (double.IsNaN(resultd) || float.IsNaN(resultf)) {
                            //    Console.WriteLine("");
                            //}
                        }
                    }
                }

            //}
            //Console.WriteLine("Fix8: {0} total, {1} per division", sw.ElapsedMilliseconds, sw.Elapsed.Milliseconds / (100 * 65536.0));
            //Console.WriteLine("Double: {0} total, {1} per division", swd.ElapsedMilliseconds, swd.Elapsed.Milliseconds / (100 * 65536.0));
            //Console.WriteLine("Float: {0} total, {1} per division", swf.ElapsedMilliseconds, swf.Elapsed.Milliseconds / (100 * 65536.0));
            Assert.Equal(0, failed);
        }

        [Fact]
        public void Modulus() {
            for (int i = sbyte.MinValue; i <= sbyte.MaxValue; ++i) {
                var f1 = Fix8.FromRaw((sbyte)i);
                for (int j = sbyte.MinValue; j <= sbyte.MaxValue; ++j) {
                    var f2 = Fix8.FromRaw((sbyte)j);

                    if (j == 0) {
                        Assert.Throws<DivideByZeroException>(() => Ignore(f1 % f2));
                    }
                    else {
                        var d1 = (decimal)f1;
                        var d2 = (decimal)f2;
                        var expected = d1 % d2;
                        var actual = (decimal)(f1 % f2);
                        //var delta = Math.Abs(expected - actual);
                        Assert.Equal(expected, actual);
                        //Assert.LessOrEqual(delta, Fix8.Precision, string.Format("{0} % {1} = expected {2} but got {3}", f1, f2, expected, actual));
                    }

                }
            }
        }

        [Fact]
        public void Sin() {
            for (sbyte i = sbyte.MinValue; i <= sbyte.MaxValue; ++i) {
                var f = Fix8.FromRaw(i);
                var expected = (decimal)Math.Sin((double)(decimal)f);
                var actual = (decimal)Fix8.Sin(f);
                var delta = Math.Abs(expected - actual);
                Assert.True(delta <= 0.0625m, string.Format("Source = {0}, expected = {1}, actual = {2}", f, expected, actual));
            }
        }

        [Fact]
        public void Abs() {
            Assert.Equal(Fix8.MaxValue, Fix8.Abs(Fix8.MinValue));
            for (int i = sbyte.MinValue + 1; i <= sbyte.MaxValue; ++i) {
                var f = Fix8.FromRaw((sbyte)i);
                var original = (decimal)f;
                var expected = Math.Abs(original);
                var actual = (decimal)(Fix8.Abs(f));
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Sign() {
            for (int i = sbyte.MinValue; i <= sbyte.MaxValue; ++i) {
                var f = Fix8.FromRaw((sbyte)i);
                var expected = Math.Sign((decimal)f);
                var actual = Fix8.Sign(f);
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Floor() {
            for (int i = sbyte.MinValue; i <= sbyte.MaxValue; ++i) {
                var f = Fix8.FromRaw((sbyte)i);
                var expected = Math.Floor((decimal)f);
                var actual = (decimal)Fix8.Floor(f);
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Ceiling() {
            for (int i = sbyte.MinValue; i <= sbyte.MaxValue; ++i) {
                var f = Fix8.FromRaw((sbyte)i);
                var expected = Math.Ceiling((decimal)f);
                expected =
                    expected > (decimal)Fix8.MaxValue ? (decimal)Fix8.MaxValue :
                    expected < (decimal)Fix8.MinValue ? (decimal)Fix8.MinValue
                             : expected;
                var actual = (decimal)Fix8.Ceiling(f);
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Round() {
            for (int i = sbyte.MinValue; i <= sbyte.MaxValue; ++i) {
                var f = Fix8.FromRaw((sbyte)i);
                var expected = Math.Round((decimal)f);
                expected =
                    expected > (decimal)Fix8.MaxValue ? (decimal)Fix8.MaxValue :
                    expected < (decimal)Fix8.MinValue ? (decimal)Fix8.MinValue
                             : expected;
                var actual = (decimal)Fix8.Round(f);
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Sqrt() {
            for (int i = sbyte.MinValue; i <= sbyte.MaxValue; ++i) {
                var f = Fix8.FromRaw((sbyte)i);
                if (i < 0) {
                    Assert.Throws<ArgumentOutOfRangeException>(() => Fix8.Sqrt(f));
                }
                else {
                    var expected = (decimal)Math.Sqrt((double)(decimal)f);
                    var actual = (decimal)Fix8.Sqrt(f);
                    var delta = Math.Abs(expected - actual);
                    Assert.True(delta <= 0.0625m / 2m);
                }
            }
        }
    }
}
