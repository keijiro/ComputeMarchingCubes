using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MarchingCubes
{

    public class ColliderMeshBuilder : System.IDisposable
    {
        public ColliderMeshBuilder(ComputeShader computeShader, Mesh mesh, GraphicsBuffer indexBuffer, ComputeBuffer counterBuffer, int triangleBudget)
        {
            _compute = computeShader;
            _compute.EnableKeyword("GENERATE_COLLIDER_MESH_DATA");
            _mesh = mesh;
            _indexBuffer = indexBuffer;
            _counterBuffer = counterBuffer;
            _triangleBudget = triangleBudget;

            AllocateBuffer();

            _persistentPositions = new NativeArray<Vector3>(3 * _triangleBudget, Allocator.Persistent);
            _persistentIndices = new NativeArray<int>(3 * _triangleBudget, Allocator.Persistent);
        }

        readonly ComputeShader _compute;
        readonly Mesh _mesh;
        readonly GraphicsBuffer _indexBuffer;
        readonly ComputeBuffer _counterBuffer;
        readonly int _triangleBudget;


        private readonly NativeArray<Vector3> _persistentPositions;
        private readonly NativeArray<int> _persistentIndices;

        private ComputeBuffer _triangleCountBuffer;
        private GraphicsBuffer _positionBuffer;

        public bool isAysncReadLocked => _isBuilding;


        public void Dispose() => ReleaseBuffer();

        public Mesh GetColliderMesh()
        {
            int[] countData = new int[1];
            _triangleCountBuffer.GetData(countData);
            int triangleCountActual = countData[0];
            int vertexCountActual = triangleCountActual * 3;

            if (vertexCountActual == 0)
            {
                return null;
            }

            Mesh colliderMesh = new Mesh();
            colliderMesh.indexFormat = IndexFormat.UInt32;

            Vector3[] positions = new Vector3[vertexCountActual];
            _positionBuffer.GetData(positions, 0, 0, vertexCountActual);

            int[] triangleIndices = new int[vertexCountActual];
            _indexBuffer.GetData(triangleIndices, 0, 0, vertexCountActual);

            colliderMesh.vertices = positions;
            colliderMesh.triangles = triangleIndices;
            colliderMesh.bounds = _mesh.bounds;

            return colliderMesh;
        }



        public void BindComputeBuffer()
        {
            _compute.SetBuffer(0, "PositionBuffer", _positionBuffer);
        }

        public void CopyCountBuffer()
        {
            ComputeBuffer.CopyCount(src: _counterBuffer, dst: _triangleCountBuffer, dstOffsetBytes: 0);
        }

        private void AllocateBuffer()
        {
            _triangleCountBuffer = new ComputeBuffer(count: 1, stride: 4, type: ComputeBufferType.Raw);
            _positionBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Raw | GraphicsBuffer.Target.Structured, 3 * _triangleBudget, sizeof(float) * 3);

        }
        private void ReleaseBuffer()
        {
            _triangleCountBuffer.Dispose();
            _positionBuffer.Dispose();

            _persistentPositions.Dispose();
            _persistentIndices.Dispose();
        }

        private bool _isBuilding = false;

        public void GetColliderMeshAsync(Action<Mesh> onComplete)
        {

            if (_isCancellingReadbacks)
            {
                onComplete?.Invoke(null);
                return;
            }

            if (_isBuilding)
            {
                Debug.LogWarning("Already building; please wait for the first request to finish.");
                onComplete?.Invoke(null);
                return;
            }

            _isBuilding = true;

            AsyncGPUReadbackRequest countRequest = AsyncGPUReadback.Request(_triangleCountBuffer, (Action<AsyncGPUReadbackRequest>)(request =>
            {
                if (_isCancellingReadbacks)
                    return;

                if (request.hasError)
                {
                    Debug.LogError("Triangle count readback failed");
                    onComplete?.Invoke(null);
                    _isBuilding = false;
                    return;
                }

                int triangleCount = request.GetData<int>()[0];
                triangleCount = Mathf.Min(triangleCount, _triangleBudget);
                int vertexCount = triangleCount * 3;
                

                CreatePositionAndIndexReadbacks(vertexCount, onComplete);
            })
            );

            _pendingReadbackRequests.Add(countRequest);
        }


        private void CreatePositionAndIndexReadbacks(int vertexCount, Action<Mesh> onComplete)
        {
            if (vertexCount == 0 || _mesh == null)
            {
                onComplete?.Invoke(null);
                _isBuilding = false;
                return;
            }

            Mesh colliderMesh = new Mesh
            {
                indexFormat = IndexFormat.UInt32,
                bounds = _mesh.bounds
            };


            bool positionsDone = false;
            bool indicesDone = false;

            void TryFinish()
            {
                if (_isCancellingReadbacks)
                {
                    onComplete?.Invoke(null);
                    _isBuilding = false;
                    return;
                }

                if (!positionsDone || !indicesDone)
                {
                    return;
                }
                    

                colliderMesh.SetVertices(_persistentPositions, 0, vertexCount);
                colliderMesh.SetIndices(_persistentIndices, 0, vertexCount, MeshTopology.Triangles, 0, false, 0);

                onComplete?.Invoke(colliderMesh);
                _isBuilding = false;
            }

            // Position readback
            AsyncGPUReadbackRequest posRequest = AsyncGPUReadback.Request(_positionBuffer, vertexCount * UnsafeUtility.SizeOf<Vector3>(), 0, (Action<AsyncGPUReadbackRequest>)(request =>
            {
                if (_isCancellingReadbacks)
                {
                    return;
                } 

                if (request.hasError)
                {
                    Debug.LogError("Position readback failed");
                    onComplete?.Invoke(null);
                    _isBuilding = false;
                    return;
                }

                NativeArray<Vector3> oneFramePositions = request.GetData<Vector3>();
                NativeArray<Vector3>.Copy(src: oneFramePositions, dst: _persistentPositions, length: vertexCount);

                positionsDone = true;
                TryFinish();
            })
            );
            _pendingReadbackRequests.Add(posRequest);


            // Index readback
            AsyncGPUReadbackRequest idxRequest = AsyncGPUReadback.Request(_indexBuffer, vertexCount * UnsafeUtility.SizeOf<int>(), 0, (Action<AsyncGPUReadbackRequest>)(request =>
            {
                if (_isCancellingReadbacks)
                {
                    return;
                }
                    

                if (request.hasError)
                {
                    Debug.LogError("Index readback failed");
                    onComplete?.Invoke(null);
                    _isBuilding = false;
                    return;
                }


                NativeArray<int> oneFrameIndices = request.GetData<int>();
                NativeArray<int>.Copy(src: oneFrameIndices, dst: _persistentIndices, length: vertexCount);

                indicesDone = true;
                TryFinish();
            })
            );
            _pendingReadbackRequests.Add(idxRequest);
        }





        private static readonly List<AsyncGPUReadbackRequest> _pendingReadbackRequests = new List<AsyncGPUReadbackRequest>();
        private static bool _isCancellingReadbacks = false;

#if UNITY_EDITOR

        [InitializeOnLoad]
        static class MeshBuilderEditorHooks //Required to avoid editor crash on script reload
        {
            static MeshBuilderEditorHooks()
            {
                AssemblyReloadEvents.beforeAssemblyReload += ColliderMeshBuilder.CancelAllPendingReadbacks;

                EditorApplication.playModeStateChanged += state =>
                {
                    if (state == PlayModeStateChange.ExitingPlayMode)
                        ColliderMeshBuilder.CancelAllPendingReadbacks();
                };
            }
        }

        public static void CancelAllPendingReadbacks()
        {
            if (_isCancellingReadbacks) return;
            _isCancellingReadbacks = true;

            for (int i = 0; i < _pendingReadbackRequests.Count; i++)
            {
                AsyncGPUReadbackRequest request = _pendingReadbackRequests[i];
                if (!request.done)
                    request.WaitForCompletion();
            }
            _pendingReadbackRequests.Clear();
            _isCancellingReadbacks = false;
        }
#endif

    }

} // namespace MarchingCubes