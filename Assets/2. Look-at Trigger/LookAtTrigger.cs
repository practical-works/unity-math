using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public class LookAtTrigger : MonoBehaviour
{
    [SerializeField]
    private Transform _sensor, _actor;

    [SerializeField, BoxGroup("Vision Cone Gizmo")]
    private int _edgesCount = 20;

    [SerializeField, BoxGroup("Sensitivity"), Range(0f, 1f)]
    private float _sensitivity;

    //----------------------------------------------------------------------------------------------------

    public bool Ready => _actor && _sensor;

    public Vector3 SensorFaceDirection => _sensor.transform.up;

    public Vector3 DistanceVector => _actor.position - _sensor.position;

    [ShowInInspector, BoxGroup("Sensitivity"), DisplayAsString, SuffixLabel("degrees")]
    public float VisionAngle => 2f * Mathf.Acos(_sensitivity) * Mathf.Rad2Deg;

    [ShowInInspector, BoxGroup("Sensitivity"), DisplayAsString, SuffixLabel("degrees")]
    public float VisionAngleApprox => Mathf.Round(VisionAngle);

    [ShowInInspector, BoxGroup("Sensitivity"), DisplayAsString, EnableGUI, SuffixLabel("degrees")]
    public float HalfVisionAngle => 0.5f * VisionAngle;

    [ShowInInspector, BoxGroup("Sensitivity"), DisplayAsString, EnableGUI, SuffixLabel("degrees")]
    public float HalfVisionAngleApprox => Mathf.Round(HalfVisionAngle);

    [ShowInInspector, BoxGroup("Dot Product"), DisplayAsString, SuffixLabel("degrees")]
    public float Angle => Vector3.Angle(SensorFaceDirection, DistanceVector.normalized);

    [ShowInInspector, BoxGroup("Dot Product"), DisplayAsString, EnableGUI, SuffixLabel("degrees")]
    public float AngleApprox => Mathf.Round(Angle);

    [ShowInInspector, BoxGroup("Dot Product"), DisplayAsString]
    public float AngleCos => Mathf.Cos(Angle * Mathf.Deg2Rad);

    [ShowInInspector, BoxGroup("Dot Product"), DisplayAsString]
    public float DotProduct => Ready ? Vector3.Dot(SensorFaceDirection, DistanceVector.normalized) : 0f;

    [ShowInInspector, BoxGroup("Dot Product"), DisplayAsString, EnableGUI]
    public float DotProductApprox => Mathf.Round(DotProduct * 100f) / 100f;

    [ShowInInspector, DisplayAsString]
    public bool Seen => DotProduct >= _sensitivity;

    //----------------------------------------------------------------------------------------------------

    private void OnDrawGizmos()
    {
        if (!Ready) return;
        bool seen = Seen;
        Gizmos.color = seen ? Color.white : Color.gray;

        // Draw distance line between sensor and actor
        Gizmos.DrawLine(_sensor.position, _actor.position);

        // Draw sensor's face direction
        Vector3 sensorFaceDirection = SensorFaceDirection;
        Gizmos.DrawRay(_sensor.position, 15f * sensorFaceDirection);

        // Draw sensor's vision cone
        Gizmos.color = seen ? Color.red : Color.blue;
        DrawConeGizmo(_sensor.position, sensorFaceDirection, VisionAngle);
        //DrawPyramidGizmo(_sensor.position, sensorFaceDirection, VisionAngle);
    }

    //----------------------------------------------------------------------------------------------------

    private void DrawConeGizmo(Vector3 origin, Vector3 direction, float angleDeg, float edgeLength = 10f)
    {
        //float angleRad = angleDeg * Mathf.Deg2Rad;
        //for (int i = 0; i < _edgesCount; i++) // 2D only
        //{
        //    float ratio = i / (_edgesCount - 1f);
        //    float radians = ratio * angleRad;
        //    radians += GetAngleFromVector(direction) - (0.5f * angleRad); // offset
        //    Vector3 edge = edgeLength * GetVectorFromAngle(radians, 0f);
        //    Gizmos.DrawRay(origin, edge);
        //}
        Vector3 directionPerpendicular = GetPerpendicularVector(direction);
        for (int i = 0; i < _edgesCount; i++)
        {
            float horizontalAngle = i / (_edgesCount - 1f) * (2f * Mathf.PI) * Mathf.Rad2Deg;
            float verticalAngle = 0.5f * angleDeg;

            Quaternion horizontalRotation = Quaternion.AngleAxis(horizontalAngle, direction);
            Quaternion verticalRotation = Quaternion.AngleAxis(verticalAngle, directionPerpendicular);
            Quaternion rotation = horizontalRotation * verticalRotation;

            Vector3 edge = edgeLength * (rotation * direction);
            Gizmos.DrawRay(origin, edge);
        }
    }

    //private void DrawPyramidGizmo(Vector3 origin, Vector3 direction, float angleDeg, float edgeLength = 10f)
    //{
    //    float halfAngleDeg = 0.5f * angleDeg;
    //    Vector3 GetVectorFromAngle(float angle, Vector3 axis) => Quaternion.AngleAxis(angle, axis) * direction * edgeLength;
    //    Vector3[] edges = new Vector3[4]
    //    {
    //        GetVectorFromAngle(-halfAngleDeg, Vector3.forward), // Up edge
    //        GetVectorFromAngle(-halfAngleDeg, Vector3.right), // Right edge
    //        GetVectorFromAngle(halfAngleDeg, Vector3.forward), // Bottom edge
    //        GetVectorFromAngle(halfAngleDeg, Vector3.right) // Left edge
    //    };
    //    for (int i = 0; i < 4; i++)
    //    {
    //        Gizmos.DrawRay(origin, edges[i]);
    //        Gizmos.DrawLine(origin + edges[i], origin + edges[(i + 1) % 4]);
    //    }
    //}

    //----------------------------------------------------------------------------------------------------

    /// <summary>
    /// Converts a vector to an angle.
    /// </summary>
    /// <param name="vector">vector to convert.</param>
    /// <returns></returns>
    private float GetAngleFromVector(Vector3 vector) => Mathf.Atan2(vector.y, vector.x);

    /// <summary>
    /// Converts an angle to a vector in 2D space.
    /// </summary>
    /// <param name="angle">Angle to convert in radians.</param>
    private Vector2 GetVectorFromAngle(float angle) => new(Mathf.Cos(angle), Mathf.Sin(angle));

    /// <summary>
    /// Converts an angle to a vector in 3D space.
    /// </summary>
    /// <param name="azimuth">Horizontal Angle to convert in radians.</param>
    /// <param name="elevation">Vertical Angle to convert in radians.</param>
    /// <returns></returns>
    private Vector3 GetVectorFromAngle(float azimuth, float elevation)
    {
        return new()
        {
            x = Mathf.Cos(azimuth) * Mathf.Cos(elevation),
            y = Mathf.Sin(azimuth) * Mathf.Cos(elevation),
            z = Mathf.Sin(elevation)
        };
    }

    /// <summary>
    /// Gets a normalized perpendicular vector to the given vector.
    /// </summary>
    /// <param name="vector">The input vector.</param>
    /// <returns>A normalized Vector3 that is perpendicular to the input vector.</returns>
    private Vector3 GetPerpendicularVector(Vector3 vector)
    {
        float dotProduct = Vector3.Dot(vector.normalized, Vector3.right);
        Vector3 arbitraryVector = Mathf.Abs(dotProduct) < 0.99f ? Vector3.right : Vector3.up;
        return Vector3.Cross(vector, arbitraryVector).normalized;
    }
}