This is a C# port of the excellent libfixmath library, which can currently be found at http://code.google.com/p/libfixmath/

It implements a 64 bit fixed point 32.32 numeric type and transcendent operations on it (square root, trig, etc).

It is covered by unit tests, most of which are also ported from libfixmath.

The solution includes 3 different types but the most developed and tested one is Fix64.
Fix8 and Fix16 were mainly used for experimentation.

Note that the type requires explicit casts to convert to floating point and this is intentional, the difference between fixed point and floating point math is as important as the one between floating point and integral math.
