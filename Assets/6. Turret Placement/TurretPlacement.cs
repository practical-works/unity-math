using Sirenix.OdinInspector;
using UnityEngine;

public class TurretPlacement : MonoBehaviour
{
    [SerializeField, BoxGroup("Casting")]
    private Vector3 _baseDirection = Vector3.right;

    [SerializeField, BoxGroup("Guns"), Range(0f, 3f)]
    private float _gunHeight = 1.3f;

    [SerializeField, BoxGroup("Guns"), Range(0f, 3f)]
    private float _gunsSeparation = 0.3f;

    [SerializeField, BoxGroup("Guns"), Range(0f, 3f)]
    private float _gunBarrelLength = 0.8f;

    //----------------------------------------------------------------------------------------------------

    [ShowInInspector, BoxGroup("Casting"), DisplayAsString, EnableGUI]
    private Vector3 Direction => transform.rotation * _baseDirection;

    //----------------------------------------------------------------------------------------------------

    private void OnValidate() => _baseDirection.Normalize();

    private void OnDrawGizmos()
    {
        Vector3 origin = transform.position;
        if (Physics.Raycast(origin, Direction, out RaycastHit hit))
        {
            Vector3 hitPoint = hit.point, hitNormal = hit.normal;
            DrawLine(origin, hitPoint);
            DrawBasisVectorsGizmo(hitPoint, hitNormal, out Vector3 tangent, out Vector3 biTangent);
            DrawBoundingBoxGizmo(hitPoint, hitNormal, tangent, biTangent);
            DrawGunBarrelsGizmo(hitPoint, hitNormal, tangent, biTangent);
        }
        else DrawRay(origin, 10f * Direction, Color.gray);
    }

    private void DrawBasisVectorsGizmo(Vector3 hitPoint, Vector3 normal, out Vector3 tangent, out Vector3 biTangent)
    {
        tangent = Vector3.Cross(Direction, normal).normalized;
        biTangent = Vector3.Cross(normal, tangent).normalized;
        DrawRay(hitPoint, normal, Color.green);
        DrawRay(hitPoint, tangent, Color.blue);
        DrawRay(hitPoint, biTangent, Color.red);
    }

    private void DrawBoundingBoxGizmo(Vector3 hitPoint, Vector3 normal, Vector3 tangent, Vector3 biTangent)
    {
        Gizmos.matrix = new(biTangent, normal, tangent, hitPoint);
        Gizmos.color = Color.cyan;
        Vector3[] cubeCorners = new Vector3[]
        {
             new ( 1, 0, 1 ), new ( -1, 0, 1 ), new ( -1, 0, -1 ), new ( 1, 0, -1 ), // Bottom 4 positions
             new ( 1, 2, 1 ), new ( -1, 2, 1 ), new ( -1, 2, -1 ), new ( 1, 2, -1 ) // Top 4 positions
        };
        for (int i = 0; i < 4; i++) // Draw wire cube
        {
            Gizmos.DrawLine(cubeCorners[i], cubeCorners[(i + 1) % 4]); // Draw bottom face
            Gizmos.DrawLine(cubeCorners[i + 4], cubeCorners[((i + 1) % 4) + 4]); // Draw top face
            Gizmos.DrawLine(cubeCorners[i], cubeCorners[i + 4]); // Draw other faces (Connect bottom and top)
        }
        Gizmos.matrix = Matrix4x4.identity;
    }

    private void DrawGunBarrelsGizmo(Vector3 hitPoint, Vector3 normal, Vector3 tangent, Vector3 biTangent)
    {
        Gizmos.matrix = new(biTangent, normal, tangent, hitPoint);
        Gizmos.color = Color.magenta;
        float _halfGunSeparation = 0.5f * _gunsSeparation;
        Gizmos.DrawRay(new(0f, _gunHeight, _halfGunSeparation), _gunBarrelLength * Vector3.right);
        Gizmos.DrawRay(new(0f, _gunHeight, -_halfGunSeparation), _gunBarrelLength * Vector3.right);
        Gizmos.matrix = Matrix4x4.identity;
    }

    //----------------------------------------------------------------------------------------------------

    private void DrawLine(Vector3 from, Vector3 to, Color color = default)
    {
        if (color != default) Gizmos.color = color;
        Gizmos.DrawLine(from, to);
    }

    private void DrawRay(Vector3 from, Vector3 direction, Color color = default)
    {
        if (color != default) Gizmos.color = color;
        Gizmos.DrawRay(from, direction);
    }
}