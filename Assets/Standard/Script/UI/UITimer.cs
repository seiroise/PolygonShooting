using UnityEngine;
using System.Collections;
/// <summary>
/// タイマー
/// </summary>
public class UITimer : MonoBehaviour {
	[Header("時間")]
	public float time;
	private float startTime;
	public bool playOnAwake = false;
	[Header("UI")]
	public UILabel label;
	public string timerEndText = "GO!!";
	public UISprite bar;
	[Header("イベント")]
	public GameObject eventTarget;
	public string functionName = "OnTimerEnd";
	public float[] timeEvent;			//時間ごとのイベント
	public string timeEventFunctionName = "OnTimeEvent";
	private int timeEventIndex = 0;		//時間ごとイベントの要素数
	//その他
	protected bool flagPlay = false;
#region MonoBehaviourイベント
	private void Awake() {
		if(playOnAwake) {
			Play(time);
		}
	}
	protected void Update () {
		if(flagPlay) {
			//ラベル
			if(label) label.text = Mathf.CeilToInt(time).ToString();
			//バー
			if(bar) bar.fillAmount = time / startTime;
			//時間
			if(time > 0) {
				time -= Time.deltaTime;
				//時間ごとイベント確認
				if(timeEvent != null) {
					if(timeEvent.Length > timeEventIndex) {
						if(timeEvent[timeEventIndex] > time) {
							FuncBox.Notify(eventTarget, timeEventFunctionName, timeEvent[timeEventIndex]);
							timeEventIndex ++;
						}
					}
				}
				//終了確認
				if(time < 0) {
					FuncBox.Notify(eventTarget, functionName, null);
					time = 0f;
					flagPlay = false;
					if(label) label.text = timerEndText;
				}
			}
		}
	}
#endregion
#region 関数
	/// <summary>
	/// 時間を指定してタイマーを開始
	/// </summary>
	public void Play(float time = 3f) {
		flagPlay = true;
		timeEventIndex = 0;
		this.time = startTime = time;
	}
	/// <summary>
	/// 一時停止 trueで停止、falseで再開
	/// </summary>
	public void Pause(bool flag) {
		flagPlay = !flag;
	}
#endregion
}