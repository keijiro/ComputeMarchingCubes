using UnityEngine;

namespace MarchingCube {

static class Util
{
    public static float[] GenerateDummyData(int size)
    {
        var data = new float[size * size * size];
        var i = 0;
        for (var x = 0; x < size; x++)
            for (var y = 0; y < size; y++)
                for (var z = 0; z < size; z++)
                    data[i++] = Mathf.Sin(0.2f * x) + Mathf.Sin(0.23f * y) + Mathf.Sin(0.25f * z);
        return data;
    }
}

} // namespace MarchingCube
