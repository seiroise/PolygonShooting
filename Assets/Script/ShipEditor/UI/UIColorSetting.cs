using UnityEngine;
using System.Collections;
/// <summary>
/// 機体パーツの色変更
/// </summary>
public class UIColorSetting : MonoBehaviour {
	[Header("RGBスライダー")]
	public UIRGBSliderInput rgbSlider;
	[Header("スプライト")]
	public UISprite prevSprite;
	protected Color prevColor;
	public UISprite nextSprite;
	protected Color nextColor;
	[Header("イベント")]
	public GameObject target;
	public string functionName = "OnChangeColor";
	//Other
	protected ToolBox.ShipPartsData selectShipPartsData;
#region MonoBehaviourイベント
	protected void Awake() {
		rgbSlider.target = gameObject;
		rgbSlider.functionName = "OnChangeColor";
	}
#endregion
#region 関数
	/// <summary>
	/// 機体パーツデータの設定
	/// </summary>
	public void SetShipPartsData(ToolBox.ShipPartsData shipPartsData) {
		selectShipPartsData = shipPartsData;
		SetColor(selectShipPartsData.figureData.GetColor());
	}
	/// <summary>
	/// 前の色と次の色を設定
	/// </summary>
	protected void SetColor(Color color) {
		SetPrevColor(color);
		SetNextColor(color);
		rgbSlider.SetColor(color);
	}
	/// <summary>
	/// 前の色を設定
	/// </summary>
	protected void SetPrevColor(Color color) {
		prevSprite.color = color;
		prevColor = color;
	}
	/// <summary>
	/// 次の色を設定
	/// </summary>
	protected void SetNextColor(Color color) {
		nextSprite.color = color;
		nextColor = color;
	}
#endregion
#region UIイベント
	protected void OnChangeColor(Color color) {
		if(selectShipPartsData == null) return;
		//次の色を設定
		SetNextColor(color);
	}
	protected void OKButtonClicked() {
		FuncBox.Notify(target, functionName, nextColor);
	}
	protected void ResetButtonClicked() {
		SetColor(prevColor);
	}
#endregion
}