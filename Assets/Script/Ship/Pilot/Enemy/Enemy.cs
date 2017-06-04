using UnityEngine;
using System.Collections;
/// <summary>
/// 敵。単純な敵
///<para>近くに寄って弾を撃つだけ</para>
/// </summary>
public class Enemy : Pilot {
	[Header("エネミー")]
	public float subUpdateInterval = 2f;
	[Header("パラメータ")]
	public int strongLevel;
	public int dropItemNum;
#region MonoeBehaviourイベント
	protected override void Start() {
		base.Start();
		//サブアップデートスタート
		StartCoroutine(Coroutine_SubUpdate());
	}
	protected override void Update() {
		if(flagControl) {
			Move();
		}
	}
#endregion
#region 関数
	/// <summary>
	/// 有効化
	/// </summary>
	public override void Activate(GameManager gm, string objectName, string objectTag) {
		base.Activate(gm, objectName, objectTag);
	}
	/// <summary>
	/// パラメータの設定
	/// </summary>
	public void SetParametor(int strongLevel, int hp, int dropItemNum) {
		this.strongLevel = strongLevel;
		subUpdateInterval /= (float)strongLevel;
		this.dropItemNum = dropItemNum;
		ship.SetHP(hp);
	}
	/// <summary>
	/// 行動
	/// </summary>
	protected override void Move() {
		if(lockObject) {
			//locktargetへの向きベクトルを取得。
			Vector2 direction = (lockObject.transform.position - transform.position).normalized;
			//directionの方向を向く
			ship.MoveAngle(direction);
			//距離を計測
			float distance = Vector3.Distance(lockObject.transform.position, transform.position);
			//一定値以上なら移動
			if(distance >= 20f) {
				ship.BoostSwitch(true);
			} else {
				ship.BoostSwitch(false);
			}
		} else {
			//何もしない
		}
	}
	/// <summary>
	/// 攻撃
	/// </summary>
	protected override void Attack() {
		//リロード可なら攻撃
		ship.Shot();
	}
#endregion
#region サブアップデート
	/// <summary>
	/// サブアップデートコルーチン
	/// </summary>
	IEnumerator Coroutine_SubUpdate() {
		yield return new WaitForSeconds(0.5f);
		while(true) {
			if(!ship.BreakCheck() && flagControl) {
				SubUpdate();
			}
			yield return new WaitForSeconds(subUpdateInterval);
		}
	}
	/// <summary>
	/// サブアップデート関数
	/// </summary>
	protected virtual void SubUpdate() {
		//ロック対象がいなければ探す
		ship.QuickBoost();
		if(!lockObject) {
			lockObject = sm.GetNearPlayer(this);
		}
		Attack();
	}
#endregion
#region ロック関連
	//ロック
	public override void Lock() {
		//base.Lock();
	}
#endregion
#region 当たり判定関連
	/// <summary>
	/// 破壊されたとき
	/// </summary>
	protected override void OnBreak(GameObject rival) {
 		base.OnBreak(rival);
		//ラベル表示
		LabelManager.Instance.SetLabel("Break!", 4f, transform.position, Color.red, Vector3.one * 16f);
	}
#endregion
}