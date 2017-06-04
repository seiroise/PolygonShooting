using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// 複数のポップアップラベルを管理
/// </summary>
public class UIPopupLabelManager : MonoBehaviour {
	[Header("コンテンツ")]
	public UIPopupLabelContents contentsPrefab;			//プレハブ
	public int contentsNum;								//生成数
	public UIGrid contentsParent;							//コンテンツ親オブジェクト
	protected List<UIPopupLabelContents> contentsList;		//コンテンツリスト
	protected Queue<KeyValuePair<string, bool>> queue;	//キュー
#region MonoBehaviourイベント
	protected void Awake() {
		queue = new Queue<KeyValuePair<string,bool>>();
		CreateContents();
	}
#endregion
#region 関数
	/// <summary>
	/// コンテンツ生成
	/// </summary>
	protected void CreateContents() {
		contentsList = new List<UIPopupLabelContents>();
		GameObject contents;
		UIPopupLabelContents con;
		for(int i = 0 ; i < contentsNum; i++) {
			contents = (GameObject)Instantiate(contentsPrefab.gameObject);
			con = contents.GetComponent<UIPopupLabelContents>();
			con.manager = this;
			contentsList.Add(con);
			contents.transform.parent = contentsParent.transform;
			contents.transform.localScale = Vector3.one;
		}
		contentsParent.Reposition();
	}
	/// <summary>
	/// ポップアップ再生
	/// </summary>
	public void PlayPopup(string text, bool flagEffect) {
		//再生してないポップアップを取得
		bool flagGet = false;
		foreach(UIPopupLabelContents con in contentsList) {
			if(!con.flagPlay) {
				con.Play(text, flagEffect);
				flagGet = true;
				break;
			}
		}
		//取得できなかった場合キューに追加
		if(!flagGet) {
			queue.Enqueue(new KeyValuePair<string, bool>(text, flagEffect));
		}
	}
	/// <summary>
	/// キューから取得 -> 再生
	/// </summary>
	protected void QueueGetWithPlay() {
		if(queue.Count <= 0) return;
		KeyValuePair<string, bool> pair = queue.Dequeue();
		PlayPopup(pair.Key, pair.Value);
	}
	/// <summary>
	/// 色設定
	/// </summary>
	public void SetColor(Color color) {
		foreach(UIPopupLabelContents con in contentsList) {
			con.sprite.color = color;
		}
	}
#endregion
#region コンテンツイベント
	public void OnFinished() {
		//キューから再生
		QueueGetWithPlay();
	}
#endregion
}