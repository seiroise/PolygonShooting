using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//UIButtonMessageを管理する
public class ButtonMessageManager : MonoBehaviour {

	public List<UIButtonMessage> buttonMessage;

	public void SetTargetObjet(GameObject target) {
		foreach (UIButtonMessage m in buttonMessage) {
			m.target = target;
		}
	}
}
