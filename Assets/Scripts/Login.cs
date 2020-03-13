using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text;

public class Login : MonoBehaviour
{
    private int semaforo = 0;
    private bool loged = false;
    private string httpServerAddress;
    public HelperScript gameManager;
    // Cached references
    public InputField emailInputField;
    public InputField passwordInputField;
    public Button loginButton;
    void Start()
    {
        gameManager = (HelperScript)FindObjectOfType(typeof(HelperScript));
        httpServerAddress = gameManager.GetHttpServer();
        this.semaforo = 0;
        this.loged = false;
    }
    public void OnLoginButtonClicked()
    {
        TryLogin();
        while(this.semaforo != 1)
        {
            Task.Delay(1);
        }
        if(loged == true)
        {
            //GetNickName();
            //GetAvatar();
            GetPlayer();
            SceneManager.LoadScene(1);
        }
        else
        {
            this.semaforo = 0;
        }
    }

    private void GetPlayer()
    {
        UnityWebRequest httpClient = new UnityWebRequest(httpServerAddress + "/api/Player?email=" + emailInputField.text, "GET");

        httpClient.downloadHandler = new DownloadHandlerBuffer();

        httpClient.SetRequestHeader("Accept", "application/json");

        httpClient.certificateHandler = new ByPassCertificate();

        httpClient.SendWebRequest();

        while (!httpClient.isDone)
        {
            Task.Delay(1);
        }

        if (httpClient.isNetworkError || httpClient.isHttpError)
        {
            //throw new Exception("OnRegisterButtonClick: Network Error");
            Debug.Log(httpClient.isNetworkError);
            Debug.Log(httpClient.isHttpError);
            Debug.Log(httpClient.error);
        }

        string info = httpClient.downloadHandler.text;

        Player player = new Player();

        player = JsonUtility.FromJson<Player>(info);

        gameManager.LogInPlayer(player);

        httpClient.Dispose();
    }

    /*private void GetNickName()
    {
        UnityWebRequest httpClient = new UnityWebRequest(httpServerAddress + "/api/Player/Nickname?email="+emailInputField.text, "GET");

        httpClient.downloadHandler = new DownloadHandlerBuffer();

        httpClient.certificateHandler = new ByPassCertificate();

        httpClient.SendWebRequest();

        while (!httpClient.isDone)
        {
            Task.Delay(1);
        }

        if (httpClient.isNetworkError || httpClient.isHttpError)
        {
            //throw new Exception("OnRegisterButtonClick: Network Error");
            Debug.Log(httpClient.isNetworkError);
            Debug.Log(httpClient.isHttpError);
            Debug.Log(httpClient.error);
        }

        Player player = new Player();

        string nickName = httpClient.downloadHandler.text.Replace("\"", "");

        player.NickName = nickName;

        gameManager.LogInPlayer(player);
    }

    private void GetAvatar()
    {
        UnityWebRequest httpClient = new UnityWebRequest(httpServerAddress + "/api/Player/Avatar?email=" + emailInputField.text, "GET");

        httpClient.downloadHandler = new DownloadHandlerBuffer();

        httpClient.certificateHandler = new ByPassCertificate();

        httpClient.SendWebRequest();

        while (!httpClient.isDone)
        {
            Task.Delay(1);
        }

        if (httpClient.isNetworkError || httpClient.isHttpError)
        {
            //throw new Exception("OnRegisterButtonClick: Network Error");
            Debug.Log(httpClient.isNetworkError);
            Debug.Log(httpClient.isHttpError);
            Debug.Log(httpClient.error);
        }

        Player player = gameManager.GetPlayer();

        string url = httpClient.downloadHandler.text.Replace("\"", "");

        player.BlobUri = url;

        gameManager.LogInPlayer(player);
    }*/

    private void TryLogin()
    {
        if (string.IsNullOrEmpty(gameManager.Token))
        {
            GetToken();
        }
        UnityWebRequest httpClient = new UnityWebRequest(httpServerAddress + "/api/Account/UserId", "GET");
        httpClient.SetRequestHeader("Authorization", "bearer " + gameManager.Token);
        httpClient.SetRequestHeader("Accept", "application/json");

        httpClient.downloadHandler = new DownloadHandlerBuffer();
        httpClient.certificateHandler = new ByPassCertificate();
        httpClient.SendWebRequest();


        while (!httpClient.isDone)
        {
            Task.Delay(1);
        }

        if (httpClient.isNetworkError || httpClient.isHttpError)
        {
            Debug.Log(httpClient.error);
        }
        else
        {
            gameManager.PlayerId = httpClient.downloadHandler.text;
            Debug.Log(gameManager.PlayerId + " Bienvenido");
            loginButton.interactable = false;
            loged = true;
        }

        httpClient.Dispose();
        this.semaforo = 1;
    }

    private void GetToken()
    {
        UnityWebRequest httpClient = new UnityWebRequest(httpServerAddress + "/Token", "POST");

        WWWForm dataToSend = new WWWForm();
        dataToSend.AddField("grant_type", "password");
        dataToSend.AddField("username", emailInputField.text);
        dataToSend.AddField("password", passwordInputField.text);

        httpClient.uploadHandler = new UploadHandlerRaw(dataToSend.data);
        httpClient.downloadHandler = new DownloadHandlerBuffer();

        httpClient.SetRequestHeader("Accept", "application/json");
        httpClient.certificateHandler = new ByPassCertificate();
        httpClient.SendWebRequest();

        while (!httpClient.isDone)
        {
            Task.Delay(1);
        }


        if (httpClient.isNetworkError || httpClient.isHttpError)
        {
            Debug.Log(httpClient.error);
        }
        else
        {
            string jsonResponse = httpClient.downloadHandler.text;
            AuthorizationToken authToken = JsonUtility.FromJson<AuthorizationToken>(jsonResponse);
            gameManager.Token = authToken.access_token;
        }

        httpClient.Dispose();
    }

    public void OnQuitButtonClicked()
    {
        Application.Quit();
    }
}
