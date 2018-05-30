using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SphereController : MonoBehaviour {

	protected AudioSource source;
	protected Rigidbody rb;
	protected PointerController pc;

    private GameController gc;
	private Listener listener;

    // Stuff for control

	public float force;
	private float lastNote;


	// Use this for initialization
	void Start () {

        gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
	    pc = GameObject.Find ("pointer").GetComponent<PointerController> ();
		rb = GetComponent<Rigidbody> ();
		listener = GameObject.Find ("Listener").GetComponent<Listener> ();
	}




	void Update () {
		float newNote = listener.getNote ();
		if (!float.IsNaN (newNote)) {
			lastNote = newNote;
		}
		float note = lastNote;
		pc.SetAngle (newNote * 30);
		float theta = Mathf.Deg2Rad * note * 30;
		float z = force * Mathf.Cos (theta);
		float x = force * Mathf.Sin (theta);
		Vector3 direction = new Vector3 (x, 0.0f, z);
		rb.velocity = direction;

	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy")
        {
            gc.PlayerDied();

        }
    }

}
