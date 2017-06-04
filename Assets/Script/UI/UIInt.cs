using UnityEngine;
using System.Collections;
/// <summary>
/// int型の数値を表示
/// </summary>
public class UIInt : MonoBehaviour {
	[Header("UIパーツ")]
	public UILabel label;
	[Header("設定")]
	public float lerpPar = 20f;
	//その他
	private int targetNum;
	private int nowNum;
#region MonoBehaviourイベント
	protected void Start() {
		label.text = "0";
		targetNum = nowNum = 0;
	}
	protected void Update() {
		if(targetNum != nowNum) {
			nowNum = (int)Mathf.Lerp(nowNum, targetNum, lerpPar * Time.deltaTime);
			if(Mathf.Abs(targetNum - nowNum) <= 2) {
				nowNum = targetNum;
			}
			label.text = nowNum.ToString();

		}
	}
#endregion
#region 関数
	/// <summary>
	/// 数値を設定
	/// </summary>
	public void SetInt(int num) {
		nowNum = num;
	}
	/// <summary>
	/// 加算した数値を設定
	/// </summary>
	public void AddInt(int num) {
		targetNum += num;
	}
	/// <summary>
	/// 減算した数値を設定
	/// </summary>
	public void SubInt(int num) {
		targetNum -= num;
	}
	/// <summary>
	/// 目標数値を取得
	/// </summary>
	public int GetTargetNum() {
		return targetNum;		
	}
#endregion
}