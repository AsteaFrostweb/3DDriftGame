
using Assets.Scripts.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class NetworkManager : MonoBehaviour
{
    public struct VerificationRequest 
    {
        public string VerificationToken;
        public LoginErrors?[] error;

        public VerificationRequest(string token, LoginErrors?[] _error) 
        {
            VerificationToken = token;
            error = _error;
        }
    }

    public struct LoginReponse 
    {
        public bool wasSuccess;
        public LoginErrors?[] error;

        public LoginReponse(bool _success, LoginErrors?[] _error) 
        {
            wasSuccess = _success;            
            error = _error;
        }
    }

    //Error enums and functions
    public enum LoginErrors{Invalid_Credentials, No_Response, Unknown_Error}
    public enum RegisterErrors { Username_Taken, Password_Required, Username_Required, Invalid_Username, Inavlid_Password, No_Response, Unknown_Error }
    public RegisterErrors?[] ParseRegisterErrors(string body, string username)
    {
        List<RegisterErrors?> error_list = new List<RegisterErrors?>();

        if(body.Contains($"Username '{username}' is already taken.")) error_list.Add(RegisterErrors.Username_Taken); 
        if (body.Contains($">The Password field is required.<")) error_list.Add(RegisterErrors.Password_Required);
        if (body.Contains($">The Email field is required.<")) error_list.Add(RegisterErrors.Username_Required);
        if (body.Contains($">The Email field is not a valid e-mail address.<")) error_list.Add(RegisterErrors.Invalid_Username);
        if (body.Contains($">The Password must be at least 6 and at max 100 characters long.<")) error_list.Add(RegisterErrors.Inavlid_Password);

        if (error_list.Count == 0)
        {
            return null;
        }
        else 
        {
            return error_list.ToArray();
        }
    }

    [SerializeField]
    private string securityString = "https://";
    [SerializeField]
    private string baseUri = "192.168.1.228";
    [SerializeField]
    private string Port = "443";
    public string Username  { get; set; }
    public string Password { private get; set; }
    public string UriString { get { return securityString + baseUri + ":" + Port; } }
    public bool IsLoggedIn { get; set; }

    private HttpClientHandler httpClientHandler;
    public HttpClient httpClient { get; set; }

    
    void Start()
    {
        httpClientHandler = new HttpClientHandler();
        httpClientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
        httpClient = new HttpClient(httpClientHandler);

        Debugging.SetIp += OnSetIp;
        Debugging.SetPort += OnSetPort;
        Debugging.ShowIp += OnShowIp;
    }

 
    void Update()
    {

    }




    private void OnSetIp(object sender, string[] components) 
    {
       
        string ip = string.Join(".", components);

        Debugging.Log("Setting IP to: " + ip);
        baseUri = ip;
    }
    private void OnSetPort(object sender, string[] components)
    {
        if (components.Length > 1) 
        {
            Debugging.Log("Error parsing port");
            return;
        }       
        string _port = components[0];

        Debugging.Log("Setting port to: " + _port);
        Port = _port;
    }
    private void OnShowIp(object sender, object obj)
    {
        Debugging.Log(UriString);
    }


    //Gets a verification token from login page and then attempts to login with username and password from networkManager
    public async Task<LoginReponse> Login()
    {
        VerificationRequest request = await GetVerificationToken();
        if (request.error != null) 
        {
            return new LoginReponse(false, request.error);
        }
        string verificationToken = request.VerificationToken;      

        FormUrlEncodedContent LoginData = GetLoginContent(verificationToken, Username, Password);

        // Send POST request with form data
        HttpResponseMessage response = await httpClient.PostAsync($"{UriString}/Identity/Account/Login", LoginData);
        if (response == null) 
        {
            return new LoginReponse(false, new LoginErrors?[] {LoginErrors.Unknown_Error});
        }
        string content = await response.Content.ReadAsStringAsync();
        if (content.Contains("Hello " + Username, StringComparison.OrdinalIgnoreCase))         
        {
            IsLoggedIn = true;
            return new LoginReponse(true, null); ;
        }
        else
        {
            return new LoginReponse(false, new LoginErrors?[]{ LoginErrors.Unknown_Error });
        }
    }
    //Gets a verification token from login page and then attempts to login with username and password specified
    public async Task<LoginReponse> Login(string _username, string _password)
    {
        Username = _username;
        VerificationRequest request = await GetVerificationToken();
        if (request.error != null)
        {
            return new LoginReponse(false, request.error);
        }
        string verificationToken = request.VerificationToken;

        FormUrlEncodedContent LoginData = GetLoginContent(verificationToken, Username, _password);
     
        HttpResponseMessage response = await httpClient.PostAsync($"{UriString}/Identity/Account/Login", LoginData);
        if (response == null)
        {
            return new LoginReponse(false, request.error);
        }
        string content = await response.Content.ReadAsStringAsync();
    
        if (content.Contains("Hello " + _username, StringComparison.OrdinalIgnoreCase))
        {
            IsLoggedIn = true;
            return new LoginReponse(true, null);
        }
        else
        {
            return new LoginReponse(false, new LoginErrors?[] { LoginErrors.Unknown_Error });
        }
    }


    private FormUrlEncodedContent GetLoginContent(string verificationToken, string username, string password)
    {
        return new FormUrlEncodedContent(new[]
        {
                new KeyValuePair<string, string>("Input.Email", username),
                new KeyValuePair<string, string>("Input.Password", password),
                new KeyValuePair<string, string>("Input.RememberMe", "true"),
                new KeyValuePair<string, string>("__RequestVerificationToken", verificationToken)
            });
    }

    private async Task<VerificationRequest> GetVerificationToken()
    {
        // Send GET request to retrieve the login page

        HttpResponseMessage loginPageResponse;
        try
        {
            loginPageResponse = await httpClient.GetAsync($"{UriString}/Identity/Account/Login");
            loginPageResponse.EnsureSuccessStatusCode(); // Throw an exception if the response is not successful
        }
        catch
        {
            return new VerificationRequest(null, new LoginErrors?[] { LoginErrors.No_Response });
        }

        string loginPageContent = await loginPageResponse.Content.ReadAsStringAsync();

        // Extract the verification token from the login page content
        string verificationToken = ExtractVerificationToken(loginPageContent);
        return new VerificationRequest(verificationToken, null);
    }
    private string ExtractVerificationToken(string htmlContent)
    {
        // Define the regular expression pattern to match the verification token
        string pattern = "<input[^>]+name=\"__RequestVerificationToken\"[^>]+value=\"([^\"]+)\"[^>]*>";
        Match match = Regex.Match(htmlContent, pattern);

        if (match.Success)
        {
            return match.Groups[1].Value;
        }
        else
        {
            Console.WriteLine("Verification token not found.");
            return "";
        }
    }

    public async Task<bool> PostHighScore(string mapName, float fastestLap, int bestComboScore, float bestComboTime)
    {
        // Create the form data
        var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("MapName", mapName),
                new KeyValuePair<string, string>("FastestLap", fastestLap.ToString()),
                new KeyValuePair<string, string>("BestComboScore", bestComboScore.ToString()),
                new KeyValuePair<string, string>("BestComboTime", bestComboTime.ToString())
            };

        // Create FormUrlEncodedContent
        var content = new FormUrlEncodedContent(formData);

        // Send POST request to the UpdateHighScore endpoint
        HttpResponseMessage response = await httpClient.PostAsync($"{UriString}/Highscores/UpdateHighScore", content);

        Console.WriteLine("Status Code: " + response.StatusCode.ToString());

        // Check if the response is successful
        return response.IsSuccessStatusCode;
    }
    public async Task<bool> PostHighScore(Highscore highscore)
    {
        Debugging.Log("Posting highscore: " + highscore.ToString());
        // Create the form data
        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("MapName", highscore.Map),
            new KeyValuePair<string, string>("FastestLap", highscore.FastestLapTimeSpan().TotalSeconds.ToString()),
            new KeyValuePair<string, string>("BestComboScore", highscore.Best_Combo_Score.ToString()),
            new KeyValuePair<string, string>("BestComboTime", highscore.BestComboTimeSpan().TotalSeconds.ToString())
        };

        // Create FormUrlEncodedContent
        var content = new FormUrlEncodedContent(formData);

        // Send POST request to the UpdateHighScore endpoint
        HttpResponseMessage response = await httpClient.PostAsync($"{UriString}/Highscores/UpdateHighScore", content);

        Console.WriteLine("Status Code: " + response.StatusCode.ToString());

        // Check if the response is successful
        return response.IsSuccessStatusCode;
    }



    public async Task<List<Highscore>> GetHighscores(string MapName, int ToRank, string UserName)
    {
        List<Highscore> highscores = new List<Highscore>();

        // Send POST request to the UpdateHighScore endpoint
        HttpResponseMessage response = await httpClient.GetAsync($"{UriString}/Highscores/GetHighscores?MapName={MapName}&ToRank={ToRank}&UserName={UserName}");

        string json = await response.Content.ReadAsStringAsync();

        highscores = HighScores.ParseHighscores(json);

        return highscores;
    }


}


