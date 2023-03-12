$input a_position, a_texcoord0
$output v_texCoord0

// Copyright (C) NeoAxis Group Ltd. 8 Copthall, Roseau Valley, 00152 Commonwealth of Dominica.
#include "Common.sh"
#include "UniformsVertex.sh"

//uniform vec4 u_lightDataVertex[LIGHTDATA_VERTEX_SIZE];

void main()
{
	//mat4 worldMatrix = u_model[0];
	//vec4 worldPosition = mul(worldMatrix, vec4(a_position, 1.0));

	//!!!!
	gl_Position = vec4(a_position, 1.0);
	//gl_Position = mul(u_modelViewProj, vec4(a_position, 1.0));

	v_texCoord0 = a_texcoord0;
/*
	v_worldPosition = worldPosition.xyz;
	v_worldNormal = normalize(mul(toMat3(worldMatrix), a_normal));
	v_depth = gl_Position.z;
	v_tangent = normalize(mul(toMat3(worldMatrix), a_tangent.xyz));
	v_bitangent = cross(v_tangent.xyz, v_worldNormal) * a_tangent.w;

	//!!!!
	v_reflectionVector = v_worldPosition - cameraPosition.xyz;
*/
}
