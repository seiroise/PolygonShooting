using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// 機体選択用のUISelect_LabelButton
/// </summary>
public class UISelect_Ship : UISelect_LabelButton {
	//管理クラス
	protected GameManager gm;
	[Header("機体")]
	public UIShipDataIndicator shipDataIndicator;	//機体データ表示用
	[Header("カテゴリ")]
	public List<string> categoryList;		//カテゴリリスト
	protected int nowCategoryIndex;		//現在のカテゴリ番号
	[Header("オプション")]
	public bool flagAddNone;			//最初にNone項目を追加するか
	//その他
	protected List<ToolBox.ShipData> shipDataList;	//機体データリスト
	protected List<string> shipNameList;			//機体名リスト
#region MonoBehaviourイベント
	protected override void Start() {
		base.Start();
		gm = GameManager.Instance;
		//テスト
		SetTextListInCategory(0);
	}
#endregion
#region 関数
	/// <summary>
	/// カテゴリからテキストリストを設定する
	/// </summary>
	public void SetTextListInCategory(string category) {
		if(!gm) gm = GameManager.Instance;
		//辞書から機体名リストを取得
		shipDataList = gm.ShipDic_Sort(category);
		if(shipDataList == null) return;
		shipNameList = new List<string>(shipDataList.Select(elem => elem.name));
		//テキストリストを設定
		SetTextList(shipNameList);
		//カテゴリラベル設定
		SetCategoryLabel(category);
	}
	/// <summary>
	/// カテゴリ番号からテキストリストを設定する
	/// </summary>
	public void SetTextListInCategory(int index) {
		//範囲確認
		if(index < 0 || categoryList.Count <= index) return;
		//番号設定
		nowCategoryIndex = index;
		//テキストリスト設定
		SetTextListInCategory(categoryList[index]);
	}
	/// <summary>
	/// カテゴリリストを設定する
	/// <para>第二カテゴリは設定するリストの番号</para>
	/// </summary>
	public void SetCategoryList(List<string> categoryList, int setCategoryIndex = 0) {
		this.categoryList = categoryList;
		//テキストリスト設定
		SetTextListInCategory(setCategoryIndex);
	}
	/// <summary>
	/// ゲームオブジェクト名から機体データを取得する
	/// </summary>
	protected ToolBox.ShipData GetShipDataFromObjectName(GameObject obj) {
		//インデックスを求める
		int index = GetTrueIndex(obj);
		//機体データを取得
		return shipDataList[index];
	}
#endregion
#region 移動関連
	/// <summary>
	/// 前のカテゴリに移動
	/// </summary>
	protected override void MovePrevCate() {
		//数値移動
		int index = nowCategoryIndex - 1;
		if(index < 0) index = categoryList.Count - 1;
		//テキストリスト設定
		SetTextListInCategory(index);
	}
	/// <summary>
	/// 次のカテゴリに移動
	/// </summary>
	protected override void MoveNextCate() {
		//数値移動
		int index = nowCategoryIndex + 1;
		if(categoryList.Count <= index) index = 0;
		//テキストリスト設定
		SetTextListInCategory(index);
	}
#endregion
#region UIイベント
	/// <summary>
	/// ボタンホバー
	/// </summary>
	protected override void OnSelectButtonHover(GameObject obj) {
		//データ取得
		ToolBox.ShipData shipData = GetShipDataFromObjectName(obj);
		//データ表示
		if(shipDataIndicator) {
			shipDataIndicator.Indicate(true);
			shipDataIndicator.SetShipData(shipData);
		}
		//イベント送信
		NotifyTarget(hoverFunctionName, shipData);
	}
	/// <summary>
	/// ボタンクリック
	/// </summary>
	protected override void OnSelectButtonClicked(GameObject obj) {
		//インデックスを求める
		ToolBox.ShipData shipData = GetShipDataFromObjectName(obj);
		//イベント送信
		NotifyTarget(clickedFunctionName, shipData);
	}
#endregion
}