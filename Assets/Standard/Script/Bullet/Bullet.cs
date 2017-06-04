using UnityEngine;
using System.Collections;
/// <summary>
/// 弾
/// </summary>
public class Bullet : MonoBehaviour {
	[Header("持ち主")]
	public GameObject owner;
	[Header("タグ")]
	public string hitWithDeathTag = "Stage";	//当たると死亡するタグ
	[Header("Bulletパラメータ")]
	public float lifeTime;				//弾が生きてる時間
	protected float measureLifeTime = 0f;	//計測している時間
	public float hitForce = 1000f;			//衝撃
	public float barrelBonus;				//砲身ボーナス
	public float caliberBonus;			//口径ボーナス
	public float velocity;				//移動速度
	public float damage;				//ダメージ
	[Header("見た目")]
	public SpriteRenderer sprite;			//スプライト
	[Header("軌跡エフェクト")]
	public GameObject trailEffectPrefab;		//軌跡エフェクトのプレハブ
	protected ParticleSystem trailEffect;		//エフェクト
	[Header("エフェクト")]
	public BulletShotEffect shotEffect;			//撃ち出し
	public AnimationController destroyEffect;	//削除
	[Header("サウンド")]
	public string shotSE;			//効果音名
	protected AudioManager audio;	//効果音再生
#region MonoBehaviourイベント
	protected virtual void Start() {
		audio = AudioManager.Instance;
		//再生
		audio.PlaySE(shotSE);
	}
	protected void FixedUpdate() {
		//移動
		Move();
		//時間を計測する
		MeasureTime();
	}
	protected virtual void OnTriggerEnter(Collider c) {
		//触れたら死ぬタグか
		if(c.tag != hitWithDeathTag) return;
		OnDestroyer();
	}
#endregion
#region 関数
	//動き
	protected virtual void Move() {
		MoveStraight();
	}
	//時間を測る
	protected virtual void MeasureTime() {
		measureLifeTime += Time.deltaTime;
		//一定時間が来たら
		if (measureLifeTime >= lifeTime) {
			//イベント
			OnDestroyer();
		}
	}
	//パラメータセット
	public virtual void SetParameter(GameObject owner, float velocity, float damage, float barrelBonus, float caliberBonus, Color color) {
		//持ち主設定
		this.owner = owner;
		
		//パラメータ設定
		this.velocity = velocity;
		this.damage = damage;
		this.barrelBonus = barrelBonus;
		this.caliberBonus = caliberBonus;
		//色設定
		sprite.color = color;

		//軌跡エフェクト生成
		if(trailEffectPrefab) {
			GameObject trail = (GameObject)Instantiate(trailEffectPrefab);
			trail.transform.parent = transform;
			trail.transform.localPosition = Vector3.zero;
			trailEffect = trail.GetComponent<ParticleSystem>();
			if(trailEffect) {
				trailEffect.startColor = FuncBox.SetColorAlpha(color, 0.75f);
			}
			trail.AddComponent<ParticleEndWithDestroy>();
		}
		//ボーナス効果設定
		SetBonusEffect();
	}
	//ボーナス効果設定
	protected virtual void SetBonusEffect() {
		//大きさ変更
		//float scale = bonus / 2f + 1;
		//transform.localScale *= scale;
	}
	//直線移動
	protected void MoveStraight() {
		//自分の向いている方向に移動する
		var direction = transform.right;	//角度をsin,cosで分解するよりも早い
		transform.position += direction * Time.deltaTime * velocity;
	}
	//弾生成
	protected Bullet InstantiateBullet(GameObject instantBullet) {
		//生成
		var g = (GameObject)Instantiate(instantBullet.gameObject);
		g.transform.eulerAngles = transform.eulerAngles;
		g.transform.position = transform.position;
			
		//弾設定
		Bullet b = g.GetComponent<Bullet>();
		b.SetParameter(owner, velocity, damage, barrelBonus, caliberBonus, sprite.color);
		b.name = name;
		b.tag = tag;

		return b;
	}
	//衝突処理(ぶつかった相手はこれを呼んであげること)
	//ダメージを返す
	public virtual int OnHit(Object hitObject) {
		//自身を削除		
		OnDestroyer();

		return (int)damage;
	}
	//削除するイベント
	public virtual void OnDestroyer() {
		Destroy(gameObject);
		//軌跡エフェクト
		if(trailEffect) {
			//軌跡の親子関係を切る
			trailEffect.transform.parent = null;
			//エフェクトストップ
			trailEffect.Stop();
		}
		//削除時エフェクトを生成
		if(destroyEffect) {
			GameObject g = (GameObject)Instantiate(destroyEffect.gameObject);
			//座標
			g.transform.position = transform.position;
			//角度
			g.transform.eulerAngles = transform.eulerAngles;
			//色
			g.GetComponent<AnimationController>().sprite.color = sprite.color;
		}
	}
#endregion
}