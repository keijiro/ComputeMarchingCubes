using UnityEngine;
using UnityEngine.Rendering;

namespace MarchingCube {

sealed class Isosurface : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] Vector3Int _dimensions = new Vector3Int(32, 32, 32);
    [SerializeField] float _gridScale = 1.0f / 32;
    [SerializeField] int _triangleBudget = 65536;
    [SerializeField] float _targetValue = 0;

    #endregion

    #region Project asset references

    [SerializeField, HideInInspector] ComputeShader _volumeGenerator = null;
    [SerializeField, HideInInspector] ComputeShader _meshConstructor = null;

    #endregion

    #region Mesh objects

    (Mesh mesh, GraphicsBuffer vertices, GraphicsBuffer indices) _surface;

    void AllocateMesh(int vertexCount)
    {
        var mesh = new Mesh();

        // We want GraphicsBuffer access as Raw (ByteAddress) buffers.
        mesh.indexBufferTarget |= GraphicsBuffer.Target.Raw;
        mesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;

        // Vertex position: float32 x 3
        var vp = new VertexAttributeDescriptor
          (VertexAttribute.Position, VertexAttributeFormat.Float32, 3);

        // Vertex normal: float32 x 3
        var vn = new VertexAttributeDescriptor
          (VertexAttribute.Normal, VertexAttributeFormat.Float32, 3);

        // Vertex/index buffer formats
        mesh.SetVertexBufferParams(vertexCount, vp, vn);
        mesh.SetIndexBufferParams(vertexCount, IndexFormat.UInt32);

        // Submesh initialization
        mesh.SetSubMesh(0, new SubMeshDescriptor(0, vertexCount),
                        MeshUpdateFlags.DontRecalculateBounds);

        // Big bounds to avoid getting culled
        mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1000);

        // GraphicsBuffer references
        _surface = (mesh, mesh.GetVertexBuffer(0), mesh.GetIndexBuffer());
    }

    void ReleaseMesh()
    {
        _surface.vertices.Dispose();
        _surface.indices.Dispose();
        Destroy(_surface.mesh);
    }

    #endregion

    #region Compute objects

    (ComputeBuffer table, ComputeBuffer voxel, ComputeBuffer count) _buffer;

    void AllocateComputeBuffers()
    {
        _buffer.table = new ComputeBuffer(256, sizeof(ulong));
        _buffer.table.SetData(PrecalculatedData.TriangleTable);

        var voxels = _dimensions.x * _dimensions.y * _dimensions.z;
        _buffer.voxel = new ComputeBuffer(voxels, sizeof(float));

        _buffer.count = new ComputeBuffer(1, 4, ComputeBufferType.Counter);
    }

    void ReleaseComputeBuffers()
    {
        _buffer.table.Dispose();
        _buffer.voxel.Dispose();
        _buffer.count.Dispose();
    }

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        AllocateMesh(3 * _triangleBudget);
        AllocateComputeBuffers();
        GetComponent<MeshFilter>().sharedMesh = _surface.mesh;
    }

    void OnDestroy()
    {
        ReleaseMesh();
        ReleaseComputeBuffers();
    }

    void Update()
    {
        _volumeGenerator.SetInts("Dims", _dimensions);
        _volumeGenerator.SetFloat("Scale", _gridScale);
        _volumeGenerator.SetFloat("Time", Time.time);
        _volumeGenerator.SetBuffer(0, "Voxels", _buffer.voxel);
        _volumeGenerator.DispatchThreads(0, _dimensions);

        _buffer.count.SetCounterValue(0);

        _meshConstructor.SetInts("Dims", _dimensions);
        _meshConstructor.SetInt("MaxTriangle", _triangleBudget);
        _meshConstructor.SetFloat("Scale", _gridScale);
        _meshConstructor.SetFloat("IsoValue", _targetValue);
        _meshConstructor.SetBuffer(0, "TriangleTable", _buffer.table);
        _meshConstructor.SetBuffer(0, "Voxels", _buffer.voxel);
        _meshConstructor.SetBuffer(0, "VertexBuffer", _surface.vertices);
        _meshConstructor.SetBuffer(0, "IndexBuffer", _surface.indices);
        _meshConstructor.SetBuffer(0, "Counter", _buffer.count);
        _meshConstructor.DispatchThreads(0, _dimensions);

        _meshConstructor.SetBuffer(1, "VertexBuffer", _surface.vertices);
        _meshConstructor.SetBuffer(1, "IndexBuffer", _surface.indices);
        _meshConstructor.SetBuffer(1, "Counter", _buffer.count);
        _meshConstructor.DispatchThreads(1, _triangleBudget, 1, 1);
    }

    #endregion
}

} // namespace MarchingCube
