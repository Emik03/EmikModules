Shader "KT/OctadecayottonShader"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // These constants can be modified, but should be kept as low as possible.
            // The maximum number of dimensions this shader can support.
            #define MAX_DIMENSIONS 30
            // The maximum number of subrotations in one rotation.
            // Corresponds to RotationOptions.MaxRotations
            #define MAX_ROTATIONS 5
            // The maximum number of axes in one subrotation.
            // Corresponds to RotationOptions.MaxLengthPerRotation
            #define MAX_AXES_PER_ROTATION 5
            // The maximum number of dimensions in one mesh.
            // Corresponds to OctadecayottonHypercube.MeshLimit
            #define MESH_LIMIT 13
            // The maximum number of vertices in one sphere.
            #define SPHERE_VERTICES 17

            uniform int _dimensions;
            uniform float4 _basis[MAX_DIMENSIONS];
            uniform int _rotation[MAX_ROTATIONS * MAX_AXES_PER_ROTATION];
            uniform int _invert[MAX_ROTATIONS * MAX_AXES_PER_ROTATION];
            uniform int _rotationSizes[MAX_ROTATIONS];
            uniform int _rotations;
            uniform float _t;
            uniform int _indexOffset;
            uniform float4 _maxOffset;
            uniform int _skipSpheres;
            uniform float _blendSphereColor;
            uniform float _blendFixedColor;
            uniform fixed4 _fixedColor;
            uniform float _jitterScale;
            uniform bool _solveAnimation;
            uniform float4 _sphereBasis[SPHERE_VERTICES];

            struct appdata
            {
                float4 vertex : POSITION;
                float2 index : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 spatialColor : TEXCOORD1;
                fixed4 color : COLOR;
            };

            // Constants chosen randomly. They don't need to be secure, just good enough visually.
            // _Time is automatically defined and set by Unity.
            float3 random (float st) {
				float3 OUT = float3(
                    sin(dot(abs(st.xx),float2(59.1793,27.9671)) + _Time.z * 400)*34.6167,
                    sin(dot(abs(st.xx),float2(54.4223,70.6064)) + _Time.z * 6400)*77.8785,
                    sin(dot(abs(st.xx),float2(90.6697,2.1329)) + _Time.z * 900)*90.2709);
				return abs(OUT) - floor(abs(OUT));
			}

            float4 axesToPos(in bool ix[MAX_DIMENSIONS])
            {
                float4 sum = float4(0, 0, 0, 0);
                for(int i = 0; i < _dimensions; i++)
                    sum += _basis[i] * ix[i];
                return sum;
            }

            #define P1 2000000153
            #define P2 500000089
            #define P3 2000000011
            #define P4 500000071
            void next(inout bool ix[MAX_DIMENSIONS], in int num)
            {
                num *= P1;
                num += P2;
                num %= 1 << min(_dimensions, MESH_LIMIT);
                int num2 = _indexOffset;
                num2 *= P3;
                num2 += P4;
                num2 %= max(1 <<_dimensions, 1);

                for(int i = 0; i < MESH_LIMIT; i++)
                {
                    ix[i] = num & 1;
                    num >>= 1;
                }
                for(i = MESH_LIMIT; i < _dimensions; i++)
                {
                    ix[i] = num2 & 1;
                    num2 >>= 1;
                }
                for(i = 0; i < _dimensions - MESH_LIMIT; i++)
                {
                    bool temp = ix[i];
                    ix[i] = ix[i + MESH_LIMIT];
                    ix[i + MESH_LIMIT] = temp;
                }
            }


            void swizzle(inout bool ix[MAX_DIMENSIONS])
            {
                for(int r = 0; r < _rotations; r++)
                {
                    int temp = ix[_rotation[MAX_AXES_PER_ROTATION * r]];
                    for(int i = 0; i < _rotationSizes[r] - 1; i++)
                        ix[_rotation[i + MAX_AXES_PER_ROTATION * r]] = _invert[i + MAX_AXES_PER_ROTATION * r] ^ ix[_rotation[i + 1 + MAX_AXES_PER_ROTATION * r]];
                    ix[_rotation[_rotationSizes[r] - 1 + MAX_AXES_PER_ROTATION * r]] = _invert[_rotationSizes[r] - 1 + MAX_AXES_PER_ROTATION * r] ^ temp;
                }
            }

            v2f vert (appdata v)
            {
                int blah = v.index.x;
                int blah2 = blah + _indexOffset;

                bool indices[MAX_DIMENSIONS];
                for(int i = 0; i < MESH_LIMIT; i++)
                {
                    indices[i] = blah & 1;
                    blah >>= 1;
                }
                blah = _indexOffset;
                for(i = MESH_LIMIT; i < _dimensions; i++)
                {
                    indices[i] = abs(fmod(blah, 2) - 1) < .5;
                    blah >>= 1;
                }
                v2f o;
                //o.color = fixed4(indices[0],indices[1],indices[2],1);
                o.color = v.color;

                float4 current = axesToPos(indices);
                if(_solveAnimation)
                    next(indices, v.index.x);
                else
                    swizzle(indices);
                float4 target = axesToPos(indices);

                float4 sphereOffset = lerp(current, target, _t) + _jitterScale * float4(random(blah2), 0);

                o.vertex = UnityObjectToClipPos(lerp(_sphereBasis[v.index.y] + sphereOffset, float4(0, 0, 0, 0),  v.index.x < _skipSpheres));

                o.spatialColor = sphereOffset / _maxOffset;

                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = lerp(lerp(i.spatialColor, i.color, _blendSphereColor), _fixedColor, _blendFixedColor);
                col.a = 1;
                return col;
            }
            ENDCG
        }
    }
}