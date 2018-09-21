using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.GetComponent<Destructor>()!=null && other.gameObject.GetComponent<Destructor>().enabled)
        {
            Destroy(gameObject);
        }

    }

    void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.GetComponent<Destructor>() != null && other.gameObject.GetComponent<Destructor>().enabled)
        {
            Destroy(gameObject);
        }

    }
}


    
