using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// エディットメニューGU管理
/// </summary>
public class ShipEditorMenuGUIManager : SingletonMonoBehaviour<ShipEditorMenuGUIManager> {
	//管理クラス
	protected GameManager gm;
	[Header("表示_Menu")]
	public GameObject menu;
	[Header("表示_New")]
	public GameObject size;
	[Header("表示_Exist")]
	public GameObject ships;
	public UISelect_Ship shipSelect;
	public GameObject editButtons;
	public GameObject warning;
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
		indicateStack = new ClassBox.GameObjectStack();
		//menuをまず詰める
		indicateStack.Push(menu);
		//表示_Edit
		ships.SetActive(false);
		editButtons.SetActive(false);
		warning.SetActive(false);
		//表示_New
		size.SetActive(false);
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
		GameObject g = indicateStack.Pop();
		if(g == ships) {
			shipSelect.shipDataIndicator.Indicate(false);
		}
		//スタックに追加
		gamepadInput.target = indicateStack.Peek();
	}
#endregion
#region UIイベント
	//Button
	protected void ReturnButtonClicked() {
		gm.LoadLevel("MainMenu");
	}
	protected void NewButtonClicked() {
		//非表示
		HideMenu();
		//sclaeをスタックに
		indicateStack.Push(size);
		//ゲームパッドのイベント送信先をsizeに
		gamepadInput.target = size;
	}
	protected void ExistButtonClicked() {
		//非表示
		HideMenu();
		//shipsをスタックに
		indicateStack.Push(ships);
		//プレイヤーカテゴリの機体リストを取得する
		List<string> cateList = new List<string>();
		cateList.Add(gm.defaultShipCategory);
		shipSelect.SetCategoryList(cateList, 0);
		//ゲームパッドのイベント送信先をshipsに
		gamepadInput.target = ships;
	}
	//Button_New
	protected void SizeButtonClicked(GameObject g) {
		//サイズ設定
		ToolBox.ShipSize s = gm.GetShipSize(g.name);
		if(s == null) s = gm.shipSizeList[0];
		gm.SetEditShipData(null);
		gm.editShipSize = s;
		gm.LoadLevel("ShipEditor");
	}
	//Button_Exist
	protected void EditButtonClicked() {
		gm.LoadLevel("ShipEditor");
	}
	protected void DeleteButtonClicked() {
		//スタックがeditButtonsにくるまでループ
		indicateStack.StackCheck(editButtons);
		//Warningをスタックに
		indicateStack.Push(warning);
	}
	protected void CopyWithEditButtonClicked() {
		
	}
	protected void YesButtonClicked() {
		Debug.Log("Delete!");
		//削除処理

		//スタックがなくなるまでループ
		indicateStack.StackCheck(null);
		//shipsをスタックに
		indicateStack.Push(ships);
	}
	protected void NoButtonClicked() {
		//スタックがeditButtonsにくるまでループ
		indicateStack.StackCheck(editButtons);
	}
	//ShipSelect
	protected void ShipSelectButtonClicked(ToolBox.ShipData shipData) {
		//スタックがshipsにくるまでループ
		indicateStack.StackCheck(ships);
		//編集ボタンを表示
		indicateStack.Push(editButtons);
		//ゲームパッドのイベント送信先をeditButtonsに
		gamepadInput.target = editButtons;
		//編集機体を表示
		gm.SetEditShipData(shipData);
		gm.editShipSize = gm.GetShipSize(shipData.shipSize);
	}
#endregion
#region ゲームパッドイベント(GamepadInput_Message)
	protected void BButtonDown() {
		//キャンセル
		Cancel();
	}
#endregion
}