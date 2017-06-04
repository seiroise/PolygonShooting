using UnityEngine;
using System.Collections;
/// <summary>
/// テキスト入力用ダイアログ
/// </summary>
public class Dialog_Input : MonoBehaviour {
	[Header("UIパーツ")]
	public UIInput input;		//入力
	public UILabel inputLabel;	//入力ラベル
	public UILabel prevLabel;	//前回の
	[Header("イベント")]
	public GameObject target;
	public string functionName = "OnInputSubmit";
#region MonoBehaviourイベント
	protected void Start() {
		input.eventReceiver = gameObject;
		input.functionName = "OnSubmit";
	}
#endregion
#region 関数
	/// <summary>
	/// 入力欄に文字列を設定
	/// </summary>
	public void SetText(string text) {
		if(prevLabel) prevLabel.text = text;
		inputLabel.text = text;
	}
#endregion
#region UIイベント
	protected void OnSubmit(string text) {
		FuncBox.Notify(target, functionName, text);
	}
#endregion
}