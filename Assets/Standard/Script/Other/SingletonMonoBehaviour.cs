using UnityEngine;
using System.Collections;

//シングルトンクラス用の基底クラス
public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour {
	private static T instance;	//インスタンスを格納する
	public static T Instance {
		get {
			if (instance == null) {
				//instanceを検索
				instance = (T)FindObjectOfType(typeof(T));
				//nullの場合エラーを出力
				if (instance == null) {
					Debug.LogError(typeof(T) + "is nothing");
				}
			}
			return instance;
		}
	}
	//派生クラスではAwakeを必ず継承して基底関数を呼ぶこと
	protected virtual void Awake() {
		//インスタンスを確認して、自身でない場合は削除する
		if (this != Instance) {
			Destroy(this);
			return;
		}
	}
}