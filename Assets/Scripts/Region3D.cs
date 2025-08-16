using UnityEngine;

public class Region3D : MonoBehaviour
{
    public string regionName; // �������� �������

    private Renderer rend;
    private Color originalColor;
    public Color SelectedColor;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
            originalColor = rend.material.color;
    }

    // ��������� �������
    public void Highlight(bool enable)
    {
        if (rend != null)
            rend.material.color = enable ? SelectedColor : originalColor;
    }

    // ��������� ������ �������
    public Vector3 GetCenter()
    {
        return GetComponent<Renderer>().bounds.center;
    }

    // ���� �� �������
    void OnMouseDown()
    {
        // ���� ������ ������� � ���� ������������
        if (UIManager.Instance.isPanelOpen)

            return;

        Debug.Log("���� �� �������: " + regionName);

        // ������: ���������� � MapManager3D
        MapManager3D.Instance.SelectRegion(this);
    }
}
