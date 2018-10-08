using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInventorySlot : MonoBehaviour {

    public Image icon;
    public int slotOrder;
    public Text text;
    public int itemNumber;

	// Use this for initialization
	void Start () {
        icon.sprite = null;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
