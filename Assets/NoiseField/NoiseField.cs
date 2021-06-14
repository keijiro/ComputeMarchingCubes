using UnityEngine;

namespace MarchingCubes {

sealed class NoiseField : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] Vector3Int _dimensions = new Vector3Int(64, 32, 64);
    [SerializeField] float _gridScale = 4.0f / 64;
    [SerializeField] int _triangleBudget = 65536;
    [SerializeField] float _targetValue = 0;

    #endregion

    #region Project asset references

    [SerializeField, HideInInspector] ComputeShader _volumeCompute = null;
    [SerializeField, HideInInspector] ComputeShader _builderCompute = null;

    #endregion

    #region MonoBehaviour implementation

    ComputeBuffer _voxelBuffer;
    MeshBuilder _builder;

    void Start()
    {
        var voxelCount = _dimensions.x * _dimensions.y * _dimensions.z;
        _voxelBuffer = new ComputeBuffer(voxelCount, sizeof(float));

        _builder = new MeshBuilder(_dimensions.x, _dimensions.y, _dimensions.z,
                                   _triangleBudget, _builderCompute);

        GetComponent<MeshFilter>().sharedMesh = _builder.Mesh;
    }

    void OnDestroy()
    {
        _voxelBuffer.Dispose();
        _builder.Dispose();
    }

    void Update()
    {
        _volumeCompute.SetInts("Dims", _dimensions);
        _volumeCompute.SetFloat("Scale", _gridScale);
        _volumeCompute.SetFloat("Time", Time.time);
        _volumeCompute.SetBuffer(0, "Voxels", _voxelBuffer);
        _volumeCompute.DispatchThreads(0, _dimensions);

        _builder.BuildIsosurface(_voxelBuffer, _targetValue, _gridScale);
    }

    #endregion
}

} // namespace MarchingCubes
