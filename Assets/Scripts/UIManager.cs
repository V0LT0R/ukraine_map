using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Elements")]
    public GameObject carouselPanel;   // ������� ��������� (������) ��� �������
    public Transform slidesContainer;  // ��������� ������ ������, ���� ����� ���������� ������
    public Button nextButton;
    public Button prevButton;

    [Header("State")]
    public bool isPanelOpen = false; // true � ���� ������ �������

    private List<GameObject> activeSlides = new List<GameObject>();
    private int currentIndex = 0;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // ���������� ����������� ������
        if (nextButton != null) nextButton.onClick.AddListener(ShowNextSlide);
        if (prevButton != null) prevButton.onClick.AddListener(ShowPrevSlide);

        if (carouselPanel != null)
            carouselPanel.SetActive(false); // ���������� ������
    }

    /// <summary>
    /// ������� ������ ������� � ��������� �������
    /// </summary>
    public void OpenPanel(List<GameObject> slidePrefabs)
    {
        if (carouselPanel != null)
            carouselPanel.SetActive(true);

        isPanelOpen = true;

        // �������� ������
        foreach (var go in activeSlides)
            Destroy(go);
        activeSlides.Clear();

        // ������� ����� ������ �� ��������
        foreach (var prefab in slidePrefabs)
        {
            GameObject panel = Instantiate(prefab, slidesContainer);
            panel.SetActive(false);
            activeSlides.Add(panel);
        }

        // �������� ������
        currentIndex = 0;
        if (activeSlides.Count > 0)
            activeSlides[currentIndex].SetActive(true);

        UpdateButtons();
    }

    /// <summary>
    /// ������� ������
    /// </summary>
    public void ClosePanel()
    {
        if (carouselPanel != null)
            carouselPanel.SetActive(false);

        isPanelOpen = false;

        // �������� ������
        foreach (var go in activeSlides)
            Destroy(go);
        activeSlides.Clear();
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
        if (prevButton != null)
            prevButton.interactable = currentIndex > 0;

        if (nextButton != null)
            nextButton.interactable = currentIndex < activeSlides.Count - 1;
    }
}
