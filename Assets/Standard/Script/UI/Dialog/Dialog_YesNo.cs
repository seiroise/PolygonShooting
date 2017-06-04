using UnityEngine;
using System.Collections;
//イエスノー表示
/// <summary>
/// 二択表示ダイアログ
/// </summary>
public class Dialog_YesNo : MonoBehaviour {
	[Header("UIパーツ")]
	public UIButtonMessage yesButton;
	public UIButtonMessage noButton;
	public UILabel titleLabel;
	public UILabel yesNoLabel;
	[Header("イベント")]
	public GameObject target;
	public string functionName = "OnYesNoResult";
#region MonoBehaviourイベント
	protected void Start() {
		//初期化
		Init();
	}
#endregion
#region 関数
	/// <summary>
	/// 初期化
	/// </summary>
	protected void Init() {
		if(yesButton) {
			yesButton.target = gameObject;
			yesButton.functionName = "OnYesCliked";
		}
		if(noButton) {
			noButton.target = gameObject;
			noButton.functionName = "OnNoCliked";
		}
	}
	/// <summary>
	/// 表示テキストの設定
	/// </summary>
	public void SetDialogText(string title, string text) {
		titleLabel.text = title;
		yesNoLabel.text = text;
	}
	/// <summary>
	/// イベント関連の設定
	/// </summary>
	public void SetEvent(GameObject target, string functionName = "OnYesNoResult") {
		this.target = target;
		this.functionName = functionName;
	}
#endregion
#region UIイベント
	protected void OnYesCliked() {
		target.SendMessage(functionName, true, SendMessageOptions.DontRequireReceiver);
	}
	protected void OnNoCliked() {
		target.SendMessage(functionName, false, SendMessageOptions.DontRequireReceiver);
	}
#endregion
}