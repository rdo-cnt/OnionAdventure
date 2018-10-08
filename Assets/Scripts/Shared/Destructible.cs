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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Destructor>()!=null ) 
        {
              if(other.GetComponent<Destructor>().enabled)
                Destroy(gameObject);
        }

    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.GetComponent<Destructor>() != null)
        {
            if (other.GetComponent<Destructor>().enabled)
                Destroy(gameObject);
        }

    }
}


    
