using UnityEngine;
using System.Collections;

//RGBスライダー
public class UIRGBSlider : MonoBehaviour {

	[Header("UIパーツ")]
	public UISlider redSlider;
	public UISlider greenSlider;
	public UISlider blueSlider;

#region 関数

	//スライダーの色を取得する
	public Color GetColor() {
		return new Color(redSlider.sliderValue, greenSlider.sliderValue, blueSlider.sliderValue);
	}

	//スライダーの値を設定する
	public void SetColor(Color color) {
		redSlider.sliderValue = color.r;
		greenSlider.sliderValue = color.g;
		blueSlider.sliderValue = color.b;
	}

	//イベントハンドラーを設定する
	public void SetIventHandler(GameObject target, string functionName) {
		redSlider.eventReceiver = target;
		greenSlider.eventReceiver = target;
		blueSlider.eventReceiver = target;

		redSlider.functionName = functionName + "R";
		greenSlider.functionName = functionName + "G";
		blueSlider.functionName = functionName + "B";
	}

#endregion
}