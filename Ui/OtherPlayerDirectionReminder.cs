using System.Collections.Generic;
using UnityEngine;

public class OtherPlayerDirectionReminder : MonoBehaviour
{
    public GameObject[] otherPlayerDirectionArrows;

    private Vector2 screenCenterPoint;


    public Vector2 GetScreenPointInCameraScreen(Vector2 _targetViewportPoint)
    {
        if (_targetViewportPoint.x < 0f) _targetViewportPoint.x = 0f;
        else if (_targetViewportPoint.x > 1f) _targetViewportPoint.x = 1f;
        if (_targetViewportPoint.y < 0f) _targetViewportPoint.y = 0f;
        else if (_targetViewportPoint.y > 1f) _targetViewportPoint.y = 1f;

        Vector2 screenPoint = Camera.main.ViewportToScreenPoint(_targetViewportPoint);
        return screenPoint;
    }
    public bool CheckObjectIsInCamera(Vector2 _targetViewportPoint)
    {
        bool onScreen = (_targetViewportPoint.x > 0 && _targetViewportPoint.x < 1 && _targetViewportPoint.y > 0 && _targetViewportPoint.y < 1);
        return onScreen;
    }

    private void Start()
    {
        screenCenterPoint = new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2);
    }
    private void Update()
    {
        List<PlayerCharacter> playerCharacters = PlayerManager.Instance.GetPlayerCharacters();

        int arrowIndex = 0;
        foreach (PlayerCharacter player in playerCharacters)
        {
            if (player.photonView.IsMine == true || !player.gameObject.activeSelf)
                continue;

            if (otherPlayerDirectionArrows.Length <= arrowIndex)
            {
                Debug.LogError("Insufficient number of arrows pointing to other players");
                break;
            }

            Vector2 viewportPoint = Camera.main.WorldToViewportPoint(player.transform.position);

            if (CheckObjectIsInCamera(viewportPoint))
                continue;

            otherPlayerDirectionArrows[arrowIndex].SetActive(true);
            Vector2 screenPosition = GetScreenPointInCameraScreen(viewportPoint);

            Vector2 direction = screenPosition - screenCenterPoint;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion angleAxis = Quaternion.AngleAxis(angle - 90f, Vector3.forward);

            otherPlayerDirectionArrows[arrowIndex].transform.position = screenPosition;
            otherPlayerDirectionArrows[arrowIndex].transform.rotation = angleAxis;

            arrowIndex++;
        }
        for (; arrowIndex < otherPlayerDirectionArrows.Length; arrowIndex++)
            otherPlayerDirectionArrows[arrowIndex].SetActive(false);
    }
}
