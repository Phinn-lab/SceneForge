using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ObjectSelector : MonoBehaviour
{
    [Header("攝影機")]
    [SerializeField] private Camera mainCamera;

    [Header("可互動物件圖層")]
    [SerializeField] private LayerMask placedObjectLayer;

    [Header("拖曳控制")]
    [SerializeField] private ObjectDragController dragController;

    [Header("互動選單")]
    [SerializeField] private ObjectInteractionMenu interactionMenu;

    private void Update()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
            return;

        if (dragController != null && dragController.IsDragging)
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = mainCamera.ScreenPointToRay(mouse.position.ReadValue());

        if (Physics.Raycast(ray,out RaycastHit hit,1000f,placedObjectLayer,QueryTriggerInteraction.Collide))
        {
            PlacementFootprint footprint = hit.collider.GetComponentInParent<PlacementFootprint>();

            if (footprint != null)
            {
                footprint.Select();
                return;
            }
        }

        if (interactionMenu != null)
            interactionMenu.Hide();
    }
}