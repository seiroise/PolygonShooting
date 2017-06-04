using UnityEngine;
using System.Collections;
/// <summary>
/// ポリゴンメッシュのグリッドを作成する
/// </summary>
public class Grid : MonoBehaviour {
	[Header("グリッド設定")]
	public int hLineNum;		//横
	public int vLineNum;		//縦
	public Vector2 spacing;		//縦横の間隔
	[Header("線の設定")]
	public float gridWidth;		//線の太さ
	public Color gridColor;		//線の色
	[Header("マテリアル設定")]
	public Material gridMaterial;	//グリッド材質
	//内部パラメータ
	protected MeshFilter meshFilter;
	protected MeshRenderer meshRenderer;
#region 関数
	/// <summary>
	/// グリッドを生成
	/// </summary>
	public void InstantiateGrid() {
		if (!meshFilter) {
			meshFilter = GetComponent<MeshFilter>();
			if (!meshFilter) {
				meshFilter = gameObject.AddComponent<MeshFilter>();
			}
		}
		if (!meshRenderer) {
			meshRenderer = GetComponent<MeshRenderer>();
			if (!meshRenderer) {
				meshRenderer = gameObject.AddComponent<MeshRenderer>();
			}
		}

		//メッシュの初期化
		Mesh mesh = new Mesh();
		mesh.Clear();
		mesh.name = "Grid";

		//頂点座標配列の下準備
		int num = hLineNum + vLineNum;
		Vector3[] vertices = new Vector3[num * 4];	//頂点座標
		Vector2[] uvs = new Vector2[num * 4];		//テクスチャ座標
		Color[] colors = new Color[num * 4];		//色配列
		int[] triangles = new int[num * 3 * 2];		//三角形インデックス
	
		//横線、縦線の長さ
		float hLineLength, vLineLength;
		hLineLength = (hLineNum - 1) * spacing.x;
		vLineLength = (vLineNum - 1) * spacing.y;

		//基準点
		Vector3 basePos = transform.localPosition;

		//始点(-x,y)の地点(左上)
		Vector3 startPos = new Vector3(hLineLength / 2f, vLineLength / 2f, 0f);

		//インデックス
		int i = 0;

		//横線(+yから-yに向けて作っていく)　↓三 この向き
		float yPos;
		for (int y = 0; y < vLineNum; y++, i++) {
			//四角の中央のy座標
			yPos = basePos.y + startPos.y - spacing.y * y;
			//-x側
			vertices[i * 4 + 0] = new Vector3(basePos.x - startPos.x, yPos - (gridWidth / 2f), basePos.z);	//左下
			vertices[i * 4 + 1] = new Vector3(basePos.x - startPos.x, yPos+ (gridWidth / 2f), basePos.z);	//左上
			//+x側
			vertices[i * 4 + 2] = new Vector3(basePos.x + startPos.x, yPos - (gridWidth / 2f), basePos.z);	//右下
			vertices[i * 4 + 3] = new Vector3(basePos.x + startPos.x, yPos + (gridWidth / 2f), basePos.z);	//右上
		
			//三角形インデックスの割り当て(時計回り)
			//一つ目
			triangles[i * 6 + 0] = i * 4 + 0;		//左下
			triangles[i * 6 + 1] = i * 4 + 1;		//左上
			triangles[i * 6 + 2] = i * 4 + 3;		//右上
			//２つ目
			triangles[i * 6 + 3] = i * 4 + 0;		//左下
			triangles[i * 6 + 4] = i * 4 + 3;		//右上
			triangles[i * 6 + 5] = i * 4 + 2;		//右下

			//テクスチャ座標の設定
			uvs[i * 4 + 0] = new Vector2(0f, 0f);
			uvs[i * 4 + 1] = new Vector2(0f, 1f);
			uvs[i * 4 + 2] = new Vector2(1f, 0f);
			uvs[i * 4 + 3] = new Vector2(1f, 1f);
		}

		//縦線(-xから+xに向けて作っていく)　->||| この向き
		float xPos;
		for (int x = 0; x < hLineNum; x++, i++) {
			//四角の中央のx座標
			xPos = basePos.x - startPos.x + spacing.x * x;
			//+y側
			vertices[i * 4 + 0] = new Vector3(xPos + (gridWidth / 2f), basePos.y + startPos.y, basePos.z);	//右上
			vertices[i * 4 + 1] = new Vector3(xPos - (gridWidth / 2f), basePos.y + startPos.y, basePos.z);	//左上
			//-y側
			vertices[i * 4 + 2] = new Vector3(xPos + (gridWidth / 2f), basePos.y - startPos.y, basePos.z);	//右下
			vertices[i * 4 + 3] = new Vector3(xPos - (gridWidth / 2f), basePos.y - startPos.y, basePos.z);	//左下

			//三角形インデックスの割り当て(時計回り)
			//一つ目
			triangles[i * 6 + 0] = i * 4 + 1;		//左上
			triangles[i * 6 + 1] = i * 4 + 0;		//右上
			triangles[i * 6 + 2] = i * 4 + 2;		//右下
			//２つ目
			triangles[i * 6 + 3] = i * 4 + 1;		//左上
			triangles[i * 6 + 4] = i * 4 + 2;		//右下
			triangles[i * 6 + 5] = i * 4 + 3;		//左下

			//テクスチャ座標の設定
			uvs[i * 4 + 0] = new Vector2(0f, 0f);
			uvs[i * 4 + 1] = new Vector2(0f, 1f);
			uvs[i * 4 + 2] = new Vector2(1f, 0f);
			uvs[i * 4 + 3] = new Vector2(1f, 1f);
		}

		//色配列
		for (i = 0; i < colors.Length; i++) {
			colors[i] = gridColor;
		}

		//頂点座標とテクスチャ座標,三角形の頂点インデックスをMeshに登録
		mesh.vertices = vertices;
		mesh.uv = uvs;
		mesh.uv2 = uvs;
		mesh.colors = colors;
		mesh.triangles = triangles;

		//なんか知らんがお約束らしいので
		mesh.Optimize();
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();

		meshFilter.mesh = mesh;

		//マテリアルの指定
		meshRenderer.material = gridMaterial;
		meshRenderer.material.color = gridColor;
	}
	/// <summary>
	/// パラメータを指定してグリッドを生成
	/// </summary>
	public void InstantiateGrid(int hLineNum, int vLineNum, Vector2 spacing) {
		this.hLineNum = hLineNum;
		this.vLineNum = vLineNum;
		this.spacing = spacing;
		//生成
		InstantiateGrid();
	}
	/// <summary>
	/// グリッド削除
	/// </summary>
	public void DestroyGrid() {
		if(!meshFilter) {
			meshFilter = GetComponent<MeshFilter>();
		}
		if(meshFilter) {
			meshFilter.mesh.Clear();
		}
	}
	/// <summary>
	/// ワールド座標をグリッド座標に変換する
	/// </summary>
	public Vector3 WorldToGridPosition(Vector3 pos) {
		Vector3 p = Vector3.zero;
		//対象座標を自分のローカルな座標系に変更する
		pos = transform.InverseTransformPoint(pos);

		//2で割ってさらに細分化する
		p.x = (int)(pos.x / (spacing.x / 2));
		p.y = (int)(pos.y / (spacing.y / 2));

		p.x += p.x >= 0 ? 1 : -1;
		p.y += p.y >= 0 ? 1 : -1;

		p.x = (int)(p.x / 2f);
		p.y = (int)(p.y / 2f);
		return p;
	}
	/// <summary>
	/// ワールド座標をグリッド交差座標に変換する
	/// </summary>
	public Vector3 WorldToGridCrossPosition(Vector3 pos) {
		Vector3 sPos = WorldToGridPosition(pos);
		sPos.x *= spacing.x;
		sPos.y *= spacing.y;
		return sPos;
	}
	/// <summary>
	/// ワールド座標をグリッド線の交差する座標に変換する。
	/// <para>戻り値はグリッド内座標ならtrueを返す</para>
	/// </summary>
	public bool WorldToGridCrossPosition(out Vector3 sPos, Vector3 dPos) {
		sPos = WorldToGridPosition(dPos);
		//範囲外確認
		if (Mathf.Abs(sPos.x) >= hLineNum / 2f) {
			return false;
		}
		if (Mathf.Abs(sPos.y) >= vLineNum / 2f) {
			return false;
		}

		sPos.x *= spacing.x;
		sPos.y *= spacing.y;

		return true;
	}
	/// <summary>
	/// 指定ワールド座標がグリッド内の座標か確認する。
	/// <para>グリッド内ならtrueを返す</para>
	/// </summary>
	public bool CheckGridPosition(Vector3 pos) {
		pos = WorldToGridPosition(pos);
		//範囲外確認
		if (Mathf.Abs(pos.x) >= hLineNum / 2f) {
			return false;
		}
		if (Mathf.Abs(pos.y) >= vLineNum / 2f) {
			return false;
		}
		return true;
	}
	/// <summary>
	/// 指定値がグリッド内に収まるxか確認する
	/// <para>グリッド内の値ならtrue</para>
	/// </summary>
	public bool CheckGridX(float x) {
		if (Mathf.Abs(x) >= (hLineNum / 2f) * spacing.x) {
			return false;
		}
		return true;
	}
	/// <summary>
	/// 指定値がグリッド内に収まるyか確認する
	/// <para>グリッド内の値ならtrue</para>
	/// </summary>
	public bool CheckGridY(float y) {
		if (Mathf.Abs(y) >= (vLineNum / 2f) * spacing.y) {
			return false;
		}
		return true;
	}
	/// <summary>
	/// グリッド線を表示/非表示する
	/// </summary>
	public void Indicate(bool flag) {
		if (!meshRenderer) {
			meshRenderer = GetComponent<MeshRenderer>();
		}
		if(meshRenderer) {
			meshRenderer.enabled = flag;
		}
	}
	/// <summary>
	/// グリッドの範囲をRectで返す。(-x, -y, w, h)
	/// </summary>
	public Rect GetGridRect() {
		float width, height;
		width = (hLineNum - 1) * spacing.x;
		height = (vLineNum - 1) * spacing.y;
		return new Rect(-width * 0.5f, -height * 0.5f, width, height);
	}
#endregion
}