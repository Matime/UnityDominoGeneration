using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour {

	// Use this for initialization
	public GameObject ControlPointCamera;

	public GameObject TopplingCamera;

	
	void Start ()
	{
		ControlPointCamera.SetActive(true);
		TopplingCamera.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SwitchToTopplingCamerea()
	{
		
		TopplingCamera.SetActive(true);
		ControlPointCamera.SetActive(false);

	}

	public void SwitchToControlPointCamera()
	{
		TopplingCamera.SetActive(false);
		ControlPointCamera.SetActive(true);
		
	}
}
