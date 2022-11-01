using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ConstructionDataManager : MonoBehaviour
{

    public Material TransparentMaterial;
    public Material FadeMaterial;

    //Here we define the markers for built/unbuilt statuses
    public readonly static string IsDone = "1";
    public readonly static string NotDone = "0";
    public readonly static string WIP = "WIP";

    public readonly static string Yes = "Yes";
    public readonly static string No = "No";



    // Dictionary utilized by construction status text to track completion percentages
    public Dictionary<string, ConstructionProgress<string>> ConstructionProgressesByKey;

    public Dictionary<GameObject, Material> MaterialByGameObject = new Dictionary<GameObject, Material>();

    private void Start()
    {
        RunConstructionDataSetupSequence();
    }

    public void OnRefreshButtonClick()
    {
        RunConstructionDataSetupSequence();
    }

    public void RunConstructionDataSetupSequence()
    {
        ConstructionProgressesByKey = new Dictionary<string, ConstructionProgress<string>>();
        string airtableURL = "https://api.airtable.com/v0/appMuJjSj7yyKrKAN/Construction%20Status?api_key=keyBjwantiCrlOnYv";
        StartCoroutine(GetRequest_ConstructionData(airtableURL));
    }

    IEnumerator GetRequest_ConstructionData(string uri)
    {
        bool moreThanOneRequest = false;
        string nextURI = uri;
        do
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(nextURI))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                string resultObject = "";

                string[] pages = uri.Split('/');
                int page = pages.Length - 1;

                if (webRequest.result == UnityWebRequest.Result.ConnectionError || 
                    webRequest.result == UnityWebRequest.Result.ProtocolError || 
                    webRequest.result == UnityWebRequest.Result.DataProcessingError)
                {
                    Debug.Log(pages[page] + ": Error: " + webRequest.error);
                }
                else
                {
                    resultObject = webRequest.downloadHandler.text; //results as string

                    // Debug.Log(pages[page] + ":\nReceived: " + resultObject);

                    // no need to get this deserialized..
                    var response = JsonUtility.FromJson<AirTable_ResponseClasses_ConstructionData>(resultObject);

                    //making sure the max number of records (by default 100 with airtable) hasn't been reached
                    if (string.IsNullOrEmpty(response.offset))
                    {
                        moreThanOneRequest = false;
                    }
                    else
                    {
                        moreThanOneRequest = true;
                        nextURI = $"{uri}&offset={response.offset}";
                    }

                    var records = response.records;
                    //Debug.Log("Received Airtable Data");
                    SetStatusModelwConstructionData(records);
                }
            }
        }
        while (moreThanOneRequest);

        //Calling construction text management
        GetComponentInChildren<ConstructionStatusTextManager>().SetupCompletionTexts();
    }

    /// <summary>
    /// Given the root to the building model and construction data record, set the unbuilt slabs to transparent and turns on construction status text for built elements
    /// </summary>
    /// <param name="record"></param>
    /// <param name="modelRoot"></param>
    private void SetStatusModelwConstructionData(AirTable_Record_ConstructionData[] record)
    {
        var buildings = GameObject.FindWithTag("Buildings");

        if (record == null || record.Length == 0)
            return;
            
        foreach (AirTable_Record_ConstructionData r in record)
        {
            if (r.fields == null)
                continue;

            var data = r.fields;
                
            string elementLegendKey = data.Key;
                

            //1. FINDING the right game object======================
            GameObject elementObj;

            string elementKey = $"{data.Section}/{data.Element}/{data.Floor}";

            elementObj = buildings.transform.Find(elementKey)?.gameObject;

            if (elementObj == null)
                continue;

            //2. MAKING sure the mesh is turned on first======================
            foreach (Transform child in elementObj.transform)
                child.gameObject.SetActive(true);

            // if WIP, then see if that's the highest in point
            // if no WIP, grab the highest of IsDone

            //3. SETTING the appearance of the building element======================
            if (data.Progress == NotDone)
            {
                if (data.ShowUnbuilt == Yes)
                {
                    Material mat = TransparentMaterial;

                    var renderers = elementObj.transform.GetComponentsInChildren<MeshRenderer>();

                    //make sure to record the original material before changing it
                    if (!MaterialByGameObject.ContainsKey(elementObj))
                        MaterialByGameObject.Add(elementObj, renderers[0].material);

                    foreach (MeshRenderer renderer in renderers)
                    {
                        renderer.material = mat;
                    }
                }
                else
                {
                    foreach (Transform child in elementObj.transform)
                        child.gameObject.SetActive(false);
                }
                if (elementObj.GetComponent<WIPStatusAnimation>() != null)
                    Destroy(elementObj.GetComponent<WIPStatusAnimation>());
            }
            else if (data.Progress == IsDone)
            {
                Material mat = null;
                if (MaterialByGameObject.ContainsKey(elementObj))
                {
                    mat = MaterialByGameObject[elementObj];

                    var renderers = elementObj.transform.GetComponentsInChildren<MeshRenderer>();
                    foreach (MeshRenderer renderer in renderers)
                    {
                        renderer.material = mat;
                    }
                }
                if (elementObj.GetComponent<WIPStatusAnimation>() != null)
                    Destroy(elementObj.GetComponent<WIPStatusAnimation>());
            }
            else //if WIP
            {

                Material mat = FadeMaterial;

                var renderers = elementObj.transform.GetComponentsInChildren<MeshRenderer>();

                //make sure to record the original material before changing it
                if (!MaterialByGameObject.ContainsKey(elementObj))
                    MaterialByGameObject.Add(elementObj, renderers[0].material);

                foreach (MeshRenderer renderer in renderers)
                {
                    renderer.material = mat;
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }



                if (elementObj.GetComponent<WIPStatusAnimation>() == null)
                    elementObj.AddComponent<WIPStatusAnimation>();
            }

            //4. UPDATING construction for ConstructionStatusTextManager to consume to show percent completion======================
            if (string.IsNullOrEmpty(elementLegendKey))
                return;
            if (!ConstructionProgressesByKey.ContainsKey(elementLegendKey))
                ConstructionProgressesByKey.Add(elementLegendKey, new ConstructionProgress<string>(IsDone, NotDone));
            ConstructionProgressesByKey[elementLegendKey].AddToData(data.Progress);
        }
            

            
    }




        
}

