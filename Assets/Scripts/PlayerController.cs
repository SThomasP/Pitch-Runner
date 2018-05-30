using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// control script for constant movement and player direction change only 

public class PlayerController : MonoBehaviour {

	protected AudioSource source;
	protected Rigidbody rb;
	protected PointerController pc;

    private GameController gc;
    private float lastX, lastZ;

    // Stuff for control
    public float moveSpeed = 0.01f;
    private bool moveUpwards = true;
    private bool moveLeft = true;

	public float analysisGap;
	public float force;
	public float threshold;

	private float timePassed = 0;

	protected int osr;

	protected int sampleCount = 256;

    void Start()
    {

        gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();

        lastX = 0;
        lastZ = 0;

        Application.RequestUserAuthorization(UserAuthorization.Microphone);
        if (Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            if (Microphone.devices.Length < 1)
            {
                Application.Quit();
            }
            osr = AudioSettings.outputSampleRate;
            rb = GetComponent<Rigidbody>();
            source = GetComponent<AudioSource>();
            pc = GameObject.Find("pointer").GetComponent<PointerController>();
            source.clip = Microphone.Start(Microphone.devices[0], true, 10, 48000);
            Debug.Log(Microphone.devices[0]);
        }
        else
        {
            Application.Quit();
        }
    }


    protected float getNote()
    {
        float[] _spectrum = new float[sampleCount];
        source.GetSpectrumData(_spectrum, 0, FFTWindow.BlackmanHarris);
        float maxV = 0;
        var maxN = 0;
        for (int i = 0; i < sampleCount; i++)
        { // find max 
            if (!(_spectrum[i] > maxV))
                continue;
            maxV = _spectrum[i];
            maxN = i; // maxN is the index of max
        }
        float note;
        if (maxV > threshold)
        {
            float freqN = maxN; // pass the index to a float variable
            if (maxN > 0 && maxN < sampleCount - 1)
            { // interpolate index using neighbours
                var dL = _spectrum[maxN - 1] / _spectrum[maxN];
                var dR = _spectrum[maxN + 1] / _spectrum[maxN];
                freqN += 0.5f * (dR * dR - dL * dL);
            }
            note = freqN * (osr / 2) / sampleCount; // convert index to frequency
            note = Mathf.Pow(note / 220.0f, 12.0f);
            note = Mathf.Log(note, 2.0f);  // convert frequnecy to note
            note = Mathf.Round(note); // round to the nearest whole note
            note = (note % 12.0f + 12.0f) % 12.0f; //make modular
            note = (note + 9) % 12; //set 0 to C, rather than A
        }
        else
        {
            note = float.NaN; //if there's no note over the threshold, we return NaN
        }
        return note;
    }


    void Update()
    {



        if (!source.isPlaying)
        {
            source.Play();
        }
        timePassed += Time.deltaTime;
        if (timePassed >= analysisGap)
        {
            float note = getNote();
            Vector3 direction = new Vector3(0.0f, 0.0f, 0.0f);
            float angle = note * 30;
            pc.SetAngle(angle);
            if (!float.IsNaN(note))
            {
                Debug.Log(note);
                float theta = Mathf.Deg2Rad * angle;
                float z = force * Mathf.Cos(theta);

                moveUpwards = (z > lastZ);
                

                float x = force * Mathf.Sin(theta);
                moveLeft = (x < 0);

                direction = new Vector3(x, 0.0f, z);
                Debug.Log(direction);
            }

            

            if (moveUpwards & moveLeft)
            {
                transform.Translate(new Vector3(moveSpeed, 0, moveSpeed));
            }
            else if (moveUpwards & !moveLeft)
            {
                transform.Translate(new Vector3(-moveSpeed, 0, moveSpeed));
            }
            else if (!moveUpwards & moveLeft)
            {
                transform.Translate(new Vector3(moveSpeed, 0, -moveSpeed));
            }
            else if (!moveUpwards & !moveLeft)
            {
                transform.Translate(new Vector3(-moveSpeed, 0, -moveSpeed));
            }
            //rb.velocity = direction;

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy")
        {
            gc.PlayerDied();

        }
    }

}