public class HighscoreUpdateModel
{
    public string MapName { get; set; }
    public float FastestLap { get; set; }
    public int BestComboScore { get; set; }
    public float BestComboTime { get; set; }
}


public class Highscore
{
    public string Name { get; set; }
    public string Map { get; set; }
    public string Fastest_Lap { get; set; }
    public int Best_Combo_Score { get; set; }
    public string Best_Combo_Time { get; set; }

    public override string ToString()
    {
        return "Name:" + Name + ", Map:" + Map + ", Fastest Lap:" + Fastest_Lap + ", Best Combo Score:" + Best_Combo_Score + ", Best Combo Time:" + Best_Combo_Time;
    }

    public TimeSpan BestComboTimeSpan()
    {
        return TimeSpan.ParseExact(Best_Combo_Time, @"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture);
    }
    public TimeSpan FastestLapTimeSpan()
    {
        return TimeSpan.ParseExact(Fastest_Lap, @"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture);
    }

    public static Highscore FromPostGameData(NetworkManager nm, PostGameData data)
    {
        Highscore h = new Highscore();

        h.Name = nm.Username;
        h.Map = Track.GetMapName(data.map);
        h.Best_Combo_Score = data.race_data.player_race_data[0].drift_data.best_combo_score;
        h.Best_Combo_Time = data.race_data.player_race_data[0].drift_data.longest_combo.ToString(@"hh\:mm\:ss\.fff");
        h.Fastest_Lap = data.race_data.player_race_data[0].fastest_lap.ToString(@"hh\:mm\:ss\.fff");

        return h;
    }

    public static Highscore Merge(Highscore h1, Highscore h2) 
    {
        if (h1.Map == null && h2.Map == null) 
        {
            Debugging.Log("Cant merge as both maps are null");
            return null;
        }
        if (h1.Map == null && h2.Map != null) 
        {
            h1.Map = h2.Map;
        }
        if (h1.Map != null && h2.Map == null)
        {
            h2.Map = h1.Map;
        }       

        Highscore return_highscore = new Highscore();
        return_highscore.Name = h1.Name;
        return_highscore.Map = h2.Map;
        if (h1.FastestLapTimeSpan().TotalSeconds < h2.FastestLapTimeSpan().TotalSeconds)
        {
            return_highscore.Fastest_Lap = h1.Fastest_Lap;
        }
        else 
        {
            return_highscore.Fastest_Lap = h2.Fastest_Lap;
        }
        if (h1.Best_Combo_Score > h2.Best_Combo_Score)
        {
            return_highscore.Best_Combo_Score = h1.Best_Combo_Score;
        }
        else 
        {
            return_highscore.Best_Combo_Score = h2.Best_Combo_Score;
        }
        if (h1.BestComboTimeSpan().TotalSeconds > h2.BestComboTimeSpan().TotalSeconds)
        {
            return_highscore.Best_Combo_Time = h1.Best_Combo_Time;
        }
        else 
        {
            return_highscore.Best_Combo_Time = h2.Best_Combo_Time;
        }

        Debugging.Log("Merged: " + return_highscore.ToString());
        return return_highscore;
    }

    public bool Equals(Highscore h) 
    {
        return (h.Name == Name && h.Map == Map && h.Fastest_Lap == Fastest_Lap && h.Best_Combo_Time == Best_Combo_Time && h.Best_Combo_Score == Best_Combo_Score);            
    }
}

public class HighScores
{
    enum Sort_Order { DESC, ASC }
    enum Sort_By { NAME, FASTEST_LAP }
  



    public static List<Highscore> ParseHighscores(string text)
    {
        List<Highscore> highscores = new List<Highscore>();
        Debugging.Log("Attempting to parse: " + text);     



        try
        {                     // Deserialize the JSON string into a list of Highscore objects
            highscores = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Highscore>>(text);          
        }
        catch (Exception ex)
        {
            // Handle any errors that may occur during deserialization
            Console.WriteLine("Error parsing highscores: " + ex.Message);
            highscores = new List<Highscore>(); // Return an empty list if parsing fails
        }

        
        foreach (Highscore highscore in highscores) 
        {
            Debugging.Log(highscore.ToString());
        }
        return highscores;
    }

}


