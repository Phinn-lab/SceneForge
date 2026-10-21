
using System;
using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.InputSystem.UI;

public class ModeManager : MonoBehaviour
{
    public static ModeManager Instance { get; private set; }

    public enum ControlMode
    {
        PC,
        VR
    }

    [Header("頭盔佩戴偵測")]
    [SerializeField] private VRPresenceDetector presenceDetector;

    [Header("PC 攝影機")]
    [SerializeField] private Camera pcCamera;

    [Header("VR")]
    [SerializeField] private XROrigin xrOrigin;
    [SerializeField] private Camera vrCamera;

    [Header("需要切換 Camera 的腳本")]
    [SerializeField] private MonoBehaviour[] cameraReceivers;

    [Header("位置同步")]
    [SerializeField] private bool syncPosition = true;

    [SerializeField] private bool syncRotation = true;

    [SerializeField] private ObjectDragController dragController;

    [SerializeField] private InputSystemUIInputModule pcInputModule;

    public ControlMode CurrentMode { get; private set; } = ControlMode.PC;

    public bool IsVRMode => CurrentMode == ControlMode.VR;

    public Camera CurrentCamera => IsVRMode ? vrCamera : pcCamera;

    public event Action<Camera> OnCameraChanged;
    public event Action<ControlMode> OnModeChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (presenceDetector != null)
        {
            presenceDetector.OnPresenceChanged += OnPresenceChanged;
        }

        ApplyMode(ControlMode.PC, false);

        if (presenceDetector != null && presenceDetector.IsWearing)
        {
            SwitchMode(ControlMode.VR);
        }
    }

    private void OnDestroy()
    {
        if (presenceDetector != null)
        {
            presenceDetector.OnPresenceChanged -= OnPresenceChanged;
        }

        if (Instance == this)
            Instance = null;
    }

    private void OnPresenceChanged(bool isWearing)
    {
        SwitchMode(isWearing ? ControlMode.VR : ControlMode.PC);
    }

    public void SwitchMode(ControlMode mode)
    {
        if (CurrentMode == mode)
            return;

        ApplyMode(mode, true);
    }

    private void ApplyMode(ControlMode mode, bool synchronize)
    {
        if (dragController != null && dragController.IsDragging)
        {
            dragController.CancelDrag();
        }

        bool isVR = mode == ControlMode.VR;

        if (synchronize)
        {
            if (isVR)
                SyncPCToVR();
            else
                SyncVRToPC();
        }

        if (pcCamera != null)
        {
            pcCamera.gameObject.SetActive(!isVR);
        }

        if (pcInputModule != null)
        {
            pcInputModule.enabled = !isVR;
        }

        if (vrCamera != null)
        {
            vrCamera.gameObject.SetActive(isVR);
        }

        CurrentMode = mode;

        UpdateCameraReceivers();

        OnCameraChanged?.Invoke(CurrentCamera);
        OnModeChanged?.Invoke(CurrentMode);
    }

    private void SyncPCToVR()
    {
        if (pcCamera == null || xrOrigin == null)
            return;

        if (syncPosition)
        {
            xrOrigin.MoveCameraToWorldLocation(pcCamera.transform.position);
        }

        if (syncRotation)
        {
            Vector3 pcForward = pcCamera.transform.forward;
            pcForward.y = 0f;

            if (pcForward.sqrMagnitude > 0.001f)
            {
                xrOrigin.MatchOriginUpCameraForward(Vector3.up,pcForward.normalized);
            }
        }
    }

    private void SyncVRToPC()
    {
        if (pcCamera == null || vrCamera == null)
            return;

        if (syncPosition)
        {
            pcCamera.transform.position = vrCamera.transform.position;
        }
    }

    private void UpdateCameraReceivers()
    {
        Camera targetCamera = CurrentCamera;

        if (cameraReceivers == null)
            return;

        foreach (MonoBehaviour component in cameraReceivers)
        {
            if (component == null)
                continue;

            if (component is ICameraReceiver receiver)
            {
                receiver.SetCamera(targetCamera);
            }
        }
    }
}
