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
    public AnimationCurve easingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // ��������� ������ ��� easing (����� ��������� � ����������)
    public float swayAmplitude = 0.5f; // ��������� ����������� (������� ���������)
    public float swayFrequency = 1f;  // ������� �����������

    [Header("UI Panel")]
    public GameObject infoPanel;        // ������/������ � �����
    public Text regionTitle;        // �������� �������
    public Text regionDescription;  // �������� �������
    public Transform scrollContent;       // ��������� ��� ������ �������
    public Button closeButton;          // ������ ��������
    public Button EventButtonPrefab;          // ������ ��������



    [Header("Event Panel")]
    public GameObject eventPanel;         // ������ �������
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

        infoPanel.SetActive(false); // ������ ���������� ������
        eventPanel.SetActive(false); // ������ ���������� ������
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

    private IEnumerator MoveCameraToRegion(Region3D region)
    {
        Vector3 targetPos = region.GetCenter() + new Vector3(0, cameraHeight, -cameraDistance);
        Quaternion targetRot = Quaternion.LookRotation(region.GetCenter() - targetPos);

        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        float duration = 2f / cameraMoveSpeed; // ����� ������������ ��������
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easedT = easingCurve.Evaluate(t); // ��������� easing ������ ��� ���������

            // ������� � easing � ���������� ����������� ��� ������� "�������" (����� ����� ����)
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, easedT);
            // ��������� ������������ "dip" ��� �������� �������: ��������� ������ ���� � �������� ��������
            float dipAmount = Mathf.Sin(t * Mathf.PI) * (cameraHeight * 1f); // �������������� ������, max � ��������
            currentPos.y -= dipAmount; // �������� ������� ���� ��� ������� �������

            // ��������� ������ ����������� (sway) ��� � ����: �������������� ��������� �� ��� X � Z
            float swayOffset = Mathf.Sin(t * Mathf.PI * swayFrequency) * swayAmplitude * (1f - easedT); // ��������� ��������� � �����
            currentPos += mainCamera.transform.right * swayOffset * 2f; // ����������� �����-������
            currentPos += mainCamera.transform.forward * swayOffset * 0.5f; // ������ ������-�����

            mainCamera.transform.position = currentPos;

            // ������� � easing
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, easedT);

            yield return null;
        }

        // ������������� ��������� ������� � �������
        mainCamera.transform.position = targetPos;
        mainCamera.transform.rotation = targetRot;

        // ����� �������� ������ � �������� ������
        ShowRegionInfo(region);
    }

    private void ShowRegionInfo(Region3D region)
    {
        regionTitle.text = region.regionName;
        regionDescription.text = $"�������� �������: {region.regionName}\n����� ����� ������� ���������, ������� � �.�.";
        infoPanel.SetActive(true);

        // �������� ������ ������
        foreach (Transform child in scrollContent)
            Destroy(child.gameObject);

        // ������� ����� ������ �������
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

        // ����� ���������
        if (currentRegion != null)
            currentRegion.Highlight(false);

        UIManager.Instance.ClosePanel();

        // ������� ������ � �������� ���������
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