using UnityEngine;
using System.Collections;
/// <summary>
/// SE再生アニメーションイベント
/// </summary>
public class AnimationEvent_PlaySE : MonoBehaviour {
	public void Event_PlaySE(string playSEName) {
		AudioManager.Instance.PlaySE(playSEName);
	}
}