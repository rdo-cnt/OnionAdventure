using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : BaseEnemy
{

    //Components
    protected AnimationManager animationManager;
    public Collider2D collider;

    public bool hit = false;
    public Transform spawnPoint;
    public GameObject spawnThis;
    

    // Use this for initialization
    void Start () {
        getComponentReferences();
	}

    void getComponentReferences()
    {
        collider = GetComponent<Collider2D>();
        animationManager = GetComponent<AnimationManager>();
    }

    // Update is called once per frame
    void Update () {
		
	}

    public override void OnAttacked(PlayerScript player)
    {
        if (!hit)
        {
            hit = true;
            StartCoroutine(MakeUnsolid());
            animationManager.getAnimator().SetTrigger("Hit");
            GameObject spawnedObject = Instantiate(spawnThis, transform);
            spawnedObject.transform.parent = spawnPoint;
        }
    }

    IEnumerator MakeUnsolid()
    {
        yield return new WaitForSeconds(0.1f);
        collider.enabled = false;
    }
}
