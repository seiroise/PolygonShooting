using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GamepadInput;
/// <summary>
/// プレイヤー
/// </summary>
public class Player : Pilot {
	[Header("UI")]
	public StagePlayerIndicator_Single indicator;
	[Header("操作")]
	public ToolBox.InputType inputType;	//入力の種類
	public bool flagInputTypeHold = false;	//入力種類の固定
	public GamePad.Index playerNo;		//プレイヤー番号
	[Header("エフェクト")]
	public ParticleSystem breakEffect;		//破壊時エフェクト
	//取得アイテム
	private List<string> haveItemList;
#region MonoBehaviourイベント
	protected override void Start() {
		base.Start();
		haveItemList = new List<string>();
	}
	protected override void Update() {
		if(flagControl) {
			//パッドチェック
			GamePadCheck();
			//ロック
			Lock();
			//移動
			Move();
			//攻撃
			Attack();
			//その他入力
			OtherInput();
		}
	}
	protected void OnCollisionEnter(Collision c) {
		//体当たり
		if(c.gameObject.name != notCollisionName && c.gameObject.tag != "Stage") {
			Debug.Log("1");
			//速度を比べる
			if(c.rigidbody) {
				Debug.Log("2");
				if(c.rigidbody.velocity.magnitude > ship.GetVelocity()) {
					//自分にダメージを加える
					int realDamage;
					if(!ship.Damage((int)ship.GetVelocity(), out realDamage)) {
						//破壊処理
						OnBreak(c.gameObject);
					} else {
						//ラベル表示
						LabelManager.Instance.SetLabel(realDamage.ToString(), 1f, transform.position);
					}
				}
			}
		}
		//回復
		if(c.gameObject.tag == "Player"){
			if(gm.playMode == ToolBox.PlayMode.VsEnemy) {
				Debug.Log("3");
				//もしも戦闘不能ならば回復
				if(ship.BreakCheck()) {
					Debug.Log("4");
					sm.PlayerReborn(this);
					int hp = (int)(ship.maxHP / 10f);
					if(hp <= 0) {
						hp = 10;
					}
					ship.nowHP = hp;
				}
			}
		}
	}
#endregion
#region 関数
	/// <summary>
	/// 有効化
	/// </summary>
	public override void Activate(GameManager gm, string objectName, string objectTag) {
		base.Activate(gm, objectName, objectTag);
		//HPに倍率を掛ける
		ship.nowHP = ship.maxHP = (int)(ship.nowHP * gm.playerHPScale);
	}
	/// <summary>
	/// ゲームパッド設定
	/// </summary>
	public void SetGamePad(int number, ToolBox.InputType inputType, bool flagInputTypeHold) {
		//プレイヤー番号
		switch(number) {
			case(0) : playerNo = GamePad.Index.Any;	break;
			case(1) : playerNo = GamePad.Index.One;	break;
			case(2) : playerNo = GamePad.Index.Two;	break;
			case(3) : playerNo = GamePad.Index.Three;	break;
			case(4) : playerNo = GamePad.Index.Four;	break;
		}
		//その他の設定
		this.inputType = inputType;
		this.flagInputTypeHold = flagInputTypeHold;
	}
	/// <summary>
	/// 移動
	/// </summary>
	protected override void Move() {
		//移動
		Vector2 direction = Vector2.zero;
		switch (inputType) {
			case ToolBox.InputType.Mouse :
				//クイックブースト
				if(Input.GetMouseButtonDown(0)) {
					//クイックブースト
					ship.QuickBoost();
					//高速ブーストスイッチ
					ship.HighBoostSwitch(true);
				} else if(Input.GetMouseButtonUp(0)) {
					//高速ブーストスイッチ
					ship.HighBoostSwitch(false);
				}
				//向き
				direction = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
				break;
			case ToolBox.InputType.GamePad :
				//ブースト
				if(GamePad.GetButtonDown(GamePad.Button.LeftShoulder, playerNo)) {
					//クイックブースト
					ship.QuickBoost();
					//高速ブーストスイッチ
					ship.HighBoostSwitch(true);
				} else if(GamePad.GetButtonUp(GamePad.Button.LeftShoulder, playerNo)){
					//高速ブーストスイッチ
					ship.HighBoostSwitch(false);
				}
				//向き
				direction = GamePad.GetAxis(GamePad.Axis.LeftStick, playerNo);
				break;
		}
		//向きのmagnitudeが0.2未満
		if (direction.magnitude < 0.2f) {
			//通常ブーストスイッチ
			ship.BoostSwitch(false);
			//lockTargetがnullでないならlockTargetの方向を向く
			 if(lockObject) {
				//locktargetへの向きベクトルを取得。
				direction = (lockObject.transform.position - transform.position).normalized;
				//directionの方向を向く
				ship.MoveAngle(direction);
			 }
		} else {
			//通常ブーストスイッチ
			ship.BoostSwitch(true);
			//directionの方向を向く
			ship.MoveAngle(direction);
		}
	}
	/// <summary>
	/// 攻撃
	/// </summary>
	protected override void Attack() {
		if (ship) {
			//Zまたはマウスクリック,L1,R1
			switch (inputType) {
				case ToolBox.InputType.Mouse:
					//X
					if(Input.GetKeyDown(KeyCode.X)) {
						ship.SetReload("Normal", true);
						indicator.OnShotNormal();
					} else if(Input.GetKeyUp(KeyCode.X)) {
						ship.SetReload("Normal", false);
					}
					//Z
					if(Input.GetKeyDown(KeyCode.Z)) {
						ship.SetReload("Missile", true);
						indicator.OnShotMissile();
					} else if(Input.GetKeyUp(KeyCode.Z)) {
						ship.SetReload("Missile", false);
					}
					//C
					if(Input.GetKeyDown(KeyCode.C)) {
						ship.SetReload("Optical", true);
						indicator.OnShotOptical();
					} else if(Input.GetKeyUp(KeyCode.C)) {
						ship.SetReload("Optical", false);
					}
					break;
				case ToolBox.InputType.GamePad:
					//A
					if(GamePad.GetButtonDown(GamePad.Button.A, playerNo)) {
						ship.SetReload("Normal", true);
						indicator.OnShotNormal();
					} else if(GamePad.GetButtonUp(GamePad.Button.A, playerNo)) {
						ship.SetReload("Normal", false);
					}
					//X
					if(GamePad.GetButtonDown(GamePad.Button.X, playerNo)) {
						ship.SetReload("Missile", true);
						indicator.OnShotMissile();
					} else if(GamePad.GetButtonUp(GamePad.Button.X, playerNo)) {
						ship.SetReload("Missile", false);
					}
					//B
					if(GamePad.GetButtonDown(GamePad.Button.B, playerNo)) {
						ship.SetReload("Optical", true);
						indicator.OnShotOptical();
					} else if(GamePad.GetButtonUp(GamePad.Button.B, playerNo)) {
						ship.SetReload("Optical", false);
					}
					break;
				default : 
					break;
			}
		}
	}
	/// <summary>
	/// その他諸々の入力処理など
	/// </summary>
	protected void OtherInput() {
		switch(inputType) {
			case ToolBox.InputType.Mouse:
				if(Input.GetKeyDown(KeyCode.Space)) {
					indicator.OnRightShoulder();
				}
			break;
			case ToolBox.InputType.GamePad:
				if(GamePad.GetButtonDown(GamePad.Button.RightShoulder, playerNo)) {
					indicator.OnRightShoulder();
				}
			break;
		}
		
	}
	/// <summary>
	/// パッド接続確認
	/// </summary>
	protected void GamePadCheck() {
		if (!flagInputTypeHold) {
			string[] joyNames = Input.GetJoystickNames();
			if (joyNames.Length == 0) {
				inputType = ToolBox.InputType.Mouse;
			} else {
				inputType = ToolBox.InputType.GamePad;
			}
		}
	}
	/// <summary>
	/// アイテム取得
	/// </summary>
	public void GetItem(UISprite itemSprite, string itemName) {
		//所持リストに追加
		haveItemList.Add(itemName);
		//SE再生
		AudioManager.Instance.PlaySE("ItemGet_1");
		//UIに反映
		if(indicator.OnGetItem(itemSprite.spriteName, itemSprite.color)) {
			//溜まったので新しくパーツをくっつける
			//とりあえずランダムに
			ToolBox.ShipData addParts = GameManager.Instance.ShipDic_RandomSelect();
			ship.AddParts(addParts, Vector3.up * 2f);
			//表示
			int total, normal, missile, optical;
			addParts.GetLauncherNum(out total, out normal, out missile, out optical);
			indicator.IndicateLvUp(0, normal, missile, optical);
			//アイテムストック数を増やす
			indicator.itemList.AddItemNum();
			//HP全回復
			ship.SetHP(ship.maxHP);
		} else {
			//ラベル表示
			LabelManager.Instance.SetLabel("ItemGet!", 2f, transform.position, itemSprite.color, Vector3.one * 8f);
		}
	}
	/// <summary>
	/// 復活
	/// </summary>
	public override void Reborn(Vector3 respawnPos) {
		base.Reborn(respawnPos);
		//表示を更新
		indicator.OnReborn();
	}
#endregion
#region ロック関連
	/// <summary>
	/// ロック
	/// </summary>
	public override void Lock() {
		//入力
		bool flag = false;
		switch (inputType) {
			case ToolBox.InputType.Mouse :
				//V
				if(Input.GetKeyDown(KeyCode.V)) {
					flag = true;
				}
			break;
			case ToolBox.InputType.GamePad :
				//Y or R1
				if(GamePad.GetButtonDown(GamePad.Button.Y, playerNo) || GamePad.GetButtonDown(GamePad.Button.RightShoulder, playerNo)) {
					flag = true;
				}
			break;
		}
		if(flag) {
			switch(gm.playMode) {
				case ToolBox.PlayMode.Battle:
					base.Lock();
				break;
				case ToolBox.PlayMode.VsEnemy:
					lockObject = sm.GetNearEnemy(this);
				break;
			}
			//表示に通知
			indicator.OnLockObjectChange();
		}
	}
	/// <summary>
	/// ロック解除
	/// </summary>
	public override void LockClear() {
		//プレイモードがVsEnemyなら新しく近くの敵を検索する
		if(gm.playMode == ToolBox.PlayMode.VsEnemy) {
			lockObject = sm.GetNearEnemy(this);
			//表示に通知
			Debug.Log(lockObject);
			if(lockObject) {
				indicator.OnLockObjectChange();
			} else {
				base.LockClear();
				//表示に通知
				indicator.OnUnLock();
			}
		} else {
			base.LockClear();
			//表示に通知
			indicator.OnUnLock();
		}
	}
#endregion
#region 当たり判定関連
	/// <summary>
	/// 衝突処理
	/// </summary>
	protected override int OnBulletHit(Bullet b) {
		int realDamage = base.OnBulletHit(b);
		//表示更新
		indicator.OnDamage();
		//相手がPlayerか確認してスコアを加算
		if(b.owner) {
			if(b.owner.tag == "Player") {
				Player p = b.owner.GetComponent<Player>();
				if(p) p.indicator.AddSocre(realDamage);
			}
		}
		return realDamage;
	}
	/// <summary>
	/// 死亡処理(引数は相手)
	/// </summary>
	protected override void OnBreak(GameObject rivalObject) {
		base.OnBreak(rivalObject);
		//ステージ管理クラスにイベントを送信
		if(sm) {
			sm.OnPlayerBreak(this);
		}
		//表示更新
		indicator.OnBreak();
	}
#endregion
}