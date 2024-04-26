using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static Track;

public class MenuUIHandler : MonoBehaviour
{
    public enum SubMenus {NONE, SETTINGS, MAP_SELECT, CAR_SELECT, CREDITS, HIGHSCORES};
   
    
    private NetworkManager networkManager;
    private GameState game_state;
    private SubMenus current_sub_menu;
    private Maps current_map;
    private GameObject logout_button;

    public TextMeshProUGUI usernameTMP;

    public GameObject SettingsPanel;
    public GameObject main_menu_panel;
    public GameObject map_select_panel;
    public GameObject car_select_panel;
    public GameObject credits_panel;
  
    private GameObject[] sub_panels;
    private GameObject lap_count_slider_obj;
    private UnityEngine.UI.Slider lap_count_slider;
    private TextMeshProUGUI lap_count_value;
    private bool username_set = false;
    // Start is called before the first frame update
    void Start()
    {
        networkManager = GameObject.FindAnyObjectByType<NetworkManager>();
        lap_count_slider_obj = GameObject.Find("LapCountSlider");
       
        game_state = GameObject.Find("GameState").GetComponent<GameState>();
        game_state.game_state = GameState.State.MENU;
        logout_button = GameObject.Find("LogoutButton");
        current_sub_menu = SubMenus.NONE;
        current_map = Maps.NONE;

        sub_panels = new GameObject[] { map_select_panel, car_select_panel, credits_panel , SettingsPanel};
        CloseSubMenus();

      
    }

    // Update is called once per frame
    void Update()
    {
        if (lap_count_slider_obj != null)
        {            
            lap_count_slider = lap_count_slider_obj.GetComponent<UnityEngine.UI.Slider>();
            lap_count_value = lap_count_slider_obj.transform.Find("Value").GetComponent<TextMeshProUGUI>();
            lap_count_value.text = ((int)lap_count_slider.value).ToString();
        }
        else 
        {
            lap_count_slider_obj = GameObject.Find("LapCountSlider");
        }

        if (usernameTMP != null && !username_set)
        {
            if (logout_button == null) 
            {
                logout_button = GameObject.Find("LogoutButton");
                Debugging.Log("Could't find logout button");
                if (logout_button == null) return;
            }
            if (networkManager.IsLoggedIn)
            {                
                logout_button.SetActive(true);
                usernameTMP.text = "User: " + networkManager.Username;
            }
            else
            {
                logout_button.SetActive(false); 
                usernameTMP.text = "User: Offline";
            }
            username_set = true;
        }
    }



    public void OnPlay() 
    {
        
        if (current_sub_menu == SubMenus.MAP_SELECT || current_sub_menu == SubMenus.CAR_SELECT) 
        {
            if (map_select_panel.activeInHierarchy)
            {
                map_select_panel.SetActive(false);
                current_sub_menu = SubMenus.MAP_SELECT;
                return;
            }
        }
        if (current_sub_menu != SubMenus.NONE)
        {
            //if are in some other menu that isnt the play menu OR none
            CloseSubMenus();
        }
        CloseSubMenus();
        map_select_panel.SetActive(true);
        current_sub_menu = SubMenus.MAP_SELECT;
    }
    public void OnSettings()
    {
        CloseSubMenus();
        SettingsPanel.SetActive(true);
    }
    public void OnCredits()
    {

    }
    public void OnQuit()
    {
        Application.Quit();
    }


    void CloseSubMenus() 
    {
        foreach (GameObject panel in sub_panels) 
        {
            if (panel != null) 
            {
                panel.SetActive(false);
            }
        }       
    }







    //-------- Map Select Functions----------
    public void OnCarteenaValley()
    {
        current_map = Maps.CARTEENA;
        game_state.current_map = Maps.CARTEENA;
        CloseSubMenus();
        car_select_panel.SetActive(true);
        
    }
    public void OnSandySlalom()
    {
        current_map = Maps.SANDY;
        game_state.current_map = Maps.SANDY;
        CloseSubMenus();
        car_select_panel.SetActive(true);

    }
    public void OnRacewayRidge()
    {
        current_map = Maps.RACEWAYRIDGE;
        game_state.current_map = Maps.RACEWAYRIDGE;
        CloseSubMenus();
        car_select_panel.SetActive(true);
    }
    public void OnGearshiftGorge()
    {
        current_map = Maps.GEARSHIFT;
        game_state.current_map = Maps.GEARSHIFT;
        CloseSubMenus();
        car_select_panel.SetActive(true);
    }


    //--------Car Select Functions--------

    public void OnGreenCar() 
    {
        SelectCar(GameState.Cars.GREEN);
    }
    public void OnWhiteCar()
    {
        SelectCar(GameState.Cars.WHITE);
    }
    public void OnRedCar()
    {
        SelectCar(GameState.Cars.RED);
    }
    public void OnBlueCar()
    {
        SelectCar(GameState.Cars.BLUE);
    }

    private void SelectCar(GameState.Cars car) 
    {     
        game_state.current_car = car;
        game_state.lap_count = (int)lap_count_slider.value;
        game_state.game_state = GameState.State.IN_GAME;

        CloseSubMenus();                
        LoadMap(current_map);
    }

    void LoadMap(Maps map) 
    {
        switch (map) 
        {
            case Maps.CARTEENA:
                SceneManager.LoadScene("CarteenaValley");
                break;
            case Maps.SANDY:
                SceneManager.LoadScene("SandySlalom");
                break;
            case Maps.RACEWAYRIDGE:
                SceneManager.LoadScene("RacewayRidge");
                break;
            case Maps.GEARSHIFT:
                SceneManager.LoadScene("GearshiftGorge");
                break;
        }
    }



}
