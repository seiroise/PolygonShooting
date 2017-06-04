using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//UI的なボタンリストを管理するクラス(DraggableCamera用)
public class UIListManager : MonoBehaviour {

	[Header("オブジェクト")]
	public UIDraggableCamera draggableCamera;	//ドラッグ用カメラ
	public UIGrid buttonListParent;				//リストの親オブジェクト
	public UIButtonComponents buttonPrefab;		//ボタンのプレハブ

	[Header("イベント")]
	public GameObject iventHandler;		//送信先
	public string iventName;				//イベント名

	//コンテンツ
	protected List<UIButtonComponents> buttonList;	//ボタン

#region 関数

	public void SetContents(List<string> list, GameObject target, string iventName) {
		if (buttonList == null) {
			buttonList = new List<UIButtonComponents>();
		}
		//ボタンの数よりもリストの要素のほうが大きい場合
		if (buttonList.Count < list.Count) {
			//差分を生成する
			InstantiateButton(list.Count - buttonList.Count);
		}

		int i;
		//リストの要素数だけアクティブをtrueに
		for (i = 0; i < list.Count; i++) {
			buttonList[i].gameObject.SetActive(true);
			buttonList[i].label.text = list[i];
			buttonList[i].buttonMessages[0].target = target;
			buttonList[i].buttonMessages[0].functionName = iventName;
		}
		//リストの要素数から漏れたものはアクティブをfalseに
		for (; i < buttonList.Count; i++) {
			buttonList[i].gameObject.SetActive(false);
		}
	}

	//ボタンの生成
	public void InstantiateButton(int num) {
		if (buttonList == null) {
			buttonList = new List<UIButtonComponents>();
		}

		for (int i = 0; i < num; i++) {
			//生成
			var b = (GameObject)Instantiate(buttonPrefab.gameObject);
			b.transform.parent = buttonListParent.transform;
			b.transform.localScale = Vector3.one;
			b.transform.localPosition = Vector3.zero;

			b.name = i.ToString();

			//イベント
			var comp = b.GetComponent<UIButtonComponents>();
			comp.label.text = "-";
			comp.buttonMessages[0].target = iventHandler;
			comp.buttonMessages[0].functionName = iventName;

			//ドラッグカメラ
			var drag = b.GetComponent<UIDragCamera>();
			drag.draggableCamera = draggableCamera;

			buttonList.Add(comp);
		}

		//位置調整
		buttonListParent.Reposition();
	}

#endregion
}