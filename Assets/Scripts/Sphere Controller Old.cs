using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SphereControllerOld : MonoBehaviour {

	public enum MicState {Off, Recording, Playback};

	protected MicState mic = MicState.Off;
	protected AudioSource source;
	protected NavMeshAgent nma;
	protected GameObject recordingUI;

	public enum ConfigStage {Loud, Silence, High, Low, Done};

	public ConfigStage config;

	protected float pitch;
	protected float volume;
	protected int frameCounter = 0;

	public float minPitch;
	public float maxPitch;
	public float minVol;
	public float maxVol;

	public float minX;
	public float maxX;
	public float minZ;
	public float maxZ;


	public int recordingTime;
	protected float timeRecorded = 0;

	protected int osr;

	protected int sampleCount = 256;


	// Use this for initialization
	void Start () {
		if (Microphone.devices.Length < 1) {
			Application.Quit ();
		}
		osr = AudioSettings.outputSampleRate;
		nma = GetComponent<NavMeshAgent> ();
		source = GetComponent<AudioSource> ();
		Debug.Log (Microphone.devices [0]);
		recordingUI = GameObject.FindGameObjectWithTag ("Recording");
		recordingUI.SetActive (false); 
	}


	protected void AnalyseAudio(out float volume, out float pitch)
	{
		float[] _samples = new float[sampleCount];
		source.GetOutputData(_samples, 0); // fill array with samples
		int i;
		float sum = 0;
		for (i = 0; i < sampleCount; i++)
		{
			sum += _samples[i] * _samples[i]; // sum squared samples
		}
		float volumeRMS = Mathf.Sqrt(sum / sampleCount) * 10000; // rms = square root of average
		volume = 20 * Mathf.Log10(volumeRMS / 1.0f); // calculate dB
		volume = Mathf.Max(volume, -160.0f);
		// get sound spectrum
		float[] _spectrum = new float[sampleCount];
		source.GetSpectrumData(_spectrum, 0, FFTWindow.BlackmanHarris);
		float maxV = 0;
		var maxN = 0;
		for (i = 0; i < sampleCount; i++)
		{ // find max 
			if (!(_spectrum[i] > maxV))
				continue;
			maxV = _spectrum[i];
			maxN = i; // maxN is the index of max
		}
		float freqN = maxN; // pass the index to a float variable
		if (maxN > 0 && maxN < sampleCount - 1)
		{ // interpolate index using neighbours
			var dL = _spectrum[maxN - 1] / _spectrum[maxN];
			var dR = _spectrum[maxN + 1] / _spectrum[maxN];
			freqN += 0.5f * (dR * dR - dL * dL);
		}
		pitch = freqN * (osr / 2) / sampleCount; // convert index to frequency
	}

	protected Vector3 getPosition(float pitch, float volume){
		float xPos = (pitch - minPitch) / (maxPitch - minPitch) * (maxX - minX) + minX;
		float zPos = (volume - minVol) / (maxVol - minVol) * (maxZ - minZ) + minZ;
		xPos = Mathf.Clamp (xPos, minX, maxX);
		zPos = Mathf.Clamp(zPos, minZ, maxZ);
		Debug.Log ("(" + xPos + ",0," + zPos + ")");
		return  new Vector3(xPos, 0.0f, zPos);	
	}

	protected void startRecording(){
		Debug.Log ("Started Recording");
		mic = MicState.Recording;
		recordingUI.SetActive (true);
		source.clip = Microphone.Start (Microphone.devices [0], true,recordingTime, osr);
		timeRecorded = 0.0f;
	}

	protected void stopRecording(){
		Debug.Log ("Stopped Recording");
		mic = MicState.Playback; 
		recordingUI.SetActive (false);
		pitch = 0.0f;
		volume = 0.0f; 
		frameCounter = 0; 
		source.Play ();
	}

	protected void moveTo(Vector3 pos){
		nma.destination = pos;
	}

	protected void move (){
		float x = (minX + maxX) / 2;
		float z = (minZ + maxZ) / 2;
		switch (config) {
			case ConfigStage.Loud:
				maxVol = volume;
				moveTo (new Vector3 (x, 0.0f, maxZ)); 
				config = ConfigStage.Silence;
				break;
			case ConfigStage.Silence:
				minVol = volume;
				moveTo (new Vector3 (x, 0.0f, minZ));
				config = ConfigStage.High;
				break;
			case ConfigStage.High:
				maxPitch = pitch;
				moveTo (new Vector3 (minX, 0.0f, z));
				config = ConfigStage.Low;
				break;
			case ConfigStage.Low:
				minPitch = pitch;
				moveTo (new Vector3 (maxX, 0.0f, z));
				config = ConfigStage.Done;
				break;
			default:
			case ConfigStage.Done:
				moveTo (getPosition (pitch, volume));
				minPitch = Mathf.Min (pitch, minPitch);
				maxPitch = Mathf.Max (pitch, maxPitch);
				minVol = Mathf.Min (volume, minVol);
				maxVol = Mathf.Max (volume, maxVol);
				break;
		}
	}

	void Update () {
		if (Input.GetKeyDown (KeyCode.R) && mic == MicState.Off) {
			startRecording ();
		} else if (mic == MicState.Recording) {
			timeRecorded += Time.deltaTime; 
			if (timeRecorded >= recordingTime) {
				stopRecording ();
			}
		}
		if (mic == MicState.Playback) {
			if (source.isPlaying) {
				float updateVol = 0;
				float updatePitch = 0;
				AnalyseAudio (out updateVol, out updatePitch);
				volume += updateVol;
				pitch += updatePitch;
				frameCounter++;
			} else {
				volume = volume / frameCounter;
				pitch = pitch / frameCounter;
				Debug.Log ("volume: "+volume+", pitch: "+pitch);
				mic = MicState.Off;
				move ();
			}
		}

	}
}
