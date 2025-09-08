using System;
using System.Collections.Generic;
using UnityEngine;

public class Region3D : MonoBehaviour
{
    public List<GameObject> slidePrefabs = new List<GameObject>();

    public string regionName; // Название региона (может не совпадать с GameObject.name)

    [Header("Highlight")]
    private Renderer rend;
    private Color originalColor;
    public Color SelectedColor = new Color(0.28f, 0.27f, 0.1f, 0f);

    [Header("Lift Settings")]
    public float liftHeight = 0.17f;   // высота подъёма
    public float liftSpeed = 1.2f;      // скорость (чем больше — тем быстрее)
    private Vector3 basePosition;     // исходная позиция региона
    private Coroutine moveCo;
    private bool isRaised = false;

    void Awake()
    {
        // Рендерер
        rend = GetComponent<Renderer>();
        if (rend == null)
            rend = GetComponentInChildren<Renderer>();
        if (rend != null)
            originalColor = rend.material.color;

        // Запоминаем исходную позицию
        basePosition = transform.position;
    }

    // Подсветка региона
    public void Highlight(bool enable, bool isLinked = false)
    {
        if (rend != null)
            rend.material.color = enable ? SelectedColor : originalColor;

        // Пробрасываем подсветку связанным
        if (!isLinked)
        {
            if (gameObject.name == "Kyiv")
            {
                GameObject linked = GameObject.Find("Kyiv-City");
                if (linked != null)
                {
                    Region3D r = linked.GetComponent<Region3D>();
                    if (r != null) r.Highlight(enable, true);
                }
            }
            else if (gameObject.name == "Kyiv-City")
            {
                GameObject linked = GameObject.Find("Kyiv");
                if (linked != null)
                {
                    Region3D r = linked.GetComponent<Region3D>();
                    if (r != null) r.Highlight(enable, true);
                }
            }
        }
    }

    // Плавный подъём/опускание региона
    public void SetRaised(bool enable, bool isLinked = false)
    {
        isRaised = enable;

        Vector3 target = enable ? (basePosition + transform.forward * liftHeight) : basePosition;
        if (moveCo != null) StopCoroutine(moveCo);
        moveCo = StartCoroutine(MoveTo(target, liftSpeed));

        // Пробрасываем подъём связанным регионам
        if (!isLinked)
        {
            if (gameObject.name == "Kyiv")
            {
                GameObject linked = GameObject.Find("Kyiv-City");
                if (linked != null)
                {
                    Region3D r = linked.GetComponent<Region3D>();
                    if (r != null) r.SetRaised(enable, true);
                }
            }
            else if (gameObject.name == "Kyiv-City")
            {
                GameObject linked = GameObject.Find("Kyiv");
                if (linked != null)
                {
                    Region3D r = linked.GetComponent<Region3D>();
                    if (r != null) r.SetRaised(enable, true);
                }
            }
        }
    }

    private System.Collections.IEnumerator MoveTo(Vector3 target, float speed)
    {
        while ((transform.position - target).sqrMagnitude > 0.0001f)
        {
            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * speed);
            yield return null;
        }
        transform.position = target;
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