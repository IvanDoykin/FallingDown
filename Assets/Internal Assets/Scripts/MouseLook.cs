using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public enum RotationAxes
{
    MouseX = 0,
    MouseY = 1
}

public class MouseLook : NetworkBehaviour
{
    public RotationAxes axes;

    [Range(1, 12)]
    [SerializeField] private float sensitivityHorizontal = 9.0f;
    public float SensitivityHorizontal
    {
        get
        {
            return sensitivityHorizontal;
        }
    }

    [Range(1, 12)]
    [SerializeField] private float sensitivityVertical = 9.0f;
    public float SensitivityVertical
    {
        get
        {
            return sensitivityVertical;
        }
    }

    private const float minimumVert = -90f;
    private const float maximumVert = 90f;

    public PlayerInput player;
    Vector2 rotation = Vector2.zero;
    [Client]
    private void Start()
    {
        if (player != null)
            return;

        player = GetComponent<PlayerInput>();
        axes = RotationAxes.MouseX;
    }

    [Client]
    private void FixedUpdate()
    {
        if (hasAuthority != true && axes != RotationAxes.MouseY)
            return;

        switch (axes)
        {
            case RotationAxes.MouseX:
                player.CmdRotate(new Vector3(0, Input.GetAxis("Mouse X") * sensitivityHorizontal, 0), transform);
                break;

            case RotationAxes.MouseY:
                rotation.y += Input.GetAxis("Mouse Y") * sensitivityVertical;
                rotation.y = Mathf.Clamp(rotation.y, minimumVert, maximumVert);
                var xQuat = Quaternion.AngleAxis(rotation.x, Vector3.up);
                var yQuat = Quaternion.AngleAxis(rotation.y, Vector3.left);

                transform.localRotation = xQuat * yQuat;
                break;
        }
    }
}
