using UnityEngine;
using System.Collections;

//入力確認用
public class InputCheckGUIManager : SingletonMonoBehaviour<InputCheckGUIManager> {

	protected GameManager gm;

#region MonoBehaviourイベント

	protected void Start() {
		gm = GameManager.Instance;
	}

#endregion

#region UIイベント

	protected void ReturnButtonClicked() {
		//前回のシーンへ戻る
		gm.LoadPrevLevel();
	}

#endregion
}