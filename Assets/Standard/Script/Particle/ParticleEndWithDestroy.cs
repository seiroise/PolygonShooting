using UnityEngine;
using System.Collections;
/// <summary>
/// パーティクルの終了と同時にDestroyする
/// </summary>
public class ParticleEndWithDestroy : MonoBehaviour {	
	public ParticleSystem target;
	private void Start() {
		if(!target) {
			target = gameObject.GetComponent<ParticleSystem>();
			if(!target) Destroy(this);
		}
	}
	private void Update () {
		if(target.isStopped && target.particleCount <= 0) {
			Destroy(gameObject);
		}
	}
	private void OnDisable() {
		enabled = true;
	}
}
