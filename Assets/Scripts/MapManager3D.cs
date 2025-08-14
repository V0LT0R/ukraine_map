using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapManager3D : MonoBehaviour
{
    public static MapManager3D Instance { get; private set; }

    [Header("Camera Settings")]
    public Camera mainCamera;
    public float cameraDistance = 5f;
    public float cameraHeight = 3f;
    public float cameraMoveSpeed = 2f;

    [Header("UI Panel")]
    public GameObject infoPanel;        // Канвас/панель с инфой
    public Text regionTitle;        // Название области
    public Text regionDescription;  // Описание области
    public Button closeButton;          // Кнопка закрытия

    private Region3D currentRegion;
    private Vector3 defaultCamPos;
    private Quaternion defaultCamRot;

    void Awake()
    {
        Instance = this;
        defaultCamPos = mainCamera.transform.position;
        defaultCamRot = mainCamera.transform.rotation;

        infoPanel.SetActive(false); // Панель изначально скрыта
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

    private System.Collections.IEnumerator MoveCameraToRegion(Region3D region)
    {
        Vector3 targetPos = region.GetCenter() + new Vector3(0, cameraHeight, -cameraDistance);
        Quaternion targetRot = Quaternion.LookRotation(region.GetCenter() - targetPos);

        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * cameraMoveSpeed;
            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        // После движения камеры — показать панель
        ShowRegionInfo(region);
    }

    private void ShowRegionInfo(Region3D region)
    {
        regionTitle.text = region.regionName;
        regionDescription.text = $"Описание области: {region.regionName}\nЗдесь можно вывести население, площадь и т.д.";
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

    private System.Collections.IEnumerator ReturnCameraToDefault()
    {
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * cameraMoveSpeed;
            mainCamera.transform.position = Vector3.Lerp(startPos, defaultCamPos, t);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, defaultCamRot, t);
            yield return null;
        }
    }
}
