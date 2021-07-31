// Copyright (C) 2021 NeoAxis Group Ltd. 8 Copthall, Roseau Valley, 00152 Commonwealth of Dominica.
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace NeoAxis
{
	//!!!!�������� �� ������������ ��� Matrix4F

	/// <summary>
	/// A structure encapsulating a single precision 3x4 matrix.
	/// </summary>
	[StructLayout( LayoutKind.Sequential )]
	public struct Matrix3x4F
	{
		[Browsable( false )]
		public Vector4F Item0;
		[Browsable( false )]
		public Vector4F Item1;
		[Browsable( false )]
		public Vector4F Item2;
	}
}
