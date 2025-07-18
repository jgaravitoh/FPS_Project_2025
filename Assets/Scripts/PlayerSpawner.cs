using UnityEngine;
using Photon.Pun;
using System.Collections;
public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner Instance;
    private void Awake()
    {
        Instance = this;
    }

    public GameObject playerPrefab;
    private GameObject player;
    public GameObject deathEffect;

    public float respawnTime = 5f;

    private void Start()
    {
        if (PhotonNetwork.IsConnected) 
        {
            StartCoroutine(startSpawnPlayer());
        }
    }
    public IEnumerator startSpawnPlayer()
    {
        Debug.Log("=== PhotonNetwork.InRoom:" + PhotonNetwork.InRoom);

        while (!PhotonNetwork.InRoom)
        {
            yield return null;
        }
        Debug.Log("=== spawn player");
        Transform spawnPoint = SpawnManager.instance.GetSpawnPoint();

        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);

    }
    public void Die(string damager)
    {
        
        UIController.instance.deathText.text = "You were killed by " + damager;

        //PhotonNetwork.Destroy(player);
        //Invoke(nameof(SpawnPlayer), 3);
        MatchManager.instance.UpdateStatsSend(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);

        if (player != null)
        {
            StartCoroutine(DieCoroutine());
        }
    }

    public IEnumerator DieCoroutine()
    {
        PhotonNetwork.Instantiate(deathEffect.name, player.transform.position, Quaternion.identity);

        PhotonNetwork.Destroy(player);

        UIController.instance.deathScreen.SetActive(true);
        
        yield return new WaitForSeconds(respawnTime);

        UIController.instance.deathScreen.SetActive(false);

        StartCoroutine(startSpawnPlayer());
    }
}
