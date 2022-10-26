using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropdownHandler : MonoBehaviour
{
    public Dropdown dropdown; 
    public SenseSomething _senseSomething;


    // Start is called before the first frame update
    void Start()
    {
        var dropdown = this.GetComponent<Dropdown>();

        dropdown.onValueChanged.AddListener(delegate
        {
            DropdownValueChanged(dropdown);
        });
    }

    void DropdownValueChanged(Dropdown change)
    {
        Debug.Log(change.value);
        _senseSomething.PublishInterval = change.value;
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
