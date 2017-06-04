using UnityEngine;
using System.Collections;

//追尾カメラ
public class Aiming : MonoBehaviour {

	public GameObject target;
	public Vector3 offset;

	protected void Update() {
		if (target) {
			transform.position = target.transform.position + offset;
		}
	}

	public void SetTarget(GameObject target) {
		this.target = target;
		offset = transform.position - target.transform.position;
	}

	public void SetTarget(GameObject target, Vector3 offset) {
		this.target = target;
		this.offset = offset;
	}
}