using UnityEngine;
using System.Collections;
/// <summary>
/// ランチャー状態の表示
/// </summary>
public class UILauncherState : MonoBehaviour {
	[Header("UIパーツ")]
	public UILabel reloadCountLabel;	//リロード数
	public UISprite reloadParSprite;	//リロード率表示
	[Header("エフェクト")]
	public UITweener shotEffectTween;	//発射エフェクト
#region 関数
	public void Set(string text, float par) {
		reloadCountLabel.text = text;
		reloadParSprite.fillAmount = par;
	}
#endregion
}