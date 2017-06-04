using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
/// <summary>
/// ウェーブの管理
/// </summary>
public class WaveManager : MonoBehaviour {
	/// <summary>
	/// ウェーブに登場する敵の情報
	/// </summary>
	private class WaveEnemyInfo {
		public string shipDataName;		//機体データ名
		public string type;				//付属コンポーネントタイプ
		public int dropItemMin;			//ドロップアイテム数の最小
		public int dropItemMax;			//ドロップアイテム数の最大
		public int strongLebel;			//強さ
		public int hp;					//HP
		public Vector3 pos;				//座標
	#region コンストラクタ
		public WaveEnemyInfo() {
			shipDataName = "";
			type = "";
			dropItemMin = 0;
			dropItemMax = 0;
			strongLebel = -1;
			hp = 0;
			pos = Vector3.zero;
		}
		public WaveEnemyInfo(string shipDataName, string type,
			int dropItemMin, int dropItemMax, int strongLebel, int hp, Vector3 pos) {
			this.shipDataName = shipDataName;
			this.type = type;
			this.dropItemMin = dropItemMin;
			this.dropItemMax = dropItemMax;
			this.strongLebel = strongLebel;
			this.hp = hp;
			this.pos = pos;
		}
	#endregion
	#region IO関数
		public static WaveEnemyInfo ReadString(string[] split) {
			WaveEnemyInfo w = new WaveEnemyInfo();
			w.shipDataName = split[0];
			w.type = split[1];
			//ドロップアイテム
			string[] dropSplit = split[2].Split('-');
			w.dropItemMin = int.Parse(dropSplit[0]);
			w.dropItemMax = int.Parse(dropSplit[1]);
			w.strongLebel = int.Parse(split[3]);
			w.hp = int.Parse(split[4]);
			//座標
			string[] posSplit = split[5].Split('_');
			w.pos.x = float.Parse(posSplit[0]);
			w.pos.y = float.Parse(posSplit[1]);
			return w;
		}
	#endregion
	}
	//管理クラス
	private GameManager gm;
	[Header("デフォルトパス")]
	public string readFilePath = "WaveInfo.txt";
	//ウェーブ情報リスト
	private List<List<WaveEnemyInfo>> waveInfoList;
	[Header("機体データ")]
	public string shipDataCategory = "Enemy";
	private List<ToolBox.ShipData> shipDataList;
#region MonoBehaviourイベント
	private void Awake() {
		//読み込み
		ReadWaveInfo();
	}
	private void Start() {
		gm = GameManager.Instance;
	}
#endregion
#region 関数
	/// <summary>
	/// ウェーブ数を指定して敵を生成戻り値は生成した敵
	/// </summary>
	public Enemy[] InstantiateWaveEnemys(int waveNum, float stageScale, int numScale = 1, float hpScale = 1f) {
		//範囲確認
		if(waveNum < 0 || waveInfoList.Count <= waveNum) return null;
		//要素数確認
		if(waveInfoList[waveNum].Count < 1) return null;
		//リストの取得
		if(shipDataList == null) {
			shipDataList = new List<ToolBox.ShipData>(gm.ShipDic_Select(shipDataCategory).Values);
		}
		//生成
		ToolBox.ShipData shipData;
		List<Enemy> enemys = new List<Enemy>();
		foreach(WaveEnemyInfo eInfo in waveInfoList[waveNum]) {
			shipData = null;
			//機体データの取得
			for(int i = 0 ; i < shipDataList.Count; i++) {
				if(string.Compare(shipDataList[i].name, eInfo.shipDataName) == 0) {
					shipData = shipDataList[i];
				}
			}
			if(shipData == null) continue;
			for(int i = 0; i < numScale; i++) {
				//敵の生成
				Enemy e;
				switch(eInfo.type) {
					case "普通":
					default:
						e = gm.InstantiateEnemy<Enemy>(shipData);
					break;
					case "集中":
						e = gm.InstantiateEnemy<Enemy_Concentration>(shipData);
					break;
					case "優柔":
						e = gm.InstantiateEnemy<Enemy_Indecision>(shipData);
					break;
					case "砲台":
						e = gm.InstantiateEnemy<Enemy_Canon>(shipData);
					break;
				}
				//座標
				e.transform.position = eInfo.pos * stageScale;
				//パラメータの設定
				e.SetParametor(eInfo.strongLebel, (int)(eInfo.hp * hpScale), Random.Range(eInfo.dropItemMin, eInfo.dropItemMax));
				//追加
				enemys.Add(e);
			}
		}
		return enemys.ToArray();
	}
	/// <summary>
	/// 指定したウェーブ数が範囲内か確認
	/// </summary>
	public bool CheckWaveIndex(int waveNum) {
		if(waveNum < 0 || waveInfoList.Count <= waveNum) {
			return false;
		} else {
			return true;
		}
	}
#endregion
#region IO関数
	/// <summary>
	/// ウェーブエネミー情報をデフォルトのパスから読み込み
	/// </summary>
	public void ReadWaveInfo() {
		ReadWaveInfo(Application.streamingAssetsPath + "/" + readFilePath);
	}
	/// <summary>
	/// パスを指定してウェーブエネミー情報を読み込み
	/// </summary>
	public void ReadWaveInfo(string filePath) {
		waveInfoList = new List<List<WaveEnemyInfo>>();
		if(!File.Exists(filePath)) return;
		//読み込み
		using(TextReader r = FuncBox.GetTextReader(filePath)) {
			string line = "";
			string[] split;
			List<WaveEnemyInfo> enemyInfoList = new List<WaveEnemyInfo>();
			//ファイル走査
			while((line = r.ReadLine()) != null) {
				split = line.Split(',');
				if(split[0] == "<Wave>") {
					enemyInfoList = new List<WaveEnemyInfo>();
					waveInfoList.Add(enemyInfoList);
				} else {
					Debug.Log(split[0]);
					enemyInfoList.Add(WaveEnemyInfo.ReadString(split));
				}
			}
		}
	}
#endregion
}