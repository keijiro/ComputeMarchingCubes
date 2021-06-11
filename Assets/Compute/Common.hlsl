#ifndef _MARCHINGCUBES_COMMON_H_
#define _MARCHINGCUBES_COMMON_H_

struct Point { float3 p, n; };

Point MakePoint(float3 p, float3 n)
{
    Point temp;
    temp.p = p;
    temp.n = n;
    return temp;
}

struct Triangle { Point v1, v2, v3; };

Triangle MakeTriangle(Point v1, Point v2, Point v3)
{
    Triangle temp;
    temp.v1 = v1;
    temp.v2 = v2;
    temp.v3 = v3;
    return temp;
}

#endif