/// <summary>
/// A class that keeps a record of construction status and calculates percentage done based on defined markers isDone and notDone of type T
/// for example, currently we use isDone = "1", notDone = "0"  of type string
/// </summary>
/// <typeparam name="T"></typeparam>
public class ConstructionProgress<T>
{
    private List<T> _Data;
    private T _IsDone;
    private T _NotDone;
    private double _CompletionPercentage;
    private int _NumFinished = -1;
    private int _NumTotal = -1;


    public double CompletionPercentage
    {
        get
        {
            CalculateCompletionPercentage();
            return _CompletionPercentage;
        }
    }

    public ConstructionProgress(T isDone, T notDone)
    {
        _Data = new List<T>();
        _IsDone = isDone;
        _NotDone = notDone;
    }

    public void AddToData(T entry)
    {
        _Data.Add(entry);
    }

    public void CalculateCompletionPercentage()
    {
        int done = 0;
        int total = 0;
        foreach (var entry in _Data)
        {
            if (entry.Equals(_IsDone))
                done++;
            total++;
        }
        _NumFinished = done;
        _NumTotal = total;
        _CompletionPercentage = (double)done / _Data.Count * 100;
    }

    public int NumFinished
    {
        get
        {
            if (_NumFinished == -1)
                CalculateCompletionPercentage();
            return _NumFinished;
        }
    }

    public int NumTotal
    {
        get
        {
            if (_NumTotal == -1)
                CalculateCompletionPercentage();
            return _NumTotal;
        }
    }
}


