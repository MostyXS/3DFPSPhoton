using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;


    // Start is called before the first frame update
    void Start()
    {
        if (!PhotonNetwork.IsConnectedAndReady) return;
        int randomPoint = Random.Range(-10, 10);
        PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(randomPoint, 0, randomPoint), Quaternion.identity);
        

        


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
