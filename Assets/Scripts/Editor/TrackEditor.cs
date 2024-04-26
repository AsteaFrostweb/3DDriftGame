using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Track))]
public class TrackEditor : Editor
{
    Track target_track;
    GameObject[] node_objs;  
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        target_track = (Track)target;

        PopulateNodeObjects();

        PopulatePathNodeOrder();

        PopulateNodes();
       
    }

    void PopulateNodeObjects() 
    {
        target_track.node_objects = new Transform[target_track.node_objects.Length];
        node_objs = new GameObject[target_track.node_objects.Length];
        for (int i = 0; i < target_track.node_objects.Length; i++)
        {
            GameObject go = GameObject.Find("Node" + i);
            if (go != null)
            {
                target_track.node_objects[i] = go.transform;
                node_objs[i] = go;
            }
        }
    }

    void PopulatePathNodeOrder() 
    {
        if (target_track.track_type == Track.TrackType.ASCENDING)
        {
            target_track.path_node_order = new int[target_track.node_objects.Length];
            for (int i = 0; i < target_track.node_objects.Length; i++)
            {
                target_track.path_node_order[i] = i;
            }
        }
    }
    void PopulateNodes() 
    {
        Track.Node[] nodes = new Track.Node[node_objs.Length]; ;
        for (int i = 0; i < nodes.Length; i++) 
        {
            try {
                NodeComponent component = node_objs[i].GetComponent<NodeComponent>();
                nodes[i].position = component.transform.position;
                nodes[i].radius = component.radius;
            }
            catch { Debugging.Log("Node object doesn't have NodeComponent!"); }
            
        }
        target_track.SetNodes(nodes);
    }
}