using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public SquadArrangementRoom room;

    [HideInInspector]
    public Transform player;

    public float speed = 10;

    void Update()
    {
        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");
        if (player != null && (horizontal != 0 || vertical != 0))
        {
            player.position += new Vector3(horizontal, vertical, 0) * speed * Time.deltaTime;
            room.MovePlayer(player.position);
        }
    }
}
