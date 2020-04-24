using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public SquadArrangementRoom room;

    [HideInInspector]
    public Rigidbody2D player;

    public float speed = 10;
    private Vector2 movement = Vector2.zero;

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
            room.MovePlayer(player.position);
        }
    }
}
