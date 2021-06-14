using UnityEngine;

namespace MarchingCubes {

sealed class VolumeDataVisualizer : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] TextAsset _volumeData = null;
    [SerializeField] Vector3Int _dimensions = new Vector3Int(64, 32, 64);
    [SerializeField] float _gridScale = 4.0f / 64;
    [SerializeField] int _triangleBudget = 65536;
    [SerializeField] float _targetValue = 0;

    #endregion

    #region Project asset references

    [SerializeField, HideInInspector] ComputeShader _converterCompute = null;
    [SerializeField, HideInInspector] ComputeShader _builderCompute = null;

    #endregion

    #region Volume data conversion

    void ConvertVolumeData()
    {
        using var readBuffer = new ComputeBuffer(256 * 256 * 113 / 2, sizeof(uint));
        readBuffer.SetData(_volumeData.bytes);

        _converterCompute.SetInts("Dims", _dimensions);
        _converterCompute.SetBuffer(0, "Source", readBuffer);
        _converterCompute.SetBuffer(0, "Voxels", _voxelBuffer);
        _converterCompute.DispatchThreads(0, _dimensions);
    }

    #endregion

    #region MonoBehaviour implementation

    ComputeBuffer _voxelBuffer;
    MeshBuilder _builder;

    void Start()
    {
        var voxelCount = _dimensions.x * _dimensions.y * _dimensions.z;
        _voxelBuffer = new ComputeBuffer(voxelCount, sizeof(float));

        ConvertVolumeData();

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
      => _builder.BuildIsosurface(_voxelBuffer, _targetValue, _gridScale);

    #endregion
}

} // namespace MarchingCubes
