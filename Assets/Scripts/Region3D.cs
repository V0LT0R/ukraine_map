using System;
using System.Collections.Generic;
using UnityEngine;

public class Region3D : MonoBehaviour
{

    public List<GameObject> slidePrefabs = new List<GameObject>();

    public string regionName; // Название региона (может не совпадать с GameObject.name)

    private Renderer rend;
    private Color originalColor;
    public Color SelectedColor;

    void Awake()
    {
        // Если рендерер на этом объекте или дочернем
        rend = GetComponent<Renderer>();
        if (rend == null)
            rend = GetComponentInChildren<Renderer>();

        if (rend != null)
            originalColor = rend.material.color;
    }

    // Подсветка региона
    public void Highlight(bool enable, bool isLinked = false)
    {
        if (rend != null)
            rend.material.color = enable ? SelectedColor : originalColor;

        // Если это вызов из основного объекта — включаем подсветку связанного
        if (!isLinked)
        {
            if (gameObject.name == "Kyiv")
            {
                GameObject linked = GameObject.Find("Kyiv-City");
                if (linked != null)
                {
                    Region3D r = linked.GetComponent<Region3D>();
                    if (r != null)
                        r.Highlight(enable, true); // флажок true = не вызывать обратно
                }
            }
            else if (gameObject.name == "Kyiv-City")
            {
                GameObject linked = GameObject.Find("Kyiv");
                if (linked != null)
                {
                    Region3D r = linked.GetComponent<Region3D>();
                    if (r != null)
                        r.Highlight(enable, true); // флажок true = не вызывать обратно
                }
            }
        }
    }

    // Получение центра объекта
    public Vector3 GetCenter()
    {
        if (rend != null)
            return rend.bounds.center;
        return transform.position;
    }

    // Клик по региону
    void OnMouseDown()
    {
        if (UIManager.Instance.isPanelOpen)
            return;

        MapManager3D.Instance.SelectRegion(this);
    }
}
