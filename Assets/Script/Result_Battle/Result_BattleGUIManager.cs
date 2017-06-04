using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// 対戦リザルト画面のGUI管理
/// </summary>
public class Result_BattleGUIManager : SingletonMonoBehaviour<Result_BattleGUIManager> {
	//管理クラス
	private GameManager gm;
	[Header("プレイヤーリザルト")]
	public UIResultIndicator[] playerResults;
#region MonoBehaviourイベント
	protected override void Awake() {
		base.Awake();
	}
	private void Start() {
		gm = GameManager.Instance;
		//結果表示のactiveをfalseに
		for(int i = 0; i < playerResults.Length; i++) {
			playerResults[i].gameObject.SetActive(false);
		}
		//結果設定
		SetPlayerScore();
		//BGM
		gm.PlayMenuBGM();
	}
	private void Update() {
		if(Input.GetKeyDown(KeyCode.Escape)) {
			gm.LoadLevel("ShipSelect");
		}
	}
#endregion
#region 関数
	/// <summary>
	/// プレイヤースコアの設定
	/// </summary>
	private void SetPlayerScore() {
		List<ToolBox.PlayerScore> scoreList = new List<ToolBox.PlayerScore>(gm.playerScores);
		//スコアからランキングを作成
		scoreList.Sort((a, b) => {
			if(a == null) return 1;
			if(b == null) return -1;
			if(a.score > b.score) {
				return -1;
			} else if(a.score < b.score){
				return 1;
			} else {
				return 0;
			}
		});
		for(int i = 0; i < scoreList.Count; i++) {
			if(scoreList[i] != null) {
				int playerNo = scoreList[i].playerNo;
				//スコア
				playerResults[playerNo].Activate(playerNo + 1, gm.playerColor[playerNo], i + 1, scoreList[i], gm.GetPlayerSelectShipData(playerNo));
			}
		}
	}
#endregion
#region ゲームパッドイベント
	protected void StartButtonDown() {
		gm.LoadLevel("ShipSelect");
	}
#endregion
}