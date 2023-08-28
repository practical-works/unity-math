using Sirenix.OdinInspector;
using System;
using UnityEngine;

[Serializable]
public class Item : MonoBehaviour
{
    [ShowInInspector, PropertyRange(0.005f, 0.5f)]
    public float Radius
    {
        get => transform.localScale.magnitude;
        set
        {
            Vector3 normalizedLocalScale = transform.localScale.normalized;
            if (normalizedLocalScale == default) normalizedLocalScale = Vector3.one;
            transform.localScale = value * normalizedLocalScale;
        }
    }
}