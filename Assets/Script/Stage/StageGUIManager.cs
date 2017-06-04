using UnityEngine;
using System.Collections;
/// <summary>
/// ステージ(テスト)のGUI管理
/// </summary>
public class StageGUIManager : SingletonMonoBehaviour<StageGUIManager> {
	//管理クラス
	protected GameManager gm;
	protected StageManager sm;
	[Header("PlayerUI")]
	public StagePlayerIndicator_Single[] stagePlayerIndicator;
	[Header("GameStart")]
	public UITimer gameStartTimer;
	public Animator gameStartTimerAnimation;
	[Header("PlayModeUI")]
	public GameObject battleUIParent;
	public GameObject vsEnemyUIParent;
	[Header("BattleTimer")]
	public UITimer battleTimer;
	public Animator battleTimerAnimation;
	[Header("WaveTimer")]
	public UITimer waveTimer;
	[Header("Wave")]
	public UILabel waveNumLabel;	//ウェーブ数
	[Header("WaveIndicator")]
	public GameObject waveIndicator;			//ウェーブ数表示用
	public Animator waveIndicatorAnimator;	
	public UILabel waveIndicatorLabel;
	[Header("Comment")]
	public GameObject comment;
	public UILabel commentLabel;
#region MonoBehaviourイベント
	protected override void Awake() {
		base.Awake();
	}
	protected void Start() {
		gm = GameManager.Instance;
		sm = StageManager.Instance;
	}
#endregion
#region 関数
	/// <summary>
	/// プレイヤーUIの有効化
	/// </summary>
	public void ActivatePlayerUI(Player[] players) {
		//UIの割り当て
		for(int i = 0; i < stagePlayerIndicator.Length; i++) {
			if(stagePlayerIndicator[i] == null) continue;
			if(players.Length > i) {
				stagePlayerIndicator[i].SetPlayer(players[i]);

			} else {
				stagePlayerIndicator[i].Indicate(false);
			}
		}
	}
	/// <summary>
	/// プレイヤーUIの表示/非表示
	/// </summary>
	public void IndicatePlayerUI(bool flag) {
		if(flag) {
			//trueの場合はplayerの設定されているものだけ
			for(int i = 0; i < stagePlayerIndicator.Length; i++) {
				if(stagePlayerIndicator[i].CheckSetPlayer()) {
					stagePlayerIndicator[i].Indicate(flag);
				}
			}
		} else {
			//falseの場合は問答無用で
			for(int i = 0; i < stagePlayerIndicator.Length; i++) {
				stagePlayerIndicator[i].Indicate(flag);
			}
		}
	}
	/// <summary>
	/// ゲーム開始タイマーの開始
	/// </summary>
	public void PlayGameStartTimer() {
		gameStartTimer.Play(3);
		//gameStartTimerAnimation.SetBool("Plaing", true);
	}
	/// <summary>
	/// バトルタイマーの開始
	/// </summary>
	public void PlayBattleTimer(float time) {
		battleTimer.Play(time);
		battleTimerAnimation.SetBool("Plaing", true);
	}
	/// <summary>
	/// プレイヤーのスコアを設定する
	/// </summary>
	public void SetPlayerScore(ToolBox.PlayerScore[] scores) {
		for(int i = 0; i < scores.Length; i++) {
			if(stagePlayerIndicator[i].CheckSetPlayer()) {
				scores[stagePlayerIndicator[i].GetPlayerNo()].score
					= stagePlayerIndicator[i].scoreLabel.GetTargetNum();
			}
		}
	}
	/// <summary>
	/// プレイモード固有のUIを表示する
	/// </summary>
	public void IndicatePlayModeUI(ToolBox.PlayMode playMode) {
		//全てのプレイモード固有のUIを非表示に
		HidePlayModeUI();
		//プレイモード毎に表示
		switch(playMode) {
			case ToolBox.PlayMode.Battle:
				battleUIParent.SetActive(true);
			break;
			case ToolBox.PlayMode.VsEnemy:
				vsEnemyUIParent.SetActive(true);
			break;
		}
	}
	/// <summary>
	/// プレイモード固有のUIを全て非表示にする
	/// </summary>
	public void HidePlayModeUI() {
		battleUIParent.SetActive(false);
		vsEnemyUIParent.SetActive(false);
	}
	/// <summary>
	/// コメントを表示
	/// </summary>
	public void IndicateComment(string commentText) {
		comment.SetActive(true);
		commentLabel.text = commentText;
	}
	/// <summary>
	/// コメント非表示
	/// </summary>
	public void HideComment() {
		comment.SetActive(false);
	}
	/// <summary>
	/// ウェーブインディケータを表示
	/// </summary>
	public void SetWaveIndicator(string text) {
		if(waveIndicatorAnimator) {
			waveIndicator.SetActive(true);
			waveIndicatorAnimator.SetTrigger("indicate");
			waveIndicatorLabel.text = text;
		}
	}
#endregion
}