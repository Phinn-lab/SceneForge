using UnityEngine;
using UnityEngine.EventSystems;

public class DropdownItem : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private GameObject objectPrefab;
    private ObjectDragController dragController;

    private bool isDragging;

    public void Initialize(GameObject prefab,ObjectDragController controller)
    {
        objectPrefab = prefab;
        dragController = controller;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (objectPrefab == null || dragController == null)
            return;

        dragController.BeginDrag(objectPrefab, eventData);

        isDragging = dragController.IsDragging;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 提供 DragController 更新
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        isDragging = false;

        if (dragController != null)
            dragController.EndDrag(eventData);
    }

    private void OnDisable()
    {
        isDragging = false;
    }
}