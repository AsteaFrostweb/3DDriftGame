using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugUIHandler : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI debug_inputText;

    public void OnInputEnd() 
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            //Debugging.Log("Attempting to parse command: " + debug_inputText.text);
            Debugging.ParseCommand(debug_inputText.text);
        }
    }
}
