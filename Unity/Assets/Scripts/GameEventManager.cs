using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;

public class GameEventManager : MonoBehaviour {
	
	public int GameState = 0;
	public int LastGameState = 1;
	public int TouchState = 0;
	public float FadeAmount = .02f;
	public float NoTouchAlpha = 1;
	public float OneTouchAlpha = .5f;
	public float TwoTouchAlpha = 0;
	// In Pixels
	public int TouchRange = 400;
	public GameObject Title;
	public float TitleVolume = .3f;
	private TitleController TitleScript;
	public GameObject End;
	private TitleController EndScript;
	public GameObject Static;
	private StaticController StaticScript;
	public float StaticVolume = .08f;
	public float MinStaticVolume = .01f;
	public float StaticFade = .002f;
	public GameObject ClaraGhost;
	private GhostlyPosition ClaraGhostScript;
	public GameObject Hankerchief;
	private GhostlyPositionAndSound HankerchiefScript;
	public int ClaraWaitTime = 10;
	public float ClaraDialogThreshold = 10;
	public GameObject[] PresentGhostlyObjects;
	public GameObject[] TimeTravelGhostlyObjects;
	public GameObject[] PastGhostlyObjects;
	public GameObject[] AllSelectableObjects;
	private GhostlyPositionAndSound[] AllGhostlyScripts;
	public Canvas Interface;
	public Image GoalMessage;
	public bool fadeOutGoalMessage = false;
	public Image ReturnMessage;
	public bool fadeOutReturnMessage = false;
	public bool pickedUpHankerchief = false;
	public int MessageDisplayTime;
	public Button PickUp;
	public Slider Slider;
	public bool sliderMovement = false;
	public bool lastSliderMovement = false;
	public float SliderMovementSpeed;
	public AudioSource[] Sounds;
	
	public const int TitleSoundIndex = 0;
	public const int StaticSoundIndex = 1;
	public const int StaticSound2Index = 2;
	public const int SuccessSoundIndex = 3;
	public const int ItemSelectIndex = 4;
	public const int ItemReleaseIndex = 5;
	public const int ItemSelectFailIndex = 6;
	public const int EndSoundIndex = 7;
	
	public const int Present = 0;
	public const int Past = 1;
	private float lastValue = Present;
	private float oldValue = Present;
	int command = 0;
	int lastCommand = -1;
	int Delay = 10;
	
	int Highlighted = -1;
	int Selected = -1;
	
	// Bluetooth
	//private SerialPort sp;
	//private string c = "1";
	private AndroidJavaClass jc;
	private AndroidJavaObject activity;
	private AndroidJavaObject jo;
	private const string RelieverNormal = "0"; //half glow, half vibe. low flicker lights
	private const string RelieverActive = "1"; //full glow, high vibe , fast flicker lights
	private const string RelieverTimeTravel = "2"; //sparkly, high vibe, fast flicker lights
	private const string RelieverHold = "3"; //red, high vibe, fast flicker lights
	private const string RelieverRelease = "4"; // blue, SILENT, lights normal
	private const string RelieverRed = "5"; // set color red
	private const string RelieverBlue = "6"; // set color blue
	private const string RelieverLowVibe = "7"; // set vibration to low
	private const string RelieverHighVibe = "8"; // set vibration to high
	
	// Use this for initialization
	void Start () {
		// Setup Bluetooth
		if (Application.platform == RuntimePlatform.Android){
			Debug.Log ("=============EFM HERE==============");
			jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); 
			activity = jc.GetStatic<AndroidJavaObject>("currentActivity");
			Debug.Log ("=============ONE==============");
			jo = new AndroidJavaObject("SimpleAndroidBluetoothCommunication");
			Debug.Log ("=============TWO==============");
			
			Debug.Log("=============SUCCESS: " + jo.Call<string> ("setupAdapter"));
			Debug.Log("=============ADAPTER STATUS: " + jo.Call<string> ("adapterOn"));
			int numberPairedDevices = jo.Call<int>("getPairedDevices");
			Debug.Log ("=============NUMBER PAIRED DEVICES: " + numberPairedDevices.ToString());
			for (int i = 0; i < numberPairedDevices; i++) {
				Debug.Log ("=============DEVICE " + i.ToString() + " INFO: " + jo.Call<string> ("getPairedDeviceInfo", i));
			}
			string connected = jo.Call<string> ("startCommunication");
			Debug.Log ("=============CONNECTION SUCCESS: " + connected);
			
			StartCoroutine(MessageReliever(RelieverNormal));
		}
		
