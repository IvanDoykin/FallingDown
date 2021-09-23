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

    private float rotationX = 0;

    [Client]
    private void FixedUpdate()
    {
        if (axes == RotationAxes.MouseX)
        {
            transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityHorizontal, 0);
        }

        if (axes == RotationAxes.MouseY)
        {
            rotationX -= Input.GetAxis("Mouse Y") * sensitivityVertical;
            rotationX = Mathf.Clamp(rotationX, minimumVert, maximumVert);
            float rotationY = transform.localEulerAngles.y;
            transform.localEulerAngles = new Vector3(rotationX, rotationY, 0);
        }
    }
}
