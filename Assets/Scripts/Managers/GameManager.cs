using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    //Crear singleton para que otros objetos tengan acceso
    private static GameManager _instance = null;
    public static GameManager instance { get { return _instance; } }

    //Hacer que cualquier objeto pueda acceder al jugador desde aqui
    public PlayerScript _player;
    public PlayerScript player { get { return _player; } }

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

    // Update is called once per frame
    void Update () {
		
	}
}
