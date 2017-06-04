using UnityEngine;
using System.Collections;

//RGB入力
public class UIRGBInput : MonoBehaviour {

	[Header("UIパーツ")]
	public UIInput redInput;
	public UIInput greenInput;
	public UIInput blueInput;

#region 関数

	//色を設定する
	public void SetColor(Color color) {
		var c32 = new Color32((byte)(color.r * 255), (byte)(color.g * 255), (byte)(color.b * 255), 255);

		redInput.label.text = c32.r.ToString();
		greenInput.label.text = c32.g.ToString();
		blueInput.label.text = c32.b.ToString();
	}

	//イベントハンドラーを設定する
	public void SetIventHandler(GameObject target, string functionName) {
		redInput.eventReceiver = target;
		greenInput.eventReceiver = target;
		blueInput.eventReceiver = target;

		redInput.functionName = functionName + "R";
		greenInput.functionName = functionName + "G";
		blueInput.functionName = functionName + "B";
	}

#endregion
}