using UnityEngine;
using System.Collections;

//あたり判定イベントを伝える
public class CollisionReciever : MonoBehaviour {

	[Header("イベント関連")]
	public GameObject iventReciever;	//受け手
	public bool flagCollision;
	public bool flagTrigger;

	protected void OnCollisionEnter(Collision c) {
		if (flagCollision) {
			if (iventReciever) {
				iventReciever.SendMessage("OnCollisionEnter", c);
			}
		}
	}

	protected void OnTriggerEnter(Collider c) {
		if (flagTrigger) {
			if (iventReciever) {
				iventReciever.SendMessage("OnTriggerEnter", c);
			}
		}
	}
}