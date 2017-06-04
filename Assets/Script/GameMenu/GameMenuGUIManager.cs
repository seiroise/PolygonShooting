using UnityEngine;
using System.Collections;
/// <summary>
/// ゲームメニューのGUI管理
/// </summary>
public class GameMenuGUIManager : SingletonMonoBehaviour<GameMenuGUIManager> {
	protected GameManager gm;
	[Header("表示_Menu")]
	public GameObject menu;
	[Header("表示_Battle")]
	public GameObject battle;
	[Header("Gamepad")]
	public GamepadInput_Message gamepadInput;
	//表示スタック
	protected ClassBox.GameObjectStack indicateStack;
#region MonoBehaviourイベント
	protected override void Awake() {
		base.Awake();
	}
	protected void Start() {
		gm = GameManager.Instance;
		//gamepadInput
		gamepadInput.target = menu;
		gamepadInput.subTarget = gameObject;
		//初期化
		indicateStack = new ClassBox.GameObjectStack();
		//menuを詰めておく
		indicateStack.Push(menu);
		//非表示に
		battle.SetActive(false);
		//BGM再生
		gm.PlayMenuBGM();
	}
	protected void Update() {
		if(Input.GetMouseButtonDown(1)) Cancel();
	}
#endregion
#region 表示関数
	/// <summary>
	/// 表示されているメニューを全て閉じる
	/// </summary>
	public void HideMenu() {
		//スタックがmenuになるまでループ
		indicateStack.StackCheck(menu);
		//ゲームパッドのイベント送信先はmenuに
		gamepadInput.target = menu;
	}
	/// <summary>
	/// キャンセル処理(表示を１つ戻す)
	/// </summary>
	public void Cancel() {
		//peekしてmenuだったら何もしない
		if(menu ==indicateStack.Peek()) return;
		//１つ戻す
		indicateStack.Pop();
		//スタックに追加
		gamepadInput.target = indicateStack.Peek();
	}
#endregion
#region UIイベント
	//Button
	protected void BattleButtonClicked() {
		gm.playMode = ToolBox.PlayMode.Battle;
		gm.battleMode = ToolBox.BattleMode.Normal;
		gm.SetSelectStage("Simple", 0);
		gm.LoadLevel("ShipSelect");
		////非表示
		//HideMenu();
		////表示
		//indicateStack.Push(battle);
		//gamepadInput.target = battle;
	}
	protected void VsEnemyButtonClicked() {
		gm.playMode = ToolBox.PlayMode.VsEnemy;
		gm.SetSelectStage("Simple", 1);
		Debug.Log(gm.selectStage.name);
		gm.LoadLevel("ShipSelect");
	}
	protected void ReturnButtonClicked() {
		gm.LoadLevel("MainMenu");
	}
	//Button_Battle
	protected void Battle_NormalButtonClicked() {
		gm.playMode = ToolBox.PlayMode.Battle;
		gm.battleMode = ToolBox.BattleMode.Normal;
		gm.SetSelectStage("Simple", 0);
		gm.LoadLevel("ShipSelect");
	}
	protected void Battle_ItemButtonClicked() {
		gm.playMode = ToolBox.PlayMode.Battle;
		gm.battleMode = ToolBox.BattleMode.Item;
		gm.SetSelectStage("Simple", 0);
		gm.LoadLevel("ShipSelect");
	}
	protected void Battle_PartsButtonClicked() {
		//gm.LoadLevel("GameStandbyMenu");
		Debug.Log("未定");
	}
#endregion
#region ゲームパッド入力
	protected void BButtonDown() {
		//キャンセル
		Cancel();
	}
#endregion
}