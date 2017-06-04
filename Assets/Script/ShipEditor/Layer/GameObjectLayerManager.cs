using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ゲームオブジェクトのレイヤー(重なり)を管理する
/// </summary>
public class GameObjectLayerManager : MonoBehaviour {
	[Header("設定")]
	public ToolBox.Direction layerDirection = ToolBox.Direction.Z;	//レイヤーの方向
	public float min = 0f;		//配置する最小値
	public float max = -1f;		//配置する最大値
	protected List<GameObject> objectList;
#region MonoBehaviourイベント
	protected void Awake() {
		//初期化
		objectList = new List<GameObject>();
	}
#endregion
#region アクセサ
	/// <summary>
	/// ゲームオブジェクトのリストを取得
	/// </summary>
	public List<GameObject> GetObjectList() {
		return objectList;
	}
#endregion
#region 関数
	/// <summary>
	/// 追加
	/// </summary>
	public void Add(GameObject g, bool flagReplace = true) {
		if(g == null) return;
		objectList.Add(g);
		//再配置
		if(flagReplace) {
			Replace();
		}
	}
	/// <summary>
	/// 削除
	/// </summary>
	public void Remove(GameObject g, bool flagReplace = true) {
		if(g == null) return;
		objectList.Remove(g);
		//再配置
		if(flagReplace) {
			Replace();
		}
	}
	/// <summary>
	/// 再配置
	/// </summary>
	public void Replace() {
		if(objectList.Count <= 0) return;
		float space = (max - min) / objectList.Count;
		float value = min;
		Vector3 pos;
		switch(layerDirection) {
			case ToolBox.Direction.X:
				foreach(GameObject g in objectList) {
					pos = g.transform.localPosition;
					pos.x = value;
					g.transform.localPosition = pos;
					value += space;
				}
			break;
			case ToolBox.Direction.Y:
				foreach(GameObject g in objectList) {
					pos = g.transform.localPosition;
					pos.y = value;
					g.transform.localPosition = pos;
					value += space;
				}
			break;
			case ToolBox.Direction.Z:
				foreach(GameObject g in objectList) {
					pos = g.transform.localPosition;
					pos.z = value;
					g.transform.localPosition = pos;
					value += space;
				}
			break;
		}		
	}
#endregion
}