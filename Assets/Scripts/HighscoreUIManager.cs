using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.UI;

public class HighscoreUIManager : MonoBehaviour
{
    enum SortOrder {FastestLap, LongestCombo, BestComboScore }

    [SerializeField]
    private GameObject DataRowsParent;
    private GameObject[] DataRows;

    private PostGameUIHandler pg_uiHandler;
    private PostGameData pg_data;
    private NetworkManager networkManager;
    private GameState gameState;
    private SortOrder sortOrder;
    public List<Highscore> current_highscores;


    private bool highscores_updated = false;
    private bool started_Display = false;

    void Start()
    {      

        DataRows = GetDataRows();
        networkManager = GameObject.FindAnyObjectByType<NetworkManager>();
        gameState = GameObject.FindAnyObjectByType<GameState>();
        pg_data = gameState.post_game_data;
        pg_uiHandler = GameObject.FindAnyObjectByType<PostGameUIHandler>();
        highscores_updated = false;
        started_Display = false;

        if (networkManager.IsLoggedIn)
        {
            GetAndUpdateHighscore();
        }
        else 
        {
            highscores_updated = true;
        }
    
        
    }
    private void Update()
    {
        if (highscores_updated && !started_Display) 
        {
            Debugging.Log("Attempting to load and siaply highscores");
            LoadAndDisplayHighscores(null);
            started_Display = true;
        }
    }
    async void LoadAndDisplayHighscores(List<Highscore> highscores)
    {
        Debugging.Log("Attempting to load data for map:" + Track.GetMapName(gameState.current_map));
        if (highscores == null)
        {            
            highscores = await networkManager.GetHighscores(Track.GetMapName(gameState.current_map), 18, null);
            if (highscores.Count == 0)
            { Debugging.Log("Error Retreiving highscores"); return; }
        }
        else 
        {
            current_highscores = highscores;
        }
        Debugging.Log("Stating data asign");
        for (int i = 0; i < highscores.Count; i++) 
        {
            Debugging.Log("Assigning: " + highscores[i].ToString() + " to row " + highscores[i]);
            AssignDataRowValues(DataRows[i], highscores[i]);
        }
    }
    async void GetAndUpdateHighscore() 
    {
        Highscore server_score = await GetBestHighscore();
        Highscore current_score = Highscore.FromPostGameData(networkManager, pg_data);
        if (server_score == null)
        {
            Debugging.Log("No Player highscore found. Attempting to upload current score...");
            if (await networkManager.PostHighScore(current_score)) 
            {
                Debugging.Log("Highscore posted succesfuly");
            }
            else 
            {
                Debugging.Log("Unable to post highscore to server");
            }
            highscores_updated = true;
            return;
        }

        //Merge highscores to create the new higshcore(any better scores are updated)
        Debugging.Log("Attmepting to merge:" + server_score.ToString() + current_score.ToString());
        Highscore merged_highscore = Highscore.Merge(server_score, current_score);
        if (!server_score.Equals(merged_highscore))  //if the new merged highscore is different than the current one
        {
            Debugging.Log("NEW HIGHSCORE!    Attempting to post highscore to server");
            pg_uiHandler.isNewHighscore = true;
            if (await networkManager.PostHighScore(merged_highscore))
            {
                Debugging.Log("Highscore posted succesfuly");
            }
            else
            {
                Debugging.Log("Unable to post highscore to server");
            }
        }
        highscores_updated = true;
    }

    async Task<Highscore> GetBestHighscore() 
    {
        List<Highscore> get_list = await networkManager.GetHighscores(Track.GetMapName(gameState.current_map), 1, networkManager.Username);
        if (get_list.Count == 0) { return null; }
        return get_list[0];
    }
   


    private GameObject[] GetDataRows() 
    {
        List<GameObject> object_list = new List<GameObject>();       
        int count = 1;
        while (true) 
        {
            Transform obj_trans = DataRowsParent.transform.Find($"DataRow ({count})");
            if (obj_trans == null)
            {
                break;
            }
            else 
            {
                object_list.Add(obj_trans.gameObject);
            }

            count++;
        }

        return object_list.ToArray();  
    }
    private bool AssignDataRowValues(GameObject row, Highscore data) 
    {
        try
        {
            TextMeshProUGUI userText = row.transform.Find("Cell (1)").GetComponentInChildren<TextMeshProUGUI>();
            TextMeshProUGUI BestComboScoreText = row.transform.Find("Cell (2)").GetComponentInChildren<TextMeshProUGUI>();
            TextMeshProUGUI BestComboTimeText = row.transform.Find("Cell (3)").GetComponentInChildren<TextMeshProUGUI>();
            TextMeshProUGUI FastestLapTimeText = row.transform.Find("Cell (4)").GetComponentInChildren<TextMeshProUGUI>();

            userText.text = data.Name;
            BestComboScoreText.text = data.Best_Combo_Score.ToString("N0");
            BestComboTimeText.text = data.Best_Combo_Time;
            FastestLapTimeText.text = data.Fastest_Lap;

            return true;
        }
        catch { return false;  }      
     
    }

    private List<Highscore> SortBy(List<Highscore> highscores, SortOrder order) 
    {
        switch (order) 
        {
            case SortOrder.FastestLap:
                return highscores.OrderBy(h => h.FastestLapTimeSpan().TotalSeconds).ToList();
            case SortOrder.LongestCombo:
                return highscores.OrderBy(h => h.BestComboTimeSpan().TotalSeconds).ToList();
            case SortOrder.BestComboScore:
                return highscores.OrderBy(h => h.Best_Combo_Score).ToList();
        }
        return highscores;
    }

    public void OnFastestLapSort() 
    {
        LoadAndDisplayHighscores(SortBy(current_highscores, SortOrder.FastestLap));
    }
    public void OnBestComboSort()
    {
        LoadAndDisplayHighscores(SortBy(current_highscores, SortOrder.BestComboScore));
    }
    public void OnLongestComboSort()
    {
        LoadAndDisplayHighscores(SortBy(current_highscores, SortOrder.LongestCombo));
    }

}
