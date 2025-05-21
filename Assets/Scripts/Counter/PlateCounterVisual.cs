using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateCounterVisual : MonoBehaviour
{
    [SerializeField] private PlatesCounter platesCounter;
    [SerializeField] private Transform counterTopPoint;
    [SerializeField] private Transform plateVisualPrefab;

    private List<GameObject> plateVisualGOList;
    private void Awake()
    {
        plateVisualGOList = new List<GameObject>();
    }
    private void Start()
    {
        platesCounter.OnPlateSpawned += PlatesCounter_OnPlateSpawned;
        platesCounter.OnPlateRemoved += PlatesCounter_OnPlateRemove;
    }

    private void PlatesCounter_OnPlateRemove(object sender, EventArgs e)
    {
        GameObject plateGO = plateVisualGOList[plateVisualGOList.Count - 1];
        plateVisualGOList.Remove(plateGO);
        Destroy(plateGO);
    }

    private void PlatesCounter_OnPlateSpawned(object sender, EventArgs e)
    {
        Transform platVisualTransform = Instantiate(plateVisualPrefab,counterTopPoint);
        float plateOffsetY = .1f;
        platVisualTransform.gameObject.GetComponent<PlateKitchenObject>().isDirty = platesCounter.isDirtyPlate;
        platVisualTransform.localPosition = new Vector3(0,plateOffsetY*plateVisualGOList.Count,0);
        plateVisualGOList.Add(platVisualTransform.gameObject);
    }
}
