using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Track;
using Debug = UnityEngine.Debug;


public class RaceGameplayHandler : MonoBehaviour
{
    public struct PlayerRace
    {
        public int id;        
        public CarController car_controller;
        public GameObject player_obj;
        public int lap_count;
        public bool was_on_start_finish;
        public bool on_start_finish;
        public List<Lap> laps;
        public Lap current_lap;
        


        public PlayerRace(int _id, GameObject _player_obj, bool on_sf, List<Lap> _laps, Lap _current_lap)
        {

            car_controller = null;
            id = _id;
            player_obj = _player_obj;
            was_on_start_finish = false;
            lap_count = 0;
            on_start_finish = on_sf;
            laps = _laps;
            current_lap = _current_lap;
           
        }
    }
    public struct Racetracker
    {
        public Track track;
        public PlayerRace[] players;
        public bool[] finished_players;

    }
    public struct PlayerRaceData
    {
        public DriftScoreHandler.DriftData drift_data;
        public TimeSpan fastest_lap;      
        public TimeSpan total_time;
        public int lap_count;
        
    }
    public struct RaceData
    {
        public PlayerRaceData[] player_race_data;

        public RaceData(PlayerRaceData[] _player_race_data) 
        {
            player_race_data = _player_race_data;
        }
    }
    [Header("Inputs")]
    public float round_countdown_time;
 

    [Header("Outputs")]
    public TimeSpan race_time;
    public bool race_started;
    public int player_count;

    public DriftScoreHandler[] score_handlers;
    private RaceData race_data;
    private PlayerRaceData[] player_race_data;    
    public DateTime round_start_time;
    public Racetracker race;
    private GameState game_state;
    private bool players_found;

    public Racetracker GetRace() 
    {
        return race;
    }

    private bool FindPlayers()
    {
        bool ans = true;
        
        for (int i = 1; i <= race.players.Length; i++)
        {

            GameObject player = GameObject.Find("Player" + i);
            if (player != null)
            {
                Debugging.Log("player" + i + " found.");
                race.players[i - 1] = new PlayerRace(i, player, true, null, new Lap());
                score_handlers[i - 1] = race.players[i - 1].player_obj.GetComponent<DriftScoreHandler>();
                race.players[i-1].car_controller = player.GetComponent<CarController>();
                
            }
            else { Debugging.Log("Couldn't find player: " + i); ans = false; }
        }
        return ans;
    }

    // Start is called before the first frame update
    void Start()
    {       
        race_started = false;


        
        game_state = GameObject.FindAnyObjectByType<GameState>();
        race.track = GameObject.FindAnyObjectByType<Track>();

        player_count = game_state.GetRacePlayerCount();

        score_handlers = new DriftScoreHandler[player_count];
        race.players = new PlayerRace[player_count];
        race.finished_players = new bool[player_count];
        player_race_data = new PlayerRaceData[player_count];

        round_start_time = DateTime.Now;
        race_time = round_start_time.Subtract(DateTime.Now);           
       

        players_found = FindPlayers(); //fasle if it fails to find one of the players game objects
        if(players_found) 
        {
            EnableControlLock();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!players_found)
        {
            players_found = FindPlayers();
            if (players_found)
            {
                EnableControlLock();
            }
            else 
            {
                return;
            }           
        }
       


        race_time = DateTime.Now.Subtract(round_start_time); //Caultulate race time
        //Handle the coundown timer
        if (!race_started)
        {
            if (race_time.Seconds < round_countdown_time)
            {
                return;
            }
            else 
            {
                race_started = true;
                round_start_time = DateTime.Now;
                DisableControlLock(); //Starts the race and disables the control lock on the cars
            }
        }

        
      

        for (int i = 0; i < race.players.Length; i++) 
        {
            //UnityEngine.Debugging.Log("player: " + (i + 1) + " is on node " + race.players[i].current_lap.current_node_order_index + "   Final node of track: " + (race.track.path_node_order.Length - 1));
            CheckStartFinishLine(i);
            //UnityEngine.Debugging.Log("2." + race.players[i].current_lap.current_node_order_index);
            UpdatePlayerLapTime(i);
            //UnityEngine.Debugging.Log("3." + race.players[i].current_lap.current_node_order_index);
            if (race.players[i].current_lap.current_node_order_index != race.track.path_node_order.Length - 1)
            {
                CheckNodeProximity(i); //If we arent on the last node of the track, check the proximity to the next node
            }
            //UnityEngine.Debugging.Log("4." + race.players[i].current_lap.current_node_order_index);
        }
    }
    public Vector3 GetVecToNextNode(int id) 
    {
        int next_node_order_index = race.players[id].current_lap.current_node_order_index + 1; //gets the next position in the node_order array        
        int next_node_index = race.track.path_node_order[next_node_order_index]; //gets the index of the Node in the nodes

        Node next_node = race.track.GetNodes()[next_node_index]; //gets the node from the tracks nodes array
        Node current_node = race.players[id].current_lap.current_node;


        return current_node.position - race.players[id].player_obj.transform.position;
    }
    public float GetDistToNextNode(int id)
    {
        return GetVecToNextNode(id).magnitude;
    }

