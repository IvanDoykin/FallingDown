using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System.Collections;

public class LevelNetwork : NetworkManager
{
    public static int InventorySize { get; private set; } = 4;

    private const float Epsilon = 0.0001f;
    private const float Gravity = -9.81f;
    private static readonly float Speed = 60f;
    private static readonly float JumpForce = 1.0f;

    private static Dictionary<int, int[,]> PlayerInventories = new Dictionary<int, int[,]>();
    private static Dictionary<int, PlayerInput> Players = new Dictionary<int, PlayerInput>();
    private static Dictionary<int, int> PlayersHealth = new Dictionary<int, int>();
    private static Dictionary<int, Vector3> PlayersPositions = new Dictionary<int, Vector3>();

    private int currentTick = 0;

    [Server]
    public override void OnStartServer()
    {
        Debug.Log(Speed * Time.fixedDeltaTime);
        base.OnStartServer();
        NetworkServer.SpawnObjects();
    }

    [Server]
    public override void OnServerAddPlayer(NetworkConnection connection)
    {
        base.OnServerAddPlayer(connection);

        int[,] cells = new int[InventorySize, 2];
        int health = 100;

        PlayersHealth.Add(connection.connectionId, health);
        PlayerInventories.Add(connection.connectionId, cells);
        Players.Add(connection.connectionId, connection.identity.gameObject.GetComponent<PlayerInput>());
        PlayersPositions.Add(connection.connectionId, connection.identity.transform.position);
    }

    [Server]
    public override void OnServerDisconnect(NetworkConnection connection)
    {
        base.OnServerDisconnect(connection);

        PlayersHealth.Remove(connection.connectionId);
        PlayerInventories.Remove(connection.connectionId);
        Players.Remove(connection.connectionId);
        PlayersPositions.Remove(connection.connectionId);
    }

    [Server]
    private void FixedUpdate()
    {
        currentTick++;
        if (currentTick == 60)
        {
            currentTick = 0;
            foreach (var player in Players)
            {
                Vector3 playerPosition;
                if (PlayersPositions.TryGetValue(player.Key, out playerPosition) == false)
                    throw new UnityException();

                playerPosition.y = player.Value.transform.position.y;
                Debug.Log("length = " + (player.Value.transform.position - playerPosition).sqrMagnitude);
                Debug.Log("max = " + Speed * Speed * Time.fixedDeltaTime * Time.deltaTime * 225);
                if ((player.Value.transform.position - playerPosition).sqrMagnitude > Speed * Speed * Time.fixedDeltaTime * Time.deltaTime * 225)
                {
                    Debug.LogError(player.Value.netIdentity.transform.position);
                    Debug.LogError(playerPosition);
                    //NetworkServer.RemovePlayerForConnection(player.Value.connectionToClient, true);
                }
            }
        }

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
    public static void CheckPlayerMovement(int playerId, Vector3 playerPosition)
    {
        Vector3 newPos = new Vector3();
        if (PlayersPositions.TryGetValue(playerId, out newPos) == false)
            throw new UnityException();

        PlayerInput player;
        if (Players.TryGetValue(playerId, out player) == false)
            throw new UnityException();

        Vector2 tempPos = new Vector2(newPos.x - playerPosition.x, newPos.z - playerPosition.z);
        Debug.Log("temp = " + tempPos);
        if (tempPos.sqrMagnitude > 2 * Speed * Speed * Time.fixedDeltaTime * Time.fixedDeltaTime + Epsilon)
        {
            //NetworkServer.RemovePlayerForConnection(player.connectionToClient, true);
        }

        newPos = playerPosition;
        PlayersPositions.Remove(playerId);
        PlayersPositions.Add(playerId, newPos);
    }

    [Server]
    public static void ApplyDamageToPlayer(int playerId, int damage)
    {
        int health;
        PlayersHealth.TryGetValue(playerId, out health);

        health = Mathf.Clamp(health - damage, 0, 100);
        if (health == 0)
        {
            PlayerInput player;
            Players.TryGetValue(playerId, out player);
            player.RpcDie();
        }
    }

    [Server]
    public static int AddItemToPlayer(int playerId, int objectId)
    {
        int[,] temp = new int[InventorySize, 2];
        PlayerInventories.TryGetValue(playerId, out temp);

        for (int cellIndex = 0; cellIndex < InventorySize; cellIndex++)
        {
            if (temp[cellIndex, 0] == 0 || temp[cellIndex, 0] == objectId)
            {
                temp[cellIndex, 0] = objectId;
                temp[cellIndex, 1]++;
                PlayerInventories.Remove(playerId);
                PlayerInventories.Add(playerId, temp);
                return cellIndex;
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
}
