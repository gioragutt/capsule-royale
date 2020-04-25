using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [HideInInspector]
    public Rigidbody2D player;

    public float speed = 10;
    private Vector2 movement = Vector2.zero;

    public delegate void OnPlayerMoveDelegate(Vector3 pos);

    public OnPlayerMoveDelegate OnPlayerMove;

    void Update()
    {
        movement.x = Input.GetAxis("Horizontal");
        movement.y = Input.GetAxis("Vertical");
    }

    private void FixedUpdate()
    {
        if (player != null && movement.magnitude != 0)
        {
            player.MovePosition(player.position + movement * speed * Time.fixedDeltaTime);
            OnPlayerMove?.Invoke(player.position);
        }
    }
}
