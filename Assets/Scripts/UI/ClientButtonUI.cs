using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

class ClientButtonUI : MonoBehaviour
{
    [SerializeField] private TMP_Text clientButtonText;

    void Start()
    {
        EventManager.StartListening("connecting", new UnityAction(OnConnecting));
        EventManager.StartListening("disconnecting", new UnityAction(OnDisconnecting));
    }

    void OnConnecting()
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
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
        if (!NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.StartClient();
            EventManager.TriggerEvent("connecting");
            clientButtonText.text = "Stop <b>Client</b>";
        }
        else if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.Shutdown();
            EventManager.TriggerEvent("disconnecting");
            clientButtonText.text = "Start <b>Client</b>";
        }
    }
}