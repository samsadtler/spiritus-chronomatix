using UnityEngine;
using System.Collections;

public class StaticController : MonoBehaviour {

	public float StaticAlpha = 0f;
	public float MinStaticAlpha = 0f;
	public float MaxStaticAlpha = .3f;
	public float StaticBlue = .5f;
	public float MinStaticBlue = .5f;
	public float MaxStaticBlue = .3f;
	public int AnimationSpeed = 4;
	public Texture2D[] Textures;
	private int currentImage;
	private int timeSinceLastImage;

	// Use this for initialization
	void Start () {
		currentImage = 0;
		timeSinceLastImage = 0;
		this.GetComponent<GUITexture>().texture = Textures[currentImage];
		setAlpha(StaticAlpha);
	}

	public void setAlpha(float a){
		this.GetComponent<GUITexture>().color = new Color(this.GetComponent<GUITexture>().color.r, this.GetComponent<GUITexture>().color.g, this.GetComponent<GUITexture>().color.b, a);
	}

	public float getAlpha(){
		return this.GetComponent<GUITexture>().color.a;
	}

	public void setTint(float r, float g, float b, float a){
		this.GetComponent<GUITexture>().color = new Color(r, g, b, a);
	}
	
	// Update is called once per frame
	void Update () {
		timeSinceLastImage++;
		if (timeSinceLastImage >= AnimationSpeed) {
			timeSinceLastImage = 0;
			currentImage++;
			currentImage %= Textures.Length;
			this.GetComponent<GUITexture>().texture = Textures[currentImage];
			//setAlpha(StaticAlpha);
			setTint(StaticBlue, StaticBlue, 1, StaticAlpha);
		}
	}
}