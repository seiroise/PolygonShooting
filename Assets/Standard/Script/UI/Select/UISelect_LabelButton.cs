using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
/// <summary>
/// ラベルボタンをページとカテゴリで管理する
/// </summary>
public class UISelect_LabelButton : MonoBehaviour {
	//メンバ
	protected int nowPage;
	protected int maxPage;
	protected List<string> textList;
	[Header("イベント関連")]
	public GameObject target;
	public string hoverFunctionName = "SelectButtonHover";
	public string clickedFunctionName = "SelectButtonClicked";
	public string prevCateFunctionName = "PrevCateButtonClicked";
	public string nextCateFunctionName = "NextCateButtonClicked";
	[Header("UIパーツ_ボタン")]
	public UIGrid buttonParent;
	public UIButtonComponents buttonPrefab;
	public int buttonNum;
	protected List<UIButtonComponents> buttonList;
	protected int forcusButtonNum = -1;	//フォーカスボタン番号
	protected int indicateButtonNum;		//表示しているボタンの数
	[Header("UIパーツ_ページ")]
	public GameObject pageParent;	//親オブジェクト
	public UILabel pageLabel;		//ラベル
	public UIButtonMessage prevPageButton;
	public UIButtonMessage nextPageButton;
	[Header("UIパーツ_カテゴリ")]
	public GameObject categoryParent;	//親オブジェクト
	public UILabel categoryLabel;			//ラベル
	public UIButtonMessage prevCateButton;
	public UIButtonMessage nextCateButton;
#region Monobehaviourイベント
	protected virtual void Awake() {
		//ボタン生成
		InstantiateButton();
		//ボタンにイベントを設定
		//前のページ
		prevPageButton.functionName = "PrevPageButtonClicked";
		prevPageButton.trigger = UIButtonMessage.Trigger.OnClick;
		prevPageButton.target = gameObject;
		//次のページ
		nextPageButton.functionName = "NextPageButtonClicked";
		nextPageButton.trigger = UIButtonMessage.Trigger.OnClick;
		nextPageButton.target = gameObject;
		//前のカテゴリ
		prevCateButton.functionName = "PrevCateButtonClicked";
		prevCateButton.trigger = UIButtonMessage.Trigger.OnClick;
		prevCateButton.target = gameObject;
		//次のカテゴリ
		nextCateButton.functionName = "NextCateButtonClicked";
		nextCateButton.trigger = UIButtonMessage.Trigger.OnClick;
		nextCateButton.target = gameObject;
	}
	protected virtual void Start() {
	}
	protected virtual void Update() {
		//確認用
		//ページ移動
		if(Input.GetKeyDown(KeyCode.RightArrow)) {
			MoveNextPage();
		} else if(Input.GetKeyDown(KeyCode.LeftArrow)) {
			MoveNextPage();
		}
		//ボタン移動
		if(Input.GetKeyDown(KeyCode.UpArrow)) {
			MoveUpButton();
		} else if(Input.GetKeyDown(KeyCode.DownArrow)) {
			MoveDownButton();
		}
	}
#endregion
#region 関数
	/// <summary>
	/// テキストリストの設定
	/// </summary>
	public void SetTextList(List<string> textList) {
		//ボタン確認
		if(buttonList == null) InstantiateButton();
		this.textList = textList;
		//ページ設定
		nowPage = 0;
		if(textList.Count == 0) {
			maxPage = 0;
		} else {
			maxPage = textList.Count / buttonList.Count;
			//あまりが無ければmaxPage -= 1
			if(textList.Count % buttonList.Count == 0) maxPage -= 1;
		}
		//ボタン設定
		SetButtonLabel(nowPage);
		//ページUI設定
		SetPageLabel();
	}
	/// <summary>
	/// ターゲットにイベントを送信
	/// </summary>
	protected void NotifyTarget(string functionName, object value) {
		
		if(target) {
			target.SendMessage(functionName, value, SendMessageOptions.DontRequireReceiver);
		}
	}
	/// <summary>
	/// フォーカスボタンにイベントを送信
	/// </summary>
	protected void NotifyForcusButton(string functionName, object value) {
		//範囲確認
		if(forcusButtonNum < 0 || indicateButtonNum <= forcusButtonNum) return;
		//送信
		buttonList[forcusButtonNum].SendMessage(functionName, value, SendMessageOptions.DontRequireReceiver);
	}
	/// <summary>
	/// フォーカスボタンにUIイベントを送り直す
	/// <para>ページ移動用</para>
	/// </summary>
	protected void ReSendUIEvent() {
		NotifyForcusButton("OnHover", false);
		//範囲確認
		if(forcusButtonNum >= indicateButtonNum) {
			forcusButtonNum = indicateButtonNum - 1;
		}
		NotifyForcusButton("OnHover", true);
	}
	/// <summary>
	/// フォーカスボタンを移動(1, 0, -1)させ、UIイベントを送り直す
	/// <para>ボタン移動用</para>
	/// </summary>
	protected void ReSendUIEvent(int moveNum) {
		//現在のフォーカスボタン
		NotifyForcusButton("OnHover", false);
		//範囲確認
		forcusButtonNum += moveNum;
		if(forcusButtonNum >= indicateButtonNum) {
			forcusButtonNum = 0;
		} else if(forcusButtonNum < 0){
			forcusButtonNum = indicateButtonNum - 1;
		}
		NotifyForcusButton("OnHover", true);
	}
	/// <summary>
	/// ページを考慮したインデックスを取得する
	/// </summary>
	protected int GetTrueIndex(GameObject g) {
		int index = int.Parse(g.name);
		index += buttonList.Count * nowPage;
		return index;
	}
#endregion
#region 移動関連
	/// <summary>
	/// 前ページに移動
	/// </summary>
	protected void MovePrevPage() {
		//マイナス
		int movePage = nowPage - 1;
		if(movePage < 0) movePage = maxPage;
		//設定
		if(movePage != nowPage) {
			SetButtonLabel(movePage);
		}
		//移動先ページを現在のページに設定
		nowPage = movePage;
		//イベントを送信
		ReSendUIEvent();
		//UI更新
		SetPageLabel();
	}
	/// <summary>
	/// 次ページに移動
	/// </summary>
	protected void MoveNextPage() {
		//プラス
		int movePage = nowPage + 1;
		if(movePage > maxPage) movePage = 0;
		//設定
		if(movePage != nowPage) {
			SetButtonLabel(movePage);
		}
		//移動先ページを現在のページに設定
		nowPage = movePage;
		//イベントを送信
		ReSendUIEvent();
		//UI更新
		SetPageLabel();
	}
	/// <summary>
	/// 上ボタンに移動
	/// </summary>
	protected void MoveUpButton() {
		//イベントを送信
		ReSendUIEvent(-1);
	}
	/// <summary>
	/// 下ボタンに移動
	/// </summary>
	protected void MoveDownButton() {
		//イベントを送信
		ReSendUIEvent(1);
	}
	/// <summary>
	/// 前のカテゴリに移動
	/// </summary>
	protected virtual void MovePrevCate() {
		//イベント送信
		NotifyTarget(prevCateFunctionName, null);
	}
	/// <summary>
	/// 次のカテゴリに移動
	/// </summary>
	protected virtual void MoveNextCate() {
		//イベント送信
		NotifyTarget(nextCateFunctionName, null);
	}
#endregion
#region UI関連
	/// <summary>
	/// ボタンを生成
	/// </summary>
	public void InstantiateButton() {
		buttonList = new List<UIButtonComponents>();
		GameObject b;
		UIButtonComponents bc;
		for(int i = 0; i < buttonNum; i++) {
			//生成
			b = (GameObject)Instantiate(buttonPrefab.gameObject);
			b.name = i.ToString();
			b.transform.parent = buttonParent.transform;
			b.transform.localScale = Vector3.one;
			//設定
			bc = b.GetComponent<UIButtonComponents>();
			//クリック
			bc.buttonMessages[0].target = gameObject;
			bc.buttonMessages[0].functionName = "OnSelectButtonClicked";
			bc.buttonMessages[0].trigger = UIButtonMessage.Trigger.OnClick;
			//ホバー
			bc.buttonMessages[1].target = gameObject;
			bc.buttonMessages[1].functionName = "OnSelectButtonHover";
			bc.buttonMessages[1].trigger = UIButtonMessage.Trigger.OnMouseOver;
			//追加
			buttonList.Add(bc);
		}
		//整列
		buttonParent.Reposition();
		indicateButtonNum = buttonNum;
	}
	/// <summary>
	/// ボタンラベル設定
	/// </summary>
	protected void SetButtonLabel(int page) {
		//開始インデックス
		int index = page * buttonList.Count;
		//範囲確認
		if(index < 0 || textList.Count < index) return;
		//ボタンの数だけループ
		int j = 0;
		for(int i = 0; i < buttonList.Count; i++, index++) {
			//範囲確認
			if(textList.Count <= index) {
				//範囲外。ボタンは非表示
				buttonList[i].gameObject.SetActive(false);
			} else {
				//範囲内。ボタンは表示
				buttonList[i].gameObject.SetActive(true);
				buttonList[i].label.text = textList[index];
				j++;
			}
		}
		indicateButtonNum = j;
	}
	/// <summary>
	/// ページラベル設定
	/// </summary>
	protected void SetPageLabel() {
		if(pageLabel) {
			StringBuilder sb = new StringBuilder();
			if(textList.Count == 0) {
				sb.Append("0/0");
			} else {
				sb.Append(nowPage * buttonNum + 1);
				sb.Append("-");
				if(nowPage != maxPage) {
					sb.Append((nowPage + 1) * buttonNum);
				} else {
					sb.Append(textList.Count);
				}
				sb.Append("/");
				sb.Append(textList.Count);
			}
			pageLabel.text = sb.ToString();
		}
	}
	/// <summary>
	/// ページの表示
	/// </summary>
	public void IndicatePage(bool flag) {
		if(pageParent) {
			pageParent.SetActive(flag);
		}
	}
	/// <summary>
	/// カテゴリラベル設定
	/// </summary>
	protected void SetCategoryLabel(string category) {
		if(categoryLabel) {
			categoryLabel.text = category;
		}
	}
	/// <summary>
	/// カテゴリの表示
	/// </summary>
	public void IndicateCategory(bool flag) {
		if(categoryParent) {
			categoryParent.SetActive(flag);
		}
	}
#endregion
#region UIイベント
	/// <summary>
	/// ボタンホバー
	/// </summary>
	protected virtual void OnSelectButtonHover(GameObject obj) {
		//インデックスを求める
		int index = GetTrueIndex(obj);
		//イベント送信
		NotifyTarget(hoverFunctionName, index);
	}
	/// <summary>
	/// ボタンクリック
	/// </summary>
	protected virtual void OnSelectButtonClicked(GameObject obj) {
		//インデックスを求める
		int index = GetTrueIndex(obj);
		//イベント送信
		NotifyTarget(clickedFunctionName, index);
	}
	/// <summary>
	/// 前のページへボタンクリック
	/// </summary>
	protected void PrevPageButtonClicked() {
		MovePrevPage();
	}
	/// <summary>
	/// 次のページへボタンクリック
	/// </summary>
	protected void NextPageButtonClicked() {
		MoveNextPage();
	}
	/// <summary>
	/// 前のカテゴリへボタンクリック
	/// </summary>
	protected void PrevCateButtonClicked() {
		MovePrevCate();
	}
	/// <summary>
	/// 次のカテゴリへボタンクリック
	/// </summary>
	protected void NextCateButtonClicked() {
		MoveNextCate();
	}
#endregion
#region ゲームパッドイベント(GamepadInput_Message)
	protected virtual void AButtonDown() {
		NotifyForcusButton("OnPress", true);
	}
	protected virtual void AButtonUp() {
		NotifyForcusButton("OnClick", null);
	}

	protected virtual void RShoulderButtonDown() {
		MoveNextCate();
	}
	protected virtual void LShoulderButtonDown() {
		MovePrevCate();
	}

	protected virtual void Up() {
		MoveUpButton();
	}
	protected virtual void Down() {
		MoveDownButton();
	}
	protected virtual void Right() {
		MoveNextPage();
	}
	protected virtual void Left() {
		MovePrevPage();
	}
#endregion
}