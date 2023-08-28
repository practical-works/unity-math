using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Mathf;

public class BudgetCutsInventory : MonoBehaviour
{
    [SerializeField, Min(0.05f)]
    private float _arcRadius = 0.5f;

    [SerializeField, InlineEditor(InlineEditorModes.GUIAndPreview)]
    [ListDrawerSettings(CustomAddFunction = nameof(CreateItem), CustomRemoveElementFunction = nameof(RemoveItem))]
    private List<Item> _items = new();

    [SerializeField, Range(0f, 100f)]
    private float _animationSpeed = 3f;

    //----------------------------------------------------------------------------------------------------

    private void Update()
    {
        Control();
        PlaceItems(gizmos: false, smooth: true);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;
        using (new Handles.DrawingScope(transform.localToWorldMatrix)) PlaceItems(gizmos: true);
    }

    private void Reset() => _items = GetComponentsInChildren<Item>().Where(item => item.gameObject != gameObject).ToList();

    //----------------------------------------------------------------------------------------------------

    public void PlaceItems(bool gizmos = false, bool smooth = false)
    {
        if (gizmos)
        {
            Handles.DrawWireArc(default, Vector3.forward, Vector3.up, 45f, _arcRadius, 2f);
            Handles.DrawWireArc(default, Vector3.forward, Vector3.up, -45f, _arcRadius, 2f);
            Handles.color = Color.cyan;
        }

        if (_items == null || _items.Count == 0) return;

        float angleOffset = 0f; for (int i = 0; i < _items.Count; i++) if (i > 0) angleOffset +=
        Asin(_items[i].Radius / _arcRadius);

        float angle = -angleOffset; for (int i = 0; i < _items.Count; i++)
        {
            Vector3 center = _arcRadius * new Vector3(Sin(angle), Cos(angle), 0f);
            if (gizmos) Handles.DrawWireDisc(center, Vector3.forward, _items[i].Radius, 2f);

            if (center.magnitude > Vector3.kEpsilon)
            {
                Transform itemTransform = _items[i].transform;
                if (smooth)
                {
                    float time = (_animationSpeed <= 0f) ? 1f : _animationSpeed * Time.deltaTime;
                    Vector3 centerInWorld = transform.TransformPoint(center);
                    itemTransform.position = Vector3.Lerp(itemTransform.position, centerInWorld, time);
                    itemTransform.up = Vector3.Lerp(itemTransform.up, centerInWorld, time);
                }
                else
                {
                    Vector3 centerInWorld =
                    transform.TransformPoint(center); itemTransform.position = centerInWorld; itemTransform.up =
                    centerInWorld;
                }
            }

            if (i < _items.Count - 1)
                angle += GetAngleUsingLawOfCosines(_arcRadius, _arcRadius, _items[i].Radius + _items[i + 1].Radius);
        }
    }

    public Item CreateItem()
    {
        PrimitiveType[] primitiveTypes = new PrimitiveType[]
        {
            PrimitiveType.Cube,
            PrimitiveType.Sphere,
            PrimitiveType.Cylinder,
            PrimitiveType.Capsule
        };
        PrimitiveType randomType = primitiveTypes[Random.Range(0, primitiveTypes.Length)];
        GameObject primitiveGameObject = GameObject.CreatePrimitive(randomType);
        Material material = new(Shader.Find("Standard")) { color = new(Random.value, Random.value, Random.value) };
        primitiveGameObject.GetComponent<Renderer>().material = material;
        primitiveGameObject.transform.position = Vector3.zero;
        primitiveGameObject.transform.SetParent(transform);
        Item item = primitiveGameObject.AddComponent<Item>();
        item.name = $"{item.name} Item";
        item.Radius = Random.Range(0.05f, 0.2f);
        return item;
    }

    public void RemoveItem(Item item)
    {
        if (_items.Count == 0 || item == null) return;
        GameObject itemGameObject = item.transform.gameObject;
        if (itemGameObject)
        {
            if (Application.isPlaying) Destroy(itemGameObject);
            else DestroyImmediate(itemGameObject);
        }
        _items.Remove(item);
    }

    private void Control()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        float keyX = Input.GetAxis("Horizontal") * Time.deltaTime;
        float keyY = Input.GetAxis("Vertical") * Time.deltaTime;

        Transform camTransform = Camera.main.transform; camTransform.position += keyY * new
        Vector3(camTransform.forward.x, 0f, camTransform.forward.z); camTransform.rotation *=
        Quaternion.Euler(-mouseY, (100f * keyX) + mouseX, 0f);

        if (Input.GetButtonDown("Fire1")) _items.Add(CreateItem());
        if (Input.GetButtonDown("Fire2") && _items.Count > 0) RemoveItem(_items[Random.Range(0, _items.Count - 1)]);
    }

    private float GetAngleUsingLawOfCosines(float adjacent0, float adjacent1, float opposite)
        => Acos(((adjacent0 * adjacent0) + (adjacent1 * adjacent1) - (opposite * opposite)) / (2f * adjacent0 * adjacent1));
}