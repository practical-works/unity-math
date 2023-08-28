using Sirenix.OdinInspector;
using UnityEngine;

public class AdaptiveFieldOfView : MonoBehaviour
{
    [SerializeField, HideInInspector]
    private CameraInfos _camInfos;

    [Title("Actors")]
    [SerializeField, OnValueChanged(nameof(LoadActors))]
    private string _actorTag = "Actor";

    [SerializeField, TableList]
    private SphericActor[] _actors;

    //----------------------------------------------------------------------------------------------------

    [Title("Camera")]
    [ShowInInspector, HideReferenceObjectPicker]
    public CameraInfos CamInfos
    {
        get
        {
            _camInfos ??= new();
            _camInfos.Camera = _camInfos.Camera == null ? GetComponent<Camera>() : _camInfos.Camera;
            return _camInfos;
        }
        set => _camInfos = value;
    }

    //----------------------------------------------------------------------------------------------------

    private void OnDrawGizmos()
    {
        CamInfos.DrawGizmos();
        if (_actors == null || _actors.Length == 0) return;

        // # Camera data (We are using camera's local space as a virtual 2D space)
        Camera cam = CamInfos.Camera;
        Vector3 camPos = cam.transform.position;
        Vector2 camVirtualDir = Vector2.right;

        // # Vertical data
        SphericActor vertiActor = default;
        Vector3 vertiActorPos = default;
        Vector3 vertiActorVirtualPos = default;
        float vertiActorCamDirsDotProd = float.MaxValue;

        // # Horiztontal data
        SphericActor horizActor = default;
        Vector3 horizActorPos = default;
        Vector3 horizActorVirtualPos = default;
        float horizActorCamDirsDotProd = float.MaxValue;

        // Get vertically and horizontally farthest actor from camera field
        foreach (SphericActor currentActor in _actors)
        {
            Vector3 currentActorPos = currentActor.transform.position;

            // Get vertically farthest
            Vector2 currentActorVirtualPos = Swizzle(cam.transform.InverseTransformPoint(currentActorPos), "z", "y");
            Vector2 currentActorPosOffset = (currentActorVirtualPos.y > 0f ? 1f : -1f) * currentActor.Radius * Vector2.up;
            float currentActorCamDirsDotProd = Vector3.Dot((currentActorVirtualPos + currentActorPosOffset).normalized, camVirtualDir);
            if (currentActorCamDirsDotProd <= vertiActorCamDirsDotProd)
            {
                vertiActorCamDirsDotProd = currentActorCamDirsDotProd;
                vertiActor = currentActor;
                vertiActorVirtualPos = currentActorVirtualPos;
                vertiActorPos = currentActor.transform.position;
            }

            // Get horizontally farthest
            currentActorVirtualPos = Swizzle(cam.transform.InverseTransformPoint(currentActorPos), "z", "x");
            currentActorPosOffset = (currentActorVirtualPos.y > 0f ? 1f : -1f) * currentActor.Radius * Vector2.up;
            currentActorCamDirsDotProd = Vector3.Dot((currentActorVirtualPos + currentActorPosOffset).normalized, camVirtualDir);
            if (currentActorCamDirsDotProd <= horizActorCamDirsDotProd)
            {
                horizActorCamDirsDotProd = currentActorCamDirsDotProd;
                horizActor = currentActor;
                horizActorVirtualPos = currentActorVirtualPos;
                horizActorPos = currentActor.transform.position;
            }

            // Draw current actor
            currentActor.DrawGizmos(Color.gray);
        }

        // Draw vertically farthest actor and its distance from camera
        vertiActor.DrawGizmos(Color.white);
        Gizmos.DrawRay(from: camPos, direction: vertiActorPos - camPos);

        // Draw horizontally farthest actor and its distance from camera
        horizActor.DrawGizmos(Color.cyan);
        Gizmos.DrawRay(from: camPos, direction: horizActorPos - camPos);

        // Adjust camera Vertical FOV
        float vertiFov = GetAdjustedFov(camVirtualDir, vertiActorVirtualPos)
            + GetAdjustedFovOffset(vertiActorVirtualPos, vertiActor.Radius);

        // Adjust camera Horizontal FOV
        float horizFov = GetAdjustedFov(camVirtualDir, horizActorVirtualPos)
            + GetAdjustedFovOffset(horizActorVirtualPos, horizActor.Radius);
        float vertiFovFromHorizFov = GetVerticalFov(horizFov, cam.aspect);

        // Set camera FOV (Supports setting vertical FOV only)
        cam.fieldOfView = Mathf.Max(vertiFov, vertiFovFromHorizFov);
    }

