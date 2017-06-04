using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
/// <summary>
/// このプロジェクトオリジナルの関数やらクラス
/// </summary>
public class ToolBox {
#region クラス
	/// <summary>
	/// 機体データ
	/// </summary>
	[Serializable]
	public class ShipData {
	#region メンバ
		[Header("機体パーツ")]
		public List<ShipPartsData> shipPartsList;	//機体パーツリスト : 並び順 = 重なり順
		[Header("基本パラメータ")]
		public string category;	//カテゴリー
		public string id;		//ID
		public string name;		//名前
		public string shipSize;	//機体サイズ
		public string created;	//作成日時
		public string updated;	//更新日時
		public int updateNum;	//更新回数
		[Header("フラグ")]
		public bool flagNew;	//新規作成
		public bool flagUpdate;	//更新
	#endregion
	#region コンストラクタ
		/// <summary>
		/// GameManager経由で呼び出すこと
		/// </summary>
		public ShipData() {
			shipPartsList = new List<ShipPartsData>();
			//パラメータ
			category = "NoneCategory";
			id = "NoneID";
			name = "名無しの機体パーツ";
			shipSize = "";
			created = "";
			updated = "";
			updateNum = 0;
			//フラグ
			flagNew = false;
			flagUpdate = false;
		}
		/// <summary>
		/// GameManager経由で呼び出すこと
		/// </summary>
		public ShipData(List<ShipPartsData> shipPartsList) : this() {
			this.shipPartsList = shipPartsList;
		}
	#endregion
	#region 関数
		/// <summary>
		/// 機体パーツデータから結合したMeshを取得する
		/// </summary>
		public Mesh GetConnectedMesh() {
			List<Vector3> vertices = new List<Vector3>();
			List<Color> colors = new List<Color>();
			List<int> triangles = new List<int>();
			
			int index = 0;
			ClassBox.Figure f;
			ClassBox.Vertex[] vertexArray;
			Vector3 pos;
			int[] triangleArray;
			for(int i = 0; i < shipPartsList.Count; i++) {
				f = shipPartsList[i].figureData;
				//頂点、色
				vertexArray = f.GetVertexArray();
				for(int j = 0; j < vertexArray.Length; j++) {
					pos = vertexArray[j].vertex + shipPartsList[i].offset;
					//pos.z = ((float)i / shipPartsList.Count); ずらす
					vertices.Add(pos);
					colors.Add(f.GetColor());
				}
				//三角形インデックス
				triangleArray = f.GetTriangleArray();
				foreach(int t in triangleArray) {
					triangles.Add(t + index);
				}
				index += vertexArray.Length;
			}
			//Meshを作成
			Mesh mesh = new Mesh();
			mesh.vertices = vertices.ToArray();
			mesh.colors = colors.ToArray();
			mesh.triangles = triangles.ToArray();

			mesh.Optimize();
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();

			return mesh;
		}
		/// <summary>
		/// 機体パーツデータから結合したボリュームのあるMeshを取得する
		/// </summary>
		public Mesh GetConnectedMesh(float volume) {
			List<Vector3> vertices = new List<Vector3>();
			List<Color> colors = new List<Color>();
			List<int> triangles = new List<int>();

			int index = 0;
			Mesh m;
			Vector3 pos;
			Vector3[] vertexArray;
			Vector3 offset;
			Color c;
			int[] triangleArray;
			for(int i = 0; i < shipPartsList.Count; i++) {
				//メッシュデータを取得
				float v = ((float)(i + 1) / shipPartsList.Count) * volume;
				m = shipPartsList[i].figureData.GetMesh(v);
				//頂点、色
				vertexArray = m.vertices;
				c = shipPartsList[i].figureData.GetColor();
				for(int j = 0; j < vertexArray.Length; j++) {
					offset = shipPartsList[i].offset;
					offset.z = 0f;
					pos = vertexArray[j] + offset;
					vertices.Add(pos);
					colors.Add(c);
				}
				//三角形インデックス
				triangleArray = m.triangles;
				foreach(int t in triangleArray) {
					triangles.Add(t + index);
				}
				index += vertexArray.Length;
			}

			//Meshを作成
			Mesh mesh = new Mesh();
			mesh.vertices = vertices.ToArray();
			mesh.colors = colors.ToArray();
			mesh.triangles = triangles.ToArray();

			mesh.Optimize();
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();

			return mesh;
		}
		/// <summary>
		/// ランチャーのリストを返す
		/// </summary>
		public List<Launcher> GetLauncherList() {
			List<Launcher> lList = new List<Launcher>();
			foreach(ShipPartsData shipParts in shipPartsList) {
				foreach(Launcher l in shipParts.launcher) {
					//追加
					lList.Add(l);
				}
			}
			return lList;
		}
		/// <summary>
		/// ブースターのリストを返す
		/// </summary>
		public List<Booster> GetBoosterList() {
			List<Booster> bList = new List<Booster>();
			foreach(ShipPartsData shipParts in shipPartsList) {
				foreach(Booster b in shipParts.booster) {
					bList.Add(b);
				}
			}
			return bList;
		}
		/// <summary>
		/// ディープコピーを行う
		/// </summary>
		public ToolBox.ShipData DeepCopy() {
			ShipData sData = (ShipData)MemberwiseClone();
			//機体パーツリストのコピー
			List<ShipPartsData> partsList = new List<ShipPartsData>();
			foreach(ShipPartsData parts in shipPartsList) {
				partsList.Add(parts.DeepCopy());
			}
			sData.shipPartsList = partsList;
			return sData;
		}
	#endregion
	#region パラメータ取得用
		/// <summary>
		/// HPを取得。引数は倍率
		/// </summary>
		public int GetHP(int scale = 100) {
			float totalHP = 0;
			foreach(ShipPartsData parts in shipPartsList) {
				totalHP += parts.figureData.GetTotalSize();
			}
			return (int)(totalHP * scale);
		}
		/// <summary>
		/// ランチャーの数を数える
		/// </summary>
		public void GetLauncherNum(out int total, out int normal, out int missile, out int optical) {
			total = normal = missile = optical = 0;
			foreach(ShipPartsData parts in shipPartsList) {
				foreach(Launcher l in parts.launcher) {
					total++;
					if(l.flagUse) {
						switch(l.tag) {
							case "Normal":
							normal++;
							break;
							case "Missile":
							missile++;
							break;
							case "Optical":
							optical++;
							break;
						}
					}
				}
			}
		}
		/// <summary>
		/// ブースターの情報(数、出力)を取得する
		/// </summary>
		public void GetBoosterInfo(out int total, out float output) {
			total = 0;
			output = 0f;
			foreach(ShipPartsData parts in shipPartsList) {
				foreach(Booster b in parts.booster) {
					total++;
					//if(b.flagUse) {
					output += b.performance.output;
					//}
				}
			}
		}
		/// <summary>
		/// 現在のコストを取得する
		/// </summary>
		public int GetNowCost() {
			int cost = 0;
			foreach(ShipPartsData parts in shipPartsList) {
				foreach(Launcher l in parts.launcher) {
					if(l.flagUse) {
						//l.totalPerformance
					}
				}
			}
			return cost;
		}
	#endregion
	#region 書き込み
		/// <summary>
		/// folderPath以下に書き込み
		/// </summary>
		public void Write(string folderPath) {
			folderPath += "/" + id;
			TextWriter w;
			//ディレクトリの作成
			if(!Directory.Exists(folderPath)) {
				Directory.CreateDirectory(folderPath);
			}
			//パラメータ
			using(w = FuncBox.GetTextWriter(folderPath + "/Parameter.txt")) {
				WriteParameter(w);
			}
			//機体パーツリスト
			WriteShipPartsList(folderPath + "/ShipParts");
		}
		/// <summary>
		/// パラメータ書き込み
		/// </summary>
		protected void WriteParameter(TextWriter w) {
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("<Category>\n" + category);
			sb.AppendLine("<ID>\n" + id);
			sb.AppendLine("<Name>\n" + name);
			sb.AppendLine("<Size>\n" + shipSize);
			sb.AppendLine("<Created>\n" + created);
			sb.AppendLine("<Updated>\n" + updated);
			sb.AppendLine("<UpdateNum>\n" + updateNum);
			sb.Append("<End>");
			w.WriteLine(sb.ToString());
		}
		/// <summary>
		/// 機体パーツリストを書き込み
		/// </summary>
		protected void WriteShipPartsList(string folderPath) {
			for(int i = 0; i < shipPartsList.Count; i++) {
				//番号順に書き込み
				shipPartsList[i].Write(folderPath, i.ToString());
			}
		}
	#endregion
	#region 読み込み
		/// <summary>
		/// folderPath以下から読み込み
		/// </summary>
		public static ShipData Read(string folderPath) {
			//フォルダの存在を確認する
			if (!Directory.Exists(folderPath)) {
				return null;
			}
			ShipData data = new ShipData();
			TextReader r;
			//パラメータ
			using(r = FuncBox.GetTextReader(folderPath + "/Parameter.txt")) {
				ReadParameter(r, data);
			}
			//機体パーツ
			data.shipPartsList = ReadShipPartsList(folderPath + "/ShipParts");
			return data;
		}
		/// <summary>
		/// パラメータ読み込み
		/// </summary>
		protected static void ReadParameter(TextReader r, ShipData shipData) {
			string line = "";
			while((line = r.ReadLine()) != null) {
				switch(line) {
					case "<Category>":
						shipData.category = r.ReadLine();
					break;
					case "<ID>":
						shipData.id = r.ReadLine();
					break;
					case "<Name>":
						shipData.name = r.ReadLine();
					break;
					case "<Size>":
						shipData.shipSize = r.ReadLine();
					break;
					case "<Created>":
						shipData.created = r.ReadLine();
					break;
					case "<Updated>":
						shipData.updated = r.ReadLine();
					break;
					case "<UpdateNum>":
						shipData.updateNum = int.Parse(r.ReadLine());
					break;
					case "<End>": 	return;
				}
			}
		}
		/// <summary>
		/// 機体パーツリストを読み込み
		/// </summary>
		protected static List<ShipPartsData> ReadShipPartsList(string folderPath) {
			//ディレクトリを確認
			if (!Directory.Exists(folderPath)) {
				return null;
			}

			List<ShipPartsData> shipPartsList = new List<ShipPartsData>();

			//folderPath下にあるディレクトリを全て取得
			string[] folderList;
			folderList = Directory.GetDirectories(folderPath);
			//フォルダ名のみの配列に変換
			for (int i = 0; i < folderList.Length; i++) {
				folderList[i] = Path.GetFileName(folderList[i]);
			}
			Array.Sort(folderList, (a, b) => {
				//小さいほうを前
				return int.Parse(a) < int.Parse(b) ? -1 : 1;
			});

			//読み込み
			for (int i = 0; i < folderList.Length; i++) {
				folderList[i] = folderPath + "/" + folderList[i];
				shipPartsList.Add(ShipPartsData.Read(folderList[i]));
			}

			return shipPartsList;
		}
	#endregion
	}
	/// <summary>
	/// 機体パーツデータ
	/// </summary>
	[Serializable]
	public class ShipPartsData {
	#region メンバ
		[Header("図形情報")]
		public ClassBox.Figure figureData;		//図形データ
		[Header("特殊点情報")]
		public List<Launcher> launcher;		//ランチャー
		public List<Booster> booster;			//ブースター
		[Header("パラメータ")]
		public Vector3 offset;	//座標のずれ
		[Header("実行時")]
		public Bounds bounds;	//図形の作る矩形範囲
		public GameObject instance;	//インスタンス
	#endregion
	#region コンストラクタ
		public ShipPartsData() {
			//図形データ
			figureData = new ClassBox.Figure();
			//特殊点情報
			launcher = new List<Launcher>();
			booster = new List<Booster>();
			//パラメータ
			bounds = new Bounds();
			offset = Vector3.zero;
			//実行時
			instance = null;
		}
		public ShipPartsData(ClassBox.Figure figure) : this() {
			SetFigureData(figure);
		}
	#endregion
	#region 関数
		/// <summary>
		/// 図形データを設定する
		/// </summary>
		public void SetFigureData(ClassBox.Figure figure, bool flagSetSpeciality = true) {
			figureData = figure;
			bounds = figure.GetMesh().bounds;
			//特殊点設定
			if(flagSetSpeciality) {
				SetSpecialityPoint(figureData);
			}
		}
		/// <summary>
		/// 図形データから特殊点情報を設定する
		/// </summary>
		protected void SetSpecialityPoint(ClassBox.Figure figure) {
			launcher = new List<ToolBox.Launcher>();
			booster = new List<Booster>();
			//射撃点判定を行う
			//頂点の数が4つ以上の場合のみ判定を行う
			ClassBox.Vertex[] vertices = figure.GetVertexArray();
			if (vertices.Length >= 4) {
				float angle0, angle1;
				for (int i = 0; i < vertices.Length; i++) {
					//後ろの点3つを探してくる(iを含めると4つの点, iと3番目の点を砲身の付け根として判定する)
					int[] prevIndex;
					prevIndex = FuncBox.FindPrevPoint(i, vertices, 3);
					//0と1の角度を求める
					angle0 = vertices[prevIndex[0]].angle;
					angle1 = vertices[prevIndex[1]].angle;
					
					//ランチャー
					Launcher l = CreateLauncher(vertices, prevIndex, i, angle0, angle1);
					//リストに追加
					if (l != null) {
						launcher.Add(l);
					}

					//ブースター
					Booster b = CreateBooster(vertices, prevIndex, i, angle0, angle1);
					//リストに追加
					if (b != null) {
						booster.Add(b);
					}
				}
			}
			
		}
		/// <summary>
		/// ランチャーを作成
		/// </summary>
		protected Launcher CreateLauncher(ClassBox.Vertex[] vertices, int[] prevIndex, int i, float angle0, float angle1) {
			//足して180付近 == ほぼ並行なら
			if (179f <= angle0 + angle1 & angle0 + angle1 < 181f) {
				//中点を求める
				Vector3 center = (vertices[prevIndex[0]].vertex + vertices[prevIndex[1]].vertex) / 2;
				center.z = 0;
				//向きを求めるiから0の角度
				Vector3 angle = Vector3.zero;
				angle.z = FuncBox.TwoPointAngleD(vertices[i].vertex, vertices[prevIndex[0]].vertex);
				//射撃点の強さを決めるprevとiの距離(砲身距離)で基本攻撃力が0と1の距離(口径)で攻撃倍率が決まる
				float barrel, barrelP, barrelS, caliber;
				//砲身はiと0, 1と2の距離の長いほうが優先される
				barrelP = Vector3.Distance(vertices[i].vertex, vertices[prevIndex[0]].vertex);
				barrelS = Vector3.Distance(vertices[prevIndex[1]].vertex, vertices[prevIndex[2]].vertex);
				if (barrelP > barrelS) {
					barrel = barrelP;
				} else {
					barrel = barrelS;
				}
				caliber = Vector3.Distance(vertices[prevIndex[0]].vertex, vertices[prevIndex[1]].vertex);
				//Debug.Log("砲身長 : " + barrel + " : 口径 : " + caliber);
				//発射点情報を加える
				return new ToolBox.Launcher(center, angle, barrel, caliber);
			}
			return null;
		}
		/// <summary>
		/// ブースターを作成
		/// </summary>
		protected Booster CreateBooster(ClassBox.Vertex[] vertices, int[] prevIndex, int i, float angle0, float angle1) {
			//angle0とangle1が85度未満で角度が同じ場合
			if (angle0 < 88f && angle1 < 88f && angle0 == angle1) {
				//向きを求める
				//中点を求める
				Vector3 midPointA, midPointB;
				midPointA = (vertices[i].vertex + vertices[prevIndex[2]].vertex) / 2f;
				midPointB = (vertices[prevIndex[0]].vertex + vertices[prevIndex[1]].vertex) / 2f;
				Vector3 angle = Vector3.zero;
				angle.z = FuncBox.TwoPointAngleD(midPointA, midPointB);
				Debug.Log("----------------------------------------------------------");
				Debug.Log("ブースター : 向き" + angle.z);
				//向きが135から225の間なら
				if(135 < angle.z && angle.z < 225) {
					Debug.Log(angle.z);
					//ブースターの情報を決める
					float barrel, caliber_In, caliber_Out;
					//砲身
					Vector3 center1, center2;
					center1 = (vertices[prevIndex[0]].vertex + vertices[prevIndex[1]].vertex) / 2;
					center1.z = 0f;
					center2 = (vertices[i].vertex + vertices[prevIndex[2]].vertex) / 2;
					center2.z = 0f;
					barrel = Vector3.Distance(center1, center2);
					
					//口径
					caliber_In = Vector3.Distance(vertices[i].vertex, vertices[prevIndex[2]].vertex);
					caliber_Out = Vector3.Distance(vertices[prevIndex[0]].vertex, vertices[prevIndex[1]].vertex);
					

					Debug.Log(barrel + ", In " + caliber_In + ", Out " + caliber_Out);
					return new Booster(center1, angle, barrel, caliber_In, caliber_Out);
				}
			}
			return null;
		}
		/// <summary>
		/// ディープコピーを行う
		/// </summary>
		public ToolBox.ShipPartsData DeepCopy() {
			ToolBox.ShipPartsData shipPartsData = (ToolBox.ShipPartsData)MemberwiseClone();
			//図形データ
			 shipPartsData.figureData= figureData != null ? figureData.Clone() : null;
			//ランチャー
			if(launcher != null) {
				shipPartsData.launcher = new List<Launcher>();
				for(int i = 0; i < launcher .Count; i++) {
					shipPartsData.launcher.Add(launcher[i].DeepCopy());
				}
			} else {
				shipPartsData.launcher = null;
			}
			//ブースター
			if(booster != null) {
				shipPartsData.booster = new List<Booster>();
				for(int i = 0; i < booster .Count; i++) {
					shipPartsData.booster.Add(booster[i].DeepCopy());
				}
			} else {
				shipPartsData.booster = null;
			}
			shipPartsData.instance = null;
			return shipPartsData;
		}
		/// <summary>
		/// 文字列で情報を取得
		/// </summary>
		public string GetStringInfo() {
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("パーツ");
			sb.Append("面積 " + figureData.GetTotalSize());
			return sb.ToString();
		}
	#endregion
	#region 書き込み
		/// <summary>
		/// folderPath以下に書き込み。
		/// </summary>
		public void Write(string folderPath, string folderName) {
			folderPath += "/" + folderName;
			TextWriter w;
			//ディレクトリの作成
			if(!Directory.Exists(folderPath)) {
				Directory.CreateDirectory(folderPath);
			}
			//パラメータ
			using(w = FuncBox.GetTextWriter(folderPath + "/Parameter.txt")) {
				WriteParameter(w);
			}
			//図形
			figureData.Write(folderPath + "/Figure");
			//ランチャー
			using(w = FuncBox.GetTextWriter(folderPath + "/Launcher.txt")) {
				WriteLauncher(w);
			}
			//ブースター
			using(w = FuncBox.GetTextWriter(folderPath + "/Booster.txt")) {
				WriteBooster(w);
			}
		}
		/// <summary>
		/// パラメータ書き込み
		/// </summary>
		protected void WriteParameter(TextWriter w) {
			string str = "";
			str += "<Offset>\n" + offset.x + "," + offset.y + "," + offset.z + "\n";
			str += "<End>";
			w.WriteLine(str);
		}
		/// <summary>
		/// ランチャー情報書き込み
		/// </summary>
		protected void WriteLauncher(TextWriter w) {
			for(int i = 0; i < launcher.Count; i++) {
				w.WriteLine(launcher[i].GetWriteText());
			}
			//書き込み情報の終端目印
			w.WriteLine("<LauncherEnd>");
		}
		/// <summary>
		/// ブースター情報書き込み
		/// </summary>
		protected void WriteBooster(TextWriter w) {
			for(int i = 0; i < booster.Count; i++) {
				w.WriteLine(booster[i].GetWriteText());
			}
			//書き込み情報の終端目印
			w.WriteLine("<BoosterEnd>");
		}
	#endregion
	#region 読み込み
		/// <summary>
		/// folderPath以下から読み込み
		/// </summary>
		public static ShipPartsData Read(string folderPath) {
			//フォルダの存在を確認する
			if (!Directory.Exists(folderPath)) {
				return null;
			}
			ShipPartsData data = new ShipPartsData();
			TextReader r;
			//パラメータ
			using(r = FuncBox.GetTextReader(folderPath + "/Parameter.txt")) {
				ReadParameter(r, data);
			}
			//図形
			data.SetFigureData(ClassBox.Figure.Read(folderPath + "/Figure"), false);
			//ランチャー
			using(r = FuncBox.GetTextReader(folderPath + "/Launcher.txt")) {
				if(r != null) {
					data.launcher = ReadLauncher(r);
				}
			}
			//ブースター
			using(r = FuncBox.GetTextReader(folderPath + "/Booster.txt")) {
				if(r != null) {
					data.booster = ReadBooster(r);
				}
			}
			return data;
		}
		/// <summary>
		/// パラメータ読み込み
		/// </summary>
		protected static void ReadParameter(TextReader r, ShipPartsData shipPartsData) {
			string line = "";
			while((line = r.ReadLine()) != null) {
				switch(line) {
					case "<Offset>":
						string[] split = r.ReadLine().Split(',');
						shipPartsData.offset = new Vector3(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]));
					break;
					case "<End>": 	return;
				}
			}
		}
		/// <summary>
		/// ランチャー読み込み
		/// </summary>
		protected static List<Launcher> ReadLauncher(TextReader r) {
			List<Launcher> list = new List<Launcher>();
			Launcher l;
			while(true) {
				l = new Launcher();
				if(l.SetReadText(r)) break;
				list.Add(l);
			}
			return list;
		}
		/// <summary>
		/// ブースター読み込み
		/// </summary>
		protected static List<Booster> ReadBooster(TextReader r) {
			List<Booster> list = new List<Booster>();
			Booster b;
			while(true) {
				b = new Booster();
				if(b.SetReadText(r)) break;
				list.Add(b);
			}
			return list;
		}
	#endregion
	}
	/// <summary>
	/// 特殊点
	/// </summary>
	[Serializable]
	public class SpecialPoint {
		[Header("基底情報")]
		public Vector3 point;		//点
		public Vector3 angle;		//向き
		public bool flagUse;		//使用しているか
		public string tag;		//タグ
	#region コンストラクタ
		public SpecialPoint() {
			this.point = Vector3.zero;
			this.angle = Vector3.zero;
			this.flagUse = false;
			this.tag = "1";
		}
		public SpecialPoint(Vector3 point, Vector3 angle, string tag) {
			this.point = point;
			this.angle = angle;
			this.flagUse = false;
			this.tag = tag;
		}
	#endregion
	#region IO関数
		//書き込み用テキスト出力(UML風)
		public virtual string GetWriteText() {
			//ヘッダー
			string text = "<Special>\n";
			//座標
			text += "<Point>\n";
			text += point.x + "," + point.y + "," + point.z + "\n";
			//角度
			text += "<Angle>\n";
			text += angle.x + "," + angle.y + "," + angle.z + "\n";
			//使用してるか
			text += "<FlagUse>\n";
			text += flagUse + "\n";
			//タグ
			text += "<Tag>\n";
			text += tag + "\n";
			//終了
			text += "<End>";

			return text;
		}
		/// <summary>
		/// 出力テキスト読み込み。戻り値は\0に達したらtrueがそれ以外のときはfalse
		/// </summary>
		public virtual bool SetReadText(TextReader r) {
			string line = "";
			string[] split = null;
			//ヘッダー確認
			if((line = r.ReadLine()) == null) return true;
			split = line.Split(',');
			if(split[0] != "<Special>") return false;
			//項目確認
			while((line = r.ReadLine()) != null) {
				split = line.Split(',');
				switch(split[0]) {
					case "<Point>":
						split = r.ReadLine().Split(',');
						point = new Vector3(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]));
						break;
					case "<Angle>":
						split = r.ReadLine().Split(',');
						angle = new Vector3(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]));
						break;
					case "<FlagUse>":
						flagUse = bool.Parse(r.ReadLine());
						break;
					case "<Tag>":
						tag = r.ReadLine();
						break;
					case "<End>": return false;
				}
			}
			return true;
		}
	#endregion
	}
	/// <summary>
	/// ランチャー
	/// </summary>
	[Serializable]
	public class Launcher : SpecialPoint{
		[Header("基本情報")]
		public string bulletID;		//弾ID
		public string bulletCategory;	//弾カテゴリー
		public float barrelBonus;		//砲身ボーナス
		public float caliberBonus;	//口径ボーナス
		[Header("ランチャー性能")]
		public LauncherPerformance basePerformance;	//基本の性能
		public LauncherPerformance totalPerformance;	//弾を設定した性能
	#region コンストラクタ
		public Launcher() :base(){
			bulletID = bulletCategory = "Hoge";
			basePerformance = new LauncherPerformance();
			totalPerformance = new LauncherPerformance();
		}
		public Launcher(Vector3 point, Vector3 angle) : base(point, angle, "") {
			bulletID = bulletCategory = "Hoge";
			basePerformance = new LauncherPerformance();
			totalPerformance = new LauncherPerformance();
		}
		public Launcher(Vector3 point, Vector3 angle, float barrel, float caliber) : base(point, angle, "") {
			bulletID = bulletCategory = "Hoge";
			basePerformance = new LauncherPerformance(barrel, caliber);
			totalPerformance = new LauncherPerformance();
		}
	#endregion
	#region 関数
		/// <summary>
		/// 弾を設定する
		/// </summary>
		public void SetBullet(Bullet bullet) {
			//総合性能を決定する
			totalPerformance = LauncherPerformance.Add(basePerformance, bullet.performance);
			//ID
			bulletID = bullet.nameID;
			//カテゴリー
			bulletCategory = bullet.category;
			//タグ
			tag = bullet.category;
			//砲身ボーナス
			barrelBonus = basePerformance.barrel / bullet.performance.barrel;	//割る
			barrelBonus = barrelBonus > 2 ? 2 : barrelBonus;				//上限は2
			barrelBonus = barrelBonus >= 1 ? barrelBonus : 0;			//1~2, 0に分ける
			//口径ボーナス
			caliberBonus = basePerformance.caliber / bullet.performance.caliber;
			caliberBonus = caliberBonus > 2 ? 2 : caliberBonus;
			caliberBonus = caliberBonus >= 1 ? caliberBonus : 0;
			//使用フラグを立てる
			flagUse = true;
		}
		/// <summary>
		/// 弾を外す
		/// </summary>
		public void RemoveBullet(){
			if(flagUse) {
				bulletID = bulletCategory = "Hoge";
				totalPerformance = new LauncherPerformance();
				flagUse = false;
			}
		}
		/// <summary>
		/// ディープコピー
		/// </summary>
		public Launcher DeepCopy() {
			Launcher l = (Launcher)MemberwiseClone();
			return l;
		}
		/// <summary>
		/// 文字列で情報を取得
		/// </summary>
		public string GetStringInfo() {
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("ランチャー");
			if(flagUse) {
				sb.AppendLine("弾 : " + bulletID);
			} else {
				sb.AppendLine("弾 : 未設定");
			}
			sb.AppendLine("砲身長 : " + basePerformance.barrel + "m");
			sb.Append("口径 : " + basePerformance.caliber + "m");
			return sb.ToString();
		}
	#endregion
	#region IO関数
		/// <summary>
		/// 書き込み用文字列を生成
		/// </summary>
		public override string GetWriteText() {
			string text = base.GetWriteText();
			//ヘッダー
			text += "\n<Launcher>\n";
			//BulletID
			text += "<BulletID>\n" + bulletID + "\n";
			//BulletCategory
			text += "<BulletCategory>\n" + bulletCategory + "\n";
			//BarrelBonus
			text += "<BarrelBonus>\n" + barrelBonus + "\n";
			//CaliberBonus
			text += "<CaliberBonus>\n" + caliberBonus + "\n";
			//performance
			text += "<BasePerformance>\n" + basePerformance.GetWriteText() + "\n";
			//totalPerformance
			text += "<TotalPerformance>\n" + totalPerformance.GetWriteText() + "\n";
			//End
			text += "<End>";
			return text;
		}
		/// <summary>
		/// 出力テキストを読み込む、戻り値はテキストの末尾に達したか
		/// </summary>
		public override bool SetReadText(TextReader r) {
			base.SetReadText(r);
			
			string line = "";
			string[] split = null;
			//ヘッダー確認
			if((line = r.ReadLine()) == null) return true;
			split = line.Split(',');
			if(split[0] != "<Launcher>") return false;
			//項目確認
			while((line = r.ReadLine()) != null) {
				split = line.Split(',');
				switch(split[0]) {
					case "<BulletID>":
						bulletID = r.ReadLine();
						break;
					case "<BulletCategory>":
						bulletCategory = r.ReadLine();
						break;
					case "<BarrelBonus>":
						barrelBonus = float.Parse(r.ReadLine());
						break;
					case "<CaliberBonus>":
						caliberBonus = float.Parse(r.ReadLine());
						break;
					case "<BasePerformance>":
						basePerformance.SetReadText(r);
						break;
					case "<TotalPerformance>":
						totalPerformance.SetReadText(r);
						break;
					case "<End>": return false;
				}
			}
			return true;
		}
	#endregion
	}
	/// <summary>
	/// ランチャー性能
	/// </summary>
	[Serializable]
	public class LauncherPerformance {
		[Header("基本パラメータ")]
		public float barrel;			//砲身長
		public float caliber;			//口径
		[Header("応用パラメータ")]
		public float velocity;		//弾速
		public float damage;		//威力
		public float reloadSpeed;	//装填速度
		public int cost;				//コスト
	#region コンストラクタ
		public LauncherPerformance() {
			cost = 0;
			barrel = 0f;
			caliber = 0f;
			velocity = 0f;
			damage = 0f;
			reloadSpeed = 0f;
		}
		public LauncherPerformance(float barrel, float caliber) {
			//パラメータ設定
			SetParameter(barrel, caliber);
		}
		public LauncherPerformance(float barrel, float caliber, float velocity, float damage, float reloadSpeed, int cost) {
			this.barrel = barrel;
			this.caliber = caliber;
			this.velocity = velocity;
			this.damage = damage;
			this.reloadSpeed = reloadSpeed;
			this.cost = cost;
		}
		public LauncherPerformance(LauncherPerformance l) {
			barrel = l.barrel;
			caliber = l.caliber;
			velocity = l.velocity;
			damage = l.damage;
			reloadSpeed = l.reloadSpeed;
			cost = l.cost;
		}
	#endregion
	#region 関数
		//砲身、口径からパラメータを設定
		public void SetParameter(float barrel, float caliber){
			this.barrel = barrel;
			this.caliber = caliber;
			//砲身と口径の割り値
			float divBarrel = 10f;
			float divCaliber = 10f;

			//弾速 barrel(砲身)依存 (倍率と底上げ値は要調整)
			velocity = (barrel / divBarrel) * 50f + 50f;
			//威力 caliber(口径)依存 (倍率と底上げ値は要調整)
			damage = (caliber / divCaliber) * 50f + 100f;
			//装填速度(barrel caliber 依存) (倍率と底上げ値は要調整)
			//砲身1 : 口径 1　のときに1になるように
			reloadSpeed = ((barrel + caliber) / (divBarrel + divCaliber)) * 3f + 0.7f;
			//コスト

			//パラメータ整形
			ParameterBuild();
		}
		//パラメータの整形
		protected void ParameterBuild() {
			//小数点以下三桁に収める
			float buildNum = 1000f;
			barrel = Mathf.FloorToInt(barrel * buildNum) / buildNum;
			caliber = Mathf.FloorToInt(caliber * buildNum) / buildNum;
			velocity = Mathf.FloorToInt(velocity * buildNum) / buildNum;
			damage = Mathf.FloorToInt(damage * buildNum) / buildNum;
			reloadSpeed = Mathf.FloorToInt(reloadSpeed * buildNum) / buildNum;
		}
		//二つの性能から新しく性能を計算する
		public static LauncherPerformance Add(LauncherPerformance s, LauncherPerformance d) {
			LauncherPerformance l = new LauncherPerformance();
			//性能を計算する
			l.barrel = s.barrel;
			l.caliber = s.caliber;
			l.velocity = s.velocity * d.velocity;
			l.damage = s.damage * d.damage;
			l.reloadSpeed = s.reloadSpeed * d.reloadSpeed;
			//パラメータ整形
			l.ParameterBuild();
			return l;
		}
	#endregion
	#region IO関数
		//読み込み(CSVから)
		public int SetReadString(string[] split, int index) {
			barrel = float.Parse(split[index]);	index++;
			caliber = float.Parse(split[index]);	index++;
			velocity = float.Parse(split[index]);	index++;
			damage = float.Parse(split[index]);	index++;
			reloadSpeed = float.Parse(split[index]);	index++;
			return index;
		}
		//書き込み用テキスト
		public string GetWriteText() {
			//ヘッダー
			string text = "<LauncherPerformance>\n";
			//barrel
			text += "<Barrel>\n" + barrel + "\n";
			//caliber
			text += "<Caliber>\n" + caliber + "\n";
			//velocity
			text += "<Velocity>\n" + velocity + "\n";
			//damage
			text += "<Damage>\n" + damage + "\n";
			//reloadSpeed
			text += "<ReloadSpeed>\n" + reloadSpeed + "\n";
			//End
			text += "<End>";
			return text;
		}
		//出力テキスト読み込み
		public void SetReadText(TextReader r) {
			string line = "";
			string[] split = null;
			//ヘッダー確認
			if((line = r.ReadLine()) == null) return;
			split = line.Split(',');
			if(split[0] != "<LauncherPerformance>") return;
			//項目確認
			while((line = r.ReadLine()) != null) {
				split = line.Split(',');
				switch(split[0]) {
					case "<Barrel>" :
						barrel = float.Parse(r.ReadLine());
						break;
					case "<Caliber>" :
						caliber = float.Parse(r.ReadLine());
						break;
					case "<Velocity>" :
						velocity = float.Parse(r.ReadLine());
						break;
					case "<Damage>" :
						damage = float.Parse(r.ReadLine());
						break;
					case "<ReloadSpeed>" :
						reloadSpeed = float.Parse(r.ReadLine());
						break;
					case "<End>" : 
						SetParameter(barrel, caliber);
						return;
				}
			}
		}
	#endregion
	}
	/// <summary>
	/// バレット
	/// </summary>
	[Serializable]
	public class Bullet {
		public string category;		//カテゴリー
		public string nameID;		//名前兼ID
		public string caption;		//説明文
		public string atlasName;		//アトラス名
		public GameObject instance;	//弾のインスタンス
		public LauncherPerformance performance;	//弾の性能

		//コンストラクタ
		public Bullet() {
			this.nameID = "Hoge";
			this.caption = "説明文を書き忘れてます。\n誰か作者に教えてあげて!";
			this.instance = null;
			this.performance = new LauncherPerformance();
			atlasName = "hoge";
		}
		public Bullet(string category, string nameID, string caption, string atlasName, GameObject instance, LauncherPerformance performance) {
			this.category = category;
			this.nameID = nameID;
			this.caption = caption;
			this.atlasName = atlasName;
			this.instance = instance;
			this.performance = performance;
		}
	}
	/// <summary>
	/// ブースター
	/// </summary>
	[Serializable]
	public class Booster : SpecialPoint{
		[Header("基本情報")]
		public string boosterID;	//ブースターID
		[Header("ブースター性能")]
		public BoosterPerformance performance;
	#region コンストラクタ
		public Booster() : base() {
			boosterID = "";
			performance = new BoosterPerformance();
		}
		public Booster(Vector3 point, Vector3 angle, float barrel, float caliber_In, float caliber_Out) : base(point, angle, "") {
			boosterID = "";
			performance = new BoosterPerformance(barrel, caliber_In, caliber_Out);
		}
	#endregion
	#region 関数
		/// <summary>
		/// ディープコピー
		/// </summary>
		public Booster DeepCopy() {
			Booster b = (Booster)MemberwiseClone();
			return b;
		}
		/// <summary>
		/// 文字列で情報を取得
		/// </summary>
		public string GetStringInfo() {
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("ブースター");
			sb.AppendLine("ノズル長 : " + performance.barrel + "m");
			sb.AppendLine("ノズル口径 : " + performance.caliber_Out + "m");
			sb.Append("出力 : " + performance.output);
			return sb.ToString();
		}
	#endregion
	#region IO関数
		////書き込み用文字列を出力
		//public override string GetWriteString() {
		//	string line = base.GetWriteString();

		//	line += "," + boosterID + "," + performance.GetWriteString();

		//	return line;
		//}
		////出力文字列読み込み
		//public override int SetReadString(string[] split) {
		//	int i = base.SetReadString(split);

		//	//ブースターID
		//	boosterID = split[i];	i++;
			
		//	//性能
		//	i = performance.SetReadString(split, i);

		//	return i;
		//}
		//書き込みテキスト出力
		public override string GetWriteText() {
			string text = base.GetWriteText();
			//ヘッダー
			text += "\n<Booster>\n";
			//boosterID
			text += "<BoosterID>\n" + boosterID + "\n";
			//performance
			text += "<Performance>\n" + performance.GetWriteText() + "\n";
			//End
			text += "<End>";
			return text;
		}
		//出力テキスト読み込み
		public override bool SetReadText(TextReader r) {
			base.SetReadText(r);

			string line = "";
			string[] split = null;
			//ヘッダー確認
			if((line = r.ReadLine()) == null) return true;
			split = line.Split(',');
			if(split[0] != "<Booster>") return false;
			//項目確認
			while((line = r.ReadLine()) != null) {
				split = line.Split(',');
				switch(split[0]) {
					case "<BoosterID>":
						boosterID = r.ReadLine();
						break;
					case "<Performance>":
						performance.SetReadText(r);
						break;
					case "<End>": return false;
				}
			}
			return true;
		}
	#endregion
	}
	/// <summary>
	/// ブースター性能
	/// </summary>
	[Serializable]
	public class BoosterPerformance {
		[Header("パラメータ")]
		public float barrel;		//ノズル長
		public float caliber_In;	//ノズル口径(内側)
		public float caliber_Out;	//ノズル口径(外側)
		public float output;		//出力
		public float turn;		//旋回
	#region コンストラクタ
		public BoosterPerformance() {
			barrel = 0f;
			caliber_In = 0f;
			caliber_Out = 0f;

			output = 0f;
		}
		public BoosterPerformance(float barrel, float caliber_In, float caliber_Out) {
			this.barrel = barrel;
			this.caliber_In = caliber_In;
			this.caliber_Out = caliber_Out;

			//出力
			output = barrel * 10f + (caliber_In + caliber_Out) * 5f;
			//旋回
			turn = caliber_Out * barrel;
		}
	#endregion
	#region IO関数
		//書き込みテキスト出力
		public string GetWriteText() {
			//ヘッダー
			string text = "<BoosterPerformance>\n";
			//barrel
			text += "<Barrel>\n" + barrel + "\n";
			//caliber1
			text += "<Caliber1>\n" + caliber_In + "\n";
			//caliber2
			text += "<Caliber2>\n" + caliber_Out + "\n";
			//output
			text += "<Output>\n" + output + "\n";
			//turn
			text += "<Turn>\n" + turn + "\n";
			//End
			text += "<End>";
			return text;
		}
		//出力テキスト読み込み
		public void SetReadText(TextReader r) {
			string line = "";
			string[] split = null;
			//ヘッダー確認
			if((line = r.ReadLine()) == null) return;
			split = line.Split(',');
			if(split[0] != "<BoosterPerformance>") return;
			//項目確認
			while((line = r.ReadLine()) != null) {
				split = line.Split(',');
				switch(split[0]) {
					case "<Barrel>":
						barrel = float.Parse(r.ReadLine());
						break;
					case "<Caliber1>":
						caliber_In = float.Parse(r.ReadLine());
						break;
					case "<Caliber2>":
						caliber_Out = float.Parse(r.ReadLine());
						break;
					case "<Output>":
						output = float.Parse(r.ReadLine());
						break;
					case "<Turn>":
						turn = float.Parse(r.ReadLine());
						break;
					case "<End>": return;
				}
			}			
		}
	#endregion
	}
	/// <summary>
	/// ステージでのプレイヤーのスコア
	/// </summary>
	[Serializable]
	public class PlayerScore {
		public int playerNo;	//プレイヤー番号
		public int score;	//スコア
		public int kill;		//倒した回数
		public int dead;	//倒された回数
	#region コンストラクタ
		public PlayerScore() {
			playerNo = 0;
			score = 0;
			kill = 0;
			dead = 0;
		}
		public PlayerScore(int playerNo, int score, int kill, int dead) {
			this.playerNo = playerNo;
			this.score = score;
			this.kill = kill;
			this.dead = dead;
		}
	#endregion
	}
	/// <summary>
	/// ステージ情報
	/// </summary>
	[Serializable]
	public class Stage {
		public string name;					//名前
		public List<KeyValuePair<GameObject, Color>> stageObjectList;
		public List<Vector3> playerRespawnPos;	//プレイヤーリスポーン位置
		public List<Vector3> itemSpawnPos;	//アイテム出現位置
		public List<Vector3> enemySpawnPos;	//敵の出現位置
	#region コンストラクタ
		public Stage() {
			name = "テストステージ";
			stageObjectList = new List<KeyValuePair<GameObject,Color>>();
			playerRespawnPos = new List<Vector3>();
		}
		public Stage(string name, List<KeyValuePair<GameObject, Color>> stageObjectList, List<Vector3> playerRespawnPos) {
			this.name = name;
			this.stageObjectList = stageObjectList;
			this.playerRespawnPos = playerRespawnPos;
		}
	#endregion
	#region IO関数
		/// <summary>
		/// 読み込み
		/// </summary>
		public static Stage Read(TextReader r) {
			Stage s = new Stage();
			GameObject stageObject;
			Color color;
			string line = "";
			string[] splitLine;
			while((line = r.ReadLine()) != null) {
				Debug.Log("ReadLine" + line);
				//ヘッダ確認
				splitLine = line.Split(',');
				switch(splitLine[0]) {
					case "<Name>":
						//次の列が名前
						line = r.ReadLine();
						s.name = line;
					break;
					case "<Mesh>":
						//次の列がメッシュへのパス
						line = r.ReadLine();
						stageObject = (GameObject)Resources.Load(line);
						//次の列が色
						line = r.ReadLine();
						splitLine = line.Split(',');
						color = new Color(float.Parse(splitLine[0]) / 255f, float.Parse(splitLine[1]) / 255f, float.Parse(splitLine[2]) / 255f, float.Parse(splitLine[3]) / 255f);
						//追加
						s.stageObjectList.Add(new KeyValuePair<GameObject,Color>(stageObject, color));
					break;
					case "<Respawn>":
						//次の列から"<End>"までが座標
						float x, y, z;
						s.playerRespawnPos = new List<Vector3>();
						while((line = r.ReadLine()) != null) {
							splitLine = line.Split(',');
							if(splitLine[0] == "<RespawnEnd>") break;
							x = float.Parse(splitLine[0]);
							y = float.Parse(splitLine[1]);
							z = float.Parse(splitLine[2]);

							s.playerRespawnPos.Add(new Vector3(x, y, z));
						}
					break;
					default:
					break;
				}
			}
			return s;
		} 
	#endregion
	}
	/// <summary>
	/// 機体サイズ情報
	/// </summary>
	[Serializable]
	public class ShipSize {
		[Header("パラメータ")]
		public string name;
		public int hLineNum;	//horizontal
		public int vLineNum;	//vertical
		public int cost;
	#region コンストラクタ
		public ShipSize() {
			name = "-";
			hLineNum = 1;
			vLineNum = 1;
			cost = 0;
		}
	#endregion
	}
	/// <summary>
	/// ランキング用のレコード
	/// </summary>
	[SerializeField]
	public class RankingRecord {
		public int score;
		public int wave;
		public int player;
		public string date;

	#region コンストラクタ
		public RankingRecord() {
			score = 0;
			wave = 0;
			player = 0;
			date = "--/-- --:--";
		}
		public RankingRecord(int score, int wave, int player) {
			this.score = score;
			this.wave = wave;
			this.player = player;
			DateTime dt = System.DateTime.Now;
			//フォーマットは[02/08 11:52]みたいな
			date = dt.ToString("MM/dd HH:mm");
		}
	#endregion
	#region IO関数
		/// <summary>
		/// 書き込み文字列を取得
		/// </summary>
		public string GetWriteString() {
			return score + "," + wave + "," + player + "," + date;
		}
		/// <summary>
		/// 読み込み分割文字列を指定して新しいインスタンスを取得
		/// </summary>
		public static RankingRecord ReadString(string[] split) {
			RankingRecord r = new RankingRecord();
			r.score = int.Parse(split[0]);
			r.wave = int.Parse(split[1]);
			r.player = int.Parse(split[2]);
			r.date = split[3];
			return r;
		}
	#endregion
	}
#endregion
#region 列挙
	/// <summary>
	/// 入力方法
	/// </summary>
	public enum InputType {
		Mouse,		//マウス
		GamePad,	//ゲームパッド
		LeapMotion,	//リープモーション
	}	
	/// <summary>
	/// プレイモード
	/// </summary>
	public enum PlayMode {
		Battle,
		VsEnemy,
		Test,
		None,
	}
	/// <summary>
	/// 対戦モード
	/// </summary>
	public enum BattleMode {
		Normal,
		Item,
		Parts,
	}
	/// <summary>
	/// 機体エディット方法
	/// </summary>
	public enum ShipEditMode {
		New,	//新規作成
		Edit,		//既存を調整
	}
	/// <summary>
	/// 機体エディット状態
	/// </summary>
	public enum ShipEditState {
		Design,
		Adjust,
		Check,
		None,
	}
	/// <summary>
	/// X, Y, Zの方向
	/// </summary>
	public enum Direction {
		X,
		Y,
		Z,
	}
#endregion
}