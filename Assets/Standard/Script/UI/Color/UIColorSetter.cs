using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// UIWidgetの色を一括変更
/// </summary>
public class UIColorSetter : MonoBehaviour {	
	[Header("UI")]
	public List<UIWidget> uiPartsList;
	[Header("Color")]
	public Color setColor;
#region 関数
	/// <summary>
	/// 指定した色を一括でUIWidgetに設定
	/// </summary>
	public void SetColor(Color color) {
		foreach(UIWidget t in uiPartsList) {
			t.color = color;
		}
	}
#endregion
}