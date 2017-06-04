using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// xy平面上にメッシュで線を描く。常に線の幅は一定。
/// </summary>
public class MeshLine : MonoBehaviour {
	//区画の情報
	[Serializable]
	public class Section {
		public Vector3[] basePosition;	//基準座標
		public Vector3[] vertices;		//区画の頂点
	}
	[Header("Line")]
	public float width = 1f;
	public float lineZ = 0f;
	public Color vertexColor;
	[Header("Mesh")]
	public Material material;
	protected MeshFilter mf;
	protected MeshRenderer mr;
	public List<Vector3> positions;
	public List<Section> sections;

#region MonoBehaviourイベント
	protected void Awake() {
		//コンポーネント追加
		mf = GetComponent<MeshFilter>();
		if(mf == null) {
			mf = gameObject.AddComponent<MeshFilter>();
		}
		mr = GetComponent<MeshRenderer>();
		if(mr == null) {
			mr = gameObject.AddComponent<MeshRenderer>();
		}
		//座標リスト
		positions = new List<Vector3>();
		sections = new List<Section>();
	}
#endregion
#region 関数(virtual)
	/// <summary>
	/// 座標を追加
	/// </summary>
	public virtual void AddPosition(Vector3 pos) {
		pos.z = lineZ;
		positions.Add(pos);
		//要素が2未満なら何もしない
		if(positions.Count < 2) return;
		//区画データを作成
		int count = positions.Count;
		sections.Add(CreateSection(positions[count - 2], positions[count - 1]));
		//線の作成
		CreateLine(sections);
	}
	/// <summary>
	/// 座標を挿入。
	/// </summary>
	public virtual void InsertPosition(int index, Vector3 pos) {
		//範囲確認
		if(index < 0 || positions.Count <= index) return;
		//座標
		positions.Insert(index, pos);
		//区画
		Section sec;
		if(index == 0) {
			sec = CreateSection(positions[index], positions[index + 1]);
			sections.Insert(index, sec);
		} else {
			sections.RemoveAt(index - 1);
			sec = CreateSection(positions[index], positions[index + 1]);
			sections.Insert(index - 1, sec);
			sec = CreateSection(positions[index - 1], positions[index]);
			sections.Insert(index - 1, sec);
		}
		CreateLine(sections);
	}
	/// <summary>
	/// 座標を削除
	/// </summary>
	public virtual void RemovePosition(int index) {
		//範囲確認
		if(index < 0 || positions.Count <= index) return;
		//インデックスから座標、区画を削除
		if(index == 0) {
			//Debug.Log("A");
			positions.RemoveAt(index);
			if(sections.Count > 0) {
				sections.RemoveAt(index);
			}
		} else if(index == positions.Count - 1){
			//Debug.Log("B");
			positions.RemoveAt(index);
			sections.RemoveAt(index - 1);
		} else {
			//Debug.Log("C");
			positions.RemoveAt(index);
			//Debug.Log("Remove " + sections.Count + " : " + index); 
			sections.RemoveAt(index);
			sections.RemoveAt(index - 1);
			sections.Insert(index - 1, CreateSection(positions[index - 1], positions[index]));
		}
		//Debug.Log(sections);
		CreateLine(sections);
	}
	/// <summary>
	/// インデックスの座標を変更
	/// </summary>
	public virtual void ChangePosition(int index, Vector3 pos) {
		//範囲確認
		if(index < 0 || positions.Count <= index) return;
		pos.z = lineZ;
		positions[index] = pos;
		if(positions.Count <= 1)	return;
		//区画変更
		if(index == 0) {
			//Debug.Log("A");
			sections[index] = CreateSection(positions[index], positions[index + 1]);
		} else if(index == positions.Count - 1) {
			//Debug.Log("B");
			sections[index - 1] = CreateSection(positions[index - 1], positions[index]);
		} else {
			//Debug.Log("C");
			sections[index] = CreateSection(positions[index], positions[index + 1]);
			sections[index - 1] = CreateSection(positions[index - 1], positions[index]);
		}
		//線の作成
		CreateLine(sections);
		////変更
		//pos.z = lineZ;
		//positions[index] = pos;
		////座標の数が1以下の場合はこれ以上変更する必要なし
		//if(positions.Count <= 1) return;
		////インデックスが最後の要素を指していたら-1する
		//if(index == positions.Count - 1) index--;
		//Section sec = CreateSection(positions[index], positions[index + 1]);
		////区画リストを変更
		//sections[index] = sec;
	}
	/// <summary>
	/// 座標、区画、メッシュのリセット
	/// </summary>
	public virtual void Reset() {
		positions = new List<Vector3>();
		sections = new List<Section>();
		CreateLine(sections);
	}
	/// <summary>
	/// 一時的に座標を追加して線を描画する
	/// </summary>
	public virtual void FlashPosition(Vector3 pos) {
		if(positions.Count <= 0) return;
		pos.z = lineZ;
		Section sec = CreateSection(positions[positions.Count - 1], pos);
		sections.Add(sec);
		CreateLine(sections);
		sections.RemoveAt(sections.Count - 1);
	}
#endregion
#region 関数(その他)
	/// <summary>
	/// 座標リストの要素数を返す
	/// </summary>
	public int PositionCount() {
		return positions.Count;
	}
	/// <summary>
	/// 座標リストを返す。
	/// </summary>
	public List<Vector3> GetPositions() {
		return positions;
	}
	/// <summary>
	/// 座標リストを設定する。flagAddEndは末尾に始点を追加するか
	/// </summary>
	public void SetPositions(List<Vector3> positions, bool flagAddEnd = false) {
		if(flagAddEnd) {
			positions.Add(positions[0]);
		}
		this.positions = positions;
		CreateLine(positions);
	}
	/// <summary>
	/// 二つのベクトルから区画データを作成する
	/// </summary>
	protected Section CreateSection(Vector3 v1, Vector3 v2) {
		Section sec = new Section();
		//基準座標
		sec.basePosition = new Vector3[2];
		sec.basePosition[0] = v1;
		sec.basePosition[1] = v2;
		//区画頂点
		Vector3 direction = v2 - v1;
		direction = Quaternion.AngleAxis(90f, Vector3.forward) * direction;
		direction = direction.normalized * width;
		sec.vertices = new Vector3[4];
		sec.vertices[0] = sec.basePosition[0] + direction;
		sec.vertices[1] = sec.basePosition[0] - direction;
		sec.vertices[2] = sec.basePosition[1] + direction;
		sec.vertices[3] = sec.basePosition[1] - direction;
		return sec;
	}
	/// <summary>
	/// インデックスを二つ指定して、一致判定を行う
	/// </summary>
	public bool CheckEqualPos(int p1, int p2) {
		if(positions.Count <= 1) return false;
		if(positions[p1].x == positions[p2].x && positions[p1].y == positions[p2].y) {
			return true;
		} else {
			return false;
		}
	}
	/// <summary>
	/// 始点と終点の一致判定を行います
	/// </summary>
	public bool CheckStartEndPos() {
		return CheckEqualPos(0, positions.Count - 1);
	}
#endregion
#region メッシュ関連
	/// <summary>
	/// 線のメッシュを作成
	/// </summary>
	public void CreateLine(List<Vector3> positions) {
		sections = CreateSectionList(positions);
		if(sections == null) return;
		CreateLine(sections);
	}
	public void CreateLine(List<Section> sections) {
		if(sections.Count <=  0) {
			mf.mesh.Clear();
		} else {
			//線のメッシュを作成
			mf.mesh = CreateLineMesh(sections);
			//色を変更
			ChangeMeshColor(vertexColor);
			//マテリアル設定
			mr.material = material;
		}
	}
	/// <summary>
	/// 座標リストから区画リストを作成して。要素数が1以下のときはnullを返す
	/// </summary>
	protected List<Section> CreateSectionList(List<Vector3> positions) {
		//要素数が1以下のときは作れないのでnull
		if(positions.Count <= 1) return null;
		List<Section> sections = new List<Section>();
		for(int i = 0; i < positions.Count - 1; i++) {
			//追加
			sections.Add(CreateSection(positions[i], positions[i + 1]));
		}
		//結果を返す
		return sections;
	}
	/// <summary>
	/// 区画データから線のメッシュを作成する
	/// </summary>
	protected Mesh CreateLineMesh(List<Section> sections) {
		Mesh m = new Mesh();
		m.Clear();
		//配列初期化
		Vector3[] vertices = new Vector3[sections.Count * 4];
		Vector2[] uv = new Vector2[vertices.Length];
		int[] triangles = new int[sections.Count * 6 + (sections.Count - 1) * 6];		//区画ごとの平面 + つなぎ目
		int i;
		//区画
		for(i = 0; i < sections.Count; i++) {
			//頂点
			for(int j = 0; j < 4; j++) {
				vertices[i * 4 + j] = sections[i].vertices[j];
			}
			//三角形
			triangles[i * 6 + 0] = i * 4 + 0;
			triangles[i * 6 + 1] = i * 4 + 3;
			triangles[i * 6 + 2] = i * 4 + 1;

			triangles[i * 6 + 3] = i * 4 + 0;
			triangles[i * 6 + 4] = i * 4 + 2;
			triangles[i * 6 + 5] = i * 4 + 3;
		}
		//つなぎ目
		for(int j = 0; j < sections.Count - 1; i++, j++) {
			triangles[i * 6 + 0] = j * 4 + 2;
			triangles[i * 6 + 1] = (j + 1) * 4 + 0;
			triangles[i * 6 + 2] = j * 4 + 3;

			triangles[i * 6 + 3] = j * 4 + 2;
			triangles[i * 6 + 4] = (j + 1) * 4 + 1;
			triangles[i * 6 + 5] = j * 4 + 3;
		}
		//Mesh設定
		m.vertices = vertices;
		m.uv = uv;
		m.triangles = triangles;
		m.Optimize();
		m.RecalculateBounds();
		m.RecalculateNormals();
		
		return m;
	}
	/// <summary>
	/// メッシュの頂点カラーの変更
	/// </summary>
	protected void ChangeMeshColor(Color c) {
		Mesh m = mf.mesh;
		Color[] colors = new Color[m.vertexCount];

		for(int i = 0; i < colors.Length; i++) {
			colors[i] = c;
		}

		m.colors = colors;
	}
#endregion
}