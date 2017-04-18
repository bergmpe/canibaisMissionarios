using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatScript : MonoBehaviour {



	void OnTriggerEnter2D(Collider2D other){
		if(other.CompareTag("leftMargin")){
			Debug.Log ("colidiu left");
		}
		else if(other.CompareTag("rightMargin"))
		Debug.Log ("colidiu right");
	}
}
