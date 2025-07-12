using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class MatchManager : MonoBehaviour
{
    public static MatchManager instance;
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        if (!PhotonNetwork.IsConnected) { SceneManager.LoadScene("Main Menu"); }
    }

    void Update()
    {
        
    }
}
