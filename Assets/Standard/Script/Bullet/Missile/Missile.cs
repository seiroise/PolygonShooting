using UnityEngine;
using System.Collections;
/// <summary>
/// ミサイル
/// </summary>
public class Missile : Bullet {
	protected GameObject target;		//対象
	[Header("Missileパラメータ")]
	[Header("活動時間")]
	public float lifeTimeMin;		//活動時間の最小値
	public float lifeTimeMax;		//活動時間の最大値
	[Header("準備時間")]
	public float warmTime;			//この間は移動処理をしない
	protected bool flagWarm = true;	//フラグ
	[Header("加速倍率")]
	public float accelerationScale;			//加速倍率
	public float accelerationScaleMin = 0.5f;		//加速倍率の最低値
	public float accelerationScaleMax = 1.5f;		//加速倍率の最大値
	[Header("減速倍率")]
	public float decelerationScale;		//減速倍率
	public float decelerationScaleMin = 0.4f;		//減速倍率の最低値
	public float decelerationScaleMax = 0.6f;		//減速倍率の最大値
	[Header("最高速度")]
	public float maxVelocity;				//最高速度
	public float maxVelocityMin = 250f;			//最高速度の最低値
	public float maxVelocityMax = 300f;			//最高速度の最大値
	[Header("加減速閾値")]
	public float acceleDeccleBorder;		//回転%が閾値より低ければ加速,回転%が閾値より高ければ減速
	public float acceleDeccleBorderMin = 0.05f;	//閾値の最低値
	public float acceleDeccleBorderMax = 0.1f;	//閾値の最大値
	[Header("回転倍率")]
	public float rotAnglePer;					//一秒間にする回転の倍率
	public float rotAnglePerMin = 3f;			//回転倍率の最低値
	public float rotAnglePerMax = 4f;			//回転倍率の最大値
	protected float rotPer;					//前回の回転率
#region 関数
	//動き
	protected override void Move() {
		if(!flagWarm && target) {
			//準備期間過ぎ && target != nullなら追尾
			MoveAiming();	
		}
		//基本的に直進する
		MoveStraight();
	}
	//時間を測る
	protected override void MeasureTime() {
		base.MeasureTime();
		if(measureLifeTime >= warmTime) {
			flagWarm = false;
		} else {
			flagWarm = true;
		}		
	}
	//パラメータセット
	public override void SetParameter(GameObject owner, float velocity, float damage, float barrelBonus, float caliberBonus, Color color) {
		base.SetParameter(owner, velocity, damage, barrelBonus, caliberBonus, color);
		//誤差の計算
		CalcParameterRand();
	}
	//追尾移動
	protected void MoveAiming() {
		//目標角度の何％回転したかによって速度を変化させる
		rotPer = FuncBox.RotateToGameObject(gameObject, target, 180f, rotAnglePer);
		if(rotPer < acceleDeccleBorder) {
			//加速
			velocity += (velocity * accelerationScale) * Time.deltaTime;
		} else {
			//減速
			velocity -= (velocity * decelerationScale) * Time.deltaTime;
		}
		//最高速判定(減速にマイナスが設定してある場合もあるのでここでする)
		if(velocity >= maxVelocity) {
			velocity = maxVelocity;
		}
	}
	//ランダム誤差を計算
	protected void CalcParameterRand() {
		lifeTime = Random.Range(lifeTimeMin, lifeTimeMax);
		accelerationScale = Random.Range(accelerationScaleMin, accelerationScaleMax);
		decelerationScale = Random.Range(decelerationScaleMin, decelerationScaleMax);
		maxVelocity = Random.Range(maxVelocityMin, maxVelocityMax);
		acceleDeccleBorder = Random.Range(acceleDeccleBorderMin, acceleDeccleBorderMax);
		rotAnglePer = Random.Range(rotAnglePerMin, rotAnglePerMax);
	}
	//衝突時処理
	public override int OnHit(Object hitObject) {
		int damage =  base.OnHit(hitObject);
		return damage;
	}
#endregion
}