using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

    public GameObject player;
    public GameObject[] collectables;
    public Text scoreUIText, livesUIText;

    private int score = 0;
	private int maxScore;
    public int lives;
	private int maxLives;

    // Use this for initialization
    void Start()
    {
		maxLives = lives;
		maxScore = 0;
        player = GameObject.FindGameObjectWithTag("Player");
        collectables = GameObject.FindGameObjectsWithTag("Collectable");
		foreach (GameObject c in collectables) {
			maxScore += c.GetComponent<Collectable> ().points;
			Debug.Log (maxScore);
		}
        UpdateScore();
        UpdateLives();
    }

    // Update is called once per frame
    void Update () {
		if (Input.GetKeyDown(KeyCode.Escape)){
            GameOver();
        }
	}

	//move to the next level
	void NextLevel(){
		int currentLevelIndex = SceneManager.GetActiveScene ().buildIndex; 
		if (currentLevelIndex == SceneManager.sceneCountInBuildSettings - 1) {
			GameOver ();
			SceneManager.LoadScene (0, LoadSceneMode.Single);
		} else {
			int nextLevelIndex = currentLevelIndex + 1;
			SceneManager.LoadScene (nextLevelIndex, LoadSceneMode.Single);
		}
	}

    public void PlayerDied()
    {
        lives--;
        UpdateLives();
        if (lives < 1)
        {
            GameOver();
        }
        else
        {
            player.transform.position = new Vector3(0, 1, 0);
        }
    }

    public void AddScore(int scoreToAdd)
    {
        score += scoreToAdd;
        UpdateScore();
		if (score >= maxScore) {
			NextLevel ();
		}
    }

    private void UpdateLives()
    {
		livesUIText.text = ("Lives: " + lives + "/" + maxLives);
    }

    private void UpdateScore()
    {
		scoreUIText.text = ("Score: " + score + "/" +maxScore);
    }

    public void GameOver()
    {
        Destroy(player);
		SceneManager.LoadScene (0, LoadSceneMode.Single);
    }

}
