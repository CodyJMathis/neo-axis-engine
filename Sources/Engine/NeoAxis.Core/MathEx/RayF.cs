// Copyright (C) NeoAxis Group Ltd. 8 Copthall, Roseau Valley, 00152 Commonwealth of Dominica.
using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace NeoAxis
{
	//[TypeConverter( typeof( MathExGeneralTypeConverter<RayF> ) )]
	/// <summary>
	/// Represents a single precision three-dimensional line based on a point in space and a direction.
	/// </summary>
	[StructLayout( LayoutKind.Sequential )]
	public struct RayF
	{
		[Serialize]
		public Vector3F Origin;
		[Serialize]
		public Vector3F Direction;

		[MethodImpl( MethodImplOptions.AggressiveInlining | (MethodImplOptions)512 )]
		public RayF( RayF source )
		{
			Origin = source.Origin;
			Direction = source.Direction;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining | (MethodImplOptions)512 )]
		public RayF( Vector3F origin, Vector3F direction )
		{
			this.Origin = origin;
			this.Direction = direction;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining | (MethodImplOptions)512 )]
		public override bool Equals( object obj )
		{
			return ( obj is RayF && this == (RayF)obj );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining | (MethodImplOptions)512 )]
		public override int GetHashCode()
		{
			return ( Origin.GetHashCode() ^ Direction.GetHashCode() );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining | (MethodImplOptions)512 )]
		public static bool operator ==( RayF v1, RayF v2 )
		{
			return ( v1.Origin == v2.Origin && v1.Direction == v2.Direction );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining | (MethodImplOptions)512 )]
		public static bool operator !=( RayF v1, RayF v2 )
		{
			return ( v1.Origin != v2.Origin || v1.Direction != v2.Direction );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining | (MethodImplOptions)512 )]
		public bool Equals( RayF v, float epsilon )
		{
			if( !Origin.Equals( v.Origin, epsilon ) )
				return false;
			if( !Direction.Equals( v.Direction, epsilon ) )
				return false;
			return true;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining | (MethodImplOptions)512 )]
		public Vector3F GetPointOnRay( float t )
		{
			Vector3F result;
			result.X = Origin.X + Direction.X * t;
			result.Y = Origin.Y + Direction.Y * t;
			result.Z = Origin.Z + Direction.Z * t;
			return result;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining | (MethodImplOptions)512 )]
		public void GetPointOnRay( float t, out Vector3F result )
		{
			result.X = Origin.X + Direction.X * t;
			result.Y = Origin.Y + Direction.Y * t;
			result.Z = Origin.Z + Direction.Z * t;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining | (MethodImplOptions)512 )]
		[AutoConvertType]
		public Ray ToRay()
		{
			Ray result;
			result.Origin = Origin.ToVector3();
			result.Direction = Direction.ToVector3();
			return result;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining | (MethodImplOptions)512 )]
		[AutoConvertType]
		public Ray2F ToRay2()
		{
			Ray2F result;
			result.Origin = Origin.ToVector2();
			result.Direction = Direction.ToVector2();
			return result;
		}

		//!!!!
		//[AutoConvertType]
		//Parse, ToString

#if !DISABLE_IMPLICIT
		[MethodImpl( MethodImplOptions.AggressiveInlining | (MethodImplOptions)512 )]
		public static implicit operator Ray( RayF v )
		{
			return new Ray( v );
		}
#endif

		[MethodImpl( MethodImplOptions.AggressiveInlining | (MethodImplOptions)512 )]
		public Vector3F GetEndPoint()
		{
			return Origin + Direction;
		}

	}
}
