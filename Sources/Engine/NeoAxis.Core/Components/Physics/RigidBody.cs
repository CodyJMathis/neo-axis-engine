// Copyright (C) NeoAxis Group Ltd. 8 Copthall, Roseau Valley, 00152 Commonwealth of Dominica.
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using System.Runtime.CompilerServices;

namespace NeoAxis
{
	/// <summary>
	/// A rigid physical body.
	/// </summary>
	public class RigidBody : PhysicalBody
	{
		static FastRandom staticRandom = new FastRandom( 0 );

		//!!!!������� ��� ��� ����� ������

		Scene.PhysicsWorldClass.Body physicalBody;
		Vector3 physicalBodyCreatedTransformScale;

		//internal CollisionShape[] collisionShapeByIndex;

		bool duringCreateDestroy;
		bool updatePropertiesWithoutUpdatingBody;

		bool collisionSoundSpecified;
		float collisionSoundRemainingTime;

		//!!!!need?
		//Matrix4 centerOfMassOffset;
		//Matrix4 centerOfMassOffsetInverted;

		//!!!!
		//class CenterOfMassGeometry
		//{
		//	public float radius;
		//	public Vector3F[] positions;
		//	public int[] indices;
		//}
		//static List<CenterOfMassGeometry> centerOfMassGeometryCache = new List<CenterOfMassGeometry>();

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// The type of motion used.
		/// </summary>
		[DefaultValue( PhysicsMotionType.Static )]
		[Category( "Rigid Body" )]
		public Reference<PhysicsMotionType> MotionType
		{
			get { if( _motionType.BeginGet() ) MotionType = _motionType.Get( this ); return _motionType.value; }
			set
			{
				if( _motionType.BeginSet( this, ref value ) )
				{
					try
					{
						MotionTypeChanged?.Invoke( this );

						//!!!!����������� �� �������������? �����������?
						if( physicalBody != null )
							RecreateBody();
					}
					finally { _motionType.EndSet(); }
				}
			}
		}
		/// <summary>Occurs when the <see cref="MotionType"/> property value changes.</summary>
		public event Action<RigidBody> MotionTypeChanged;
		ReferenceField<PhysicsMotionType> _motionType = PhysicsMotionType.Static;

		/// <summary>
		/// The method of detecting collisions to solve issues on a high velocity.
		/// </summary>
		[DefaultValue( PhysicsMotionQuality.Discrete )]
		[Category( "Rigid Body" )]
		public Reference<PhysicsMotionQuality> MotionQuality
		{
			get { if( _motionQuality.BeginGet() ) MotionQuality = _motionQuality.Get( this ); return _motionQuality.value; }
			set
			{
				if( _motionQuality.BeginSet( this, ref value ) )
				{
					try
					{
						MotionQualityChanged?.Invoke( this );
						if( physicalBody != null )
							physicalBody.MotionQuality = MotionQuality;
					}
					finally { _motionQuality.EndSet(); }
				}
			}
		}
		/// <summary>Occurs when the <see cref="MotionQuality"/> property value changes.</summary>
		public event Action<RigidBody> MotionQualityChanged;
		ReferenceField<PhysicsMotionQuality> _motionQuality = PhysicsMotionQuality.Discrete;

		/// <summary>
		/// The mass of the rigid body.
		/// </summary>
		[DefaultValue( 1.0 )]
		[Category( "Rigid Body" )]
		public Reference<double> Mass
		{
			get { if( _mass.BeginGet() ) Mass = _mass.Get( this ); return _mass.value; }
			set
			{
				if( value < 0 )
					value = new Reference<double>( 0, value.GetByReference );
				if( _mass.BeginSet( this, ref value ) )
				{
					try
					{
						MassChanged?.Invoke( this );

						//!!!!����������� �� �������������? �����������?
						if( physicalBody != null )
							RecreateBody();
					}
					finally { _mass.EndSet(); }
				}
			}
		}
		/// <summary>Occurs when the <see cref="Mass"/> property value changes.</summary>
		public event Action<RigidBody> MassChanged;
		ReferenceField<double> _mass = 1;

		/// <summary>
		/// Offset of the center of mass from the automatically calculated value.
		/// </summary>
		[Category( "Rigid Body" )]
		[DefaultValue( "0 0 0" )]
		public Reference<Vector3> CenterOfMassOffset
		{
			get { if( _centerOfMassOffset.BeginGet() ) CenterOfMassOffset = _centerOfMassOffset.Get( this ); return _centerOfMassOffset.value; }
			set
			{
				if( _centerOfMassOffset.BeginSet( this, ref value ) )
				{
					try
					{
						CenterOfMassOffsetChanged?.Invoke( this );

						//!!!!����������� �� �������������? �����������?
						if( physicalBody != null )
							RecreateBody();
					}
					finally { _centerOfMassOffset.EndSet(); }
				}
			}
		}
		/// <summary>Occurs when the <see cref="CenterOfMassOffset"/> property value changes.</summary>
		public event Action<RigidBody> CenterOfMassOffsetChanged;
		ReferenceField<Vector3> _centerOfMassOffset = new Vector3( 0, 0, 0 );

		///// <summary>
		///// Whether the rigid body is using manual center of mass.
		///// </summary>
		//[Category( "Rigid Body" )]
		//[DefaultValue( false )]
		//public Reference<bool> CenterOfMassManual
		//{
		//	get { if( _centerOfMassManual.BeginGet() ) CenterOfMassManual = _centerOfMassManual.Get( this ); return _centerOfMassManual.value; }
		//	set
		//	{
		//		if( _centerOfMassManual.BeginSet( this, ref value ) )
		//		{
		//			try
		//			{
		//				CenterOfMassManualChanged?.Invoke( this );

		//				//!!!!����������� �� �������������? �����������?
		//				if( physicalBody != null )
		//					RecreateBody();
		//			}
		//			finally { _centerOfMassManual.EndSet(); }
		//		}
		//	}
		//}
		///// <summary>Occurs when the <see cref="CenterOfMassManual"/> property value changes.</summary>
		//public event Action<RigidBody> CenterOfMassManualChanged;
		//ReferenceField<bool> _centerOfMassManual = false;

		///// <summary>
		///// The offset of center of mass from the body position.
		///// </summary>
		//[Category( "Rigid Body" )]
		//[DefaultValue( "0 0 0" )]
		//public Reference<Vector3> CenterOfMassPosition
		//{
		//	get { if( _centerOfMassPosition.BeginGet() ) CenterOfMassPosition = _centerOfMassPosition.Get( this ); return _centerOfMassPosition.value; }
		//	set
		//	{
		//		if( _centerOfMassPosition.BeginSet( this, ref value ) )
		//		{
		//			try
		//			{
		//				CenterOfMassPositionChanged?.Invoke( this );

		//				//!!!!����������� �� �������������? �����������?
		//				if( physicalBody != null )
		//					RecreateBody();
		//			}
		//			finally { _centerOfMassPosition.EndSet(); }
		//		}
		//	}
		//}
		///// <summary>Occurs when the <see cref="CenterOfMassPosition"/> property value changes.</summary>
		//public event Action<RigidBody> CenterOfMassPositionChanged;
		//ReferenceField<Vector3> _centerOfMassPosition = new Vector3( 0, 0, 0 );

		/// <summary>
		/// The factor of affect global gravity.
		/// </summary>
		[Category( "Rigid Body" )]
		[DefaultValue( 1.0 )]
		[Range( 0, 2 )]
		public Reference<double> GravityFactor
		{
			get { if( _gravityFactor.BeginGet() ) GravityFactor = _gravityFactor.Get( this ); return _gravityFactor.value; }
			set
			{
				if( _gravityFactor.BeginSet( this, ref value ) )
				{
					try
					{
						GravityFactorChanged?.Invoke( this );
						if( physicalBody != null )
							physicalBody.GravityFactor = (float)GravityFactor;
					}
					finally { _gravityFactor.EndSet(); }
				}
			}
		}
		/// <summary>Occurs when the <see cref="GravityFactor"/> property value changes.</summary>
		public event Action<RigidBody> GravityFactorChanged;
		ReferenceField<double> _gravityFactor = 1.0;

		///// <summary>
		///// Whether the rigid body is affected by the gravity.
		///// </summary>
		//[DefaultValue( true )]
		//[Category( "Rigid Body" )]
		//public Reference<bool> EnableGravity
		//{
		//	get { if( _enableGravity.BeginGet() ) EnableGravity = _enableGravity.Get( this ); return _enableGravity.value; }
		//	set
		//	{
		//		if( _enableGravity.BeginSet( this, ref value ) )
		//		{
		//			try
		//			{
		//				EnableGravityChanged?.Invoke( this );
		//				if( rigidBody != null )
		//				{
		//					impl
		//					//if( !EnableGravity )
		//					//	rigidBody.Flags |= RigidBodyFlags.DisableWorldGravity;
		//					//else
		//					//	rigidBody.Flags &= ~RigidBodyFlags.DisableWorldGravity;
		//				}
		//			}
		//			finally { _enableGravity.EndSet(); }
		//		}
		//	}
		//}
		///// <summary>Occurs when the <see cref="EnableGravity"/> property value changes.</summary>
		//public event Action<RigidBody> EnableGravityChanged;
		//ReferenceField<bool> _enableGravity = true;

