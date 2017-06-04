using UnityEngine;
using System.Collections;
/// <summary>
/// 機体選択画面のGUI管理クラス
/// </summary>
public class ShipSelectGUIManager : SingletonMonoBehaviour<ShipSelectGUIManager> {
	//管理クラス
	protected GameManager gm;
	[Header("プレイヤー")]
	public UISelect_PlayerShip[] playerShipSelect;
	[Header("UIパーツ")]
	public TweenAlpha goLabelEffect_Entry;			//ラベル登場エフェクト
	public TweenColor goLabelEffect_Stay;			//ラベル常時エフェクト
	//その他
	protected ToolBox.ShipData[] selectShipData;	//プレイヤー選択機体データ
	protected int entryNum = 0;					//参加人数
	protected bool flagStartAndGo = false;			//Go出来る状態
	protected bool flagFirstUpdate = false;			//初回アップデートフラグ
#region MonoBehaviourイベント
	protected override void Awake() {
		base.Awake();
		//初期化
		selectShipData = new ToolBox.ShipData[4];
		for(int i = 0; i < selectShipData.Length; i++) {
			selectShipData[i] = null;
		}
	}
	protected void Start() {
		gm = GameManager.Instance;
		//BGM再生
		gm.PlayMenuBGM();
	}
	protected void Update() {
		if(!flagFirstUpdate) {
			//プレイヤーの設定をUISelectに反映
			for(int i = 0; i < 4; i++) {
				playerShipSelect[i].gui = this;
				if(gm.GetPlayerSelectShipData(i) != null) {
					playerShipSelect[i].SetShipData(gm.GetPlayerSelectShipData(i));
				}
			}
			flagFirstUpdate = true;
		}
		if(Input.GetKeyDown(KeyCode.Space)) {
			GoNext();
		}
		if(Input.GetKeyDown(KeyCode.Escape)) {
			BackButtonDown();
		}
	}
#endregion
#region プレイヤー機体セレクト関連
	/// <summary>
	/// 指定番号に機体データを設定する
	/// </summary>
	public void SetShipData(int index, ToolBox.ShipData shipData) {
		selectShipData[index] = shipData;
		//参加人数の確認
		CheckEntryAndPlayEffect();
	}
	/// <summary>
	/// プレイヤー参加
	/// </summary>
	public void EntryPlayer() {
		//参加人数インクリ
		entryNum++;
		//参加人数の確認
		CheckEntryAndPlayEffect();
	}
	/// <summary>
	/// 選択し直し
	/// </summary>
	public void Reselect(int index) {
		selectShipData[index] = null;
		//参加人数の確認
		CheckEntryAndPlayEffect();
	}
	/// <summary>
	/// エントリー解除
	/// </summary>
	public void EntryOut(int index) {
		entryNum--;
		selectShipData[index] = null;
		//参加人数の確認
		CheckEntryAndPlayEffect();
	}
	/// <summary>
	/// 参加人数と選択人数が同値か確認する
	/// </summary>
	public bool CheckEntry() {
		int count = 0;
		for(int i = 0; i < selectShipData.Length; i++) {
			if(selectShipData[i] != null) {
				count++;
			}
		}
		Debug.Log(count + " : "  + entryNum);
		if(count == entryNum) {
			return true;
		} else {
			return false;
		}
	}
	/// <summary>
	/// 参加人数の確認とラベルエフェクトの再生を行う
	/// </summary>
	public void CheckEntryAndPlayEffect() {
		if(CheckEntry()) {
			goLabelEffect_Entry.Play(true);
			goLabelEffect_Stay.enabled = true;
			flagStartAndGo = true;
		} else {
			goLabelEffect_Entry.Play(false);
			goLabelEffect_Stay.enabled = false;
			flagStartAndGo = false;
		}
	}
	/// <summary>
	/// 決定してゲーム管理クラスに選択データを渡す
	/// </summary>
	protected void GoNext() {
		if(flagStartAndGo) {
			//ゲーム管理クラスに設定を渡す
			gm.SetPlayerSelectShipData(selectShipData);
			//ステージセレクトへ
			gm.LoadLevel("Stage_SingleCamera_Test_1");
			//BGMフェードアウト
			AudioManager.Instance.FadeOutBGM();
			//SE再生
			gm.PlayClickSE();
		}
	}
#endregion
#region ゲームパッドイベント
	protected void StartButtonDown() {
		GoNext();
	}
	protected void BackButtonDown() {
		gm.LoadLevel("GameMenu");
		gm.PlayClickSE();
	}
#endregion
}