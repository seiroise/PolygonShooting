using UnityEngine;
using System.Collections;
/// <summary>
/// プレイヤーの所持アイテムリスト
/// </summary>
public class UIPlayerItemList : MonoBehaviour {
	[Header("UIパーツ")]
	public UISprite[] itemSprites;
	public UILabel numLabel;
	private int nowItemNum;
	private int nowItemIndex = 0;
	public UIAtlas atlas;
	public Color emptyColor;
	public string emptySpriteName;
#region MonoBehaviourイベント
	private void Start() {
		ClearItem();
	}
#endregion
#region 関数
	/// <summary>
	/// ストックアイテムの数を設定する
	/// </summary>
	public void SetItemNum(int num) {
		if(num < 1) return;
		if(itemSprites.Length <= num) num = itemSprites.Length;
		ClearItem();
		for(int i = 0; i < itemSprites.Length; i++) {
			if(num > i) {
				itemSprites[i].gameObject.SetActive(true);
			} else {
				itemSprites[i].gameObject.SetActive(false);
			}
		}
		nowItemNum = num;
	}
	/// <summary>
	/// ストックアイテムの数を1増やす
	/// </summary>
	public void AddItemNum() {
		SetItemNum(nowItemNum + 1);
	}
	/// <summary>
	/// 所持リストにアイテムを追加する、追加していっぱいになった場合はtrueを返す
	/// </summary>
	public bool AddItem(string itemName, Color color) {
		UISprite sprite = itemSprites[nowItemIndex];
		sprite.SetAtlasSprite(atlas.GetSprite(itemName));
		sprite.color = color;
		//インデックスを進める
		nowItemIndex++;
		if(nowItemNum <= nowItemIndex) {
			ClearItem();
			return true;
		} else {
			UpdateNumLabel();
			return false;
		}
	}
	/// <summary>
	/// アイテムを全て空にする
	/// </summary>
	public void ClearItem() {
		UISprite s;
		for(int i = 0; i < itemSprites.Length; i++) {
			s = itemSprites[i];
			s.color = emptyColor;
			s.SetAtlasSprite(atlas.GetSprite(emptySpriteName));
		}
		nowItemIndex = 0;
		UpdateNumLabel();
	}
	/// <summary>
	/// 数ラベルを更新する
	/// </summary>
	public void UpdateNumLabel() {
		numLabel.text = nowItemIndex + "/" + nowItemNum;
	}
#endregion
}