		// Handle Variables
		TitleScript = Title.GetComponent<TitleController>();
		EndScript = End.GetComponent<TitleController> ();
		EndScript.setAlpha (0);
		StaticScript = Static.GetComponent<StaticController>();
		ClaraGhostScript = ClaraGhost.GetComponent<GhostlyPosition>();
		HankerchiefScript = Hankerchief.GetComponent<GhostlyPositionAndSound>();
		Sounds = this.GetComponents<AudioSource>();
		PickUp.onClick.AddListener(pickup);
		Slider.maxValue = Past;
		Slider.minValue = Present;
		Slider.wholeNumbers = false;
		Slider.value = Present;
		Slider.onValueChanged.AddListener(TimeTravel);
		Interface.gameObject.SetActive (false);
		
		AllGhostlyScripts = new GhostlyPositionAndSound[AllSelectableObjects.Length];
		for (int i = 0; i < AllSelectableObjects.Length; i++) {
			AllGhostlyScripts[i] = AllSelectableObjects[i].GetComponent<GhostlyPositionAndSound>();
		}
		
		for(int i = 0; i < PresentGhostlyObjects.Length; i++){
			PresentGhostlyObjects[i].SetActive(true);
		}
		for(int i = 0; i < TimeTravelGhostlyObjects.Length; i++){
			TimeTravelGhostlyObjects[i].SetActive(false);
		}
		for(int i = 0; i < PastGhostlyObjects.Length; i++){
			PastGhostlyObjects[i].SetActive(false);
		}
		
		if (GameState == 0) {
			Sounds [TitleSoundIndex].volume = TitleVolume;
			Sounds [TitleSoundIndex].Play ();
			TitleScript.setAlpha (NoTouchAlpha);
			StaticScript.setAlpha (0);
		} else if (GameState == 5) {
			Interface.gameObject.SetActive (true);
			Debug.Log(Interface.gameObject.activeSelf);
			Sounds[SuccessSoundIndex].Play();
			StartCoroutine(SetupClara());
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			jo.Call("stopCommunication");
			Application.Quit ();
		}
		
		if (Input.touches.Length > 0 && Delay == 0) {
			command++;
			command %= 9;
			Delay = 10;
		}
		if (Delay > 0) {
			Delay--;
		}
		
		// Make sure that there are two touch points on the tango screen, set game state appropriately
		// If only want to do this one time, oncomment the line below
		if(GameState == 0){
			Touch[] myTouches = Input.touches;
			bool leftSide = false;
			bool rightSide = false;
			//Debug.Log ("# Touches: " + myTouches.Length);
			for (int i = 0; i < Input.touchCount; i++) {
				//Debug.Log("Position: " + myTouches[i].position.x + "   "  + myTouches[i].position.y);
				if(myTouches[i].position.x >= 0 && myTouches[i].position.x <= TouchRange){
					leftSide = true;
				}else if(myTouches[i].position.x >= Screen.width - TouchRange && myTouches[i].position.x <= Screen.width){
					rightSide = true;
				}
			}
			
			//Debug.Log ("Volume: " + Sounds[TitleSoundIndex].volume + " Left: " + leftSide.ToString () + " Right: " + rightSide.ToString ());
			if (leftSide && rightSide) {
				StartCoroutine(SetupClara());
				StartCoroutine(MessageReliever(RelieverRed));
				GameState = LastGameState;
			}else if (leftSide || rightSide) {
				if(GameState != 0){
					LastGameState = GameState;
					Sounds[TitleSoundIndex].volume = 0;
					Sounds[TitleSoundIndex].Play();
				}
				if(TouchState == 0){
					StartCoroutine(MessageReliever(RelieverActive));
				}
				GameState = 0;
				TouchState = 1;
			} else {
				if(GameState != 0){
					LastGameState = GameState;
					Sounds[TitleSoundIndex].volume = 0;
					Sounds[TitleSoundIndex].Play();
				}
				if(TouchState == 1){
					StartCoroutine(MessageReliever(RelieverNormal));
				}
				GameState = 0;
				TouchState = 0;
			}
		}
		
