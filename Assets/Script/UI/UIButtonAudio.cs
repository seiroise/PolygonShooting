using UnityEngine;
using System.Collections;

//GameManagerにhoverとClickのSE再生を伝える
public class UIButtonAudio : MonoBehaviour {
	protected GameManager gm;
#region UIイベント
	protected void OnHover(bool isOver) {
		if(isOver) {
			if(!gm) gm = GameManager.Instance;
			gm.PlayHoverSE();
		}
	}
	protected void OnClick() {
		if(!gm) gm = GameManager.Instance;
		gm.PlayClickSE();
	}
#endregion
}