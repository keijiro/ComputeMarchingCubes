using UnityEngine;

namespace MarchingCubes
{

    class NoiseFieldVisualizer : MonoBehaviour
    {
        #region Editable attributes

        [SerializeField] protected Vector3Int _dimensions = new Vector3Int(64, 32, 64);
        [SerializeField] protected float _gridScale = 4.0f / 64;
        [SerializeField] protected int _triangleBudget = 65536;
        [SerializeField] protected float _targetValue = 0;

        #endregion

        #region Project asset references

        public ComputeShader _volumeCompute;
        public ComputeShader _builderCompute;

        #endregion

        #region Private members

        int VoxelCount => _dimensions.x * _dimensions.y * _dimensions.z;

        protected ComputeBuffer _voxelBuffer;
        protected MeshBuilder _builder;


        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
            _voxelBuffer = new ComputeBuffer(VoxelCount, sizeof(float));

            initBuilder();
        }

        protected virtual void initBuilder()
        {
            _builder = new MeshBuilder(_dimensions, _triangleBudget, _builderCompute);
        }




        void OnDestroy()
        {
            _voxelBuffer.Dispose();
            _builder.Dispose();
        }

        protected virtual void Update()
        {

            // Noise field update for visualization mesh
            _volumeCompute.SetInts("Dims", _dimensions);
            _volumeCompute.SetFloat("Scale", _gridScale);
            _volumeCompute.SetFloat("Time", Time.time);
            _volumeCompute.SetBuffer(0, "Voxels", _voxelBuffer);
            _volumeCompute.DispatchThreads(0, _dimensions);

            // Isosurface reconstruction for visualization mesh
            _builder.BuildIsosurface(_voxelBuffer, _targetValue, _gridScale);
            GetComponent<MeshFilter>().sharedMesh = _builder.Mesh;

        }

        #endregion
    }

} // namespace MarchingCubes