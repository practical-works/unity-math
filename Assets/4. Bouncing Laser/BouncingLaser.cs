using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class BouncingLaser : MonoBehaviour
{
    [SerializeField, BoxGroup("Ray")]
    private uint _maxBounces = 1;

    [SerializeField, BoxGroup("Actor")]
    private Transform _actor;

    [SerializeField, BoxGroup("Actor")]
    private float _actorSpeed = 1f;

    [ShowInInspector, ReadOnly]
    private readonly List<Vector3> _rayDirections = new();

    [ShowInInspector, ReadOnly]
    private readonly List<float> _rayDistances = new();

    [ShowInInspector, DisplayAsString]
    private Vector3 _actorDirection;

    [ShowInInspector, DisplayAsString]
    private float _actorRemainingDistance;

    [ShowInInspector, DisplayAsString]
    private int _rayIndex = -1;

    [ShowInInspector, DisplayAsString]
    private bool _reverseActorPath;

    //----------------------------------------------------------------------------------------------------

    [ShowInInspector, DisplayAsString, EnableGUI, SuffixLabel("Vectors")]
    public int RayPathLength
    {
        get
        {
            if (_rayDirections.Count != _rayDirections.Count) return 0;
            else if (_rayDirections.Count == 0) return 0;
            else return _rayDirections.Count;
        }
    }

    //----------------------------------------------------------------------------------------------------

    private Vector3 Reflect(Vector3 inDirection, Vector3 inNormal)
    {
        // Method 1
        Vector3 displacement = -2f * Vector3.Dot(inDirection, inNormal) * inNormal;
        return inDirection + displacement;

        // Method 2
        //return Vector3.Reflect(inDirection, inNormal);
    }

    private bool Cast(Vector3 origin, Vector3 direction, out Vector3 hitPoint, out Vector3 hitNormal, out Vector3 reflection)
    {
        if (Physics.Raycast(origin, direction, out RaycastHit hit))
        {
            hitPoint = hit.point;
            hitNormal = hit.normal;
            reflection = Reflect(direction, hitNormal);
            return true;
        }
        else
        {
            hitPoint = hitNormal = reflection = default;
            return false;
        }
    }

    private float MoveActor(Vector3 direction)
    {
        float step = _actorSpeed * Time.deltaTime;
        Vector3 displacement = step * direction;
        Vector3 oldPosition = _actor.transform.position;
        _actor.transform.position = oldPosition + displacement;
        return step;
    }

    private void AddToRayPath(Vector3 direction, Vector3 origin, Vector3 hitPoint)
    {
        _rayDirections.Add(direction);
        _rayDistances.Add(Vector3.Distance(origin, hitPoint));
    }

    private void ClearRayPath()
    {
        _rayDirections.Clear();
        _rayDistances.Clear();
    }

    //----------------------------------------------------------------------------------------------------

    private void Awake()
    {
        if (_actor) _actor.transform.position = transform.position;
    }

    private void Update()
    {
        if (!_actor) return;
        int length = RayPathLength;
        if (length <= 0) return;
        if (_actorRemainingDistance > 0)
        {
            _actorRemainingDistance -= MoveActor(_actorDirection);
        }
        else
        {
            if (_rayIndex < length - 1)
            {
                _rayIndex++;
                if (_reverseActorPath)
                {
                    int _reverseRayIndex = length - _rayIndex - 1;
                    _actorDirection = -_rayDirections[_reverseRayIndex];
                    _actorRemainingDistance = _rayDistances[_reverseRayIndex];
                }
                else
                {
                    _actorDirection = _rayDirections[_rayIndex];
                    _actorRemainingDistance = _rayDistances[_rayIndex];
                }
            }
            else
            {
                _rayIndex = -1;
                _reverseActorPath = !_reverseActorPath;
            }
        }
    }

    private void OnDrawGizmos()
    {
        ClearRayPath();
        uint bounces = 0;
        Vector3 origin = transform.position;
        Vector3 direction = transform.right;
        while (bounces < _maxBounces + 1)
        {
            if (Cast(origin, direction, out Vector3 hitPoint, out Vector3 hitNormal, out Vector3 reflection))
            {
                AddToRayPath(direction, origin, hitPoint);
                DrawLine(from: origin, to: hitPoint, Color.red);
                DrawRay(from: hitPoint, direction: hitNormal, Color.cyan);
                origin = hitPoint;
                direction = reflection;
                bounces++;
            }
            else
            {
                DrawRay(from: origin, direction: 100f * direction, Color.gray);
                break;
            }
        }
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