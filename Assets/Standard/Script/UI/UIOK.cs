using UnityEngine;
using System.Collections;

//OK表示
public class UIOK : MonoBehaviour {

	[Header("UIパーツ")]
	public UIButtonMessage okButton;
	public UILabel okText;

	[Header("イベント")]
	public GameObject target;
	public string functionName = "OnResult";

#region MonoBehaviourイベント

	protected void Start() {
		//初期化
		Init();
	}

#endregion

#region 関数

	protected void Init() {
		okButton.target = target;
		okButton.functionName = functionName;
	}

	public void SetText(string text) {
		okText.text = text;
	}

#endregion
}
