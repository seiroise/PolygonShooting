using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// ボタンにくっついてるコンポーネントをまとめて管理
/// </summary>
public class UIButtonComponents : MonoBehaviour {
	[Header("コンポーネント")]
	public List<UIButtonMessage> buttonMessages;
	public UILabel label;
	public UISprite sprite;
	public UITweener tweener;

#region 関数
	/// <summary>
	/// ButtonMessageに値を設定
	/// </summary>
	public void SetButtonMessage(int index, GameObject target, string functionName, UIButtonMessage.Trigger trigger) {
		//範囲確認
		if(index < 0 || buttonMessages.Count <= index) {
			Debug.LogWarning("インデックスが正しくありません");
			return;
		}
		buttonMessages[index].target = target;
		buttonMessages[index].functionName = functionName;
		buttonMessages[index].trigger = trigger;
	}
#endregion
}