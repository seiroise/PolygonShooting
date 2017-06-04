using UnityEngine;
using System.Collections;
/// <summary>
/// 操縦者
/// </summary>
public class Pilot : MonoBehaviour {
	//管理クラス
	protected StageManager sm;			//ステージ管理クラス
	protected GameManager gm;			//ゲーム管理クラス
	[Header("パラメータ")]
	public Ship ship;					//機体クラス
	public bool flagControl = true;		//操作フラグ
	protected bool flagBoost = false;
	[Header("ロック")]
	public Pilot lockObject;				//ロック対象
	[Header("当たり判定関連")]
	public string notCollisionName;		//当たり判定を行わないオブジェクト名
	[Header("エフェクト")]
	public ParticleSystem breakParticle;	//破壊時エフェクト
#region MonoBehaviourイベント
	protected virtual void Start() {
		sm = StageManager.Instance;
	}
	protected virtual void Update() {
		if(flagControl) {
			//ロック
			Lock();
			//移動
			Move();
			//攻撃
			Attack();
		}
	}
	protected void OnTriggerStay(Collider c) {
		if(ship.BreakCheck()) return;
		//名前確認
		if(notCollisionName.Equals(c.name)) return;
		//タグによって処理変更
		switch(c.tag) {
		case "Bullet":
			//弾用コンポーネントを取得
			Bullet b = c.GetComponent<Bullet>();
			if(b == null) return;
			//あとはHit関数に任せる
			OnBulletHit(b);
			break;
		case "Item":
			break;
		}
	}
#endregion
#region 関数
	/// <summary>
	/// 有効化
	/// </summary>
	public virtual void Activate(GameManager gm, string objectName, string objectTag) {
		this.gm = gm;
		//ShipControllerを取得
		ship = gameObject.GetComponent<Ship>();
		//当たり判定の設定
		ship.SetTagAndName(objectName, objectTag, objectName);
		//当たり判定を行わないオブジェクト名の設定
		notCollisionName = objectName;
	}
	/// <summary>
	/// 移動
	/// </summary>
	protected virtual void Move() {
		
	}
	/// <summary>
	/// 攻撃
	/// </summary>
	protected virtual void Attack() {
		
	}
	/// <summary>
	/// 復活
	/// </summary>
	public virtual void Reborn(Vector3 respawnPos) {
		//背景オブジェクトの色を元の色に戻す
		ship.SetBackgroundSymbolColor();
		//操作フラグ
		flagControl = true;
		//機体の初期化
		ship.flagBreak = false;
		ship.nowHP = ship.maxHP;
		ship.nowEnergy = ship.maxEnergy;
		//座標
		transform.position = respawnPos;
	}
#endregion
#region ロック関連
	/// <summary>
	/// ロック
	/// </summary>
	public virtual void Lock() {
		if(lockObject) {
			lockObject = sm.GetNextLockObject(this, lockObject);
		} else {
			lockObject = sm.GetNextLockObject(this, this);
		}
		if(lockObject == this) lockObject = null;
	}
	/// <summary>
	/// ロック解除
	/// </summary>
	public virtual void LockClear() {
		lockObject = null;
	}
#endregion
#region 当たり判定関連
	/// <summary>
	/// 弾との衝突処理、ダメージ量を返す
	/// </summary>
	protected virtual int OnBulletHit(Bullet b) {
		//衝撃処理
		HitForce(b);
		//弾の衝突処理を呼ぶ
		int hitDamage = b.OnHit(ship);
		//ダメージ処理
		int realDamage;
		if(!ship.Damage(hitDamage, out realDamage)) {
			//破壊処理
			OnBreak(b.owner);
		} else {
			//ラベル表示
			LabelManager.Instance.SetLabel(realDamage.ToString(), 1f, transform.position);
		}
		return realDamage;
	}
	/// <summary>
	/// 破壊処理
	/// </summary>
	protected virtual void OnBreak(GameObject rival) {
		//背景オブジェクトの色を灰色に
		ship.SetBackgroundColor(Color.gray);
		//ステージ管理クラスに死亡を告げる
		//相手が破壊されたが弾は残っている可能性があるので
		Pilot rivalPilot = null;
		if(rival) {
			rivalPilot = rival.GetComponent<Pilot>();
		}
		sm.OnStageObjectBreak(this, rivalPilot);
	}
	/// <summary>
	/// 反動処理
	/// </summary>
	protected void HitForce(Bullet b) {
		//弾との向きを取得
		Vector3 direction = transform.position - b.transform.position;
		//力を加える
		//ship.MoveForce(direction, b.hitForce);
	}
#endregion
}