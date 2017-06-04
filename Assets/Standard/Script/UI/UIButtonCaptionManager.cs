using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

//ボタンの説明を管理
public class UIButtonCaptionManager : MonoBehaviour {
	[Serializable]
	public class ButtonCaption {
		public string caption;
		public GameObject button;
	}
	public UIButtonMessage.Trigger captionTrigger;
	public UILabel captionLabel;
	public List<ButtonCaption> buttonList;
	protected Dictionary<GameObject, string> buttonDic;	//検索用にbuttonListを辞書化したもの
#region MonoBehaviourイベント
	protected void Start() {
		//初期化
		SetButtonMessage();
		CreateButtonDic();
	}
#endregion
#region 関数
	/// <summary>
	/// buttonListのボタンにUIButtonMessageを追加、設定を行う。
	/// </summary>
	protected void SetButtonMessage() {
		UIButtonMessage bMessage;
		foreach(ButtonCaption b in buttonList) {
			if(!b.button) continue;
			//UIButtonMessageの追加
			bMessage = b.button.AddComponent<UIButtonMessage>();
			bMessage.target = gameObject;
			bMessage.trigger = captionTrigger;
			bMessage.functionName = "OnTrigger";
		}
	}
	/// <summary>
	/// 検索用辞書の作成
	/// </summary>
	protected void CreateButtonDic() {
		buttonDic = new Dictionary<GameObject,string>();
		foreach(ButtonCaption b in buttonList) {
			buttonDic.Add(b.button, b.caption);
		}
	}
#endregion
#region UIイベント
	//Button
	protected void OnTrigger(GameObject g) {
		if(!buttonDic.ContainsKey(g)) return;
		//説明文を設定
		captionLabel.text = buttonDic[g];
	}
#endregion
}