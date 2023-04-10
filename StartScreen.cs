using System.Collections;
using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;

public class StartScreen : MonoBehaviourPunCallbacks
{
    public GameObject loadingTextObject;
    public GameObject clickTheScreenTextObject;

    private Animator animator;
 
    public override void OnJoinedLobby()
    {
        StartCoroutine(StartScreenOffDelayTimer());
    }

    private void Awake()
    {
        Screen.SetResolution(1280, 720, false);
        animator = GetComponent<Animator>();
    }
    private void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            gameObject.SetActive(false);
            return;
        }
    }
    private void Update()
    {
        if (animator.GetBool("loading"))
            return;

        if (Mouse.current.leftButton.isPressed)
        {
            animator.SetBool("loading", true);
            loadingTextObject.SetActive(true);
            clickTheScreenTextObject.SetActive(false);
            ConnectPhotonNetwork();
        }
    }
    private void ConnectPhotonNetwork()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    IEnumerator StartScreenOffDelayTimer()
    {
        yield return new WaitForSeconds(2);
        gameObject.SetActive(false);
    }
}
