using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class properties : MonoBehaviour {

    public Vector3 leftPosition;
    public Vector3 rightPosition;

    // Use this for initialization
	void Start () {
        var currentPosition = transform.position;
        leftPosition = currentPosition;
        rightPosition = new Vector3(currentPosition.x * -1, currentPosition.y, currentPosition.z);
	}
}
