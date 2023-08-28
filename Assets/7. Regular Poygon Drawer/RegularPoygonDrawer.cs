using Sirenix.OdinInspector;
using UnityEngine;

public class RegularPoygonDrawer : MonoBehaviour
{
    [SerializeField, BoxGroup("Polygon"), Range(3f, 100f)]
    private int _sidesCount = 3;

    [SerializeField, BoxGroup("Polygon"), Range(1f, 10f)]
    private int _density = 1;

    [SerializeField, BoxGroup("Polygon"), Range(0f, 10f)]
    private float _radius = 3;

    [SerializeField, BoxGroup("Polygon"), DisplayAsString]
    private float _area;

    //----------------------------------------------------------------------------------------------------

    private void OnDrawGizmos()
    {
        Vector3[] radiusVects = new Vector3[_sidesCount];
        for (int i = 0; i < _sidesCount; i++)
        {
            float angleRad = ((i / (float)_sidesCount * 2f) + 0.5f) * Mathf.PI; // Added 2π offset rotation
            Vector3 radiusVect = transform.position + _radius *
                (transform.rotation * new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad)));

            // Draw radius
            Gizmos.color = Color.gray;
            Gizmos.DrawLine(transform.position, radiusVect);

            // Draw vertex
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(radiusVect, 0.1f);

            // Save radius vector
            radiusVects[i] = radiusVect;
        }

        // Draw sides and calculate area
        _area = 0;
        for (int i = 0; i < _sidesCount; i++)
        {
            Vector3 start = radiusVects[i];
            Vector3 end = radiusVects[(i + _density) % _sidesCount];
            _area += 0.5f * Vector3.Cross(start, end).magnitude;
            Gizmos.DrawLine(start, end);
        }
    }
}