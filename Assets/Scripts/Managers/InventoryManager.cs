using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour {


    //Crear singleton para que otros objetos tengan acceso
    private static InventoryManager _instance = null;
    public static InventoryManager instance { get { return _instance; } }

    public float money = 0f;
    public bool[] collectibles = new bool[4];

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void addCollectible(int collectibleNumber)
    {
        collectibles[collectibleNumber] = true;
    }

    public bool checkIfAllCollected()
    {
        bool collected = true;
        foreach(bool collectible in collectibles)
        {
            if (!collectible)
                collected = false;
        }

        return collected;

    }
}
