using UnityEngine;
using System.Collections;

//Meshをワイヤーフレームで描画する
public class WireFrame : MonoBehaviour {
	
	[Header("表示方法")]
	public MeshTopology mt;
	protected MeshFilter mf;

	// Use this for initialization
	void Start () {
		SetMT();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetMT() {
		if (!mf) {
			mf = GetComponent<MeshFilter>();
		}

		if(mf) {
			mf.mesh.SetIndices(mf.mesh.GetIndices(0), mt, 0);
		}
	}

	public void SetMT(MeshTopology meshTopology) {
		if (!mf) {
			mf = GetComponent<MeshFilter>();
		}

		if(mf) {
			mf.mesh.SetIndices(mf.mesh.GetIndices(0), meshTopology, 0);
		}
	}
}