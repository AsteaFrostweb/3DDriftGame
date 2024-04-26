using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameState;

public struct PostGameData
{
    public Track.Maps map;    
    public RaceGameplayHandler.RaceData race_data;
}



public class PostGameUIHandler : MonoBehaviour
{
    public bool isNewHighscore { private get; set; } = false;

    private GameState game_state;
    private PostGameData pg_data;

    [SerializeField]
    public GameObject[] car_prefabs;
    [SerializeField]
    public Cars[] car_types;
    


    [SerializeField]
    public Image car_image;
    [SerializeField]
    public TextMeshProUGUI map_name_text;   
    [SerializeField]
    public TextMeshProUGUI total_score_text;
    private string total_score_text_base = "";
    [SerializeField]
    public TextMeshProUGUI total_time_text;
    private string total_time_text_base = "";
    [SerializeField]
    public TextMeshProUGUI best_combo_score_text;
    private string best_combo_text_base = "";
    [SerializeField]
    public TextMeshProUGUI fastest_lap_text;
    private string fastest_lap_text_base = "";
    [SerializeField]
    public TextMeshProUGUI longest_combo_text;
    private string longest_combo_text_base = "";
    [SerializeField]
    public TextMeshProUGUI total_combo_time_text;
    private string total_combo_time_text_base = "";

    [SerializeField]
    private GameObject NewHighscoreText;

    private bool menu_populated = false;
  

    // Start is called before the first frame update
    void Start()
    {
        game_state = GameObject.FindAnyObjectByType<GameState>();
        pg_data = game_state.post_game_data;
        
        total_score_text_base = total_score_text.text;
        total_time_text_base = total_time_text.text;
        best_combo_text_base = best_combo_score_text.text;
        fastest_lap_text_base = fastest_lap_text.text;
        longest_combo_text_base = longest_combo_text.text;
        total_combo_time_text_base = total_combo_time_text.text;

        menu_populated = false;
        game_state.game_state = GameState.State.POST_GAME;
    }

    // Update is called once per frame
    void Update()
    {
        if (NewHighscoreText != null)
        {
            if (!NewHighscoreText.activeInHierarchy && isNewHighscore)
            {
                NewHighscoreText.SetActive(true);
            }
        }
       

        if (!menu_populated) 
        {
            UpdateTextBoxes();
            menu_populated=true;
        }
        
    }

   

    void UpdateTextBoxes() 
    {


        RaceGameplayHandler.PlayerRaceData pr_data = pg_data.race_data.player_race_data[0]; //get player1's data
        DriftScoreHandler.DriftData dr_data = pg_data.race_data.player_race_data[0].drift_data;
        map_name_text.text = Track.GetMapName(game_state.current_map);
        total_score_text.text = total_score_text_base + dr_data.total_score.ToString("N0");
        total_time_text.text = total_time_text_base + pr_data.total_time.ToString(@"hh\:mm\:ss\.fff");
        best_combo_score_text.text = best_combo_text_base + dr_data.best_combo_score.ToString("N0");
        fastest_lap_text.text = fastest_lap_text_base + pr_data.fastest_lap.ToString(@"hh\:mm\:ss\.fff");
        longest_combo_text.text = longest_combo_text_base + dr_data.longest_combo.ToString(@"hh\:mm\:ss\.fff");
        total_combo_time_text.text = total_combo_time_text_base + dr_data.total_combo_time.ToString(@"hh\:mm\:ss");

        //SetCarImageByType(game_state.current_car);
    }

    public void OnContinue() 
    {
        SceneManager.LoadScene("MainMenu");
    }

    void SetCarImageByType(Cars type) 
    {
        
        for (int i = 0; i < car_types.Length; i++) 
        {
            if (car_types[i] == type) 
            {
               
                SpriteRenderer img = car_prefabs[i].transform.Find("CarSprite").GetComponent<SpriteRenderer>();
                if (img == null) 
                {
                    Debugging.Log("couldnt find SpriteRenderer in car component");
                }
                else
                {
                    car_image.sprite = img.sprite; //sets the UI car image tot the ssame sprite as the casr image
                }
            }
        }
    }
}
