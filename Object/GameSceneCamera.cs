using UnityEngine;

public class GameSceneCamera : MonoBehaviour
{
    public Transform mainTower;
    public float moveSpeed;
    public float zoomInFocusValue;

    private Transform myPlayerCharacter;
    private Vector3 basicPosition;
    private Vector3 zoomInFocusPosition;

    private void Awake()
    {
        PlayerManager.Instance.OnMyPlayerCharacterActive += OnMyPlayerCharacterActive;
        basicPosition = transform.position;
        zoomInFocusPosition = basicPosition + (transform.forward * zoomInFocusValue);
    }
    private void LateUpdate()
    {
        if (myPlayerCharacter == null)
            return;

        Vector3 targetPosition;
        if (!mainTower.gameObject.activeSelf)
            targetPosition = mainTower.position + zoomInFocusPosition;
        else if (myPlayerCharacter.gameObject.activeSelf)
            targetPosition = myPlayerCharacter.position + basicPosition;
        else
            targetPosition = myPlayerCharacter.position + zoomInFocusPosition;

        transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }
    private void OnMyPlayerCharacterActive(PlayerCharacter _myPlayerCharacter)
    {
        myPlayerCharacter = _myPlayerCharacter.transform;
    }
}
