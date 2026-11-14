using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public sealed class LaserShutdown : MonoBehaviour
{
    [SerializeField] private VerticalLaser beam;
    [SerializeField, HideInInspector] private float shutdownProgress;

    private static readonly int EndState = Animator.StringToHash("Base Layer.LaserEnd");

#if UNITY_EDITOR
    private bool previewApplied;

    private void OnEnable()
    {
        if (!Application.IsPlaying(gameObject))
            UnityEditor.EditorApplication.update += ResetAfterPreview;
    }

    private void OnDisable()
    {
        UnityEditor.EditorApplication.update -= ResetAfterPreview;
        ClearPreview();
    }

    private void ResetAfterPreview()
    {
        if (previewApplied && !UnityEditor.AnimationMode.InAnimationMode())
        {
            ClearPreview();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }
    }

    private void ClearPreview()
    {
        if (previewApplied && beam != null)
            beam.SetShutdownProgress(0f);
        previewApplied = false;
    }
#endif

    public float Play()
    {
        Animator animator = GetComponent<Animator>();
        if (beam == null || !animator.isActiveAndEnabled || !animator.HasState(0, EndState))
            return 0f;

        beam.StopDamage();
        animator.Play(EndState, 0, 0f);
        animator.Update(0f);
        return animator.GetCurrentAnimatorStateInfo(0).length;
    }

    private void OnDidApplyAnimationProperties()
    {
        if (beam == null)
            return;

#if UNITY_EDITOR
        if (!Application.IsPlaying(gameObject))
            previewApplied = true;
#endif
        beam.SetShutdownProgress(shutdownProgress);
    }
}
