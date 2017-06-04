using UnityEngine;
using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
/// <summary>
/// 便利なクラス・構造体を定義
/// </summary>
public class ClassBox {
	/// <summary>
	/// 頂点情報とその点がなす角度とフラグ情報
	/// </summary>
	[System.Serializable]
	public class Vertex {
	#region メンバ
		public Vector3 vertex;	//頂点
		public float angle;		//前後の頂点とのなす角
		public bool flagActive;	//有効かどうか	
	#endregion
	#region コンストラクタ
		public Vertex() {
			vertex = Vector3.zero;
			angle = 0f;
			flagActive = false;
		}
		public Vertex(Vector3 vertex, float angle) {
			this.vertex = vertex;
			this.angle = angle;
			this.flagActive = true;
		}
	#endregion
	#region 関数
		/// <summary>
		/// ディープコピー
		/// </summary>
		public Vertex DeepCopy() {
			return (Vertex)MemberwiseClone();
		}
	#endregion
	}
	/// <summary>
	/// 図形
	/// </summary>
	[System.Serializable]
	public class Figure {
		//メンバ
		protected Vertex[] vertices;	//頂点情報
		protected int[] triangles;		//三角形情報
		protected Color color;		//色情報
		protected float totalSize;		//総面積
		[Header("実行時メンバ")]
		public MeshFilter mf;
		public MeshRenderer mr;
	#region コンストラクタ
		public Figure() {
			//メンバ
			vertices = new Vertex[0];
			color =Color.white;
			triangles = new int[0];
			totalSize = 0f;
			//実行時メンバ
			mf = null;
			mr = null;
		}
		public Figure(Vertex[] vertices, int[] triangles, Color color) : this() {
			this.vertices = vertices;
			this.color = color;
			this.triangles = triangles;
			GetTotalSize();
		}
	#endregion
	#region Static関数
		/// <summary>
		/// 頂点リストを指定して新しくFigureデータを作成する。
		/// </summary>
		public static ClassBox.Figure CreateNewFigure(List<Vector3> posList) {
			Vector3 pos;
			for(int i = 0; i < posList.Count; i++) {
				pos = posList[i];
				pos.z = 0f;
				posList[i] = pos;
			}
			//座標配列に連続して重複する点がないか確認する
			CheckRemovePosition(posList);
			//この時点で要素数が2以下になったらnullを返す
			if(posList.Count <= 2) return null;
			//座標配列を頂点情報を付加した配列に変換する
			var vertices = CreateVertices(posList.ToArray());
			//頂点情報から立体情報に変換する
			return CreateFigureData(vertices);
		}
		/// <summary>
		/// 座標リストの連続して重複する点を削除する。
		/// </summary>
		protected static void CheckRemovePosition(List<Vector3> posList) {
			Vector3 p = posList[posList.Count - 1];
			for (int i = posList.Count - 2; i >= 0; i--) {
				//座標が同じか確認する
				if (p == posList[i]) {
					//同じだった場合はそのインデックスを削除する
					posList.RemoveAt(i);
				} else {
					//違う場合は新しく比較対象に追加する
					p = posList[i];
				}
			}
		}
		/// <summary>
		/// 作成に必要な情報を付加した頂点リストを返す。
		/// </summary>
		protected static ClassBox.Vertex[] CreateVertices(Vector3[] posList) {
			var vertices = new List<ClassBox.Vertex>();
			//角度
			//まずは一番遠い頂点の外積の向きを求める
			int index = -1;	//一番遠い頂点の番号
			float aDis = 0f;	//一番遠い頂点までの距離を記憶する
			for (int i = 0; i < posList.Length; i++) {
				float bDis = Vector3.Distance(Vector3.zero, posList[i]);
				//Debug.Log("Distance : " + i + " : "+ bDis);
				if (bDis > aDis) {
					aDis = bDis;
					index = i;
				}
			}

			//前後の点を見つける
			int next, prev;
			FuncBox.FindNextPrevPoint(index, posList, out next, out prev);

			//外積を求める
			Vector3 nextVector, prevVector;	//indexから見たnextとprevのベクトルから外積を求める
			nextVector = posList[next] - posList[index];
			prevVector = posList[prev] - posList[index];

			Vector3 crossDirection;	//内向き(0~180)の外積
			crossDirection = Vector3.Cross(nextVector, prevVector);	//外積を求める

			//頂点を順に調べて角度を求める
			for (int i = 0; i < posList.Length; i++) {
				FuncBox.FindNextPrevPoint(i, posList, out next, out prev);

				//ベクトルを求める
				nextVector = posList[next] - posList[i];
				prevVector = posList[prev] - posList[i];

				//外積を求める
				Vector3 crossDir;	//外積
				crossDir = Vector3.Cross(nextVector, prevVector);	//外積を求める

				//内積から角度を求める
				//まず距離
				float nextVecDis = Vector3.Distance(posList[next], posList[i]);
				float prevVecDis = Vector3.Distance(posList[prev], posList[i]);
				//角度
				float cosS = Vector3.Dot(nextVector, prevVector) / (nextVecDis * prevVecDis);
				float radian = Mathf.Acos(cosS);
				float degree = radian * Mathf.Rad2Deg;

				//角度が180°の場合頂点として成り立たないので弾く
				if (degree != 180) {
					////外積の向きが異なる場合360°から引く
					if (!FuncBox.CrossDirection(crossDirection, crossDir)) {
						//Debug.Log("外積の向きが異なっています");
						degree = 360 - degree;
					}
					Debug.Log("Angle : " + i + " : " + degree);
					//情報格納
					vertices.Add(new ClassBox.Vertex(posList[i], degree));
				}
			}

			return vertices.ToArray();
		}
		/// <summary>
		/// 頂点情報配列から実際にメッシュとして使えるデータに変換する。
		/// </summary>
		protected static ClassBox.Figure CreateFigureData(ClassBox.Vertex[] positions) {
			//頂点座標配列の下準備
			int polyNum = positions.Length - 2;	//生成物が何角形か
			Vector3[] vertices = new Vector3[positions.Length];	//頂点の数と同じ
			int[] triangles = new int[polyNum * 3 * 2];			//何角形 * 3 * 2(両面)

			//頂点座標とテクスチャ座標の設定
			for (int i = 0; i < vertices.Length; i++) {
				vertices[i] = positions[i].vertex;
			}

			for (int polyIndex = 0; polyIndex < polyNum; polyIndex++) {
				int index = -1;	//一番遠い頂点の番号を記憶する
				int next = 0, prev = 0;		//前後の点
				float aDis = 0f;	//一番遠い頂点までの距離を記憶する
				for (int i = 0; i < positions.Length; i++) {
					if (positions[i].flagActive) {
						float bDis = Vector3.Distance(Vector3.zero, positions[i].vertex);
						//Debug.Log("Distance : " + i + " : "+ bDis);
						if (bDis > aDis) {
							aDis = bDis;
							index = i;
						}
					}
				}

				//前後の点との外積の向きを記録しておく
				FuncBox.FindNextPrevPoint(index, positions, out next, out prev);
				Vector3 nextVector, prevVector;	//indexから見たnextとprevのベクトルから外積を求める
				nextVector = positions[next].vertex - positions[index].vertex;
				prevVector = positions[prev].vertex - positions[index].vertex;

				Vector3 crossDirection;	//外積
				crossDirection = Vector3.Cross(nextVector, prevVector);	//外積を求める

				//ループカウンタ(無限ループに陥る可能性があるので)
				int loopCounter = 0;
				//適した頂点が見つかるまで繰り返す
				while (true) {
					//Debug.Log("index is " + index);
					//頂点と二辺のなす角が180°未満か確かめる
					//前後の頂点の番号を求める
					FuncBox.FindNextPrevPoint(index, positions, out next, out prev);
					//Debug.Log("next : " + next + " prev : " + prev);

					//一番離れている点 = 180°以内なので意味ない？↓
					/*
					//前後の頂点が判明したのでなす角を求める
					//前後の頂点との角度を求めた差が角度になる(内積を使ってもいいみたいだが、180°以上の角になる場合向きが必要なので今回は不採用)
					float angle = 0f, pAngle = 0f, nAngle = 0f;
					//Vector3のAngle関数を使うより自分でAtan2を使って角度を求めるほうが早い(1/2 ~ 2/3早い程度)し何故か正確
					pAngle = TwoPointAngleD(positions[prev].vertex, positions[index].vertex);
					nAngle = TwoPointAngleD(positions[next].vertex, positions[index].vertex);

					angle = Mathf.Abs(nAngle - pAngle);

					Debug.Log("nextAngle : " + nAngle + " prevAngle : " + pAngle + " Angle : " + angle);
		
					//180°以上の場合より離れている点が存在しているので他の点を探す(おそらくありえない　←　ありえました!!!!!!!!)
					if(angle >= 180) {
						Debug.Log("もっと遠い点があるよ!");
						//インデックスをずらす
						index = next;
					}*/

					//ちょうどいい点とは、三点の外積の向きと三点がつくる三角形の中に他の頂点が入っていない場合
					bool flagVertex = true;	//ちょうどいい点かどうか

					//前後と座標のなす三角形の中に他の頂点がかぶっていないか確認する
					for (int i = 0; i < positions.Length; i++) {
						if (i != prev && i != index && i != next) {
							if (positions[i].flagActive) {
								if (FuncBox.PointOnTriangle(positions[prev].vertex, positions[index].vertex, positions[next].vertex, positions[i].vertex)) {
									//Debug.Log("三角形にかぶっている点があるので再探索します。");
									flagVertex = false;
									break;
								}
							}
						}
					}

					//三点の外積を求める
					Vector3 nextVec, prevVec;	//indexから見たnextとprevのベクトルから外積を求める
					nextVec = positions[next].vertex - positions[index].vertex;
					prevVec = positions[prev].vertex - positions[index].vertex;

					Vector3 crossDir;	//外積
					crossDir = Vector3.Cross(nextVec, prevVec);	//外積を求める

					//外積の向きを比較する
					if (!FuncBox.CrossDirection(crossDirection, crossDir)) {
						//Debug.Log("外積の向きが異なります!");
						flagVertex = false;
					}

					if (flagVertex) {
						//ちょうど良い点だった場合
						//trianglesに頂点を設定し
						//indexの頂点のflagActiveをfalseにする
						//Debug.Log("ちょうどいい点が見つかりました。");
						//うまく表示されない場合はここの頂点指定が逆になっているかも
						//時計回りと反時計回りの二回分登録する(両面)
						triangles[polyIndex * 6 + 0] = next;
						triangles[polyIndex * 6 + 1] = index;
						triangles[polyIndex * 6 + 2] = prev;

						triangles[polyIndex * 6 + 3] = prev;
						triangles[polyIndex * 6 + 4] = index;
						triangles[polyIndex * 6 + 5] = next;

						positions[index].flagActive = false;
						break;
					} else {
						//駄目な点だった場合
						//indexをずらす
						index = next;
					}
					//ループカウンタ
					loopCounter++;
					//強制的にループを抜ける
					if (loopCounter > positions.Length) {
						loopCounter = 0;
						//Debug.Log("無限ループに陥る可能性があったので処理を中断しました。");
						polyIndex = polyNum;
						break;
					}
				}
		}

		ClassBox.Figure figure = new ClassBox.Figure(positions, triangles, Color.black);

		return figure;
	}
	#endregion
	#region アクセサ_面積
		/// <summary>
		/// 図形の総面積を求める
		/// </summary>
		public float GetTotalSize() {
			float total = 0;
			float size = 0;
			for (int i = 0; i < triangles.Length; i += 3) {
				size = FuncBox.TriangleSize(vertices[triangles[i + 0]].vertex,
								vertices[triangles[i + 1]].vertex,
								vertices[triangles[i + 2]].vertex);
				total += size;
			}
			//裏表貼っているので半分にする
			return totalSize = total * 0.5f;
		}
	#endregion
	#region アクセサ_頂点情報
		/// <summary>
		/// 頂点情報配列から座標を配列に変換して取得
		/// </summary>
		public Vector3[] GetVector3Array() {
			return new List<Vector3>(vertices.Select(elem => elem.vertex)).ToArray();
		}
		/// <summary>
		/// 頂点情報配列から座標をリストに変換して取得
		/// </summary>
		public List<Vector3> GetVector3List() {
			return new List<Vector3>(vertices.Select(elem => elem.vertex));
		}
		/// <summary>
		/// 頂点情報配列を取得
		/// </summary>
		public Vertex[] GetVertexArray() {
			return vertices;
		}
	#endregion
	#region アクセサ_三角形情報
		/// <summary>
		/// 三角形情報配列を取得する
		/// </summary>
		public int[] GetTriangleArray() {
			return triangles;
		}
	#endregion
	#region アクセサ_色
		/// <summary>
		/// 色を設定する。
		/// </summary>
		public void SetColor(Color c) {
			color = c;
		}
		/// <summary>
		/// 色を設定する。mrがあればマテリアルの色も変える
		/// </summary>
		public void SetColor(Color c, string colorPropertyName) {
			SetColor(c);
			//マテリアルに色を設定(nullでなければ)
			if(mr) {
				if(mr.material != null) {
					//α値は維持
					c.a = mr.material.GetColor(colorPropertyName).a;
					mr.material.SetColor(colorPropertyName, c);
				}
			}
		}
		/// <summary>
		/// 設定されている色を取得する
		/// </summary>
		public Color GetColor() {
			return color;
		}
		/// <summary>
		/// 設定されている色の配列(長さは頂点配列の長さ)
		/// </summary>
		protected Color[] GetColorArray() {
			Color[] colors = new Color[vertices.Length];
			for(int i = 0 ; i < colors.Length; i++) {
				colors[i] = color;
			}
			return colors;
		}
	#endregion
	#region アクセサ_メッシュ
		/// <summary>
		/// メッシュを取得する
		/// </summary>
		public Mesh GetMesh() {
			var mesh = new Mesh();
			mesh.Clear();

			mesh.vertices = GetVector3Array();
			mesh.colors = GetColorArray();
			mesh.triangles = triangles;

			//なんか知らんがお約束らしいので
			mesh.Optimize();
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();

			return mesh;
		}
		/// <summary>
		/// ボリュームのあるメッシュを作成する
		/// </summary>
		public Mesh GetMesh(float volume) {
			var mesh = new Mesh();
			mesh.Clear();
			//頂点配列を生成
			int i = 0, length = vertices.Length;
			Vector3[] v = new Vector3[length * 2];
			Vector3 p;
			for (; i < length; i++) {
				p = vertices[i].vertex;
				p.z = volume * -0.5f;
				v[i] = p;
				p.z += volume;
				v[i + length] = p;
			}

			//色配列も同様に
			Color[] c = new Color[length * 2];
			for (i = 0; i < length; i++) {
				c[i] = c[i + length] = color;
			}

			//トライアングルインデックス
			int tLength = triangles.Length;
			//二面分 + 側面(頂点 * 12)
			int[] t = new int[tLength * 2 + length * 12];
			//トライアングルインデックス(面)
			for (i = 0; i < tLength; i++) {
				t[i] = triangles[i];
				t[i + tLength] = triangles[i] + length;
			}

			//トライアングルインデックス(側面)
			int j = 0;
			for (; j < length - 1; j++) {
				//一つ目
				t[i + j * 12 + 0] = j + 0;
				t[i + j * 12 + 1] = j + 1;
				t[i + j * 12 + 2] = j + 0 + length;
				//一つ目(逆回り)
				t[i + j * 12 + 3] = j + 0 + length;
				t[i + j * 12 + 4] = j + 1;
				t[i + j * 12 + 5] = j + 0;

				//二つ目
				t[i + j * 12 + 6] = j + 0 + length;
				t[i + j * 12 + 7] = j + 1 + length;
				t[i + j * 12 + 8] = j + 1;
				//二つ目(逆回り)
				t[i + j * 12 + 9] = j + 1;
				t[i + j * 12 + 10] = j + 1 + length;
				t[i + j * 12 + 11] = j + 0 + length;
			}
			//最後の一面だけはj + 1の部分が0になる
			//一つ目
			t[i + j * 12 + 0] = j + 0;
			t[i + j * 12 + 1] = 0;
			t[i + j * 12 + 2] = j + 0 + length;
			//一つ目(逆回り)
			t[i + j * 12 + 3] = j + 0 + length;
			t[i + j * 12 + 4] = 0;
			t[i + j * 12 + 5] = j + 0;

			//二つ目
			t[i + j * 12 + 6] = j + 0 + length;
			t[i + j * 12 + 7] = 0 + length;
			t[i + j * 12 + 8] = 0;
			//二つ目(逆回り)
			t[i + j * 12 + 9] = 0;
			t[i + j * 12 + 10] = 0 + length;
			t[i + j * 12 + 11] = j + 0 + length;

			mesh.vertices = v;
			//mesh.uv = uvs;
			//mesh.uv2 = uvs;
			mesh.colors = c;
			mesh.triangles = t;
			//なんか知らんがお約束らしいので
			mesh.Optimize();
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			return mesh;
		}
	#endregion
	#region 作成関連
		/// <summary>
		/// ゲームオブジェクトにメッシュを設定する
		/// メッシュ以外にも様々なものを設定する
		/// </summary>
		public GameObject SetMeshToGameObject(GameObject emptyObject, Material material, Color color, string colorPropertyName = "_Color", bool flagAddCollision = false) {
			//MeshFilter
			mf = emptyObject.GetComponent<MeshFilter>();
			if (!mf) {
				mf = emptyObject.AddComponent<MeshFilter>();
			}
			mf.mesh = GetMesh();
			//MeshRenderer
			mr = emptyObject.GetComponent<MeshRenderer>();
			if (!mr) {
				mr = emptyObject.AddComponent<MeshRenderer>();
			}
			mr.material = material;
			//色
			SetColor(color, colorPropertyName);
			//当たり判定
			if(flagAddCollision) {
				MeshCollider mc = emptyObject.GetComponent<MeshCollider>();
				if(!mc) {
					mc = emptyObject.AddComponent<MeshCollider>();
				}
			}
			return emptyObject;
		}
		/// <summary>
		/// 図形のx,yそれぞれの最大点を取得する
		/// </summary>
		public Vector3[] GetMinMaxPoint() {
			//最大、最小値のインデックス
			int xMin, xMax, yMin, yMax;
			int length = vertices.Length;
			
			//最小、最大
			xMin = xMax = yMin = yMax = 0;
			for (int i = 0; i < length; i++) {
				//x
				if (vertices[i].vertex.x < vertices[xMin].vertex.x) {
					//最小
					xMin = i;
				} else if(vertices[i].vertex.x > vertices[xMax].vertex.x){
					//最大
					xMax = i;
				}

				//y
				if (vertices[i].vertex.y < vertices[xMin].vertex.y) {
					//最小
					yMin = i;
				} else if(vertices[i].vertex.y > vertices[xMax].vertex.y){
					//最大
					yMax = i;
				}
			}
			Vector3[] vList = new Vector3[4];
			vList[0] = vertices[xMin].vertex;
			vList[1] = vertices[xMax].vertex;
			vList[2] = vertices[yMin].vertex;
			vList[3] = vertices[yMax].vertex;

			return vList;
		}
		/// <summary>
		/// 図形が収まる円の半径を求める
		/// </summary>
		public float GetFullCircleRadius(float addSpace = 0f) {
			Vector3[] vList = GetMinMaxPoint();

			//xとyの最大から最小への0からの距離
			float xDis, yDis;
			if (Mathf.Abs(vList[0].x) >Mathf.Abs(vList[1].x) ) {
				xDis = Mathf.Abs(vList[0].x);
			} else {
				xDis = Mathf.Abs(vList[1].x);
			}
			if (Mathf.Abs(vList[2].y) >Mathf.Abs(vList[3].y) ) {
				yDis = Mathf.Abs(vList[2].y);
			} else {
				yDis = Mathf.Abs(vList[3].y);
			}
			//大きいほうが半径
			float d = xDis > yDis ? xDis : yDis;
			return d + addSpace;
		}
		/// <summary>
		/// ディープコピー
		/// </summary>
		public ClassBox.Figure Clone() {
			ClassBox.Figure figure = (ClassBox.Figure)MemberwiseClone();
			//メンバ
			figure.vertices = (Vertex[])vertices.Clone();
			figure.triangles = (int[])triangles.Clone();
			//実行時メンバ
			figure.mf = null;
			figure.mr = null;
			return figure;
		}
	#endregion
	#region 書き込み
		/// <summary>
		/// folderPath以下に書き込み
		/// </summary>
		public void Write(string folderPath) {
			TextWriter w;
			//ディレクトリ確認
			if(!Directory.Exists(folderPath)) {
				Directory.CreateDirectory(folderPath);
			}
			//主情報
			using (w = FuncBox.GetTextWriter(folderPath + "/Main.txt")) {
				WriteMain(w);
			}
			//頂点情報
			using (w = FuncBox.GetTextWriter(folderPath + "/Vertex.txt")) {
				WriteVertex(w);
			}
			//三角形インデックス
			using (w = FuncBox.GetTextWriter(folderPath + "/Index.txt")) {
				WriteIndex(w);
			}
		}
		/// <summary>
		/// 主情報
		/// </summary>
		protected void WriteMain(TextWriter w) {
			string line = "";
			line = totalSize + "," + color.r + "," + color.g + "," + color.b + "," + color.a;
			w.WriteLine(line);
		}
		/// <summary>
		/// 頂点情報
		/// </summary>
		protected void WriteVertex(TextWriter w) {
			string line = "";
			for (int i = 0; i < vertices.Length; i++) {
				//座標
				line = vertices[i].vertex.x + "," + vertices[i].vertex.y + "," + vertices[i].vertex.z;
				//角度
				line += "," + vertices[i].angle;

				w.WriteLine(line);
			}
		}
		/// <summary>
		/// 三角形インデックス
		/// </summary>
		protected void WriteIndex(TextWriter w) {
			string line = "";
			for(int i = 0; i < triangles.Length; i += 3) {
				//３つずつ(一つの三角形)で書き込む
				line = triangles[i] + "," + triangles[i + 1] + "," + triangles[i + 2];
				w.WriteLine(line);
			}
		}
	#endregion
	#region 読み込み
		/// <summary>
		/// folderPath以下から読み込み
		/// </summary>
		public static Figure Read(string folderPath) {
			TextReader r;
			Figure f = new Figure();

			//頂点情報
			using (r = FuncBox.GetTextReader(folderPath + "/Vertex.txt")) {
				f.vertices = ReadVertex(r).ToArray();
			}
			//三角形インデックス
			using (r = FuncBox.GetTextReader(folderPath + "/Index.txt")) {
				f.triangles = ReadIndex(r).ToArray();
			}
			//主情報
			using (r = FuncBox.GetTextReader(folderPath + "/Main.txt")) {
				ReadMain(r, f);
			}

			return f;
		}
		/// <summary>
		/// 主情報
		/// </summary>
		protected static void ReadMain(TextReader reader, Figure figure) {
			string line = "";
			string[] split;
			line = reader.ReadLine();
			split = line.Split(',');

			int i = 0;
			figure.totalSize = float.Parse(split[i]); i++;
			
			float r, g, b, a;
			r = float.Parse(split[i]); i++;
			g = float.Parse(split[i]); i++;
			b = float.Parse(split[i]); i++;
			a = float.Parse(split[i]); i++;

			//色設定
			figure.SetColor(new Color(r, g, b, a));
		}
		/// <summary>
		/// 頂点情報
		/// </summary>
		protected static List<Vertex> ReadVertex(TextReader reader) {
			string line = "";
			string[] split;
			int i = 0;
			Vertex v;
			List<Vertex> vertices = new List<Vertex>();
			while((line = reader.ReadLine()) != null) {
				i = 0;
				split = line.Split(',');

				//座標
				v = new Vertex();
				v.vertex.x = float.Parse(split[i]); i++;
				v.vertex.y = float.Parse(split[i]); i++;
				v.vertex.z = float.Parse(split[i]); i++;

				//角度
				v.angle = float.Parse(split[i]);

				//追加
				vertices.Add(v);
			}
			return vertices;
		}
		/// <summary>
		/// 三角形インデックス
		/// </summary>
		protected static List<int> ReadIndex(TextReader reader) {
			string line = "";
			string[] split;
			List<int> triangles = new List<int>();
			int i = 0;
			while((line = reader.ReadLine()) != null) {
				i = 0;
				split = line.Split(',');
				triangles.Add(int.Parse(split[i])); i++;
				triangles.Add(int.Parse(split[i])); i++;
				triangles.Add(int.Parse(split[i])); i++;
			}

			return triangles;
		}
	#endregion
	}
	/// <summary>
	/// ゲームオブジェクトのスタックなどの管理
	/// </summary>
	[System.Serializable]
	public class GameObjectStack {
	#region メンバ
		protected Stack<GameObject> stack;
	#endregion
	#region コンストラクタ
		/// <summary>
		/// デフォルト
		/// </summary>
		public GameObjectStack() {
			stack = new Stack<GameObject>();
		}
	#endregion
	#region 関数
		/// <summary>
		/// スタックから"g"が見つかるまでポップする。ついでにActiveをfalseにする
		/// </summary>
		public void StackCheck(GameObject g) {
			if(stack == null) return;
			while(stack.Count > 0) {
				if(stack.Peek() != g) {
					GameObject popG = stack.Pop();
					popG.SetActive(false);
				} else {
					break;
				}
			}
		}
		/// <summary>
		/// スタックに"g"を追加。ついでにActiveをtrueにする。
		/// </summary>
		public void Push(GameObject g) {
			g.SetActive(true);
			stack.Push(g);
		}
		/// <summary>
		/// スタックから一つ目をActiveをfalseにして取り出す
		/// </summary>
		public GameObject Pop() {
			if(stack.Count >= 1) {
				GameObject g = stack.Pop();
				g.SetActive(false);
				return g;
			} else {
				return null;
			}
		}
		/// <summary>
		/// スタックから一つ目をただ単に取り出す(スタックから削除されない)
		/// </summary>
		public GameObject Peek() {
			if(stack.Count >= 1) {
				return stack.Peek();
			} else {
				return null;
			}
		}
	#endregion
	}
}