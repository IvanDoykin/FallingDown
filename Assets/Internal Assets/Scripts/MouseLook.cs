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

    private const float minimumVert = -89f;
    private const float maximumVert = 89f;

    Vector2 rotation = Vector2.zero;

    [Client]
    private void FixedUpdate()
    {
        rotation.x -= Input.GetAxis("Mouse Y") * sensitivityVertical;
        rotation.x = Mathf.Clamp(rotation.x, minimumVert, maximumVert);
        float rotationY = transform.localEulerAngles.y;
        transform.localEulerAngles = new Vector3(rotation.x, rotationY, 0);
    }
}
