using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSetup : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject[] FPS_Hands_ChildGameObjects;
    [SerializeField] GameObject[] SoliderChildGameObjects;

    [SerializeField] GameObject playerUIPrefab;
    [SerializeField] Camera FPSCamera;

    MovementController playerMovementController;
    Animator anim;
    GameObject playerUIGameObject;
    private void Awake()
    {
        anim = GetComponent<Animator>();
        playerMovementController = GetComponent<MovementController>();

    }


    void Start()
    {

        if (photonView.IsMine)
        {
            anim.SetBool("IsFirstPerson", true);
             playerUIGameObject = Instantiate(playerUIPrefab);
            playerMovementController.SetJoystickAndTouchField(playerUIGameObject.GetComponentInChildren<Joystick>(), playerUIGameObject.GetComponentInChildren<FixedTouchField>());
            playerUIGameObject.GetComponentInChildren<Button>().onClick.AddListener(GetComponent<Shooting>().Fire);
            FPSCamera.enabled = true;
        } 
        else
        {
            anim.SetBool("IsSolider", true);
            playerMovementController.enabled = false;
            FPSCamera.enabled = false;
        }
        foreach (GameObject go in FPS_Hands_ChildGameObjects)
        {
            go.SetActive(photonView.IsMine);
        }
        foreach(GameObject go in SoliderChildGameObjects)
        {
            go.SetActive(!photonView.IsMine);
        }
    }
    public void SetUIActive(bool value)
    {
        playerUIGameObject.SetActive(value);
    }
}
