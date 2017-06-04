using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

//便利な関数まとめ
public class FuncBox{
	/// <summary>
	/// 指定カメラから見たマウスの座標を取得する
	/// <para>戻り値のzは常に0</para>
	/// </summary>
	public static Vector3 GetMousePoint(Camera targetCamera) {
		Vector3 pos = Input.mousePosition;
		pos.z =  -targetCamera.transform.position.z;
		return targetCamera.ScreenToWorldPoint(pos);
	}
	/// <summary>
	/// 指定カメラからみたスクリーン座標を取得する
	/// </summary>
	public static Vector2 GetScreenPoint(Camera targetCamera, Vector3 worldPos) {
		//worldPos.z =  -targetCamera.transform.position.z;
		return targetCamera.WorldToScreenPoint(worldPos);
	}
	/// <summary>
	/// 指定カメラから指定した他のカメラ座標に変換する(ワールド座標 -> ビューポート座標 -> ワールド座標)
	/// </summary>
	public static Vector3 ViewPointTransform(Camera fromCamera, Vector3 point, Camera toCamera) {
		//ビューポート座標に変換
		Vector3 pos = fromCamera.WorldToViewportPoint(point);
		pos = toCamera.ViewportToWorldPoint(pos);
		pos.z = 0f;
		return pos;
	}
	/// <summary>
	/// 始点と向きを指定してRaycast処理を行う
	/// </summary>
	public static GameObject RaycastGameObject(Vector3 origin, Vector3 direction, string filterName = null, string filterTag = null) {
		RaycastHit hit;
		if(!Physics.Raycast(origin, direction, out hit)) return null;
		GameObject g = hit.collider.gameObject;
		//フィルター
		if(filterName != null) {
			if(g.name != filterName) return null;
		}
		if(filterTag != null) {
			if(g.tag != filterTag) return null;
		}
		return g;
	}
	/// <summary>
	/// 二点間のなす角を求める(0 ~ 360)(degree)
	/// </summary>
	public static float TwoPointAngleD(Vector3 from, Vector3 to) {
		float degree = TwoPointAngleR(from, to) * Mathf.Rad2Deg;
		//0°以下の場合360を足す
		if (degree < 0) {
			degree += 360;
		}
		return degree;
	}
	/// <summary>
	/// 二点間のなす角を求める(radian)
	/// </summary>
	public static float TwoPointAngleR(Vector3 from, Vector3 to) {
		return Mathf.Atan2(to.y - from.y, to.x - from.x);
	}
	/// <summary>
	/// 角度をベクトルに分解(x, y)
	/// </summary>
	public static Vector3 DegreeToVector3(float degree) {
		degree *= Mathf.Deg2Rad;
		return new Vector3(Mathf.Cos(degree), Mathf.Sin(degree));
	}	
	/// <summary>
	/// Vector3版Lerp
	/// </summary>
	public static Vector3 Vector3Lerp(Vector3 from, Vector3 to, float t) {
		var vec = new Vector3();
		vec.x = Mathf.Lerp(from.x, to.x, t);
		vec.y = Mathf.Lerp(from.y, to.y, t);
		vec.z = Mathf.Lerp(from.z, to.z, t);

		return vec;
	}
	/// <summary>
	/// ズーム操作
	/// </summary>
	public static void MouseZoom(Camera targetCamera, float scale = 16f, float max = 10f, float min = 5f) {
		//マウスホイールの回転を取得する
		var scroll = Input.GetAxis("Mouse ScrollWheel");
		if (scroll != 0) {
			//拡大
			if (scroll < 0) {
				targetCamera.orthographicSize +=
					targetCamera.orthographicSize * Time.deltaTime * scale;
				
				if(targetCamera.orthographicSize >= max) {
					targetCamera.orthographicSize = max;
				}
			}

			if (scroll > 0) {
				//縮小
				targetCamera.orthographicSize -=
					targetCamera.orthographicSize * Time.deltaTime * scale;
				if (targetCamera.orthographicSize <= min) {
					targetCamera.orthographicSize = min;
				}
			}
		}
	}
	/// <summary>
	/// 移動範囲、現在位置、移動量を指定して移動させる
	/// </summary>
	public static Vector2 MoveArea(Vector2 pos, Vector2 v, Rect area) {
		Vector2 p = pos + v;
		//範囲確認
		if (p.x <= area.xMin) {
			p.x = area.xMin;
		} else if (p.x >= area.xMax) {
			p.x = area.xMax;
		}

		if (p.y <= area.yMin) {
			p.y = area.yMin;
		} else if (p.y >= area.yMax) {
			p.y = area.yMax;
		}
		return p;
	}
	public static Vector2 MoveArea(Vector2 pos, Vector2 v, Rect area, out bool isOut) {
		Vector2 p = pos + v;
		isOut = false;
		//範囲確認
		if(p.x <= area.xMin) {
			p.x = area.xMin;
			isOut = true;
		} else if (p.x >= area.xMax) {
			p.x = area.xMax;
			isOut = true;
		}

		if (p.y <= area.yMin) {
			p.y = area.yMin;
			isOut = true;
		} else if(p.y >= area.yMax) {
			p.y = area.yMax;
			isOut = true;
		}
		return p;
	}
	/// <summary>
	/// ベクトルを絶対値にして返す
	/// </summary>
	public static Vector3 Vector3Abs(Vector3 source) {
		Vector3 d = new Vector3();
		d.x = Mathf.Abs(source.x);
		d.y = Mathf.Abs(source.y);
		d.z = Mathf.Abs(source.z);
		return d;
	}
	/// <summary>
	/// 高さから必要な初速を求める(重力9.81)
	/// </summary>
	public static float GetJumpVelocity(float height) {
		return Mathf.Sqrt(2 * 9.81f * height);
	}
	/// <summary>
	/// 掛け算(16ビット同士のシフト掛け算)
	/// </summary>
	public static void Multi(ushort x, ushort y, out uint z) {
		z = 0x0;			//0で初期化
		ushort i = 1;
		while (true) {
			Debug.Log(i + " : " + z);
			//第一ビットが1か確認
			if ((y & 0x0001) == 0x0001) {
				z += x;		//zにxを加算
			}
			//シフト
			y >>= 0x0001;		//yを右シフト
			x <<= 0x0001;		//xを左シフト
		
			i++;
			if (i >= 16) {
				return;
			}
		}
	}
	/// <summary>
	/// ラインレンダラーに座標を設定する
	/// <para>flagAddDoubleは同じ点を連続して二回追加するか</para>
	/// <para>二次元上の点の場合ひねりが加わるので連続して追加する必要がある</para>
	/// </summary>
	public static void SetLineRenderer(LineRenderer lineRenderer, List<Vector3> posList, bool flagAddDouble = false) {
		int count;
		if(flagAddDouble) {
			count = posList.Count * 2 - 1;
		} else {
			count = posList.Count;
		}

		lineRenderer.SetVertexCount(count);
		
		if(flagAddDouble) {
			lineRenderer.SetPosition(0, posList[0]);
			for(int i = 1; i < posList.Count; i++) {
				lineRenderer.SetPosition(i * 2  - 1, posList[i] + posList[i - 1] * 0.001f);
				lineRenderer.SetPosition(i * 2 + 0, posList[i]);
			}
		} else {
			for(int i = 0; i < posList.Count; i++) {
				lineRenderer.SetPosition(i, posList[i]);
			}
		}
	}
	/// <summary>
	/// ゲームオブジェクトにMeshを設定する。コンポーネントは自動で追加
	/// </summary>
	public static void SetMesh(GameObject g, Mesh mesh, Material material) {
		//コンポーネント追加
		MeshFilter mf = g.GetComponent<MeshFilter>();
		if(mf == null) {
			mf = g.AddComponent<MeshFilter>();
		}
		MeshRenderer mr = g.GetComponent<MeshRenderer>();
		if(mr == null) {
			mr = g.AddComponent<MeshRenderer>();
		}
		//メッシュ設定
		mf.mesh = mesh;
		//マテリアル設定
		mr.material = material;
	}
	/// <summary>
	/// 前後の点を見つける(Vector3)
	/// </summary>
	public static void FindNextPrevPoint(int index, Vector3[] positions, out int next, out int prev) {
		next = prev = -1;
		//オーバーフロー確認をしつつ頂点を探す
		int i = index + 1;
		if (i >= positions.Length) {
			i = 0;
		}
		next = i;

		//オーバーフロー確認をしつつ頂点を見つける
		int j = index - 1;
		if (j < 0) {
			j = positions.Length - 1;
		}
		prev = j;
	}
	/// <summary>
	/// 前後の点を見つける(vertex);
	/// </summary>
	public static void FindNextPrevPoint(int index, ClassBox.Vertex[] positions, out int next, out int prev, bool flag = true) {
		next = prev = -1;
		//アクティブな点を考慮するかドウか
		if (flag) {
			//オーバーフロー確認、有効確認をしつつ頂点を探す
			for (int i = 0, j = index + 1; i < positions.Length; i++, j++) {
				//オーバーフロー対策
				if (j >= positions.Length) {
					j = 0;
				}
				if (positions[j].flagActive) {
					next = j;
					break;
				}
			}
			//オーバーフロー確認、有効確認をしつつ頂点を探す
			for (int i = 0, j = index - 1; i < positions.Length; i++, j--) {
				//オーバーフロー対策
				if (j < 0) {
					j = positions.Length - 1;
				}
				if (positions[j].flagActive) {
					prev = j;
					break;
				}
			}
		} else {
			//オーバーフロー確認をしつつ頂点を探す
			int i = index + 1;
			if (i >= positions.Length) {
				i = 0;
			}
			next = i;

			//オーバーフロー確認をしつつ頂点を見つける
			int j = index - 1;
			if (j < 0) {
				j = positions.Length - 1;
			}
			prev = j;
		}
	}
	/// <summary>
	/// 色情報を16進表現文字列に変換する
	/// </summary>
	public static string ColorToHexString(Color32 color) {
		StringBuilder sb = new StringBuilder();
		sb.Append(string.Format("{0:X}", color.r));
		sb.Append(string.Format("{0:X}", color.g));
		sb.Append(string.Format("{0:X}", color.b));
		//sb.Append(string.Format("{0:X}", color.a));
		Debug.Log(sb.ToString());
		return sb.ToString();
	}
	/// <summary>
	/// 色のアルファだけを設定する
	/// </summary>
	public static Color SetColorAlpha(Color c, float alpha) {
		c.a = alpha;
		return c;
	}
	/// <summary>
	/// 対象のゲームオブジェクトにイベントを通知する
	/// </summary>
	public static void Notify(GameObject target, string functionName, object value) {
		if(target) {
			target.SendMessage(functionName, value, SendMessageOptions.DontRequireReceiver);
		}
	}
	//指定した点から指定しただけ後ろの点を探してくる(Vertex)(アクティブ確認はしない)
	public static int[] FindPrevPoint(int index, ClassBox.Vertex[] positions, int prevIndex) {
		List<int> getIndex = new List<int>();	//取得した頂点インデックス

		int j = index - 1;
		for (int i = 0; i < prevIndex; i++) {
			//オーバーフローを確認する
			if (j < 0) {
				j = positions.Length - 1;
			}
			getIndex.Add(j);
			j--;
		}

		return getIndex.ToArray();
	}

