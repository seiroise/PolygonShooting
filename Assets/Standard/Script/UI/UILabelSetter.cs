using UnityEngine;
using System.Collections;

//シーン中のラベルにいろんなことをする
public class UILabelSetter : MonoBehaviour {
	
	public UIFont font;

#region 関数
	protected UILabel[] GetAllUILabel() {
		return FindObjectsOfType<UILabel>();
	}
	public void FontSet() {
		UILabel[] labels = GetAllUILabel();
		foreach(UILabel label in labels) {
			label.font = font;
		}
	}
#endregion
}