using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ObjectDragController : MonoBehaviour, ICameraReceiver
{
    [Header("攝影機")]
    [SerializeField] private Camera mainCamera;

    [Header("VR 右手射線")]
    [SerializeField] private NearFarInteractor rightNearFarInteractor;

    [Header("VR 射線目標")]
    [SerializeField] private GameObject dragRayTarget;

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

    private int activePointerId = -1;
    private bool dragStartedInVR;
    private bool pointerOverUI;

    public bool IsDragging => isDragging;

    private void Update()
    {
        if (!isDragging)
            return;

        if (ModeManager.Instance != null && ModeManager.Instance.IsVRMode != dragStartedInVR)
        {
            CancelDrag();
            return;
        }

        UpdatePreviewPosition();
    }

    public void BeginDrag(GameObject prefab,PointerEventData eventData)
    {
        if (prefab == null)
            return;

        CancelDrag();

        dragStartedInVR = ModeManager.Instance != null && ModeManager.Instance.IsVRMode;

        if (dragStartedInVR)
        {
            if (rightNearFarInteractor == null)
                return;
        }
        else
        {
            if (mainCamera == null)
                return;
        }

        if (interactionMenu != null)
            interactionMenu.Hide();

        currentPrefab = prefab;
        previewObject = Instantiate(prefab);

        SetLayerRecursively(previewObject, 2);

        foreach (Collider col in previewObject.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        activePointerId = eventData.pointerId;
        pointerOverUI = false;

        isDragging = true;
        hasValidPosition = false;

        UpdatePreviewPosition();
    }

    public void UpdateDragPointer(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        if (eventData.pointerId != activePointerId)
            return;

        pointerOverUI = IsPointerOverUI(eventData);
    }

    private bool IsPointerOverUI(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject != null)
            return true;

        return false;
    }

    private void UpdatePreviewPosition()
    {
        if (previewObject == null)
            return;

        Ray ray;

        if (dragStartedInVR)
        {
            if (rightNearFarInteractor == null)
            {
                HidePlacementPreview();
                return;
            }

            Transform rayOrigin = rightNearFarInteractor.curveOrigin;

            if (rayOrigin == null)
            {
                HidePlacementPreview();
                return;
            }

            ray = new Ray(rayOrigin.position,rayOrigin.forward);
        }
        else
        {
            Mouse mouse = Mouse.current;

            if (mouse == null || mainCamera == null)
            {
                HidePlacementPreview();
                return;
            }

            Vector2 mousePosition = mouse.position.ReadValue();

            ray = mainCamera.ScreenPointToRay(mousePosition);
        }

        if (IsPointerOverUI())
        {
            HidePlacementPreview();
            return;
        }

        hasValidPosition = Physics.Raycast( ray, out RaycastHit hit, raycastDistance, terrainLayer, QueryTriggerInteraction.Ignore);

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

            placementLine.SetPosition(0, previewPosition);

            placementLine.SetPosition(1, placementPosition + Vector3.up * lineOffset);

            placementLine.material = isPlacementBlocked ? invalidLineMaterial : validLineMaterial;
        }

        if (placementDecalEffect != null)
        {
            placementDecalEffect.transform.position = placementPosition + Vector3.up * decalHeightOffset;

            placementDecalEffect.SetPlacementValid(!isPlacementBlocked);

            placementDecalEffect.Play();
        }

        if (dragStartedInVR && dragRayTarget != null)
        {
            dragRayTarget.transform.position = placementPosition;
            dragRayTarget.SetActive(true);
        }
    }

    public void EndDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        if (eventData.pointerId != activePointerId)
            return;

        UpdatePreviewPosition();

        bool isOverUI = IsPointerOverUI();

        if (hasValidPosition &&!isPlacementBlocked &&!isOverUI)
        {
            GameObject placedObject = Instantiate(currentPrefab,placementPosition,previewObject.transform.rotation);

            SetLayerRecursively(placedObject,LayerMask.NameToLayer("PlacedObject"));
        }

        CancelDrag();
    }

    public void CancelDrag()
    {
        if (previewObject != null)
            Destroy(previewObject);

        previewObject = null;
        currentPrefab = null;

        isDragging = false;
        hasValidPosition = false;
        isPlacementBlocked = false;

        activePointerId = -1;
        pointerOverUI = false;

        if (placementLine != null)
            placementLine.enabled = false;

        if (placementDecalEffect != null)
            placementDecalEffect.Stop();

        if (dragRayTarget != null)
            dragRayTarget.SetActive(false);
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        if (dragStartedInVR)
            return IsVRPointerOverUI();

        return EventSystem.current.IsPointerOverGameObject();
    }

    private bool IsVRPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        if (activePointerId == -1)
            return false;

        return EventSystem.current.IsPointerOverGameObject(activePointerId);
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

        if (dragRayTarget != null)
            dragRayTarget.SetActive(false);
    }

    private void SetLayerRecursively(GameObject obj, int layer)
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