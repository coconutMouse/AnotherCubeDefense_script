using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerSpawnPoint : MonoBehaviour
{
    public delegate void TriggerEvent(PlayerSpawnPoint playerSpawnPoint, int _viewId);
    public event TriggerEvent OnLoadingComplete;
    public RectTransform loadingBar;
    public Image imageComp; 

    private float speed;
    private PlayerCharacter playerToFix;

    public void FixPlayer(PlayerCharacter _palyer, float _loadingCount)
    {
        playerToFix = _palyer;
        speed = 1.0f / _loadingCount;
        imageComp.fillAmount = 0.0f;
        loadingBar.gameObject.SetActive(true);
        StartCoroutine(FixPlayerLoadingFillUpdate());
    }
    public void StopFixPlayer()
    {
        loadingBar.gameObject.SetActive(false);
        StopAllCoroutines();
    }

    private void FixComplete()
    {
        if (playerToFix == null)
            return;
        PhotonView playerPhotonView = playerToFix.photonView;
        if (playerPhotonView.IsMine)
        {
            playerPhotonView.RPC("Resurrection", RpcTarget.AllViaServer);
        }
    }

    IEnumerator FixPlayerLoadingFillUpdate()
    {
        while (imageComp.fillAmount < 1.0f)
        {
            imageComp.fillAmount = imageComp.fillAmount + Time.deltaTime * speed;
            yield return null;
        }
        loadingBar.gameObject.SetActive(false);

        FixComplete();
    }
}
