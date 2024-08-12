using UnityEngine;
using UnityEngine.UIElements;
using Unity.Mathematics;
using Klak.Math;

namespace MarchingCubes {

sealed class DragReceiver : PointerManipulator
{
    CameraPivotController _target;

    public DragReceiver(CameraPivotController target)
    {
        _target = target;
        activators.Add(new ManipulatorActivationFilter{button = MouseButton.LeftMouse});
    }

    protected override void RegisterCallbacksOnTarget()
      => target.RegisterCallback<PointerMoveEvent>(OnPointerMove);

    protected override void UnregisterCallbacksFromTarget()
      => target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);

    void OnPointerMove(PointerMoveEvent e)
    {
        if (e.pressedButtons == 1) _target.OnPointerDrag(e.deltaPosition);
        e.StopPropagation();
    }
}

public sealed class CameraPivotController : MonoBehaviour
{
    [field:SerializeField] public float2 Speed { get; set; } = 0.02f;

    quaternion _rotation;

    public void OnPointerDrag(float3 delta)
    {
        var rx = quaternion.RotateY(delta.x * Speed.x);
        var ry = quaternion.RotateX(delta.y * Speed.y);
        _rotation = math.mul(_rotation, math.mul(rx, ry));
    }

    void Start()
    {
        _rotation = transform.localRotation;
        FindFirstObjectByType<UIDocument>().rootVisualElement.
          Q("empty-area").AddManipulator(new DragReceiver(this));
    }

    void Update()
      => transform.localRotation = ExpTween.Step(transform.localRotation, _rotation, 12);
}

} // namespace MarchingCubes