		/// <summary>
		/// The linear reduction of velocity over time.
		/// </summary>
		[DefaultValue( 0.05 )]//1 )]
		[Range( 0, 1 )]
		[Category( "Rigid Body" )]
		[NetworkSynchronize( false )]
		public Reference<double> LinearDamping
		{
			get { if( _linearDamping.BeginGet() ) LinearDamping = _linearDamping.Get( this ); return _linearDamping.value; }
			set
			{
				if( _linearDamping.BeginSet( this, ref value ) )
				{
					try
					{
						LinearDampingChanged?.Invoke( this );
						if( physicalBody != null )
							physicalBody.LinearDamping = (float)value;
					}
					finally { _linearDamping.EndSet(); }
				}
			}
		}
		/// <summary>Occurs when the <see cref="LinearDamping"/> property value changes.</summary>
		public event Action<RigidBody> LinearDampingChanged;
		ReferenceField<double> _linearDamping = 0.05;//0.1;

		/// <summary>
		/// The angular reduction of velocity over time.
		/// </summary>
		[DefaultValue( 0.05 )] //0.1
		[Range( 0, 1 )]
		[Category( "Rigid Body" )]
		[NetworkSynchronize( false )]
		public Reference<double> AngularDamping
		{
			get { if( _angularDamping.BeginGet() ) AngularDamping = _angularDamping.Get( this ); return _angularDamping.value; }
			set
			{
				if( _angularDamping.BeginSet( this, ref value ) )
				{
					try
					{
						AngularDampingChanged?.Invoke( this );
						if( physicalBody != null )
							physicalBody.AngularDamping = (float)value;
					}
					finally { _angularDamping.EndSet(); }
				}
			}
		}
		/// <summary>Occurs when the <see cref="AngularDamping"/> property value changes.</summary>
		public event Action<RigidBody> AngularDampingChanged;
		ReferenceField<double> _angularDamping = 0.05;//0.1;

		/// <summary>
		/// The physical material used by the rigid body.
		/// </summary>
		[DefaultValue( null )]
		[Category( "Rigid Body" )]
		public Reference<PhysicalMaterial> Material
		{
			get { if( _material.BeginGet() ) Material = _material.Get( this ); return _material.value; }
			set
			{
				if( _material.BeginSet( this, ref value ) )
				{
					try
					{
						MaterialChanged?.Invoke( this );
						if( physicalBody != null )
							SetMaterial( physicalBody );
					}
					finally { _material.EndSet(); }
				}
			}
		}
		/// <summary>Occurs when the <see cref="Material"/> property value changes.</summary>
		public event Action<RigidBody> MaterialChanged;
		ReferenceField<PhysicalMaterial> _material;

		//!!!!
		///// <summary>
		///// The type of friction applied on the rigid body.
		///// </summary>
		//[DefaultValue( PhysicalMaterial.FrictionModeEnum.Simple )]
		//[Category( "Rigid Body" )]
		//public Reference<PhysicalMaterial.FrictionModeEnum> MaterialFrictionMode
		//{
		//	get { if( _materialFrictionMode.BeginGet() ) MaterialFrictionMode = _materialFrictionMode.Get( this ); return _materialFrictionMode.value; }
		//	set
		//	{
		//		if( _materialFrictionMode.BeginSet( this, ref value ) )
		//		{
		//			try
		//			{
		//				MaterialFrictionModeChanged?.Invoke( this );
		//				if( rigidBody != null )
		//					SetMaterial( rigidBody );
		//			}
		//			finally { _materialFrictionMode.EndSet(); }
		//		}
		//	}
		//}
		///// <summary>Occurs when the <see cref="MaterialFrictionMode"/> property value changes.</summary>
		//public event Action<RigidBody> MaterialFrictionModeChanged;
		//ReferenceField<PhysicalMaterial.FrictionModeEnum> _materialFrictionMode = PhysicalMaterial.FrictionModeEnum.Simple;

		/// <summary>
		/// The amount of friction applied on the rigid body.
		/// </summary>
		[DefaultValue( 0.5 )]
		[Range( 0, 1 )]
		[Category( "Rigid Body" )]
		public Reference<double> MaterialFriction
		{
			get { if( _materialFriction.BeginGet() ) MaterialFriction = _materialFriction.Get( this ); return _materialFriction.value; }
			set
			{
				if( _materialFriction.BeginSet( this, ref value ) )
				{
					try
					{
						MaterialFrictionChanged?.Invoke( this );
						if( physicalBody != null )
							SetMaterial( physicalBody );
					}
					finally { _materialFriction.EndSet(); }
				}
			}
		}
		/// <summary>Occurs when the <see cref="MaterialFriction"/> property value changes.</summary>
		public event Action<RigidBody> MaterialFrictionChanged;
		ReferenceField<double> _materialFriction = 0.5;

		//!!!!
		///// <summary>
		///// The amount of directional friction applied on the rigid body.
		///// </summary>
		//[DefaultValue( "1 1 1" )]
		////[ApplicableRange( 0, 1 )]
		//[Category( "Rigid Body" )]
		//public Reference<Vector3> MaterialAnisotropicFriction
		//{
		//	get { if( _materialAnisotropicFriction.BeginGet() ) MaterialAnisotropicFriction = _materialAnisotropicFriction.Get( this ); return _materialAnisotropicFriction.value; }
		//	set
		//	{
		//		if( _materialAnisotropicFriction.BeginSet( this, ref value ) )
		//		{
		//			try { MaterialAnisotropicFrictionChanged?.Invoke( this ); }
		//			finally { _materialAnisotropicFriction.EndSet(); }
		//		}
		//	}
		//}
		///// <summary>Occurs when the <see cref="AnisotropicFriction"/> property value changes.</summary>
		//public event Action<RigidBody> MaterialAnisotropicFrictionChanged;
		//ReferenceField<Vector3> _materialAnisotropicFriction = Vector3.One;

		//!!!!
		///// <summary>
		///// The amount of friction applied when rigid body is spinning.
		///// </summary>
		//[DefaultValue( 0.5 )]
		//[Range( 0, 1 )]
		//[Category( "Rigid Body" )]
		//public Reference<double> MaterialSpinningFriction
		//{
		//	get { if( _materialSpinningFriction.BeginGet() ) MaterialSpinningFriction = _materialSpinningFriction.Get( this ); return _materialSpinningFriction.value; }
		//	set
		//	{
		//		if( _materialSpinningFriction.BeginSet( this, ref value ) )
		//		{
		//			try
		//			{
		//				MaterialSpinningFrictionChanged?.Invoke( this );
		//				if( rigidBody != null )
		//					SetMaterial( rigidBody );
		//			}
		//			finally { _materialSpinningFriction.EndSet(); }
		//		}
		//	}
		//}
		///// <summary>Occurs when the <see cref="MaterialSpinningFriction"/> property value changes.</summary>
		//public event Action<RigidBody> MaterialSpinningFrictionChanged;
		//ReferenceField<double> _materialSpinningFriction = 0.5;

		//!!!!
		///// <summary>
		///// The amount of friction applied when rigid body is rolling.
		///// </summary>
		//[DefaultValue( 0.5 )]
		//[Range( 0, 1 )]
		//[Category( "Rigid Body" )]
		//public Reference<double> MaterialRollingFriction
		//{
		//	get { if( _materialRollingFriction.BeginGet() ) MaterialRollingFriction = _materialRollingFriction.Get( this ); return _materialRollingFriction.value; }
		//	set
		//	{
		//		if( _materialRollingFriction.BeginSet( this, ref value ) )
		//		{
		//			try
		//			{
		//				MaterialRollingFrictionChanged?.Invoke( this );
		//				if( rigidBody != null )
		//					SetMaterial( rigidBody );
		//			}
		//			finally { _materialRollingFriction.EndSet(); }
		//		}
		//	}
		//}
		///// <summary>Occurs when the <see cref="MaterialRollingFriction"/> property value changes.</summary>
		//public event Action<RigidBody> MaterialRollingFrictionChanged;
		//ReferenceField<double> _materialRollingFriction = 0.5;

		/// <summary>
		/// The ratio of the final relative velocity to initial relative velocity of the rigid body after collision.
		/// </summary>
		[DefaultValue( 0.0 )]
		[Range( 0, 1 )]
		[Category( "Rigid Body" )]
		public Reference<double> MaterialRestitution
		{
			get { if( _materialRestitution.BeginGet() ) MaterialRestitution = _materialRestitution.Get( this ); return _materialRestitution.value; }
			set
			{
				if( _materialRestitution.BeginSet( this, ref value ) )
				{
					try
					{
						MaterialRestitutionChanged?.Invoke( this );
						//!!!!
						if( physicalBody != null )//&& Material.Value == null )
						{
							SetMaterial( physicalBody );
							//rigidBody.Restitution = (float)MaterialRestitution;
						}

						//if( rigidBody != null && Material.Value == null )
						//{
						//rigidBody.Restitution = (float)MaterialRestitution;
						//}
					}
					finally { _materialRestitution.EndSet(); }
				}
			}
		}
		/// <summary>Occurs when the <see cref="MaterialRestitution"/> property value changes.</summary>
		public event Action<RigidBody> MaterialRestitutionChanged;
		ReferenceField<double> _materialRestitution;

