using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movemodel : MonoBehaviour
{

    public static Movemodel _Movemodel;

    void Awake()
    {
        _Movemodel = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        transform.localRotation = Quaternion.Euler(0, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void moveModel(string position){
        Debug.Log("moveModel: " + position);
        var newPosition = StringToVector3(position);
        transform.localRotation = new Quaternion(newPosition.x, newPosition.y, newPosition.z, 0);
    }

    public static Vector3 StringToVector3(string sVector)
    {
        // Remove the parentheses
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        // split the items
        string[] sArray = sVector.Split(',');

        // store as a Vector3
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

        return result;
    }
}
