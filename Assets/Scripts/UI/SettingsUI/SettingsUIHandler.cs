using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUIHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject VideoPanel;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HidePanels() 
    {
        VideoPanel.SetActive(false);
    }

    public void OnVideoTab() 
    {
        HidePanels();
        VideoPanel.SetActive(true);
    }
    public void OnAudioTab() 
    {

    }
    public void OnControlsTab() 
    {

    }
}
