using UnityEngine;
using TMPro;

public class TogglePopUp : MonoBehaviour
{
    [SerializeField]
    TMP_Text m_TextField;

    public GameObject PopUpGO;
    private bool IsPopUpActive;

    void Start()
    {
        IsPopUpActive = true;
    }

    void Update()
    {
        if (IsPopUpActive)
        {
            m_TextField.text = "Disable Pop-Up";
        }
        else
        {
            m_TextField.text = "Enable Pop-Up";
        }
    }

    public void Toggle()
    {
        IsPopUpActive = !IsPopUpActive;
        PopUpGO.SetActive(IsPopUpActive);
    }
}