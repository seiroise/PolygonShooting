using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIMarkerManager : MonoBehaviour {
	
	[Header("パラメータ")]
	public Camera worldCamera;
	public Camera uiCamera;
	public GameObject prefab;
	public int poolNum = 512;

	//プール
	protected List<UIButtonMessage> prefabPool;

#region MonoBehaviourイベント

	protected void Start() {
		prefabPool = new List<UIButtonMessage>();
		Init(poolNum);
	}

#endregion

#region 関数
	
	//初期化
	protected void Init(int num) {
		for (int i = 0; i < poolNum; i++) {
			var g = (GameObject)Instantiate(prefab);
			g.name = i.ToString();
			g.transform.parent = transform;
			g.transform.localScale = Vector3.one;

			var bm = g.GetComponent<UIButtonMessage>();
			if (!bm) {
				bm = g.AddComponent<UIButtonMessage>();
			}

			g.SetActive(false);
			prefabPool.Add(bm);
		}
	}

	//割り当てる(一括)
	public Dictionary<GameObject , int> GetMarker(List<Vector3> posList, GameObject target, string functionName, float z = 0f) {

		var gDic = new Dictionary<GameObject, int>();
		int j = 0;
		for (int i = 0; i < posList.Count; i++) {
			UIButtonMessage g = null;
			for (; j < prefabPool.Count; j++) {
				if (!prefabPool[j].gameObject.activeInHierarchy) {
					g = prefabPool[j];
					break;
				}
			}
			if (g) {
				g.gameObject.SetActive(true);
				g.transform.localScale = Vector3.one;
				var pos = FuncBox.ViewPointTransform(worldCamera, posList[i], uiCamera);
				pos.z = z;
				g.transform.position = pos;
				g.target = target;
				g.functionName = functionName;

				gDic.Add(g.gameObject, i);
			}
		}

		return gDic;
	}

	//割り当てる角度も指定
	public Dictionary<GameObject, int> GetMarker(List<Vector3> posList, List<Vector3> angleList, GameObject target, string functionName, float z = 0f) {
		var gDic = new Dictionary<GameObject, int>();
		int j = 0;
		for (int i = 0; i < posList.Count; i++) {
			UIButtonMessage g = null;
			for (; j < prefabPool.Count; j++) {
				if (!prefabPool[j].gameObject.activeInHierarchy) {
					g = prefabPool[j];
					break;
				}
			}
			if (g) {
				g.gameObject.SetActive(true);
				g.transform.localScale = Vector3.one;

				//座標
				var pos = FuncBox.ViewPointTransform(worldCamera, posList[i], uiCamera);
				pos.z = z;
				g.transform.position = pos;

				//角度
				g.transform.eulerAngles = angleList[i];

				//イベント設定
				g.target = target;
				g.functionName = functionName;

				gDic.Add(g.gameObject, i);
			}
		}

		return gDic;
	}

	//全て非アクティブに
	public void Clear() {
		for (int i = 0; i < prefabPool.Count; i++) {
			if (prefabPool[i].gameObject.activeInHierarchy) {
				prefabPool[i].gameObject.SetActive(false);
			}
		}
	}

#endregion
}