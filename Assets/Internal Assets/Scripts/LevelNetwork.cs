using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System.Collections;

public class LevelNetwork : NetworkManager
{
    public static int InventorySize { get; private set; } = 4;

    private const float Gravity = -9.81f;
    private static readonly float Speed = 6f;
    private static readonly float JumpForce = 1.0f;

    private static Dictionary<int, int[,]> PlayerInventories = new Dictionary<int, int[,]>();
    private static Dictionary<int, PlayerInput> Players = new Dictionary<int, PlayerInput>();

    [Server]
    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.SpawnObjects();
    }

    [Server]
    private void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("===== INVENTORY INFORMATION =====");
            foreach (var inventory in PlayerInventories)
            {
                Debug.Log("*** Player id: " + inventory.Key + " ***");
                Debug.Log("*** Items id ***");

                foreach (var item in inventory.Value)
                {
                    string id = "" + item;
                    if (item == 0)
                        id = "empty";

                    Debug.Log("Item id: " + id);
                }
                Debug.Log("****************");
            }
            Debug.Log("=======================================");
        }
    }

    [Server]
    public static void MovePlayer(NetworkConnection connection, Vector2 direction)
    {
        Vector3 movement = new Vector3(direction.x * Speed, 0, direction.y * Speed);
        movement = Vector3.ClampMagnitude(movement, Speed);
        movement.y = Gravity;
        movement *= Time.deltaTime;
        movement = connection.identity.transform.TransformDirection(movement);

        connection.identity.GetComponent<CharacterController>().Move(movement);
    }

    [Server]
    public override void OnServerAddPlayer(NetworkConnection connection)
    {
        base.OnServerAddPlayer(connection);

        int[,] cells = new int[InventorySize, 2];

        PlayerInventories.Add(connection.connectionId, cells);
        Players.Add(connection.connectionId, connection.identity.gameObject.GetComponent<PlayerInput>());
    }

    [Server]
    public static int AddItemToPlayer(int playerId, int objectId)
    {
        int[,] temp = new int[InventorySize, 2];
        PlayerInventories.TryGetValue(playerId, out temp);

        for (int i = 0; i < InventorySize; i++)
        {
            if (temp[i, 0] == 0 || temp[i, 0] == objectId)
            {
                temp[i, 0] = objectId;
                temp[i, 1] ++;
                PlayerInventories.Remove(playerId);
                PlayerInventories.Add(playerId, temp);
                return i;
            }
        }

        return -1;
    }

    [Server]
    public static void SetItemInPlayerHand(int playerId, int cellIndex)
    {
        PlayerInput temp;
        Players.TryGetValue(playerId, out temp);

        int[,] tempInventory = new int[InventorySize, 2];
        if (PlayerInventories.TryGetValue(playerId, out tempInventory) != true)
            throw new UnityException();

        if (tempInventory[cellIndex, 0] == 0)
            return;

        GameObject newItem = Instantiate(Resources.Load<Item>("HandleObjects/" + tempInventory[cellIndex, 0]).ItemPrefab, temp.transform);
        NetworkServer.Spawn(newItem);

        foreach (var player in Players)
        {
            player.Value.RpcSetItemInHands(newItem.GetComponent<NetworkIdentity>(), temp);
        }
    }

    [Server]
    public static void RemoveItemFromPlayer(int playerId, int cellIndex)
    {
        int[,] temp = new int[InventorySize, 2];
        if (PlayerInventories.TryGetValue(playerId, out temp) != true)
            throw new UnityException();

        temp[cellIndex, 1]--;
        if (temp[cellIndex, 1] == 0)
        {
            temp[cellIndex, 0] = 0;
            PlayerInput player;
            if (Players.TryGetValue(playerId, out player) == false)
                throw new UnityException();

            player.RpcUiUpdate(cellIndex, 0);
            if (player.ActiveCell == cellIndex)
            {
                HandleObject handleObject = player.GetComponentInChildren<HandleObject>();
                if (handleObject == null)
                    throw new UnityException();
                Destroy(handleObject.gameObject);
            }
        }

        PlayerInventories.Remove(playerId);
        PlayerInventories.Add(playerId, temp);
    }

    [Server]
    public override void OnServerDisconnect(NetworkConnection connection)
    {
        base.OnServerDisconnect(connection);

        PlayerInventories.Remove(connection.connectionId);
        Players.Remove(connection.connectionId);
    }
}
