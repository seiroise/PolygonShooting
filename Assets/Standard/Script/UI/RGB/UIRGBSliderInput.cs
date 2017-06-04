using UnityEngine;
using System.Collections;
/// <summary>
/// 3つのUISliderInputから色を入力する
/// </summary>
public class UIRGBSliderInput : MonoBehaviour {
	[Header("Slider")]
	public UISliderInput[] rgbSlider;
	[Header("Event")]
	public GameObject target;
	public string functionName = "OnChangeColor";
	protected float r = 1f, g = 1f, b = 1f;
#region MonoBehaviourイベント
	protected void Start() {
		//スライダー設定
		rgbSlider[0].name = "Red";
		rgbSlider[1].name = "Green";
		rgbSlider[2].name = "Blue";
		for(int i = 0; i < rgbSlider.Length; i++) {
			rgbSlider[i].baseNum = 255;
			rgbSlider[i].target = gameObject;
			rgbSlider[i].functionName = "OnSliderValueChange";
		}
	}
#endregion
#region 関数
	/// <summary>
	/// スライダーを色
	/// </summary>
	public void SetColor(Color c) {
		//色を設定
		r = c.r;
		g = c.g;
		b = c.b;
		rgbSlider[0].SetSliderNum(c.r);
		rgbSlider[1].SetSliderNum(c.g);
		rgbSlider[2].SetSliderNum(c.b);
	}
	/// <summary>
	/// ターゲットにイベントを通知
	/// </summary>
	protected void NotifyTarget() {
		if(target) {
			target.SendMessage(functionName, new Color(r, g, b));
		}
	}
#endregion
#region UIイベント
	protected void OnSliderValueChange(UISliderInput slider) {
		switch(slider.name) {
			case "Red":
				r = slider.slider.sliderValue;
			break;
			case "Green":
				g = slider.slider.sliderValue;
			break;
			case "Blue":
				b = slider.slider.sliderValue;
			break;
		}
		//イベント通知
		NotifyTarget();
	}
#endregion
}