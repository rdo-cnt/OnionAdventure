using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour {

	// Use this for initialization
	public void Start () {
		
	}

    public virtual void OnCollectiblePick(){}

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.GetComponent<PlayerHurtBox>() != null)
        {
            OnCollectiblePick();
        }

    }





}
