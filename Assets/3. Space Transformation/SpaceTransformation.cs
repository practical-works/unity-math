using Sirenix.OdinInspector;
using UnityEngine;

public class SpaceTransformation : MonoBehaviour
{
    [SerializeField]
    private Transform _origin;

    [SerializeField, HideInInspector]
    private Vector3 _point, _pointInWorld;

    //----------------------------------------------------------------------------------------------------

    [ShowInInspector, GUIColor(0f, 1f, 1f), PropertySpace]
    private Vector3 Point
    {
        get => _point;
        set => _pointInWorld = GetWorldPosition(_point = value);
    }

    [ShowInInspector]
    private Vector3 PointInWorld
    {
        get => _pointInWorld;
        set => _point = GetLocalPosition(_pointInWorld = value);
    }

    //----------------------------------------------------------------------------------------------------

    public bool Ready => _origin;

    //----------------------------------------------------------------------------------------------------

    private void OnDrawGizmos()
    {
        if (!Ready) return;
        DrawAxises(null, 10f);
        DrawAxises(_origin, 10f);
        DrawPoint(PointInWorld);
        if (_origin.transform.hasChanged)
        {
            _pointInWorld = GetWorldPosition(_point);
            _origin.transform.hasChanged = false;
        }
    }

    //----------------------------------------------------------------------------------------------------

    private Vector3 GetLocalPosition(Vector3 worldPosition)
    {
        // Method 1
        Vector3 originXAxis = _origin.transform.right;
        Vector3 originYAxis = _origin.transform.up;
        Vector3 originZAxis = _origin.transform.forward;
        Vector3 offset = worldPosition - _origin.position;
        return new()
        {
            x = Vector3.Dot(offset, originXAxis),
            y = Vector3.Dot(offset, originYAxis),
            z = Vector3.Dot(offset, originZAxis)
        };

        // Method 2
        //return _origin.transform.InverseTransformPoint(worldPosition);
    }

    private Vector3 GetWorldPosition(Vector3 localPosition)
    {
        // Method 1
        Vector3 originXAxis = _origin.transform.right;
        Vector3 originYAxis = _origin.transform.up;
        Vector3 originZAxis = _origin.transform.forward;
        Vector3 offset = originXAxis * localPosition.x + originYAxis * localPosition.y + originZAxis * localPosition.z;
        return offset + _origin.position;

        // Method 2
        //return _origin.transform.TransformPoint(localPosition);
    }

    //----------------------------------------------------------------------------------------------------

    private void DrawPoint(Vector3 position)
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(position, 0.1f);
    }

    private void DrawAxises(Transform transform = null, float axisLength = 1f)
    {
        Vector3 origin = transform ? transform.position : Vector3.zero;
        Vector3 xAxis = transform ? transform.right : Vector3.right;
        Vector3 yAxis = transform ? transform.up : Vector3.up;
        Vector3 zAxis = transform ? transform.forward : Vector3.forward;
        void DrawAxis(Vector3 axis, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawRay(origin, axisLength * axis);
        }
        DrawAxis(xAxis, Color.red);
        DrawAxis(yAxis, Color.green);
        DrawAxis(zAxis, Color.blue);
    }
}