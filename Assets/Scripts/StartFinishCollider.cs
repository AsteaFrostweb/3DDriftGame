using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartFinishCollider : MonoBehaviour
{
    public RaceGameplayHandler gameplayHandler;
    private void Start()
    {
        gameplayHandler = GameObject.FindAnyObjectByType<RaceGameplayHandler>();
    }
    private void OnTriggerEnter(Collider collision)
    {
        Debugging.Log("Enter");
        gameplayHandler.SetFinishline(collision.gameObject.transform.parent.gameObject, true);
    }
    private void OnTriggerExit(Collider collision)
    {
        Debugging.Log("Exit");
        gameplayHandler.SetFinishline(collision.gameObject.transform.parent.gameObject, false);
    }
 
}
