using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System.Collections;
using System;

public class LevelNetwork : NetworkManager
{
    public static int InventorySize { get; private set; } = 4;

    private const float Epsilon = 0.0001f;
    private const float Gravity = -9.81f;
    private static readonly float Speed = 5f;
    private static readonly float JumpForce = 1.0f;

    private static Dictionary<int, int[,]> PlayerInventories = new Dictionary<int, int[,]>();
    private static Dictionary<int, Player> Players = new Dictionary<int, Player>();
    private static Dictionary<int, Vector3> PlayersPositions = new Dictionary<int, Vector3>();
    private int frame = 0;

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
        connection.identity.GetComponent<Player>().Health = 100;

        PlayerInventories.Add(connection.connectionId, cells);
        Players.Add(connection.connectionId, connection.identity.GetComponent<Player>());
        PlayersPositions.Add(connection.connectionId, connection.identity.transform.position);
    }

    [Server]
    public override void OnServerDisconnect(NetworkConnection connection)
    {
        base.OnServerDisconnect(connection);

        PlayerInventories.Remove(connection.connectionId);
        Players.Remove(connection.connectionId);
        PlayersPositions.Remove(connection.connectionId);
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

                for (int index = 0; index < InventorySize; index++)
                {
                    string id = inventory.Value[index, 0].ToString();
                    string amounts = inventory.Value[index, 1].ToString();

                    if (inventory.Value[index, 0] == 0)
                        id = "empty";

                    if (inventory.Value[index, 1] == 0)
                        amounts = "none";

                    Debug.Log("Item id: " + id);
                    Debug.Log("Item amounts = " + amounts);
                }
                Debug.Log("****************");
            }
            Debug.Log("=======================================");
        }

        frame++;
        return; //disable check movement

        if (frame == 60)
        {
            frame = 0;
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
                    NetworkServer.RemovePlayerForConnection(player.Value.connectionToClient, true);
                }
            }
        }
    }

    [Server]
    public static void CheckPlayerMovement(int playerId, Vector3 playerPosition)
    {
        Vector3 newPos = new Vector3();
        if (PlayersPositions.TryGetValue(playerId, out newPos) == false)
            throw new UnityException();

        Player player;
        if (Players.TryGetValue(playerId, out player) == false)
            throw new UnityException();

        Vector2 tempPos = new Vector2(newPos.x - playerPosition.x, newPos.z - playerPosition.z);
        if (tempPos.sqrMagnitude > 2 * Speed * Speed * Time.fixedDeltaTime * Time.fixedDeltaTime + Epsilon)
        {
            //NetworkServer.RemovePlayerForConnection(player.connectionToClient, true);
        }

        newPos = playerPosition;
        PlayersPositions.Remove(playerId);
        PlayersPositions.Add(playerId, newPos);
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
                Debug.Log("Added item id = " + objectId);
                Debug.Log("Cell index = " + cellIndex);
                return cellIndex;
            }
        }

        return -1;
    }

    [Server]
    public static void SetItemInPlayerHand(int playerId, int cellIndex)
    {
        Debug.Log("set item = " + cellIndex);
        Player player;
        Players.TryGetValue(playerId, out player);

        int[,] tempInventory = new int[InventorySize, 2];
        if (PlayerInventories.TryGetValue(playerId, out tempInventory) != true)
            throw new UnityException();

        if (tempInventory[cellIndex, 0] == 0)
            return;

        Debug.Log("passed checks");
        Debug.Log(tempInventory[cellIndex, 0]);
        Debug.Log(tempInventory[cellIndex, 1]);

        player.SelectedCell = cellIndex;
        player.HandleObjectId = tempInventory[cellIndex, 0];
        foreach (var currentPlayer in Players.Values)
        {
            //currentPlayer.RpcSetItemInHands(tempInventory[cellIndex, 0], player);
        }
    }

    [Server]
    public static void SetItemInHands(NetworkIdentity identity, Player player)
    {
        HandleObject handleObject = player.Hand.GetComponentInChildren<HandleObject>();
        if (handleObject != null)
        {
            Debug.Log("Destroy HO");
            NetworkServer.Destroy(handleObject.gameObject);
        }

        identity.GetComponent<HandleObject>().Parent = player.Hand;
        identity.transform.SetParent(player.Hand);
        identity.transform.localPosition = Vector3.zero;
        Destroy(identity.GetComponent<Collider>());
    }

    [Server]
    private static void SimulateAction(int frameId, float clientSubFrame, Action action)
    {
        ActionSimulator.Simulate(frameId, clientSubFrame, action);
    }

    [Server]
    public static void ShootSimulate(int frameId, float clientSubFrame, int playerId, Vector3 direction, Vector3 additionalVector)
    {
        Vector3 newPos = new Vector3();
        if (PlayersPositions.TryGetValue(playerId, out newPos) == false)
            throw new UnityException();
        newPos += additionalVector;

        Action action = () =>
        {
            Debug.DrawRay(newPos, direction, Color.red, 100f);
            Ray ray = new Ray(newPos, direction);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000f))
            {
                Debug.Log(hit.collider.name);
                Player player = hit.collider?.GetComponent<PlayerCollider>()?.GetComponentInParent<Player>();
                if (player != null)
                {
                    player.Health -= 25;
                }
                else
                {
                    Debug.Log("No player collider at ray");
                }
            }
        };

        SimulateAction(frameId, clientSubFrame, action);
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

            Player player;
            if (Players.TryGetValue(playerId, out player) != true)
                throw new UnityException();

            player.SelectedCell = -1;
            player.HandleObjectId = 0;
        }

        if (temp[cellIndex, 1] < 0)
            throw new UnityException();

        PlayerInventories.Remove(playerId);
        PlayerInventories.Add(playerId, temp);
    }
}
