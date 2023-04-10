using UnityEngine;
using UnityEngine.UI;

public class MessageManager : Singleton<MessageManager>
{
    public GameObject messageWin;
    public Text messageText;
    public void Message(string message)
    {
        messageWin.SetActive(true);
        messageText.text = message;
    }
}
