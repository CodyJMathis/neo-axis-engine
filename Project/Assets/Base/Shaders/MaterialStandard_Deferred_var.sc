vec3 a_position : POSITION;
vec3 a_normal : NORMAL;
vec4 a_tangent : TANGENT;
vec2 a_texcoord0 : TEXCOORD0;
vec2 a_texcoord1 : TEXCOORD1;
vec2 a_texcoord2 : TEXCOORD2;
vec4 a_color0 : COLOR0;
float a_color2 : COLOR2 = 0.0;
float a_color3 : COLOR3 = 0.0;
uvec4 a_indices : BLENDINDICES;
vec4 a_weight : BLENDWEIGHT;
vec4 i_data0 : TEXCOORD7;
vec4 i_data1 : TEXCOORD6;
vec4 i_data2 : TEXCOORD5;
vec4 i_data3 : TEXCOORD4;
vec4 i_data4 : TEXCOORD3;

vec4 v_texCoord01 : TEXCOORD0;
vec4 v_worldPosition_depth : TEXCOORD1;
vec4 v_worldNormal_materialIndex : TEXCOORD2;
vec4 v_tangent : TEXCOORD3;
vec4 v_color0 : TEXCOORD4;
vec3 v_eyeTangentSpace : TEXCOORD5 = vec3(0.0, 0.0, 0.0);
vec3 v_normalTangentSpace : TEXCOORD6 = vec3(0.0, 0.0, 0.0);
vec4 v_position : TEXCOORD7 = vec4(0.0, 0.0, 0.0, 0.0);
vec4 v_previousPosition : TEXCOORD8 = vec4(0.0, 0.0, 0.0, 0.0);
vec4 v_texCoord23 : TEXCOORD9;
vec4 v_colorParameter : TEXCOORD10;
vec4 v_lodValue_visibilityDistance_receiveDecals_motionBlurFactor : TEXCOORD11;
vec3 v_objectSpacePosition : TEXCOORD12 = vec3(0.0, 0.0, 0.0);
vec3 v_cameraPositionObjectSpace : TEXCOORD13 = vec3(0.0, 0.0, 0.0);
vec4 v_worldMatrix0 : TEXCOORD14 = vec4(0.0, 0.0, 0.0, 0.0);
vec4 v_worldMatrix1 : TEXCOORD15 = vec4(0.0, 0.0, 0.0, 0.0);
vec4 v_worldMatrix2 : TEXCOORD16 = vec4(0.0, 0.0, 0.0, 0.0);
