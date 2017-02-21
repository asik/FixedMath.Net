This library implements "Fix64", a 64 bit fixed point 31.32 numeric type and transcendent operations on it (square root, trig, etc). It is well covered by unit tests. However, it is still missing some operations; in particular, Tangent is not well tested yet.

In the unit tests you'll find implementations for Int16-based (Q7.8) and Byte-based (Q3.4) numeric types. These were used for exploration of boundary conditions etc., but I'm keeping the code there only for reference.

This project started as a port of libfixmath (http://code.google.com/p/libfixmath/).

Note that the type requires explicit casts to convert to floating point and this is intentional, the difference between fixed point and floating point math is as important as the one between floating point and integral math.