using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class UIInventoryTab : UIPauseTab 
{
    //Cosas que actualizar
    public UIInventorySlot[] Buttons;
    


    //Tab grande con descripcion
    public Text nombre;
    public Text tipo;
    public Image imagen;
    public Text descripcion;

    private int tempSlot;


    // Use this for initialization
    void Start () {

    }
	
	// Update is called once per frame
	void Update () {

        }

}
