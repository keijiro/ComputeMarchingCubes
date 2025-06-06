using UnityEngine;

namespace MarchingCubes
{
    class ColliderNoiseFieldVisualizer : NoiseFieldVisualizer
    {

        public bool useAsync = true;
        public bool freezeTime = false;

        private MeshCollider _meshCollider;

        private void setMeshColliderMesh(Mesh mesh)
        {
            if (mesh != null)
            {
                _meshCollider.sharedMesh = mesh;
            }
        }

        protected override void initBuilder()
        {
            _builder = new MeshBuilder(_dimensions, _triangleBudget, _builderCompute, supportCollisionMesh: true);

            _meshCollider = GetComponent<MeshCollider>();
        }

        protected override void Update()
        {

            // Noise field update for visualization mesh
            _volumeCompute.SetInts("Dims", _dimensions);
            _volumeCompute.SetFloat("Scale", _gridScale);
            _volumeCompute.SetBuffer(0, "Voxels", _voxelBuffer);
            

            if (freezeTime)
            {
                _volumeCompute.SetFloat("Time", 0);
            }
            else
            {
                _volumeCompute.SetFloat("Time", Time.time);
            }

            _volumeCompute.DispatchThreads(0, _dimensions);

            // Isosurface reconstruction for visualization mesh
            bool wasAbleToBuild = _builder.BuildIsosurface(_voxelBuffer, _targetValue, _gridScale);

            if (!wasAbleToBuild) { return; }

            GetComponent<MeshFilter>().sharedMesh = _builder.Mesh;

            if (useAsync)
            {
                _builder.GetMeshColliderMeshAsync(setMeshColliderMesh);
            }
            else
            {
                _meshCollider.sharedMesh = _builder.GetMeshColliderMesh();
            }      
        }

        
    }


} // namespace MarchingCubes