		// Update based on game state
		UpdateState ();
	}
	
	void UpdateState(){
		//Debug.Log ("LAST GAME STATE: " + LastGameState + " GAME STATE: " + GameState + " TOUCH STATE: " + TouchState);
		// Set our alpha based on touch state
		if (GameState == 0) {
			if(TouchState == 0){
				// Visual
				if(TitleScript.getAlpha() < NoTouchAlpha){
					TitleScript.setAlpha(Mathf.Min(TitleScript.getAlpha() + FadeAmount, NoTouchAlpha));
				}
				// Audio
				if(Sounds[TitleSoundIndex].volume < TitleVolume){
					Sounds[TitleSoundIndex].volume = Mathf.Min(Sounds[TitleSoundIndex].volume + FadeAmount, TitleVolume);
				}
			}else if(TouchState == 1){
				// Visual
				if(TitleScript.getAlpha() > OneTouchAlpha){
					TitleScript.setAlpha(Mathf.Max(TitleScript.getAlpha() - FadeAmount, OneTouchAlpha));
				}else if(TitleScript.getAlpha() < OneTouchAlpha){
					TitleScript.setAlpha(Mathf.Min(TitleScript.getAlpha() + FadeAmount, OneTouchAlpha));
				}
				// Audio
				if(Sounds[TitleSoundIndex].volume > TitleVolume/2){
					Sounds[TitleSoundIndex].volume = Mathf.Max(Sounds[TitleSoundIndex].volume - FadeAmount, TitleVolume/2);
				}else if(Sounds[TitleSoundIndex].volume < TitleVolume/2){
					Sounds[TitleSoundIndex].volume = Mathf.Min(Sounds[TitleSoundIndex].volume + FadeAmount, TitleVolume/2);
				}
			}
		}
		// Make sure we can't see the title and then carry on with the other states
		else {
			// Visual
			if(TitleScript.getAlpha() > TwoTouchAlpha){
				TitleScript.setAlpha(Mathf.Max(TitleScript.getAlpha() - FadeAmount, TwoTouchAlpha));
			}
			
			// Audio
			if(Sounds[TitleSoundIndex].volume > 0){
				Sounds[TitleSoundIndex].volume = Mathf.Max(Sounds[TitleSoundIndex].volume - FadeAmount, 0);
			}else{
				if(Sounds[TitleSoundIndex].isPlaying){
					Sounds[TitleSoundIndex].Stop();
				}
			}
			
			// We are searching and haven't met Clara yet
			if(GameState == 1){
				// Make sure baseline static is playing
				if(!Sounds[StaticSoundIndex].isPlaying){
					Sounds[StaticSoundIndex].Play();
					Sounds[StaticSoundIndex].volume = MinStaticVolume;
				}
				
				// Don't do anything else until clara is active...
				if(!ClaraGhost.activeSelf) return;
				
				if(ClaraGhostScript.getPlayerDistance() < ClaraDialogThreshold){
					ClaraGhost.GetComponents<AudioSource>()[0].Play();
					Sounds[StaticSoundIndex].volume = MinStaticVolume;
					GameState = 2;
					StartCoroutine(MessageReliever(RelieverHold));
				}
			}
			// We are searching and have heard Clara's first dialogue so play static for a bit...
			else if(GameState == 2){
				if(!ClaraGhost.activeSelf) return;
				
				float ghostDist = ClaraGhostScript.getPlayerDistance();
				
				// Visual
				if(ghostDist < ClaraDialogThreshold){
					float blue = ghostDist > ClaraDialogThreshold/2 ? (1 - (ghostDist - ClaraDialogThreshold/2)/(ClaraDialogThreshold/2)) * StaticScript.MaxStaticBlue : StaticScript.MaxStaticBlue;
					StaticScript.setTint (blue, blue, 1, ghostDist > ClaraDialogThreshold/2 ? (1 - (ghostDist - ClaraDialogThreshold/2)/(ClaraDialogThreshold/2)) * StaticScript.MaxStaticAlpha : StaticScript.MaxStaticAlpha); 
					//StaticScript.setAlpha(ghostDist > ClaraDialogThreshold/2 ? ((1 - ghostDist/ClaraDialogThreshold) * StaticScript.MaxStaticAlpha) : StaticScript.MaxStaticAlpha);
				}else{
					StaticScript.setAlpha(0);
				}
				
				// Audio
				if(ClaraGhost.GetComponents<AudioSource>()[0].isPlaying){
					if(ghostDist < ClaraDialogThreshold){
						ClaraGhost.GetComponents<AudioSource>()[0].mute = false;
						if(ghostDist > ClaraDialogThreshold/2){
							float targetVolume = ((ghostDist - ClaraDialogThreshold/2)/(ClaraDialogThreshold/2)) * StaticVolume;
							if(targetVolume > Sounds[StaticSoundIndex].volume){
								Sounds[StaticSoundIndex].volume = Mathf.Max(targetVolume, Sounds[StaticSoundIndex].volume - StaticFade);
							}else{
								Sounds[StaticSoundIndex].volume = Mathf.Min(targetVolume, Sounds[StaticSoundIndex].volume + StaticFade);
							}
						}else{
							Sounds[StaticSoundIndex].volume = Mathf.Max(MinStaticVolume, Sounds[StaticSoundIndex].volume - StaticFade);
						}
						//Sounds[StaticSoundIndex].volume = ghostDist > ClaraDialogThreshold/2 ? ((ghostDist/ClaraDialogThreshold) * StaticVolume)/2 : .03f;
					}else{
						Sounds[StaticSoundIndex].volume = Mathf.Max(MinStaticVolume, Sounds[StaticSoundIndex].volume - StaticFade);
						ClaraGhost.GetComponents<AudioSource>()[0].mute = true;
					}
				}else{
					Sounds[StaticSoundIndex].volume = MinStaticVolume;
					Sounds[StaticSound2Index].Play();
					Sounds[StaticSound2Index].volume = 0;//.25f;
					GameState = 3;
					StartCoroutine(MessageReliever(RelieverRed));
				}
			}
			// Play static interuption
			else if(GameState == 3){
				float ghostDist = ClaraGhostScript.getPlayerDistance();
				
				// Visual
				if(ghostDist < ClaraDialogThreshold){
					float blue = ghostDist > ClaraDialogThreshold/2 ? (1 - (ghostDist - ClaraDialogThreshold/2)/(ClaraDialogThreshold/2)) * StaticScript.MaxStaticBlue : StaticScript.MaxStaticBlue;
					StaticScript.setTint (blue, blue, 1, ghostDist > ClaraDialogThreshold/2 ? (1 - (ghostDist - ClaraDialogThreshold/2)/(ClaraDialogThreshold/2)) * StaticScript.MaxStaticAlpha : StaticScript.MaxStaticAlpha); 
					//StaticScript.setAlpha(ghostDist > ClaraDialogThreshold/2 ? ((1 - ghostDist/ClaraDialogThreshold) * StaticScript.MaxStaticAlpha) : StaticScript.MaxStaticAlpha);
				}else{
					StaticScript.setAlpha(0);
				}
				
				if(Sounds[StaticSound2Index].isPlaying){
					//Sounds[StaticSound2Index].volume = ghostDist > ClaraDialogThreshold/2 ? ((ghostDist/ClaraDialogThreshold) * StaticVolume)/2 : .03f;
				}else{
					// If the player is close enough, start the next dialogue
					if(ghostDist < ClaraDialogThreshold){
						Sounds[StaticSoundIndex].volume = MinStaticVolume;
						ClaraGhost.GetComponents<AudioSource>()[1].Play();
						GameState = 4;
						StartCoroutine(MessageReliever(RelieverHold));
					}
				}
			}
			// Listen to Clara's second dialogue and show time travel interface
			else if(GameState == 4){
				float ghostDist = ClaraGhostScript.getPlayerDistance();
				
				// Visual
				if(ghostDist < ClaraDialogThreshold){
					float blue = ghostDist > ClaraDialogThreshold/2 ? (1 - (ghostDist - ClaraDialogThreshold/2)/(ClaraDialogThreshold/2)) * StaticScript.MaxStaticBlue : StaticScript.MaxStaticBlue;
					StaticScript.setTint (blue, blue, 1, ghostDist > ClaraDialogThreshold/2 ? (1 - (ghostDist - ClaraDialogThreshold/2)/(ClaraDialogThreshold/2)) * StaticScript.MaxStaticAlpha : StaticScript.MaxStaticAlpha); 
					//StaticScript.setAlpha(ghostDist > ClaraDialogThreshold/2 ? ((1 - ghostDist/ClaraDialogThreshold) * StaticScript.MaxStaticAlpha) : StaticScript.MaxStaticAlpha);
				}else{
					StaticScript.setAlpha(0);
				}
				
				// Audio
				if(ClaraGhost.GetComponents<AudioSource>()[1].isPlaying){
					if(ghostDist < ClaraDialogThreshold){
						ClaraGhost.GetComponents<AudioSource>()[1].mute = false;
						if(ghostDist > ClaraDialogThreshold/2){
							float targetVolume = ((ghostDist - ClaraDialogThreshold/2)/(ClaraDialogThreshold/2)) * StaticVolume;
							if(targetVolume > Sounds[StaticSoundIndex].volume){
								Sounds[StaticSoundIndex].volume = Mathf.Max(targetVolume, Sounds[StaticSoundIndex].volume - StaticFade);
							}else{
								Sounds[StaticSoundIndex].volume = Mathf.Min(targetVolume, Sounds[StaticSoundIndex].volume + StaticFade);
							}
						}else{
							Sounds[StaticSoundIndex].volume = Mathf.Max(MinStaticVolume, Sounds[StaticSoundIndex].volume - StaticFade);
						}
						//Sounds[StaticSoundIndex].volume = ghostDist > ClaraDialogThreshold/2 ? ((ghostDist/ClaraDialogThreshold) * StaticVolume)/2 : .03f;
					}else{
						Sounds[StaticSoundIndex].volume = Mathf.Max(MinStaticVolume, Sounds[StaticSoundIndex].volume - StaticFade);
						ClaraGhost.GetComponents<AudioSource>()[1].mute = true;
					}
				}else{
					Sounds[StaticSoundIndex].volume = MinStaticVolume;
					Interface.gameObject.SetActive (true);
					Color c = GoalMessage.color;
					c.a = 0;
					GoalMessage.color = c;
					c = ReturnMessage.color;
					c.a = 0;
					ReturnMessage.color = c;
					fadeOutGoalMessage = false;
					StartCoroutine(hideGoalMessage());
					Sounds[SuccessSoundIndex].Play();
					if(Sounds[StaticSoundIndex].volume > MinStaticVolume){
						Sounds[StaticSoundIndex].volume = Mathf.Max(MinStaticVolume, Sounds[StaticSoundIndex].volume - StaticFade);
					}else{
						Sounds[StaticSoundIndex].volume = Mathf.Min(MinStaticVolume, Sounds[StaticSoundIndex].volume + StaticFade);
					}
					GameState = 5;
					if(!ClaraGhost.GetComponents<AudioSource>()[4].isPlaying){
						ClaraGhost.GetComponents<AudioSource>()[4].volume = .6f;
						ClaraGhost.GetComponents<AudioSource>()[4].loop = true;
						ClaraGhost.GetComponents<AudioSource>()[4].Play();
					}
					StartCoroutine(MessageReliever(RelieverRed));
				}
			}else if(GameState == 5){
				//Debug.Log ("Highlighted: " + Highlighted);
				if(Selected != -1){
					AllSelectableObjects[Selected].transform.position = Camera.main.transform.position + Camera.main.transform.forward * 8;
				}
				
				if(Slider.value == Present){
					ClaraGhost.SetActive(true);
					float ghostDist = ClaraGhostScript.getPlayerDistance();
					if(Selected != -1 && ghostDist < ClaraDialogThreshold - ClaraDialogThreshold/4){
						if(AllSelectableObjects[Selected].tag == "hankerchief"){
							if(!ClaraGhost.GetComponents<AudioSource>()[2].isPlaying){
								ClaraGhost.GetComponents<AudioSource>()[2].Play();
								if(ClaraGhost.GetComponents<AudioSource>()[4].isPlaying){
									ClaraGhost.GetComponents<AudioSource>()[4].Stop();
								}
								AllGhostlyScripts[Selected].stopSounds();
								Interface.gameObject.SetActive(false);
								GameState = 6;
							}
						}else{
							if(ClaraGhost.GetComponents<AudioSource>()[4].isPlaying){
								ClaraGhost.GetComponents<AudioSource>()[4].Stop();
							}
							if(!ClaraGhost.GetComponents<AudioSource>()[5].isPlaying){
								ClaraGhost.GetComponents<AudioSource>()[5].Play();
							}
						}
					}else{
						if(ClaraGhost.GetComponents<AudioSource>()[5].isPlaying){
							ClaraGhost.GetComponents<AudioSource>()[5].Stop();
						}
						if(!ClaraGhost.GetComponents<AudioSource>()[4].isPlaying){
							ClaraGhost.GetComponents<AudioSource>()[4].volume = .6f;
							ClaraGhost.GetComponents<AudioSource>()[4].loop = true;
							ClaraGhost.GetComponents<AudioSource>()[4].Play();
						}
					}
					
					if(Sounds[StaticSoundIndex].volume > MinStaticVolume){
						Sounds[StaticSoundIndex].volume = Mathf.Max(MinStaticVolume, Sounds[StaticSoundIndex].volume - StaticFade);
					}else{
						Sounds[StaticSoundIndex].volume = Mathf.Min(MinStaticVolume, Sounds[StaticSoundIndex].volume + StaticFade);
					}
					
					for(int i = 0; i < PresentGhostlyObjects.Length; i++){
						PresentGhostlyObjects[i].SetActive(true);
					}
					for(int i = 0; i < PastGhostlyObjects.Length; i++){
						PastGhostlyObjects[i].SetActive(false);
					}
					if(Selected != -1){
						AllSelectableObjects[Selected].SetActive(true);
					}
					
					setHighlighted(AllSelectableObjects);
				}else if(Slider.value == Past){
					// Do past stuff here
					if(ClaraGhost.GetComponents<AudioSource>()[4].isPlaying){
						ClaraGhost.GetComponents<AudioSource>()[4].Pause();
					}
					ClaraGhost.SetActive(false);
					Sounds[StaticSoundIndex].volume = 0;
					for(int i = 0; i < PresentGhostlyObjects.Length; i++){
						PresentGhostlyObjects[i].SetActive(false);
					}
					for(int i = 0; i < PastGhostlyObjects.Length; i++){
						PastGhostlyObjects[i].SetActive(true);
					}
					
					if(Selected != -1){
						AllSelectableObjects[Selected].SetActive(true);
					}
					
					setHighlighted(AllSelectableObjects);
				}else{
					// Do time travel stuff here
					if(ClaraGhost.GetComponents<AudioSource>()[4].isPlaying){
						ClaraGhost.GetComponents<AudioSource>()[4].Pause();
					}
					ClaraGhost.SetActive(false);
					Sounds[StaticSoundIndex].volume = 0;
					for(int i = 0; i < PresentGhostlyObjects.Length; i++){
						PresentGhostlyObjects[i].SetActive(false);
					}
					for(int i = 0; i < PastGhostlyObjects.Length; i++){
						PastGhostlyObjects[i].SetActive(false);
					}
					if(Selected != -1){
						AllSelectableObjects[Selected].SetActive(true);
					}
				}
			}
			else if(GameState == 6){
				if(!ClaraGhost.GetComponents<AudioSource>()[2].isPlaying){
					AllSelectableObjects[Selected].SetActive(false);
					GameState = 7;
					StartCoroutine(MessageReliever(RelieverTimeTravel));
				}
			}else if(GameState == 7){
				if(!Sounds[EndSoundIndex].isPlaying){
					Sounds[EndSoundIndex].volume = .01f;
					Sounds[EndSoundIndex].Play();
				}
				
				if(Sounds[EndSoundIndex].volume < .3f){
					Sounds[EndSoundIndex].volume = Mathf.Min(Sounds[EndSoundIndex].volume + .01f, .3f);
				}
				
				if(EndScript.getAlpha() < 1){
					EndScript.setAlpha(Mathf.Min(EndScript.getAlpha() + FadeAmount, 1));
					ClaraGhostScript.setAlpha(Mathf.Max (ClaraGhostScript.getAlpha() - FadeAmount, 0));
				}else{
					ClaraGhost.SetActive(false);
				}
			}
		}
		
		// Special cases handling showing the popup message and automatically moving the slider...
		if (GameState >= 5) {
			if(fadeOutGoalMessage){
				Color d = GoalMessage.color;
				d.a = Mathf.Max(d.a - FadeAmount, 0);
				GoalMessage.color = d;
			}else{
				Color d = GoalMessage.color;
				d.a = Mathf.Min(d.a + FadeAmount, 1);
				GoalMessage.color = d;
			}

			if(pickedUpHankerchief){
				if(fadeOutReturnMessage){
					Color e = ReturnMessage.color;
					e.a = Mathf.Max(e.a - FadeAmount, 0);
					ReturnMessage.color = e;
				}else{
					Color e = ReturnMessage.color;
					e.a = Mathf.Min(e.a + FadeAmount, 1);
					ReturnMessage.color = e;
				}
			}

			// Time travel communication with the reliever
			if((lastValue != Past && oldValue == Past) || (lastValue != Present && oldValue == Present)){
				//StartCoroutine(MessageReliever(RelieverTimeTravel));
			}else if(oldValue != Past && lastValue == Past){
				StartCoroutine (MessageReliever (RelieverRelease));
			}else if(oldValue != Present && lastValue == Present){
				StartCoroutine (MessageReliever (RelieverRed));
			}


			// Automatically update slider positions
			lastSliderMovement = sliderMovement;
			if(lastSliderMovement == false && lastValue != Present && lastValue != Past){
				if(oldValue < lastValue){
					Slider.value = Mathf.Min(Slider.value + SliderMovementSpeed, Past);
				}else{
					Slider.value = Mathf.Max(Slider.value - SliderMovementSpeed, Present);
				}
			}
			oldValue = lastValue;
			lastValue = Slider.value;
			sliderMovement = false;
		}
	}
	
	private IEnumerator hideGoalMessage(){
		yield return new WaitForSeconds(MessageDisplayTime);
		fadeOutGoalMessage = true;
	}
	
	private IEnumerator hideReturnMessage(){
		yield return new WaitForSeconds(MessageDisplayTime);
		fadeOutReturnMessage = true;
	}

	public void pickup(){
		//Debug.Log ("Pick Up");
		if (Selected != -1) // || AllSelectableObjects[Selected].tag == "hankerchief"
			return;
		
		if(Highlighted != -1 && AllSelectableObjects[Highlighted].tag == "hankerchief"){
			if (Sounds [ItemSelectIndex].isPlaying) {
				Sounds[ItemSelectIndex].Stop();
			}
			Sounds[ItemSelectIndex].Play ();
			// EFM
			pickedUpHankerchief = true;
			fadeOutReturnMessage = false;
			StartCoroutine(hideReturnMessage());
			Selected = Highlighted;
			HankerchiefScript.setHighlight(true);
			AllSelectableObjects[Selected].SetActive(false);
		}else{
			if (Sounds[ItemSelectFailIndex].isPlaying) {
				Sounds[ItemSelectFailIndex].Stop();
			}
			Sounds[ItemSelectFailIndex].volume = 1;
			Sounds[ItemSelectFailIndex].Play();
		}
	}
	
	public void TimeTravel(float value){
		sliderMovement = true;

		// Handle time travel related values
		if (value == Past) {
			if(lastValue != Past){
				StartCoroutine (MessageReliever (RelieverRelease));
			}
		} else if (value == Present) {
			if(lastValue != Present){
				StartCoroutine(MessageReliever(RelieverRed));
			}
		}else{
			if(lastValue == Past || lastValue == Present){
				//StartCoroutine(MessageReliever(RelieverTimeTravel));
			}
		}
		
		oldValue = lastValue;
		lastValue = value;
	}
	
	public void setHighlighted(GameObject[] objs){
		int closest = -1;
		for (int i = 0; i < objs.Length; i++) {
			if (objs[i].activeSelf && objs[i].GetComponent<Renderer> ().IsVisibleFrom (Camera.main)) {
				Vector3 pos = Camera.main.WorldToScreenPoint(objs[i].transform.position);
				//Debug.Log (i + " is visible at x: " + pos.x + " y: " + pos.y);
				if(closest == -1 || Vector3.Distance(new Vector3(pos.x, pos.y, pos.z), new Vector3(Screen.width/2, Screen.height/2, Camera.main.nearClipPlane)) < Vector3.Distance(new Vector3(Camera.main.WorldToScreenPoint(objs[closest].transform.position).x, Camera.main.WorldToScreenPoint(objs[closest].transform.position).y, Camera.main.WorldToScreenPoint(objs[closest].transform.position).z), new Vector3(Screen.width/2, Screen.height/2, Camera.main.nearClipPlane))){
					//Debug.Log (i + " is visible at x: " + pos.x + " y: " + pos.y);
					closest = i;
				}
			}
		}
		//Debug.Log ("Closest = " + closest);
		if (Highlighted == -1) {
			if(closest != -1){
				AllGhostlyScripts[closest].Moving = false;
			}
			Highlighted = closest;
		} else{
			if(Highlighted != closest){
				AllGhostlyScripts[Highlighted].Moving = true;
			}
			if(closest != -1){
				AllGhostlyScripts[closest].Moving = false;
			}
			Highlighted = closest;
		}
	}
	
	void setAlpha(GameObject obj, float a){
		Color old = obj.GetComponent<Renderer>().material.color;
		obj.GetComponent<Renderer>().material.color = new Color (old.r, old.g, old.b, a);
	}
	
	float getAlpha(GameObject obj){
		return obj.GetComponent<Renderer>().material.color.a;
	}
	
	IEnumerator SetupClara()
	{
		yield return new WaitForSeconds(ClaraWaitTime);
		
		ClaraGhost.SetActive(true);
	}
	
	IEnumerator MessageReliever(string command){
		for(int i = 0; i < 4; i++){
			jo.Call("writeString", "baka", command);
			yield return new WaitForSeconds(.01f);
		}
	}
}