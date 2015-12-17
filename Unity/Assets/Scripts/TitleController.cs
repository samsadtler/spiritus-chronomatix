using UnityEngine;
using System.Collections;

public class TitleController : MonoBehaviour {

	public Texture2D Texture;
	private Color titleColor;

	// Use this for initialization
	void Start () {
		titleColor = Color.white;
	}
	
	// Update is called once per frame
	void Update () {
		//titleColor = new Color(1, 1, 1, 0f);
	}

	public void setAlpha(float a){
		titleColor = new Color (titleColor.r, titleColor.g, titleColor.b, a);
	}

	public float getAlpha(){
		return titleColor.a;
	}

	void OnGUI() {
		GUI.color = titleColor;
		GUI.DrawTexture(new Rect(0.0f, 0.0f, Screen.width, Screen.height), Texture);
		GUI.color = Color.white;
	}
}
