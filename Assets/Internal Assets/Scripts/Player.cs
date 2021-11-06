using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Collections;

public class Player : NetworkBehaviour
{
    [SyncVar(hook = nameof(SetItemInHands))] public int HandleObjectId;
    [SyncVar(hook = nameof(SyncHealthWithUi))] public int Health;
    [SyncVar] public int SelectedCell;
    [SyncVar] public int Frame;

    [SerializeField] private Transform hand;
    public Transform Hand
    {
        get
        {
            return hand;
        }
        private set
        {
            hand = value;
        }
    }

    [SerializeField] private float gravity = 0;
    public float Gravity
    {
        get
        {
            return gravity;
        }

        set
        {
            if (gravity == 0 && value != 0)
                gravity = value;
        }
    }

    [SerializeField] private float speed = 0;
    public float Speed
    {
        get
        {
            return speed;
        }

        set
        {
            if (speed == 0 && value != 0)
                speed = value;
        }
    }

    private UIToWorldWrapper ui;
    private Rigidbody characterController;
    private MouseLook cameraLook;
    private SimulatedObject simulatedObject;

    private void Start()
    {
        simulatedObject = GetComponent<SimulatedObject>();
    }

    public override void OnStartLocalPlayer()
    {
        Player[] players = (Player[])FindObjectsOfType(typeof(Player));
        foreach (var currentPlayer in players)
        {
            currentPlayer.SetItemInHands();
        }

        if (hasAuthority != true)
            return;

        cameraLook = GetComponentInChildren<MouseLook>();
        characterController = GetComponent<Rigidbody>();
        ui = GameObject.Find("[UI]").GetComponent<UIToWorldWrapper>();
        ui.Player = this;

        Transform cameraTransform = GameObject.Find("Camera").transform;
        cameraTransform.SetParent(transform);
        cameraTransform.localPosition = new Vector3(0, 1.67f, 0.7f); //for the 1-st person camera
        //cameraTransform.localPosition = new Vector3(0, 2f, -2.2f); //for the 3-rd person camera
    }

    public override void OnStopClient()
    {
        if (hasAuthority != true)
            return;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    [Command]
    public void CmdGetHandleObjectFromServer(int cellIndex)
    {
        LevelNetwork.SetItemInPlayerHand(connectionToClient.connectionId, cellIndex);
    }

    [ClientRpc]
    public void RpcDie()
    {
        //some die :)
    }

    private void SyncHealthWithUi(int oldValue, int newValue)
    {
        if (hasAuthority == true)
            ui.ChangeHealth(newValue);
    }

    private void SetItemInHands(int oldValue = 0, int newValue = 0)
    {
        Debug.Log("set item");
        HandleObject handleObject = Hand.GetComponentInChildren<HandleObject>();
        if (handleObject != null)
        {
            Destroy(handleObject.gameObject);
        }

        if (HandleObjectId == 0)
        {
            Debug.Log("0 object");
            return;
        }

        GameObject newItem = Instantiate(Resources.Load<Item>("HandleObjects/" + HandleObjectId).ItemPrefab, Hand);
        newItem.transform.localPosition = Vector3.zero;
        Destroy(newItem.transform.GetComponent<Collider>());
    }

    [Server]
    private void GetItem(int id, GameObject itemObj)
    {
        int newItemIndex = LevelNetwork.AddItemToPlayer(connectionToClient.connectionId, id);
        if (newItemIndex != -1)
        {
            RpcUiUpdate(newItemIndex, itemObj.GetComponent<HandleObject>().HandleItem.Id);
            NetworkServer.Destroy(itemObj);
        }
    }

    [ClientRpc]
    public void RpcUiUpdate(int cellIndex, int itemObjId)
    {
        ui?.SetSpriteToCell(cellIndex, itemObjId);
        ui?.AddItem(cellIndex);
    }

    [Client]
    private void FixedUpdate()
    {
        if (hasAuthority != true)
            return;

        if (Input.GetMouseButtonDown(1))
        {
            if (SelectedCell == -1)
                return;

            CmdRemove(SelectedCell);
            ui.RemoveItem(SelectedCell);
            if (ui.GetCellAmounts(SelectedCell) == 0)
            {
                Destroy(Hand.GetComponentInChildren<HandleObject>().gameObject);
            }
        }

        float x = Input.GetAxis("Horizontal");
        float y = Gravity;
        float z = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(x * Speed, y, z * Speed);
        movement = transform.TransformDirection(movement);
        Debug.Log("move = " + movement);
        characterController.MovePosition(transform.position + new Vector3(movement.x, Gravity, movement.z) * Time.fixedDeltaTime);

        CmdCheckMovement(transform.position);

        if (Input.GetMouseButtonDown(0))
        {
            float rotationX = GetComponentsInChildren<MouseLook>()[1].RotationX * -1;
            CmdShoot(transform.forward + new Vector3(0, Mathf.Sin(rotationX * 3.1415f / 180f), 0), transform.rotation * new Vector3(0, 1.7f, 0.7f), simulatedObject.frameKeys[simulatedObject.frameKeys.Count - 1]);
        }
    }

    [Command]
    private void CmdShoot(Vector3 cameraRotation, Vector3 additionalVector, int frameId)
    {
        LevelNetwork.ShootSimulate(frameId, 1, connectionToClient.connectionId, cameraRotation, additionalVector);
    }

    [ClientRpc]
    public void RpcSetPosition(Vector3 firstPosition, Vector3 secondPosition, Quaternion rotation, float clientSubFrame)
    {
        transform.position = Vector3.Lerp(firstPosition, secondPosition, clientSubFrame);
        transform.rotation = rotation;
    }

    [Command]
    private void CmdCheckMovement(Vector3 playerPosition)
    {
        LevelNetwork.CheckPlayerMovement(connectionToClient.connectionId, playerPosition);
    }

    [Command]
    private void CmdRemove(int cellIndex)
    {
        LevelNetwork.RemoveItemFromPlayer(connectionToClient.connectionId, cellIndex);
    }

    [Server]
    private void OnTriggerEnter(Collider other)
    {
        HandleObject handleObject = other.GetComponent<HandleObject>();
        if (handleObject != null)
        {
            GetItem(handleObject.HandleItem.Id, handleObject.gameObject);
        }
    }
}
