using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
/// <summary>
/// ステージでのプレイヤー情報の表示
/// </summary>
public class StagePlayerIndicator_Multi : MonoBehaviour {
	//フラグ
	protected bool flagActivate = false;		//有効化フラグ
	//プレイヤー情報
	public Player player;
	public Ship playerShip;
	[Header("Camera")]
	public Camera uiCamera;					//UI表示カメラ
	public TargetCenterCamera playerCamera;		//プレイヤーを中心とした範囲カメラ
	protected float playerCameraScale = 10f;			//プレイヤーカメラの標準倍率
	protected float longRangePlayerCameraScale = 30f;	//最大カメラ倍率
	protected bool flagLongRange = false;			//広範囲表示
	[Header("ImageEffect")]
	public GlitchController glitch;				//画面効果その1 ダメージエフェクト
	public float glitchIntensity = 0.5f;
	public float glitchTime = 2f;
	[Header("UI")]
	public UIColorSetter colorSetter;			//色変更用
	[Header("UI_Left")]
	public UIPanel leftPanel;
	public UILabel playerNoLabel;				//プレイヤー番号
	public UILabel scoreLabel;				//スコア
	[Header("UI_Right")]
	public UIPanel rightPanel;
	public UIPopupLabelManager popupLabel;		//ポップアップラベル
	public float[] popupHPBorders;				//HPポップアップの境界線
	protected int nowPopupHPBorderIndex;		//現在の
	public TargetCenterCamera lockObjectCamera;	//ロック対象表示カメラ
	protected float lockObjectCameraScale = 1.5f;		//ロック対象表示カメラの標準倍率
	public UIViewport lockObjectCameraViewport;	//ロック対象表示カメラのビューポート
	public Transform topLeft, bottomRight;			//左上と右下
	public UILabel lockObjectLabel;				//ロック対象の名前表示用
	public UITweener lockChangeEffect;			//エフェクト
	[Header("UI_Center")]
	public UIPanel centerPanel;
	//バー
	public UISprite hpBar;				//HP
	public UILabel hpLabel;
	public UISprite energyBar;			//Energy
	public UILabel energyLabel;
	//向き
	public UISprite playerDirection;		//自分の向き
	public UISprite lockObjectDirection;	//ロック対象の向き
	//アイテムポップアップ
	public UITweener popupItem;			//アイテムポップアップ
	public UISprite popupItemSprite;		//アイテムポップアップのスプライト
	public UILabel popupItemLabel;		//ポップアップアイテムのラベル
	//ロングレンジのときの機体マーカー
	public UISprite shipMarker;
	public UISprite lockObjectMarker;
	//弾毎の表示
	public UILabel normalLabel;			//通常
	public UISprite normalReloadBar;			
	public UITweener normalShotEffect;
	public UILabel missileLabel;			//ミサイル
	public UISprite missileReloadBar;
	public UITweener missileShotEffect;
	public UILabel opticalLabel;			//光学
	public UISprite opticalReloadBar;
	public UITweener opticalShotEffect;
#region MonoBehaviourイベント
	protected void Update() {
		//フラグ確認
		if(!flagActivate) return;
		//向き
		UpdateDirectionIndicator();
		//HP・Energy
		UpdateHPIndicator();
		UpdateEnergynIndicator();
		//リロード
		UpdateReloadIndicator();
		//マーカー
		if(flagLongRange) {
			UpdateShipMarker();
		}
	}
#endregion
#region 関数
	/// <summary>
	/// カメラの画面への映写範囲の設定
	/// </summary>
	public void SetCameraViewportRect(Rect viewportRect) {
		uiCamera.rect = viewportRect;
		playerCamera.camera.rect = viewportRect;
	}
	/// <summary>
	/// アイテムポップアップ再生
	/// </summary>
	public void PlayPopupItem(string spriteName, int num) {
		popupItem.Reset();
		popupItem.Play(true);
		popupItemSprite.SetAtlasSprite(popupItemSprite.atlas.GetSprite(spriteName));
		if(num > 0) {
			popupItemLabel.text = "✖" + num;
		} else {
			popupItemLabel.text = "";
		}
	}
#endregion
#region 有効化関連
	/// <summary>
	/// プレイヤーを設定して有効化
	/// </summary>
	public void Activate(Player player) {
		//表示
		Indicate(true);
		this.player = player;
		this.playerShip = player.ship;
		//自身を設定
		//this.player.indicator = this;
		//カメラ
		playerCamera.mainTarget = player.gameObject;
		playerCamera.minSize = playerShip.collisionRadius * playerCameraScale;
		//色
		colorSetter.SetColor(playerShip.symbolColor);
		//左側
		Activate_Left();
		//右側
		Activate_Right();
		//中央
		Activate_Center();
		//フラグ
		flagActivate = true;
	}
	/// <summary>
	/// 左側の有効化
	/// </summary>
	protected void Activate_Left() {
		//プレイヤー番号
		playerNoLabel.text = "Player" + (int)player.playerNo;
		//スコア
		scoreLabel.text = "0";
	}
	/// <summary>
	/// 右側の有効化
	/// </summary>
	protected void Activate_Right() {
		//ポップアップの色
		popupLabel.SetColor(playerShip.symbolColor);
		nowPopupHPBorderIndex = 0;
		//ロック対象表示カメラ
		lockChangeEffect.Play(false);
		lockObjectCamera.gameObject.SetActive(true);
		lockObjectCameraViewport.sourceCamera = uiCamera;
		lockObjectCameraViewport.topLeft = topLeft.transform;
		lockObjectCameraViewport.bottomRight = bottomRight.transform;
		//追尾の対象に自分を設定
		lockObjectCamera.mainTarget = player.gameObject;
		lockObjectCamera.minSize = playerShip.collisionRadius * lockObjectCameraScale;
	}
	/// <summary>
	/// 中央の有効化
	/// </summary>
	protected void Activate_Center() {
		//リロード
		normalLabel.text = missileLabel.text = opticalLabel.text = "None";
		//スプライトの設定
		normalReloadBar.type = missileReloadBar.type = opticalReloadBar.type = UISprite.Type.Filled;
		normalReloadBar.fillDirection = missileReloadBar.fillDirection = opticalReloadBar.fillDirection = UISprite.FillDirection.Horizontal;
		normalReloadBar.fillAmount = missileReloadBar.fillAmount = opticalReloadBar.fillAmount = 0f;
		//tweener設定
		normalShotEffect.Play(false);
		missileShotEffect.Play(false);
		opticalShotEffect.Play(false);
		//HP,Energyスプライトの設定
		hpBar.type = energyBar.type = UISprite.Type.Filled;
		hpBar.fillDirection = energyBar.fillDirection = UISprite.FillDirection.Horizontal;
		hpBar.fillAmount = energyBar.fillAmount = 0f;
		UpdateHPIndicator();
		UpdateEnergynIndicator();
		//方向表示
		lockObjectDirection.gameObject.SetActive(false);
		//アイテムポップアップ
		popupItem.enabled = false;
	}
	/// <summary>
	/// 表示のオンオフ
	/// </summary>
	public void Indicate(bool flag) {
		gameObject.SetActive(flag);
		uiCamera.gameObject.SetActive(flag);
		playerCamera.gameObject.SetActive(flag);
	}
#endregion
#region 毎フレーム更新関連
	/// <summary>
	/// リロード表示の更新
	/// </summary>
	protected void UpdateReloadIndicator() {
		string text = "";
		float par = 0f;
		Ship.Launchers l;
		foreach(string str in playerShip.launcherDic.Keys) {
			l = playerShip.launcherDic[str];
			text = l.reloadCount + "/" + l.launchers.Count;
			par = l.nowReloadTime / l.maxReloadTime;
			switch(str) {
				case "Normal":
					normalLabel.text = text;
					normalReloadBar.fillAmount = par;
				break;
				case "Missile":
					missileLabel.text = text;
					missileReloadBar.fillAmount = par;
				break;
				case "Optical":
					opticalLabel.text = text;
					opticalReloadBar.fillAmount = par;
				break;
			}
		}
	}
	/// <summary>
	/// HP表示の更新
	/// </summary>
	protected void UpdateHPIndicator() {
		float par = 0f;
		par = playerShip.GetHPPar();
		hpBar.fillAmount = Mathf.Lerp(hpBar.fillAmount, par, 10f * Time.deltaTime);
		hpLabel.text = "HP " + playerShip.nowHP + "/" + playerShip.maxHP;
	}
	/// <summary>
	/// エネルギー表示の更新
	/// </summary>
	protected void UpdateEnergynIndicator() {
		float par = 0f;
		par = playerShip.nowEnergy / playerShip.maxEnergy;
		energyBar.fillAmount = Mathf.Lerp(energyBar.fillAmount, par, 10f * Time.deltaTime);
		energyLabel.text = "EN " + Mathf.Floor(par * 100f) + "%";
	}
	/// <summary>
	/// 方向表示の更新
	/// </summary>
	protected void UpdateDirectionIndicator() {
		//自身の
		playerDirection.transform.eulerAngles = player.transform.eulerAngles;
		//ロック対象の
		if(player.lockObject) {
			Vector3 angles = Vector3.zero;
			angles.z = FuncBox.TwoPointAngleD(player.transform.position, player.lockObject.transform.position);
			lockObjectDirection.transform.eulerAngles = angles;
		}
	}
	/// <summary>
	/// 機体マーカーの更新
	/// </summary>
	protected void UpdateShipMarker() {
		if(flagLongRange) {
			Vector3 pos = FuncBox.ViewPointTransform(playerCamera.camera, player.transform.position, uiCamera);
			shipMarker.transform.position = pos;
			shipMarker.transform.eulerAngles = player.transform.eulerAngles;
			if(player.lockObject) {
				pos = FuncBox.ViewPointTransform(playerCamera.camera, player.lockObject.transform.position, uiCamera);
				lockObjectMarker.transform.position = pos;
				lockObjectMarker.transform.eulerAngles = player.lockObject.transform.eulerAngles;
			}
		}
	}
#endregion
#region プレイヤーイベント
	/// <summary>
	/// ダメージを受けたとき
	/// </summary>
	public void OnDamage() {
		float par, inversePar;
		par = playerShip.GetHPPar();
		inversePar = 1 - par;
		//Glitch
		glitch.SetGlitch(inversePar * glitchIntensity, inversePar * glitchTime);
		//Popup
		par *= 100f;
		while(true) {
			if(popupHPBorders.Length <= nowPopupHPBorderIndex) break;
			if(popupHPBorders[nowPopupHPBorderIndex] > par) {
				popupLabel.PlayPopup("残りHP" + (int)popupHPBorders[nowPopupHPBorderIndex] +"%", false);
				nowPopupHPBorderIndex++;
			} else {
				break;
			}
		}
	}
	/// <summary>
	/// 通常弾発射
	/// </summary>
	public void OnAButton() {
		normalShotEffect.Reset();
		normalShotEffect.Play(false);
		//テスト
		PlayPopupItem("Icon_Missile", 10);
	} 
	/// <summary>
	/// ミサイル発射
	/// </summary>
	public void OnXButton() {
		missileShotEffect.Reset();
		missileShotEffect.Play(false);
	}
	/// <summary>
	/// 光学弾発射
	/// </summary>
	public void OnBButton() {
		opticalShotEffect.Reset();
		opticalShotEffect.Play(false);
	}
	/// <summary>
	/// ロック対象変更
	/// </summary>
	public void OnLockObjectChange() {
		lockChangeEffect.Reset();
		lockChangeEffect.Play(false);

		Pilot lockObj = player.lockObject;
		if(lockObj) {
			//追尾の対象に設定
			lockObjectCamera.mainTarget = lockObj.gameObject;
			lockObjectLabel.text = lockObj.name + "[" + lockObj.ship.shipName + "]";
			lockObjectCamera.minSize = lockObj.ship.collisionRadius * lockObjectCameraScale;
			lockObjectCameraViewport.enabled = false;
			//方向表示
			lockObjectDirection.gameObject.SetActive(true);
			Color c = lockObj.ship.symbolColor;//FuncBox.SetColorAlpha(, lockObjectDirection.color.a);
			lockObjectDirection.color = c;
			//マーカー
			if(flagLongRange) {
				lockObjectMarker.enabled = true;
			}
			lockObjectMarker.color = c;
			//ポップアップ
			popupLabel.PlayPopup("ロック : " + lockObj.name, false);
		} else {
			OnUnLock();
		}
	}
	/// <summary>
	/// ロック解除
	/// </summary>
	public void OnUnLock() {
		//追尾の対象に自分を設定
		lockObjectCamera.mainTarget = player.gameObject;
		lockObjectLabel.text = "None";
		//方向表示
		lockObjectDirection.gameObject.SetActive(false);
		//マーカー
		lockObjectMarker.enabled = false;
		//ポップアップ
		popupLabel.PlayPopup("ロック解除", false);
	}
	/// <summary>
	/// カメラサイズ変更
	/// </summary>
	public void OnRightShoulder() {
		flagLongRange = flagLongRange ? false : true;
		if(flagLongRange) {
			playerCamera.minSize = playerShip.collisionRadius * longRangePlayerCameraScale;
			//マーカー表示
			shipMarker.enabled = true;
			if(player.lockObject) {
				lockObjectMarker.enabled = true;
				lockObjectMarker.color = player.lockObject.ship.symbolColor;
			}
		} else {
			playerCamera.minSize = playerShip.collisionRadius * playerCameraScale;
			//マーカー非表示
			shipMarker.enabled = false;
			lockObjectMarker.enabled = false;
		}
	}
	/// <summary>
	/// プレイヤー死亡時
	/// </summary>
	public void OnPlayerDie() {
		//Popup
		popupLabel.PlayPopup("Break!!", false);
	}
#endregion
}