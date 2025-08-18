using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MapManager3D : MonoBehaviour
{
    public static MapManager3D Instance { get; private set; }

    [Header("Camera Settings")]
    public Camera mainCamera;
    public float cameraDistance = 5f;
    public float cameraHeight = 3f;
    public float cameraMoveSpeed = 2f;
    public AnimationCurve easingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // Кастомная кривая для easing (можно настроить в инспекторе)
    public float swayAmplitude = 0.5f; // Амплитуда покачивания (степень колебания)
    public float swayFrequency = 1f;  // Частота покачивания

    [Header("UI Panel")]
    public GameObject infoPanel;        // Канвас/панель с инфой
    public Text regionTitle;        // Название области
    public Text regionDescription;  // Описание области
    public Transform scrollContent;       // Контейнер для кнопок событий
    public Button closeButton;          // Кнопка закрытия
    public Button EventButtonPrefab;          // Кнопка закрытия



    [Header("Event Panel")]
    public GameObject eventPanel;         // Панель события
    public Text eventTitle;
    public Text eventDescription;

    private Region3D currentRegion;
    private Vector3 defaultCamPos;
    private Quaternion defaultCamRot;

    void Awake()
    {
        Instance = this;
        defaultCamPos = mainCamera.transform.position;
        defaultCamRot = mainCamera.transform.rotation;

        infoPanel.SetActive(false); // Панель изначально скрыта
        eventPanel.SetActive(false); // Панель изначально скрыта
        closeButton.onClick.AddListener(CloseRegionInfo);
    }

    public void SelectRegion(Region3D region)
    {
        // Снять подсветку с предыдущей области
        if (currentRegion != null)
            currentRegion.Highlight(false);

        // Подсветить новую
        currentRegion = region;
        currentRegion.Highlight(true);
        UIManager.Instance.OpenPanel();

        // Остановить корутины камеры и начать новую
        StopAllCoroutines();
        StartCoroutine(MoveCameraToRegion(region));
    }

    private IEnumerator MoveCameraToRegion(Region3D region)
    {
        Vector3 targetPos = region.GetCenter() + new Vector3(0, cameraHeight, -cameraDistance);
        Quaternion targetRot = Quaternion.LookRotation(region.GetCenter() - targetPos);

        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        float duration = 2f / cameraMoveSpeed; // Общая длительность анимации
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easedT = easingCurve.Evaluate(t); // Применяем easing кривую для плавности

            // Позиция с easing и нелинейной траекторией для эффекта "падения" (лёгкий изгиб вниз)
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, easedT);
            // Добавляем вертикальный "dip" для ощущения падения: небольшой прогиб вниз в середине анимации
            float dipAmount = Mathf.Sin(t * Mathf.PI) * (cameraHeight * 1f); // Синусоидальный прогиб, max в середине
            currentPos.y -= dipAmount; // Опускаем позицию вниз для эффекта падения

            // Добавляем легкое покачивание (sway) как у пера: синусоидальное колебание по оси X и Z
            float swayOffset = Mathf.Sin(t * Mathf.PI * swayFrequency) * swayAmplitude * (1f - easedT); // Уменьшаем амплитуду к концу
            currentPos += mainCamera.transform.right * swayOffset * 2f; // Покачивание влево-вправо
            currentPos += mainCamera.transform.forward * swayOffset * 0.5f; // Легкое вперед-назад

            mainCamera.transform.position = currentPos;

            // Ротация с easing
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, easedT);

            yield return null;
        }

        // Зафиксировать финальную позицию и ротацию
        mainCamera.transform.position = targetPos;
        mainCamera.transform.rotation = targetRot;

        // После движения камеры — показать панель
        ShowRegionInfo(region);
    }

    private void ShowRegionInfo(Region3D region)
    {
        regionTitle.text = region.regionName;
        regionDescription.text = $"Описание области: {region.regionName}\nЗдесь можно вывести население, площадь и т.д.";
        infoPanel.SetActive(true);

        // Очистить старые кнопки
        foreach (Transform child in scrollContent)
            Destroy(child.gameObject);

        // Создать новые кнопки событий
        foreach (var ev in region.events)
        {
            Button btnObj = Instantiate(EventButtonPrefab, scrollContent);
            var btnText = btnObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            btnText.text = ev.eventName;

            var btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() => ShowEventInfo(ev));
        }

        UIManager.Instance.OpenPanel();
    }

    private void ShowEventInfo(Region3D.RegionEvent ev)
    {
        eventTitle.text = ev.eventName;
        eventDescription.text = ev.eventDescription;
        
        eventPanel.SetActive(true);
        infoPanel.SetActive(false);
    }

    public void CloseEventInfo()
    {
        eventPanel.SetActive(false);
        infoPanel.SetActive(true);
    }

    public void CloseRegionInfo()
    {
        infoPanel.SetActive(false);

        // Снять подсветку
        if (currentRegion != null)
            currentRegion.Highlight(false);

        UIManager.Instance.ClosePanel();

        // Вернуть камеру в исходное положение
        StopAllCoroutines();
        StartCoroutine(ReturnCameraToDefault());
    }

    private IEnumerator ReturnCameraToDefault()
    {
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        float duration = 1f / cameraMoveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easedT = easingCurve.Evaluate(t);

            mainCamera.transform.position = Vector3.Lerp(startPos, defaultCamPos, easedT);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, defaultCamRot, easedT);

            yield return null;
        }

        mainCamera.transform.position = defaultCamPos;
        mainCamera.transform.rotation = defaultCamRot;
    }
}