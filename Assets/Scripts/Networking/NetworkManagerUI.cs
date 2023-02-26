using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class NetworkManagerUI : NetworkBehaviour
{
    [SerializeField]
    Button serverButton;
    [SerializeField]
    Button hostButton;
    [SerializeField]
    Button clientButton;
    [SerializeField]
    GameObject player;

    private void Awake()
    {
        serverButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
        });
        hostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            
        });
        clientButton.onClick.AddListener(() =>
        {
            //NetworkManager.Singleton.StartClient();
            GameObject temp = Instantiate(player);
            temp.GetComponent<NetworkObject>().SpawnAsPlayerObject(OwnerClientId);
        });
    }


}
