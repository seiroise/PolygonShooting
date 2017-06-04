using UnityEngine;
using System.Collections;

//削除アニメーションイベント
public class AnimationEvent_Destroy : MonoBehaviour {
	
	public GameObject destroyObject;

	public void Event_Destroy() {
		Destroy(destroyObject);
	}
}