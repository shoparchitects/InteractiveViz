using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace shop
{
    public class ConstructionDataManager_Airtable : MonoBehaviour
    {
        public void RunConstructionDataSetupSequence(GameObject modelRoot)
        {
            IsConstructionDataLoaded = false;
            //ConstructionProgressesByKey = new Dictionary<string, ConstructionProgress<string>>();
            //KeyToHeight = new Dictionary<string, KeyGameObjectToHeightPair>();
            string airtableURL = "https://api.airtable.com/v0/apph85QmsjCctxMLo/Construction%20Status?api_key=keyHPsjb9CclmOT7i";
            StartCoroutine(GetRequest_ConstructionData(airtableURL, modelRoot));
        }

        IEnumerator GetRequest_ConstructionData(string uri, GameObject modelRoot)
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

                    if (webRequest.isNetworkError)
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
                        SetStatusModelwConstructionData(records, modelRoot);
                    }
                }
            }
            while (moreThanOneRequest);

            //Making the callbacks
            IsConstructionDataLoaded = true;
            OnConstructionDataLoaded(null);
        }

        /// <summary>
        /// Given the root to the building model and construction data record, set the unbuilt slabs to transparent and turns on construction status text for built elements
        /// </summary>
        /// <param name="record"></param>
        /// <param name="modelRoot"></param>
        private void SetStatusModelwConstructionData(AirTable_Record_ConstructionData[] record, GameObject modelRoot)
        {

            
            if (record != null && record.Length != 0)
            {
                List<Fields_ConstructionData> listConstructionData = new List<Fields_ConstructionData>(record.Length);

                foreach (AirTable_Record_ConstructionData r in record)
                {
                    if (r.fields != null)
                        listConstructionData.Add(r.fields);
                }

                //SetStatusModelwConstructionData(listConstructionData, modelRoot);
            }

            
        }

        public bool IsConstructionDataLoaded = false;

        public event EventHandler ConstructionDataLoadedEvent;


        protected virtual void OnConstructionDataLoaded(EventArgs e)
        {
            EventHandler handler = ConstructionDataLoadedEvent;
            handler?.Invoke(this, e);
        }

        //Here we define the markers for built/unbuilt statuses
        public readonly static string IsDone = "1";
        public readonly static string NotDone = "0";
        public readonly static string WIP = "WIP";

        public readonly static string Yes = "Yes";
        public readonly static string No = "No";

        // Dictionary utilized by construction status text to track completion percentages
        //public Dictionary<string, ConstructionProgress<string>> ConstructionProgressesByKey;

        // Dictionary used to find the corresponding game object and pre-defined actual height based on given key
        //public Dictionary<string, KeyGameObjectToHeightPair> KeyToHeight;


        /// <summary>
        /// Common way to connect construction data from database with 3D model
        /// </summary>
        /// <param name="constructionDataList"></param>
        /// <param name="modelRoot"></param>
        /*protected void SetStatusModelwConstructionData(List<Fields_ConstructionData> constructionDataList, GameObject modelRoot)
        {

            foreach (Fields_ConstructionData data in constructionDataList)
            {
                string elementLegendKey = data.Key;
                //find the right slab for the corresponding floor
                string floorSection = "";
                if (!string.IsNullOrEmpty(data.Section))//if there is an intermediary level
                {
                    floorSection = $"{data.Floor}_{data.Section}";
                }

                //1. FINDING the right element======================
                GameObject elementObj;
                if (string.IsNullOrEmpty(floorSection))
                    elementObj = modelRoot.transform.Find($"{data.Element}/{data.Floor}")?.gameObject;
                else //if there is an intermediary level
                    elementObj = modelRoot.transform.Find($"{data.Element}/{data.Floor}/{floorSection}")?.gameObject;

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
                        Material mat = GameControl.control.StatusModelTransparentMaterial;

                        var renderers = elementObj.transform.GetComponentsInChildren<MeshRenderer>();
                        foreach (MeshRenderer renderer in renderers)
                        {
                            renderer.material = mat;
                        }
                        //elementObj.transform.GetComponentInChildren<MeshRenderer>().material = mat;
                    }
                    else
                    {
                        foreach (Transform child in elementObj.transform)
                            child.gameObject.SetActive(false);
                    }

                }
                else if (data.Progress == IsDone)
                {
                    // see if this new elevation exceeds previous heights
                    CheckAndReplaceHeight(elementLegendKey, data.Height, elementObj);

                    if (data.ShowDate == Yes)
                    {
                        ConstructionStatusTrackingText cSTtext = elementObj.GetComponent<ConstructionStatusTrackingText>();
                        if (cSTtext == null)
                            cSTtext = elementObj.AddComponent<ConstructionStatusTrackingText>();

                        cSTtext.enabled = false;
                        cSTtext.TrackedObject = elementObj.transform.GetChild(0).gameObject;
                        cSTtext.StatusText = elementObj.name + "F on " + data.Date;
                    }
                }
                else //if WIP
                {
                    // see if this new elevation exceeds previous heights
                    CheckAndReplaceHeight(elementLegendKey, data.Height, elementObj);

                    Material mat = GameControl.control.StatusModelFadeMaterial;

                    var renderers = elementObj.transform.GetComponentsInChildren<MeshRenderer>();
                    foreach (MeshRenderer renderer in renderers)
                    {
                        renderer.material = mat;
                        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    }

                    //MeshRenderer renderer = elementObj.transform.GetComponentInChildren<MeshRenderer>();
                    //renderer.material = mat;


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

        private void CheckAndReplaceHeight(string key, string height, GameObject elementObj)
        {
            if (string.IsNullOrEmpty(key))
                return;
            // making sure the height look up has an initial value
            if (!KeyToHeight.ContainsKey(key))
            {
                KeyToHeight.Add(key, new KeyGameObjectToHeightPair(elementObj, height));
            }
            else
            {
                float prevHighest = KeyToHeight[key].ModelHeight;
                float thisElementHeight = KeyGameObjectToHeightPair.CalculateModelHeight(elementObj);

                // if this element obj has a higher height then replace the prev in dictionary
                if (thisElementHeight > prevHighest)
                    KeyToHeight[key] = new KeyGameObjectToHeightPair(elementObj, height);
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


    public class KeyGameObjectToHeightPair
    {
        public GameObject KeyGameObject;
        public string Height;
        public float ModelHeight;

        public KeyGameObjectToHeightPair(GameObject keyGameObject, string height)
        {
            KeyGameObject = keyGameObject;
            Height = height;
            ModelHeight = CalculateModelHeight(keyGameObject);
        }

        public static float CalculateModelHeight(GameObject parent)
        {
            float modelHeight = float.MinValue;
            foreach (Transform child in parent.transform)
            {
                Renderer childRenderer = child.GetComponent<Renderer>();

                if (childRenderer.bounds.center.y > modelHeight)
                    modelHeight = childRenderer.bounds.center.y;
            }
            return modelHeight;
        }*/


    }

}
