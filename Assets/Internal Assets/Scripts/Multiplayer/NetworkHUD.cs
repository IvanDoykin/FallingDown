using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

[RequireComponent(typeof(NetworkManager))]
public class NetworkHUD : MonoBehaviour
{
    private NetworkManager manager;

    [SerializeField] private GameObject startHud;
    [SerializeField] private GameObject connectionHud;
    [SerializeField] private TMP_InputField text;

    private void Awake()
    {
        manager = GetComponent<NetworkManager>();
    }

    private void Start()
    {
        manager.networkAddress = "localhost";
    }

    public void StartHost()
    {
        manager.networkAddress = text.text;
        startHud.SetActive(false);
        manager.StartHost();
    }

    public void StartServer()
    {
        manager.networkAddress = text.text;
        startHud.SetActive(false);
        manager.StartServer();
    }

    public void StartClient()
    {
        manager.networkAddress = text.text;
        startHud.SetActive(false);
        connectionHud.SetActive(true);
        manager.StartClient();
    }

    public void StopClient()
    {
        manager.StopClient();
        startHud.SetActive(true);
        connectionHud.SetActive(false);
    }

    public void StopServer()
    {
        manager.StopServer();
        startHud.SetActive(true);
        connectionHud.SetActive(false);

    }

    public void StopHost()
    {
        manager.StopHost();
        startHud.SetActive(true);
        connectionHud.SetActive(false);
    }
}
