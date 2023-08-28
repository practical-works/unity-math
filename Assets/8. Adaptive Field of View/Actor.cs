using Sirenix.OdinInspector;
using System;
using UnityEngine;

[Serializable]
public class SphericActor
{
    [HideInInspector] public Transform transform;
    [Range(0f, 5f)] public float Radius = 0.3f;

    //----------------------------------------------------------------------------------------------------

    [ShowInInspector, DisplayAsString, EnableGUI] public string Name => transform ? transform.name : "";

    //----------------------------------------------------------------------------------------------------

    public SphericActor(Transform transform) => this.transform = transform;

    public void DrawGizmos(Color color = default)
    {
        if (!transform) return;
        Gizmos.color = color == default ? Color.white : color;
        Gizmos.DrawWireSphere(transform.position, Radius);
    }
}