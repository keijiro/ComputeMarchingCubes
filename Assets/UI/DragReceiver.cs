using UnityEngine;
using UnityEngine.UIElements;

namespace MarchingCubes {

public sealed class DragReceiver : PointerManipulator
{
    #region Constructor

    public delegate void DragCallback(Vector2 delta);

    public DragReceiver(DragCallback callback)
    {
        _callback = callback;
        activators.Add(new ManipulatorActivationFilter{button = MouseButton.LeftMouse});
    }

    #endregion

    #region Private variables

    DragCallback _callback;
    int _pointerID = -1;
    Vector3 _position;

    #endregion

    #region PointerManipulator implementation

    bool IsActive => _pointerID >= 0;

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(OnPointerDown);
        target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        target.RegisterCallback<PointerUpEvent>(OnPointerUp);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
        target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
        target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
    }

    #endregion

    #region Pointer callbacks

    void OnPointerDown(PointerDownEvent e)
    {
        if (IsActive)
        {
            e.StopImmediatePropagation();
        }
        else if (CanStartManipulation(e))
        {
            _position = e.position;
            target.CapturePointer(_pointerID = e.pointerId);
            e.StopPropagation();
        }
    }

    void OnPointerMove(PointerMoveEvent e)
    {
        if (!IsActive || !target.HasPointerCapture(_pointerID)) return;

        _callback(e.position - _position);
        _position = e.position;

        e.StopPropagation();
    }

    void OnPointerUp(PointerUpEvent e)
    {
        if (!IsActive || !target.HasPointerCapture(_pointerID)) return;

        if (CanStopManipulation(e))
        {
            _pointerID = -1;
            target.ReleaseMouse();
            e.StopPropagation();
        }
    }

    #endregion
}

} // namespace MarchingCubes
