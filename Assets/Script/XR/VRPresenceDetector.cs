
using System;
using UnityEngine;
using UnityEngine.XR;

public class VRPresenceDetector : MonoBehaviour
{
    [Header("°»´ú³]©w")]
    [SerializeField] private float checkInterval = 0.1f;

    [SerializeField] private float stableDuration = 1f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLog = true;

    public bool IsWearing { get; private set; }

    public event Action<bool> OnPresenceChanged;

    private InputDevice headset;

    private float nextCheckTime;
    private float pendingStartTime;

    private bool? pendingPresence;
    private bool hasConfirmedState;

    private void Update()
    {
        if (Time.unscaledTime < nextCheckTime)
            return;

        nextCheckTime = Time.unscaledTime + checkInterval;

        if (!headset.isValid)
        {
            headset = InputDevices.GetDeviceAtXRNode(XRNode.Head);
        }

        if (!headset.isValid)
        {
            ResetPending();
            return;
        }

        if (!headset.TryGetFeatureValue(CommonUsages.trackingState,out InputTrackingState trackingState))
        {
            ResetPending();
            return;
        }

        bool detectedWearing = (trackingState & InputTrackingState.Velocity) != 0;

        if (pendingPresence != detectedWearing)
        {
            pendingPresence = detectedWearing;
            pendingStartTime = Time.unscaledTime;
        }

        if (Time.unscaledTime - pendingStartTime < stableDuration)
            return;

        if (hasConfirmedState && IsWearing == detectedWearing)
            return;

        hasConfirmedState = true;
        IsWearing = detectedWearing;

        OnPresenceChanged?.Invoke(IsWearing);
    }

    private void ResetPending()
    {
        pendingPresence = null;
    }
}
