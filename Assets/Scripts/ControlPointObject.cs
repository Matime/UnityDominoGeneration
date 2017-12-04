using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPointObject : MonoBehaviour
{

	public float InitializationTime;
	// Use this for initialization
	void Start ()
	{
		InitializationTime = Time.timeSinceLevelLoad;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
