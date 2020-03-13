using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HelperScript : MonoBehaviour
{
    public const string httpserver = "http://valentinclickycratesapi.azurewebsites.net/";
    private Player player;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    public string GetHttpServer()
    {
        return httpserver;
    }

    private string _token;
    public string Token
    {
        get { return _token; }
        set { _token = value; }
    }

    private string _playerID;
    public string PlayerId
    {
        get { return _playerID; }
        set { _playerID = value; }
    }

    public void onRegisterButton()
    {
        SceneManager.LoadScene(3);
    }

    public void LogInPlayer(Player player) { this.player = player; }

    public void LogOutPlayer() { this.player = null; }

    public Player GetPlayer() { return this.player; }
}