	//外積の向きがあっているかを返す
	public static bool CrossDirection(Vector3 c1, Vector3 c2) {
		//x方向
		if (c1.x > 0 && c2.x > 0 || c1.x <= 0 && c2.x <= 0) {
			if (c1.y > 0 && c2.y > 0 || c1.y <= 0 && c2.y <= 0) {
				if (c1.z > 0 && c2.z > 0 || c1.z <= 0 && c2.z <= 0) {
					return true;
				}
			}
		}
		return false;
	}

	//三点p1,p2,p3の内側に点tが存在するかを確かめる
	public static bool PointOnTriangle(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 t) {
		//p1からp2を見たときにtが左にあるか右にあるか
		//p2からp3を見たときにtが左にあるか右にあるか
		//p3からp1を見たときにtが左にあるか右にあるか
		//「この3つの位置する方向がすべて同じなら,
		//点tは3点で結ばれた三角形の内側に存在する」ということなので
		float z1, z2, z3;
		z1 = (p3.x - p2.x) * (t.y - p2.y) - (p3.y - p2.y) * (t.x - p2.x);
		z2 = (p1.x - p3.x) * (t.y - p3.y) - (p1.y - p3.y) * (t.x - p3.x);
		z3 = (p2.x - p1.x) * (t.y - p1.y) - (p2.y - p1.y) * (t.x - p1.x);
		//(外積の応用で)

		//同じ方向(正負を向いているか確認する),(=をつけると線分上の点の場合もかぶっているかの判定になる)
		if (z1 >= 0 && z2 >= 0 && z3 >= 0 || z1 <= 0 && z2 <= 0 && z3 <= 0) {
			return true;
		}
		return false;
	}

