using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throwable : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    public virtual void OnPlayerTouch(PlayerScript player) { }
    public virtual void OnAttacked(PlayerScript player) { }
    public virtual void OnThrown(PlayerScript player) { }
    public virtual void OnThrownUpwards(PlayerScript player) { }
    public virtual void OnReleased(PlayerScript player) { }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerPickUpBox>() != null)
        {
            if (other.GetComponent<PlayerPickUpBox>().enabled)
                OnPlayerTouch(other.GetComponent<PlayerPickUpBox>().playerReference);
        }

        if (other.GetComponent<PlayerAttackBox>() != null)
        {
            if (other.GetComponent<PlayerAttackBox>().enabled)
                OnAttacked(other.GetComponent<PlayerAttackBox>().playerReference);
        }

    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.GetComponent<PlayerPickUpBox>() != null)
        {
            if (other.GetComponent<PlayerPickUpBox>().enabled)
                OnPlayerTouch(other.GetComponent<PlayerPickUpBox>().playerReference);
        }

        if (other.GetComponent<PlayerAttackBox>() != null)
        {
            if (other.GetComponent<PlayerAttackBox>().enabled)
                OnAttacked(other.GetComponent<PlayerAttackBox>().playerReference);
        }
    }
}
