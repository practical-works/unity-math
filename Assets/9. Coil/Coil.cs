using Sirenix.OdinInspector;
using System;
using UnityEditor;
using UnityEngine;

public class Coil : MonoBehaviour
{
    private enum CoilShapeMode
    { Line, Torus, BézierCurve }

    private const float Tau = 6.283185307179586f; // 360° in radians (TAU = 2*PI)

    [SerializeField, LabelText("Quality"), Range(1f, 128f)] private int _segmentsPerTurnCount = 64;

    [SerializeField, EnumToggleButtons] private CoilShapeMode _shape = CoilShapeMode.Line;
    [SerializeField, BoxGroup("Size"), Min(1f)] private int _turnsCount = 1;
    [SerializeField, BoxGroup("Size"), Min(1f)] private float _height = 1f;
    [SerializeField, BoxGroup("Size"), Min(0.01f)] private float _radius = 1f;

    [ShowInInspector, BoxGroup("Torus"), ShowIf(nameof(_shape), CoilShapeMode.Torus), DisplayAsString, EnableGUI]
    private float TorusMajorRadius => _height / Tau; // height is torus's circumference (c = 2π*r = τ*r)

    [SerializeField, BoxGroup("Bézier Curve"), ShowIf(nameof(_shape), CoilShapeMode.BézierCurve)]
    private Vector3 _bezierPointA = new(2f, 0f, 0f);

    [SerializeField, BoxGroup("Bézier Curve"), ShowIf(nameof(_shape), CoilShapeMode.BézierCurve)]
    private Vector3 _bezierPointB = new(1f, 1f, 0f);

    [SerializeField, BoxGroup("Bézier Curve"), ShowIf(nameof(_shape), CoilShapeMode.BézierCurve)]
    private Vector3 _bezierPointC = new(-1f, 1f, 0f);

    [SerializeField, BoxGroup("Bézier Curve"), ShowIf(nameof(_shape), CoilShapeMode.BézierCurve)]
    private Vector3 _bezierPointD = new(-2f, 0f, 0f);

    [SerializeField, FoldoutGroup("Color")] private Color _startColor = Color.red;
    [SerializeField, FoldoutGroup("Color")] private Color _endColor = Color.green;

    //----------------------------------------------------------------------------------------------------

    private void OnDrawGizmos()
    {
        Vector3 startPoint = default;
        int segmentsCount = _segmentsPerTurnCount * _turnsCount;
        for (int i = 0; i < segmentsCount; i++)
        {
            float ratio = (i + 1f) / segmentsCount;
            float nextRatio = (i + 2f) / segmentsCount;
            Color color = Color.Lerp(_startColor, _endColor, ratio);
            switch (_shape)
            {
                case CoilShapeMode.Line:
                    if (startPoint == default) startPoint = GetLineCoilPoint(0f);
                    DrawLine(startPoint, startPoint = GetLineCoilPoint(ratio), color);
                    break;

                case CoilShapeMode.Torus:
                    if (startPoint == default) startPoint = GetTorusCoilPoint(0f);
                    DrawLine(startPoint, startPoint = GetTorusCoilPoint(ratio), color);
                    break;

                case CoilShapeMode.BézierCurve:
                    if (startPoint == default) startPoint = GetBézierCurveCoilPoint(0f);
                    DrawLine(startPoint, startPoint = GetBézierCurveCoilPoint(ratio), color);
                    DrawLine(GetBezierPoint(ratio), GetBezierPoint(nextRatio));
                    break;

                default:
                    break;
            }
        }
        if (_shape == CoilShapeMode.BézierCurve) DrawBezierCurve();
    }

    //----------------------------------------------------------------------------------------------------

    private Vector3 GetLineCoilPoint(float ratio)
    {
        float angle = ratio * _turnsCount * Tau;
        return _radius * new Vector3()
        {
            x = Mathf.Cos(angle),
            y = ratio * _height,
            z = Mathf.Sin(angle)
        };
    }

    private Vector3 GetTorusCoilPoint(float ratio)
    {
        float angle = ratio * Tau;
        Vector3 coilCenterPoint = TorusMajorRadius * new Vector3()
        {
            x = 0f,
            y = Mathf.Sin(angle),
            z = Mathf.Cos(angle)
        };
        Vector3 lineCoilPoint = GetLineCoilPoint(ratio);
        Vector3 xCoilPoint = lineCoilPoint.x * Vector3.right;
        Vector3 zCoilPoint = lineCoilPoint.z * coilCenterPoint.normalized;
        return coilCenterPoint + xCoilPoint + zCoilPoint;
    }

    private Vector3 GetBézierCurveCoilPoint(float ratio)
    {
        Vector3 bezierPoint = GetBezierPoint(ratio);
        Vector3 lineCoilPoint = GetLineCoilPoint(ratio);
        Vector3 xCoilPoint = lineCoilPoint.x * Vector3.forward;
        Vector3 zCoilPoint = lineCoilPoint.z * bezierPoint.normalized;
        return bezierPoint + xCoilPoint + zCoilPoint;
    }

    //----------------------------------------------------------------------------------------------------

    private Vector3 GetBezierPoint(float ratio)
    {
        Vector3 bezierPointAB = Vector3.Lerp(_bezierPointA, _bezierPointB, ratio);
        Vector3 bezierPointBC = Vector3.Lerp(_bezierPointB, _bezierPointC, ratio);
        Vector3 bezierPointCD = Vector3.Lerp(_bezierPointC, _bezierPointD, ratio);
        Vector3 bezierPointABC = Vector3.Lerp(bezierPointAB, bezierPointBC, ratio);
        Vector3 bezierPointBCD = Vector3.Lerp(bezierPointBC, bezierPointCD, ratio);
        return Vector3.Lerp(bezierPointABC, bezierPointBCD, ratio);
    }

    //----------------------------------------------------------------------------------------------------

    private void DrawBezierCurve()
    {
        DrawLine(_bezierPointA, _bezierPointB, Color.gray);
        DrawLine(_bezierPointC, _bezierPointD, Color.gray);
        DrawPoint(_bezierPointA, Color.red);
        DrawPoint(_bezierPointB, Color.green);
        DrawPoint(_bezierPointC, Color.blue);
        DrawPoint(_bezierPointD, Color.cyan);
        DrawText(_bezierPointA + 0.1f * transform.up, "A", Color.red);
        DrawText(_bezierPointB + 0.1f * transform.up, "B", Color.green);
        DrawText(_bezierPointC + 0.1f * transform.up, "C", Color.blue);
        DrawText(_bezierPointD + 0.1f * transform.up, "D", Color.cyan);
    }

    private void DrawLine(Vector3 from, Vector3 to, Color color = default)
    {
        Gizmos.color = color == default ? Color.white : color;
        from = transform.position + (transform.rotation * from);
        to = transform.position + (transform.rotation * to);
        Gizmos.DrawLine(from, to);
    }

    private void DrawPoint(Vector3 point, Color color = default)
    {
        Gizmos.color = color == default ? Color.white : color;
        Gizmos.DrawSphere(transform.position + (transform.rotation * point), 0.05f);
    }

    private void DrawText(Vector3 position, string text, Color color = default)
    {
        GUIStyle style = new(GUI.skin.label);
        style.normal.textColor = color == default ? Color.white : color;
        Handles.Label(transform.position + (transform.rotation * position), text, style);
    }
}