	//三点から面積を割り出す
	public static float TriangleSize(Vector3 a, Vector3 b, Vector3 c) {
		//aを原点に持ってくる
		b -= a;
		c -= a;
		return Mathf.Abs(b.x * c.y - b.y * c.x) * 0.5f;
	}

	//三点のなす角を求める
	public static float GetThreePointAngle(List<Vector3> posList, int index, int prev, int next) {
		//ベクトルを求める
		Vector3 nextVector = posList[next] - posList[index];
		Vector3 prevVector = posList[prev] - posList[index];

		//まず距離
		float nextVecDis = Vector3.Distance(posList[next], posList[index]);
		float prevVecDis = Vector3.Distance(posList[prev], posList[index]);
		//角度
		float cosS = Vector3.Dot(nextVector, prevVector) / (nextVecDis * prevVecDis);
		float radian = Mathf.Acos(cosS);
		float degree = radian * Mathf.Rad2Deg;

		return degree;
	}
	/// <summary>
	/// ランダムに色を作成する。
	/// <para>flagFixedRGBは乱数をrgbで同じ値を使うか</para>
	/// </summary>
	public static Color GetRandomColor(float min = 0f, float max = 1f, bool flagFixedRGB = false) {
		if(flagFixedRGB) {
			float v = Random.Range(min, max);
			return new Color(v, v, v);
		} else {
			return new Color(Random.Range(min, max), Random.Range(min, max), Random.Range(min, max));
		}
	}
	/// <summary>
	/// ランダムに二次元ベクトルを取得する
	/// </summary>
	public static Vector2 GetRandomVector2(float min = 0f, float max = 1f) {
		return new Vector2(Random.Range(min, max), Random.Range(min, max));
	}
	//ゲームオブジェクトをじわ～っと指定したゲームオブジェクトの方向に向ける(戻り値は指定した最大角度の何％かを返す)
	//sObjをdObjの方向に最大maxRotAngle * rotScale度向ける
	public static float RotateToGameObject(GameObject sObj, GameObject dObj, float maxRotAngle = 90f, float rotScale = 1f) {
		float targetAngle = 	Mathf.Atan2(dObj.transform.position.y - sObj.transform.position.y,
								     dObj.transform.position.x - sObj.transform.position.x);
		targetAngle *= Mathf.Rad2Deg;

		//角度を 0~ 360の間に収める
		if (targetAngle < 0) {
			targetAngle += 360;
		}

		//ここの2行がだいたい0.0000025秒程度
		float currentAngle = sObj.transform.eulerAngles.z;	//現在の角度を取得
		float diffAngle = targetAngle - currentAngle;			//目的角度と現在の角度の差

		//下のif文ハだいたい0.000005秒程度
		if (diffAngle >= 180) {
			currentAngle += 360;
		} else if (diffAngle <= -180) {
			currentAngle -= 360;
		}
		//目的角度と現在角度の距離
		//float distanceAngle = targetAngle - currentAngle;下のaddAngleの宣言に含ませる
		//現在のフレームで何度回転するかに直す
		float addAngle = (targetAngle - currentAngle) * Time.deltaTime;// * Random.Range(1f, 2f);

		//最大回転角度内に収める
		float rotS = maxRotAngle * Time.deltaTime;
		float addAngleAbs = Mathf.Abs(addAngle);
		if (addAngleAbs >= rotS) {
			addAngle = addAngle >= 0 ? rotS : -rotS;
		}
		addAngle *= rotScale;	//倍率を掛ける
		currentAngle += addAngle;
		sObj.transform.eulerAngles = new Vector3(0f, 0f, currentAngle);
		//何％回転したかを返す
		return addAngleAbs / rotS;
	}

