using UnityEngine;
using System.Collections;
/// <summary>
/// 光線を撃つ弾
/// </summary>
public class RayShotBullet : Bullet {
	[Header("OpticalBulletパラメータ")]
	public RayBullet rayBulletPrefab;		//光線
	private Bullet rayBullet;
	
#region MonoBehaviourイベント
	protected override void Start() {
		base.Start();
		//生成
		rayBullet = InstantiateBullet(rayBulletPrefab.gameObject);
		rayBullet.lifeTime = lifeTime;
	}
#endregion
#region 関数
	public override void SetParameter(GameObject owner, float velocity, float damage, float barrelBonus, float caliberBonus, Color color) {
		base.SetParameter(owner, velocity, damage, barrelBonus, caliberBonus, color);
		//固定
		this.velocity = 0f;
	}
	public override void OnDestroyer(){
 		base.OnDestroyer();
		//Destroy時エフェクトを光線の位置に生成

	}
#endregion
}