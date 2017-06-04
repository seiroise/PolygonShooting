using UnityEngine;
using System.Collections;

//メニュー画面のGUI管理
public class MenuGUIManager : SingletonMonoBehaviour<MenuGUIManager> {
	protected GameManager gm;
#region MonoBehaviourイベント
	protected override void Awake() {
		base.Awake();
	}
	protected void Start() {
		gm = GameManager.Instance;
		gm.PlayMenuBGM();
	}
	protected void Update() {
		if(Input.GetKeyDown(KeyCode.C)) {
			FadeManager.Instance.LoadLevel("Config");
		}
	}
#endregion
#region UIイベント
	//プレイ
	protected void GameButtonClicked() {
		gm.LoadLevel("GameMenu");
	}
	//エディター
	protected void EditorButtonClicked() {
		gm.LoadLevel("ShipEditorMenu");
	}
	//コンフィグ
	protected void ConfigButtonClicked() {
		gm.LoadLevel("Title");
	}
	//タイトル
	protected void TitleButtonClicked() {
		gm.LoadLevel("Title");
	}
#endregion
}