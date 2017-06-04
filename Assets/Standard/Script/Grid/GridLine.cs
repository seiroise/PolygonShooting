using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// Grid3Dを利用して線を描く
/// </summary>
public class GridLine : MonoBehaviour {
	[Header("基本パラメータ")]
	public Grid grid;
	public Camera targetCamera;
	public LineRenderer lineRenderer;
	public MeshLine line;
	public bool flagControl = true;		//操作中
	public bool flagDrawLine = false;		//描画処理中
	public bool flagGridEndWithReset = false;	//線を書き終わったらリセットする
	[Header("イベント")]	//図形に関するイベントを送信
	public GameObject target;
	public string functionName = "OnGridLineEnd";
	//座標リスト
	protected List<Vector3> posList;
#region MonoBehaviourイベント
	protected void Start() {
		posList = new List<Vector3>();

		if (!targetCamera) {
			targetCamera = Camera.main;
		}
	}
	protected void Update() {
		if (flagControl) {
			//マウス移動処理
			OnMouseMove();
			//マウス入力処理
			OnMouseDown();
		}
	}
#endregion
#region 関数
	/// <summary>
	/// マウス移動処理
	/// </summary>
	protected void OnMouseMove() {
		var p = FuncBox.GetMousePoint(targetCamera);
		if (grid.WorldToGridCrossPosition(out p, p)) {
			Push(p);
			//ラインレンダラに座標を設定
			line.CreateLine(posList);
			//FuncBox.SetLineRenderer(lineRenderer, posList, true);
			Pop();
		}
	}
	/// <summary>
	/// マウス入力処理
	/// </summary>
	protected void OnMouseDown() {
		if (Input.GetMouseButtonDown(0)) {
			//フラグ操作
			if (!flagDrawLine) {
				if (posList.Count == 0) {
					flagDrawLine = true;
				}
			}
			//追加
			PushVertex();			
		}

		if (Input.GetMouseButtonDown(1)) {
			//取り出し
			PopVertex();
			//フラグ操作
			if (flagDrawLine) {
				if (posList.Count == 0) {
					flagDrawLine = false;
				}
			}
		}
	}
	/// <summary>
	/// 頂点Push処理
	/// </summary>
	protected void PushVertex() {
		Vector3 p = FuncBox.GetMousePoint(targetCamera);
		if (grid.WorldToGridCrossPosition(out p, p)) {
			//頂点の数が2つ以下(三角形を作れる最低の頂点数)
			Push(p);
			if (posList.Count > 2) {
				//始点と終点が同じか
				if (ContainStartEndPos()) {
					//イベント送信(被っている終点を取り除いてから)
					Vector3 pos = Pop();
					if (target) {
						target.SendMessage(functionName, posList, SendMessageOptions.DontRequireReceiver);
					}
					if(flagGridEndWithReset) {
						OnReset();
					} else {
						Push(pos);
					}
				}
			}
		}
	}
	/// <summary>
	/// 頂点Pop処理
	/// </summary>
	protected void PopVertex() {
		if (posList.Count != 0) {
			Pop();
		}
	}
	/// <summary>
	/// 座標Push
	/// </summary>
	protected void Push(Vector3 pos) {
		posList.Add(pos);
	}
	/// <summary>
	/// 座標Pop
	/// </summary>
	protected Vector3 Pop() {
		var p = posList[posList.Count - 1];
		posList.RemoveAt(posList.Count - 1);
		return p;
	}
	/// <summary>
	/// 座標を挿入する
	/// </summary>
	public void Insert(int index, Vector3 pos) {
		posList.Insert(index, pos);
	}
	/// <summary>
	/// 頂点を削除する
	/// </summary>
	public void Remove(int index) {
		posList.RemoveAt(index);
	}
	/// <summary>
	/// 頂点を動かす(マウス)
	/// </summary>
	public void MoveVertex(int index) {
		if (0 <= index || index < posList.Count) {
			var p = FuncBox.GetMousePoint(targetCamera);
			if (grid.WorldToGridCrossPosition(out p, p)) {
				posList[index] = p;
			}			
		}
	}
	/// <summary>
	/// 外部からラインレンダラに描画命令を送る
	/// </summary>
	public void SetLineRenderer() {
		FuncBox.SetLineRenderer(lineRenderer, posList);
	}
	/// <summary>
	/// 始点と終点が同じ座標か判定する
	/// </summary>
	protected bool ContainStartEndPos() {
		
		if (posList[0] == posList[posList.Count - 1]) {
			return true;
		}
		return false;
	}
	/// <summary>
	/// リストの中に同じ点がないか確認する
	/// </summary>
	protected bool ContainPosition(Vector3 pos) {
		for (int i = 0; i < posList.Count; i++) {
			if (posList[i] == pos) {
				return true;
			}
		}
		return false;
	}
	/// <summary>
	/// 入力頂点,LineRendererをリセット
	/// </summary>
	public void OnReset() {
		lineRenderer.SetVertexCount(0);
		posList = new List<Vector3>();
	}
	/// <summary>
	/// 座標リストを設定。第二引数は始点を末尾に追加するか
	/// </summary>
	public void SetPos(List<Vector3> posList, bool flagAddStartPosEnd = false) {
		this.posList = posList;
		if(flagAddStartPosEnd) {
			lineRenderer.SetVertexCount(posList.Count + 1);
		} else {
			lineRenderer.SetVertexCount(posList.Count);
		}
		int i = 0;
		for(; i < posList.Count; i++) {
			lineRenderer.SetPosition(i, posList[i]);
		}
		if(flagAddStartPosEnd) {
			lineRenderer.SetPosition(i, posList[0]);
		}
	}
	/// <summary>
	/// 座標リストを取得する。引数は末尾を取り除くか
	/// </summary>
	public List<Vector3> GetPos(bool flagRemoveEndPos = false) {
		//新しいインスタンスを生成
		List<Vector3> pos = new List<Vector3>(posList.Select(a => a));
		if(flagRemoveEndPos) {
			pos.RemoveAt(pos.Count - 1);
		}
		return pos;
	}
#endregion
}