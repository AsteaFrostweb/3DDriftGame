using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static RaceGameplayHandler;

public class RaceUIHandler : MonoBehaviour
{
    

   
    [SerializeField]
    private Sprite red_light;
    [SerializeField]
    private Sprite amber_light;
    [SerializeField]
    private Sprite green_light;
    [SerializeField]
    private float traffic_light_lifetime = 1f;
    private bool traffic_light_decayed;
    private bool brake_lightActive = false;
    
    private DateTime race_start_time;

    public Image traffic_light;
    public TextMeshProUGUI lap_current_text;
    public TextMeshProUGUI lap_max_text;

    public RaceGameplayHandler gameplay_handler;
    public RaceGameplayHandler.Racetracker race_tracker;
    private TextMeshProUGUI total_drift_score_tmp;
    private TextMeshProUGUI total_time_tmp;
    private TextMeshProUGUI current_lap_time_tmp;

    public GameObject canvas;
    // Start is called before the first frame update
    void Start()
    {
        race_start_time = DateTime.MinValue;
        gameplay_handler = GameObject.FindAnyObjectByType<RaceGameplayHandler>();
      
        race_tracker = gameplay_handler.race;

        traffic_light_decayed = false;

        canvas = GameObject.Find("Canvas");
        Transform scoreText = canvas.transform.Find("ScoreText");

        traffic_light = canvas.transform.Find("TrafficLight").GetComponent<Image>();
        total_drift_score_tmp = GameObject.Find("ScoreText_TMP").GetComponent<TextMeshProUGUI>();
        total_time_tmp = GameObject.Find("TotalTime_TMP").GetComponent<TextMeshProUGUI>();
        current_lap_time_tmp = GameObject.Find("LapTime_TMP").GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!traffic_light_decayed)
        {
            HandleTrafficLight();
        }

      
        
        if (lap_max_text == null) { Debugging.Log($"NO LAP MAX TMP"); }
        if (race_tracker.track == null) { race_tracker = gameplay_handler.race; }
        
        lap_max_text.text = race_tracker.track.loop_count.ToString();
        lap_current_text.text = race_tracker.players[0].lap_count.ToString();

        if (gameplay_handler.score_handlers[0] != null)
        {
            string score_string = gameplay_handler.score_handlers[0].current_total_drift_score.ToString("N0");
            total_drift_score_tmp.text = score_string;
        }
      

        if (gameplay_handler.race_started)// only update timers if the race as started as they contain rando values
        {
            string total_time_string = DateTime.Now.Subtract(gameplay_handler.round_start_time).ToString(@"hh\:mm\:ss");
            string lap_time_string = DateTime.Now.Subtract(race_tracker.players[0].current_lap.start_time).ToString(@"hh\:mm\:ss");
            total_time_tmp.text = total_time_string;
            current_lap_time_tmp.text = lap_time_string;
        }
    }

    private void HandleTrafficLight() 
    {
        if (!gameplay_handler.race_started)
        {
            TimeSpan race_time = DateTime.Now.Subtract(gameplay_handler.round_start_time);
            float countdown_ratio = (float)race_time.TotalSeconds / gameplay_handler.round_countdown_time;

            if (countdown_ratio > 0.5f)
            {
                traffic_light.sprite = amber_light;
            }
            else if(countdown_ratio > 0f)
            {
                traffic_light.sprite = red_light;
            }



        }
        else
        {
            traffic_light.sprite = green_light;
            if (race_start_time == DateTime.MinValue) //happens if it hast been set yet
            {
                race_start_time = DateTime.Now;
            }

            if (DateTime.Now.Subtract(race_start_time).TotalSeconds >= traffic_light_lifetime)
            {
                traffic_light.gameObject.SetActive(false);
                traffic_light_decayed = true;
            }
        }
    }
}
