This library implements "Fix64", a 64 bit fixed point 31.32 numeric type and transcendent operations on it (square root, trig, etc). It is well covered by unit tests. However, it is still missing some operations; in particular, Tangent is not well tested yet.

The solution includes 3 different types but the most developed and tested one is Fix64.
Fix8 and Fix16 were mainly used for experimentation. This project started as a port of libfixmath (http://code.google.com/p/libfixmath/).

Note that the type requires explicit casts to convert to floating point and this is intentional, the difference between fixed point and floating point math is as important as the one between floating point and integral math.
