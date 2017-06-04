using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//線ポリゴン描画クラス
public class MyLineRenderer : MonoBehaviour {

	[Header("パラメータ")]
	public float width;		//太さ
	public Color color;		//色
	public Material material;	//マテリアル

	protected List<Vector3> posList;

	protected MeshFilter meshFilter;
	protected MeshRenderer meshRenderer;

#region MonoBehaviourイベント

	protected void Start() {
		posList = new List<Vector3>();
	}

#endregion

#region 関数
	
	//メッシュに関するコンポーネントを取得する
	protected void SetMesh() {
		if (!meshFilter) {
			meshFilter = gameObject.GetComponent<MeshFilter>();
			if (!meshFilter) {
				meshFilter = gameObject.AddComponent<MeshFilter>();
			}
		}

		if (!meshRenderer) {
			meshRenderer = gameObject.GetComponent<MeshRenderer>();
			if (!meshRenderer) {
				meshRenderer = gameObject.AddComponent<MeshRenderer>();
			}
		}
	}

	public void SetPosition(List<Vector3> posList) {

	}

	//メッシュを生成する(posListから)
	public void CreateMesh() {
		
	}


#endregion
}