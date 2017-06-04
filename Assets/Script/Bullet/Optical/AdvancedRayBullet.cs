using UnityEngine;
using System.Collections;

//発展型光線
public class AdvancedRayBullet : RayBullet {

	[Header("AdvancedRayBulletパラメータ")]
	public float drainEnergyPerSec;		//減らす速度
	protected Object prevHitObject;	//前回当たったオブジェクト

	//衝突処理
	public override int OnHit(Object hitObject) {
		//キャスト
		Ship ship = (Ship)hitObject;

		//衝突機体のエネルギーを奪う
		ship.nowEnergy -= drainEnergyPerSec * Time.deltaTime;
		if(ship.nowEnergy < 0f) ship.nowEnergy = 0f;
		return base.OnHit(hitObject);
	}
}