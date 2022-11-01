using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConstructionStatusTextManager : MonoBehaviour
{
    static readonly string _ConstructionTextKeyName = "Key";
    static readonly string _ConstructionTextPercentageName = "PercentageText";

    //Setting up the percentages in here
    public void SetupCompletionTexts()
    {
        var constructionProgressesByKey = GetComponentInParent<ConstructionDataManager>().ConstructionProgressesByKey;

        var percentCompletionVLG = GameObject.FindWithTag("PercentagePanel").transform;
        GameObject prefab = percentCompletionVLG.transform.GetChild(0).gameObject;

        //Delete all except for the first one for template
        if(percentCompletionVLG.transform.childCount > 1)
        {
            for(int j = percentCompletionVLG.transform.childCount - 1; j > 0; j --)
            {
                Destroy(percentCompletionVLG.transform.GetChild(j).gameObject);
            }
        }


        int i = 0;

        foreach (string key in constructionProgressesByKey.Keys)
        {
            GameObject textContainer;
            if (i == 0)
            {
                textContainer = percentCompletionVLG.GetChild(0).gameObject;
            }
            else
            {
                textContainer = GameObject.Instantiate(prefab);
                textContainer.transform.SetParent(percentCompletionVLG);
                textContainer.GetComponent<RectTransform>().localScale = Vector3.one;
            }

            double percentage = constructionProgressesByKey[key].CompletionPercentage;
            int numFinished = constructionProgressesByKey[key].NumFinished;
            int numTotal = constructionProgressesByKey[key].NumTotal;

            Transform keyText = textContainer.transform.Find(_ConstructionTextKeyName);
            Transform percentageText = keyText.transform.GetChild(0);
            string percentageString = Math.Round(percentage) + "%";
            keyText.GetComponent<Text>().text = key;
            percentageText.GetComponent<Text>().text = percentageString;

            i++;
        }

    }
}
