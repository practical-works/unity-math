using Sirenix.OdinInspector;
using System;
using UnityEngine;

[Serializable]
public class CameraInfos
{
    [BoxGroup("Clip Plane"), ToggleLeft] public bool DrawNearClipPlaneDistance = true;
    [BoxGroup("Clip Plane"), ToggleLeft] public bool DrawFarClipPlaneDistance = true;
    [BoxGroup("Frustum"), ToggleLeft] public bool DrawFrustumNearRect = true;
    [BoxGroup("Frustum"), ToggleLeft] public bool DrawFrustumFarRect = true;
    [BoxGroup("Frustum"), ToggleLeft] public bool DrawFrustumSides = true;
    [BoxGroup("Field of View"), ToggleLeft] public bool DrawVerticalFovLines = true;
    [BoxGroup("Field of View"), ToggleLeft] public bool DrawHorizontalFovLines = true;

    //----------------------------------------------------------------------------------------------------

    [ShowInInspector, ReadOnly, PropertyOrder(-1)]
    public Camera Camera { get; set; }

    //----------------------------------------------------------------------------------------------------

    public CameraInfos(Camera camera = null) => Camera = camera;

    public void DrawGizmos()
    {
        if (!Camera) return;

        Vector3 nearClipPlaneVect = Camera.nearClipPlane * Camera.transform.forward;
        Vector3 farClipPlaneVect = Camera.farClipPlane * Camera.transform.forward;

        // Draw clip plane distances
        if (DrawNearClipPlaneDistance)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(Camera.transform.position, nearClipPlaneVect);
        }
        if (DrawFarClipPlaneDistance)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawRay(Camera.transform.position + nearClipPlaneVect, farClipPlaneVect - nearClipPlaneVect);
        }

        // Draw frustum
        if (DrawFrustumNearRect || DrawFrustumFarRect || DrawFrustumSides)
        {
            Vector3[] nearCorners = new Vector3[4];
            Vector3[] farCorners = new Vector3[4];
            Camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), Camera.nearClipPlane, Camera.MonoOrStereoscopicEye.Mono, nearCorners);
            Camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), Camera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, farCorners);
            for (int i = 0; i < 4; i++)
            {
                nearCorners[i] = Camera.transform.TransformPoint(nearCorners[i]);
                farCorners[i] = Camera.transform.TransformPoint(farCorners[i]);
            }
            for (int i = 0; i < 4; i++)
            {
                Gizmos.color = Color.red;
                if (DrawFrustumNearRect) Gizmos.DrawLine(nearCorners[i], nearCorners[(i + 1) % 4]);
                if (DrawFrustumFarRect) Gizmos.DrawLine(farCorners[i], farCorners[(i + 1) % 4]);
                if (DrawFrustumSides)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(nearCorners[i], farCorners[i]);
                }
            }
        }

        if (DrawVerticalFovLines || DrawHorizontalFovLines)
        {
            // Get vertical FOV
            float verticalFovDeg = Camera.fieldOfView;
            float verticalFovRad = verticalFovDeg * Mathf.Deg2Rad;

            // Get horizontal FOV
            float horizontalFovRad = 2f * Mathf.Atan(Camera.aspect * Mathf.Tan(0.5f * verticalFovRad));
            float horizontalFovDeg = horizontalFovRad * Mathf.Rad2Deg;

            // Get dimensions
            //float width = 2f * cam.farClipPlane * Mathf.Tan(0.5f * horizontalFovRad);
            //float height = 2f * cam.farClipPlane * Mathf.Tan(0.5f * verticalFovRad);

            // Get vertical FOV vectors
            Vector3 verticalFovVect0 = Camera.transform.rotation * (Quaternion.Euler(x: -0.5f * verticalFovDeg, 0f, 0f) * Vector3.forward);
            Vector3 verticalFovVect1 = Camera.transform.rotation * (Quaternion.Euler(x: 0.5f * verticalFovDeg, 0f, 0f) * Vector3.forward);
            float verticalFovVectMagnitude = Camera.farClipPlane / Mathf.Cos(0.5f * verticalFovRad);
            verticalFovVect0 *= verticalFovVectMagnitude;
            verticalFovVect1 *= verticalFovVectMagnitude;

            // Get horizontal FOV vectors
            Vector3 horizontalFovVect0 = Camera.transform.rotation * (Quaternion.Euler(0f, y: -0.5f * horizontalFovDeg, 0f) * Vector3.forward);
            Vector3 horizontalFovVect1 = Camera.transform.rotation * (Quaternion.Euler(0f, y: 0.5f * horizontalFovDeg, 0f) * Vector3.forward);
            float horizontalFovVectMagnitude = Camera.farClipPlane / Mathf.Cos(0.5f * horizontalFovRad);
            horizontalFovVect0 *= horizontalFovVectMagnitude;
            horizontalFovVect1 *= horizontalFovVectMagnitude;

            // Draw Vertical FOV bounds
            if (DrawVerticalFovLines)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawRay(Camera.transform.position, verticalFovVect0);
                Gizmos.DrawRay(Camera.transform.position, verticalFovVect1);
            }

            // Draw Horizontal FOV bounds
            if (DrawHorizontalFovLines)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawRay(Camera.transform.position, horizontalFovVect0);
                Gizmos.DrawRay(Camera.transform.position, horizontalFovVect1);
            }
        }
    }
}