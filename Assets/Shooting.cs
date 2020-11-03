using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shooting : MonoBehaviourPunCallbacks
{
    [SerializeField] Camera FPS_Camera;
    [SerializeField] GameObject hitEffect;

    [Header("Health-related Stuff")]
    [SerializeField] float startHealth = 100f;
    [SerializeField] Image healthBar = null;

    float health;

    Animator anim;
    MovementController mover;

    private void Awake()
    {
        mover = GetComponent<MovementController>();
        anim = GetComponent<Animator>();
        health = startHealth;
        healthBar.fillAmount = health / startHealth;
    }

    public void Fire()
    {
        RaycastHit hit;
        Ray ray = FPS_Camera.ViewportPointToRay(new Vector3(.5f, .5f));
        if (Physics.Raycast(ray, out hit, 100))
        {
            photonView.RPC("CreateHitEffect", RpcTarget.All, hit.point);
            Collider hitCollider = hit.collider;

            if(hitCollider.CompareTag("Player") && !hitCollider.GetComponent<PhotonView>().IsMine)
            {
                hitCollider.GetComponent<PhotonView>().RPC("TakeDamage",RpcTarget.AllBuffered, 15f);
            }
        }
    }
    [PunRPC]
    public void TakeDamage(float damage, PhotonMessageInfo info)
    {
        health -= damage;
        healthBar.fillAmount = health / startHealth;

        if(health<=0)
        {
            Debug.Log(info.Sender.NickName + " Killed " + info.photonView.Owner.NickName);
            Die();
        }
    }
    [PunRPC]
    public void CreateHitEffect(Vector3 position)
    {
        Instantiate(hitEffect, position, Quaternion.identity);

    }
    private void Die()
    {
        if(photonView.IsMine)
        {
            anim.SetBool("IsDead", true);
            StartCoroutine(Respawn());
        }
    }

    IEnumerator Respawn()
    {
        GameObject respawnText = GameObject.Find("RespawnText");
        mover.enabled = false;
        GetComponent<PlayerSetup>().SetUIActive(false);
        float respawnTime = 8f;
        while(respawnTime >0)
        {
            respawnTime -= Time.deltaTime;
            yield return null;
            respawnText.GetComponent<Text>().text = "You are killed. Respawning at: " + (int)respawnTime;
        }
        respawnText.GetComponent<Text>().text = "";
        anim.SetBool("IsDead", false);
        mover.enabled = true;
        GetComponent<PlayerSetup>().SetUIActive(true);
        float randomPoint = Random.Range(-20f, 20f);
        transform.position = new Vector3(-randomPoint, 0, randomPoint);
        photonView.RPC("RegainHealth", RpcTarget.AllBuffered);

    }
    [PunRPC]
    void RegainHealth()
    {
        
        health = startHealth;
        healthBar.fillAmount = health / startHealth;
    }

}
