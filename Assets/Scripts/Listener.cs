
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Listener : MonoBehaviour {

	public string mic;
	public float threshold = 0.1f;
	public bool active = false;
	public float analysisFrequency = 0.25f;
	private float note = float.NaN;

	private int sampleCount = 256;

	private AudioSource source;
	private float osr;
	private float timePassed = 0.0f;

    private void Awake()
    {

        DontDestroyOnLoad(this);
        osr = AudioSettings.outputSampleRate;
        source = GetComponent<AudioSource>();
        Debug.Log(source);
        if (!string.IsNullOrEmpty(mic))
        {
            changeMic();
        }
    }

	public void disable(){
		active = false;
        source.Stop();
        source.clip = null;
        Debug.Log ("Disabled");
        Microphone.End(Microphone.devices[0]);
        Debug.Log(Microphone.IsRecording(Microphone.devices[0]));
		
		
	}

	public void changeMic(){
		this.mic = Microphone.devices[0];
		this.enabled = true;
        source.clip = Microphone.Start(Microphone.devices[0], true, 10, 44100);
        Debug.Log ("Started Recording");
        source.loop = true;
        while (!(Microphone.GetPosition(null) > 0)) { }
		source.Play ();
	}

	public float getNote(){
		return note;
	}

	private void AnalyseAudio(){
		float threshold = this.threshold;
		float[] _spectrum = new float[sampleCount];
		source.GetSpectrumData(_spectrum, 0, FFTWindow.BlackmanHarris);
		float maxV = 0.0f;
		var maxN = 0;
		for (int i = 0; i < sampleCount; i++)
		{ // find max 
			if (!(_spectrum[i] > maxV))
				continue;
			maxV = _spectrum[i];
			maxN = i; // maxN is the index of max
		}
		float note;
		if (maxV > threshold) {
			float freqN = maxN; // pass the index to a float variable
			if (maxN > 0 && maxN < sampleCount - 1) { // interpolate index using neighbours
				var dL = _spectrum [maxN - 1] / _spectrum [maxN];
				var dR = _spectrum [maxN + 1] / _spectrum [maxN];
				freqN += 0.5f * (dR * dR - dL * dL);
			}
			note = freqN * (osr / 2) / sampleCount; // convert index to frequency
			note = Mathf.Pow (note / 220.0f, 12.0f);
			note = Mathf.Log (note, 2.0f);  // convert frequnecy to note
			note = Mathf.Round (note); // round to the nearest whole note
			note = (note % 12.0f + 12.0f) % 12.0f; //make modular
			note = (note + 9) % 12; //set 0 to C, rather than A
		} else {
			note = float.NaN; //if there's no note over the threshold, we return NaN
		}
		this.note = note;
		
	}

    // Update is called once per frame
    void Update () {
		if (!active) {
			timePassed += Time.deltaTime;
			if (timePassed >= analysisFrequency) {
				AnalyseAudio ();
				timePassed = 0.0f;
			}
		}

	}
}
