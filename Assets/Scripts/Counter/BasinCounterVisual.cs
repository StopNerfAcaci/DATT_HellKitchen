using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasinCounterVisual : MonoBehaviour
{
    [SerializeField] private BasinCounter basinCounter;
    [SerializeField] private Transform counterPointTop;
    [SerializeField] private Transform cleanCounterPointTop;
    [SerializeField] private Transform plateVisualPrefab;

    private List<GameObject> dirtyPlateVisualGOList;
    private List<GameObject> cleanPlateVisualGOList;
    private void Awake()
    {
        dirtyPlateVisualGOList = new List<GameObject>();
        cleanPlateVisualGOList = new List<GameObject> { };
    }
    private void Start()
    {
        basinCounter.OnPlateAddedToSink += BasinCounter_OnPlateAddedToSink;
        basinCounter.OnPlateAddedToClean += BasinCounter_OnPlateAddedToClean;
        basinCounter.OnPlateRemoved += BasinCounter_OnPlateRemoved;
    }


    private void BasinCounter_OnPlateAddedToClean(object sender, EventArgs e)
    {
        Debug.Log("Add to clean");
        Transform platVisualTransform = Instantiate(plateVisualPrefab, cleanCounterPointTop);
        float plateOffsetY = .1f;
        platVisualTransform.localPosition = new Vector3(0, plateOffsetY * cleanPlateVisualGOList.Count, 0);
        cleanPlateVisualGOList.Add(platVisualTransform.gameObject);
        GameObject plateGO = dirtyPlateVisualGOList[dirtyPlateVisualGOList.Count - 1];
        dirtyPlateVisualGOList.Remove(plateGO);
        Destroy(plateGO);

    }

    private void BasinCounter_OnPlateAddedToSink(object sender, EventArgs e)
    {
        Transform platVisualTransform = Instantiate(plateVisualPrefab, counterPointTop);
        platVisualTransform.gameObject.GetComponent<PlateKitchenObject>().isDirty = true;
        float plateOffsetY = .1f;
        float plateOffsetX = .1f;
        platVisualTransform.localPosition = new Vector3(plateOffsetX * dirtyPlateVisualGOList.Count, plateOffsetY * dirtyPlateVisualGOList.Count, 0);
        platVisualTransform.localEulerAngles = new Vector3(0, 0, -30);
        dirtyPlateVisualGOList.Add(platVisualTransform.gameObject);
    }
    private void BasinCounter_OnPlateRemoved(object sender, EventArgs e)
    {
        Debug.Log("Get clean dish");
        GameObject plateGO = cleanPlateVisualGOList[cleanPlateVisualGOList.Count - 1];
        cleanPlateVisualGOList.Remove(plateGO);
        Destroy(plateGO);
    }

}
