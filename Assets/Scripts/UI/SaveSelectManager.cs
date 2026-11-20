using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[ExecuteAlways]
public sealed class SaveSelectManager : MonoBehaviour
{
    [Header("Scene navigation")]
    [SerializeField] private string firstLevel = "Level01_TorreDelOlvido";
    [SerializeField] private string mainMenu = "MainMenu";
    [SerializeField] private Material transitionMaterial;
    [Header("Presentation preview only; no save data is written")]
    [SerializeField] private bool previewExampleRecords;
    [Header("UI")]
    [SerializeField] private SaveSlotView[] slots = Array.Empty<SaveSlotView>();
    [SerializeField] private Button backButton;
    [SerializeField] private Text feedback;

    private int selectedSlot;
    private bool busy;
    private bool lastPreview;
    private float feedbackUntil;

    private void Awake()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Application.isPlaying && Array.IndexOf(Environment.GetCommandLineArgs(), "--save-ui-preview") >= 0)
            previewExampleRecords = true;
#endif
    }

    private void OnEnable()
    {
        if (Ready())
            RefreshPresentation();
    }

    private void Start()
    {
        if (Application.isPlaying && Ready())
            slots[selectedSlot].Select();
    }

    private void Update()
    {
        if (!Ready())
            return;
        if (previewExampleRecords != lastPreview)
            RefreshPresentation();
        if (!Application.isPlaying)
            return;
        if (!busy && Input.GetKeyDown(KeyCode.Escape))
            Back();
        if (!busy && EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
            slots[selectedSlot].Select();
        if (feedbackUntil > 0f && Time.unscaledTime >= feedbackUntil && !busy)
        {
            feedbackUntil = 0f;
            feedback.text = IdleMessage;
        }
    }

    public void Configure(SaveSlotView[] views, Button back, Text message, Material transition)
    {
        slots = views;
        backButton = back;
        feedback = message;
        transitionMaterial = transition;
        RefreshPresentation();
    }

    public void RefreshPresentation()
    {
        if (!Ready())
            return;
        lastPreview = previewExampleRecords;
        for (int index = 0; index < slots.Length; index++)
            slots[index].Present(previewExampleRecords && index < 2 ? ArchiveSlotPresentation.Example(index) : ArchiveSlotPresentation.Empty());
        feedback.text = IdleMessage;
        FocusSlot(selectedSlot);
    }

    // Bind future storage results here without adding storage behavior to the UI.
    public void SetSlot(int index, ArchiveSlotPresentation presentation)
    {
        if (index < 0 || index >= slots.Length)
            throw new ArgumentOutOfRangeException(nameof(index));
        slots[index].Present(presentation);
    }

    public void FocusSlot(int index)
    {
        if (busy || index < 0 || index >= slots.Length)
            return;
        selectedSlot = index;
        for (int item = 0; item < slots.Length; item++)
            slots[item].SetFocused(item == index);
    }

    public void ActivateSlot(int index)
    {
        if (busy || index < 0 || index >= slots.Length)
            return;
        FocusSlot(index);
        if (slots[index].Data.occupied)
        {
            feedback.text = "RECORD " + (index + 1).ToString("00") + " SELECTED // UI PREVIEW";
            feedbackUntil = Time.unscaledTime + 2f;
            return;
        }
        if (!Application.CanStreamedLevelBeLoaded(firstLevel))
        {
            feedback.text = "DESTINATION UNAVAILABLE";
            return;
        }
        busy = true;
        SetInteractable(false);
        feedback.text = "TIMELINE " + (index + 1).ToString("00") + " // INITIALIZING";
        StartCoroutine(EnterLevel(index));
    }

    private IEnumerator EnterLevel(int index)
    {
        Time.timeScale = 1f;
        var loading = SceneManager.LoadSceneAsync(firstLevel);
        loading.allowSceneActivation = false;
        while (loading.progress < .9f)
            yield return null;
        yield return new WaitForEndOfFrame();
        Texture2D departure = ScreenCapture.CaptureScreenshotAsTexture();
        var card = (RectTransform)slots[index].transform;
        var focusObject = new GameObject("Archive Entry Focus", typeof(RectTransform));
        var focus = (RectTransform)focusObject.transform;
        focus.position = card.TransformPoint(card.rect.center);
        var transition = ChronoSceneTransition.Create(transitionMaterial, departure, focus);
        Destroy(focusObject);
        yield return transition.Cover();
        transition.Activate(loading);
    }

    public void Back()
    {
        if (busy)
            return;
        if (!Application.CanStreamedLevelBeLoaded(mainMenu))
        {
            feedback.text = "MAIN MENU UNAVAILABLE";
            return;
        }
        busy = true;
        SetInteractable(false);
        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync(mainMenu);
    }

    private void SetInteractable(bool value)
    {
        foreach (SaveSlotView slot in slots)
            slot.interactable = value;
        backButton.interactable = value;
    }

    private string IdleMessage => previewExampleRecords ? "UI PREVIEW // EXAMPLE RECORDS" : "CHOOSE A TIMELINE";

    private bool Ready() => slots.Length == 3 && slots[0] != null && slots[1] != null && slots[2] != null && feedback != null;
}
