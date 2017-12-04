using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpawnOnClick : MonoBehaviour
{


    Vector3 mousePosition, targetPosition;

    //To Instantiate TargetObject at mouse position
    public Transform targetObject;

    public GameObject toPlace;

    public bool doUpdate = true;
    float distance = 30.0f;

    // Use this for initialization
    void Start()
    {

    }

    public void StopUpdate()
    {
        doUpdate = false;
    }

    public void ResumeUpdate()
    {
        doUpdate = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (doUpdate)
        {
            //To get the current mouse position
            mousePosition = Input.mousePosition;

            //Convert the mousePosition according to World position
            targetPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, distance));

            //Set the position of targetObject
            targetObject.position = targetPosition;

            //Debug.Log(mousePosition+"   "+targetPosition);


            //If Left Button is clicked
            if (Input.GetMouseButtonUp(0))
            {
                if(!EventSystem.current.IsPointerOverGameObject())
                //create the instance of targetObject and place it at given position.
                    Instantiate(toPlace, targetObject.transform.position, targetObject.transform.rotation);
            }
        }
    }
}
