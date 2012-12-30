using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace FixMath.NET {

    [TestFixture]
    class Fix8Tests {

        [Test]
        public void DecimalToFix8AndBack() {
            var sources = new[] { -8m, -7.99m, -7.98m, -7.97m, -7.96m, -7.95m, -0.0m, 0.0m, 7.87m, 7.88m, 7.89m, 7.90m, 7.91m, 7.92m, 7.93m, 7.94m, 7.95m };
            var expecteds = new[] { -8m, -8m, -8m, -8m, -7.9375m, -7.9375m, 0.0m, 0.0m, 7.875m, 7.875m, 7.875m, 7.875m, 7.9375m, 7.9375m, 7.9375m, 7.9375m, 7.9375m };
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
            Assert.AreEqual(0, failed);
        }

        [Test]
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
                    Assert.AreEqual(expected, actualM);
                }
            }
            //}
            //Console.WriteLine("Fix8: {0} total, {1} per addition", sw.ElapsedMilliseconds, sw.Elapsed.Milliseconds / (100 * 65536.0));
        }

        [Test]
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
                    Assert.AreEqual(expected, actualM);
                }
            }
            //}
            //Console.WriteLine("Fix8: {0} total, {1} per substraction", sw.ElapsedMilliseconds, sw.Elapsed.Milliseconds / (100 * 65536.0));
        }

        [Test]
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
                    //Assert.AreEqual(expected, actual);
                }
            }
            //}
            //Console.WriteLine("Fix8: {0} total, {1} per multiplication", sw.ElapsedMilliseconds, sw.Elapsed.Milliseconds / (100*65536.0));
            //Console.WriteLine("Double: {0} total, {1} per multiplication", swd.ElapsedMilliseconds, swd.Elapsed.Milliseconds / (100*65536.0));
            //Console.WriteLine("Float: {0} total, {1} per multiplication", swf.ElapsedMilliseconds, swf.Elapsed.Milliseconds / (100*65536.0));
            Assert.AreEqual(0, failed);
        }

        static void Ignore<T>(T value) {}

        [Test]
        public void Division() {
            int failed = 0;
            var sw = new Stopwatch();
            var swf = new Stopwatch();
            var swd = new Stopwatch();
            for (int k = 0; k < 100; ++k) {
                for (int i = sbyte.MinValue; i <= sbyte.MaxValue; ++i) {
                    var op1 = Fix8.FromRaw((sbyte)i);
                    for (int j = sbyte.MinValue; j <= sbyte.MaxValue; ++j) {
                        var op2 = Fix8.FromRaw((sbyte)j);
                        if (j == 0) {
                            Assert.Throws<DivideByZeroException>(() => Ignore(op1 / op2));
                        }
                        else {
                            var op1d = (double)(decimal)op1;
                            var op2d = (double)(decimal)op2;
                            swd.Start();
                            var resultd = op1d / op2d;
                            swd.Stop();

                            var op1f = (float)(decimal)op1;
                            var op2f = (float)(decimal)op2;
                            swf.Start();
                            var resultf = op1f / op2f;
                            swf.Stop();

                            sw.Start();
                            var actualF = op1 / op2;
                            sw.Stop();
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


                            if (double.IsNaN(resultd) || float.IsNaN(resultf)) {
                                Console.WriteLine("");
                            }
                        }
                    }
                }

            }
            Console.WriteLine("Fix8: {0} total, {1} per division", sw.ElapsedMilliseconds, sw.Elapsed.Milliseconds / (100 * 65536.0));
            Console.WriteLine("Double: {0} total, {1} per division", swd.ElapsedMilliseconds, swd.Elapsed.Milliseconds / (100 * 65536.0));
            Console.WriteLine("Float: {0} total, {1} per division", swf.ElapsedMilliseconds, swf.Elapsed.Milliseconds / (100 * 65536.0));
            Assert.AreEqual(0, failed);
        }

        [Test]
        public void Abs() {
            Assert.Throws<OverflowException>(() => Fix8.Abs(Fix8.MinValue));
            for (int i = sbyte.MinValue + 1; i <= sbyte.MaxValue; ++i) {
                var f = Fix8.FromRaw((sbyte)i);
                var original = (decimal)f;
                var expected = Math.Abs(original);
                var actual = (decimal)(Fix8.Abs(f));
                Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void Sign() {
            for (int i = sbyte.MinValue; i <= sbyte.MaxValue; ++i) {
                var f = Fix8.FromRaw((sbyte)i);
                var expected = Math.Sign((decimal)f);
                var actual = Fix8.Sign(f);
                Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void Floor() {
            for (int i = sbyte.MinValue; i <= sbyte.MaxValue; ++i) {
                var f = Fix8.FromRaw((sbyte)i);
                var expected = Math.Floor((decimal)f);
                var actual = (decimal)Fix8.Floor(f);
                Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void Ceiling() {
            for (int i = sbyte.MinValue; i <= sbyte.MaxValue; ++i) {
                var f = Fix8.FromRaw((sbyte)i);
                var expected = Math.Ceiling((decimal)f);
                expected =
                    expected > (decimal)Fix8.MaxValue ? (decimal)Fix8.MaxValue :
                    expected < (decimal)Fix8.MinValue ? (decimal)Fix8.MinValue
                             : expected;
                var actual = (decimal)Fix8.Ceiling(f);
                Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void Round() {
            for (int i = sbyte.MinValue; i <= sbyte.MaxValue; ++i) {
                var f = Fix8.FromRaw((sbyte)i);
                var expected = Math.Round((decimal)f);
                expected =
                    expected > (decimal)Fix8.MaxValue ? (decimal)Fix8.MaxValue :
                    expected < (decimal)Fix8.MinValue ? (decimal)Fix8.MinValue
                             : expected;
                var actual = (decimal)Fix8.Round(f);
                Assert.AreEqual(expected, actual);
            }
        }
    }
}
