using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Register : MonoBehaviour
{

    public int semaforo = 0;
    public InputField emailInputField;
    public InputField passwordInputField;
    public InputField confirmInputField;
    public InputField firstNameInputField;
    public InputField lastNameInputField;
    public InputField nickNameInputField;
    public InputField cityInputField;
    public InputField birthInputField;
    private HelperScript gameManager;
    public Text messageBoardText;
    public Image avatar;


    private Player player = new Player();

    private string[] images = { "https://valentinstorage.blob.core.windows.net/cliclycratesblol/default/bulbasur.png", "https://valentinstorage.blob.core.windows.net/cliclycratesblol/default/charmander.png", "https://valentinstorage.blob.core.windows.net/cliclycratesblol/default/pikachu.png", "https://valentinstorage.blob.core.windows.net/cliclycratesblol/default/squirtle.png" };


    private int image = 0;

    string httpServer;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = (HelperScript)FindObjectOfType(typeof(HelperScript));
        httpServer = gameManager.GetHttpServer();
        StartCoroutine(LoadImage());
    }

    void Update()
    {
        if (this.semaforo == 3)
        {
            if (!messageBoardText.text.Contains("400"))
            {
                SceneManager.LoadScene(0);
            }
            else
            {
                this.semaforo = 0;
            }
        }
    }

    public void OnNextImageClicked()
    {
        if(image == 3)
        {
            image = 0;
        }
        else
        {
            image++;
        }
        StartCoroutine(LoadImage());
    }
    public void OnPreviousImageClicked()
    {
        if(image == 0)
        {
            image = 3;
        }
        else
        {
            image--;
        }
        StartCoroutine(LoadImage());
    }

    
    public void OnRegisterButtonClicked()
    {
        StartCoroutine(Registracion());
    }
    
    public void OnBackButtonClicked()
    {
        SceneManager.LoadScene(0);
    }

    private IEnumerator Registracion()
    {
        UnityWebRequest httpClient = new UnityWebRequest(httpServer + "/api/Account/Register", "POST");
        if (string.IsNullOrEmpty(emailInputField.text))
        {
            throw new NullReferenceException("Email can't be void");
        }
        AspNetUserRegister newUser = new AspNetUserRegister();
        newUser.Email = emailInputField.text;
        newUser.Password = passwordInputField.text;
        newUser.ConfirmPassword = confirmInputField.text;

        string jsonData = JsonUtility.ToJson(newUser);
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

        messageBoardText.text += "\n" + httpClient.responseCode;

        httpClient.Dispose();
        this.semaforo++;

        AccountId();
        StartCoroutine(RegisterPlayer());
    }

    private void AccountId()
    {
        UnityWebRequest httpClient = new UnityWebRequest(httpServer + "/api/Player/GetAccountId?email="+emailInputField.text, "GET");
        httpClient.certificateHandler = new ByPassCertificate();

        httpClient.downloadHandler = new DownloadHandlerBuffer();


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
        this.player.Id = httpClient.downloadHandler.text;

        httpClient.Dispose();
        this.semaforo++;
    }

    private IEnumerator RegisterPlayer()
    {
        UnityWebRequest httpClient = new UnityWebRequest(httpServer + "/api/Player", "POST");

        this.player.NickName = nickNameInputField.text;
        this.player.FirstName = firstNameInputField.text;
        this.player.LastName = lastNameInputField.text;
        this.player.Email = emailInputField.text;
        this.player.City = cityInputField.text;
        this.player.BirthDate = birthInputField.text;
        this.player.DateJoined = DateTime.Now.ToString("dd/MM/yyyy");
        this.player.BlobUri = images[image];


        string jsonData = JsonUtility.ToJson(this.player);
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

        messageBoardText.text += "\n" + httpClient.responseCode;

        httpClient.Dispose();

        this.semaforo++;

    }

    private IEnumerator LoadImage()
    {
        using(UnityWebRequest httpClient = new UnityWebRequest(images[image]))
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
}