		//!!!!impl
		///// <summary>
		///// The position of center of mass.
		///// </summary>
		//[DefaultValue( "0 0 0" )]
		//[Category( "Rigid Body" )]
		//public Reference<Vector3> CenterOfMassPosition
		//{
		//	get { if( _centerOfMassPosition.BeginGet() ) CenterOfMassPosition = _centerOfMassPosition.Get( this ); return _centerOfMassPosition.value; }
		//	set
		//	{
		//		if( _centerOfMassPosition.BeginSet( this, ref value ) )
		//		{
		//			try
		//			{
		//				CenterOfMassPositionChanged?.Invoke( this );

		//				//!!!!����������� �� �������������? ��� ��� �����
		//				if( rigidBody != null )
		//					RecreateBody();
		//			}
		//			finally { _centerOfMassPosition.EndSet(); }
		//		}
		//	}
		//}
		///// <summary>Occurs when the <see cref="CenterOfMassPosition"/> property value changes.</summary>
		//public event Action<RigidBody> CenterOfMassPositionChanged;
		//ReferenceField<Vector3> _centerOfMassPosition = Vector3.Zero;

		//!!!!impl
		///// <summary>
		///// The moment of inertia.
		///// </summary>
		//[DefaultValue( "1 1 1" )]
		//[Category( "Rigid Body" )]
		//public Reference<Vector3> InertiaTensorFactor
		//{
		//	get { if( _inertiaTensorFactor.BeginGet() ) InertiaTensorFactor = _inertiaTensorFactor.Get( this ); return _inertiaTensorFactor.value; }
		//	set
		//	{
		//		if( _inertiaTensorFactor.BeginSet( this, ref value ) )
		//		{
		//			try
		//			{
		//				InertiaTensorFactorChanged?.Invoke( this );

		//				//!!!!����������� �� �������������?

		//				//!!!!new, ����� ��� �� ��������
		//				//if( rigidBody != null )
		//				//{
		//				//	rigidBody.SetMassProps( Mass.Value, rigidBody.CollisionShape.CalculateLocalInertia( Mass.Value ) * BulletUtils.Convert( value ) );
		//				//	rigidBody.UpdateInertiaTensor();
		//				//}
		//				if( rigidBody != null )
		//					RecreateBody();
		//			}
		//			finally { _inertiaTensorFactor.EndSet(); }
		//		}
		//	}
		//}
		///// <summary>Occurs when the <see cref="InertiaTensorFactor"/> property value changes.</summary>
		//public event Action<RigidBody> InertiaTensorFactorChanged;
		//ReferenceField<Vector3> _inertiaTensorFactor = Vector3.One;

		//!!!!need?

		///// <summary>
		///// The linear velocity threshold below which the body will stop movement.
		///// </summary>
		//[DefaultValue( 0.5 )]//1.0 )]
		//[Category( "Rigid Body" )]
		//[Range( 0, 10, RangeAttribute.ConvenientDistributionEnum.Exponential )]
		//public Reference<double> LinearSleepingThreshold
		//{
		//	get { if( _linearSleepingThreshold.BeginGet() ) LinearSleepingThreshold = _linearSleepingThreshold.Get( this ); return _linearSleepingThreshold.value; }
		//	set
		//	{
		//		if( _linearSleepingThreshold.BeginSet( this, ref value ) )
		//		{
		//			try
		//			{
		//				LinearSleepingThresholdChanged?.Invoke( this );

		//				//!!!!impl

		//				//rigidBody?.SetSleepingThresholds( value, AngularSleepingThreshold.Value );
		//			}
		//			finally { _linearSleepingThreshold.EndSet(); }
		//		}
		//	}
		//}
		///// <summary>Occurs when the <see cref="LinearSleepingThreshold"/> property value changes.</summary>
		//public event Action<RigidBody> LinearSleepingThresholdChanged;
		//ReferenceField<double> _linearSleepingThreshold = 0.5;

		///// <summary>
		///// The angular velocity threshold below which the body will stop rotating.
		///// </summary>
		//[DefaultValue( 0.5 )]//0.8 )]
		//[Category( "Rigid Body" )]
		//[Range( 0, 10, RangeAttribute.ConvenientDistributionEnum.Exponential )]
		//public Reference<double> AngularSleepingThreshold
		//{
		//	get { if( _angularSleepingThreshold.BeginGet() ) AngularSleepingThreshold = _angularSleepingThreshold.Get( this ); return _angularSleepingThreshold.value; }
		//	set
		//	{
		//		if( _angularSleepingThreshold.BeginSet( this, ref value ) )
		//		{
		//			try
		//			{
		//				AngularSleepingThresholdChanged?.Invoke( this );

		//				//!!!!impl


		//				//rigidBody?.SetSleepingThresholds( LinearSleepingThreshold.Value, value );
		//			}
		//			finally { _angularSleepingThreshold.EndSet(); }
		//		}
		//	}
		//}
		///// <summary>Occurs when the <see cref="AngularSleepingThreshold"/> property value changes.</summary>
		//public event Action<RigidBody> AngularSleepingThresholdChanged;
		//ReferenceField<double> _angularSleepingThreshold = 0.5;


		//!!!!������� ��� � 2D?

		////!!!!name: ContactGroup, ContactMask
		///// <summary>
		///// The collision filtering group.
		///// </summary>
		//[DefaultValue( 1 )]
		//[Category( "Collision Filtering" )]
		//public Reference<int> CollisionGroup
		//{
		//	get { if( _collisionGroup.BeginGet() ) CollisionGroup = _collisionGroup.Get( this ); return _collisionGroup.value; }
		//	set
		//	{
		//		if( _collisionGroup.BeginSet( this, ref value ) )
		//		{
		//			try
		//			{
		//				CollisionGroupChanged?.Invoke( this );
		//				if( rigidBody != null )
		//					RecreateBody();
		//			}
		//			finally { _collisionGroup.EndSet(); }
		//		}
		//	}
		//}
		///// <summary>Occurs when the <see cref="CollisionGroup"/> property value changes.</summary>
		//public event Action<RigidBody> CollisionGroupChanged;
		//ReferenceField<int> _collisionGroup = 1;

		///// <summary>
		///// The collision filtering mask.
		///// </summary>
		//[DefaultValue( 1 )]
		//[Category( "Collision Filtering" )]
		//public Reference<int> CollisionMask
		//{
		//	get { if( _collisionMask.BeginGet() ) CollisionMask = _collisionMask.Get( this ); return _collisionMask.value; }
		//	set
		//	{
		//		if( _collisionMask.BeginSet( this, ref value ) )
		//		{
		//			try
		//			{
		//				CollisionMaskChanged?.Invoke( this );
		//				if( rigidBody != null )
		//					RecreateBody();
		//			}
		//			finally { _collisionMask.EndSet(); }
		//		}
		//	}
		//}
		///// <summary>Occurs when the <see cref="CollisionMask"/> property value changes.</summary>
		//public event Action<RigidBody> CollisionMaskChanged;
		//ReferenceField<int> _collisionMask = 1;

		/// <summary>
		/// The initial linear velocity of the body.
		/// </summary>
		[DefaultValue( "0 0 0" )]
		[Category( "Velocity" )]
		[NetworkSynchronize( false )]//don't sync to optimize networking
		public Reference<Vector3> LinearVelocity
		{
			get { if( _linearVelocity.BeginGet() ) LinearVelocity = _linearVelocity.Get( this ); return _linearVelocity.value; }
			set
			{
				if( _linearVelocity.BeginSet( this, ref value ) )
				{
					try
					{
						LinearVelocityChanged?.Invoke( this );
						if( physicalBody != null && !updatePropertiesWithoutUpdatingBody )
							physicalBody.LinearVelocity = value.Value.ToVector3F();
					}
					finally { _linearVelocity.EndSet(); }
				}
			}
		}
		/// <summary>Occurs when the <see cref="LinearVelocity"/> property value changes.</summary>
		public event Action<RigidBody> LinearVelocityChanged;
		ReferenceField<Vector3> _linearVelocity = Vector3.Zero;

		/// <summary>
		/// The initial angular velocity of the body.
		/// </summary>
		[DefaultValue( "0 0 0" )]
		[Category( "Velocity" )]
		[NetworkSynchronize( false )]//don't sync to optimize networking
		public Reference<Vector3> AngularVelocity
		{
			get { if( _angularVelocity.BeginGet() ) AngularVelocity = _angularVelocity.Get( this ); return _angularVelocity.value; }
			set
			{
				if( _angularVelocity.BeginSet( this, ref value ) )
				{
					try
					{
						AngularVelocityChanged?.Invoke( this );
						if( physicalBody != null && !updatePropertiesWithoutUpdatingBody )
							physicalBody.AngularVelocity = value.Value.ToVector3F();
					}
					finally { _angularVelocity.EndSet(); }
				}
			}
		}
		/// <summary>Occurs when the <see cref="AngularVelocity"/> property value changes.</summary>
		public event Action<RigidBody> AngularVelocityChanged;
		ReferenceField<Vector3> _angularVelocity = Vector3.Zero;

