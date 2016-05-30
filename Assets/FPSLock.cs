using UnityEngine;
using System.Collections;

public class FPSLock : MonoBehaviour {

    public int lockValue = 60;

    void Awake()
    {
        Application.targetFrameRate = lockValue;
    }
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
