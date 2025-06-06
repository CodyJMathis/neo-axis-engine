// Copyright (C) NeoAxis Group Ltd. 8 Copthall, Roseau Valley, 00152 Commonwealth of Dominica.
using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace NeoAxis
{
	/// <summary>
	/// The base class of the module for changing the velocity of a particle.
	/// </summary>
	public abstract class ParticleVelocityByTime : ParticleModule
	{
		/// <summary>
		/// The method for calculating the value.
		/// </summary>
		[DefaultValue( TypeEnum.Constant )]
		public Reference<TypeEnum> Type
		{
			get { if( _type.BeginGet() ) Type = _type.Get( this ); return _type.value; }
			set { if( _type.BeginSet( this, ref value ) ) { try { TypeChanged?.Invoke( this ); ShouldRecompileParticleSystem(); } finally { _type.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="Type"/> property value changes.</summary>
		public event Action<ParticleVelocityByTime> TypeChanged;
		ReferenceField<TypeEnum> _type = TypeEnum.Constant;

		/// <summary>
		/// Constant value over all time.
		/// </summary>
		[DefaultValue( "0 0 0" )]
		//!!!!�� ��������������[Range( -100, 100, RangeAttribute.ConvenientDistributionEnum.Exponential )]
		public Reference<Vector3F> Constant
		{
			get { if( _constant.BeginGet() ) Constant = _constant.Get( this ); return _constant.value; }
			set { if( _constant.BeginSet( this, ref value ) ) { try { ConstantChanged?.Invoke( this ); ShouldRecompileParticleSystem(); } finally { _constant.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="Constant"/> property value changes.</summary>
		public event Action<ParticleVelocityByTime> ConstantChanged;
		ReferenceField<Vector3F> _constant = Vector3F.Zero;

		/// <summary>
		/// Interval value over all time.
		/// </summary>
		[DefaultValue( "0 0 0 0 0 0" )]
		//!!!![Range
		public Reference<RangeVector3F> Range
		{
			get { if( _range.BeginGet() ) Range = _range.Get( this ); return _range.value; }
			set { if( _range.BeginSet( this, ref value ) ) { try { RangeChanged?.Invoke( this ); ShouldRecompileParticleSystem(); } finally { _range.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="Range"/> property value changes.</summary>
		public event Action<ParticleVelocityByTime> RangeChanged;
		ReferenceField<RangeVector3F> _range = RangeVector3F.Zero;

		/// <summary>
		/// A value specified by the curve for X axis over time.
		/// </summary>
		[Serialize]
		[Cloneable( CloneType.Deep )]
		public ReferenceList<CurvePoint1F> CurveX
		{
			get { return _curveX; }
		}
		public delegate void CurveChangedDelegate( ParticleVelocityByTime sender );
		public event CurveChangedDelegate CurveXChanged;
		ReferenceList<CurvePoint1F> _curveX;

		/// <summary>
		/// A value specified by the curve for Y axis over time.
		/// </summary>
		[Serialize]
		[Cloneable( CloneType.Deep )]
		public ReferenceList<CurvePoint1F> CurveY
		{
			get { return _curveY; }
		}
		public event CurveChangedDelegate CurveYChanged;
		ReferenceList<CurvePoint1F> _curveY;

		/// <summary>
		/// A value specified by the curve for Z axis over time.
		/// </summary>
		[Serialize]
		[Cloneable( CloneType.Deep )]
		public ReferenceList<CurvePoint1F> CurveZ
		{
			get { return _curveZ; }
		}
		public event CurveChangedDelegate CurveZChanged;
		ReferenceList<CurvePoint1F> _curveZ;

		/////////////////////////////////////////

		public enum TypeEnum
		{
			Constant,
			Range,
			Curve,
		}

		/////////////////////////////////////////

		public ParticleVelocityByTime()
		{
			_curveX = new ReferenceList<CurvePoint1F>( this, delegate () { CurveXChanged?.Invoke( this ); ShouldRecompileParticleSystem(); } );
			_curveY = new ReferenceList<CurvePoint1F>( this, delegate () { CurveYChanged?.Invoke( this ); ShouldRecompileParticleSystem(); } );
			_curveZ = new ReferenceList<CurvePoint1F>( this, delegate () { CurveZChanged?.Invoke( this ); ShouldRecompileParticleSystem(); } );
		}

		protected override void OnMetadataGetMembersFilter( Metadata.GetMembersContext context, Metadata.Member member, ref bool skip )
		{
			base.OnMetadataGetMembersFilter( context, member, ref skip );

			var p = member as Metadata.Property;
			if( p != null )
			{
				switch( p.Name )
				{
				case nameof( Constant ):
					if( Type != TypeEnum.Constant )
						skip = true;
					break;

				case nameof( Range ):
					if( Type != TypeEnum.Range )
						skip = true;
					break;

				case nameof( CurveX ):
				case nameof( CurveY ):
				case nameof( CurveZ ):
					if( Type != TypeEnum.Curve )
						skip = true;
					break;
				}
			}
		}
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Specifies a linear velocity of the particles depending on their lifetime.
	/// </summary>
	[NewObjectDefaultName( "Linear Velocity By Time" )]
	public class ParticleLinearVelocityByTime : ParticleVelocityByTime
	{
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Specifies an angular velocity of the particles depending on their lifetime.
	/// </summary>
	[NewObjectDefaultName( "Angular Velocity By Time" )]
	public class ParticleAngularVelocityByTime : ParticleVelocityByTime
	{
	}
}
