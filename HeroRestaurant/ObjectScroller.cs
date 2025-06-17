using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ObjectScroller : MonoBehaviour {
    [SerializeField]
    private Transform   respawnPoint   = null;
    [SerializeField]
    private Transform   endPoint       = null;
    [SerializeField]
    private Transform[] pallaxTargets  = null;
    [SerializeField]
    private float       scrollingSpeed = 1f;

    private void Update()
    {
        foreach (var pallaxTarget in pallaxTargets)
        {
            pallaxTarget.position = Vector3.MoveTowards(pallaxTarget.position, endPoint.position, Time.smoothDeltaTime * scrollingSpeed);
            if (pallaxTarget.position == endPoint.position)
                pallaxTarget.position = respawnPoint.position;
        }
    }
}
