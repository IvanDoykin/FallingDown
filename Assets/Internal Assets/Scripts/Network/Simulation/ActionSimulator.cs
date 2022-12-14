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

    public static void SyncClientStartFrame(SimulatedObject simulatedObject)
    {
        simulatedObject.StartClientFrame = Time.frameCount;
    }

    private void FixedUpdate()
    {
        foreach (var simulatedObject in SimulatedObjects)
        {
            simulatedObject.AddFrame();
        }
    }
}