	//ゲームオブジェクトを回転させる
	public static float RotateAngle(float from, float to, float t) {
		float diffAngle = to - from;	//目的角度と現在の角度の差

		//-からいくか+からいくか
		if (diffAngle >= 180) {
			from += 360;
		} else if (diffAngle <= -180) {
			from -= 360;
		}

		float addAngle = (to - from) * t;

		return from + addAngle;
	}

	//コンポーネントのコピー
	public static T CopyComponent<T>(T original, GameObject destination) where T : Component{
		System.Type type = original.GetType();
		Component copy = destination.AddComponent(type);
		System.Reflection.FieldInfo[] fields = type.GetFields();
		foreach (System.Reflection.FieldInfo field in fields) {
			field.SetValue(copy, field.GetValue(original));
		}
		return copy as T;
	}
	//文字コード
	//名称	: encoding
	//----------------------------------------------
	//Shift_JIS	: shift-jis
	//Unicode: utf-16
	//Readerを作成する
	public static TextReader GetTextReader(string path, string encoding = "utf-16") {
		if (File.Exists(path)) {
			return new StreamReader(path, System.Text.Encoding.GetEncoding(encoding));
		}
		return null;
	}
	//Writerを作成する
	public static TextWriter GetTextWriter(string path, string encoding = "utf-16") {
		return new StreamWriter(path, false, System.Text.Encoding.GetEncoding(encoding));
	}
	//ディープコピー[Serializable]が必要
	public static T CloneDeep<T>(T target){
		object clone = null;
		using (MemoryStream stream = new MemoryStream()) {
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream, target);
			stream.Position = 0;
			clone = formatter.Deserialize(stream);
		}
		return (T)clone;
	}
	/// <summary>
	/// 指定ベクトルに対して内積が0になるようなベクトルを取得する。aが0ベクトルの場合は0ベクトルを返す
	/// </summary>
	public static Vector3 DotZeroVector3(Vector3 a) {
		//0ベクトルでないか
		if(a == Vector3.zero) return Vector3.zero;
		//垂直ベクトル
		Vector3 v = new Vector3();
		v.x = 1f;
		v.y = 1f;
		v.z = 1f;
		//x, y, z確認
		if(a.x != 0) {
			//x
			v.x = (-(a.y * v.y)-(a.z * v.z)) / a.x;
		} else if(a.y != 0) {
			//y
			v.y = (-(a.x * v.x)-(a.z * v.z)) / a.y;
		} else {
			//z
			v.z = (-(a.x * v.x)-(a.y * v.y)) / a.z;
		}
		return v;
	}
