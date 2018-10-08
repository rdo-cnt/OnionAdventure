using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIPauseTab : MonoBehaviour {

    public string tabName;  
    public GameObject firstSelectedObject;
    public EventSystem es;

    protected virtual void OnEnable()
    {
        if (firstSelectedObject != null)
        {
            es = GameObject.Find("EventSystem").GetComponent<EventSystem>();
            firstSelectedObject.GetComponent<Button>().Select();
            firstSelectedObject.GetComponent<Button>().OnSelect(new BaseEventData(EventSystem.current));
            es.SetSelectedGameObject(null);
            es.SetSelectedGameObject(firstSelectedObject);
        }
    }
}
