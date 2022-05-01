using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

class HostButtonUI : MonoBehaviour
{
    [SerializeField] private TMP_Text hostButtonText;

    void Start()
    {
        EventManager.StartListening("connecting", new UnityAction(OnConnecting));
        EventManager.StartListening("disconnecting", new UnityAction(OnDisconnecting));
    }

    void OnConnecting()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            gameObject.GetComponent<Button>().interactable = false;
        }
    }

    void OnDisconnecting()
    {
        gameObject.GetComponent<Button>().interactable = true;
    }

    public void OnClickButton()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.StartHost();
            EventManager.TriggerEvent("connecting");
            hostButtonText.text = "Stop <b>Host</b>";
        }
        else
        {
            NetworkManager.Singleton.Shutdown();
            EventManager.TriggerEvent("disconnecting");
            hostButtonText.text = "Start <b>Host</b>";
        }
    }
}