    //----------------------------------------------------------------------------------------------------

    /// <summary>
    /// Swizzles a Vector3 into a Vector2 by selecting two components from the original vector.
    /// </summary>
    /// <param name="vector">The Vector3 to swizzle.</param>
    /// <param name="xComponent">
    /// The component to use for the x value of the Vector2. Can be "x", "y" or "z".
    /// </param>
    /// <param name="yComponent">
    /// The component to use for the y value of the Vector2. Can be "x", "y" or "z".
    /// </param>
    /// <returns>A new Vector2 with the selected components from the Vector3.</returns>
    public static Vector2 Swizzle(Vector3 vector, string xComponent, string yComponent)
    {
        static float GetValue(Vector3 vector, string component)
        {
            return component.Trim().ToLower() switch
            {
                "x" => vector.x,
                "y" => vector.y,
                "z" => vector.z,
                _ => throw new System.ArgumentException($"Invalid component: {component}"),
            };
        }
        float xValue = GetValue(vector, xComponent);
        float yValue = GetValue(vector, yComponent);
        return new(xValue, yValue);
    }

    /// <summary>
    /// Calculates the adjusted field of view for a camera based on its direction and the position
    /// of an actor.
    /// </summary>
    /// <param name="cameraDirection">The normalized direction vector of the camera.</param>
    /// <param name="actorPosition">The position vector of the actor.</param>
    /// <returns>The adjusted field of view in degrees.</returns>
    private static float GetAdjustedFov(Vector2 cameraDirection, Vector2 actorPosition)
        => 2f * Mathf.Acos(Vector2.Dot(cameraDirection, actorPosition.normalized)) * Mathf.Rad2Deg;

    /// <summary>
    /// Calculates the offset angle for the field of view based on the actor's position and radius.
    /// </summary>
    /// <param name="actorPosition">The position of the actor.</param>
    /// <param name="actorRadius">The radius of the actor's bounding sphere.</param>
    /// <returns>The offset angle in degrees that should be added to the camera's field of view.</returns>
    private static float GetAdjustedFovOffset(Vector2 actorPosition, float actorRadius)
        => 2f * Mathf.Asin(actorRadius / actorPosition.magnitude) * Mathf.Rad2Deg;

    /// <summary>
    /// Calculates the vertical field of view (FOV) in degrees from the horizontal FOV and the
    /// aspect ratio.
    /// </summary>
    /// <param name="horizontalFov">The horizontal FOV in degrees.</param>
    /// <param name="aspectRatio">The aspect ratio of the screen (width / height).</param>
    /// <returns>The vertical FOV in degrees.</returns>
    private static float GetVerticalFov(float horizontalFov, float aspectRatio)
        => 2f * Mathf.Atan(Mathf.Tan(0.5f * horizontalFov * Mathf.Deg2Rad) / aspectRatio) * Mathf.Rad2Deg;

    //----------------------------------------------------------------------------------------------------

    private void Reset()
    {
        _camInfos = new(GetComponent<Camera>())
        {
            DrawNearClipPlaneDistance = false,
            DrawFarClipPlaneDistance = false,
            DrawFrustumSides = false
        };
        LoadActors();
    }

    private void LoadActors()
    {
        try
        {
            GameObject[] actorsGameObjects = GameObject.FindGameObjectsWithTag(_actorTag);
            int length = actorsGameObjects.Length;
            _actors = new SphericActor[length];
            for (int i = 0; i < length; i++) _actors[i] = new(actorsGameObjects[i].transform);
        }
        catch { _actors = new SphericActor[0]; }
    }
}