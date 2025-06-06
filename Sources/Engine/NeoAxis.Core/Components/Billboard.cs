// Copyright (C) NeoAxis Group Ltd. 8 Copthall, Roseau Valley, 00152 Commonwealth of Dominica.
using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace NeoAxis
{
	/// <summary>
	/// A flat object in the scene, which faces the camera.
	/// </summary>
	public class Billboard : ObjectInSpace
	{
		static Mesh billboardMesh;

		double transformPositionByTime1_Time;
		Vector3 transformPositionByTime1_Position;
		double transformPositionByTime2_Time;
		Vector3 transformPositionByTime2_Position;

		//List<SceneLODUtility.RenderingContextItem> renderingContextItems;

		/////////////////////////////////////////

		/// <summary>
		/// The size of the billboard.
		/// </summary>
		[DefaultValue( "1 1" )]
		[Range( 0, 10, RangeAttribute.ConvenientDistributionEnum.Exponential )]
		public Reference<Vector2> Size
		{
			get { if( _size.BeginGet() ) Size = _size.Get( this ); return _size.value; }
			set
			{
				if( _size.BeginSet( this, ref value ) )
				{
					try
					{
						SizeChanged?.Invoke( this );
						SpaceBoundsUpdate();
					}
					finally { _size.EndSet(); }
				}
			}
		}
		/// <summary>Occurs when the <see cref="Size"/> property value changes.</summary>
		public event Action<Billboard> SizeChanged;
		ReferenceField<Vector2> _size = new Vector2( 1, 1 );

		/// <summary>
		/// The rotation of the billboard.
		/// </summary>
		[DefaultValue( 0.0 )]
		[Range( -360, 360 )]
		public Reference<Degree> Rotation
		{
			get { if( _rotation.BeginGet() ) Rotation = _rotation.Get( this ); return _rotation.value; }
			set { if( _rotation.BeginSet( this, ref value ) ) { try { RotationChanged?.Invoke( this ); } finally { _rotation.EndSet(); } } }
		}
		public event Action<Billboard> RotationChanged;
		ReferenceField<Degree> _rotation = new Degree( 0.0 );

		/// <summary>
		/// The material of the billboard.
		/// </summary>
		[DefaultValue( null )]
		public Reference<Material> Material
		{
			get
			{
				//fast exit optimization
				var cachedReference = _material.value.Value?.GetCachedResourceReference();
				if( cachedReference != null )
				{
					if( ReferenceEquals( _material.value.GetByReference, cachedReference ) )
						return _material.value;
					if( _material.value.GetByReference == cachedReference )
					{
						_material.value.GetByReference = cachedReference;
						return _material.value;
					}
				}

				if( _material.BeginGet_WithoutFastExitOptimization() ) Material = _material.Get( this ); return _material.value;
				//if( _material.BeginGet() ) Material = _material.Get( this ); return _material.value;
			}
			set { if( _material.BeginSet( this, ref value ) ) { try { MaterialChanged?.Invoke( this ); } finally { _material.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="Material"/> property value changes.</summary>
		public event Action<Billboard> MaterialChanged;
		ReferenceField<Material> _material = null;

		/// <summary>
		/// The base color and opacity multiplier.
		/// </summary>
		[DefaultValue( "1 1 1" )]
		public Reference<ColorValue> Color
		{
			get { if( _color.BeginGet() ) Color = _color.Get( this ); return _color.value; }
			set { if( _color.BeginSet( this, ref value ) ) { try { ColorChanged?.Invoke( this ); } finally { _color.EndSet(); } } }
		}
		public event Action<Billboard> ColorChanged;
		ReferenceField<ColorValue> _color = new ColorValue( 1, 1, 1 );

		//[DefaultValue( 1.0 )]
		//[Range( 0, 1 )]
		//public Reference<double> Opacity
		//{
		//	get { if( _opacity.BeginGet() ) Opacity = _opacity.Get( this ); return _opacity.value; }
		//	set { if( _opacity.BeginSet( this, ref value ) ) { try { OpacityChanged?.Invoke( this ); } finally { _opacity.EndSet(); } } }
		//}
		//public event Action<Billboard> OpacityChanged;
		//ReferenceField<double> _opacity = 1.0;

		/// <summary>
		/// The factor of maximum visibility distance. The maximum distance is calculated based on the size of the object on the screen.
		/// </summary>
		[DefaultValue( 1.0 )]
		[Range( 0, 6, RangeAttribute.ConvenientDistributionEnum.Exponential )]
		public Reference<double> VisibilityDistanceFactor
		{
			get { if( _visibilityDistanceFactor.BeginGet() ) VisibilityDistanceFactor = _visibilityDistanceFactor.Get( this ); return _visibilityDistanceFactor.value; }
			set { if( _visibilityDistanceFactor.BeginSet( this, ref value ) ) { try { VisibilityDistanceFactorChanged?.Invoke( this ); } finally { _visibilityDistanceFactor.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="VisibilityDistanceFactor"/> property value changes.</summary>
		public event Action<Billboard> VisibilityDistanceFactorChanged;
		ReferenceField<double> _visibilityDistanceFactor = 1.0;

		///// <summary>
		///// Maximum visibility range of the object.
		///// </summary>
		//[DefaultValue( 10000.0 )]
		//[Range( 0, 10000, RangeAttribute.ConvenientDistributionEnum.Exponential, 4 )]
		//public Reference<double> VisibilityDistance
		//{
		//	get { if( _visibilityDistance.BeginGet() ) VisibilityDistance = _visibilityDistance.Get( this ); return _visibilityDistance.value; }
		//	set { if( _visibilityDistance.BeginSet( this, ref value ) ) { try { VisibilityDistanceChanged?.Invoke( this ); } finally { _visibilityDistance.EndSet(); } } }
		//}
		///// <summary>Occurs when the <see cref="VisibilityDistance"/> property value changes.</summary>
		//public event Action<Billboard> VisibilityDistanceChanged;
		//ReferenceField<double> _visibilityDistance = 10000.0;

		/// <summary>
		/// Whether to cast shadows on the other surfaces.
		/// </summary>
		[DefaultValue( true )]
		[Serialize]
		public Reference<bool> CastShadows
		{
			get { if( _castShadows.BeginGet() ) CastShadows = _castShadows.Get( this ); return _castShadows.value; }
			set { if( _castShadows.BeginSet( this, ref value ) ) { try { CastShadowsChanged?.Invoke( this ); } finally { _castShadows.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="CastShadows"/> property value changes.</summary>
		public event Action<Billboard> CastShadowsChanged;
		ReferenceField<bool> _castShadows = true;

		/// <summary>
		/// Indent multiplier when rendering shadows to fix overlapping effect of the object with the shadow.
		/// </summary>
		[DefaultValue( 1.0 )]
		[Range( 0, 1 )]
		public Reference<double> ShadowOffset
		{
			get { if( _shadowOffset.BeginGet() ) ShadowOffset = _shadowOffset.Get( this ); return _shadowOffset.value; }
			set { if( _shadowOffset.BeginSet( this, ref value ) ) { try { ShadowOffsetChanged?.Invoke( this ); } finally { _shadowOffset.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="ShadowOffset"/> property value changes.</summary>
		public event Action<Billboard> ShadowOffsetChanged;
		ReferenceField<double> _shadowOffset = 1.0;

		/// <summary>
		/// Whether to can apply decals the billboard.
		/// </summary>
		[DefaultValue( true )]
		public Reference<bool> ReceiveDecals
		{
			get { if( _receiveDecals.BeginGet() ) ReceiveDecals = _receiveDecals.Get( this ); return _receiveDecals.value; }
			set { if( _receiveDecals.BeginSet( this, ref value ) ) { try { ReceiveDecalsChanged?.Invoke( this ); } finally { _receiveDecals.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="ReceiveDecals"/> property value changes.</summary>
		public event Action<Billboard> ReceiveDecalsChanged;
		ReferenceField<bool> _receiveDecals = true;

		/// <summary>
		/// The multiplier of the motion blur effect.
		/// </summary>
		[DefaultValue( 1.0 )]
		[Range( 0, 1 )]
		public Reference<double> MotionBlurFactor
		{
			get { if( _motionBlurFactor.BeginGet() ) MotionBlurFactor = _motionBlurFactor.Get( this ); return _motionBlurFactor.value; }
			set { if( _motionBlurFactor.BeginSet( this, ref value ) ) { try { MotionBlurFactorChanged?.Invoke( this ); } finally { _motionBlurFactor.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="MotionBlurFactor"/> property value changes.</summary>
		public event Action<Billboard> MotionBlurFactorChanged;
		ReferenceField<double> _motionBlurFactor = 1.0;

		/// <summary>
		/// Specifies settings for special object effects, such as an outline effect.
		/// </summary>
		[Cloneable( CloneType.Deep )]
		public Reference<List<ObjectSpecialRenderingEffect>> SpecialEffects
		{
			get { if( _specialEffects.BeginGet() ) SpecialEffects = _specialEffects.Get( this ); return _specialEffects.value; }
			set { if( _specialEffects.BeginSet( this, ref value ) ) { try { SpecialEffectsChanged?.Invoke( this ); } finally { _specialEffects.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="SpecialEffects"/> property value changes.</summary>
		public event Action<Billboard> SpecialEffectsChanged;
		ReferenceField<List<ObjectSpecialRenderingEffect>> _specialEffects = new List<ObjectSpecialRenderingEffect>();

		[Browsable( false )]
		public RenderingPipeline.RenderSceneData.CutVolumeItem[] CutVolumes { get; set; }

		/////////////////////////////////////////

		protected override void OnSpaceBoundsUpdate( ref SpaceBounds newBounds )
		{
			base.OnSpaceBoundsUpdate( ref newBounds );

			var tr = Transform.Value;
			var halfSize = Size.Value * tr.Scale.MaxComponent() * 0.5;
			var r = Math.Sqrt( halfSize.X * halfSize.X + halfSize.Y * halfSize.Y );

			var bounds = new Bounds( tr.Position );
			bounds.Expand( r );
			newBounds = new SpaceBounds( bounds, new Sphere( tr.Position, r ) );
		}

		protected override void OnCheckSelectionByRay( CheckSelectionByRayContext context )
		{
			base.OnCheckSelectionByRay( context );

			context.thisObjectWasChecked = true;

			if( SpaceBounds.BoundingSphere.Intersects( context.ray, out var scale1, out var scale2 ) )
			{
				var m = GetBillboardMesh();

				if( m.Result.ExtractedIndices.Length != 0 )
				{
					Quaternion meshFaceRotation = Quaternion.Identity;
					switch( m.Result.MeshData.BillboardMode )
					{
					case 1: meshFaceRotation = Quaternion.FromDirectionZAxisUp( Vector3.Zero - new Vector3( 0, -1, 0 ) ); break;
					case 2: meshFaceRotation = Quaternion.FromDirectionZAxisUp( Vector3.Zero - new Vector3( 0, 1, 0 ) ); break;
					case 3: meshFaceRotation = Quaternion.FromDirectionZAxisUp( Vector3.Zero - new Vector3( 1, 0, 0 ) ); break;
					case 4: meshFaceRotation = Quaternion.FromDirectionZAxisUp( Vector3.Zero - new Vector3( -1, 0, 0 ) ); break;
					}

					var rotation = context.viewport.CameraSettings.Rotation * meshFaceRotation.GetInverse();
					var tr = TransformV;
					var size2 = new Vector2( Size.Value.X * Math.Max( tr.Scale.X, tr.Scale.Y ), Size.Value.Y * tr.Scale.Z );
					var tranform = new Matrix4( rotation.ToMatrix3() * Matrix3.FromScale( new Vector3( size2.X, size2.X, size2.Y ) ), tr.Position );

					var vertices = m.Result.ExtractedVerticesPositions;
					var indices = m.Result.ExtractedIndices;
					for( int nTriangle = 0; nTriangle < indices.Length / 3; nTriangle++ )
					{
						var vertex0 = vertices[ indices[ nTriangle * 3 + 0 ] ];
						var vertex1 = vertices[ indices[ nTriangle * 3 + 1 ] ];
						var vertex2 = vertices[ indices[ nTriangle * 3 + 2 ] ];

						var v0 = tranform * vertex0;
						var v1 = tranform * vertex1;
						var v2 = tranform * vertex2;

						if( MathAlgorithms.IntersectTriangleRay( ref v0, ref v1, ref v2, ref context.ray, out var rayScale ) )
						{
							context.thisObjectResultRayScale = rayScale;
							break;
						}
					}
				}
				else
				{
					double scale = Math.Min( scale1, scale2 );
					context.thisObjectResultRayScale = scale;
				}
			}
		}

		protected override void OnGetRenderSceneData( ViewportRenderingContext context, GetRenderSceneDataMode mode, Scene.GetObjectsInSpaceItem modeGetObjectsItem )
		{
			base.OnGetRenderSceneData( context, mode, modeGetObjectsItem );

			var time = context.Owner.LastUpdateTime;
			if( time != transformPositionByTime1_Time )
			{
				transformPositionByTime2_Time = transformPositionByTime1_Time;
				transformPositionByTime2_Position = transformPositionByTime1_Position;
				transformPositionByTime1_Time = time;
				transformPositionByTime1_Position = TransformV.Position;
				//context.ConvertToRelative( TransformV.Position, out transformPositionByTime1_Position );
				////transformPositionByTime1_Position = TransformV.Position;
			}

			if( mode == GetRenderSceneDataMode.InsideFrustum || ( mode == GetRenderSceneDataMode.ShadowCasterOutsideFrustum && CastShadows ) )
			{
				var size = Size.Value;
				//var color = Color.Value;
				//var opacity = Opacity.Value;
				//if( size != Vector2.Zero )//&& color.Alpha != 0 && opacity != 0 )
				//{

				var context2 = context.ObjectInSpaceRenderingContext;
				context2.disableShowingLabelForThisObject = true;

				var tr = Transform.Value;
				var cameraSettings = context.Owner.CameraSettings;

				var cameraDistanceMinSquared = SceneLODUtility.GetCameraDistanceMinSquared( cameraSettings, SpaceBounds );

				var boundingSize = (float)Size.Value.MaxComponent();
				var visibilityDistance = context.GetVisibilityDistanceByObjectSize( boundingSize ) * (float)VisibilityDistanceFactor.Value;

				if( cameraDistanceMinSquared < visibilityDistance * visibilityDistance/* || mode == GetRenderSceneDataMode.ShadowCasterOutsideFrustum*/ )
				{
#if !DEPLOY
					var allowOutlineSelect = context.renderingPipeline.UseRenderTargets && ProjectSettings.Get.SceneEditor.SceneEditorSelectOutlineEffectEnabled;
#endif

					var item = new RenderingPipeline.RenderSceneData.BillboardItem();
					item.Creator = this;
					SpaceBounds.boundingBox.GetCenter( out item.BoundingBoxCenter );
					item.BoundingSphere = SpaceBounds.BoundingSphere;
					item.CastShadows = CastShadows && cameraDistanceMinSquared < context.GetShadowVisibilityDistanceSquared( visibilityDistance );
					item.ShadowOffset = (float)ShadowOffset.Value;
					item.ReceiveDecals = ReceiveDecals;
					item.MotionBlurFactor = (float)MotionBlurFactor;
					item.Material = Material;
					item.VisibilityDistance = (float)visibilityDistance;
					item.CutVolumes = CutVolumes;

					var specialEffects = SpecialEffects.Value;
					if( specialEffects != null && specialEffects.Count != 0 )
						item.SpecialEffects = specialEffects;

#if !DEPLOY
					//display outline effect of editor selection
					if( mode == GetRenderSceneDataMode.InsideFrustum && allowOutlineSelect && context2.selectedObjects.Contains( this ) )
					{
						if( context2.displayBillboardsCounter < context2.displayBillboardsMax )
						{
							context2.displayBillboardsCounter++;

							var color = ProjectSettings.Get.Colors.SelectedColor.Value;
							//color.Alpha *= .5f;

							var effect = new ObjectSpecialRenderingEffect_Outline();
							effect.Group = int.MaxValue;
							effect.Color = color;

							if( item.SpecialEffects != null )
								item.SpecialEffects = new List<ObjectSpecialRenderingEffect>( item.SpecialEffects );
							else
								item.SpecialEffects = new List<ObjectSpecialRenderingEffect>();
							item.SpecialEffects.Add( effect );
						}
					}
#endif

					context.ConvertToRelative( tr.Position, out item.PositionRelative );
					//item.Position = tr.Position.ToVector3F();

					item.Size = new Vector2( size.X * Math.Max( tr.Scale.X, tr.Scale.Y ), size.Y * tr.Scale.Z ).ToVector2F();
					item.RotationAngle = Rotation.Value.InRadians().ToRadianF();
					if( tr.Rotation != Quaternion.Identity )
					{
						tr.Rotation.ToAngles( out var angles );
						item.RotationAngle += (float)MathEx.DegreeToRadian( angles.Yaw );
					}
					item.RotationQuaternion = tr.Rotation.ToQuaternionF();
					item.Color = Color;
					RenderingPipeline.GetColorForInstancingData( ref item.Color, out item.ColorForInstancingData1, out item.ColorForInstancingData2 );
					//item.ColorForInstancingData = RenderingPipeline.GetColorForInstancingData( ref item.Color );

					//PositionPreviousFrame
					var previousTime = time - context.Owner.LastUpdateTimeStep;
					if( !GetTransformPositionByTime( previousTime, out var previousPosition ) )
					{
						previousPosition = tr.Position;
						//context.ConvertToRelative( tr.Position, out previousPosition );
						////previousPosition = tr.Position;
					}
					item.PreviousFramePositionChange = ( tr.Position - previousPosition ).ToVector3F();
					//item.PositionPreviousFrameRelative = previousPosition;
					////context.ConvertToRelative( ref previousPosition, out item.PositionPreviousFrameRelative );
					////item.PositionPreviousFrame = previousPosition.ToVector3F();

					context.FrameData.RenderSceneData.Billboards.Add( ref item );

#if !DEPLOY
					//display editor selection
					if( mode == GetRenderSceneDataMode.InsideFrustum )
					{
						if( ( !allowOutlineSelect && context2.selectedObjects.Contains( this ) ) || context2.canSelectObjects.Contains( this ) )// || context.dragDropCreateObject == this )
						{
							if( context2.displayBillboardsCounter < context2.displayBillboardsMax )
							{
								context2.displayBillboardsCounter++;

								ColorValue color;
								if( context2.selectedObjects.Contains( this ) )
									color = ProjectSettings.Get.Colors.SelectedColor;
								else
									color = ProjectSettings.Get.Colors.CanSelectColor;
								color.Alpha *= .5f;

								var viewport = context.Owner;
								if( viewport.Simple3DRenderer != null )
								{
									viewport.Simple3DRenderer.SetColor( color, color * ProjectSettings.Get.Colors.HiddenByOtherObjectsColorMultiplier );

									item.GetWorldMatrixRelative( out var worldMatrix );

									context.ConvertToAbsolute( ref worldMatrix, out var worldMatrixAbsolute );
									viewport.Simple3DRenderer.AddMesh( GetBillboardMesh().Result, ref worldMatrixAbsolute, false, false );

									//worldMatrix.ToMatrix4( out var worldMatrix2 );
									//viewport.Simple3DRenderer.AddMesh( GetBillboardMesh().Result, ref worldMatrix2, false, false );
								}
							}
						}
					}
#endif
				}

				//}
			}
		}

		public static Mesh GetBillboardMesh()
		{
			if( billboardMesh == null || billboardMesh.Disposed )
				billboardMesh = null;

			if( billboardMesh == null )
			{
				var mesh = ComponentUtility.CreateComponent<Mesh>( null, true, false );
				var geometry = mesh.CreateComponent<MeshGeometry_Plane>();
				geometry.Axis = 1;
				mesh.Billboard = true;
				mesh.Enabled = true;

				billboardMesh = mesh;
			}

			return billboardMesh;
		}

		//maybe add GetLinearVelocityByRenderingData()
		bool GetTransformPositionByTime( double time, out Vector3 position )
		{
			if( Math.Abs( transformPositionByTime2_Time - time ) < 0.00001 )
			{
				position = transformPositionByTime2_Position;
				return true;
			}
			if( Math.Abs( transformPositionByTime1_Time - time ) < 0.00001 )
			{
				position = transformPositionByTime1_Position;
				return true;
			}
			position = Vector3F.Zero;
			return false;
		}

		//public void ResetLodTransitionStates( ViewportRenderingContext resetOnlySpecifiedContext = null )
		//{
		//	SceneLODUtility.ResetLodTransitionStates( ref renderingContextItems, resetOnlySpecifiedContext );
		//}

		public override void NewObjectSetDefaultConfiguration( bool createdFromNewObjectWindow = false )
		{
			base.NewObjectSetDefaultConfiguration( createdFromNewObjectWindow );

			Material = ReferenceUtility.MakeReference( @"Base\Components\Billboard default.material" );
		}

		protected override void OnMetadataGetMembersFilter( Metadata.GetMembersContext context, Metadata.Member member, ref bool skip )
		{
			base.OnMetadataGetMembersFilter( context, member, ref skip );

			if( member is Metadata.Property )
			{
				switch( member.Name )
				{
				case nameof( ShadowOffset ):
					if( !CastShadows )
						skip = true;
					break;
				}
			}
		}
	}
}
