using UnityEngine;
using System.Collections;
/// <summary>
/// スライダー入力用
/// </summary>
public class UISliderInput : MonoBehaviour {
	[Header("Slider")]
	public UISlider slider;
	public float baseNum;	//基数
	[Header("Label")]
	public UILabel numLabel;
	public bool flagIndicateInt = false;	//表示するときにintにするか
	[Header("Button")]
	public UIButtonComponents add;
	public float addNum = 1f;
	public UIButtonComponents sub;
	public float subNum = -1f;
	public UIButtonComponents strongAdd;
	public float strongAddNum = 10f;
	public UIButtonComponents strongSub;
	public float strongSubNum = -10f;
	[Header("Event")]
	public GameObject target;
	public string functionName = "OnSliderValueChange";
#region MonoBehaviourイベント
	protected void Start() {
		//設定
		//Slider
		slider.eventReceiver = gameObject;
		slider.functionName = "OnSliderValueChange";
		slider.numberOfSteps = 0;
		//Button
		add.SetButtonMessage(0, gameObject, "OnAddButtonClicked", UIButtonMessage.Trigger.OnClick);
		sub.SetButtonMessage(0, gameObject, "OnSubButtonClicked", UIButtonMessage.Trigger.OnClick);
		strongAdd.SetButtonMessage(0, gameObject, "OnStrongAddButtonClicked", UIButtonMessage.Trigger.OnClick);
		strongSub.SetButtonMessage(0, gameObject, "OnStrongSubButtonClicked", UIButtonMessage.Trigger.OnClick);
	}
#endregion
#region 関数
	/// <summary>
	/// スライダーの値に基数をかけたものにaddNumを足す
	/// </summary>
	protected void AddSliderBaseNum(float addNum) {
		float num = GetSliderBaseFloatNum();
		num += addNum;
		SetSliderBaseNum(num);
	}
	/// <summary>
	/// スライダーの値に基数をかけたものを取得する
	/// </summary>
	public float GetSliderBaseFloatNum() {
		return slider.sliderValue * baseNum;
	}
	public int GetSliderBaseIntNum() {
		return (int)(slider.sliderValue * baseNum);
	}
	/// <summary>
	/// 基数の掛かっている値を0~1に戻してスライダーに設定
	/// </summary>
	protected void SetSliderBaseNum(float num) {
		num /= baseNum;
		slider.sliderValue = num;
		//ラベル更新
		UpdateNumLabel();
	}
	//0~1の値をスライダーに設定
	public void SetSliderNum(float num) {
		slider.sliderValue = num;
		//ラベル更新
		UpdateNumLabel(false);
	}
	/// <summary>
	/// イベントターゲットにイベントを通知
	/// </summary>
	protected void NotifyTarget() {
		if(target) {
			target.SendMessage(functionName, this);
		}
	}
#endregion
#region UI関連
	/// <summary>
	/// 数値ラベルを更新。flagNotifyは通知するか
	/// </summary>
	protected void UpdateNumLabel(bool flagNotify = true) {
		if(flagIndicateInt) {
			int num = GetSliderBaseIntNum();
			numLabel.text = num.ToString();
		} else {
			float num = GetSliderBaseFloatNum();
			numLabel.text = num.ToString();
		}
		//イベント通知
		if(flagNotify) {
			NotifyTarget();
		}
	}
#endregion
#region UIイベント
	protected void OnSliderValueChange(float value) {
		//ラベル更新
		UpdateNumLabel();
	}
	protected void OnAddButtonClicked() {
		AddSliderBaseNum(addNum);
	}
	protected void OnSubButtonClicked() {
		AddSliderBaseNum(subNum);
	}
	protected void OnStrongAddButtonClicked() {
		AddSliderBaseNum(strongAddNum);
	}
	protected void OnStrongSubButtonClicked() {
		AddSliderBaseNum(strongSubNum);
	}
#endregion
}