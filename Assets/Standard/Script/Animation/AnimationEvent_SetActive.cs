using UnityEngine;
using System.Collections;
/// <summary>
/// アクティブのオンオフ
/// </summary>
public class AnimationEvent_SetActive : MonoBehaviour {
	public void Event_SetActiveTrue() {
		gameObject.SetActive(true);
	}
	public void Event_SetActiveFalse() {
		gameObject.SetActive(false);
	}
}