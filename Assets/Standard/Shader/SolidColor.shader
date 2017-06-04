Shader "Custom/SolidColor" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
	}
	SubShader {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		//Zバッファへの書き込みをオフ
		ZWrite Off
		//ブレンド方法
		Blend SrcAlpha OneMinusSrcAlpha
		Pass {
			Color[_Color]
		}
	} 
}