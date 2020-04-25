using CapsuleRoyale.BattleRoyaleMatchmaking;
using Cinemachine;
using Colyseus;
using GameDevWare.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class BattleRoyaleMatchmakingRoom : MonoBehaviour
{
    private Room<BattleRoyaleMatchmakingState> room;

    protected IndexedDictionary<string, Player> players =
        new IndexedDictionary<string, Player>();
    protected IndexedDictionary<string, GameObject> playerObjects =
        new IndexedDictionary<string, GameObject>();

    public PlayerController playerController;
    public GameObject playerPrefab;
    public CinemachineVirtualCamera cam;

    void Start()
    {
        room = ColyseusClient.Instance.RetrieveRoom<BattleRoyaleMatchmakingState>();
        RegisterRoomHandlers();
        
    }

    private void RegisterRoomHandlers()
    {
        room.State.players.OnAdd += OnPlayerAdd;
        room.State.players.OnRemove += OnPlayerRemove;
        room.State.players.OnChange += OnPlayerMove;
        room.State.TriggerAll();
    }

    void OnPlayerAdd(Player player, string key)
    {
        GameObject playerGameObject = Instantiate(
            playerPrefab,
            new Vector3(player.pos.x, player.pos.y, 0),
            Quaternion.identity);

        playerGameObject.GetComponentInChildren<Text>().text = player.name;
        playerGameObject.name = $"Player ({player.name} - {player.id})";

        players.Add(player.id, player);
        playerObjects.Add(player.id, playerGameObject);

        if (player.id == room.SessionId)
        {
            playerController.player = playerGameObject.GetComponent<Rigidbody2D>();
            playerController.enabled = true;
            playerController.OnPlayerMove += MovePlayer;
            cam.Follow = playerGameObject.transform;
        }
        else
        {
            Destroy(playerGameObject.GetComponent<Rigidbody2D>());
        }
    }

    void MovePlayer(Vector3 newPosition)
    {
        _ = room.Send("move", new Position
        {
            x = newPosition.x,
            y = newPosition.y
        });
    }

    void OnPlayerRemove(Player player, string key)
    {
        if (playerObjects.TryGetValue(player.id, out GameObject cube) && cube != null)
        {
            Destroy(cube);
        }

        players.Remove(player.id);
        playerObjects.Remove(player.id);
    }

    void OnPlayerMove(Player player, string key)
    {
        if (player.id == room.SessionId)
        {
            return;
        }

        if (playerObjects.TryGetValue(player.id, out GameObject cube) && cube != null)
        {
            cube.transform.position = new Vector3(player.pos.x, player.pos.y, 0);
        }
    }
}
