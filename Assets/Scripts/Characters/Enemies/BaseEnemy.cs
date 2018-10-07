using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEnemy : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
    public virtual void OnPlayerTouch(PlayerScript player) { }
    public virtual void OnAttacked(PlayerScript player) { }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerHurtBox>() != null)
        {
            if (other.GetComponent<PlayerHurtBox>().enabled)
            OnPlayerTouch(other.GetComponent<PlayerHurtBox>().playerReference);
        }

        if (other.GetComponent<PlayerAttackBox>() != null)
        {
            if (other.GetComponent<PlayerAttackBox>().enabled)
                OnAttacked(other.GetComponent<PlayerAttackBox>().playerReference);
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.GetComponent<PlayerHurtBox>() != null)
        {
            if (other.GetComponent<PlayerHurtBox>().enabled)
                OnPlayerTouch(other.GetComponent<PlayerHurtBox>().playerReference);
        }

        if (other.GetComponent<PlayerAttackBox>() != null)
        {
            if (other.GetComponent<PlayerAttackBox>().enabled)
                OnAttacked(other.GetComponent<PlayerAttackBox>().playerReference);
        }
    }


}
