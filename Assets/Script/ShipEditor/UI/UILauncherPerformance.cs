using UnityEngine;
using System.Collections;
/// <summary>
/// ランチャーの性能を表示
/// </summary>
public class UILauncherPerformance : MonoBehaviour {
	[Header("UIパーツ")]
	public UILabel barrel;
	public UILabel caliber;
	public UILabel velocity;
	public UILabel damage;
	public UILabel reloadSpeed;
#region 関数
	/// <summary>
	/// 性能を表示する。フラグは単位を表示するか
	/// </summary>
	public void SetPerformance(ToolBox.LauncherPerformance p, bool flagTani) {
		if(flagTani) {
			barrel.text = p.barrel + "m";
			caliber.text = p.caliber + "m";
			velocity.text = p.velocity + "m/s";
			damage.text = p.damage.ToString();
			reloadSpeed.text = p.reloadSpeed + "s";
		} else {
			barrel.text = p.barrel.ToString();
			caliber.text = p.caliber.ToString();
			velocity.text = p.velocity.ToString();
			damage.text = p.damage.ToString();
			reloadSpeed.text = p.reloadSpeed.ToString();
		}
	}
	/// <summary>
	/// 指定した文字列を表示する
	/// </summary>
	public void SetPerformance(string text) {
		barrel.text = text;
		caliber.text = text;
		velocity.text = text;
		damage.text = text;
		reloadSpeed.text = text;
	}
#endregion
}