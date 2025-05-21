using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingCounters : MonoBehaviour
{

    [SerializeField] private Vector3 pointA;
    [SerializeField] private Vector3 pointB;
    [SerializeField] private float moveTimeMax = 10;
    [SerializeField] private float moveDuration = 3;
    private void Start()
    {
        StartCoroutine(StartAfterDelay());
    }
    private IEnumerator StartAfterDelay()
    {
        yield return new WaitForSeconds(moveTimeMax);
        StartCoroutine(MoveLoop());
    }
    private IEnumerator MoveLoop()
    {
        while (KitchenGameManager.Instance.IsGamePlaying())
        {
            // Move from A to B
            yield return StartCoroutine(MoveToPosition(pointA, pointB));
            yield return new WaitForSeconds(moveTimeMax);

            // Move from B to A
            yield return StartCoroutine(MoveToPosition(pointB, pointA));
            yield return new WaitForSeconds(moveTimeMax);
        }
    }
    private IEnumerator MoveToPosition(Vector3 start, Vector3 end)
    {
        float elapsedTime = 0f;
        while (elapsedTime < moveDuration)
        {
            Vector3 prevPosition = transform.position;
            transform.position = Vector3.Lerp(start, end, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = end;
    }
}
