using UnityEngine;
using UnityEngine.UIElements;
using Unity.Mathematics;
using Klak.Math;

namespace MarchingCubes {

public sealed class PivotController : MonoBehaviour
{
    [field:SerializeField] public float2 Speed { get; set; } = 0.02f;

    quaternion _rotation;

    void OnPointerDrag(Vector2 delta)
    {
        var rx = quaternion.RotateY(delta.x * Speed.x);
        var ry = quaternion.RotateX(delta.y * Speed.y);
        _rotation = math.mul(_rotation, math.mul(rx, ry));
    }

    void Start()
    {
        _rotation = transform.localRotation;
        FindFirstObjectByType<UIDocument>().rootVisualElement.
          Q("empty-area").AddManipulator(new DragReceiver(OnPointerDrag));
    }

    void Update()
      => transform.localRotation =
           ExpTween.Step(transform.localRotation, _rotation, 12);
}

} // namespace MarchingCubes
