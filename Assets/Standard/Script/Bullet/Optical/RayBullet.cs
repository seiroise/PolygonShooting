using UnityEngine;
using System.Collections;
/// <summary>
/// 光線。一定時間で大きさを指定の大きさにするだけ
/// </summary>
public class RayBullet : Bullet {
	[Header("Rayパラメータ")]
	public float width;				//厚み
	public float rangeVelocity;		//伸びる速度
	protected float range = 0;		//長さ
	protected bool flagStop = false;	//伸びるのを止めるか
	[Header("エフェクト")]
	public ParticleSystem headEffectPrefab;		//光線の先頭のエフェクト
	private ParticleSystem headEffect;

#region MonoeBehaviourイベント
	protected override void Start() {
		base.Start();
		//先頭エフェクトの作成
		if(headEffectPrefab) {
			GameObject g = (GameObject)Instantiate(headEffectPrefab.gameObject, transform.position, Quaternion.identity);
			g.transform.parent = transform;
			g.transform.localPosition = Vector3.right;
			headEffect = g.GetComponent<ParticleSystem>();
			g.AddComponent<ParticleEndWithDestroy>();
			headEffect.startColor = sprite.color;
		}
	}
	protected override void OnTriggerEnter(Collider c) {
		if(c.tag == hitWithDeathTag) {
			flagStop = true;
			//生成エフェクトの調整
			headEffect.emissionRate = 10f;
			headEffect.startSize *= 2f;
			headEffect.startSpeed *= 0.25f;
		}
	}
#endregion
#region 関数
	public override void SetParameter(GameObject owner, float velocity, float damage, float barrelBonus, float caliberBonus, Color color) {
		base.SetParameter(owner, velocity, damage, barrelBonus, caliberBonus, color);
		//速度は0固定
		this.velocity = 0f;
	}
	protected override void MeasureTime() {
		base.MeasureTime();
		Vector3 size = transform.localScale;
		if(!flagStop) {
			//光線を伸ばす
			size.x = range += rangeVelocity * Time.deltaTime;
		}
		//y軸の厚さ
		size.y = width * (1- (measureLifeTime / lifeTime));
		transform.localScale = size;
		//パーティクルのサイズ(常に1になるように)
		Vector3 lossScale = headEffect.transform.lossyScale;
		Vector3 localScale = headEffect.transform.localScale;
		headEffect.transform.localScale = new Vector3(
			localScale.x / lossScale.x,
			localScale.y / lossScale.y,
			localScale.z / lossScale.z
		);
	}
	public override int OnHit(Object hitObject) {
		//時間が来るまで自身を削除しない
		//継続ダメージなので一秒間のダメージに直す
		return (int)(damage * Time.deltaTime);
	}
	public override void OnDestroyer() {
		base.OnDestroyer();
		//先頭エフェクト
		if(headEffect) {
			//先頭エフェクトの親子関係を切る
			headEffect.transform.parent = null;
			//エフェクトストップ
			headEffect.Stop();
			int num = 75;
			Vector3 offset = (headEffect.transform.position - transform.position) / num;
			float speed = width;
			for(int i = 0; i < num; i++) {
				headEffect.Emit(transform.position + (offset * i),
					FuncBox.GetRandomVector2(-speed, speed), width * 0.5f, Random.Range(0.5f, 1f), headEffect.startColor);
			}
		}
	}
#endregion
}