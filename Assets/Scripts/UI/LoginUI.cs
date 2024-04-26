using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static NetworkManager;

public class LoginUI : MonoBehaviour
{
    [SerializeField]   
    private TMP_InputField usernameTMP;
    [SerializeField]
    private TMP_InputField passwordTMP;
    [SerializeField]
    private TextMeshProUGUI errorTMP;
    private NetworkManager networkManager;    

    public string username;
    public string password;
    public bool isLoggedIn = false;

  
    // Start is called before the first frame update
    void Start()
    {
        networkManager = GameObject.FindAnyObjectByType<NetworkManager>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateLoginCredentials();
    }

    public void UpdateLoginCredentials() 
    {
        username = usernameTMP.text.Replace("\u200B", string.Empty);
        password = passwordTMP.text.Replace("\u200B", string.Empty);
    }

    public string GetErrorMessage(LoginErrors? error)
    {
        switch (error) 
        {
            case LoginErrors.Invalid_Credentials:
                return "Invalid Username or Password.";                    
            case LoginErrors.No_Response:
                return "Unable to connect to server.";                
            case LoginErrors.Unknown_Error:
                return "Unknown error";
            default:
                return "Default unknown error";
        }
    }
    public string GetRegisterErrorMessage(RegisterErrors? error)
    {
        switch (error)
        {
            case RegisterErrors.Username_Required:
                return "Username field is required";
            case RegisterErrors.Password_Required:
                return "Password fiels id required";
            case RegisterErrors.Invalid_Username:
                return "Invalid username. Must be valid e-mail address.";
            case RegisterErrors.Inavlid_Password:
                return "Invalid password. Must be between 6-100 characters";
            case RegisterErrors.No_Response:
                return "Unable to connect to server.";
            case RegisterErrors.Unknown_Error:
                return "Unknown error";
            default:
                return "Default unknown error";
        }
    }

    public async void OnLoginButton() 
    {
        UpdateLoginCredentials();

        LoginReponse login_response = await networkManager.Login(username, password);
        if (login_response.wasSuccess)
        {
            //login sucessful
            Debugging.Log("login succesful");
            SceneManager.LoadScene("MainMenu");
        }
        else 
        {
            HandleFailedLogin(login_response.error);
        }
    }

    public async void OnRegisterButton()
    {
        //await networkManager.GetHighscores("Sandy Slalom");
       //handle registration
    }

    public void OnContinueOffline() 
    {
        SceneManager.LoadScene("MainMenu");
    }


    public void HandleFailedLogin(NetworkManager.LoginErrors?[] errors) 
    {
        errorTMP.text = "";
        if (errors != null) 
        {
            foreach (LoginErrors error in errors)
            {
                errorTMP.text += GetErrorMessage(error) + "\n";
            }
        }
    }
}
