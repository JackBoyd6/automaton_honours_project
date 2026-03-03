using UnityEngine;
using TMPro;

public class ErrorPopupManager : MonoBehaviour
{
    public static ErrorPopupManager Instance;
    
    [SerializeField]
    private GameObject popupPanel;
    
    [SerializeField]
    private TextMeshProUGUI errorText;
    
    void Awake()
    {
        // Singleton pattern - only one instance exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicates
        }
    }
    
    public void ShowError(string message)
    {
        errorText.text = message;
        popupPanel.SetActive(true);
    }
    
    public void ClosePopup()
    {
        popupPanel.SetActive(false);
    }
}
