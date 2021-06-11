using UnityEngine;
using UnityEngine.Rendering;

namespace MarchingCube {

sealed class Isosurface : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] float _targetValue = 0;

    #endregion

    #region Project asset references

    [SerializeField, HideInInspector] ComputeShader _volumeGenerator = null;
    [SerializeField, HideInInspector] ComputeShader _meshConstructor = null;
    [SerializeField, HideInInspector] ComputeShader _meshConverter = null;

    #endregion

    #region Isosurface settings

    const int Size = 32;
    const int TriangleBudget = 65536;

    #endregion

    #region Mesh objects

    Mesh _mesh;
    GraphicsBuffer _vertexBuffer;
    GraphicsBuffer _indexBuffer;

    void AllocateMesh(int vertexCount)
    {
        _mesh = new Mesh();

        // We want GraphicsBuffer access as Raw (ByteAddress) buffers.
        _mesh.indexBufferTarget |= GraphicsBuffer.Target.Raw;
        _mesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;

        // Vertex position: float32 x 3
        var vp = new VertexAttributeDescriptor
          (VertexAttribute.Position, VertexAttributeFormat.Float32, 3);

        // Vertex normal: float32 x 3
        var vn = new VertexAttributeDescriptor
          (VertexAttribute.Normal, VertexAttributeFormat.Float32, 3);

        // Vertex/index buffer formats
        _mesh.SetVertexBufferParams(vertexCount, vp, vn);
        _mesh.SetIndexBufferParams(vertexCount, IndexFormat.UInt32);

        // Submesh initialization
        _mesh.SetSubMesh(0, new SubMeshDescriptor(0, vertexCount),
                         MeshUpdateFlags.DontRecalculateBounds);

        // Big bounds to avoid getting culled
        _mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1000);

        // GraphicsBuffer references
        _vertexBuffer = _mesh.GetVertexBuffer(0);
        _indexBuffer = _mesh.GetIndexBuffer();
    }

    void ReleaseMesh()
    {
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
        Destroy(_mesh);
    }

    #endregion

    #region Compute objects

    ComputeBuffer _triangleTable;
    ComputeBuffer _voxelBuffer;
    ComputeBuffer _triangleBuffer;
    ComputeBuffer _countBuffer;

    void AllocateComputeBuffers()
    {
        _triangleTable = new ComputeBuffer(256, 8);
        _triangleTable.SetData(PrecalculatedData.TriangleTable);

        _voxelBuffer = new ComputeBuffer(Size * Size * Size, 4);

        _triangleBuffer = new ComputeBuffer
          (TriangleBudget, sizeof(float) * 3 * 3, ComputeBufferType.Append);

        _countBuffer = new ComputeBuffer
          (1, sizeof(uint), ComputeBufferType.Raw);
    }

    void ReleaseComputeBuffers()
    {
        _triangleTable.Dispose();
        _voxelBuffer.Dispose();
        _triangleBuffer.Dispose();
        _countBuffer.Dispose();
    }

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        AllocateMesh(3 * TriangleBudget);
        AllocateComputeBuffers();
        GetComponent<MeshFilter>().sharedMesh = _mesh;
    }

    void OnDestroy()
    {
        ReleaseMesh();
        ReleaseComputeBuffers();
    }

    void Update()
    {
        _volumeGenerator.SetFloat("Time", Time.time);
        _volumeGenerator.SetInts("Dims", Size, Size, Size);
        _volumeGenerator.SetBuffer(0, "Voxels", _voxelBuffer);
        _volumeGenerator.Dispatch(0, Size / 8, Size / 8, Size / 8);

        _triangleTable.SetCounterValue(0);
        _meshConstructor.SetInts("Dims", Size, Size, Size);
        _meshConstructor.SetFloat("IsoValue", _targetValue);
        _meshConstructor.SetBuffer(0, "TriangleTable", _triangleTable);
        _meshConstructor.SetBuffer(0, "Voxels", _voxelBuffer);
        _meshConstructor.SetBuffer(0, "Output", _triangleBuffer);
        _meshConstructor.Dispatch(0, Size / 8, Size / 8, Size / 8);

        ComputeBuffer.CopyCount(_triangleBuffer, _countBuffer, 0);

        _meshConverter.SetBuffer(0, "Input", _triangleBuffer);
        _meshConverter.SetBuffer(0, "Count", _countBuffer);
        _meshConverter.SetBuffer(0, "VertexBuffer", _vertexBuffer);
        _meshConverter.SetBuffer(0, "IndexBuffer", _indexBuffer);
        _meshConverter.Dispatch(0, TriangleBudget / 64, 1, 1);
    }

    #endregion
}

} // namespace MarchingCube
