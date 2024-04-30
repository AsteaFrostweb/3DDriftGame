using Car;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.UI;

public class DriftUIHandler : MonoBehaviour
{
    DriftScoreHandler drift_handler;
    TextMeshProUGUI drift_score_tmp;
    TextMeshProUGUI total_drift_score_tmp;

    private string previous_text;

    private CarController controller;

    [SerializeField]
    public Color score_text_colour = Color.white;
    [SerializeField]
    public Color failed_score_color = Color.red;
    // Start is called before the first frame update
    void Start()
    {
        drift_score_tmp = GameObject.Find("DriftScoreText").GetComponent<TextMeshProUGUI>();
        total_drift_score_tmp = GameObject.Find("ScoreText_TMP").GetComponent<TextMeshProUGUI>();


        controller = GameObject.FindAnyObjectByType<CarController>();
        if (controller != null)
        {
            drift_handler = controller.gameObject.GetComponent<DriftScoreHandler>();
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (drift_handler == null || drift_score_tmp == null)
        {
            drift_score_tmp = GameObject.Find("DriftScoreText").GetComponent<TextMeshProUGUI>();
            controller = GameObject.FindAnyObjectByType<CarController>();
            if (controller != null)
            {
                drift_handler = controller.gameObject.GetComponent<DriftScoreHandler>();
                controller.OnBrakeStart += () => OnBrakeStart();
                controller.OnBrakeEnd += () => OnBrakeEnd();
                controller.OnDriftStart += () => OnDriftStart();
                controller.OnDriftEnd += () => OnDriftEnd();
            }
            Debugging.Log("Returning from DriftungUI as no handler or scoretmp found.");
            return;
        }  





        if (drift_handler.in_combo)
        {
            drift_score_tmp.gameObject.SetActive(true);
            previous_text = drift_handler.current_combo_multiplier + " x " + Mathf.Round(drift_handler.current_drift_score) + " : " + Mathf.Round(drift_handler.current_combo.TotalScore());
            drift_score_tmp.text = previous_text;
            drift_score_tmp.faceColor = score_text_colour;
        }
        else if (drift_handler.crashed && (DateTime.Now.Subtract(drift_handler.previous_drift_end).TotalSeconds <= drift_handler.crash_no_drift_time))
        {
            drift_score_tmp.gameObject.SetActive(true);
            drift_score_tmp.text = previous_text;
            drift_score_tmp.faceColor = failed_score_color;
        }
        else
        {
            drift_score_tmp.gameObject.SetActive(false);
        }

        UpdateDriftScore();

    }

    void UpdateDriftScore()
    {
        total_drift_score_tmp.text = "Total Score: " + Mathf.Round(drift_handler.current_total_drift_score);
    }


    private void OnBrakeStart()
    {
        foreach (Wheel w in controller.GetWheels())
        {
            
            w.trailRenderer.emitting = true;
            
        }
    }
    private void OnBrakeEnd()
    {
        OnDriftEnd();
    }




    private void OnDriftStart() 
    {
       
        foreach (Wheel w in controller.GetWheels()) 
        {
            if (w.axel == Axel.Rear)
            {
                w.trailRenderer.emitting = true;
            }
        }
    }
    private void OnDriftEnd()
    {
        
        foreach (Wheel w in controller.GetWheels())
        {
            w.trailRenderer.emitting = false;
        }
    }
    

}
