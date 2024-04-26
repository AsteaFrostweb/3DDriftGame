using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class Track : MonoBehaviour
{
    public enum Maps { NONE, CARTEENA, SANDY, RACEWAYRIDGE, GEARSHIFT }
    public static string GetMapName(Maps maps) 
    {
        switch (maps) 
        {
            case Maps.NONE:
                return "";
            case Maps.CARTEENA:
                return "Carteena Valley";
            case Maps.SANDY:
                return "Sandy Slalom";
            case Maps.GEARSHIFT:
                return "Gearshift Gorge";
            case Maps.RACEWAYRIDGE:
                return "Raceway Ridge";
            default:
                return "";
        }
    }

    public struct Lap 
    {
        public LapState state;
        public TimeSpan current_time;
        public DateTime start_time;
        public DateTime end_time;
        public Node current_node;
        public int current_node_order_index;

        public Lap(DateTime start, int order_index, Track _track) 
        {
            state = LapState.STARTED;
            start_time = start;
            end_time = DateTime.Now;
            current_time = new TimeSpan(0,0,0,0,0);

            current_node_order_index = order_index;
           
            current_node = _track.GetNodes()[_track.path_node_order[order_index]]; 
            //gets the node that is in slot n in the array, where n is the number stored in position "order_inex" of the tracks "path_node_orders" array
            
        }
    }
    public struct Node
    {
        public float radius;
        public Vector3 position;
        public Node(float _radius, Vector3 _position) 
        {
            radius = _radius;
            position = _position;
        }
    }
    public enum LapState {STARTED, NOT_STARTED}
    public enum TrackType { PATH, ASCENDING}
    [SerializeField]
    public TrackType track_type;

    [Header("The track node objects")]
    [SerializeField]
    public Transform[] node_objects;
    [Header("Which order the nodes come in")]
    public int[] path_node_order; 
    [Header("Does the track loop?")]
    public bool loop;
    [Header("Leave as 0 for infinite loop")]
    public int loop_count;

    private Node[] nodes;
    public void SetNodes(Node[] n) 
    {
        nodes = n;
    }
    public Node[] GetNodes() 
    {
        return nodes;
    }


    
    // Start is called before the first frame update
    void Start()
    {
        PopulateNodeObjects();
        UpdateNodes();  
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void PopulateNodeObjects()
    {
        node_objects = new Transform[node_objects.Length];
      
        for (int i = 0; i < node_objects.Length; i++)
        {
            Debugging.Log("looking for node " + i);
            GameObject go = GameObject.Find("Node" + i);
            if (go != null)
            {
               
                node_objects[i] = go.transform;
                
               
            }
        }
    }
    private void UpdateNodes() 
    {
        Track.Node[] nodes = new Track.Node[node_objects.Length]; ;
        for (int i = 0; i < nodes.Length; i++)
        {
            try
            {
                NodeComponent component = node_objects[i].GetComponent<NodeComponent>();
                nodes[i].position = component.transform.position;
                nodes[i].radius = component.radius;
                Debugging.Log("Assigning node " + i + " position of " + nodes[i].position.ToString() + "   radius of" + nodes[i].radius.ToString());
            }
            catch { Debugging.Log("Node object doesn't have NodeComponent!"); }

        }
        SetNodes(nodes);
    }
}
