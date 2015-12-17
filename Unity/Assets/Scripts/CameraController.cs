using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	WebCamTexture CameraTexture;
	string cams = "";

	// Use this for initialization
	void Start () {
		WebCamDevice[] devices = WebCamTexture.devices;
		string backCamName="";
		for( int i = 0 ; i < devices.Length ; i++ ) {
			cams += devices[i].name;
			Debug.Log("Device:" + devices[i].name + "IS FRONT FACING:" + devices[i].isFrontFacing);
			
			if (!devices[i].isFrontFacing) {
				backCamName = devices[i].name;
			}
		}

		if (backCamName == "" && devices.Length > 0) {
			backCamName = devices[0].name;
		}

		backCamName = devices [0].name;
		
		CameraTexture = new WebCamTexture(backCamName,640,360,15);
		CameraTexture.Play();
		this.GetComponent<GUITexture>().texture = CameraTexture;
	}
	
	// Update is called once per frame
	void Update () {

	}

	void OnGUI(){
		//GUILayout.Label ("dis " + Time.deltaTime.ToString()) ;
	}
}
