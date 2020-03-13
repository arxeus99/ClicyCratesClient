using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    
    public List<GameObject> targets;
    private float spawnRate = 1.0f;
    private int score;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI nicknameText;
    public TextMeshProUGUI gameOverText;
    public bool isGameActive;
    public Button restartButton;
    public Button returnButton;
    public GameObject titleScreen;
    private HelperScript helper;
    private GamesPlayed game;
    private Player player;
    private string httpServer;
    public Image avatar;
    private RootObjectPlayer players;
    public Text onlinePlayers;
    public Image lvl;
    public GameObject onlineObject;

    private string[] medals = { "https://valentinstorage.blob.core.windows.net/cliclycratesblol/default/bronze.png", "https://valentinstorage.blob.core.windows.net/cliclycratesblol/default/silver.png", "https://valentinstorage.blob.core.windows.net/cliclycratesblol/default/golden.png" };

    // Start is called before the first frame update
    void Start()
    {
        returnButton.gameObject.SetActive(true);
        game = new GamesPlayed();
        helper = (HelperScript)FindObjectOfType(typeof(HelperScript));
        httpServer = helper.GetHttpServer();
        player = helper.GetPlayer();
        StartCoroutine(LoadImage(player.BlobUri, avatar));
        game.playerId = player.Id;
        nicknameText.text = player.NickName;
        switch (player.TotalScore)
        {
            case int n when (n < 50):
                StartCoroutine(LoadImage(medals[0], lvl));
                break;
            case int n when (n > 49 && n < 500):
                StartCoroutine(LoadImage(medals[1], lvl));
                break;
            case int n when (n > 499):
                StartCoroutine(LoadImage(medals[2], lvl));
                break;
        }
    }

    

    // Update is called once per frame
    void Update()
    {
        scoreText.text = "SCORE: " + score;
        onlinePlayers.text = "";
        //players.players.Clear();
        StartCoroutine(LoadOnlinePlayers());
        Task.Delay(1000);
        foreach(Player p in players.players)
        {
            onlinePlayers.text = onlinePlayers.text+ "\n" + p.NickName + ": ";
            if (p.State.Equals("1"))
            {
                onlinePlayers.text = onlinePlayers.text + "On the main menu";
            }
            else
            {
                onlinePlayers.text = onlinePlayers.text + "Playing a game";
            }
        }
    }

    public void UpdateScore(int scoreToAdd)
    {
        score += scoreToAdd;
        scoreText.text = "SCORE: " + score;
    }

    IEnumerator SpawnTarget()
    {
        while (isGameActive)
        {
            yield return new WaitForSeconds(spawnRate);
            int index = UnityEngine.Random.Range(0, targets.Count);
            Instantiate(targets[index]);
        }
    }

    public void GameOver()
    {
        game.score = score;
        game.dateEnded = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        StartCoroutine(InsertGame(game));
        UpdatePlayerScore(score);
        isGameActive = false;
        returnButton.gameObject.SetActive(true);
        gameOverText.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(true);
    }

    public void OnReturnButtonClicked()
    {
        SceneManager.LoadScene(1);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void StartGame(int difficulty)
    {
        returnButton.gameObject.SetActive(false);
        game.dateStarted = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        game.difficult = difficulty;
        isGameActive = true;
        spawnRate /= difficulty;
        StartCoroutine(SpawnTarget());
        score = 0;
        UpdateScore(0);
        titleScreen.gameObject.SetActive(false);
    }


    private IEnumerator InsertGame(GamesPlayed game)
    {
        UnityWebRequest httpClient = new UnityWebRequest(httpServer + "/api/Games", "POST");

        string jsonData = JsonUtility.ToJson(game);
        byte[] dataToSend = Encoding.UTF8.GetBytes(jsonData);
        httpClient.uploadHandler = new UploadHandlerRaw(dataToSend);

        httpClient.SetRequestHeader("Content-Type", "application/json");

        httpClient.certificateHandler = new ByPassCertificate();
        yield return httpClient.SendWebRequest();  // Blocking call

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

    private IEnumerator LoadImage(string avatarUri, Image image)
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
                image.sprite = Sprite.Create(texture,
                    new Rect(0.0f, 0.0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100.0f);
            }
        }
    }

    private IEnumerator LoadOnlinePlayers()
    {
        using (UnityWebRequest httpClient = new UnityWebRequest(httpServer + "/api/Player/Online", "GET"))
        {
            httpClient.downloadHandler = new DownloadHandlerBuffer();
            httpClient.certificateHandler = new ByPassCertificate();
            yield return httpClient.SendWebRequest();
            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                //throw new Exception("OnRegisterButtonClick: Network Error");
                Debug.Log(httpClient.isNetworkError);
                Debug.Log(httpClient.isHttpError);
                Debug.Log(httpClient.error);
            }

            string info = "{\"players\":" + httpClient.downloadHandler.text + "}";

            players = JsonUtility.FromJson<RootObjectPlayer>(info);

            httpClient.Dispose();
        }
    }

    private void UpdateState(string state, string id)
    {
        using (UnityWebRequest httpClient = new UnityWebRequest(httpServer + "/api/Player/State?state=" + state + "&id=" + id, "POST"))
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

    private void UpdatePlayerScore(int scoreToAdd)
    {
        using(UnityWebRequest httpClient = new UnityWebRequest(httpServer+"/api/Player/Score?score="+scoreToAdd+"&id="+player.Id, "POST"))
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

}
