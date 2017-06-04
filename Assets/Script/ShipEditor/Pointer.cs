using UnityEngine;
using System.Collections;
/// <summary>
/// 移動量を指定してxy平面上を移動させるポインター
/// </summary>
public class UIPointer : MonoBehaviour {
	[Header("ポインター")]
	public GameObject pointer;
	[Header("カメラ")]
	public Camera camera;
	[Header("移動範囲")]
	public Rect moveArea;
#region 関数
	/// <summary>
	/// 移動
	/// </summary>
	protected void Move(Vector2 move) {
		
	}
#endregion
}