using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour {

    public int points;

    private GameController gc;

	// Use this for initialization
	void Start () {
        gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
		points = 1;
        //points = Random.Range(2, 5);

         

        //Vector3 scale = gameObject.transform.localScale;

        //gameObject.transform.localScale = new Vector3(points/10.0f, points / 10.0f, points / 10.0f)   


    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Player")
        {
            gc.AddScore(points);
            Destroy(gameObject);
        }
    }


}
