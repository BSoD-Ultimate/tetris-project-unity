using UnityEngine;
using System.Collections;

public class Loader : MonoBehaviour {
    public GameObject gameManager;
    void Awake()
    {
        if (GameManager.instance == null)
            Instantiate(gameManager);
    }
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
