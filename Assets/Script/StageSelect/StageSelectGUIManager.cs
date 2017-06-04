using UnityEngine;
using System.Collections;

//ステージ選択GUIの管理クラス
public class StageSelectGUIManager : SingletonMonoBehaviour<StageSelectGUIManager> {

	//依存クラス
	protected GameManager gm;

	//選択ステージリスト

#region MonoBehaviourイベント
	protected override void Awake() {
		base.Awake();
	}
	protected void Start() {
		gm = GameManager.Instance;
		gm.PlayMenuBGM();
	}
#endregion

#region 関数

#endregion

#region UIイベント
	protected void ReturnButtonClicked() {
		gm.LoadLevel("Menu_MultiPlay");
	}
	protected void StageButtonClicked(GameObject g) {
		gm.LoadLevel("Stage");
	}
#endregion
}