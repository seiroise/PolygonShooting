using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// ランチャー用のダイアログ
/// </summary>
public class UILauncher : MonoBehaviour {
	//Manager
	protected GameManager gm;
	[Header("UI_BulletSelect")]
	public UISelect_LabelButton bulletSelect;
	public List<string> categoryList;
	protected bool flagInit = false;			//初期化が済んでいるか
	protected int nowCategoryNum;			//現在の弾カテゴリー番号
	protected List<string> bulletIDList;		//現在カテゴリーの弾IDリスト
	[Header("UI_BulletIndicater")]
	public Camera previewCamera;			//プレビュー用カメラ
	public SpriteRenderer previewSprite;		//プレビュー用スプライト
	public UILabel name;
	public UILabel caption;
	[Header("UI_Performance")]
	public UILauncherPerformance launcher;
	public UILauncherPerformance bullet;
	public UILauncherPerformance total;
	//Other
	protected ToolBox.Launcher selectLauncher;
#region MonoBehaviourイベント
	protected void Start() {
		gm = GameManager.Instance;
	}
#endregion
#region 関数
	/// <summary>
	/// 初期化
	/// </summary>
	protected void Init() {
		if(gm == null) gm = GameManager.Instance;
		SetBulletCategory(0);
		flagInit = true;
	}
	/// <summary>
	/// ランチャーを設定
	/// </summary>
	public void SetLauncher(ToolBox.Launcher l) {
		//初期化確認
		if(!flagInit) {
			Init();
		}
		//選択ランチャーに設定
		selectLauncher = l;
		//ランチャー性能
		launcher.SetPerformance(l.basePerformance, true);
		//弾装備確認
		bool flagBullet = false;
		if(gm.bulletDic.ContainsKey(l.bulletCategory)) {
			if(gm.bulletDic[l.bulletCategory].ContainsKey(l.bulletID)) {
				flagBullet = true;
			}
		}
		//弾性能など設定
		if(flagBullet) {
			//設定済み
			//弾データを取得
			ToolBox.Bullet b = gm.bulletDic[l.bulletCategory][l.bulletID];
			SetBullet(b);
		} else {
			//Performance
			bullet.SetPerformance("-");
			total.SetPerformance("-");
			//BulletIndicator
			SetBulletIndicator(null);
		}
	}
	/// <summary>
	/// 弾を設定。ID
	/// </summary>
	public void SetBullet(string bulletID) {
		//弾データを取得
		ToolBox.Bullet b;
		if(bulletID == "None") {
			b = null;
		} else {
			 b = gm.bulletDic[categoryList[nowCategoryNum]][bulletID];
		}
		SetBullet(b);
	}
	/// <summary>
	/// 弾を設定。インスタンス
	/// </summary>
	public void SetBullet(ToolBox.Bullet b) {
		//nullチェック
		if(b == null) {
			//外す
			selectLauncher.RemoveBullet();
			//UI更新_Performance
			bullet.SetPerformance("-");
			total.SetPerformance("-");
			//UI更新_BulletIndicator
			SetBulletIndicator(b);
		} else {
			selectLauncher.SetBullet(b);
			//UI更新_Performance
			bullet.SetPerformance(b.performance, false);
			total.SetPerformance(selectLauncher.totalPerformance, true);
			//UI更新_BulletIndicator
			SetBulletIndicator(b);	
		}
	}
	/// <summary>
	/// 弾のカテゴリ設定
	/// </summary>
	public void SetBulletCategory(int categoryNum) {
		//範囲確認
		if(categoryNum < 0 || categoryList.Count <= categoryNum) return;
		string category = categoryList[categoryNum];
		//辞書確認
		Debug.Log(gm);
		if(!gm.bulletDic.ContainsKey(category)) return;
		nowCategoryNum = categoryNum;
		//弾辞書から弾のIDだけをとってくる
		bulletIDList = new List<string>(gm.bulletDic[category].Values.Select(elem => elem.nameID));
		bulletIDList.Insert(0, "None");
		bulletSelect.SetTextList(bulletIDList);
		//UI設定
		bulletSelect.categoryLabel.text = category;
	}
	/// <summary>
	/// カテゴリ移動。移動量(1,0,-1)を指定
	/// </summary>
	protected void MoveCategoryIndex(int move) {
		int categoryNum = nowCategoryNum + move;
		if(categoryNum < 0)  {
			categoryNum = categoryList.Count - 1;
		} else if(categoryList.Count <= categoryNum) {
			categoryNum = 0;
		}
		SetBulletCategory(categoryNum);
	}
	/// <summary>
	/// カメラを非表示に
	/// </summary>
	public void HideCamera() {
		if (previewCamera.enabled) {
			previewCamera.enabled = false;
		}
	}
#endregion
#region UI関連
	/// <summary>
	/// 弾の表示を設定
	/// </summary>
	protected void SetBulletIndicator(ToolBox.Bullet bullet) {
		//nullチェック
		if(bullet == null) {
			name.text = "Name";
			caption.text = "Caption";
			//プレビュー
			if(previewCamera.enabled) {
				previewCamera.enabled = false;
			}
		} else {
			name.text = bullet.nameID;
			caption.text = bullet.caption;
			//プレビュー
			if(!previewCamera.enabled) {
				previewCamera.enabled = true;
			}
			Bullet bInstance = bullet.instance.GetComponent<Bullet>();
			previewSprite.sprite = bInstance.sprite.sprite;
		}
	}
#endregion
#region UISelectイベント
	/// <summary>
	/// セレクトボタンホバー
	/// </summary>
	protected void SelectButtonHover(int index) {
		
	}
	/// <summary>
	/// セレクトボタンクリック
	/// </summary>
	protected void SelectButtonClicked(int index) {
		string bulletID = bulletIDList[index];
		SetBullet(bulletID);
	}
	/// <summary>
	/// 前のカテゴリへ
	/// </summary>
	protected void PrevCateButtonClicked() {
		//-1
		MoveCategoryIndex(-1);
	}
	/// <summary>
	/// 次のカテゴリへ
	/// </summary>
	protected void NextCateButtonClicked() {
		//+1
		MoveCategoryIndex(1);
	}
#endregion
}