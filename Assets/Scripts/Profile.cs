using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Profile : MonoBehaviour
{

    private HelperScript gameManager;
    public InputField firstName;
    public InputField lastName;
    public InputField nickName;
    public InputField city;
    public InputField birthDate;
    public Text gamesPlayed;
    private Player player;
    private string httpServer;
    private RootObject games;
    // Start is called before the first frame update
    void Start()
    {
        gameManager = (HelperScript)FindObjectOfType(typeof(HelperScript));
        httpServer = gameManager.GetHttpServer();
        player = gameManager.GetPlayer();
        GetGame();
        firstName.text = player.FirstName;
        lastName.text = player.LastName;
        nickName.text = player.NickName;
        city.text = player.City;
        birthDate.text = player.BirthDate;
        foreach (GamesPlayed g in games.games)
        {
            gamesPlayed.text = gamesPlayed.text + "\n" + g.playerId.Substring(0, 3) + " || " + g.dateStarted + " || " + g.dateEnded + " || " + g.difficult + " || " + g.score;
        }

    }

    private void GetGame()
    {
        using (UnityWebRequest httpClient = new UnityWebRequest(httpServer + "/api/Games/ByPlayerId?playerId="+player.Id, "GET"))
        {
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

            string info = "{\"games\":" + httpClient.downloadHandler.text + "}";
            //string info = httpClient.downloadHandler.text;

            games = JsonUtility.FromJson<RootObject>(info);

            httpClient.Dispose();
        }
    }

    private void UpdateState(string state, string id)
    {
        using (UnityWebRequest httpClient = new UnityWebRequest(httpServer + "/api/Player/State?state=" + state + "&id=" + id, "POST"))
        {
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

            Debug.Log(httpClient.responseCode);

            httpClient.Dispose();
        }
    }

    private void OnApplicationQuit()
    {
        UpdateState("0", player.Id);
    }

    public void OnReturnClicked()
    {
        SceneManager.LoadScene(1);
    }

    public void OnUpdateButtonClicked()
    {
        player.FirstName = firstName.text;
        player.LastName = lastName.text;
        player.NickName = nickName.text;
        player.City = city.text;
        player.BirthDate = birthDate.text;

        UpdatePlayer();
        SceneManager.LoadScene(1);
    }

    public void OnDeleteButtonClicked()
    {
        using (UnityWebRequest httpClient = new UnityWebRequest(httpServer + "/api/Player/Delete?id="+player.Id, "POST"))
        {
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

            Debug.Log(httpClient.responseCode);

            httpClient.Dispose();
        }
        using (UnityWebRequest httpClient = new UnityWebRequest(httpServer + "/api/Account/Delete?id="+player.Id, "POST"))
        {
            httpClient.certificateHandler = new ByPassCertificate();
            httpClient.SetRequestHeader("Authorization", "bearer " + gameManager.Token);
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

            Debug.Log(httpClient.responseCode);

            httpClient.Dispose();
        }

        gameManager.LogOutPlayer();
        SceneManager.LoadScene(0);
    }

    private void UpdatePlayer()
    {
        using (UnityWebRequest httpClient = new UnityWebRequest(httpServer + "/api/Player/Update", "POST"))
        {
            string jsonData = JsonUtility.ToJson(player);
            byte[] dataToSend = Encoding.UTF8.GetBytes(jsonData);
            httpClient.uploadHandler = new UploadHandlerRaw(dataToSend);
            httpClient.SetRequestHeader("Content-Type", "application/json");
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

            Debug.Log(httpClient.responseCode);

            httpClient.Dispose();
        }
    }
}
