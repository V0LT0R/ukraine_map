using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MapManager3D : MonoBehaviour
{
    public static MapManager3D Instance { get; private set; }

    public List<Region3D> regions;

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
    public Button closeButton;          // Кнопка закрытия
    public Button EventButtonPrefab;          // Кнопка закрытия



    private Dictionary<string, List<string>> regionGroups = new Dictionary<string, List<string>>()
    {
        { "kyiv", new List<string> { "kyiv-city" } },
        { "kyiv-city", new List<string> { "kyiv" } },

    };


    private Region3D currentRegion;
    private Vector3 defaultCamPos;
    private Quaternion defaultCamRot;

    void Awake()
    {
        Instance = this;
        defaultCamPos = mainCamera.transform.position;
        defaultCamRot = mainCamera.transform.rotation;

        infoPanel.SetActive(false); // Панель изначально скрыта
    }

    public void SelectRegion(Region3D region)
    {
        // Снять подсветку и опустить предыдущую область (и связанные)
        if (currentRegion != null)
        {
            currentRegion.Highlight(false);
            currentRegion.SetRaised(false);

            if (regionGroups.ContainsKey(currentRegion.regionName))
            {
                foreach (string linkedRegionName in regionGroups[currentRegion.regionName])
                {
                    Region3D linkedPrev = regions.Find(r => r.regionName == linkedRegionName);
                    if (linkedPrev != null) linkedPrev.SetRaised(false);
                }
            }
        }

        // Подсветить и поднять новую
        currentRegion = region;

        currentRegion.Highlight(true);
        currentRegion.SetRaised(true);

        if (regionGroups.ContainsKey(region.regionName))
        {
            foreach (string linkedRegionName in regionGroups[region.regionName])
            {
                Region3D linked = regions.Find(r => r.regionName == linkedRegionName);
                if (linked != null)
                {
                    linked.Highlight(true);
                    linked.SetRaised(true);
                }
            }
        }

        // Слайды для UI
        List<GameObject> slides = (region.slidePrefabs != null && region.slidePrefabs.Count > 0)
            ? region.slidePrefabs
            : new List<GameObject>();

        UIManager.Instance.OpenPanel(slides);

        // Камера
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
        infoPanel.SetActive(true);
        List<GameObject> slides = (region.slidePrefabs != null && region.slidePrefabs.Count > 0)
          ? region.slidePrefabs
          : new List<GameObject>(); // пустой список — UIManager корректно обработает

        UIManager.Instance.OpenPanel(slides);
    }

    public void CloseRegionInfo()
    {
        infoPanel.SetActive(false);

        // Снять подсветку и опустить текущую область
        if (currentRegion != null)
        {
            currentRegion.Highlight(false);
            currentRegion.SetRaised(false);

            if (regionGroups.ContainsKey(currentRegion.regionName))
            {
                foreach (string linkedRegionName in regionGroups[currentRegion.regionName])
                {
                    Region3D linked = regions.Find(r => r.regionName == linkedRegionName);
                    if (linked != null) linked.SetRaised(false);
                }
            }
        }

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