    private void CheckNodeProximity(int id) 
    {
        int next_node_order_index = race.players[id].current_lap.current_node_order_index + 1; //gets the next position in the node_order array        
        int next_node_index = race.track.path_node_order[next_node_order_index]; //gets the index of the Node in the nodes
       
        Node next_node = race.track.GetNodes()[next_node_index]; //gets the node from the tracks nodes array
        Node current_node = race.players[id].current_lap.current_node;
      

        Vector3 vec_to_node = current_node.position - race.players[id].player_obj.transform.position;
      
        float dist_to_node = vec_to_node.magnitude;
        //Debugging.Log($"Distance to node({next_node_index}): {dist_to_node}");
        if (dist_to_node < current_node.radius) 
        {            
            race.players[id].current_lap.current_node = next_node;
            race.players[id].current_lap.current_node_order_index++;           
        }
    }

    private void CheckStartFinishLine(int id) 
    {
        if (race.players[id].was_on_start_finish) 
        {
            if (!race.players[id].on_start_finish) 
            {
                Debugging.Log("OnExitStartFinish. RGH");
                OnExitStartFinish(id);//Runs first frame player exits start finish
            }
        }
        else
        {
            if (race.players[id].on_start_finish) 
            {
                Debugging.Log("OnEnterStartFinish. RGH");
                OnEnterStartFinish(id);//Runs first frame player enters start finish
            }
        }
    }

    private void OnEnterStartFinish(int id)
    {
        race.players[id].was_on_start_finish = true;


        if (race.players[id].lap_count == 0) //runs on initialization as the game fierst realizes the cars are in the start finish hitbox
        {
            race.players[id].laps = new List<Lap>();
            race.players[id].current_lap = new Lap(DateTime.Now, 0, race.track);
            player_race_data[id].fastest_lap = TimeSpan.MaxValue;
            race.players[id].lap_count++;

        }
     
        if (race.players[id].current_lap.current_node_order_index == race.track.path_node_order.Length - 1) //If the player has reacher the final node before raching the finish line
        {
            Debugging.Log(race.players[id].player_obj.name + " completed a lap.");
            race.players[id].current_lap.end_time = DateTime.Now;
            race.players[id].laps.Add(race.players[id].current_lap); //adds the players lap to its laps list
           

            if (player_race_data[id].fastest_lap.TotalSeconds > race.players[id].current_lap.current_time.TotalSeconds) //If this lap is the new fastest lap
            {
                player_race_data[id].fastest_lap = race.players[id].current_lap.current_time;
            }

            race.players[id].current_lap = new Lap(DateTime.Now, 0, race.track);

            if (!race.track.loop) //If the track is not set to loop
            {               
                FinishRace(id);
                return;
            }           

            if (race.players[id].lap_count == race.track.loop_count) //If looping is enabled and we've reaced or exceeded the loop limit
            {
                FinishRace(id);
                return;
            }

            race.players[id].lap_count++;                  
        }
        else
        {
            //player has gone the wrong direction
        }

    }
    private void OnExitStartFinish(int id)
    {
        race.players[id].was_on_start_finish = false;
    }

    private void UpdatePlayerLapTime(int id)
    {
        race.players[id].current_lap.current_time = DateTime.Now.Subtract(race.players[id].current_lap.start_time); //Update lap Time
      
        
    }

    public void FinishRace(int id) 
    {
        score_handlers[id].EndComboSuccess();
        player_race_data[id].drift_data = score_handlers[id].GetDriftData();
        

        player_race_data[id].total_time = DateTime.Now.Subtract(race.players[id].laps[0].start_time);
        player_race_data[id].lap_count = race.players[id].lap_count;
        
       

        race.finished_players[id] = true;

        if (id == 0) 
        {
            for (int i = 1; i < race.players.Length; i++)
            {
                if (!race.finished_players[i]) 
                {
                    FinishRace(i); //finishing every non finished player
                }
            }

            //Finishing race as player 1 has finished
            RaceData race_data = new RaceData();
            race_data.player_race_data = player_race_data;

            PostGameData pg_data = new PostGameData();
            pg_data.map = game_state.current_map;
            pg_data.race_data = race_data;    
            

            game_state.post_game_data = pg_data;
            EnableControlLock();
            StartCoroutine(Finish("PostGame", 2f));
        }

        
    }


    public void SetFinishline(GameObject obj, bool _set) 
    {
        for(int i = 0; i < race.players.Length; i++) 
        {
            if (race.players[i].player_obj == obj) //finf the playerdata that has the same obj as the collder has passed in
            {
                race.players[i].on_start_finish = _set;
            }
        }
    }

    private void DisableControlLock() 
    {
        for (int i = 0; i < race.players.Length; i++)
        {
            race.players[i].car_controller.ControlLocked = false;
        }
    }
    private void EnableControlLock()
    {
        for (int i = 0; i < race.players.Length; i++)
        {         
            if (race.players[i].car_controller == null) Debugging.Log($"Unnable to lock player {i}'s controls");
            race.players[i].car_controller.ControlLocked = true;
        }
    }

    IEnumerator Finish(string scene, float time)
    {
        // Wait for 2 seconds
        yield return new WaitForSeconds(time);

        // Load the scene
        SceneManager.LoadScene(scene);
    }

}
