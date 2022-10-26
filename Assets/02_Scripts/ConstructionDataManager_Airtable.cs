using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace shop
{
    public class ConstructionDataManager_Airtable : ConstructionDataManager
    {
        public override void RunConstructionDataSetupSequence(GameObject modelRoot)
        {
            IsConstructionDataLoaded = false;
            ConstructionProgressesByKey = new Dictionary<string, ConstructionProgress<string>>();
            KeyToHeight = new Dictionary<string, KeyGameObjectToHeightPair>();
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

                SetStatusModelwConstructionData(listConstructionData, modelRoot);
            }

            
        }

    }

}