#region 行列
	/// <summary>
	/// 移動行列の作成
	/// </summary>
	public static Matrix4x4 MatrixTranslation(Vector3 move) {
		Matrix4x4 moveMat = Matrix4x4.identity;
		moveMat[0, 3] = move.x;
		moveMat[1, 3] = move.y;
		moveMat[2, 3] = move.z;
		return moveMat;
	}
	/// <summary>
	/// 移動行列の作成
	/// </summary>
	public static Matrix4x4 MatrixTranslation(float x, float y, float z) {
		return MatrixTranslation(new Vector3(x, y, z));
	}
	/// <summary>
	/// X軸回転行列の作成(右手座標系)
	/// </summary>
	public static Matrix4x4 MatrixRotationX(float radian) {
		Matrix4x4 rotMat = Matrix4x4.identity;
		rotMat[1, 1] = Mathf.Cos(radian);
		rotMat[2, 1] = Mathf.Sin(radian);
		rotMat[1, 2] = -Mathf.Sin(radian);
		rotMat[2, 2] = Mathf.Cos(radian);
		return rotMat;
	}
	/// <summary>
	/// Y軸回転行列の作成(右手座標系)
	/// </summary>
	public static Matrix4x4 MatrixRotationY(float radian) {
		Matrix4x4 rotMat = Matrix4x4.identity;
		rotMat[0, 0] = Mathf.Cos(radian);
		rotMat[2, 0] = -Mathf.Sin(radian);
		rotMat[0, 2] = Mathf.Sin(radian);
		rotMat[2, 2] = Mathf.Cos(radian);
		return rotMat;
	}
	/// <summary>
	/// Z軸回転行列の作成(右手座標系)
	/// </summary>
	public static Matrix4x4 MatrixRotationZ(float radian) {
		Matrix4x4 rotMat = Matrix4x4.identity;
		rotMat[0, 0] = Mathf.Cos(radian);
		rotMat[1, 0] = Mathf.Sin(radian);
		rotMat[0, 1] = -Mathf.Sin(radian);
		rotMat[1, 1] = Mathf.Cos(radian);
		return rotMat;
	}
	/// <summary>
	/// 行列を使って座標を移動
	/// </summary>
	public static Vector3[] MoveMatrix(Vector3[] vertices, Vector3 move) {
		Matrix4x4 posMat, moveMat;
		moveMat = MatrixTranslation(move);
		for(int i = 0; i < vertices.Length; i++) {
			posMat = MatrixTranslation(vertices[i]);
			posMat *= moveMat;
			vertices[i].x = posMat[0, 3];
			vertices[i].y = posMat[1, 3];
			vertices[i].z = posMat[2, 3];
		}
		return vertices;
	}
	/// <summary>
	/// 行列を使って座標をZ軸回転
	/// </summary>
	public static Vector3[] RotationMatrixZ(Vector3[] vertices, float radian) {
		Matrix4x4 posMat, rotMat;
		rotMat = MatrixRotationZ(radian);
		for(int i = 0; i < vertices.Length; i++) {
			posMat = MatrixTranslation(vertices[i]);
			posMat *= rotMat;
			vertices[i].x = posMat[0, 3];
			vertices[i].y = posMat[1, 3];
			vertices[i].z = posMat[2, 3];
		}
		return vertices;
	}
