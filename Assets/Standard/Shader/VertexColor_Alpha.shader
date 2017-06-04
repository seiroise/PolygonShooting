Shader "Custom/VertexColor_Alpha" {
	SubShader {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		//Zバッファへの書き込みをオフ
		ZWrite Off
		//ブレンド方法
		Blend SrcAlpha OneMinusSrcAlpha
		pass {
			cull off
			ColorMaterial AmbientAndDiffuse
		}
	}
}