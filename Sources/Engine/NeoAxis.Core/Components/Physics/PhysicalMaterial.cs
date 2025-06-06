// Copyright (C) NeoAxis Group Ltd. 8 Copthall, Roseau Valley, 00152 Commonwealth of Dominica.
using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace NeoAxis
{
	/// <summary>
	/// Specifies a physical material that defines the response of a physical object when interacting with the world.
	/// </summary>
	public class PhysicalMaterial : Component
	{
		//FrictionModeEnum
		public enum FrictionModeEnum
		{
			Simple,
			Anisotropic,
			AnisotropicRolling
		}

		/// <summary>
		/// The type of friction applied on the physical body.
		/// </summary>
		[DefaultValue( FrictionModeEnum.Simple )]
		[Serialize]
		[Category( "General" )]
		public Reference<FrictionModeEnum> FrictionMode
		{
			get { if( _frictionMode.BeginGet() ) FrictionMode = _frictionMode.Get( this ); return _frictionMode.value; }
			set { if( _frictionMode.BeginSet( this, ref value ) ) { try { FrictionModeChanged?.Invoke( this ); } finally { _frictionMode.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="FrictionMode"/> property value changes.</summary>
		public event Action<PhysicalMaterial> FrictionModeChanged;
		ReferenceField<FrictionModeEnum> _frictionMode = FrictionModeEnum.Simple;

		/// <summary>
		/// The amount of friction applied on physical body.
		/// </summary>
		[DefaultValue( 0.5 )]
		[Serialize]
		[Range( 0, 1 )]
		[Category( "General" )]
		public Reference<double> Friction
		{
			get { if( _friction.BeginGet() ) Friction = _friction.Get( this ); return _friction.value; }
			set { if( _friction.BeginSet( this, ref value ) ) { try { FrictionChanged?.Invoke( this ); } finally { _friction.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="Friction"/> property value changes.</summary>
		public event Action<PhysicalMaterial> FrictionChanged;
		ReferenceField<double> _friction = 0.5;

		/// <summary>
		/// The amount of directional friction applied on physical body.
		/// </summary>
		[DefaultValue( "1 1 1" )]
		[Serialize]
		//[ApplicableRange( 0, 1 )]
		[Category( "General" )]
		public Reference<Vector3> AnisotropicFriction
		{
			get { if( _anisotropicFriction.BeginGet() ) AnisotropicFriction = _anisotropicFriction.Get( this ); return _anisotropicFriction.value; }
			set { if( _anisotropicFriction.BeginSet( this, ref value ) ) { try { AnisotropicFrictionChanged?.Invoke( this ); } finally { _anisotropicFriction.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="AnisotropicFriction"/> property value changes.</summary>
		public event Action<PhysicalMaterial> AnisotropicFrictionChanged;
		ReferenceField<Vector3> _anisotropicFriction = Vector3.One;

		/// <summary>
		/// The amount friction applied when physical body is spinning.
		/// </summary>
		[DefaultValue( 0.5 )]
		[Serialize]
		[Range( 0, 1 )]
		[Category( "General" )]
		public Reference<double> SpinningFriction
		{
			get { if( _spinningFriction.BeginGet() ) SpinningFriction = _spinningFriction.Get( this ); return _spinningFriction.value; }
			set { if( _spinningFriction.BeginSet( this, ref value ) ) { try { SpinningFrictionChanged?.Invoke( this ); } finally { _spinningFriction.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="SpinningFriction"/> property value changes.</summary>
		public event Action<PhysicalMaterial> SpinningFrictionChanged;
		ReferenceField<double> _spinningFriction = 0.5;

		/// <summary>
		/// The amount friction applied when physical body is rolling.
		/// </summary>
		[DefaultValue( 0.5 )]
		[Serialize]
		[Range( 0, 1 )]
		[Category( "General" )]
		public Reference<double> RollingFriction
		{
			get { if( _rollingFriction.BeginGet() ) RollingFriction = _rollingFriction.Get( this ); return _rollingFriction.value; }
			set { if( _rollingFriction.BeginSet( this, ref value ) ) { try { RollingFrictionChanged?.Invoke( this ); } finally { _rollingFriction.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="RollingFriction"/> property value changes.</summary>
		public event Action<PhysicalMaterial> RollingFrictionChanged;
		ReferenceField<double> _rollingFriction = 0.5;

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// The ratio of the final to initial relative velocity of the rigid body after collision.
		/// </summary>
		[DefaultValue( 0.0 )]
		[Serialize]
		[Range( 0, 1 )]
		[Category( "Rigid Body" )]
		public Reference<double> RigidRestitution
		{
			get { if( _rigidRestitution.BeginGet() ) RigidRestitution = _rigidRestitution.Get( this ); return _rigidRestitution.value; }
			set { if( _rigidRestitution.BeginSet( this, ref value ) ) { try { RigidRestitutionChanged?.Invoke( this ); } finally { _rigidRestitution.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="RigidRestitution"/> property value changes.</summary>
		public event Action<PhysicalMaterial> RigidRestitutionChanged;
		ReferenceField<double> _rigidRestitution = 0.0;

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		//!!!!disabled until all parameters fixed (SoftRigidContactHardness, etc).

		////SoftLinearStiffness
		//ReferenceField<double> _softLinearStiffness = 1.0;
		//[DefaultValue( 1.0 )]
		//[ApplicableRange( 0.0, 1.0 )]
		//[Serialize]
		//[Category( "Soft Body" )]
		//public Reference<double> SoftLinearStiffness
		//{
		//	get
		//	{
		//		if( _softLinearStiffness.BeginGet() )
		//			SoftLinearStiffness = _softLinearStiffness.Get( this );
		//		return _softLinearStiffness.value;
		//	}
		//	set
		//	{
		//		if( _softLinearStiffness.BeginSet( this, ref value ) )
		//		{
		//			try { SoftLinearStiffnessChanged?.Invoke( this ); }
		//			finally { _softLinearStiffness.EndSet(); }
		//		}
		//	}
		//}
		//public event Action<PhysicalMaterial> SoftLinearStiffnessChanged;


		////SoftAngularStiffness
		//ReferenceField<double> _softAngularStiffness = 1.0;
		//[DefaultValue( 1.0 )]
		//[ApplicableRange( 0.0, 1.0 )]
		//[Serialize]
		//[Category( "Soft Body" )]
		//public Reference<double> SoftAngularStiffness
		//{
		//	get
		//	{
		//		if( _softAngularStiffness.BeginGet() )
		//			SoftAngularStiffness = _softAngularStiffness.Get( this );
		//		return _softAngularStiffness.value;
		//	}
		//	set
		//	{
		//		if( _softAngularStiffness.BeginSet( this, ref value ) )
		//		{
		//			try { SoftAngularStiffnessChanged?.Invoke( this ); }
		//			finally { _softAngularStiffness.EndSet(); }
		//		}
		//	}
		//}
		//public event Action<PhysicalMaterial> SoftAngularStiffnessChanged;


		////SoftVolumeStiffness
		//ReferenceField<double> _softVolumeStiffness = 1.0;
		//[DefaultValue( 1.0 )]
		//[ApplicableRange( 0.0, 1.0 )]
		//[Serialize]
		//[Category( "Soft Body" )]
		//public Reference<double> SoftVolumeStiffness
		//{
		//	get
		//	{
		//		if( _softVolumeStiffness.BeginGet() )
		//			SoftVolumeStiffness = _softVolumeStiffness.Get( this );
		//		return _softVolumeStiffness.value;
		//	}
		//	set
		//	{
		//		if( _softVolumeStiffness.BeginSet( this, ref value ) )
		//		{
		//			try { SoftVolumeStiffnessChanged?.Invoke( this ); }
		//			finally { _softVolumeStiffness.EndSet(); }
		//		}
		//	}
		//}
		//public event Action<PhysicalMaterial> SoftVolumeStiffnessChanged;

		//protected override void OnMetadataGetMembersFilter( Metadata.GetMembersContext context, Metadata.Member member, ref bool skip )
		//{
		//	base.OnMetadataGetMembersFilter( context, member, ref skip );

		//	if( member is Metadata.Property )
		//	{
		//		switch( member.Name )
		//		{
		//		case nameof( RollingFriction ):
		//		case nameof( SpinningFriction ):
		//		case nameof( AnisotropicFriction ):
		//			if( FrictionMode.Value == FrictionModeEnum.Simple )
		//				skip = true;
		//			break;

		//		}
		//	}
		//}

	}
}
