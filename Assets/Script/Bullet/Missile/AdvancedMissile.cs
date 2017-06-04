using UnityEngine;
using System.Collections;
/// <summary>
/// ミサイルの発展版(というかこのゲームのクラスに依存する版)
/// </summary>
public class AdvancedMissile : Missile {
#region MonoBehaviourイベント
	protected override void Start() {
		base.Start();
		//ターゲット設定
		SetTarget();
	}
#endregion
#region 関数
	//ターゲットを設定
	public void SetTarget() {
		if(!owner) return;
		Pilot p = owner.GetComponent<Pilot>();
		if(p.lockObject) {
			target = p.lockObject.gameObject;
		}
	}
#endregion
}