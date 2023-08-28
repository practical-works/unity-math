using Sirenix.OdinInspector;
using UnityEngine;

public class RadialTrigger : MonoBehaviour
{
    [SerializeField]
    private Transform _sensor, _actor;

    [Space]
    [SerializeField, Range(0f, 50f)]
    public float _radius = 1f;

    //----------------------------------------------------------------------------------------------------

    public bool Ready => _actor && _sensor;

    [ShowInInspector, DisplayAsString, EnableGUI]
    public float Distance
    {
        get
        {
            if (!Ready) return -1f;

            // Method 1
            Vector3 differenceVector = _sensor.position - _actor.position;
            return Mathf.Sqrt(differenceVector.x * differenceVector.x + differenceVector.y * differenceVector.y);

            // Method 2
            //return differenceVector.magnitude;

            // Method 3
            //return Vector3.Distance(_sensor.position, _actor.position);
        }
    }

    [ShowInInspector, DisplayAsString, EnableGUI]
    public bool Inside => Distance <= _radius;

    //----------------------------------------------------------------------------------------------------

    private void OnDrawGizmos()
    {
        if (!Ready) return;
        bool inside = Inside;

        // Draw distance line between sensor and actor
        Gizmos.color = inside ? Color.red : Color.green;
        Gizmos.DrawLine(_sensor.position, _actor.position);

        // Draw sensor's detection sphere
        Gizmos.color = inside ? Color.red : Color.white;
        Gizmos.DrawWireSphere(_sensor.position, _radius);
    }
}