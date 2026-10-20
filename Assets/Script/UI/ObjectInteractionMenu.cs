
using UnityEngine;
using UnityEngine.UI;

public class ObjectInteractionMenu : MonoBehaviour, ICameraReceiver
{
    [Header("Canvas")]
    [SerializeField] private Canvas interactionCanvas;
    [SerializeField] private Button deleteButton;

    [Header("攝影機")]
    [SerializeField] private Camera mainCamera;

    [Header("選單位置")]
    [SerializeField] private float horizontalOffset = 2.5f;
    [SerializeField] private float verticalOffset = 1.2f;

    private GameObject selectedObject;

    private void Awake()
    {
        interactionCanvas.gameObject.SetActive(false);

        deleteButton.onClick.AddListener(DeleteSelectedObject);
    }

    private void LateUpdate()
    {
        if (selectedObject == null)
            return;

        UpdateMenuPosition();
    }

    public void Show(GameObject target)
    {
        if (target == null)
            return;

        selectedObject = target;

        interactionCanvas.gameObject.SetActive(true);

        UpdateMenuPosition();
    }

    public void Hide()
    {
        selectedObject = null;

        interactionCanvas.gameObject.SetActive(false);
    }

    private void UpdateMenuPosition()
    {
        if (selectedObject == null)
        {
            Hide();
            return;
        }

        PlacementFootprint footprint = selectedObject.GetComponent<PlacementFootprint>();

        Vector3 targetPosition = footprint != null? footprint.WorldCenter : selectedObject.transform.position;

        Vector3 offset = mainCamera.transform.right * horizontalOffset + Vector3.up * verticalOffset;

        interactionCanvas.transform.position = targetPosition + offset;

        interactionCanvas.transform.rotation = mainCamera.transform.rotation;
    }

    private void DeleteSelectedObject()
    {
        if (selectedObject == null)
            return;

        GameObject target = selectedObject;

        Hide();

        Destroy(target);
    }

    private void OnDestroy()
    {
        if (deleteButton != null)
            deleteButton.onClick.RemoveListener(DeleteSelectedObject);
    }
    public void SetCamera(Camera camera)
    {
        mainCamera = camera;
    }
}
