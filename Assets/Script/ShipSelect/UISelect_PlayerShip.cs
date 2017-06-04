using UnityEngine;
using System.Collections;
using GamepadInput;
using System.Collections.Generic;
/// <summary>
/// プレイヤー用の機体選択
/// </summary>
//結構複雑なことしてるからあとから見たらわかりにくいかも
//基本的にenabledを操作して入力の制御をしてる
public class UISelect_PlayerShip : UISelect_Ship {
	//管理クラス
	[HideInInspector]
	public ShipSelectGUIManager gui;
	[Header("UIパーツ")]
	public UILabel playerNoLabel;		//プレイヤー番号
	public UILabel captionLabel;		//説明ラベル
	public UITweener captionTweener;
	[Header("Tweener")]
	public UITweener entryTween;	//参加tween;
	[Header("ColorSetter")]
	public UIColorSetter colorSetter;	//色設定
	[Header("GamepadInput")]
	public GamePad.Index index;
	public GamepadInput_Message gamepadInput;
	//その他
	public bool flagEntry;		//参加
#region MonoBehaviourイベント
	protected override void Awake() {
		base.Awake();
		entryTween.enabled = false;		
	}
	protected override void Start() {
		base.Start();
		gui = ShipSelectGUIManager.Instance;
		enabled = false;
		//機体情報は非表示
		shipDataIndicator.Indicate(false);
		//プレイヤー番号
		playerNoLabel.text = "Player" + (int)index;
		//ゲームパッド
		if(gamepadInput) {
			gamepadInput.target = gameObject;
			gamepadInput.index = index;
		}
		//色
		if(colorSetter) {
			colorSetter.SetColor(gm.playerColor[((int)index) - 1]);
		}
		//プレイモードによって選べるカテゴリを限定する
		List<string> categoryList = new List<string>();
		switch(gm.playMode) {
			case ToolBox.PlayMode.Battle:
				categoryList.Add("Player");
			break;
			case ToolBox.PlayMode.VsEnemy:
				categoryList.Add("VsEnemy");
			break;
		}
		SetCategoryList(categoryList);
	}
#endregion
#region 関数
	/// <summary>
	/// 参加
	/// </summary>
	protected void Entry() {
		PlayEntryTween(true);
		enabled = true;
		flagEntry = true;
		shipDataIndicator.Indicate(true);
		//guiに知らせる
		gui.EntryPlayer();
	}
	/// <summary>
	/// 参加Tween再生
	/// </summary>
	protected void PlayEntryTween(bool flag) {
		entryTween.Play(flag);
		if(flag) {
			captionTweener.Play(true);
		} else {
			captionTweener.Play(false);
			captionLabel.text = "Cancel\"B\"";
		}
	}
	/// <summary>
	/// 外部から機体データを設定,エントリー状態にする
	/// </summary>
	public void SetShipData(ToolBox.ShipData shipData) {
		Entry();
		shipDataIndicator.SetShipData(shipData);
		//gui管理クラスに知らせる
		gui.SetShipData(((int)index) - 1, shipData);
		//選択出来ないように
		if(enabled) enabled = false;
		PlayEntryTween(false);
	}
#endregion
#region UIイベント
	/// <summary>
	/// 参加ボタンクリック
	/// </summary>
	protected void EntryButtonClicked() {
		if(!enabled) {
			if(!flagEntry) {
				Entry();
			} else {
				BButtonDown();
			}
		}
	}
	/// <summary>
	/// ボタンホバー
	/// </summary>
	protected override void OnSelectButtonHover(GameObject obj) {
		if(enabled) {
			//データ取得
			ToolBox.ShipData shipData = GetShipDataFromObjectName(obj);
			//データ表示
			if(shipDataIndicator) {
				shipDataIndicator.SetShipData(shipData);
			}
		}
	}
	/// <summary>
	/// ボタンクリック
	/// </summary>
	protected override void OnSelectButtonClicked(GameObject obj) {
		if(enabled) {
			//インデックスを求める
			ToolBox.ShipData shipData = GetShipDataFromObjectName(obj);
			//gui管理クラスに知らせる
			gui.SetShipData(((int)index) - 1, shipData);
			//選択出来ないように
			enabled = false;
			PlayEntryTween(false);
		}
	}
#endregion
#region ゲームパッドイベント(GamepadInput_Message)
	protected override void AButtonDown() {
		if(enabled) {
			base.AButtonDown();
		} else {
			if(!flagEntry) {
				Entry();
			}
		}
	}
	protected override void AButtonUp() {
		if(enabled) {
			base.AButtonUp();
		}
	}

	protected void BButtonDown() {
		if(!enabled) {
			if(flagEntry) {
				//再選択
				gui.Reselect(((int)index) - 1);
				enabled = true;
				PlayEntryTween(true);
			}
		} else {
			//キャンセル
			enabled = false;
			flagEntry = false;
			PlayEntryTween(false);
			shipDataIndicator.Indicate(false);
			gui.EntryOut(((int)index) - 1);
		}
	}

	protected override void RShoulderButtonDown() {
		if(enabled) base.RShoulderButtonDown();
	}
	protected override void LShoulderButtonDown() {
		if(enabled) base.LShoulderButtonDown();
	}

	protected override void Up() {
		if(enabled) base.Up();
	}
	protected override void Down() {
		if(enabled) base.Down();
	}
	protected override void Right() {
		if(enabled) base.Right();
	}
	protected override void Left() {
		if(enabled) base.Left();
	}
#endregion
}