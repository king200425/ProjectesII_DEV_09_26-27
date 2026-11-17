using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class AftertimeMenuButton : Button
{
    [SerializeField] private RawImage artwork;
    [SerializeField] private Rect idle;
    [SerializeField] private Rect selected;

    public void SetArtwork(RawImage image, Rect idleRegion, Rect selectedRegion)
    {
        artwork = image;
        idle = idleRegion;
        selected = selectedRegion;
        ApplyArtwork(SelectionState.Normal);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        if (IsInteractable() && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(gameObject, eventData);
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);
        ApplyArtwork(state);
    }

    private void ApplyArtwork(SelectionState state)
    {
        if (artwork == null)
            return;

        bool focused = state == SelectionState.Selected || state == SelectionState.Pressed;
        artwork.uvRect = focused ? selected : idle;
        artwork.rectTransform.anchoredPosition = state == SelectionState.Pressed ? new Vector2(4f, -4f) : Vector2.zero;
    }
}
