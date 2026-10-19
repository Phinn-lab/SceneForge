
using UnityEngine;
using UnityEngine.EventSystems;

public class PlacementFootprint : MonoBehaviour
{
    [Header("©ñ¸m¦û¥Î½d³ò")]
    [SerializeField] private Vector3 center = new Vector3(0f, 0.5f, 0f);
    [SerializeField] private Vector3 size = new Vector3(1f, 1f, 1f);

    public Vector3 WorldCenter => transform.TransformPoint(center);

    public Vector3 HalfExtents => Vector3.Scale(size * 0.5f, Abs(transform.lossyScale));

    public Quaternion Rotation => transform.rotation;

    private static Vector3 Abs(Vector3 value)
    {
        return new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
    }

    public void Select()
    {
        if (gameObject.layer != LayerMask.NameToLayer("PlacedObject"))
            return;

        ObjectInteractionMenu interactionMenu = FindFirstObjectByType<ObjectInteractionMenu>();

        if (interactionMenu != null)
            interactionMenu.Show(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Matrix4x4 oldMatrix = Gizmos.matrix;

        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.DrawWireCube(center, size);

        Gizmos.matrix = oldMatrix;
    }
}
