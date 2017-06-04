using UnityEngine;
using System.Collections;
using System.Text;
using System.Collections.Generic;
/// <summary>
/// 協力リザルト画面のGUI管理
/// </summary>
public class Result_VsEnemyGUIManager : SingletonMonoBehaviour<Result_VsEnemyGUIManager> {
	//管理クラス
	private GameManager gm;
	[Header("YourRank")]
	public UILabel yourRankLabel;
	[Header("ランキング")]
	public int indicateNum = 15;
	public UILabel rankLabel;
	public UILabel playerLabel;
	public UILabel waveLabel;
	public UILabel dateLabel;
	[Header("プレイヤーリザルト")]
	public UIResultIndicator[] playerResults;
#region MonoBehaviourイベント
	protected override void Awake() {
		base.Awake();
	}
	private void Start() {
		gm = GameManager.Instance;
		//ランキング設定
		SetRanking();
		//結果表示のactiveをfalseに
		for(int i = 0; i < playerResults.Length; i++) {
			playerResults[i].gameObject.SetActive(false);
		}
		//結果設定
		List<ToolBox.PlayerScore> scoreList = new List<ToolBox.PlayerScore>(gm.playerScores);
		for(int i = 0; i < scoreList.Count; i++) {
			if(scoreList[i] != null) {
				int playerNo = scoreList[i].playerNo;
				//スコア
				playerResults[playerNo].Activate(playerNo + 1, gm.playerColor[playerNo], i + 1, scoreList[i], gm.GetPlayerSelectShipData(playerNo));
			}
		}
		//yourRank
		yourRankLabel.text = "YourRank " + gm.yourRank.ToString();
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
	/// ランキングデータを設定する
	/// </summary>
	private void SetRanking() {
		//ランク
		StringBuilder sb = new StringBuilder();
		for(int i = 1; i <= indicateNum; i++) {
			sb.AppendLine(i.ToString());
		}
		rankLabel.text = sb.ToString();
		//ウェーブ
		string text = gm.Ranking_Wave(indicateNum);
		waveLabel.text = text;
		//プレイヤー
		text = gm.Ranking_Player(indicateNum);
		playerLabel.text = text;
		//日付
		text = gm.Ranking_Date(indicateNum);
		dateLabel.text = text;
	}
#endregion
#region ゲームパッドイベント
	private void StartButtonDown() {
		gm.LoadLevel("ShipSelect");
	}
#endregion
}