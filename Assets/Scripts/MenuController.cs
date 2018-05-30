using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour {

	private string SelectedMic;
	private bool micsConnected;
	private GameObject playButton;
	private GameObject micSetup;
	private GameObject micName;
	private GameObject instructions;
	private Listener listener;
    public Object listenerPrefab;
	private PointerController pc;

	// Use this for initialization
	void Start () {
		playButton = GameObject.Find ("Play");
		micSetup = GameObject.Find ("Mic Setup");
		micName = GameObject.Find ("Mic Name");
		instructions = GameObject.Find ("Instructions");
		instructions.SetActive (false);
		GameObject listenerObject = GameObject.Find ("Listener");
        Debug.Log(listenerObject);
        if (listenerObject == null)
        {
            listenerObject = (GameObject) Instantiate(listenerPrefab);
            listenerObject.name = "Listener";
        }
        listener = listenerObject.GetComponent<Listener>();
		pc = GameObject.Find ("pointer").GetComponent<PointerController> ();
		RefreshMics ();
	}

	public void Quit(){
		Application.Quit ();
	}

	public void ThresholdChange(float something){
		Slider thresholdSlider = GetComponentInChildren<Slider> ();
		float value = thresholdSlider.value;
		listener.threshold = value / 100;
		GameObject.Find("Threshold").GetComponent<Text> ().text = "Threshold: " + value;
	}

	public void ToggleInstructions(){
		bool instructionsActive = instructions.activeSelf;
		if (!instructionsActive) {
			instructions.SetActive (true);
            micName.SetActive(false);
            micSetup.SetActive (false);
			GameObject.Find ("Rules").GetComponentInChildren<Text> ().text = "Configuration";
		} else {
			instructions.SetActive (false);
			micSetup.SetActive (micsConnected);
            micName.SetActive(true);
			GameObject.Find ("Rules").GetComponentInChildren<Text> ().text = "How To Play";
		}
	}

	public void Play(){
		SceneManager.LoadScene (1, LoadSceneMode.Single);
	}

	public void RefreshMics(){
		string[] mics = Microphone.devices;
		micsConnected = mics.Length > 0; 
		micSetup.SetActive (micsConnected);
		playButton.GetComponent<Button> ().interactable = micsConnected;
        if (micsConnected) { 
            if (!listener.mic.Equals(Microphone.devices[0])) {
                listener.changeMic();
            }
			micName.GetComponent<Text> ().text = "Mic: " + Microphone.devices [0];
		} else {
			listener.disable();
			micName.GetComponent<Text> ().text = "No Mic Detected, please connect a microphone and press refresh.";
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (micsConnected) {
			float note = listener.getNote ();
			pc.SetAngle (note * 30);
		}
	}
}
