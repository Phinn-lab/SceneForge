
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class ObjectDragController : MonoBehaviour, ICameraReceiver
{
    [Header("攝影機")]
    [SerializeField] private Camera mainCamera;

    [Header("地面圖層")]
    [SerializeField] private LayerMask terrainLayer;

    [Header("射線設定")]
    [SerializeField] private float raycastDistance = 1000f;

    [Header("落點指引線")]
    [SerializeField] private LineRenderer placementLine;
    [SerializeField] private float lineOffset = 0.03f;

    [Header("落點擴散效果")]
    [SerializeField] private PlacementDecalEffect placementDecalEffect;
    [SerializeField] private float decalHeightOffset = 2f;

    [Header("指引線顏色")]
    [SerializeField] private Material validLineMaterial;

    [SerializeField] private Material invalidLineMaterial;

    [Header("物件重疊檢查")]
    [SerializeField] private LayerMask placedObjectLayer;

    [Header("物件互動選單")]
    [SerializeField] private ObjectInteractionMenu interactionMenu;

    private readonly Collider[] overlapResults = new Collider[128];
    private bool isPlacementBlocked;

    private GameObject currentPrefab;
    private GameObject previewObject;

    private bool isDragging;
    private bool hasValidPosition;

    private Vector3 placementPosition;

    public bool IsDragging => isDragging;

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

        if (interactionMenu != null)
            interactionMenu.Hide();

        if (placementLine != null)
            placementLine.enabled = false;


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

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            HidePlacementPreview();
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        hasValidPosition = Physics.Raycast(ray,out RaycastHit hit,raycastDistance,terrainLayer,QueryTriggerInteraction.Ignore);

        if (!hasValidPosition)
        {
            HidePlacementPreview();
            return;
        }

        placementPosition = hit.point;

        Vector3 previewPosition = placementPosition + Vector3.up * 1.5f;

        previewObject.transform.position = previewPosition;
        previewObject.SetActive(true);

        previewObject.transform.position = placementPosition;

        isPlacementBlocked = CheckPlacementBlocked();

        previewObject.transform.position = previewPosition;

        if (placementLine != null)
        {
            placementLine.enabled = true;

            Vector3 startPosition = previewPosition;

            Vector3 endPosition = placementPosition + Vector3.up * lineOffset;

            placementLine.SetPosition(0, startPosition);
            placementLine.SetPosition(1, endPosition);

            Material lineMaterial = isPlacementBlocked ? invalidLineMaterial : validLineMaterial;

            placementLine.material = lineMaterial;
        }

        if (placementDecalEffect != null)
        {
            placementDecalEffect.transform.position = placementPosition + Vector3.up * decalHeightOffset;

            placementDecalEffect.SetPlacementValid(!isPlacementBlocked);

            placementDecalEffect.Play();
        }
    }


    public void EndDrag()
    {
        if (!isDragging)
            return;

        UpdatePreviewPosition();

        bool isOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        if (hasValidPosition &&!isPlacementBlocked &&!isOverUI)
        {
            GameObject placedObject = Instantiate(currentPrefab,placementPosition,previewObject.transform.rotation);

            SetLayerRecursively(placedObject,LayerMask.NameToLayer("PlacedObject"));
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

        if (placementLine != null)
            placementLine.enabled = false;

        if (placementDecalEffect != null)
            placementDecalEffect.Stop();
    }
    private void HidePlacementPreview()
    {
        hasValidPosition = false;
        isPlacementBlocked = false;

        if (previewObject != null)
            previewObject.SetActive(false);

        if (placementLine != null)
            placementLine.enabled = false;

        if (placementDecalEffect != null)
            placementDecalEffect.Stop();
    }

    private void SetLayerRecursively(GameObject obj,int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }


    private bool CheckPlacementBlocked()
    {
        if (previewObject == null)
            return true;

        PlacementFootprint footprint = previewObject.GetComponent<PlacementFootprint>();

        if (footprint == null)
            return true;

        Vector3 halfExtents = footprint.HalfExtents * 0.98f;

        int count = Physics.OverlapBoxNonAlloc(footprint.WorldCenter,halfExtents,overlapResults,footprint.Rotation,placedObjectLayer,QueryTriggerInteraction.Collide);

        return count > 0;
    }

    public void SetCamera(Camera camera)
    {
        mainCamera = camera;
    }
}
