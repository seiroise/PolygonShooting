using UnityEngine;
using System.Collections;

//シーン開始時に実行
public class SceneStarter : MonoBehaviour {

	public GameManager gameManager;

	protected void Awake() {
		
		var gm = GameManager.Instance;
		if (!gm) {
			Instantiate(gameManager.gameObject);
		}

		Destroy(gameObject);
	}
}