		/// <summary>
		/// Whether to display collected collision contacts data.
		/// </summary>
		[DefaultValue( false )]
		[Category( "Contacts" )]
		public Reference<bool> DisplayContacts
		{
			get { if( _displayContacts.BeginGet() ) DisplayContacts = _displayContacts.Get( this ); return _displayContacts.value; }
			set { if( _displayContacts.BeginSet( this, ref value ) ) { try { DisplayContactsChanged?.Invoke( this ); } finally { _displayContacts.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="DisplayContacts"/> property value changes.</summary>
		public event Action<RigidBody> DisplayContactsChanged;
		ReferenceField<bool> _displayContacts = false;

		/// <summary>
		/// The sound when the body collided with another body.
		/// </summary>
		[DefaultValue( null )]
		[Category( "Collision" )]
		public Reference<Sound> CollisionSound
		{
			get { if( _collisionSound.BeginGet() ) CollisionSound = _collisionSound.Get( this ); return _collisionSound.value; }
			set { if( _collisionSound.BeginSet( this, ref value ) ) { try { CollisionSoundChanged?.Invoke( this ); collisionSoundSpecified = CollisionSound.ReferenceOrValueSpecified; } finally { _collisionSound.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="CollisionSound"/> property value changes.</summary>
		public event Action<RigidBody> CollisionSoundChanged;
		ReferenceField<Sound> _collisionSound = null;

		[DefaultValue( 1.0 )]
		[Category( "Collision" )]
		public Reference<double> CollisionSoundMinSpeedChange
		{
			get { if( _collisionSoundMinSpeedChange.BeginGet() ) CollisionSoundMinSpeedChange = _collisionSoundMinSpeedChange.Get( this ); return _collisionSoundMinSpeedChange.value; }
			set { if( _collisionSoundMinSpeedChange.BeginSet( this, ref value ) ) { try { CollisionSoundMinSpeedChangeChanged?.Invoke( this ); } finally { _collisionSoundMinSpeedChange.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="CollisionSoundMinSpeedChange"/> property value changes.</summary>
		public event Action<RigidBody> CollisionSoundMinSpeedChangeChanged;
		ReferenceField<double> _collisionSoundMinSpeedChange = 1.0;

		[DefaultValue( 0.25 )]
		[Category( "Collision" )]
		public Reference<double> CollisionSoundTimeInterval
		{
			get { if( _collisionSoundTimeInterval.BeginGet() ) CollisionSoundTimeInterval = _collisionSoundTimeInterval.Get( this ); return _collisionSoundTimeInterval.value; }
			set { if( _collisionSoundTimeInterval.BeginSet( this, ref value ) ) { try { CollisionSoundTimeIntervalChanged?.Invoke( this ); } finally { _collisionSoundTimeInterval.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="CollisionSoundTimeInterval"/> property value changes.</summary>
		public event Action<RigidBody> CollisionSoundTimeIntervalChanged;
		ReferenceField<double> _collisionSoundTimeInterval = 0.25;

		/// <summary>
		/// Saved collision definition calculation settings to use when recalculate.
		/// </summary>
		[DefaultValue( "" )]
		[Category( "Special" )]
		public Reference<string> CollisionDefinitionSettings
		{
			get { if( _collisionDefinitionSettings.BeginGet() ) CollisionDefinitionSettings = _collisionDefinitionSettings.Get( this ); return _collisionDefinitionSettings.value; }
			set { if( _collisionDefinitionSettings.BeginSet( this, ref value ) ) { try { CollisionDefinitionSettingsChanged?.Invoke( this ); } finally { _collisionDefinitionSettings.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="CollisionDefinitionSettings"/> property value changes.</summary>
		public event Action<RigidBody> CollisionDefinitionSettingsChanged;
		ReferenceField<string> _collisionDefinitionSettings = "";

		[Browsable( false )]
		[Cloneable( CloneType.Deep )]
		public bool CharacterMode { get; set; }


		//!!!!��� ����� ��������? ���� �������� ������ ����� ����

		//!!!!��� ����� �������� �����������. ������������ ������� ��� ��������� ��������. ����� ��� ���������?


		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		protected override void OnMetadataGetMembersFilter( Metadata.GetMembersContext context, Metadata.Member member, ref bool skip )
		{
			base.OnMetadataGetMembersFilter( context, member, ref skip );

			var p = member as Metadata.Property;
			if( p != null )
			{
				switch( p.Name )
				{
				case nameof( MotionQuality ):
				case nameof( Mass ):
				case nameof( CenterOfMassOffset ):
					//case nameof( CenterOfMassManual ):
					//!!!!impl
					//case nameof( InertiaTensorFactor ):
					//case nameof( LinearSleepingThreshold ):
					//case nameof( AngularSleepingThreshold ):
					//!!!!
					if( MotionType.Value != PhysicsMotionType.Dynamic )
						skip = true;
					break;

				//case nameof( CenterOfMassPosition ):
				//	if( MotionType.Value != PhysicsMotionType.Dynamic || !CenterOfMassManual )
				//		skip = true;
				//	break;

				case nameof( GravityFactor ):
				case nameof( LinearDamping ):
				case nameof( AngularDamping ):
					if( MotionType.Value != PhysicsMotionType.Dynamic )
						skip = true;
					break;

				case nameof( LinearVelocity ):
				case nameof( AngularVelocity ):
					if( MotionType.Value == PhysicsMotionType.Static )
						skip = true;
					break;

				//!!!!impl
				//case nameof( CenterOfMassPosition ):
				//	if( MotionType.Value != MotionTypeEnum.Dynamic || !CenterOfMassManual.Value )
				//		skip = true;
				//	break;

				//!!!!
				//case nameof( MaterialFrictionMode ):
				//	if( Material.Value != null )
				//		skip = true;
				//	break;

				case nameof( MaterialFriction ):
					if( Material.Value != null )
						skip = true;
					break;

				//!!!!
				//case nameof( MaterialRollingFriction ):
				//case nameof( MaterialSpinningFriction ):
				//case nameof( MaterialAnisotropicFriction ):
				//	if( MaterialFrictionMode.Value == PhysicalMaterial.FrictionModeEnum.Simple || Material.Value != null )
				//		skip = true;
				//	break;

				case nameof( MaterialRestitution ):
					if( Material.Value != null )
						skip = true;
					break;

				//case nameof( DisplayContacts ):
				//	if( !ContactsCollect )
				//		skip = true;
				//	break;

				case nameof( CollisionSoundTimeInterval ):
				case nameof( CollisionSoundMinSpeedChange ):
					if( !CollisionSound.ReferenceOrValueSpecified )
						skip = true;
					break;
				}
			}
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		[Browsable( false )]
		public Scene.PhysicsWorldClass.Body PhysicalBody
		{
			get { return physicalBody; }
		}

		protected override void OnTransformChanged()
		{
			if( physicalBody != null && !updatePropertiesWithoutUpdatingBody )
			{
				var bodyTransform = Transform.Value;

				//bool alwaysRecreate = false;
				//bool alwaysRecreate = MotionType.Value == MotionTypeEnum.Dynamic;

				if( physicalBodyCreatedTransformScale != bodyTransform.Scale )//|| alwaysRecreate )
				{
					RecreateBody();
				}
				else
				{
					//update transform


					//var t = new Matrix4( bodyTransform.Rotation.ToMatrix3(), bodyTransform.Position );

					//!!!!
					//if( MotionType.Value == MotionTypeEnum.Dynamic )
					//	t *= centerOfMassOffset;


					//!!!!
					var activate = true;

					var pos = bodyTransform.Position;
					var rot = bodyTransform.Rotation.ToQuaternionF();
					physicalBody?.SetTransform( ref pos, ref rot, activate );


					//!!!!impl

					////update constraints
					//if( rigidBody.NumConstraintRefs != 0 )
					//{
					//	foreach( var c in GetLinkedCreatedConstraints() )
					//		c.RecreateConstraint();
					//}




					//var t = new Matrix4( bodyTransform.Rotation.ToMatrix3(), bodyTransform.Position );
					//if( MotionType.Value == MotionTypeEnum.Dynamic )
					//	t *= centerOfMassOffset;

					//BulletPhysicsUtility.Convert( ref t, out var tb );

					//rigidBody.WorldTransform = tb;
					//rigidBody.InterpolationWorldTransform = tb;
					//if( rigidBody.MotionState != null )
					//	rigidBody.MotionState.WorldTransform = tb;

					////GetPhysicsWorldData().world.SynchronizeSingleMotionState( rigidBody );

					////update AABB
					//GetPhysicsWorldData().world.UpdateSingleAabb( rigidBody );

					////GetPhysicsWorldData().world.ComputeOverlappingPairs();

					////update constraints
					//if( rigidBody.NumConstraintRefs != 0 )
					//{
					//	foreach( var c in GetLinkedCreatedConstraints() )
					//		c.RecreateConstraint();
					//}
				}
			}

			base.OnTransformChanged();
		}

		protected override void OnSpaceBoundsUpdate( ref SpaceBounds newBounds )
		{
			base.OnSpaceBoundsUpdate( ref newBounds );

			if( physicalBody != null )
			{
				physicalBody.GetBounds( out var bounds );
				var b = new SpaceBounds( bounds );
				newBounds = SpaceBounds.Merge( newBounds, b );

				//bounds prediction to skip small updates in future steps
				if( physicalBody.Active )
				{
					ref var realBounds = ref newBounds.boundingBox;

					if( !SpaceBoundsOctreeOverride.HasValue || !SpaceBoundsOctreeOverride.Value.Contains( ref realBounds ) )
					{
						//calculated extended bounds. predict for 2-3 seconds

						var trPosition = TransformV.Position;

						var bTotal = realBounds;
						var b2 = new Bounds( trPosition );
						b2.Add( trPosition + PhysicalBody.LinearVelocity * ( 2.0f + staticRandom.NextFloat() ) );
						b2.Expand( newBounds.boundingSphere.Radius * 1.1 );
						bTotal.Add( ref b2 );

						SpaceBoundsOctreeOverride = bTotal;
					}
				}
				else
					SpaceBoundsOctreeOverride = null;
			}
		}

		Scene.PhysicsWorldClass GetPhysicsWorldData()
		{
			//!!!!slowly?
			var scene = ParentScene;
			if( scene != null )
				return scene.PhysicsWorld;
			return null;
		}

		[Browsable( false )]
		bool CanCreate
		{
			get
			{
				if( Name == "Collision Definition" && Parent as Mesh != null )
					return false;
				return true;
			}
		}

		////!!!!slowly?
		////!!!!need recursive many levels?
		//static void GetComponentShapesRecursive( CollisionShape shape, Matrix4 shapeTransform, List<(CollisionShape, Matrix4)> result )
		//{
		//	result.Add( (shape, shapeTransform) );

		//	foreach( var child in shape.GetComponents<CollisionShape>( false, false, true ) )
		//	{
		//		var childTransform = shapeTransform * child.TransformRelativeToParent.Value.ToMatrix4();
		//		//var childTransform = child.LocalTransform.Value.ToMat4() * shapeTransform;
		//		GetComponentShapesRecursive( child, childTransform, result );
		//	}
		//}

		void CreateBody()
		{
			if( !CanCreate )
				return;

			duringCreateDestroy = true;

			var physicsWorldData = GetPhysicsWorldData();
			if( physicsWorldData != null )
			{
				if( physicalBody != null )
					Log.Fatal( "RigidBody: CreateBody: physicalBody != null." );
				if( !EnabledInHierarchy )
					Log.Fatal( "RigidBody: CreateBody: !EnabledInHierarchy." );

				//!!!!��������� ��������� ���-�� �� ������ ���. ��� �������������

				var bodyTransform = Transform.Value;
				//var bodyTransformScaleMatrix = Matrix3.FromScale( bodyTransform.Scale ).ToMatrix4();

				////get shapes. calculate local transforms with applied body scaling.
				//var componentShapes = new List<(CollisionShape, Matrix4)>();
				//foreach( var child in GetComponents<CollisionShape>( false, false, true ) )
				//	GetComponentShapesRecursive( child, bodyTransformScaleMatrix * child.TransformRelativeToParent.Value.ToMatrix4(), componentShapes );

				//if( componentShapes.Count > 0 )
				{

					//!!!!impl
					//bool needCompound = true;

					////!!!!new
					//////!!!!always compound. need compound for shape local transform and when center of mass is not 0,0,0.
					//bool needCompound = false;
					//if( componentShapes.Count > 1 || MotionType.Value == MotionTypeEnum.Dynamic )
					//	needCompound = true;
					//else
					//{
					//	var tuple = componentShapes[ 0 ];
					//	var componentShape = tuple.Item1;
					//	var transformShape = tuple.Item2;

					//	if( !transformShape.Decompose( out Vector3 pos, out Matrix3 rot, out Vector3 scl ) )
					//	{
					//		//!!!!
					//	}

					//	const double zeroTolerance = 1e-6f;
					//	if( !rot.Equals( Matrix3.Identity, zeroTolerance ) || !pos.Equals( Vector3.Zero, zeroTolerance ) )
					//		needCompound = true;
					//}


					////start transform
					////body transform without scaling. Scaling is already applied to shapes.
					//var startTransform = new Matrix4( bodyTransform.Rotation.ToMatrix3(), bodyTransform.Position );

					//!!!!
					////center of mass offset
					//centerOfMassOffset = Matrix4.Identity;
					//if( MotionType.Value == MotionTypeEnum.Dynamic )
					//{
					//	if( CenterOfMassManual )
					//		centerOfMassOffset = Matrix4.FromTranslate( CenterOfMassPosition.Value );
					//	else
					//	{
					//		Vector3 totalWeighted = Vector3.Zero;
					//		double totalVolume = 0;

					//		foreach( var tuple in componentShapes )
					//		{
					//			var componentShape = tuple.Item1;
					//			var transformShape = tuple.Item2;

					//			if( !transformShape.Decompose( out Vector3 pos, out Matrix3 rot, out Vector3 scl ) )
					//			{
					//				//!!!!
					//			}

					//			var shapeCenterOfMassPosition = new Matrix4( rot, pos ) * componentShape.GetCenterOfMassPositionNotScaledByParent();
					//			var shapeVolume = componentShape.VolumeNotScaledByParent * scl.X * scl.Y * scl.Z;

					//			totalWeighted += shapeCenterOfMassPosition * shapeVolume;
					//			totalVolume += shapeVolume;
					//		}

					//		var centerOfMassPosition = totalWeighted / totalVolume;
					//		centerOfMassOffset = Matrix4.FromTranslate( centerOfMassPosition );

					//		//centerOfMassOffset = Mat4.FromTranslate( BulletUtils.Convert( BulletUtils.GetCenterOfMass( shape ) ) );
					//	}
					//	centerOfMassOffsetInverted = centerOfMassOffset.GetInverse();
					//}

					//Internal.BulletSharp.CollisionShape shape;

					var nativeShape = physicsWorldData.AllocateShape( this, bodyTransform.Scale );

					//if( needCompound )
					//{
					//use compound shape

					//nativeShape = physicsWorldData.AllocateShape( this, bodyTransform.Scale );
					//nativeShapeItem = physicsWorldData.AllocateNativeShape( componentShapes );


					//!!!!
					//if( MotionType.Value == MotionTypeEnum.Dynamic )
					//	tr *= centerOfMassOffsetInverted;



					//var compoundShape = new CompoundShape();
					//foreach( var tuple in componentShapes )
					//{
					//	var componentShape = tuple.Item1;
					//	var transformShape = tuple.Item2;

					//	if( !transformShape.Decompose( out Vector3 pos, out Matrix3 rot, out Vector3 scl ) )
					//	{
					//		//!!!!
					//	}

					//	(var shape, var meshShapeCacheItem) = componentShape.CreateShape( physicsWorldData, ref scl );
					//	if( shape != null )
					//	{
					//		//!!!!what to do with scaling and big mesh cache
					//		shape.LocalScaling = BulletPhysicsUtility.Convert( scl );
					//		//shape.UserObject = componentShape;

					//		if( meshShapeCacheItem != null )
					//		{
					//			if( meshShapeCacheItemsToFree == null )
					//				meshShapeCacheItemsToFree = new List<Scene.PhysicsWorldDataClass.MeshShapeCacheItem>();
					//			meshShapeCacheItemsToFree.Add( meshShapeCacheItem );
					//		}
					//		else
					//		{
					//			if( collisionShapesToDispose == null )
					//				collisionShapesToDispose = new List<Internal.BulletSharp.CollisionShape>();
					//			collisionShapesToDispose.Add( shape );
					//		}

					//		//!!!!
					//		//var tr = new Matrix4( rot * Matrix3.FromScale( scl ), pos );
					//		var tr = new Matrix4( rot, pos );
					//		if( MotionType.Value == MotionTypeEnum.Dynamic )
					//			tr *= centerOfMassOffsetInverted;

					//		compoundShape.AddChildShape( BulletPhysicsUtility.Convert( tr ), shape );
					//	}
					//}

					////no children
					//if( compoundShape.NumChildShapes == 0 )
					//{
					//	compoundShape.Dispose();
					//	compoundShape = null;
					//}

					//if( compoundShape != null )
					//{
					//	if( collisionShapesToDispose == null )
					//		collisionShapesToDispose = new List<Internal.BulletSharp.CollisionShape>();
					//	collisionShapesToDispose.Add( compoundShape );

					//	collisionShape = compoundShape;
					//}
					//}
					//else
					//{
					//	//no CompoundShape


					//	//!!!!impl
					//	//If there's only 1 part, we can use a RotatedTranslatedShape instead


					//	//var tuple = componentShapes[ 0 ];
					//	//var componentShape = tuple.Item1;
					//	//var transformShape = tuple.Item2;

					//	//transformShape.Decompose( out var pos, out Matrix3 rot, out var scl );

					//	//(var shape, var meshShapeCacheItem) = componentShape.CreateShape( physicsWorldData, ref scl );
					//	//if( shape != null )
					//	//{
					//	//	//!!!!
					//	//	shape.LocalScaling = BulletPhysicsUtility.Convert( scl );
					//	//	//shape.UserObject = componentShape;

					//	//	if( meshShapeCacheItem != null )
					//	//	{
					//	//		if( meshShapeCacheItemsToFree == null )
					//	//			meshShapeCacheItemsToFree = new List<Scene.PhysicsWorldDataClass.MeshShapeCacheItem>();
					//	//		meshShapeCacheItemsToFree.Add( meshShapeCacheItem );
					//	//	}
					//	//	else
					//	//	{
					//	//		if( collisionShapesToDispose == null )
					//	//			collisionShapesToDispose = new List<Internal.BulletSharp.CollisionShape>();
					//	//		collisionShapesToDispose.Add( shape );
					//	//	}

					//	//	collisionShape = shape;
					//	//}
					//}

					if( nativeShape != null )
					{
						var motionType = MotionType.Value;

						var activate = false;
						if( motionType == PhysicsMotionType.Dynamic )
						{
							//!!!!maybe property bool ActivateOnStart
							activate = true;
						}

						//!!!!
						if( motionType == PhysicsMotionType.Kinematic && ( LinearVelocity.Value != Vector3.Zero || AngularVelocity.Value != Vector3.Zero ) )
						{
							activate = true;
						}

						var pos = bodyTransform.Position;
						var rot = bodyTransform.Rotation.ToQuaternionF();

						var centerOfMassOffset = CenterOfMassOffset.Value.ToVector3F();
						//var centerOfMassManual = CenterOfMassManual.Value;
						//var centerOfMassPosition = CenterOfMassPosition.Value.ToVector3F();
						//impl?
						//var inertiaTensorFactor = Vector3F.One;//InertiaTensorFactor.Value.ToVector3F();

						var body = physicsWorldData.CreateRigidBody( nativeShape, true, this, motionType, (float)LinearDamping.Value, (float)AngularDamping.Value, ref pos, ref rot, activate, (float)Mass, ref centerOfMassOffset, /*centerOfMassManual, ref centerOfMassPosition, ref inertiaTensorFactor, */MotionQuality.Value, CharacterMode );

						if( body != null )
						{
							if( motionType != PhysicsMotionType.Static )
							{
								body.LinearVelocity = LinearVelocity.Value.ToVector3F();
								body.AngularVelocity = AngularVelocity.Value.ToVector3F();
								body.GravityFactor = (float)GravityFactor;
							}

							if( motionType == PhysicsMotionType.Dynamic )
							{

								//!!!!

								//double mass = Mass.Value;
								//var localInertia = collisionShape.CalculateLocalInertia( mass ) * BulletPhysicsUtility.Convert( InertiaTensorFactor.Value );
								//var motionState = new DefaultMotionState( BulletPhysicsUtility.Convert( startTransform * centerOfMassOffset ) );
								//using( var rbInfo = new RigidBodyConstructionInfo( mass, motionState, collisionShape, localInertia ) )
								//	body = new Internal.BulletSharp.RigidBody( rbInfo );


								//!!!!
								//body.SetSleepingThresholds( LinearSleepingThreshold, AngularSleepingThreshold );

							}

							SetMaterial( body );

							physicalBody = body;
							physicalBodyCreatedTransformScale = bodyTransform.Scale;

							//!!!!collision group

							//collisionShapeByIndex = new CollisionShape[ componentShapes.Count ];
							//for( int n = 0; n < componentShapes.Count; n++ )
							//	collisionShapeByIndex[ n ] = componentShapes[ n ].Item1;
						}
					}
					else
						DestroyBody();
				}

				//!!!!
				SpaceBoundsUpdate();


				if( CollisionSound.ReferenceOrValueSpecified )
				{
					//starts getting contacts
					physicalBody.ContactsExist();//GetContacts();
				}
			}

			duringCreateDestroy = false;
		}

		void DestroyBody()
		{
			duringCreateDestroy = true;

			//!!!!��� ��� �������?
			//!!!!��������� �� ��� ��?

			if( physicalBody != null )
			{
				//!!!!
				//centerOfMassOffset = Matrix4.Identity;
				//centerOfMassOffsetInverted = Matrix4.Identity;

				physicalBodyCreatedTransformScale = Vector3.Zero;
				physicalBody.Dispose();
				physicalBody = null;
			}

			//!!!!
			////free from mesh shape cache
			//if( meshShapeCacheItemsToFree != null )
			//{
			//	foreach( var item in meshShapeCacheItemsToFree )
			//		physicsWorldData.FreeShapeInCache( item );
			//	meshShapeCacheItemsToFree = null;
			//}


			//!!!!
			//collisionShapeByIndex = null;

			duringCreateDestroy = false;
		}

		public void RecreateBody()
		{
			if( EnabledInHierarchy && !duringCreateDestroy )
			{
				DestroyBody();
				CreateBody();
			}
		}

		protected override void OnEnabledInHierarchyChanged()
		{
			base.OnEnabledInHierarchyChanged();

			if( EnabledInHierarchy )
			{
				//����� �������� ��������� ����� ����, �.�. ����������� RecreateBody()
				if( physicalBody == null )
					CreateBody();
			}
			else
				DestroyBody();
		}

		//!!!!
		//static AnisotropicFrictionFlags ConvertFrictionMode( PhysicalMaterial.FrictionModeEnum value )
		//{
		//	switch( value )
		//	{
		//	case PhysicalMaterial.FrictionModeEnum.Simple:
		//		return AnisotropicFrictionFlags.FrictionDisabled;
		//	case PhysicalMaterial.FrictionModeEnum.Anisotropic:
		//		return AnisotropicFrictionFlags.Friction;
		//	case PhysicalMaterial.FrictionModeEnum.AnisotropicRolling:
		//		return AnisotropicFrictionFlags.RollingFriction;
		//	}
		//	return 0;
		//}

		void SetMaterial( Scene.PhysicsWorldClass.Body b )
		{

			//!!!!����� �� ����� �� ���������


			//material settings
			PhysicalMaterial mat = Material.Value;
			if( mat != null )
			{

				//!!!!impl


				//var mode = mat.FrictionMode.Value;

				//if( mode == PhysicalMaterial.FrictionModeEnum.Simple )
				//{
				//	b.SetAnisotropicFriction( Internal.BulletSharp.Math.BVector3.One, AnisotropicFrictionFlags.FrictionDisabled );
				//	b.RollingFriction = 0;// default value
				//	b.SpinningFriction = 0;// default value
				//}
				//else
				//{
				//	b.RollingFriction = mat.RollingFriction;
				//	b.SpinningFriction = mat.SpinningFriction;
				//	//dir = b.CollisionShape.AnisotropicRollingFrictionDirection;

				//	b.SetAnisotropicFriction( BulletPhysicsUtility.Convert( mat.AnisotropicFriction.Value ), ConvertFrictionMode( mode ) );
				//}

				b.Friction = (float)mat.Friction;
				b.Restitution = (float)mat.RigidRestitution;
				//PhysicsNative.JBody_SetFriction( b, (float)mat.Friction.Value );
				//PhysicsNative.JBody_SetRestitution( b, (float)mat.RigidRestitution.Value );
			}
			else
			{

				//!!!!impl


				//var mode = MaterialFrictionMode.Value;

				//if( mode == PhysicalMaterial.FrictionModeEnum.Simple )
				//{
				//	b.SetAnisotropicFriction( Internal.BulletSharp.Math.BVector3.One, AnisotropicFrictionFlags.FrictionDisabled );
				//	b.RollingFriction = 0;// default value
				//	b.SpinningFriction = 0;// default value
				//}
				//else
				//{
				//	b.RollingFriction = MaterialRollingFriction;
				//	b.SpinningFriction = MaterialSpinningFriction;
				//	//dir = b.CollisionShape.AnisotropicRollingFrictionDirection;

				//	b.SetAnisotropicFriction( BulletPhysicsUtility.Convert( MaterialAnisotropicFriction.Value ), ConvertFrictionMode( mode ) );
				//}

				b.Friction = (float)MaterialFriction;
				b.Restitution = (float)MaterialRestitution;
				//PhysicsNative.JBody_SetFriction( b, (float)MaterialFriction.Value );
				//PhysicsNative.JBody_SetRestitution( b, (float)MaterialRestitution.Value );
			}
		}

		//public override void UpdateDataFromPhysicsEngine()
		[MethodImpl( (MethodImplOptions)512 )]
		internal void UpdateDataFromPhysicalBody()
		{
			//if( ContactsData != null )
			//	ContactsData.Clear();

			if( physicalBody != null && ( MotionType.Value == PhysicsMotionType.Dynamic || MotionType.Value == PhysicsMotionType.Kinematic && !Transform.ReferenceSpecified ) )
			{
				var pos = physicalBody.Position;
				var rot = physicalBody.Rotation;
				var linearVelocity = physicalBody.LinearVelocity;
				var angularVelocity = physicalBody.AngularVelocity;

				//physicalBody.GetData( out var pos, out var rot, out var linearVelocity, out var angularVelocity, out var newActive );

				//!!!!
				//tr *= centerOfMassOffsetInverted;


				var oldT = Transform.Value;
				//!!!!scale?
				var newT = new Transform( pos, rot, oldT.Scale );


				//rigidBody.GetWorldTransform( out var bulletT );

				//Matrix4 tr;
				//BulletPhysicsUtility.Convert( ref bulletT, out tr );
				//tr *= centerOfMassOffsetInverted;

				//tr.Decompose( out Vector3 pos, out Quaternion rot, out Vector3 scl );
				//var oldT = Transform.Value;
				////!!!!scale?
				//var newT = new Transform( pos, rot, oldT.Scale );


				//bulletT = BulletUtils.Convert( centerOfMassOffsetInverted ) * bulletT;
				//bulletT.Decompose( out Vector3 scale, out Quaternion rotation, out Vector3 translation );
				//var oldT = Transform.Value;
				////!!!!scale?
				//var newT = new Transform( BulletUtils.Convert( translation ), BulletUtils.Convert( rotation ), oldT.Scale );

				try
				{
					updatePropertiesWithoutUpdatingBody = true;
					Transform = newT;
					LinearVelocity = linearVelocity.ToVector3();
					AngularVelocity = angularVelocity.ToVector3();
				}
				finally
				{
					updatePropertiesWithoutUpdatingBody = false;
				}

				//process collision sound
				if( physicalBody != null && collisionSoundSpecified )
				{
					if( collisionSoundRemainingTime > 0 )
					{
						collisionSoundRemainingTime -= Time.SimulationDelta;
						if( collisionSoundRemainingTime < 0 )
							collisionSoundRemainingTime = 0;
					}

					if( collisionSoundRemainingTime == 0 && physicalBody.Active && physicalBody.ContactsExist() )
					{
						var changeSquared = ( physicalBody.LinearVelocity - physicalBody.PreviousLinearVelocity ).LengthSquared();
						var minChangeSquared = MathEx.Square( CollisionSoundMinSpeedChange.Value );
						if( changeSquared > minChangeSquared )
							CollisionSoundPlay();
					}
				}
			}
		}

		protected override bool OnEnabledSelectionByCursor()
		{
			var scene = ParentScene;
			if( !scene.GetDisplayDevelopmentDataInThisApplication() )
				return false;
			if( physicalBody != null )
			{
				if( !scene.DisplayPhysicalObjects )
					return false;
			}
			else
			{
				if( !scene.DisplayLabels )
					return false;
			}
			return base.OnEnabledSelectionByCursor();
		}

		protected override void OnCheckSelectionByRay( CheckSelectionByRayContext context )
		{
			base.OnCheckSelectionByRay( context );

			if( physicalBody != null )
			{
				context.thisObjectWasChecked = true;

				var item = new PhysicsRayTestItem( context.ray, PhysicsRayTestItem.ModeEnum.OneClosest, PhysicsRayTestItem.FlagsEnum.None );
				physicalBody.Shape.RayTest( item, physicalBody.Position, physicalBody.Rotation );
				if( item.Result.Length != 0 )
					context.thisObjectResultRayScale = item.Result[ 0 ].DistanceScale;

				//var item = new PhysicsRayTestItem( context.ray, PhysicsRayTestItem.ModeEnum.OneClosestForEach, PhysicsRayTestItem.FlagsEnum.None );
				//ParentScene.PhysicsRayTest( item );
				//foreach( var resultItem in item.Result )
				//{
				//	if( resultItem.Body == physicalBody )
				//	{
				//		context.thisObjectResultRayScale = resultItem.DistanceScale;
				//		break;
				//	}
				//}

				////var item = new PhysicsRayTestItem( context.ray, PhysicsRayTestItem.ModeEnum.OneClosest, PhysicsRayTestItem.FlagsEnum.None, rigidBody );
				//////var item = new PhysicsRayTestItem( context.ray, CollisionGroup.Value, -1, PhysicsRayTestItem.ModeEnum.OneClosest, rigidBody );
				////ParentScene.PhysicsRayTest( item );
				////if( item.Result.Length != 0 )
				////	context.thisObjectResultRayScale = item.Result[ 0 ].DistanceScale;
			}
		}

		//CenterOfMassGeometry GetCenterOfMassGeometry( float radius )
		//{
		//	for( int n = 0; n < centerOfMassGeometryCache.Count; n++ )
		//	{
		//		var item2 = centerOfMassGeometryCache[ n ];
		//		if( Math.Abs( item2.radius - radius ) < .01 )
		//			return item2;
		//	}

		//	while( centerOfMassGeometryCache.Count > 15 )
		//		centerOfMassGeometryCache.RemoveAt( 0 );

		//	var item = new CenterOfMassGeometry();
		//	item.radius = radius;
		//	var segments = 10;
		//	SimpleMeshGenerator.GenerateSphere( radius, segments, ( ( segments + 1 ) / 2 ) * 2, false, out item.positions, out item.indices );
		//	centerOfMassGeometryCache.Add( item );

		//	return item;
		//}

		public static void Render( ViewportRenderingContext context, bool renderActive, bool renderSelected, bool renderCanSelect, Transform bodyTransform, ref int verticesRendered, RigidBody body, Scene.PhysicsWorldClass.Body physicalBody )
		{
			var context2 = context.ObjectInSpaceRenderingContext;

			var viewport = context.Owner;
			var renderer = viewport.Simple3DRenderer;

			//var scene = ParentScene;

			//bool show = ( scene.GetDisplayDevelopmentDataInThisApplication() && scene.DisplayPhysicalObjects ) ||
			//	context2.selectedObjects.Contains( this ) || context2.canSelectObjects.Contains( this ) || context2.dragDropCreateObject == this;
			if( /*show && */ /*rigidBody != null && */renderer != null )
			{
				//!!!!
				//if( context2.displayPhysicalObjectsCounter < context2.displayPhysicalObjectsMax )
				//{
				//	context2.displayPhysicalObjectsCounter++;

				//draw body
				{
					ColorValue color;
					if( body.MotionType.Value == PhysicsMotionType.Static )
						color = ProjectSettings.Get.Colors.SceneShowPhysicsStaticColor;
					else if( renderActive )// Active )
						color = ProjectSettings.Get.Colors.SceneShowPhysicsDynamicActiveColor;
					else
						color = ProjectSettings.Get.Colors.SceneShowPhysicsDynamicInactiveColor;
					viewport.Simple3DRenderer.SetColor( color, color * ProjectSettings.Get.Colors.HiddenByOtherObjectsColorMultiplier );

					foreach( var shape in body.GetComponents<CollisionShape>() )
					{
						if( shape.Enabled )
							shape.Render( viewport, bodyTransform, false, ref verticesRendered );
					}
					//foreach( var shape in GetComponents<CollisionShape>( onlyEnabledInHierarchy: true ) )
					//	shape.Render( viewport, bodyTransform, false, ref verticesRendered );
					////foreach( var shape in GetComponents<CollisionShape>( false, true, true ) )
					////	shape.Render( viewport, Transform, false, ref verticesRendered );

					//center of mass
					if( body.MotionType.Value == PhysicsMotionType.Dynamic && physicalBody != null )
					{
						//!!!!

						Vector3 centerOfMass;
						//if( physicalBody != null )
						//{
						physicalBody.GetShapeCenterOfMass( out var centerOfMassF );
						centerOfMass = centerOfMassF.ToVector3();
						//}
						//else
						//	centerOfMass = body.CenterOfMassOffset;

						renderer.SetColor( new ColorValue( 1, 0, 0 ), new ColorValue( 1, 0, 0 ) );//, 0.5f ) );

						var size3 = body.SpaceBounds.BoundingBox.GetSize();
						var halfSize = size3.MaxComponent() / 30; //var halfSize = size3.MinComponent() / 10;

						Matrix4 t = bodyTransform.ToMatrix4();
						t *= new Matrix4( Matrix3.Identity, centerOfMass );
						t.GetTranslation( out var pos );

						renderer.AddLineThin( pos + new Vector3( -halfSize, 0, 0 ), pos + new Vector3( halfSize, 0, 0 ) );
						renderer.AddLineThin( pos + new Vector3( 0, -halfSize, 0 ), pos + new Vector3( 0, halfSize, 0 ) );
						renderer.AddLineThin( pos + new Vector3( 0, 0, -halfSize ), pos + new Vector3( 0, 0, halfSize ) );

						verticesRendered += 6;


						////Vector3 centerOfMass;
						////if( physicalBody != null )
						////{
						////	physicalBody.GetShapeCenterOfMass( out var centerOfMassF );
						////	centerOfMass = centerOfMassF.ToVector3();
						////}
						////else
						////	centerOfMass = body.CenterOfMassOffset;

						////renderer.SetColor( new ColorValue( 1, 0, 0 ), new ColorValue( 1, 0, 0 ) );//, 0.5f ) );

						////var size3 = body.SpaceBounds.BoundingBox.GetSize();
						////var halfSize = size3.MaxComponent() / 30; //var halfSize = size3.MinComponent() / 10;

						////Matrix4 t = bodyTransform.ToMatrix4();
						////t *= new Matrix4( Matrix3.Identity, centerOfMass );
						////t.GetTranslation( out var pos );

						////renderer.AddLineThin( pos + new Vector3( -halfSize, 0, 0 ), pos + new Vector3( halfSize, 0, 0 ) );
						////renderer.AddLineThin( pos + new Vector3( 0, -halfSize, 0 ), pos + new Vector3( 0, halfSize, 0 ) );
						////renderer.AddLineThin( pos + new Vector3( 0, 0, -halfSize ), pos + new Vector3( 0, 0, halfSize ) );

						////verticesRendered += 6;
					}

					//!!!!impl

					////ccd sphere radius
					//if( CCD && MotionType.Value == MotionTypeEnum.Dynamic )
					//{
					//	context.Owner.Simple3DRenderer.AddSphere( new Sphere( TransformV.Position, rigidBody.CcdSweptSphereRadius ), 16, false );
					//	verticesRendered += 16 * 3 * 8;
					//}
				}

				//!!!!copy code
				//draw selection
				if( renderSelected || renderCanSelect )
				//if( renderSelected && ( context2.selectedObjects.Contains( this ) || context2.canSelectObjects.Contains( this ) ) )
				{
					ColorValue color;
					if( renderSelected )
						color = ProjectSettings.Get.Colors.SelectedColor;
					else if( renderCanSelect )
						color = ProjectSettings.Get.Colors.CanSelectColor;
					else
						color = ProjectSettings.Get.Colors.SceneShowPhysicsDynamicActiveColor;

					//ColorValue color;
					//if( context2.selectedObjects.Contains( this ) )
					//	color = ProjectSettings.Get.Colors.SelectedColor;
					//else if( context2.canSelectObjects.Contains( this ) )
					//	color = ProjectSettings.Get.Colors.CanSelectColor;
					//else
					//	color = ProjectSettings.Get.Colors.SceneShowPhysicsDynamicActiveColor;

					//!!!!��� ��������� ����� ���������� ������������?

					color.Alpha *= .5f;
					viewport.Simple3DRenderer.SetColor( color, color * ProjectSettings.Get.Colors.HiddenByOtherObjectsColorMultiplier );

					foreach( var shape in body.GetComponents<CollisionShape>() )
					{
						if( shape.Enabled )
							shape.Render( viewport, bodyTransform, true, ref verticesRendered );
					}
					//foreach( var shape in GetComponents<CollisionShape>( onlyEnabledInHierarchy: true ) )
					//	shape.Render( viewport, bodyTransform, true, ref verticesRendered );
					////foreach( var shape in GetComponents<CollisionShape>( false, true, true ) )
					////	shape.Render( viewport, Transform, true, ref verticesRendered );

					//context.viewport.DebugGeometry.AddBounds( SpaceBounds.CalculatedBoundingBox );
				}

				//display collision contacts
				if( body.DisplayContacts && physicalBody != null ) //PhysicalBody != null )
				{
					unsafe
					{
						physicalBody.GetContacts( out var itemCount, out var itemBuffer );
						if( itemCount != 0 )
						{
							var size3 = body.SpaceBounds.BoundingBox.GetSize();
							var scale = size3.MinComponent() / 30;

							renderer.SetColor( new ColorValue( 1, 0, 0 ) );

							for( int nItem = 0; nItem < itemCount; nItem++ )
							{
								ref var item = ref itemBuffer[ nItem ];

								for( int nPoint = 0; nPoint < item.ContactPointCount; nPoint++ )
								{
									ref var point = ref item.GetContactPointOn1( nPoint );

									var bounds = new Bounds( point );
									bounds.Expand( scale );

									renderer.AddBounds( bounds, true );
								}
							}
						}
					}

					//var contacts = PhysicalBody.GetContacts();
					//if( contacts.Count != 0 )
					//{
					//	var size3 = SpaceBounds.BoundingBox.GetSize();
					//	var scale = (float)Math.Min( size3.X, Math.Min( size3.Y, size3.Z ) ) / 30;

					//	renderer.SetColor( new ColorValue( 1, 0, 0 ) );

					//	for( int nContact = 0; nContact < contacts.Count; nContact++ )
					//	{
					//		ref var contact = ref contacts.Array[ contacts.Offset + nContact ];//var contact = contacts[ n ];

					//		var pos = contact.WorldPositionOn1;//var pos = contact.PositionWorldOnA;
					//		var bounds = new Bounds(
					//			pos - new Vector3( scale, scale, scale ),
					//			pos + new Vector3( scale, scale, scale ) );

					//		renderer.AddBounds( bounds, true );
					//	}
					//}
				}

				////display collision contacts
				//if( DisplayContacts &&  ContactsData != null && ContactsData.Count != 0 )
				//{


				//	var size3 = SpaceBounds.CalculatedBoundingBox.GetSize();
				//	var scale = (float)Math.Min( size3.X, Math.Min( size3.Y, size3.Z ) ) / 30;

				//	renderer.SetColor( new ColorValue( 1, 0, 0 ) );

				//	var data = ContactsData;
				//	for( int n = 0; n < data.Count; n++ )
				//	{
				//		ref var item = ref data.Data[ n ];
				//		if( item.Distance < 0 && item.SimulationSubStep == 0 )
				//		{
				//			var pos = item.PositionWorldOnA;
				//			var bounds = new Bounds(
				//				pos - new Vector3( scale, scale, scale ),
				//				pos + new Vector3( scale, scale, scale ) );

				//			renderer.AddBounds( bounds, true );
				//		}
				//	}
				//}

				//}
			}
		}

		//public override void RenderPhysicalObject( ViewportRenderingContext context, out int verticesRendered )
		//{
		//	if( physicalBody != null )
		//	{
		//		var context2 = context.ObjectInSpaceRenderingContext;
		//		Render( context, Active, context2.selectedObjects.Contains( this ), context2.canSelectObjects.Contains( this ), Transform, out verticesRendered );
		//	}
		//	else
		//		verticesRendered = 0;
		//}

		protected override void OnGetRenderSceneData( ViewportRenderingContext context, GetRenderSceneDataMode mode, Scene.GetObjectsInSpaceItem modeGetObjectsItem )
		{
			base.OnGetRenderSceneData( context, mode, modeGetObjectsItem );

			if( mode == GetRenderSceneDataMode.InsideFrustum )
			{
				var context2 = context.ObjectInSpaceRenderingContext;

				bool show = context.SceneDisplayDevelopmentDataInThisApplication && ParentScene.DisplayPhysicalObjects;
#if !DEPLOY
				if( !show )
					show = context2.selectedObjects.Contains( this ) || context2.canSelectObjects.Contains( this ) || context2.objectToCreate == this;
#endif

				if( show && physicalBody != null )
				{
					var verticesRendered = 0;
					Render( context, Active, context2.selectedObjects.Contains( this ), context2.canSelectObjects.Contains( this ), Transform, ref verticesRendered, this, PhysicalBody );
				}

				var showLabels = /*show &&*/ physicalBody == null;
				if( !showLabels )
					context2.disableShowingLabelForThisObject = true;
			}
		}

		//!!!!maybe event ActiveChanged
		[Browsable( false )]
		public bool Active
		{
			get
			{
				if( physicalBody != null )
					return physicalBody.Active;
				return false;
			}
			set
			{
				if( physicalBody != null )
					physicalBody.Active = value;
			}
		}

		//public void Activate()
		//{
		//	physicalBody?.Activate();
		//}

		//public void WantsDeactivation()
		//{
		//	//!!!!���� ����� ������������, �� �������������

		//	physicalBody?.Deactivate();
		//}

		public void ApplyForce( Vector3 force, Vector3 relativePosition )
		{
			if( physicalBody != null && MotionType.Value == PhysicsMotionType.Dynamic && force != Vector3.Zero )
			{
				var force2 = force.ToVector3F();
				var relativePosition2 = relativePosition.ToVector3F();
				physicalBody.ApplyForce( ref force2, ref relativePosition2 );

				////Activate();
				////BulletPhysicsUtility.Convert( ref force, out var bForce );
				////BulletPhysicsUtility.Convert( ref relativePosition, out var bRelPos );
				////rigidBody?.ApplyForceRef( ref bForce, ref bRelPos );
			}
		}

		public delegate void CollisionSoundPlayBeforeDelegate( RigidBody sender, ref Sound sound, ref double volume, ref bool handled );
		public event CollisionSoundPlayBeforeDelegate CollisionSoundPlayBefore;

		public void CollisionSoundPlay()
		{
			if( !SoundWorld.BackendNull )
			{
				var sound = CollisionSound.Value;
				var volume = 1.0;

				var handled = false;
				CollisionSoundPlayBefore?.Invoke( this, ref sound, ref volume, ref handled );
				if( handled )
					return;

				if( sound != null )
				{
					//specify position of collision?
					ParentScene?.SoundPlay( sound, TransformV.Position, volume: volume );

					collisionSoundRemainingTime = (float)CollisionSoundTimeInterval;
				}
			}

			if( NetworkIsServer && CollisionSound.ReferenceOrValueSpecified )
			{
				BeginNetworkMessageToEveryone( "CollisionSoundPlay" );
				EndNetworkMessage();
			}
		}

		protected override bool OnReceiveNetworkMessageFromServer( string message, ArrayDataReader reader )
		{
			if( !base.OnReceiveNetworkMessageFromServer( message, reader ) )
				return false;

			if( message == "CollisionSoundPlay" )
				CollisionSoundPlay();

			return true;
		}
	}
}
