using UnityEngine;
using System.Collections;
/// <summary>
/// カメラ効果Glitchを操作する
/// </summary>
public class GlitchController : GlitchFx {
	protected float measureTime = -1f;
	protected float time = 0f;
	protected float power = 1f;
#region MonoBehaviourイベント
	protected override void Update() {
		if(measureTime >= 0) {
			intensity = (measureTime / time) * power;
			measureTime -= Time.deltaTime;
			if(measureTime < 0) {
				intensity = 0f;
			}
			base.Update();
		}
	}
#endregion
#region 関数
	/// <summary>
	/// 時間と強さを指定して効果を掛ける
	/// <para>powerは0 ~ 1の間で</para>
	/// </summary>
	public void SetGlitch(float power, float time) {
		this.power = power;
		measureTime = this.time = time;
	}
#endregion
}