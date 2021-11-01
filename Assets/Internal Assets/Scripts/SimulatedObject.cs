using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SimulatedObject : NetworkBehaviour
{
    private const int SimulationFramesAmount = 60;

    public Dictionary<int, SimulationFrameData> framesData = new Dictionary<int, SimulationFrameData>();
    protected List<int> frameKeys = new List<int>();

    private SimulationFrameData lastSavedFrame = new SimulationFrameData();

    [Server]
    private void OnEnable()
    {
        ActionSimulator.SimulatedObjects.Add(this);
    }

    [Server]
    private void OnDisable()
    {
        ActionSimulator.SimulatedObjects.Remove(this);
    }

    [Server]
    public void AddFrame()
    {
        if (frameKeys.Contains(Time.frameCount))
            return;

        if (frameKeys.Count >= SimulationFramesAmount)
        {
            int key = frameKeys[0];
            frameKeys.RemoveAt(0);
            framesData.Remove(key);
        }

        frameKeys.Add(Time.frameCount);
        framesData.Add(Time.frameCount, new SimulationFrameData(
            transform.position, transform.rotation));

        Debug.Log("added " + Time.frameCount + " frameKey");
    }

    [Server]
    public void SetTransform(int frameId, float clientSubFrame)
    {
        lastSavedFrame.Position = transform.position;
        lastSavedFrame.Rotation = transform.rotation;

        Debug.Log("============");
        foreach (var x in frameKeys)
        {
            Debug.Log(x);
        }
        Debug.Log("============");
        Debug.Log("******");
        foreach (var x in framesData)
        {
            Debug.Log("----------");
            Debug.Log(x.Key);
            Debug.Log(x.Value);
            Debug.Log("----------");
        }
        Debug.Log("******");

        SimulationFrameData simulationFrameData1;
        if (framesData.TryGetValue(frameId - 1, out simulationFrameData1) == false)
            return;

        SimulationFrameData simulationFrameData2;
        if (framesData.TryGetValue(frameId, out simulationFrameData2) == false)
            return;

        Vector3 pos = simulationFrameData1.Position;
        Vector3 pos2 = simulationFrameData2.Position;
        transform.position = Vector3.Lerp(simulationFrameData1.Position, simulationFrameData2.Position, clientSubFrame);
        transform.rotation = simulationFrameData1.Rotation;
    }

    [Server]
    public void ResetTransform()
    {
        transform.position = lastSavedFrame.Position;
        transform.rotation = lastSavedFrame.Rotation;
    }
}
