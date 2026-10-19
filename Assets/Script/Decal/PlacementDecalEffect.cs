using UnityEngine;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

[RequireComponent(typeof(DecalProjector))]
public class PlacementDecalEffect : MonoBehaviour
{
    [Header("擴散設定")]
    [SerializeField] private float startSize = 0.5f;
    [SerializeField] private float endSize = 3f;
    [SerializeField] private float duration = 1.2f;

    [Header("落點狀態材質")]
    [SerializeField] private Material validMaterial;
    [SerializeField] private Material invalidMaterial;

    private bool? lastValidState;

    private DecalProjector decal;
    private Sequence rippleSequence;

    private void Awake()
    {
        decal = GetComponent<DecalProjector>();

        decal.enabled = false;
    }

    public void Play()
    {
        if (rippleSequence != null && rippleSequence.IsActive() && rippleSequence.IsPlaying())
            return;

        decal.enabled = true;

        ResetEffect();

        rippleSequence = DOTween.Sequence();

        rippleSequence.Append(DOTween.To(() => decal.size.x,value =>
        {
            Vector3 size = decal.size;
            size.x = value;
            size.y = value;
            decal.size = size;
        }
        ,endSize,duration).SetEase(Ease.Linear));

        rippleSequence.Join(DOTween.To(() => decal.fadeFactor,value => decal.fadeFactor = value,0f,duration).SetEase(Ease.InQuad));

        rippleSequence.SetLoops(-1, LoopType.Restart);

        rippleSequence.OnStepComplete(ResetEffect);
    }

    public void Stop()
    {
        rippleSequence?.Kill();
        rippleSequence = null;

        decal.enabled = false;

        lastValidState = null;
    }

    private void ResetEffect()
    {
        Vector3 size = decal.size;

        size.x = startSize;
        size.y = startSize;

        decal.size = size;
        decal.fadeFactor = 1f;
    }

    private void OnDestroy()
    {
        rippleSequence?.Kill();
    }

    public void SetPlacementValid(bool isValid)
    {
        if (decal == null)
            return;

        if (lastValidState == isValid)
            return;

        lastValidState = isValid;

        decal.material = isValid ? validMaterial : invalidMaterial;
    }
}