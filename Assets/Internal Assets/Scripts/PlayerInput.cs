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

    private UIToWorldWrapper ui;

    public override void OnStartLocalPlayer()
    {
        if (hasAuthority != true)
            return;

        ui = GameObject.Find("[UI]").GetComponent<UIToWorldWrapper>();
        ui.Player = this;

        Transform cameraTransform = GameObject.Find("Camera").transform;
        cameraTransform.SetParent(transform);
        //cameraTransform.localPosition = new Vector3(0, 1.67f, 0.7f); //for the 1-st person camera
        cameraTransform.GetComponent<MouseLook>().axes = RotationAxes.MouseY;
        cameraTransform.GetComponent<MouseLook>().player = this;
        cameraTransform.localPosition = new Vector3(0, 2f, -2.2f); //for the 3-rd person camera
    }

    public override void OnStopClient()
    {
        if (hasAuthority != true)
            return;

        Transform cameraTransform = GameObject.Find("Camera").transform;
        cameraTransform.SetParent(transform.root);
        cameraTransform.position = Vector3.zero;

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

        CmdMove(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")));
    }

    [Command]
    private void CmdRemove(int cellIndex)
    {
        LevelNetwork.RemoveItemFromPlayer(connectionToClient.connectionId, cellIndex);
    }

    [Command]
    private void CmdMove(Vector2 direction)
    {
        LevelNetwork.MovePlayer(connectionToClient, direction);
    }

    [Command]
    public void CmdRotate(Vector3 rotation, Transform obj)
    {
        obj.transform.rotation *= Quaternion.Euler(rotation);
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
