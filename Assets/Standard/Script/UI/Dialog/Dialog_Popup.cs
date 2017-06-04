using UnityEngine;
using System.Collections;
/// <summary>
/// ポップアップ表示用
/// </summary>
public class Dialog_Popup : MonoBehaviour {
	[Header("カメラ")]
	public Camera targetCamera;	//ビューポート変換先
	[Header("UI")]
	public GameObject popup;
	public UILabel label;		//テキスト
	public UISprite background;	//テキストバックグラウンド
#region 関数
	/// <summary>
	/// ポップアップのテキスト、位置を設定する
	/// </summary>
	public void SetPopup(Camera sourceCam, Vector3 worldPos, string text) {
		//ポップアップの位置
		Vector3 pos = FuncBox.ViewPointTransform(sourceCam, worldPos, targetCamera);
		popup.transform.position = pos;
		//テキスト
		label.text = text;
		//背景
		Vector2 size = label.relativeSize;
		Vector2 scale = label.transform.localScale;
		size.x *= scale.x;
		size.y *= scale.y;
		background.transform.localScale = size;
	}
#endregion
}