using UnityEngine;
using UnityEngine.InputSystem;

public class MouseGroundArrow : MonoBehaviour
{
    public LayerMask hitLayerMask;
    public Transform arrow;
    public Transform parts;
    public Vector3 playerv;

    private Transform player;
    private Vector3 arrowDirection;
    private float arrowDistanceFromPlayer;

    public void SetPlayer(Transform _player)
    {
        player = _player;
    }
    public Vector3 GetArrowDirection()
    {
        return arrowDirection;
    }
    public float GetArrowDistanceFromPlayer()
    {
        return arrowDistanceFromPlayer;
    }

    private void Update()
    {
        parts.Rotate(0, 50 * Time.deltaTime, 0);
         

        Vector2 pointingPosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(pointingPosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, hitLayerMask))
        {
            transform.position = hit.point;
        }
        playerv = player.position;

        arrowDirection = transform.position - player.position;
        arrowDirection.y = 0;
        arrowDistanceFromPlayer = arrowDirection.magnitude;
        arrowDirection.Normalize();
        arrow.forward = arrowDirection;
    }
}
