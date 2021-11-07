using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationFrameData
{
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }

    public SimulationFrameData()
    {
        Position = Vector3.zero;
        Rotation = new Quaternion(0, 0, 0, 0);
    }

    public SimulationFrameData(Vector3 position, Quaternion quaternionRotation)
    {
        Position = position;
        Rotation = quaternionRotation;
    }
}
