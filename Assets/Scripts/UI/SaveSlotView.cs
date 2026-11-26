using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class SaveSlotView : Selectable, IPointerClickHandler, ISubmitHandler
{
    [SerializeField] private SaveSelectManager owner;
    [SerializeField] private int slotIndex;
    [SerializeField] private ArchiveGraphic artwork;
    [SerializeField] private Text heading;
    [SerializeField] private Text state;
    [SerializeField] private Text activeTag;
    [SerializeField] private Text action;
    [SerializeField] private GameObject emptyContent;
    [SerializeField] private GameObject occupiedContent;
    [SerializeField] private Text unitName;
    [SerializeField] private Text playTime;
    [SerializeField] private Text level;
    [SerializeField] private Text checkpoint;
    [SerializeField] private Text convergence;
    [SerializeField] private Text lastRecord;

    public ArchiveSlotPresentation Data { get; private set; } = ArchiveSlotPresentation.Empty();
    public int SlotIndex => slotIndex;
    private bool focused;
    private Vector2 restPosition;

    protected override void Awake()
    {
        base.Awake();
        restPosition = ((RectTransform)transform).anchoredPosition;
    }

    public void Configure(SaveSelectManager manager, int index, ArchiveGraphic graphic, Text title, Text status,
        Text tag, Text button, GameObject empty, GameObject occupied, Text unit, Text time, Text location,
        Text anchor, Text progress, Text updated)
    {
        owner = manager;
        slotIndex = index;
        artwork = graphic;
        targetGraphic = graphic;
        transition = Transition.None;
        heading = title;
        state = status;
        activeTag = tag;
        action = button;
        emptyContent = empty;
        occupiedContent = occupied;
        unitName = unit;
        playTime = time;
        level = location;
        checkpoint = anchor;
        convergence = progress;
        lastRecord = updated;
    }

    public void Present(ArchiveSlotPresentation value)
    {
        Data = value ?? ArchiveSlotPresentation.Empty();
        emptyContent.SetActive(!Data.occupied);
        occupiedContent.SetActive(Data.occupied);
        heading.text = "SLOT " + (slotIndex + 1).ToString("00");
        state.text = Data.occupied ? "CONTINUE" : "UNALLOCATED";
        action.text = Data.occupied ? "LOAD RECORD" : "+ INITIALIZE TIMELINE";
        unitName.text = Data.unitName;
        playTime.text = "TIME PLAYED: " + Data.playTime;
        level.text = Data.level;
        checkpoint.text = Data.checkpoint;
        convergence.text = Mathf.RoundToInt(Mathf.Clamp01(Data.progress) * 100f) + "%";
        lastRecord.text = Data.lastRecord;
        SetFocused(focused);
    }

    public void SetFocused(bool selected)
    {
        focused = selected;
        Color accent = Data.occupied && slotIndex % 2 == 1 ? new Color(.878f, .714f, 1f) : new Color(0f, .941f, 1f);
        heading.color = selected || Data.occupied ? accent : new Color(.52f, .58f, .59f);
        state.color = selected || Data.occupied ? accent : new Color(.38f, .44f, .45f);
        activeTag.gameObject.SetActive(selected);
        activeTag.color = new Color(.012f, .028f, .04f);
        action.color = selected ? new Color(.012f, .028f, .04f) : new Color(.86f, .9f, .94f);
        artwork.SetState(Data.occupied, selected, Data.progress);
    }

    private void Update()
    {
        if (!Application.isPlaying)
            return;
        var rect = (RectTransform)transform;
        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, restPosition + Vector2.up * (focused ? 5f : 0f), 1f - Mathf.Exp(-Time.unscaledDeltaTime * 16f));
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        if (IsInteractable() && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(gameObject, eventData);
    }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        if (owner != null)
            owner.FocusSlot(slotIndex);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || !IsInteractable())
            return;
        var rect = (RectTransform)transform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, eventData.position, eventData.pressEventCamera, out Vector2 local);
        owner.FocusSlot(slotIndex);
        if (local.y <= rect.rect.yMin + 80f)
            owner.ActivateSlot(slotIndex);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (IsInteractable())
            owner.ActivateSlot(slotIndex);
    }
}
