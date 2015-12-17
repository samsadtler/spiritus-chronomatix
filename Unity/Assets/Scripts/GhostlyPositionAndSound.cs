using UnityEngine;
using System.Collections;

public class GhostlyPositionAndSound : MonoBehaviour {

	public float VisibleAlpha = .7f;
	public float FadeAmount = .01f;
	public int WaitTime = 10;
	public int AnimationSpeed = 10;
	public float FadeRange = 10;
	public float InvisibleRange = 5;
	public Texture2D[] Textures;
	public Texture2D HighlightedTexture;
	public bool highlighted = false;
	private int currentImage;
	private int timeSinceLastImage;
	private float f;
	private float sinPos = 0;
	private bool sinUp = true;
	private float SinMovement = .04f;
	private float SinMovementSpeed = 1.5f;
	public bool Moving = true;
	public bool Model = true;
	public float SoundRange = 9;
	public bool playSounds = true;
	public float MaxVolume = .4f;

	// Use this for initialization
	void Start () {
		SinMovement = Random.Range (.02f, .04f);
		SinMovementSpeed = Random.Range (1.2f, 1.6f);
		if (!Model) {
			currentImage = 0;
			timeSinceLastImage = 0;
			if(!highlighted) this.GetComponent<Renderer>().material.SetTexture("_MainTex", Textures [currentImage]);
			else this.GetComponent<Renderer>().material.SetTexture("_MainTex", HighlightedTexture);
		}

		// Make Ghost invisible
		setAlpha(0);

		// Inactive at the start
		//if (gameObject.activeInHierarchy)
		//	gameObject.SetActive (false);
	}

	// Update is called once per frame
	void Update () {
		// Make sure the ghost is always facing the camera
		if (!Model) {
			transform.rotation = Quaternion.LookRotation (Camera.main.transform.position - transform.position) * Quaternion.Euler (90, 0, 0);
			transform.rotation = Quaternion.Euler (this.transform.rotation.eulerAngles.x, this.transform.rotation.eulerAngles.y, this.transform.rotation.eulerAngles.z);
		}
	
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
		if(!Model){
			timeSinceLastImage++;
			if (timeSinceLastImage >= AnimationSpeed) {
				timeSinceLastImage = 0;
				currentImage++;
				currentImage %= Textures.Length;
				if(!highlighted) this.GetComponent<Renderer>().material.SetTexture("_MainTex", Textures [currentImage]);
				else this.GetComponent<Renderer>().material.SetTexture("_MainTex", HighlightedTexture);
			}
		}

		// Play sound if we are close enough, otherwise fade out and stop
		if(f <= SoundRange && playSounds){
			if(!GetComponent<AudioSource>().isPlaying){
				GetComponent<AudioSource>().volume = 0;
				GetComponent<AudioSource>().Play();
			}else{
				if(GetComponent<AudioSource>().volume < MaxVolume){
					GetComponent<AudioSource>().volume = Mathf.Min(GetComponent<AudioSource>().volume + FadeAmount, MaxVolume);
				}
			}
		}else{
			if(GetComponent<AudioSource>().isPlaying){
				if(GetComponent<AudioSource>().volume > 0){
					GetComponent<AudioSource>().volume = Mathf.Max(GetComponent<AudioSource>().volume - FadeAmount, 0);
				}else{
					GetComponent<AudioSource>().Stop();
				}
			}
		}

		// Bob ghost in sin wave fashion
		if(Moving){
			this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + Mathf.Sin (sinPos) * SinMovement, this.transform.position.z);
			if (sinUp) {
				sinPos += Time.deltaTime * SinMovementSpeed;
				sinPos = Mathf.Min(1, sinPos);
			} else {
				sinPos -= Time.deltaTime * SinMovementSpeed;
				sinPos = Mathf.Max(-1, sinPos);
			}
			if (sinPos == 1 && sinUp) {
				sinUp = false;
			} else if (sinPos == -1 && !sinUp) {
				sinUp = true;
			}
		}
	}

	public float getPlayerDistance(){
		return Vector3.Distance(this.transform.position, new Vector3 (Camera.main.transform.position.x, this.transform.position.y, Camera.main.transform.position.z));
	}

	void setAlpha(float a){
		if (!Model) {
			this.GetComponent<Renderer> ().material.color = new Color (this.GetComponent<Renderer> ().material.color.r, this.GetComponent<Renderer> ().material.color.g, this.GetComponent<Renderer> ().material.color.b, a);
		}else{
			if(this.GetComponent<Renderer>().material.HasProperty("_Color")){
				this.GetComponent<Renderer>().material.color = new Color(this.GetComponent<Renderer> ().material.color.r, this.GetComponent<Renderer> ().material.color.g, this.GetComponent<Renderer> ().material.color.b, a);
			}
			Renderer[] all = this.GetComponentsInChildren<Renderer>();
			for (int i=0; i < all.Length; i++) {
				if(all[i].material.HasProperty("_Color")){
					all[i].material.color = new Color(all[i].material.color.r, all[i].material.color.g, all[i].material.color.b, a);
				}
			}
		}
	}

	public void stopSounds(){
		playSounds = false;
	}

	public void setHighlight(bool on){
		highlighted = on;
	}

	float getAlpha(){
		return this.GetComponent<Renderer>().material.HasProperty("_Color") ? this.GetComponent<Renderer>().material.color.a : 1;
	}

	void OnGUI(){
		//GUILayout.Label ("dis " + timeSinceLastImage.ToString()) ;
	}

}