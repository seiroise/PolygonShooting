using UnityEngine;
using System.Collections;

//捉えたら指定オブジェクトを生成する
public class LookAtInstantiateMissile : LookAtMissile {
	[Header("生成オブジェクト")]
	public Bullet instantBullet;
#region
	//閾値を超えたとき
	protected override void OnOverBorder() {
		//削除
		OnDestroyer();
	}
	//削除されるとき
	public override void OnDestroyer() {
		base.OnDestroyer();
		//生成
		InstantiateBullet(instantBullet.gameObject);
	}
#endregion
}