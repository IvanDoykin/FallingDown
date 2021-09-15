using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerInput : NetworkBehaviour
{
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

    private int activeCell = -1;
    public int ActiveCell
    {
        get
        {
            return activeCell;
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
    private CharacterController characterController;
    Vector3 hackPos;

    public override void OnStartLocalPlayer()
    {
        if (hasAuthority != true)
            return;

        hackPos = transform.position;

        characterController = GetComponent<CharacterController>();
        ui = GameObject.Find("[UI]").GetComponent<UIToWorldWrapper>();
        ui.Player = this;

        Transform cameraTransform = GameObject.Find("Camera").transform;
        cameraTransform.SetParent(transform);
        //cameraTransform.localPosition = new Vector3(0, 1.67f, 0.7f); //for the 1-st person camera
        cameraTransform.localPosition = new Vector3(0, 2f, -2.2f); //for the 3-rd person camera
    }

    public override void OnStopClient()
    {
        if (hasAuthority != true)
            return;

        //Transform cameraTransform = GameObject.Find("Camera").transform;
        //cameraTransform.SetParent(transform.root);
        //cameraTransform.position = Vector3.zero;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    [Command]
    public void CmdSetActiveCell(int newActiveCell)
    {
        activeCell = newActiveCell;
    }

    [Command]
    public void CmdGetHandleObjectFromServer(int cellIndex)
    {
        LevelNetwork.SetItemInPlayerHand(connectionToClient.connectionId, cellIndex);
    }

    [ClientRpc]
    public void RpcDie()
    {
        Debug.LogError("You died");
        Debug.LogError("You died");
        Debug.LogError("You died");
        Debug.LogError("You died");
        Debug.LogError("You died");
        Debug.LogError("You died");
        Debug.LogError("You died");
        Debug.LogError("You died");
        Debug.LogError("You died");
        Debug.LogError("You died");
        Debug.LogError("You died");
        Debug.LogError("You died");
        Debug.LogError("You died");
        Debug.LogError("You died");
        Debug.LogError("You died");
    }

    [ClientRpc]
    public void RpcSetItemInHands(NetworkIdentity identity, PlayerInput player)
    {
        HandleObject handleObject = player.Hand.GetComponentInChildren<HandleObject>();
        if (handleObject != null)
            NetworkServer.Destroy(handleObject.gameObject);

        identity.transform.SetParent(player.Hand);
        identity.transform.localPosition = Vector3.zero;
        Destroy(identity.transform.GetComponent<Collider>());
    }

    [Command]
    private void CmdGetItem(int id, GameObject itemObj)
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
    }

    [Client]
    private void FixedUpdate()
    {
        if (hasAuthority != true)
            return;

        if (Input.GetKeyDown(KeyCode.V))
        {
            CmdRemove(2);
        }

        float x = Input.GetAxis("Horizontal");
        float y = Gravity;
        float z = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(x * Speed, y, z * Speed);
        movement *= Time.fixedDeltaTime;
        characterController.Move(movement);

        CmdCheckMovement(transform.position);
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

    [Client]
    private void OnTriggerEnter(Collider other)
    {
        HandleObject handleObject = other.GetComponent<HandleObject>();
        if (handleObject != null)
        {
            CmdGetItem(handleObject.HandleItem.Id, handleObject.gameObject);
        }
    }
}
