using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//オブジェクトプールの管理クラス
//辞書的に複数のオブジェクトを管理
public class ObjectPoolManager : SingletonMonoBehaviour<ObjectPoolManager> {
	
	[Header("プール関連パラメータ")]
	public List<KeyValuePair<string, ObjectPoolCell>> objectList;	//プーリングするオブジェクト

	//実際のオブジェクトプール
	protected Dictionary<string, List<ObjectPoolCell>> pool;		//実際のプール

#region プール管理関数
	
	//新しいプールを作成する
	protected void CreateNewPool(string key, ObjectPoolCell cell, int num) {
		
	}

#endregion
}