Shader "Yly/YlyUISeqFrameAni"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)

		_RowCount ("RowCount", float) = 0
		_ColCount ("ColCount", float) = 0
		_Speed ("Speed", float) = 30

		//用在ui上的shader一般都需要加下面的模板测试逻辑，避免被有mask组件的父节点遮罩时没遮罩效果
		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15
	}
	SubShader
	{
		Tags
		{
			"Queue" = "Transparent" //一般ui都用这种渲染队列，从远到近渲染
			"IgnoreProjector" = "True" //忽略投影，一般ui的shader为提高效率都会设置为true
			"RenderType" = "Transparent"
			"PreviewType" = "Plane" //材质球预览模式为面片
			"CanUseSpriteAtlas" = "True" //设置_MainTex可以使用Sprite(2D and UI)类型的贴图
		}

		//用在ui上的shader一般都需要加下面的模板测试逻辑，避免被有mask组件的父节点遮罩时没遮罩效果
		Stencil
		{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}

		Cull Off
		Lighting Off //关掉光照，一般ui的shader为提高效率都会这样设置
		//下面两句设置ZTest只根据ui节点的树层次（即在Hierarchy视图中的层次）作为依据进行测试，不根据z值
		ZWrite Off
		ZTest[unity_GUIZTestMode]
		//
		Blend SrcAlpha OneMinusSrcAlpha //最终颜色值 = 输出颜色值 * 输出颜色Alpha值a + 背景颜色值 * (1 - a)，即如果有透明物品挡在前面的话，类似隔着透明玻璃看东西那种效果
		ColorMask[_ColorMask]

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Speed;
			float _RowCount;
			float _ColCount;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				//从左往右，从上往下进行序列帧播放 
				float totalCount = _ColCount * _RowCount; //总帧数，例如 _RowCount = 2，_ColCount = 5，totalCount = 5 * 2 = 10
				float curIndex = floor((_Time.y * _Speed) % totalCount); //当前第几帧，例如 curIndex = 8
				float2 unitSize = float2(1 / _ColCount, 1 / _RowCount); //每一帧所占大小比例，例如 unitSize = float2(1/5, 1/2)
				float offsetU = floor(curIndex % _ColCount); //uv起点u方向偏移量，例如 offsetU = floor(8 % 5) = 3
				float offsetV = floor((totalCount - 1 - curIndex) / _ColCount); //uv起点v方向偏移量，例如 offsetV = floor((10 - 1 - 8)/5) = 0
				float2 originUv = float2(offsetU, offsetV) * unitSize; //uv起点偏移量比例，例如 originUv = float2(3 * 1/5, 0 * 1/2) = float2(3/5, 0)
				float2 newUv = originUv + i.uv * unitSize; //newUv = uv起点偏移量比例 + uv大小比例
				//例如 ui四个顶点新的uv坐标计算如下：
				//左下角newUv = float2(3/5, 0) + uv(0, 0) * float2(1/5, 1/2) = float2(3/5, 0) + float2(0, 0) = float2(3/5, 0)
				//左上角newUv = float2(3/5, 0) + uv(0, 1) * float2(1/5, 1/2) = float2(3/5, 0) + float2(0, 1/2) = float2(3/5, 1/2)
				//右上角newUv = float2(3/5, 0) + uv(1, 1) * float2(1/5, 1/2) = float2(3/5, 0) + float2(1/5, 1/2) = float2(4/5, 1/2)
				//右下角newUv = float2(3/5, 0) + uv(1, 0) * float2(1/5, 1/2) = float2(3/5, 0) + float2(1/5, 0) = float2(4/5, 0)
				fixed4 col = tex2D(_MainTex, newUv);
				return col;
			}
			ENDCG
		}
	}
}
