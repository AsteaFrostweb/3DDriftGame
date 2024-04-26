using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Device;
using UnityEngine.UI;
using Screen = UnityEngine.Screen;

public class VideoSettings : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    private List<Vector2> used_res;

    private void Start()
    {      
        
        // Populate resolution dropdown with available resolutions
        Resolution[] resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        used_res = new List<Vector2>();
        foreach (Resolution res in resolutions)
        {
            bool done = false;
            foreach (Vector2 vec in used_res)
            {
                if (vec.x == res.width && vec.y == res.height) 
                {
                    done = true;                    
                }
            }
            if (!done)
            {
                used_res.Add(new Vector2(res.width, res.height));
                resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(res.width + "x" + res.height));
            }
        }

        // Set current resolution in dropdown

       // resolutionDropdown.value = GetCurrentResolutionIndex();
        //resolutionDropdown.RefreshShownValue();

        // Set fullscreen toggle state
        //fullscreenToggle.isOn = Screen.fullScreen;
    }

    public void SetResolution(int i)
    {
       
        int resolutionIndex = resolutionDropdown.value;      
        Screen.SetResolution((int)used_res[resolutionIndex].x, (int)used_res[resolutionIndex].y, Screen.fullScreen);
        
        Debugging.Log("Resolution changed to:" + used_res[resolutionIndex].ToString());
    }

    public void SetFullscreen(bool b)
    {
        bool isFullscreen = fullscreenToggle.isOn;
        Screen.fullScreen = isFullscreen;        
        Debugging.Log("Fullscreen toggled " + isFullscreen.ToString());
    }

    private int GetCurrentResolutionIndex()
    {
      
        Resolution currentResolution = Screen.currentResolution;
        for (int i = 0; i < used_res.Count; i++)
        {
            if ((int)used_res[i].x == currentResolution.width &&
                (int)used_res[i].y == currentResolution.height)
            {
                return i;
            }
        }
        return 0;
    }
}
