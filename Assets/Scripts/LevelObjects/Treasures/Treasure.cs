using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Treasure : Collectible {

    public int treasureNum = 1;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public override void OnCollectiblePick()
    {
        InventoryManager.instance.addCollectible(treasureNum);
        Destroy(gameObject);
    }
}
