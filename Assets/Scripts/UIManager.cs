using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Elements")]
    public GameObject carouselPanel;   // главный контейнер (панель) для слайдов
    public Transform slidesContainer;  // контейнер внутри панели, куда будут спавниться слайды
    public Button nextButton;
    public Button prevButton;

    [Header("Popup Anim")]
    public CanvasGroup panelGroup;     // CanvasGroup на carouselPanel
    public float inDuration = 0.25f;
    public float outDuration = 0.20f;
    public float startScale = 0.85f;
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("State")]
    public bool isPanelOpen = false; // true — если панель открыта

    private List<GameObject> activeSlides = new List<GameObject>();
    private int currentIndex = 0;
    private Coroutine popupCo;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (nextButton != null) nextButton.onClick.AddListener(ShowNextSlide);
        if (prevButton != null) prevButton.onClick.AddListener(ShowPrevSlide);

        if (carouselPanel != null)
        {
            carouselPanel.SetActive(false);
            if (panelGroup == null) panelGroup = carouselPanel.GetComponent<CanvasGroup>();
            if (panelGroup == null) panelGroup = carouselPanel.AddComponent<CanvasGroup>();
            panelGroup.alpha = 0f;
        }
    }

    /// <summary>
    /// Открыть панель слайдов и загрузить префабы (с анимацией)
    /// </summary>
    public void OpenPanel(List<GameObject> slidePrefabs)
    {
        // очистка старых
        foreach (var go in activeSlides) Destroy(go);
        activeSlides.Clear();

        // инстансим новые
        if (slidePrefabs != null)
        {
            foreach (var prefab in slidePrefabs)
            {
                var panel = Instantiate(prefab, slidesContainer);
                panel.SetActive(false);
                activeSlides.Add(panel);
            }
        }

        currentIndex = 0;
        if (activeSlides.Count > 0)
            activeSlides[currentIndex].SetActive(true);

        UpdateButtons();

        // анимация открытия
        if (popupCo != null) StopCoroutine(popupCo);
        popupCo = StartCoroutine(AnimateOpen());
    }

    /// <summary>
    /// Закрыть панель (с анимацией) и очистить слайды
    /// </summary>
    public void ClosePanel()
    {
        if (!isPanelOpen) return;
        if (popupCo != null) StopCoroutine(popupCo);
        popupCo = StartCoroutine(AnimateClose());
    }

    private IEnumerator AnimateOpen()
    {
        isPanelOpen = true;
        carouselPanel.SetActive(true);
        panelGroup.blocksRaycasts = false;

        float t = 0f;
        var rt = (RectTransform)carouselPanel.transform;
        Vector3 from = Vector3.one * startScale;
        Vector3 to = Vector3.one;

        rt.localScale = from;
        panelGroup.alpha = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / inDuration;
            float k = ease.Evaluate(Mathf.Clamp01(t));
            rt.localScale = Vector3.LerpUnclamped(from, to, k);
            panelGroup.alpha = k;
            yield return null;
        }

        rt.localScale = to;
        panelGroup.alpha = 1f;
        panelGroup.blocksRaycasts = true;
        popupCo = null;
    }

    private IEnumerator AnimateClose()
    {
        panelGroup.blocksRaycasts = false;

        float t = 0f;
        var rt = (RectTransform)carouselPanel.transform;
        Vector3 from = rt.localScale;
        Vector3 to = Vector3.one * startScale;
        float a0 = panelGroup.alpha;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / outDuration;
            float k = ease.Evaluate(Mathf.Clamp01(t));
            rt.localScale = Vector3.LerpUnclamped(from, to, k);
            panelGroup.alpha = Mathf.Lerp(a0, 0f, k);
            yield return null;
        }
        panelGroup.alpha = 0f;
        carouselPanel.SetActive(false);
        isPanelOpen = false;

        // чистим слайды
        foreach (var go in activeSlides) Destroy(go);
        activeSlides.Clear();

        popupCo = null;
    }

    private void ShowNextSlide()
    {
        if (activeSlides.Count == 0) return;
        activeSlides[currentIndex].SetActive(false);
        currentIndex = Mathf.Min(currentIndex + 1, activeSlides.Count - 1);
        activeSlides[currentIndex].SetActive(true);
        UpdateButtons();
    }

    private void ShowPrevSlide()
    {
        if (activeSlides.Count == 0) return;
        activeSlides[currentIndex].SetActive(false);
        currentIndex = Mathf.Max(currentIndex - 1, 0);
        activeSlides[currentIndex].SetActive(true);
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        if (prevButton != null) prevButton.interactable = currentIndex > 0;
        if (nextButton != null) nextButton.interactable = currentIndex < activeSlides.Count - 1;
    }
}