#endregion
#region コルーチン
	//図形作成コルーチン
	//図形作成完了後にiventReceverにイベントを送る
	public static IEnumerator MeshCreatingCoroutine(Mesh mesh, MeshFilter mf, Material material,
		GameObject iventRecever, string iventName = "OnMeshCreated", float time = 1f) {

		Mesh processMesh = new Mesh();			//処理メッシュ
		List<int> triangleList = new List<int>();		//三角形インデックス
		int i = 0;
		float cWait;							//追加間隔

		processMesh.Clear();

		//あらかじめtriangles以外は埋めておく
		processMesh.vertices = mesh.vertices;
		processMesh.colors = mesh.colors;

		//最初の三角形を加える
		for (; i < 3; i++) {
			triangleList.Add(mesh.triangles[i]);
		}
		processMesh.triangles = triangleList.ToArray();
		mf.mesh = processMesh;
		//追加間隔
		cWait = time / (mesh.triangles.Length / 3); 
		//追加用ループ
		for (; i < mesh.triangles.Length; i += 3) {;
			yield return new WaitForSeconds(cWait);

			//リストに追加
			for (int j = 0; j < 3; j++) {
				triangleList.Add(mesh.triangles[i + j]);
			}
			//メッシュに設定
			processMesh.triangles = triangleList.ToArray();
			mf.mesh = processMesh;

			mf.mesh.RecalculateNormals();
		}

		//イベント送信
		if (iventRecever) {
			iventRecever.SendMessage(iventName, mf, SendMessageOptions.DontRequireReceiver);
		}
	}
#endregion
}