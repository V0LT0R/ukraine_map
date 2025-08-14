using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Elements")]

    public bool isPanelOpen = false; // true Ч если панель открыта

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void OpenPanel()
    {
   

        isPanelOpen = true;
    }

    public void ClosePanel()
    {


        isPanelOpen = false;
    }
}
