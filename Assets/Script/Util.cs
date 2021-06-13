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

static class ComputeShaderExtensions
{
    public static void SetInts
      (this ComputeShader compute, string name, Vector3Int v)
      => compute.SetInts(name, v.x, v.y, v.z);

    public static void DispatchThreads
      (this ComputeShader compute, int kernel, int x, int y, int z)
    {
        uint xc, yc, zc;
        compute.GetKernelThreadGroupSizes(kernel, out xc, out yc, out zc);

        x = (x + (int)xc - 1) / (int)xc;
        y = (y + (int)yc - 1) / (int)yc;
        z = (z + (int)zc - 1) / (int)zc;

        compute.Dispatch(kernel, x, y, z);
    }

    public static void DispatchThreads
      (this ComputeShader compute, int kernel, Vector3Int v)
      => DispatchThreads(compute, kernel, v.x, v.y, v.z);
}

} // namespace MarchingCube
