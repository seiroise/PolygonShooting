using UnityEngine;
using System.Collections;
/// <summary>
/// プレイヤーのリザルト表示
/// </summary>
public class UIResultIndicator : MonoBehaviour {
	[Header("UIパーツ")]
	public UILabel playerNoLabel;
	public UILabel rankLabel;
	public UILabel scoreLabel;
	public UILabel killLabel;
	public UILabel deadLabel;
	[Header("Mesh")]
	public MeshFilter mf;
	public Camera meshCamera;
	[Header("Tweener")]
	public TweenPosition tweenPosition;
	[Header("Color")]
	public UIColorSetter colorSetter;
	[Header("Pos")]
	public float rankYPosOffset = -20f;		//ランクによるy座標のずれ
#region 関数
	/// <summary>
	/// 有効化
	/// </summary>
	public void Activate(int playerNo, Color color, int rank, ToolBox.PlayerScore playerScore, ToolBox.ShipData shipData) {
		//表示
		Indicate(true);
		//値の設定
		playerNoLabel.text = "Player" + playerNo;
		if(rankLabel) {
			rankLabel.text = rank.ToString();
		}
		if(scoreLabel) {
			scoreLabel.text = playerScore.score.ToString();
		}
		if(killLabel) {
			killLabel.text = playerScore.kill.ToString();
		}
		if(deadLabel) {
			deadLabel.text = playerScore.dead.ToString();
		}
		//Mesh
		mf.mesh = shipData.GetConnectedMesh();
		meshCamera.orthographicSize = mf.mesh.bounds.size.y;
		//色
		colorSetter.SetColor(color);
		//順位によって位置を変更(下位の順位ほど画面上で下へ)
		float yPos = (rank - 1) * rankYPosOffset;
		Vector3 pos = transform.localPosition;
		pos.y += yPos;
		transform.localPosition = pos;
	}
	/// <summary>
	/// 表示/非表示
	/// </summary>
	public void Indicate(bool flag) {
		gameObject.SetActive(flag);
	}
#endregion
}