
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class ObjectDragController : MonoBehaviour
{
    [Header("攝影機")]
    [SerializeField] private Camera mainCamera;

    [Header("拖曳平面")]
    [SerializeField] private float dragHeight = 5f;

    [Header("地面圖層")]
    [SerializeField] private LayerMask terrainLayer;

    [Header("射線設定")]
    [SerializeField] private float raycastDistance = 1000f;

    private GameObject currentPrefab;
    private GameObject previewObject;

    private bool isDragging;
    private bool hasValidPosition;

    private Vector3 placementPosition;

    private void Update()
    {
        if (!isDragging)
            return;

        UpdatePreviewPosition();
    }

    public void BeginDrag(GameObject prefab)
    {
        if (prefab == null || mainCamera == null)
            return;

        CancelDrag();

        currentPrefab = prefab;
        previewObject = Instantiate(prefab);

        SetLayerRecursively(previewObject, 2);

        foreach (Collider col in previewObject.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        isDragging = true;
        hasValidPosition = false;

        UpdatePreviewPosition();
    }

    private void UpdatePreviewPosition()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null || previewObject == null)
            return;

        Vector2 mousePosition = mouse.position.ReadValue();

        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        Plane dragPlane = new Plane(Vector3.up,new Vector3(0f, dragHeight, 0f));

        if (!dragPlane.Raycast(ray, out float distance))
        {
            hasValidPosition = false;
            previewObject.SetActive(false);
            return;
        }

        Vector3 dragPosition = ray.GetPoint(distance);

        previewObject.transform.position = dragPosition;
        previewObject.SetActive(true);

        Vector3 rayOrigin = dragPosition + Vector3.up * 500f;

        Ray downRay = new Ray(rayOrigin,Vector3.down);
        hasValidPosition = Physics.Raycast(downRay,out RaycastHit hit,raycastDistance,terrainLayer, QueryTriggerInteraction.Ignore);

        if (hasValidPosition)
        {
            placementPosition = hit.point;
        }
    }

    public void EndDrag()
    {
        if (!isDragging)
            return;

        UpdatePreviewPosition();

        bool isOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        if (hasValidPosition && !isOverUI)
        {
            Instantiate(currentPrefab,placementPosition,previewObject.transform.rotation);
        }

        CancelDrag();
    }

    private void CancelDrag()
    {
        if (previewObject != null)
            Destroy(previewObject);

        previewObject = null;
        currentPrefab = null;

        isDragging = false;
        hasValidPosition = false;
    }

    private void SetLayerRecursively(GameObject obj,int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}
