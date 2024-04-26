using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameState : MonoBehaviour
{
    PostGameData previous_game_data;

    private GameObject debugCanvas;    
    private GameObject debugPanel;
    private TextMeshProUGUI debugTMP;

    public enum Cars { RED, GREEN, BLUE, WHITE}
    public enum State {MENU, IN_GAME, POST_GAME}

    public PostGameData post_game_data;
    public Track.Maps current_map;
    public State game_state = State.MENU;
    public Cars current_car;
    public int lap_count = 0;
    public int race_player_count = 1;

    [Header("Inputs")]
    [Range(1, 20)]
    public int max_player_count = 4;
    [Range(1, 20)]
    public int player_count = 4;
    public int max_debug_console_lines = 72;



    public int GetRacePlayerCount() 
    {
        return race_player_count;
    }
    public void SetRacePlayerCount(int s)
    {
        if (s <= max_player_count && s >= 1) 
        {
            race_player_count = s;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        LocateObjects();

        GameObject[] PermenantObjects = new GameObject[] {debugCanvas, gameObject };
        foreach (GameObject o in PermenantObjects) 
        {
            DontDestroyOnLoad(o);
        }
      
       
        SetupDevConsole();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote)) 
        {
            Debugging.Log("Backtick key pressed.");
            Debugging.ToggleDeveloperConsole();
        }
    }

    private void LocateObjects() 
    {
        debugCanvas = GameObject.Find("DebugCanvas");
        debugTMP = GameObject.Find("DebugTextTMP").GetComponent<TextMeshProUGUI>();
        debugPanel = GameObject.Find("DebugPanel");
    }
    private void SetupDevConsole() 
    {

        //Debugging.inEditor = Application.isEditor;
        Debugging.inEditor = false;
        Debugging.maxLines = max_debug_console_lines;
        Debugging.debugTextTMP = debugTMP;
        Debugging.debugPanel = debugPanel;
        Debugging.Initialize();
        Debugging.ToggleDeveloperConsole(false);


    }
}
