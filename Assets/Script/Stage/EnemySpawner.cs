using UnityEngine;
using System.Collections;
/// <summary>
/// 敵を生成
/// </summary>
public class EnemySpawner : MonoBehaviour {
	//管理クラス
	private GameManager gm;
#region MonoBehaviourイベント
	private void Start() {
		gm = GameManager.Instance;
	}
#endregion
#region 
	/// <summary>
	/// ランダムに行動を設定した敵を生成する
	/// </summary>
	public Enemy InstantiateRandomActionEnemy(ToolBox.ShipData shipData) {
		switch(Random.Range(0, 3)) {
			case 0:
				//通常
				return InstantiateEnemy<Enemy>(shipData);
			case 1:
				//一人集中
				return InstantiateEnemy<Enemy_Concentration>(shipData);
			case 2:
				//優柔不断
				return InstantiateEnemy<Enemy_Indecision>(shipData);
			default:
				return null;
		}
	}
	/// <summary>
	/// ランダムに行動を設定した敵を複数生成する
	/// </summary>
	public Enemy[] InstantiateRandomActionEnemys(int num) {
		//ランダムに敵を生成
		ToolBox.ShipData shipData;
		Enemy enemy;
		Enemy[] enemys = new Enemy[num];
		for(int i = 0; i < num; i++) {
			shipData = gm.ShipDic_RandomSelect();
			enemys[i] = InstantiateRandomActionEnemy(shipData);
		}
		return enemys;
	}
	/// <summary>
	/// 敵を生成
	/// </summary>
	public T InstantiateEnemy<T>(ToolBox.ShipData shipData) where T : Enemy{
		return gm.InstantiateEnemy<T>(shipData);
	}
	/// <summary>
	/// 体力を指定して生成
	/// </summary>
	public T InstantiateEnemy<T>(ToolBox.ShipData shipData, int hp) where T : Enemy {
		T  enemy = gm.InstantiateEnemy<T>(shipData);
		enemy.ship.SetHP(hp);
		return enemy;
	}
#endregion
}