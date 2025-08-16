using UnityEngine;

public class Region3D : MonoBehaviour
{
    public string regionName; // Название региона

    private Renderer rend;
    private Color originalColor;
    public Color SelectedColor;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
            originalColor = rend.material.color;
    }

    // Подсветка региона
    public void Highlight(bool enable)
    {
        if (rend != null)
            rend.material.color = enable ? SelectedColor : originalColor;
    }

    // Получение центра объекта
    public Vector3 GetCenter()
    {
        return GetComponent<Renderer>().bounds.center;
    }

    // Клик по региону
    void OnMouseDown()
    {
        // Если панель открыта — клик игнорируется
        if (UIManager.Instance.isPanelOpen)

            return;

        Debug.Log("Клик по региону: " + regionName);

        // Пример: отправляем в MapManager3D
        MapManager3D.Instance.SelectRegion(this);
    }
}
