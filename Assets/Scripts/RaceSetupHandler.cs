using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceSetupHandler : MonoBehaviour
{
    [SerializeField]
    GameObject red_car_prefab;
    [SerializeField]
    GameObject blue_car_prefab;
    [SerializeField]
    GameObject green_car_prefab;
    [SerializeField]
    GameObject white_car_prefab;

    GameState game_state;
    
    [SerializeField]
    Transform Spawn_position;
    public bool player_spawned;

    private Track track;

    // Start is called before the first frame update
    void Start()
    {
        game_state = GameObject.Find("GameState").GetComponent<GameState>();
        track = GameObject.FindAnyObjectByType<Track>();
        track.loop_count = game_state.lap_count;
    }

    // Update is called once per frame
    void Update()
    {
        if (game_state == null) 
        {
            game_state = GameObject.Find("GameState").GetComponent<GameState>();
            
        }
        if (!player_spawned)
        {            
            try
            {
               SpawnCar(1, game_state.current_car);
               player_spawned = true;
            }
            catch
            {
                Debugging.Log("Error spawning player");
            }
        }
    }


    void SpawnCar(int i, GameState.Cars car) 
    {
        switch (car) 
        {
            case GameState.Cars.GREEN:
                GameObject.Instantiate(green_car_prefab, Spawn_position.position, Spawn_position.rotation).name = "Player" + i;
                break;
            case GameState.Cars.RED:
                GameObject.Instantiate(red_car_prefab, Spawn_position.position, Spawn_position.rotation).name = "Player" + i;
                break;
            case GameState.Cars.BLUE:
                GameObject.Instantiate(blue_car_prefab, Spawn_position.position, Spawn_position.rotation).name = "Player" + i;
                break;
            case GameState.Cars.WHITE:
                GameObject.Instantiate(white_car_prefab, Spawn_position.position, Spawn_position.rotation).name = "Player" + i;
                break;
                
        }
    }
}
