using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    HelperScript gameManager;
    string httpServer;
    public Text nickname;
    public Player player;
    public Image avatar;
    public Text scores;
    private int totalGames;

    private RootObject games = new RootObject();
    // Start is called before the first frame update
    void Start()
    {
        gameManager = (HelperScript)FindObjectOfType(typeof(HelperScript));
        httpServer = gameManager.GetHttpServer();
        player = gameManager.GetPlayer();
        StartCoroutine(LoadImage(player.BlobUri));
        nickname.text = player.NickName;
        UpdateState("1", player.Id);
        GetGame();

        foreach(GamesPlayed g in games.games)
        {
            scores.text = scores.text + "\n"+g.playerId.Substring(0,3) + " || " + g.dateStarted + " || " + g.dateEnded + " || " + g.difficult + " || " + g.score;
        }

    }

    public void OnProfileButtonClicked()
    {
        SceneManager.LoadScene(4);
    }

    public void OnSartButtonClicked()
    {
        UpdateState("2", player.Id);
        SceneManager.LoadScene(2);
    }

    public void OnLogOutButtonClicked()
    {
        UpdateState("0", player.Id);
        gameManager.LogOutPlayer();
        SceneManager.LoadScene(0);
    }

    private IEnumerator LoadImage(string avatarUri)
    {
        using (UnityWebRequest httpClient = new UnityWebRequest(avatarUri))
        {
            httpClient.downloadHandler = new DownloadHandlerTexture();
            httpClient.certificateHandler = new ByPassCertificate();
            yield return httpClient.SendWebRequest();
            
            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                //throw new Exception("OnRegisterButtonClick: Network Error");
                Debug.Log(httpClient.isNetworkError);
                Debug.Log(httpClient.isHttpError);
                Debug.Log(httpClient.error);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(httpClient);
                avatar.sprite = Sprite.Create(texture,
                    new Rect(0.0f, 0.0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100.0f);
            }
        }
    }

    private void GetGame()
    {
        using(UnityWebRequest httpClient = new UnityWebRequest(httpServer + "/api/Games", "GET"))
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
        using(UnityWebRequest httpClient = new UnityWebRequest(httpServer + "/api/Player/State?state="+state+"&id="+id, "POST"))
        {
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

            Debug.Log(httpClient.responseCode);

            httpClient.Dispose();
        }
    }

    private void OnApplicationQuit()
    {
        UpdateState("0", player.Id);
    }
}
