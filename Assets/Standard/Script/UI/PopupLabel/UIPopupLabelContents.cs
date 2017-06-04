using UnityEngine;
using System.Collections;

//UIPopupLabelManagerで使用
public class UIPopupLabelContents : MonoBehaviour {
	[Header("Tween")]
	public TweenPosition tweenPos;	//座標
	public UITweener tweenEffect;	//効果
	[Header("UIパーツ")]
	public UISprite sprite;			//スプライト
	public UILabel label;			//ラベル
	[Header("イベント")]
	public UIPopupLabelManager manager;		//管理クラス
	//その他
	public bool flagPlay = false;
	protected bool flag = false;
#region MonoBehaviourイベント
	protected void Start() {
		//tween
		if(tweenPos) {
			tweenPos.eventReceiver = gameObject;
			tweenPos.callWhenFinished = "OnFinished";
			tweenPos.Play(false);
		}
		if(tweenEffect) {
			tweenEffect.Play(false);
		}
	}
#endregion
#region 関数
	/// <summary>
	/// 再生 
	/// </summary>
	public void Play(string text, bool flagEffect) {
		label.text = text;
		flag = flag ? false : true;
		tweenPos.Play(flag);
		if(tweenEffect) {
			tweenEffect.Play(flagEffect);
		}
		flagPlay = true;
	}
#endregion
#region イベント
	//tweenPos終了
	protected void OnFinished() {
		flagPlay = false;
		//エフェクト停止
		if(tweenEffect) {
			tweenEffect.Play(false);
		}
		if(manager) {
			manager.OnFinished();
		}
	}
#endregion
}