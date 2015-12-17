using UnityEngine;
using System.Collections;

public class GhostlyPosition : MonoBehaviour {

	public float VisibleAlpha = .7f;
	public float FadeAmount = .01f;
	public int WaitTime = 10;
	public int AnimationSpeed = 10;
	public float FadeRange = 10;
	public float InvisibleRange = 5;
	public Texture2D[] Textures;
	private int currentImage;
	private int timeSinceLastImage;
	private float f;
	private float sinPos = 0;
	private bool sinUp = true;

	// Use this for initialization
	void Start () {
		currentImage = 0;
		timeSinceLastImage = 0;
		this.GetComponent<Renderer>().material.SetTexture("_MainTex", Textures [currentImage]);

		// Make Ghost invisible
		setAlpha(0);

		// Inactive at the start
		if (gameObject.activeInHierarchy)
			gameObject.SetActive (false);
	}

	// Update is called once per frame
	void Update () {
		// Make sure the ghost is always facing the camera
		transform.rotation = Quaternion.LookRotation( Camera.main.transform.position - transform.position ) * Quaternion.Euler(90, 0, 0);
		transform.rotation = Quaternion.Euler(this.transform.rotation.eulerAngles.x,this.transform.rotation.eulerAngles.y, this.transform.rotation.eulerAngles.z );
	
		// Fade if we are close to the ghost
		f = getPlayerDistance();
		if (Mathf.Abs(f) <= FadeRange) {
			setAlpha(Mathf.Abs(f) <= InvisibleRange ? 0 : VisibleAlpha * ((Mathf.Abs(f) - InvisibleRange)/(FadeRange - InvisibleRange)));
		}

		// If we aren't close to the ghost but it isn't completely visible then slowly fade in
		if (Mathf.Abs (f) > FadeRange && getAlpha () < VisibleAlpha) {
			setAlpha(Mathf.Min(VisibleAlpha, getAlpha() + FadeAmount));
		}

		// Update ghost animation
		timeSinceLastImage++;
		if (timeSinceLastImage >= AnimationSpeed) {
			timeSinceLastImage = 0;
			currentImage++;
			currentImage %= Textures.Length;
			this.GetComponent<Renderer>().material.SetTexture("_MainTex", Textures [currentImage]);
		}

		// Bob ghost in sin wave fashion
		this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + Mathf.Sin (sinPos) * .04f, this.transform.position.z);
		if (sinUp) {
			sinPos += Time.deltaTime * 1.5f;
			sinPos = Mathf.Min(1, sinPos);
		} else {
			sinPos -= Time.deltaTime * 1.5f;
			sinPos = Mathf.Max(-1, sinPos);
		}
		if (sinPos == 1 && sinUp) {
			sinUp = false;
		} else if (sinPos == -1 && !sinUp) {
			sinUp = true;
		}
	}

	public float getPlayerDistance(){
		return Vector3.Distance(this.transform.position, new Vector3 (Camera.main.transform.position.x, this.transform.position.y, Camera.main.transform.position.z));
	}

	public void setAlpha(float a){
		this.GetComponent<Renderer>().material.color = new Color(this.GetComponent<Renderer> ().material.color.r, this.GetComponent<Renderer> ().material.color.g, this.GetComponent<Renderer> ().material.color.b, a);
	}

	public float getAlpha(){
		return this.GetComponent<Renderer>().material.color.a;
	}

	void OnGUI(){
		//GUILayout.Label ("dis " + timeSinceLastImage.ToString()) ;
	}

}