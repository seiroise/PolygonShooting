using UnityEngine;
using System.Collections;
using System.Text;
/// <summary>
/// シングルカメラ用のプレイヤーUI
/// </summary>
public class StagePlayerIndicator_Single : MonoBehaviour {
	//対象
	protected Ship ship;
	protected Player player;
	//管理クラス
	protected StageManager sm;
	protected GameManager gm;
	[Header("カメラ")]
	public Camera mainCamera;		//全体を映すカメラ
	public Camera uiCamera;			//UIのカメラ
	[Header("UIパーツ")]
	public UILabel playerNoLabel;		//プレイヤー番号
	[Header("UI_ランチャー")]
	public UILauncherState normal;
	public UILauncherState missile;
	public UILauncherState optical;
	[Header("UI_ロック")]
	public UITweener lockButtonEffectTween;
	[Header("UI_HP")]
	public UILabel hpLabel;
	public UISprite hpBar;
	[Header("UI_Energy")]
	public UILabel energyLabel;
	public UISprite energyBar;
	[Header("UI_Velocity")]
	public UILabel velocityLabel;			//速度
	[Header("DamageEffect")]
	public UITweener damageEffect;
	[Header("AddStatus")]
	public FadeOutLabel addHP;
	public FadeOutLabel addNormal;
	public FadeOutLabel addMissile;
	public FadeOutLabel addOptical;
	[Header("UI_MoveUI")]
	public GameObject moveUIParent;		//移動UIの親オブジェクト
	public GameObject shipDirection;		//機体の向き表示
	public UISprite lockObjectDirection;	//ロック対象の向き表示
	public UISprite shipMarker;			//一定以上のカメラサイズで表示
	public UILabel moveLabel;			//移動UIのラベル
	public UILabel breakLabel;			//ブレークラベル
	public FadeOutLabel lvUpLabel;		//レベルアップラベル
	public UIInt scoreLabel;				//スコア表示
	private Vector3 targetPos;			//目標位置
	[Header("ItemList")]
	public UIPlayerItemList itemList;		//所持アイテム
	[Header("色")]
	public UIColorSetter colorSetter;
#region MonoBehaviourイベント
	protected void Start() {
		gm = GameManager.Instance;
		sm = StageManager.Instance;
	}
	protected void Update() {
		if(player) {
			UpdateHp();
			UpdateEnergy();
			UpdateVelocity();
			UpdateLauncher();
			UpdateMoveUI();
			UpdateDirectionIndicator();
		}
	}
#endregion
#region 関数
	/// <summary>
	/// 表示対象のプレイヤーを設定
	/// </summary>
	public void SetPlayer(Player player) {
		if(gm == null) gm = GameManager.Instance;
		if(gm == null) return;
		this.ship = player.ship;
		this.player = player;
		player.indicator = this;
		//色
		int playerNo = ((int)player.playerNo);
		colorSetter.SetColor(gm.playerColor[playerNo - 1]);
		//プレイヤー番号
		playerNoLabel.text = "Player" + playerNo;
		//HP
		UpdateHp();
		//エネルギー
		UpdateEnergy();
		//ランチャー
		normal.Set("None", 0f);
		normal.shotEffectTween.Play(true);
		missile.Set("None", 0f);
		normal.shotEffectTween.Play(true);
		optical.Set("None", 0f);
		normal.shotEffectTween.Play(true);
		UpdateLauncher();
		//ロック
		lockButtonEffectTween.Play(true);
		//ダメージエフェクト
		damageEffect.Play(true);
		//アイテム
		itemList.SetItemNum(4);
		//addStatus
		addHP.gameObject.SetActive(false);
		addNormal.gameObject.SetActive(false);
		addMissile.gameObject.SetActive(false);
		addOptical.gameObject.SetActive(false);
		//移動UI
		moveLabel.text = playerNo + "P";
		lockObjectDirection.gameObject.SetActive(false);
		shipMarker.gameObject.SetActive(false);
		breakLabel.gameObject.SetActive(false);
		lvUpLabel.gameObject.SetActive(false);
	}
	/// <summary>
	/// HP表示の更新
	/// </summary>
	public void UpdateHp() {
		if(!ship) return;
		//UI表示更新
		hpLabel.text = "HP" + ship.nowHP + "/" + ship.maxHP;
		float par = ship.GetHPPar();
		hpBar.fillAmount = Mathf.Lerp(hpBar.fillAmount, par, 10f * Time.deltaTime);
		hpBar.fillAmount = par;
	}
	/// <summary>
	/// エネルギー表示の更新
	/// </summary>
	public void UpdateEnergy() {
		if(!ship) return;
		//UI表示変更
		float par = ship.nowEnergy / ship.maxEnergy;
		energyBar.fillAmount = Mathf.Lerp(energyBar.fillAmount, par, 10f * Time.deltaTime);
		energyBar.fillAmount = par;
		energyLabel.text = "En" + Mathf.Floor(par * 100f).ToString() + "%";
	}
	/// <summary>
	/// 速度表示の更新
	/// </summary>
	public void UpdateVelocity() {
		if(!velocityLabel) return;
		velocityLabel.text = Mathf.FloorToInt(ship.GetVelocity()) + "km";
	}
	/// <summary>
	/// ランチャー表示の更新
	/// </summary>
	public void UpdateLauncher() {
		Ship.Launchers l;
		float par;
		foreach(string str in ship.launcherDic.Keys) {
			l = ship.launcherDic[str];
			StringBuilder sb = new StringBuilder();
			sb.Append(l.reloadCount);
			sb.Append("/");
			sb.Append(l.launchers.Count);
			par = l.nowReloadTime / l.maxReloadTime;
			switch(str) {
				case "Normal":
					normal.Set(sb.ToString(), par);
				break;
				case "Missile":
					missile.Set(sb.ToString(), par);
				break;
				case "Optical":
					optical.Set(sb.ToString(), par);
				break;
			}
		}
	}
	/// <summary>
	/// 移動UI表示の更新
	/// </summary>
	public void UpdateMoveUI() {
		//座標
		targetPos = FuncBox.ViewPointTransform(mainCamera, player.transform.position, uiCamera);
		moveUIParent.transform.position = FuncBox.Vector3Lerp(moveUIParent.transform.position, targetPos, 0.5f);
	}
	/// <summary>
	/// 方向表示の更新
	/// </summary>
	public void UpdateDirectionIndicator() {
		//機体角度
		shipDirection.transform.eulerAngles = player.transform.eulerAngles;
		//ロック対象との角度
		if(player.lockObject) {
			Vector3 angles = Vector3.zero;
			angles.z = FuncBox.TwoPointAngleD(player.transform.position, player.lockObject.transform.position);
			lockObjectDirection.transform.eulerAngles = angles;
		}
		//マーカーの表示
		//float cameraSize = sm.mainCamera.targetSize;
		//if(cameraSize >= ship.collisionRadius * 30f) {
		//	shipMarker.gameObject.SetActive(true);
		//} else {
		//	shipMarker.gameObject.SetActive(false);
		//}
	}
	/// <summary>
	/// プレイヤーが設定されているか確認
	/// </summary>
	public bool CheckSetPlayer() {
		if(player) {
			return true;
		} else {
			return false;
		}
	}
	/// <summary>
	/// プレイヤー番号を取得
	/// </summary>
	public int GetPlayerNo() {
		return (int)player.playerNo - 1;
	}
	/// <summary>
	/// スコアを加算
	/// </summary>
	public void AddSocre(int score) {
		scoreLabel.AddInt(score);
	}
#endregion
#region 表示/非表示
	/// <summary>
	/// 表示/非標示
	/// </summary>
	public void Indicate(bool flag) {
		gameObject.SetActive(flag);
	}
	/// <summary>
	/// Breakラベル
	/// </summary>
	public void IndicateBreakLabel(bool flag) {
		breakLabel.gameObject.SetActive(flag);
		//方向表示
		IndicateDirection(!flag);
	}
	/// <summary>
	/// 方向表示
	/// </summary>
	public void IndicateDirection(bool flag) {
		shipDirection.SetActive(flag);
		lockObjectDirection.gameObject.SetActive(flag);
	}
	/// <summary>
	/// レベルアップ表示
	/// </summary>
	public void IndicateLvUp(int addHP, int addNormal, int addMissile, int addOptical) {
		float fadeTime = 3f;
		lvUpLabel.StartFadeLabel("LvUp", fadeTime);
		this.addHP.StartFadeLabel("+" + addHP, fadeTime);
		this.addNormal.StartFadeLabel("+" + addNormal, fadeTime);
		this.addMissile.StartFadeLabel("+" + addMissile, fadeTime);
		this.addOptical.StartFadeLabel("+" + addOptical, fadeTime);
	}
#endregion
#region プレイヤーイベント
	/// <summary>
	/// ダメージを受けたとき
	/// </summary>
	public void OnDamage() {
		damageEffect.Reset();
		damageEffect.Play(true);
	}
	/// <summary>
	/// 通常弾発射
	/// </summary>
	public void OnShotNormal() {
		//エフェクト
		normal.shotEffectTween.Reset();
		normal.shotEffectTween.Play(true);
	} 
	/// <summary>
	/// ミサイル発射
	/// </summary>
	public void OnShotMissile() {
		//エフェクト
		missile.shotEffectTween.Reset();
		missile.shotEffectTween.Play(true);
	}
	/// <summary>
	/// 光学弾発射
	/// </summary>
	public void OnShotOptical() {
		//エフェクト
		optical.shotEffectTween.Reset();
		optical.shotEffectTween.Play(true);
	}
	/// <summary>
	/// ロック対象変更
	/// </summary>
	public void OnLockObjectChange() {
		//エフェクト
		lockButtonEffectTween.Reset();
		lockButtonEffectTween.Play(true);
		//角度表示
		if(player.lockObject) {
			lockObjectDirection.gameObject.SetActive(true);
			lockObjectDirection.color = player.lockObject.ship.symbolColor;
		} else {
			lockObjectDirection.gameObject.SetActive(false);
		}
	}
	/// <summary>
	/// ロック解除
	/// </summary>
	public void OnUnLock() {
		//角度表示
		lockObjectDirection.gameObject.SetActive(false);
	}
	/// <summary>
	/// カメラサイズ変更
	/// </summary>
	public void OnRightShoulder() {
		
	}
	/// <summary>
	/// プレイヤー破壊時
	/// </summary>
	public void OnBreak() {
		IndicateBreakLabel(true);
	}
	/// <summary>
	/// プレイヤー復活時
	/// </summary>
	public void OnReborn() {
		IndicateBreakLabel(false);
		lockObjectDirection.gameObject.SetActive(false);
	}
	/// <summary>
	/// アイテム取得
	/// </summary>
	public bool OnGetItem(string itemSpriteName, Color color) {
		if(itemList) {
			Color c = new Color(color.r, color.g, color.b, 1f);
			return itemList.AddItem(itemSpriteName, c);
		}
		return false;
	}
#endregion
}