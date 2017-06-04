using UnityEngine;
using System.Collections;

//回転用クラス
public class RotateBehaviour : MonoBehaviour {

	public Vector3 rotateVector;		//回転軸
	public float rotatevelocity;		//回転速度
	
	public bool flagRandom = false;	//ランダム
	public float interval = 10f;			//インターバル
	public float t = 1f;				//補間率
	public Vector3 min = Vector3.zero;
	public Vector3 max = Vector3.one;

	protected Vector3 targetAngle;	//目標角度

	protected void Start() {
		if(flagRandom) {
			targetAngle = rotateVector;
			StartCoroutine("Coroutine");
		}
	}

	protected void Update() {
		if(flagRandom) {
			rotateVector = FuncBox.Vector3Lerp(rotateVector, targetAngle, t * Time.deltaTime);
		}
		transform.Rotate(rotateVector * rotatevelocity * Time.deltaTime);
	}

	protected IEnumerator Coroutine() {
		while(true) {
			yield return new WaitForSeconds(interval);
			targetAngle = new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y), Random.Range(min.z, max.z));
		}
	}
}