
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class DropdownUI : MonoBehaviour
{
    [System.Serializable]
    public class DropdownItemData
    {
        public string itemName;
        public GameObject prefab;
    }

    [Header("UI")]
    [SerializeField] private Button headerButton;
    [SerializeField] private RectTransform dropdownPanel;
    [SerializeField] private RectTransform arrow;

    [Header("道具清單")]
    [SerializeField] private Transform content;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private List<DropdownItemData> items = new();

    [Header("設定")]
    [SerializeField] private bool defaultExpanded = false;
    [SerializeField] private float animationDuration = 0.2f;

    [Header("3D 拖曳控制")]
    [SerializeField]
    private ObjectDragController dragController;

    private bool isExpanded;
    private Tween panelTween;

    private void Awake()
    {
        isExpanded = defaultExpanded;

        headerButton.onClick.AddListener(ToggleDropdown);

        CreateItems();

        dropdownPanel.gameObject.SetActive(isExpanded);

        dropdownPanel.localScale = isExpanded ? Vector3.one : new Vector3(1f, 0f, 1f);

        UpdateArrow();
    }



    private void CreateItems()
    {
        foreach (DropdownItemData data in items)
        {
            GameObject item = Instantiate(itemPrefab,content);

            TMP_Text text = item.GetComponentInChildren<TMP_Text>();

            if (text != null)
                text.text = data.itemName;

            DropdownItem dropdownItem = item.GetComponent<DropdownItem>();

            if (dropdownItem != null)
            {
                dropdownItem.Initialize(data.prefab,dragController);
            }
        }
    }


    public void ToggleDropdown()
    {
        SetExpanded(!isExpanded);
    }

    public void SetExpanded(bool expanded)
    {
        if (isExpanded == expanded)
            return;

        isExpanded = expanded;

        panelTween?.Kill();

        UpdateArrow();

        if (expanded)
            dropdownPanel.gameObject.SetActive(true);

        panelTween = dropdownPanel.DOScaleY(expanded ? 1f : 0f, animationDuration).SetEase(Ease.OutCubic).OnComplete(() =>
            {
                if (!isExpanded)
                    dropdownPanel.gameObject.SetActive(false);
            });
    }

    private void UpdateArrow()
    {
        arrow.localRotation = Quaternion.Euler(0f, 0f, isExpanded ? 180f : 0f );
    }

    private void OnDestroy()
    {
        panelTween?.Kill();

        if (headerButton != null)
            headerButton.onClick.RemoveListener(ToggleDropdown);
    }
}
