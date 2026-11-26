using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 按钮根据选中状态切换图片区域，点击时轻微偏移。
public sealed class AftertimeMenuButton : Button
{
    [SerializeField] private RawImage artwork;
    [SerializeField] private Rect idle;
    [SerializeField] private Rect selected;

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
        if (focused)
            artwork.uvRect = selected;
        else
            artwork.uvRect = idle;

        artwork.rectTransform.anchoredPosition = Vector2.zero;
        if (state == SelectionState.Pressed)
            artwork.rectTransform.anchoredPosition = new Vector2(4f, -4f);
    }
}
