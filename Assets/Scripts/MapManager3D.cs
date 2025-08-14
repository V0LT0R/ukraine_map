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
    public GameObject infoPanel;        // ������/������ � �����
    public Text regionTitle;        // �������� �������
    public Text regionDescription;  // �������� �������
    public Button closeButton;          // ������ ��������

    private Region3D currentRegion;
    private Vector3 defaultCamPos;
    private Quaternion defaultCamRot;

    void Awake()
    {
        Instance = this;
        defaultCamPos = mainCamera.transform.position;
        defaultCamRot = mainCamera.transform.rotation;

        infoPanel.SetActive(false); // ������ ���������� ������
        closeButton.onClick.AddListener(CloseRegionInfo);
    }

    public void SelectRegion(Region3D region)
    {
        // ����� ��������� � ���������� �������
        if (currentRegion != null)
            currentRegion.Highlight(false);

        // ���������� �����
        currentRegion = region;
        currentRegion.Highlight(true);
        UIManager.Instance.OpenPanel();

        // ���������� �������� ������ � ������ �����
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

        // ����� �������� ������ � �������� ������
        ShowRegionInfo(region);
    }

    private void ShowRegionInfo(Region3D region)
    {
        regionTitle.text = region.regionName;
        regionDescription.text = $"�������� �������: {region.regionName}\n����� ����� ������� ���������, ������� � �.�.";
        infoPanel.SetActive(true);
    }

    public void CloseRegionInfo()
    {
        infoPanel.SetActive(false);

        // ����� ���������
        if (currentRegion != null)
            currentRegion.Highlight(false);

            UIManager.Instance.ClosePanel();

        // ������� ������ � �������� ���������
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
