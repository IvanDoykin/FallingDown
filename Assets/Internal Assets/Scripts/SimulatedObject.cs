using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SimulatedObject : MonoBehaviour
{
    private const int SimulationFramesAmount = 60;

    public Dictionary<int, SimulationFrameData> framesData = new Dictionary<int, SimulationFrameData>();
    public List<int> frameKeys = new List<int>();

    private SimulationFrameData lastSavedFrame = new SimulationFrameData();

    private void OnEnable()
    {
        ActionSimulator.SimulatedObjects.Add(this);
    }

    private void OnDisable()
    {
        ActionSimulator.SimulatedObjects.Remove(this);
    }

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
    }

    [Server]
    public void SetTransform(int frameId, float clientSubFrame)
    {
        Debug.Log("frameId = " + frameId);
        Debug.Log("lastFrame = " + frameKeys[frameKeys.Count - 1]);

        if (frameKeys[frameKeys.Count - 1] - frameId > 60)
            throw new UnityException("Ohhh...Ur ping is very big");

        lastSavedFrame.Position = transform.position;
        lastSavedFrame.Rotation = transform.rotation;

        SimulationFrameData simulationFrameData1;
        if (framesData.TryGetValue(frameId - 1, out simulationFrameData1) == false)
            return;

        SimulationFrameData simulationFrameData2;
        if (framesData.TryGetValue(frameId, out simulationFrameData2) == false)
            return;

        Debug.Log("Set transform");
        if (GetComponent<Player>() != null)
        {
            GetComponent<Player>().transform.GetComponent<Animator>().enabled = false;
        }

        else
        {
            transform.position = Vector3.Lerp(simulationFrameData1.Position, simulationFrameData2.Position, clientSubFrame);
            transform.rotation = simulationFrameData1.Rotation;
        }
    }

    [Server]
    public void ResetTransform()
    {
        transform.position = lastSavedFrame.Position;
        transform.rotation = lastSavedFrame.Rotation;

        if (GetComponent<Player>() != null)
        {
            GetComponent<Player>().transform.GetComponent<Animator>().enabled = true;
        }
    }
}
