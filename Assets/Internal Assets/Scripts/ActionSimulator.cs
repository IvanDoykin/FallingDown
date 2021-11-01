using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class ActionSimulator : NetworkBehaviour
{
    public static List<SimulatedObject> SimulatedObjects = new List<SimulatedObject>();

    [Server]
    public static void Simulate(int frameId, float clientSubFrame, Action action)
    {
        Debug.Log("frameId = " + frameId);
        if (frameId == Time.frameCount)
            frameId = Time.frameCount - 1;

        foreach (var simulatedObject in SimulatedObjects)
        {
            simulatedObject.SetTransform(frameId, clientSubFrame);
        }

        action.Invoke();

        foreach (var simulatedObject in SimulatedObjects)
        {
            simulatedObject.ResetTransform();
        }

        Debug.Log("all done");
    }

    [Server]
    private void FixedUpdate()
    {
        foreach (var simulatedObject in SimulatedObjects)
        {
            simulatedObject.AddFrame();
        }
    }
}
