using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PointerController : MonoBehaviour {

	private RectTransform rTrans;
	private Image image;

	// Use this for initialization
	void Start () {
		rTrans = GetComponent<RectTransform> (); 
		image = GetComponent<Image> ();
		image.enabled = false;

		
	}
	public void SetAngle(float angle){
		angle = - angle;
		if (!float.IsNaN (angle)) {
			image.enabled = true;
			rTrans.eulerAngles = new Vector3 (0.0f, 0.0f, angle);
		} else {
			image.enabled = false;
			
		}
	}

}
