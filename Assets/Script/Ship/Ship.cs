using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
/// <summary>
/// 機体
/// </summary>
public class Ship : MonoBehaviour {
	//ランチャー情報クラス
	[Serializable]
	public class LauncherInfo {
		//パラメータ
		public ToolBox.Launcher launcher;		//ランチャー
		public GameObject bullet;			//インスタンス
		//コンストラクタ
		public LauncherInfo() {
			launcher = null;
			bullet = null;
		}
		public LauncherInfo(ToolBox.Launcher launcher, GameObject bullet) : this(){
			this.launcher = launcher;
			this.bullet = bullet;
		}
	}
	//ランチャー集合クラス
	[Serializable]
	public class Launchers {
		//メンバ
		public List<LauncherInfo> launchers;
		public float nowReloadTime;		//現在リロード時間
		public float maxReloadTime;		//最大リロード時間
		public int reloadCount;			//リロード済みランチャーの数
		public bool flagReload;			//リロード処理が必要か
		//コンストラクタ
		public Launchers() {
			launchers = new List<LauncherInfo>();
			nowReloadTime = 0f;
			maxReloadTime = 0f;
			reloadCount = 0;
			flagReload = true;
		}
		//関数
		//リロード時間順にソート最大リロード時間も求める
		public void SortLaunchers() {
			launchers.Sort(
				(a, b) => {
					float v = (a.launcher.totalPerformance.reloadSpeed - b.launcher.totalPerformance.reloadSpeed);
					if(v < 0) {
						return -1;
					} else if(v > 0) {
						return 1;
					} else {
						return 0;
					}
				}
			);
			//最大のリロード時間
			maxReloadTime = launchers[launchers.Count - 1].launcher.totalPerformance.reloadSpeed;
		}
	}
	//ブースター情報クラス
	[Serializable]
	public class BoosterInfo {
		//エフェクト
		public ParticleSystem boost;		//ブースト
		public ParticleSystem highBoost;	//高速ブースト
		public ParticleSystem quickBoost;	//クイックブースト
		//コンストラクタ
		public BoosterInfo() {
			boost = null;
			highBoost = null;
			quickBoost = null;
		}
		public BoosterInfo(GameObject boost, GameObject highBoost, GameObject quickBoost, Color color) {
			//色は半透明に
			color = FuncBox.SetColorAlpha(color, 0.75f);
			if(boost) {
				this.boost = boost.GetComponent<ParticleSystem>();
				this.boost.startColor = color;
			}
			if(highBoost) {
				this.highBoost = highBoost.GetComponent<ParticleSystem>();
				this.highBoost.startColor = color;
			}
			if(quickBoost) {
				this.quickBoost = quickBoost.GetComponent<ParticleSystem>();
				this.quickBoost.startColor = color;
			}
		}
	}
	//連携クラス
	protected GameManager gm;
	//ランチャー辞書
	public Dictionary<string, Launchers> launcherDic;
	//ブースターリスト
	protected List<BoosterInfo> boosterList;
	//移動用
	private Rigidbody rBody;		//物理コンポーネント
	private SphereCollider sCollider;
	[Header("基本データ")]
	public string shipName;
	public ToolBox.ShipData shipData;
	[Header("HP関連")]
	public int nowHP;
	public int maxHP;
	[Header("攻撃関連")]
	public bool flagAllReload = true;	//全体のリロードを行うか
	public float reloadTimeScale = 1f;	//全体のリロード時間の倍率
	//ショットモード
	public enum ShotMode {
		One,			//一度に一つ発射
		All,			//一度にすべて発射
	}
	public ShotMode shotMode = ShotMode.All;
	//リロードモード
	public enum ReloadMode {
		Manual,		//マニュアル(押してる間リロード)
		Auto,		//オート(自動的にリロード)
	}
	protected ReloadMode reloadMode = ReloadMode.Auto;
	[Header("ブースター関連")]
	public float boostOutput;			//通常ブースト出力
	public float highBoostOutput;			//高速ブースト出力
	public float quickBoostOutput;		//クイックブースト出力
	private float baseBoostOutput = 50f;	//基本ブースト出力
	private bool flagBoost;				//通常ブーストフラグ
	private bool flagHighBoost;			//高速ブーストフラグ
	//ブースト倍率
	[SerializeField, Range(0.1f, 20f)]
	private float boostOutputScale = 1f;
	[SerializeField, Range(0.1f, 20f)]
	private float highBoostOutputScale = 2f;
	[SerializeField, Range(0.1f, 20f)]
	private float quickBoostScale = 2f;
	//挙動
	[SerializeField, Range(0, 100)]
	protected float inertia = 1f;				//慣性
	[SerializeField, Range(0.01f, 100)]
	protected float steering = 2.5f;		//方向転換の為の減速
	protected float nowVelocity;		//現在速度
	protected float maxVelocity;		//最高速度
	[Header("エネルギー関連")]
	public float nowEnergy;				//現在エネルギー
	public float maxEnergy;				//最大エネルギー
	public float energyOutput;			//一秒間に出力するエネルギー
	public bool flagEnergyCharge = true;	//エネルギーを貯めるか
	protected float boostEnergy = 10f;			//秒間
	protected float highBoostEnergy = 200f;	//秒間
	protected float quickBoostEnergy = 200f;	//一回
	[Header("背景オブジェクト")]
	public MeshRenderer backgroundObjectRenderer;	//背景オブジェクトの描画
	[Header("その他")]
	public bool flagBreak = false;		//破壊フラグ
	public string bulletName;			//弾につける名前
	public string bulletTag;			//弾につけるタグ
	public Color symbolColor;			//色
	public float collisionRadius;		//当たり判定の半径

#region MonoBehaviourイベント
	protected void Update() {
		if(BreakCheck()) return;
		//リロード
		if(flagAllReload) {
			Reload();
		}
		//エネルギー更新
		UpdateEnergy();
		//ブースト
		Boost();
	}
#endregion
#region 有効化関連
	/// <summary>
	/// 有効化
	/// </summary>
	public void Activate(ToolBox.ShipData shipData, GameManager gm, Color symbolColor) {
		this.gm = gm;
		bulletTag = "Bullet";
		this.symbolColor = symbolColor;
		//機体名
		shipName = shipData.name;
		//機体データ
		this.shipData = shipData;
		//タグ
		gameObject.tag = "Ship";
		//機体
		Activate_Ship(shipData);
		//ランチャー
		Activate_Launcher(shipData);
		//ブースター
		Activate_Booster(shipData, gm.gameSpeed);
		//物理演算
		Activate_Rigidbody();
		//背景
		Activate_Background();
		//AP関連の準備
		maxHP = nowHP = shipData.GetHP();
		//エネルギーの準備
		maxEnergy = nowEnergy = gm.maxEnergy;
		energyOutput = gm.energyOutput;
	}
	/// <summary>
	/// 機体の有効化
	/// </summary>
	protected void Activate_Ship(ToolBox.ShipData shipData) {
		//機体パーツを結合したMeshを取得
		Mesh m = shipData.GetConnectedMesh();
		FuncBox.SetMesh(gameObject, m, gm.shipMaterial);
		//半径を求める
		Vector3 size = m.bounds.size / 2f;
		Vector3 center = FuncBox.Vector3Abs(m.bounds.center);
		collisionRadius = Vector3.Distance(Vector3.zero, center + size);
	}
	/// <summary>
	/// ランチャーの有効化
	/// </summary>
	protected void Activate_Launcher(ToolBox.ShipData ship) {
		if(launcherDic == null) launcherDic = new Dictionary<string, Launchers>();
		GameObject bullet;
		foreach(ToolBox.ShipPartsData s in ship.shipPartsList) {
			foreach(ToolBox.Launcher l in s.launcher) {
				//使用していれば辞書に登録する
				if (l.flagUse) {
					//弾のインスタンスを取得する
					ToolBox.Bullet b = gm.bulletDic[l.bulletCategory][l.bulletID];
					//インスタンスを取得する
					bullet = (GameObject)Instantiate(b.instance);
					bullet.transform.parent = transform;
					bullet.transform.localPosition = l.point + s.offset;
					bullet.transform.localEulerAngles = l.angle;
					//ランチャーの性能を計算する
					l.totalPerformance = ToolBox.LauncherPerformance.Add(l.basePerformance, b.performance);
				
					//ランチャー辞書に登録されているか確認する
					if (!launcherDic.ContainsKey(l.tag)) {
						//登録されていなければ新しく項目を作る
						launcherDic[l.tag] = new Launchers();
					}
					//追加
					launcherDic[l.tag].launchers.Add(new LauncherInfo(l, bullet));
				}
			}
		}
		//ランチャー辞書の項目毎の処理
		foreach(Launchers l in launcherDic.Values) {
			l.SortLaunchers();
		}
		//リロードを終わらせる
		ReloadComplete();
	}
	/// <summary>
	/// ブースターの有効化
	/// </summary>
	protected void Activate_Booster(ToolBox.ShipData ship, float gameSpeed) {
		if(boosterList == null) boosterList = new List<BoosterInfo>();
		//ブースター出力
		float output = baseBoostOutput;
		//エフェクト
		GameObject boost;
		GameObject highBoost;
		GameObject quickBoost;
		foreach(ToolBox.ShipPartsData s in ship.shipPartsList) {
			foreach (ToolBox.Booster b in s.booster) {
				//ブースター出力
				output += b.performance.output;
				//ブーストエフェクト
				boost = (GameObject)Instantiate(gm.GetEffect("Booster_2"));
				boost.transform.parent = transform;
				boost.transform.localPosition = b.point + s.offset;
				boost.transform.localScale = Vector3.one;
				boost.transform.localEulerAngles = b.angle;
				//ハイブーストエフェクト
				highBoost = (GameObject)Instantiate(gm.GetEffect("Booster_3"));
				highBoost.transform.parent = transform;
				highBoost.transform.localPosition = b.point + s.offset;
				highBoost.transform.localScale = Vector3.one;
				Vector3 angle = b.angle;
				angle.z -= 60;
				highBoost.transform.localEulerAngles = angle;
				//クイックブーストエフェクト
				quickBoost = (GameObject)Instantiate(gm.GetEffect("QuickBoost_1"));
				quickBoost.transform.parent = transform;
				quickBoost.transform.localPosition = b.point + s.offset;
				quickBoost.transform.localScale = Vector3.one;
				quickBoost.transform.localEulerAngles = angle;
				//追加
				boosterList.Add(new BoosterInfo(boost, highBoost, quickBoost, symbolColor));
			}
		}
		boostOutput = output * boostOutputScale * gameSpeed;
		highBoostOutput = output * highBoostOutputScale * gameSpeed;
		quickBoostOutput = output * quickBoostScale * gameSpeed;
		//エフェクトの非表示
		IndicateBoostEffect(false);
		IndicateHighBoostEffect(false);
		IndicateQuickBoostEffect(false);
	}
	/// <summary>
	/// 物理演算(剛体)の有効化
	/// </summary>
	protected void Activate_Rigidbody() {
		rBody = GetComponent<Rigidbody>();
		if (!rBody) rBody = gameObject.AddComponent<Rigidbody>();
		rBody.useGravity = false;
		rBody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
		rBody.drag = 1f;
		rBody.angularDrag = 0f;
		rBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		//当たり判定
		sCollider = gameObject.AddComponent<SphereCollider>();
		sCollider.radius = collisionRadius;
		sCollider.center = Vector3.zero;
		//物理マテリアルの設定
		sCollider.material = gm.shipPhysicsMaterial;
	}
	/// <summary>
	/// 背景オブジェクトの有効化
	/// </summary>
	protected void Activate_Background() {
		MeshRenderer mr = InstantiateBackgroundObject();
		mr.material.color = symbolColor;
		//追尾設定
		Aiming aiming = mr.gameObject.GetComponent<Aiming>();
		aiming.SetTarget(gameObject, new Vector3(0f, 0f, collisionRadius * 2f));
		//レンダラの登録
		backgroundObjectRenderer = mr;
	}
#endregion
#region 関数
	/// <summary>
	/// 破壊確認
	/// </summary>
	public bool BreakCheck() {
		return flagBreak;
	}
	/// <summary>
	/// 攻撃を受けたとき
	/// <para>APが0になった場合にfalseを返す,第二引数は実際のダメージ量</para>
	/// </summary>
	public bool Damage(int damage, out int realDamage) {
		nowHP -= damage;
		if(nowHP > 0) {
			realDamage = damage;
			//まだ生きてる
			return true;
		} else {
			realDamage = damage + nowHP;
			//もう死んでる
			nowHP = 0;
			//エフェクト非表示
			IndicateBoostEffect(false);
			IndicateHighBoostEffect(false);
			flagBreak = true;
			return false;
		}
	}
	/// <summary>
	/// 現在のHP割合
	/// </summary>
	public float GetHPPar() {
		return (float)nowHP / maxHP;
	}
	/// <summary>
	/// HPを設定&その値に全回復
	/// </summary>
	public void SetHP(int hp) {
		nowHP = maxHP = hp; 
	}
	/// <summary>
	/// タグ、名前の設定
	/// </summary>
	public void SetTagAndName(string objectName, string objectTag, string bulletName) {
		gameObject.name = objectName;
		gameObject.tag = objectTag;
		this.bulletName = bulletName;
	}
	/// <summary>
	/// 機体の削除
	/// </summary>
	public void DestroyShip() {
		Destroy(gameObject);
		Destroy(backgroundObjectRenderer.gameObject);
	}
	/// <summary>
	/// 機体をパーツとして追加する
	/// </summary>
	public void AddParts(ToolBox.ShipData addParts, Vector3 offset) {
		//先に追加分だけを追加
		//ランチャー
		Activate_Launcher(addParts);
		//ブースター
		Activate_Booster(addParts, gm.gameSpeed);
		//パーツの結合
		foreach(ToolBox.ShipPartsData parts in addParts.shipPartsList) {
			//parts.offset += offset;
			shipData.shipPartsList.Add(parts);
		}
		Activate_Ship(shipData);
		//当たり判定
		sCollider.radius = collisionRadius;
		//背景オブジェクト
		Destroy(backgroundObjectRenderer.gameObject);
		Activate_Background();
	}
	/// <summary>
	/// 背景オブジェクトの作成
	/// </summary>
	public MeshRenderer InstantiateBackgroundObject() {
		GameObject backrgoundObject = (GameObject)Instantiate(gm.playerBackground).gameObject;
		float r = collisionRadius * 0.7f;
		backrgoundObject.transform.localScale = new Vector3(r, r, r);
		return backrgoundObject.GetComponent<MeshRenderer>();
	}
	/// <summary>
	/// 現在速度を取得
	/// </summary>
	public float GetVelocity() {
		return rBody.velocity.magnitude;
	}
	/// <summary>
	/// HP回復
	/// </summary>
	public void CureHP(int cureHP) {
		nowHP += cureHP;
		if(nowHP >= maxHP) {
			nowHP = maxHP;
		}
	}
#endregion
#region 攻撃関連
	/// <summary>
	/// 発射(全て)
	/// </summary>
	public void Shot() {
		if(BreakCheck()) return;
		foreach (string category in launcherDic.Keys) {
			Shot(category);
		}
	}
	/// <summary>
	/// 発射(タグ)
	/// </summary>
	public void Shot(string tag) {
		if(BreakCheck()) return;
		if (!launcherDic.ContainsKey(tag)) return;
		Launchers l = launcherDic[tag];
		//一つ以上リロードしている事を確認
		if(l.reloadCount == 0) return;
		switch(shotMode) {
		case ShotMode.All:
			//全弾発射
			for (int i = 0; i < l.reloadCount; i++) {
				BulletInstantiate(l.launchers[i]);
			}
			//リロード初期化
			l.reloadCount = 0;
			l.flagReload = true;
			l.nowReloadTime = 0f;
			break;
		case ShotMode.One:
			BulletInstantiate(l.launchers[l.reloadCount - 1]);
			//リロード設定
			l.reloadCount--;
			l.flagReload = true;
			l.nowReloadTime = l.launchers[l.reloadCount].launcher.totalPerformance.reloadSpeed;
			break;
		}
	}
	/// <summary>
	/// 弾生成
	/// </summary>
	protected void BulletInstantiate(LauncherInfo l) {
		//生成
		var b = (GameObject)Instantiate(l.bullet);
		b.transform.position = l.bullet.transform.position;
		b.transform.eulerAngles = l.bullet.transform.eulerAngles;
		//アクティブ化
		if (b.activeInHierarchy == false) {
			b.SetActive(true);
		}
		//名前とタグ
		b.name = bulletName;
		b.tag = bulletTag;

		//Bulletコンポーネントを取得する
		var bullet = b.GetComponent<Bullet>();
		bullet.SetParameter(gameObject, l.launcher.totalPerformance.velocity, l.launcher.totalPerformance.damage, l.launcher.barrelBonus, l.launcher.caliberBonus, symbolColor);
	}
	/// <summary>
	/// リロード
	/// </summary>
	protected void Reload() {
		if(BreakCheck()) return;
		float deltaTime = Time.deltaTime * reloadTimeScale;
		foreach(Launchers l in launcherDic.Values) {
			if(!l.flagReload) continue;
			//リロードが必要な場合のみ
			if(l.reloadCount < l.launchers.Count) {
				l.nowReloadTime += deltaTime;
				//ランチャーのリロード時間が経過しているか
				for(int i = l.reloadCount; i < l.launchers.Count; i++) {
					if(l.launchers[i].launcher.totalPerformance.reloadSpeed <= l.nowReloadTime) {
						l.reloadCount = i + 1;
						//全てのリロードが完了した場合
						if(l.reloadCount == l.launchers.Count) {
							l.nowReloadTime = l.maxReloadTime;
							l.flagReload = false;
						}
					} else {
						//満たしていない場合はbreak
						break;
					}
				}
			}
		}
	}
	/// <summary>
	/// リロード開始/終了
	/// </summary>
	public void SetReload(string tag, bool flag) {
		if(!launcherDic.ContainsKey(tag)) return;
		//ショットモードによって動作を変える
		switch(reloadMode) {
			case ReloadMode.Manual:
				//マニュアル
				if(flag) {
					//リロード開始
					launcherDic[tag].flagReload = true;
				} else {
					//リロード終了すなわち発射
					Shot(tag);
				}
			break;
			case ReloadMode.Auto:
				//オート
				if(flag) {
					Shot(tag);
				}
			break;
		}
	}
	/// <summary>
	/// 全てのリロードを強制的に完了させる
	/// </summary>
	public void ReloadComplete() {
		foreach(Launchers l in launcherDic.Values) {
			l.flagReload = false;
			l.nowReloadTime = l.maxReloadTime;
			l.reloadCount = l.launchers.Count;
		}
	}
	/// <summary>
	/// 攻撃関連のコンフィグ設定
	/// </summary>
	public void SetAttackConfig(ShotMode shotMode, ReloadMode reloadMode) {
		this.shotMode = shotMode;
		this.reloadMode = reloadMode;
	}
	/// <summary>
	/// 指定タグのリロードカウントを指定数増やす
	/// </summary>
	public void AddReloadCount(string tag, int num) {
		//辞書確認
		if(!launcherDic.ContainsKey(tag)) return;
		Launchers l = launcherDic[tag];
		//リロードカウント増加
		l.reloadCount += num;
		//最大値に達した場合
		if(l.launchers.Count <= l.reloadCount) {
			l.reloadCount = l.launchers.Count;
			l.nowReloadTime = l.maxReloadTime;
			l.flagReload = false;
		} else {
			l.nowReloadTime = l.launchers[l.reloadCount - 1].launcher.totalPerformance.reloadSpeed;
		} 
	}
#endregion
#region 移動関連
	/// <summary>
	/// ブースト
	/// <para>通常、高速ブーストのフラグによって出力を変える</para>
	/// </summary>
	public void Boost() {
		if(BreakCheck()) return;
		if(boostOutput <= 0f) return;
		if(flagBoost) {
			Vector3 v;		//移動量
			float e;			//使用エネルギー(秒間)
			if(flagHighBoost) {
				v = transform.right * highBoostOutput * Time.deltaTime;
				e = highBoostEnergy * Time.deltaTime;	
			} else {
				v = transform.right * boostOutput * Time.deltaTime;
				e = boostEnergy * Time.deltaTime;	
			}
			//移動
			if(!AddVelocity(v, e)) {
				//エネルギーが足りないのでエフェクトを非表示に
				IndicateBoostEffect(false);
				IndicateHighBoostEffect(false);
			}
		} else {
			//ブーストしてないなら慣性を掛ける
			rBody.velocity = FuncBox.Vector3Lerp(rBody.velocity, Vector3.zero, inertia * Time.deltaTime);
		}
	}
	/// <summary>
	/// クイックブースト
	/// </summary>
	public void QuickBoost() {
		if(BreakCheck()) return;
		//現在の速度の方向を確認
		float nowAngle = FuncBox.TwoPointAngleD(Vector3.zero, transform.right);
		float velocityAngle = FuncBox.TwoPointAngleD(Vector3.zero, rBody.velocity);
		float diff = Mathf.Abs(Mathf.DeltaAngle(nowAngle, velocityAngle));
		//差が一定以上なら速度をほぼにする
		if(diff >= 45f) rBody.velocity *= 0.2f;
		Vector3 v = transform.right * quickBoostOutput;		//移動量
		float e = quickBoostEnergy;						//使用エネルギー
		if(!AddVelocity(v, e)) return;
		//エフェクト
		IndicateQuickBoostEffect(true);
	}
	/// <summary>
	/// 角度移動
	/// </summary>
	public void MoveAngle(Vector2 direction) {
		if(BreakCheck()) return;
		float angle = FuncBox.TwoPointAngleD(Vector2.zero, direction);
		//速度ベクトルの向きを変更
		float fromAngle = FuncBox.TwoPointAngleD(Vector2.zero, rBody.velocity);
		float angleDistance = Mathf.Abs(Mathf.DeltaAngle(angle, fromAngle));
		//角度差が一定範囲の場合でブーストしている場合
		if(30 < angleDistance && angleDistance <= 170f && flagBoost) {
			float lerpAngle = Mathf.LerpAngle(fromAngle, angle, steering * Time.deltaTime);
			Vector3 velocityAngle = FuncBox.DegreeToVector3(lerpAngle);
			//速度の向き変更
			rBody.velocity = velocityAngle * (rBody.velocity.magnitude);
		}
		//角度を設定
		transform.eulerAngles = new Vector3(0f, 0f, angle);
	}
	/// <summary>
	/// エネルギーを消費して速度を加える
	/// </summary>
	protected bool AddVelocity(Vector3 addVelocity, float useEnergy) {
		if(!UseEnergy(useEnergy)) return false;
		rBody.velocity += addVelocity;
		return true;
	}
	/// <summary>
	/// 通常ブーストスイッチ
	/// <para>オン(true)/オフ(false)</para>
	/// </summary>
	public void BoostSwitch(bool flag) {
		if(BreakCheck()) return;
		if(flagBoost == flag) return;
		flagBoost = flag;
		if(flag) {
			//エフェクトの表示
			IndicateBoostEffect(true);
		} else {
			//高速ブーストフラグを下す
			HighBoostSwitch(false);
			//エフェクトの非表示
			IndicateBoostEffect(false);
		}
	}
	/// <summary>
	/// 高速ブーストスイッチ
	/// <para>オン(true)/オフ(false)</para>
	/// </summary>
	public void HighBoostSwitch(bool flag) {
		if(BreakCheck()) return;
		if(flagHighBoost == flag) return;
		flagHighBoost = flag;
		if(flagHighBoost) {
			//通常ブースト
			BoostSwitch(true);
			//エフェクトオン
			IndicateHighBoostEffect(true);
			//エネルギーチャージオフ
			flagEnergyCharge = false;
		} else {
			//通常ブースト
			BoostSwitch(false);
			//エフェクトオフ
			IndicateHighBoostEffect(false);
			//エネルギーチャージオン
			flagEnergyCharge = true;
		}
	}
#endregion
#region ブースター関連
	/// <summary>
	/// ブーストエフェクトのオン/オフ
	/// </summary>
	public void IndicateBoostEffect(bool flag) {
		if(flag) {
			for(int i = 0; i < boosterList.Count; i++) {
				boosterList[i].boost.Play();
			}
		} else {
			for(int i = 0; i < boosterList.Count; i++) {	
				boosterList[i].boost.Stop();
			}
		}
	}
	/// <summary>
	/// ハイブーストエフェクトのオン/オフ
	/// </summary>
	public void IndicateHighBoostEffect(bool flag) {
		if(flag) {
			for(int i = 0; i < boosterList.Count; i++) {	
				boosterList[i].highBoost.Play();
			}
		} else {
			for(int i = 0; i < boosterList.Count; i++) {	
				boosterList[i].highBoost.Stop();
			}
		}
	}
	/// <summary>
	/// クイックブーストエフェクトのオン/オフ
	/// </summary>
	protected void IndicateQuickBoostEffect(bool flag) {
		if(flag) {
			for(int i = 0; i < boosterList.Count; i++) {	
				boosterList[i].quickBoost.Play();
			}
		} else {
			for(int i = 0; i < boosterList.Count; i++) {	
				boosterList[i].quickBoost.Stop();
			}
		}
	}
#endregion
#region エネルギー関連関数
	/// <summary>
	/// エネルギー更新
	/// </summary>
	protected void UpdateEnergy() {		
		if(flagEnergyCharge) {
			nowEnergy += energyOutput * Time.deltaTime;

			if(nowEnergy >maxEnergy) {
				nowEnergy = maxEnergy;
			}
		}
	}
	/// <summary>
	/// エネルギー使用
	/// </summary>
	protected bool UseEnergy(float useEnergy) {
		if(nowEnergy - useEnergy >= 0f) {
			//エネルギーを消費する
			nowEnergy -= useEnergy;
			return true;
		} else {
			//エネルギーを消費しない
			return false;
		}
	}
#endregion
#region 背景オブジェクト関連
	/// <summary>
	/// 背景オブジェクトの色を設定
	/// </summary>
	public void SetBackgroundColor(Color c) {
		if(backgroundObjectRenderer) {
			backgroundObjectRenderer.material.color = c;
		}
	}
	/// <summary>
	/// 背景オブジェクトにシンボルカラーを設定
	/// </summary>
	public void SetBackgroundSymbolColor() {
		SetBackgroundColor(symbolColor);
	}
#endregion
}