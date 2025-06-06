// Copyright (C) NeoAxis Group Ltd. 8 Copthall, Roseau Valley, 00152 Commonwealth of Dominica.
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NeoAxis.Editor;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace NeoAxis
{
	/// <summary>
	/// Particle system in the scene.
	/// </summary>
#if !DEPLOY
	[SettingsCell( "NeoAxis.Editor.ParticleSystemInSpaceSettingsCell" )]
#endif
	public class ParticleSystemInSpace : ObjectInSpace
	{
		static FastRandom staticRandom = new FastRandom( 0 );

		//creation
		ParticleSystem.CompiledData currentParticleSystem;
		int currentMustRecreateCounter;

		double transformPositionByTime1_Time;
		Vector3[] transformPositionByTime1_Position;
		double transformPositionByTime2_Time;
		Vector3[] transformPositionByTime2_Position;

		//List<SceneLODUtility.RenderingContextItem> billboardRenderingContextItems;

		Emitter[] Emitters;
		Particle[] Objects;
		OpenList<bool> removedObjects;//Count = capacity of Particles. Can be null.
		Stack<int> freeObjects = new Stack<int>();

		float playingTime;
		bool playingEnded;

		int mergeSimulationStepsCounter;

		//!!!!�������. ����� ����� �������� ����
		//public Batch batch;

		/////////////////////////////////////////

		/// <summary>
		/// The particle system used by the object.
		/// </summary>
		public Reference<ParticleSystem> ParticleSystem
		{
			get
			{
				//fast exit optimization
				var cachedReference = _particleSystem.value.Value?.GetCachedResourceReference();
				if( cachedReference != null )
				{
					if( ReferenceEquals( _particleSystem.value.GetByReference, cachedReference ) )
						return _particleSystem.value;
					if( _particleSystem.value.GetByReference == cachedReference )
					{
						_particleSystem.value.GetByReference = cachedReference;
						return _particleSystem.value;
					}
				}
				if( _particleSystem.BeginGet_WithoutFastExitOptimization() ) ParticleSystem = _particleSystem.Get( this ); return _particleSystem.value;
				//if( _particleSystem.BeginGet() ) ParticleSystem = _particleSystem.Get( this ); return _particleSystem.value;
			}
			set
			{
				if( _particleSystem.BeginSet( this, ref value ) )
				{
					try
					{
						ParticleSystemChanged?.Invoke( this );
						RecreateData( false );
					}
					finally { _particleSystem.EndSet(); }
				}
			}
		}
		/// <summary>Occurs when the <see cref="ParticleSystem"/> property value changes.</summary>
		public event Action<ParticleSystemInSpace> ParticleSystemChanged;
		ReferenceField<ParticleSystem> _particleSystem;

		/// <summary>
		/// Replaces all geometries of the mesh by another material.
		/// </summary>
		[Serialize]
		[DefaultValue( null )]
		public Reference<Material> ReplaceMaterial
		{
			get { if( _replaceMaterial.BeginGet() ) ReplaceMaterial = _replaceMaterial.Get( this ); return _replaceMaterial.value; }
			set { if( _replaceMaterial.BeginSet( this, ref value ) ) { try { ReplaceMaterialChanged?.Invoke( this ); } finally { _replaceMaterial.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="ReplaceMaterial"/> property value changes.</summary>
		public event Action<ParticleSystemInSpace> ReplaceMaterialChanged;
		ReferenceField<Material> _replaceMaterial;

		/// <summary>
		/// The base color and opacity multiplier.
		/// </summary>
		[DefaultValue( "1 1 1" )]
		public Reference<ColorValue> Color
		{
			get { if( _color.BeginGet() ) Color = _color.Get( this ); return _color.value; }
			set { if( _color.BeginSet( this, ref value ) ) { try { ColorChanged?.Invoke( this ); } finally { _color.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="Color"/> property value changes.</summary>
		public event Action<ParticleSystemInSpace> ColorChanged;
		ReferenceField<ColorValue> _color = ColorValue.One;

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
		public event Action<ParticleSystemInSpace> VisibilityDistanceFactorChanged;
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
		//public event Action<ParticleSystemInSpace> VisibilityDistanceChanged;
		//ReferenceField<double> _visibilityDistance = 10000.0;

		/// <summary>
		/// Whether to cast shadows on the other surfaces.
		/// </summary>
		[DefaultValue( true )]
		public Reference<bool> CastShadows
		{
			get { if( _castShadows.BeginGet() ) CastShadows = _castShadows.Get( this ); return _castShadows.value; }
			set { if( _castShadows.BeginSet( this, ref value ) ) { try { CastShadowsChanged?.Invoke( this ); } finally { _castShadows.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="CastShadows"/> property value changes.</summary>
		public event Action<ParticleSystemInSpace> CastShadowsChanged;
		ReferenceField<bool> _castShadows = true;

		/// <summary>
		/// Whether it is possible to apply decals on the surface.
		/// </summary>
		[DefaultValue( true )]
		public Reference<bool> ReceiveDecals
		{
			get { if( _receiveDecals.BeginGet() ) ReceiveDecals = _receiveDecals.Get( this ); return _receiveDecals.value; }
			set { if( _receiveDecals.BeginSet( this, ref value ) ) { try { ReceiveDecalsChanged?.Invoke( this ); } finally { _receiveDecals.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="ReceiveDecals"/> property value changes.</summary>
		public event Action<ParticleSystemInSpace> ReceiveDecalsChanged;
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
		public event Action<ParticleSystemInSpace> MotionBlurFactorChanged;
		ReferenceField<double> _motionBlurFactor = 1.0;

		/// <summary>
		/// Whether to display the shapes of the emitters.
		/// </summary>
		[DefaultValue( false )]
		public Reference<bool> DisplayEmitters
		{
			get { if( _displayEmitters.BeginGet() ) DisplayEmitters = _displayEmitters.Get( this ); return _displayEmitters.value; }
			set { if( _displayEmitters.BeginSet( this, ref value ) ) { try { DisplayEmittersChanged?.Invoke( this ); } finally { _displayEmitters.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="DisplayEmitters"/> property value changes.</summary>
		public event Action<ParticleSystemInSpace> DisplayEmittersChanged;
		ReferenceField<bool> _displayEmitters = false;

		/// <summary>
		/// Whether the particle system is in active state.
		/// </summary>
		[DefaultValue( true )]
		public Reference<bool> Activated
		{
			get { if( _activated.BeginGet() ) Activated = _activated.Get( this ); return _activated.value; }
			set { if( _activated.BeginSet( this, ref value ) ) { try { ActivatedChanged?.Invoke( this ); } finally { _activated.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="Activated"/> property value changes.</summary>
		public event Action<ParticleSystemInSpace> ActivatedChanged;
		ReferenceField<bool> _activated = true;

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
		public event Action<ParticleSystemInSpace> SpecialEffectsChanged;
		ReferenceField<List<ObjectSpecialRenderingEffect>> _specialEffects = new List<ObjectSpecialRenderingEffect>();

		[DefaultValue( 2 )]
		[Category( "Optimization" )]
		[Range( 0, 4 )]
		public Reference<int> MergeSimulationSteps
		{
			get { if( _mergeSimulationSteps.BeginGet() ) MergeSimulationSteps = _mergeSimulationSteps.Get( this ); return _mergeSimulationSteps.value; }
			set { if( _mergeSimulationSteps.BeginSet( this, ref value ) ) { try { MergeSimulationStepsChanged?.Invoke( this ); } finally { _mergeSimulationSteps.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="MergeSimulationSteps"/> property value changes.</summary>
		public event Action<ParticleSystemInSpace> MergeSimulationStepsChanged;
		ReferenceField<int> _mergeSimulationSteps = 2;

		[Browsable( false )]
		public RenderingPipeline.RenderSceneData.CutVolumeItem[] CutVolumes { get; set; }

		public float PlayingTime
		{
			get { return playingTime; }
		}

		[Browsable( false )]
		public bool PlayingEnded
		{
			get { return playingEnded; }
		}

		/////////////////////////////////////////

		[StructLayout( LayoutKind.Sequential, Pack = 1 )]
		struct Emitter
		{
			public float StartTime;
			public float Duration;
			public float AvailableTimeToSpawn;

			//public List<SceneLODUtility.RenderingContextItem> meshRenderingContextItems;
		}

		/////////////////////////////////////////

		/// <summary>
		/// Represents a particle for <see cref="ParticleSystemInSpace"/>.
		/// </summary>
		[StructLayout( LayoutKind.Sequential, Pack = 1 )]
		public struct Particle
		{
			//constants
			public int Emitter;
			public float Lifetime;
			public float StartSize;
			public float GravityMultiplier;
			public ColorValue StartColor;

			public float Time;

			public Vector3 Position;
			public Matrix3F Rotation;
			//public Vector3F Scale;
			public float Size;

			public Vector3F LinearVelocity;
			public Vector3F AngularVelocity;

			public ColorValue Color;

			public Vector4F AnyData;
			//public Vector4F Special1;
			//public Vector4F Special2;
		}

		/////////////////////////////////////////

		//public class Batch
		//{
		//	//!!!!

		//	//public ushort Element;
		//	//public byte VariationGroup;
		//	//public byte VariationElement;

		//	//public List<int> Objects = new List<int>( 128 );

		//	////public Vector3 ObjectsCenter;
		//	//public Vector3 BoundingBoxCenter;
		//	//public Bounds BoundingBoxObjectPositions;
		//	//public Sphere BoundingSphere;

		//	public GpuVertexBuffer BatchingInstanceBufferMesh;
		//	public GpuVertexBuffer BatchingInstanceBufferBillboard;

		//	//!!!!
		//	//public List<SceneLOD.RenderingContextItem> renderingContextItems;
		//}

		/////////////////////////////////////////

		class MeshLodItem
		{
			//public SceneLODUtility.RenderingContextItem contextItem;
			public int itemLod = 0;
			public float itemLodValue = 0;
			public int item2Lod = 0;
		}

		/////////////////////////////////////////

		[MethodImpl( (MethodImplOptions)512 )]
		protected override void OnGetRenderSceneData( ViewportRenderingContext context, GetRenderSceneDataMode mode, Scene.GetObjectsInSpaceItem modeGetObjectsItem )
		{
			base.OnGetRenderSceneData( context, mode, modeGetObjectsItem );

			//update old positions for motion blur
			var time = context.Owner.LastUpdateTime;
			if( time != transformPositionByTime1_Time )
			{
				transformPositionByTime2_Time = transformPositionByTime1_Time;
				transformPositionByTime2_Position = transformPositionByTime1_Position;
				transformPositionByTime1_Time = time;

				//!!!!slowly

				transformPositionByTime1_Position = null;

				var tr = Transform.Value;
				ref var trMatrix = ref tr.ToMatrix4();

				transformPositionByTime1_Position = new Vector3[ ObjectsGetCapacity() ];
				for( int n = 0; n < transformPositionByTime1_Position.Length; n++ )
					transformPositionByTime1_Position[ n ] = new Vector3( double.NaN, double.NaN, double.NaN );
				foreach( var particleIndex in ObjectsGetAll() )
				{
					ref var particle = ref Objects[ particleIndex ];

					Vector3 p;
					if( currentParticleSystem.SimulationSpace == NeoAxis.ParticleSystem.SimulationSpaceEnum.Local )
						Matrix4.Multiply( ref trMatrix, ref particle.Position, out p );
					else
						p = particle.Position;
					transformPositionByTime1_Position[ particleIndex ] = p;
					//context.ConvertToRelative( ref p, out transformPositionByTime1_Position[ particleIndex ] );
					////transformPositionByTime1_Position[ particleIndex ] = p;
				}
			}

			if( mode == GetRenderSceneDataMode.InsideFrustum || ( mode == GetRenderSceneDataMode.ShadowCasterOutsideFrustum && CastShadows ) )
			{
				var particleSystemResult = ParticleSystem.Value?.Result;

				//check for update
				if( particleSystemResult != currentParticleSystem )
				{
					RecreateData( true );
					particleSystemResult = currentParticleSystem;
				}

				if( particleSystemResult != null && ObjectsGetCount() != 0 )
				{
					//var context2 = context.objectInSpaceRenderingContext;
					//context2.disableShowingLabelForThisObject = true;

					var cameraSettings = context.Owner.CameraSettings;
					var tr = Transform.Value;
					ref var trMatrix = ref tr.ToMatrix4();

					context.ConvertToRelative( ref trMatrix, out var trMatrixF );
					//trMatrix.ToMatrix4F( out var trMatrixF );

					var cameraDistanceMinSquared = SceneLODUtility.GetCameraDistanceMinSquared( cameraSettings, SpaceBounds );

					var boundingSize = (float)( SpaceBounds.boundingSphere.Radius * 2 );
					var visibilityDistance = (float)( context.GetVisibilityDistanceByObjectSize( boundingSize ) * VisibilityDistanceFactor * currentParticleSystem.Owner.VisibilityDistanceFactor );
					//var visibilityDistance = Math.Min( VisibilityDistance, currentParticleSystem.Owner.VisibilityDistance );

					if( cameraDistanceMinSquared < visibilityDistance * visibilityDistance /*|| mode == GetRenderSceneDataMode.ShadowCasterOutsideFrustum*/ )
					{
						var cameraDistanceMin = MathEx.Sqrt( cameraDistanceMinSquared );
						var cameraDistanceMaxSquared = SceneLODUtility.GetCameraDistanceMax( cameraSettings, SpaceBounds );
						cameraDistanceMaxSquared *= cameraDistanceMaxSquared;

						var shadowVisibilityByDistance = cameraDistanceMinSquared < context.GetShadowVisibilityDistanceSquared( visibilityDistance );

						var replaceMaterial = ReplaceMaterial.Value;
						var color = Color.Value;

						//PositionPreviousFrame
						var previousTime = time - context.Owner.LastUpdateTimeStep;
						GetTransformPositionByTime( previousTime, out var previousPositions );


						SceneLODUtility.LodState?[] meshEmitterLodStates = null;

						//init general parameters of billboard item
						var billboardItem = new RenderingPipeline.RenderSceneData.BillboardItem();
						billboardItem.Creator = this;
						billboardItem.BoundingSphere = SpaceBounds.boundingSphere;
						billboardItem.BoundingBoxCenter = billboardItem.BoundingSphere.Center;
						//SpaceBounds.CalculatedBoundingBox.GetCenter( out billboardItem.BoundingBoxCenter );

						//!!!!
						billboardItem.ShadowOffset = (float)1.0;
						//!!!!����� ������� ��������
						billboardItem.VisibilityDistance = (float)visibilityDistance;

						billboardItem.CutVolumes = CutVolumes;

						var specialEffects = SpecialEffects.Value;
						if( specialEffects != null && specialEffects.Count != 0 )
							billboardItem.SpecialEffects = specialEffects;


						//init general parameters of mesh item
						var meshItem = new RenderingPipeline.RenderSceneData.MeshItem();
						meshItem.Creator = this;
						//meshItem.BoundingBoxCenter = billboardItem.BoundingBoxCenter;
						meshItem.BoundingSphere = billboardItem.BoundingSphere;
						//!!!!����� ������� ��������
						meshItem.VisibilityDistance = (float)visibilityDistance;
						meshItem.CutVolumes = CutVolumes;

						if( specialEffects != null && specialEffects.Count != 0 )
							meshItem.SpecialEffects = specialEffects;

						//!!!!slowly? ObjectsGetAll
						foreach( var particleIndex in ObjectsGetAll() )
						{
							ref var particle = ref Objects[ particleIndex ];

							if( particle.Emitter < currentParticleSystem.Emitters.Length )
							{
								var compiledEmitter = currentParticleSystem.Emitters[ particle.Emitter ];

								var renderingMode = compiledEmitter.Owner.RenderingMode.Value;
								if( renderingMode == ParticleEmitter.RenderingModeEnum.Billboard )
								{
									//Billboard rendering mode

									billboardItem.CastShadows = compiledEmitter.CastShadows && CastShadows && shadowVisibilityByDistance;

									if( mode == GetRenderSceneDataMode.InsideFrustum || ( mode == GetRenderSceneDataMode.ShadowCasterOutsideFrustum && billboardItem.CastShadows ) )
									{
										billboardItem.ReceiveDecals = compiledEmitter.ReceiveDecals && ReceiveDecals;
										billboardItem.MotionBlurFactor = compiledEmitter.MotionBlurFactor * (float)MotionBlurFactor;
										billboardItem.Material = replaceMaterial ?? compiledEmitter.Material;

										//Position
										Vector3 p;
										if( currentParticleSystem.SimulationSpace == NeoAxis.ParticleSystem.SimulationSpaceEnum.Local )
											Matrix4.Multiply( ref trMatrix, ref particle.Position, out p );
										else
											p = particle.Position;

										context.ConvertToRelative( ref p, out billboardItem.PositionRelative );
										//billboardItem.Position = p.ToVector3F();

										//Size
										if( currentParticleSystem.SimulationSpace == NeoAxis.ParticleSystem.SimulationSpaceEnum.Local )
										{
											billboardItem.Size = new Vector2F( particle.Size * Math.Max( (float)tr.Scale.X, (float)tr.Scale.Y ), particle.Size * (float)tr.Scale.Z );
										}
										else
											billboardItem.Size = new Vector2F( particle.Size, particle.Size );

										if( billboardItem.Size.X > 0 && billboardItem.Size.Y > 0 )
										{
											//Rotation
											//!!!!slowly
											billboardItem.RotationAngle = MathEx.DegreeToRadian( particle.Rotation.ToAngles().Roll );

											//Color
											billboardItem.Color = particle.Color * color;
											RenderingPipeline.GetColorForInstancingData( ref billboardItem.Color, out billboardItem.ColorForInstancingData1, out billboardItem.ColorForInstancingData2 );
											//billboardItem.ColorForInstancingData = RenderingPipeline.GetColorForInstancingData( ref billboardItem.Color );

											//PositionPreviousFrame
											if( previousPositions != null && particleIndex < previousPositions.Length )
											{
												var p2 = previousPositions[ particleIndex ];
												if( !double.IsNaN( p2.X ) )
													billboardItem.PreviousFramePositionChange = ( p - p2 ).ToVector3F();
												else
													billboardItem.PreviousFramePositionChange = Vector3F.Zero;
											}
											else
												billboardItem.PreviousFramePositionChange = Vector3F.Zero;


											////PositionPreviousFrame
											//if( previousPositions != null && particleIndex < previousPositions.Length )
											//{
											//	var p2 = previousPositions[ particleIndex ];
											//	//context.ConvertToRelative( ref previousPositions[ particleIndex ], out var p2 );
											//	if( !float.IsNaN( p2.X ) )//if( !double.IsNaN( p2.X ) )
											//		billboardItem.PositionPreviousFrameRelative = p2;
											//	else
											//		billboardItem.PositionPreviousFrameRelative = billboardItem.PositionRelative;
											//}
											//else
											//	billboardItem.PositionPreviousFrameRelative = billboardItem.PositionRelative;


											////PositionPreviousFrame
											//if( previousPositions != null && particleIndex < previousPositions.Length )
											//{
											//	var p2 = previousPositions[ particleIndex ];
											//	if( !double.IsNaN( p2.X ) )
											//	{
											//		zzz;
											//		//!!!!double
											//		billboardItem.PositionPreviousFrame = p2.ToVector3F();
											//	}
											//	else
											//		billboardItem.PositionPreviousFrame = billboardItem.Position;
											//}
											//else
											//	billboardItem.PositionPreviousFrame = billboardItem.Position;

											//add
											context.FrameData.RenderSceneData.Billboards.Add( ref billboardItem );
										}
									}
								}
								else if( renderingMode == ParticleEmitter.RenderingModeEnum.Mesh )
								{
									//Mesh rendering mode

									var mesh = compiledEmitter.Mesh;
									if( mesh == null || mesh.Result == null )
										mesh = ResourceUtility.MeshInvalid;

									meshItem.MeshData = mesh.Result.MeshData;
									meshItem.MeshDataLOD0 = meshItem.MeshData;
									meshItem.CastShadows = compiledEmitter.CastShadows && CastShadows && shadowVisibilityByDistance;
									meshItem.ReceiveDecals = compiledEmitter.ReceiveDecals && ReceiveDecals;
									meshItem.MotionBlurFactor = compiledEmitter.MotionBlurFactor * (float)MotionBlurFactor;
									meshItem.ReplaceMaterial = replaceMaterial ?? compiledEmitter.Material;
									meshItem.Color = particle.Color * color;
									RenderingPipeline.GetColorForInstancingData( ref meshItem.Color, out meshItem.ColorForInstancingData1, out meshItem.ColorForInstancingData2 );
									//meshItem.ColorForInstancingData = RenderingPipeline.GetColorForInstancingData( ref meshItem.Color );

									var lods = meshItem.MeshData.LODs;
									ref var emitter = ref Emitters[ particle.Emitter ];

									int item0BillboardMode = 0;


									if( meshEmitterLodStates == null )
										meshEmitterLodStates = new SceneLODUtility.LodState?[ Emitters.Length ];

									SceneLODUtility.LodState lodState;
									if( meshEmitterLodStates[ particle.Emitter ] != null )
										lodState = meshEmitterLodStates[ particle.Emitter ].Value;
									else
									{
										SceneLODUtility.GetDemandedLODs( context, mesh.Result.MeshData, cameraDistanceMinSquared, cameraDistanceMaxSquared, boundingSize, out lodState );
										meshEmitterLodStates[ particle.Emitter ] = lodState;
									}

									for( int nLodItem = 0; nLodItem < lodState.Count; nLodItem++ )
									{
										lodState.GetItem( nLodItem, out var lodLevel, out var lodRange );

										meshItem.MeshData = mesh.Result.MeshData;
										if( lodLevel > 0 )
										{
											ref var lod = ref lods[ lodLevel - 1 ];
											var lodMeshData = lod.Mesh?.Result?.MeshData;
											if( lodMeshData != null )
											{
												meshItem.MeshData = lodMeshData;
												//meshItem.CastShadows = compiledEmitter.CastShadows && CastShadows && lodMeshData.CastShadows && shadowVisibilityByDistance;
											}
										}

										meshItem.CastShadows = compiledEmitter.CastShadows && CastShadows && meshItem.MeshData.CastShadows && shadowVisibilityByDistance;
										meshItem.LODValue = SceneLODUtility.GetLodValue( context, lodRange, cameraDistanceMin );
										//meshItem.LODRange = lodRange;


										var skipItem = false;

										//calculate MeshInstanceOne
										if( nLodItem == 0 )
											item0BillboardMode = meshItem.MeshData.BillboardMode;
										if( nLodItem == 0 || item0BillboardMode != meshItem.MeshData.BillboardMode )
										{
											if( meshItem.MeshData.BillboardMode != 0 )
											{
												//Position
												Vector3 position;
												if( currentParticleSystem.SimulationSpace == NeoAxis.ParticleSystem.SimulationSpaceEnum.Local )
													Matrix4.Multiply( ref trMatrix, ref particle.Position, out position );
												else
													position = particle.Position;

												//Size
												Vector2F size;
												if( currentParticleSystem.SimulationSpace == NeoAxis.ParticleSystem.SimulationSpaceEnum.Local )
												{
													var scale = tr.Scale;
													var scaleH = (float)Math.Max( scale.X, scale.Y );
													size = new Vector2F( particle.Size * scaleH, particle.Size * (float)scale.Z );
												}
												else
													size = new Vector2F( particle.Size, particle.Size );

												if( size.X > 0 && size.Y > 0 )
												{
													ref var result = ref meshItem.TransformRelative;
													result.Item0.X = size.X;
													result.Item0.Y = 0;
													result.Item0.Z = 0;
													result.Item0.W = 0;
													result.Item1.X = 0;
													result.Item1.Y = size.X;
													result.Item1.Z = 0;
													result.Item1.W = 0;
													result.Item2.X = 0;
													result.Item2.Y = 0;
													result.Item2.Z = size.Y;
													result.Item2.W = 0;
													result.Item3.X = (float)( position.X - context.OwnerCameraSettingsPosition.X );
													result.Item3.Y = (float)( position.Y - context.OwnerCameraSettingsPosition.Y );
													result.Item3.Z = (float)( position.Z - context.OwnerCameraSettingsPosition.Z );
													result.Item3.W = 1;
												}
												else
													skipItem = true;
											}
											else
											{
												//Transform
												Matrix3F.FromScale( particle.Size, out var scl );
												Matrix3F.Multiply( ref particle.Rotation, ref scl, out var mat3 );

												//!!!!good?
												context.ConvertToRelative( ref particle.Position, out var positionF );
												if( currentParticleSystem.SimulationSpace == NeoAxis.ParticleSystem.SimulationSpaceEnum.Local )
												{
													var mat4 = new Matrix4F( ref mat3, ref positionF );
													Matrix4F.Multiply( ref trMatrixF, ref mat4, out meshItem.TransformRelative );
												}
												else
													meshItem.TransformRelative = new Matrix4F( ref mat3, ref positionF );


												//var positionF = particle.Position.ToVector3F();
												//if( currentParticleSystem.SimulationSpace == NeoAxis.ParticleSystem.SimulationSpaceEnum.Local )
												//{
												//	var mat4 = new Matrix4F( ref mat3, ref positionF );
												//	Matrix4F.Multiply( ref trMatrixF, ref mat4, out meshItem.Transform );
												//}
												//else
												//	meshItem.Transform = new Matrix4F( ref mat3, ref positionF );
											}
										}

										//add to render
										if( !skipItem )
										{
											//PositionPreviousFrame
											if( previousPositions != null && particleIndex < previousPositions.Length )
											{
												var p2 = previousPositions[ particleIndex ];
												if( !double.IsNaN( p2.X ) )
												{
													//Position
													Vector3 position;
													if( currentParticleSystem.SimulationSpace == NeoAxis.ParticleSystem.SimulationSpaceEnum.Local )
														Matrix4.Multiply( ref trMatrix, ref particle.Position, out position );
													else
														position = particle.Position;

													meshItem.PreviousFramePositionChange = ( position - p2 ).ToVector3F();
												}
												else
													meshItem.PreviousFramePositionChange = Vector3F.Zero;
											}
											else
												meshItem.PreviousFramePositionChange = Vector3F.Zero;


											////PositionPreviousFrame
											//if( previousPositions != null && particleIndex < previousPositions.Length )
											//{
											//	var p2 = previousPositions[ particleIndex ];
											//	//context.ConvertToRelative( ref previousPositions[ particleIndex ], out var p2 );
											//	if( !float.IsNaN( p2.X ) )//if( !double.IsNaN( p2.X ) )
											//		meshItem.PositionPreviousFrameRelative = p2;
											//	else
											//		meshItem.TransformRelative.GetTranslation( out meshItem.PositionPreviousFrameRelative );
											//}
											//else
											//	meshItem.TransformRelative.GetTranslation( out meshItem.PositionPreviousFrameRelative );


											////PositionPreviousFrame
											//if( previousPositions != null && particleIndex < previousPositions.Length )
											//{
											//	var p2 = previousPositions[ particleIndex ];
											//	if( !double.IsNaN( p2.X ) )
											//	{
											//		zzzzz;
											//		//!!!!double
											//		meshItem.PositionPreviousFrame = p2.ToVector3F();
											//	}
											//	else
											//		meshItem.Transform.GetTranslation( out meshItem.PositionPreviousFrame );
											//}
											//else
											//	meshItem.Transform.GetTranslation( out meshItem.PositionPreviousFrame );


											////layers
											//meshItem.Layers = Layers;

											////set AnimationData from event
											//GetRenderSceneDataAddToFrameData?.Invoke( this, context, mode, ref meshItem );

											//add the item to render
											context.FrameData.RenderSceneData.Meshes.Add( ref meshItem );
										}
									}
								}
							}
						}
					}
				}
			}

			//display emitters
			if( mode == GetRenderSceneDataMode.InsideFrustum && context.SceneDisplayDevelopmentDataInThisApplication && DisplayEmitters )
			{
				//!!!!if( context2.displayLightsCounter < context2.displayLightsMax )

				if( currentParticleSystem != null )
				{
					var viewport = context.Owner;
					var context2 = context.ObjectInSpaceRenderingContext;

					var tr = Transform.Value;
					int verticesRendered = 0;

					foreach( var emitter in currentParticleSystem.Emitters )
					{
						bool emitterSelected = context2.selectedObjects.Contains( emitter.Owner );

						foreach( var shape in emitter.Owner.GetComponents<ParticleEmitterShape>( false, false, false ) )
						{
							if( shape.Enabled )
							{
								ColorValue color;
								if( emitterSelected || context2.selectedObjects.Contains( shape ) )
									color = ProjectSettings.Get.Colors.SelectedColor;
								else
								{
									//!!!!add editor options
									color = new ColorValue( 0, 0, 0.8 );
								}
								viewport.Simple3DRenderer.SetColor( color, color * ProjectSettings.Get.Colors.HiddenByOtherObjectsColorMultiplier );
								shape.PerformRender( viewport, tr, false, ref verticesRendered );
							}
						}
					}
				}
			}
		}

		protected override void OnSpaceBoundsUpdate( ref SpaceBounds newBounds )
		{
			//!!!!slowly?
			//� �������� �������, ���� �������

			if( currentParticleSystem != null )
			{
				var bounds = Bounds.Cleared;

				var tr = Transform.Value;
				ref var trMatrix = ref tr.ToMatrix4();

				foreach( var particleIndex in ObjectsGetAll() )
				{
					ref var particle = ref Objects[ particleIndex ];

					Vector3 p;
					if( currentParticleSystem.SimulationSpace == NeoAxis.ParticleSystem.SimulationSpaceEnum.Local )
						p = trMatrix * particle.Position;
					else
						p = particle.Position;

					var size = new Vector2F( particle.Size, particle.Size );

					Vector2 size2;
					if( currentParticleSystem.SimulationSpace == NeoAxis.ParticleSystem.SimulationSpaceEnum.Local )
						size2 = new Vector2( size.X * Math.Max( tr.Scale.X, tr.Scale.Y ), size.Y * tr.Scale.Z );
					else
						size2 = size;

					if( size2.X > 0 && size2.Y > 0 )
					{
						var halfSize = size2.MaxComponent() * 0.5;
						if( halfSize < 0.001 )
							halfSize = 0.001;

						var b = new Bounds( p.X - halfSize, p.Y - halfSize, p.Z - halfSize, p.X + halfSize, p.Y + halfSize, p.Z + halfSize );
						bounds.Add( ref b );
					}

				}

				if( !bounds.IsCleared() )
					newBounds = new SpaceBounds( bounds );
			}
		}

		protected override void OnEnabledInHierarchyChanged()
		{
			base.OnEnabledInHierarchyChanged();

			if( EnabledInHierarchyAndIsInstance )
				CreateData( false );
			else
				DestroyData();
		}

		protected override void OnUpdate( float delta )
		{
			base.OnUpdate( delta );

			if( EngineApp.IsEditor && currentParticleSystem != null )
			{
				bool wasUpdated = false;

				float remainingDelta = delta;
				if( remainingDelta > 1 )
					remainingDelta = 1;
				while( remainingDelta > 0.0001 )
				{
					float step = Math.Min( remainingDelta, Time.SimulationDelta );
					Simulate( step * currentParticleSystem.Owner.SimulationSpeed, out var wasUpdated2 );
					remainingDelta -= step;

					if( wasUpdated2 )
						wasUpdated = true;
				}

				if( wasUpdated )
					SpaceBoundsUpdate();
			}
		}

		protected override void OnSimulationStep()
		{
			base.OnSimulationStep();

			//!!!!����� �� ���������? �� �������, ����� ������

			if( currentParticleSystem != null )
			{
				mergeSimulationStepsCounter--;
				if( mergeSimulationStepsCounter <= 0 )
				{
					var mergeSteps = MergeSimulationSteps.Value;
					mergeSimulationStepsCounter = mergeSteps;
					Simulate( Time.SimulationDelta * currentParticleSystem.Owner.SimulationSpeed * mergeSteps, out var wasUpdated );
					if( wasUpdated )
						SpaceBoundsUpdate();
				}

				//Simulate( Time.SimulationDelta * currentParticleSystem.Owner.SimulationSpeed, out var wasUpdated );
				//if( wasUpdated )
				//	SpaceBoundsUpdate();
			}
		}

		protected override void OnSimulationStepClient()
		{
			base.OnSimulationStepClient();

			//!!!!����� �� ���������? �� �������, ����� ������

			if( currentParticleSystem != null )
			{
				mergeSimulationStepsCounter--;
				if( mergeSimulationStepsCounter <= 0 )
				{
					var mergeSteps = MergeSimulationSteps.Value;
					mergeSimulationStepsCounter = mergeSteps;
					Simulate( Time.SimulationDelta * currentParticleSystem.Owner.SimulationSpeed * mergeSteps, out var wasUpdated );
					if( wasUpdated )
						SpaceBoundsUpdate();
				}

				//Simulate( Time.SimulationDelta * currentParticleSystem.Owner.SimulationSpeed, out var wasUpdated );
				//if( wasUpdated )
				//	SpaceBoundsUpdate();
			}
		}

		void CreateData( bool canSavePreviousState )
		{
			//check must recreate simulation data
			bool mustRecreate = true;
			{
				if( canSavePreviousState && currentParticleSystem != null && Emitters != null )
				{
					var newParticleSystem = ParticleSystem.Value?.Result;
					if( newParticleSystem != null )
					{
						if( newParticleSystem.Owner.MustRecreateInstancesCounter == currentMustRecreateCounter &&
							newParticleSystem.Looping == currentParticleSystem.Looping &&
							newParticleSystem.SimulationSpace == currentParticleSystem.SimulationSpace &&
							newParticleSystem.Emitters.Length == Emitters.Length )
						{
							//����������� ���������, �.�. ���������� ��������� ������������� ����������

							//bool same = true;

							//for( int n = 0; n < Emitters.Length; n++ )
							//{
							//	var newEmitter = newParticleSystem.Emitters[ n ];
							//	var emitter = Emitters[ n ];

							//	if( !newEmitter.Equals( emitter ) )
							//	{
							//		same = false;
							//		break;
							//	}
							//}

							//if( same )
							mustRecreate = false;
						}
					}
				}
			}

			//destroy old data
			if( mustRecreate )
				DestroyData();

			//update current particle system
			currentParticleSystem = ParticleSystem.Value?.Result;
			if( currentParticleSystem == null )
			{
				DestroyData();
				return;
			}
			currentMustRecreateCounter = currentParticleSystem.Owner.MustRecreateInstancesCounter;

			//create simulation data
			if( !currentParticleSystem.Looping || mustRecreate || playingEnded )
			{
				playingTime = 0;
				playingEnded = false;
			}

			//create emitters
			if( mustRecreate )
				Emitters = new Emitter[ currentParticleSystem.Emitters.Length ];

			//update emitters
			for( int n = 0; n < Emitters.Length; n++ )
			{
				ref var emitter = ref Emitters[ n ];
				var compiledEmitter = currentParticleSystem.Emitters[ n ];

				emitter.StartTime = compiledEmitter.StartTime.GenerateValue( staticRandom );
				emitter.Duration = compiledEmitter.Duration.GenerateValue( staticRandom );
			}
		}

		unsafe void DestroyData()
		{
			BatchDestroy();
			currentParticleSystem = null;
			Emitters = null;
			ObjectsSet( null, 0 );
		}

		public void RecreateData( bool canSavePreviousState )
		{
			if( EnabledInHierarchyAndIsInstance )
			{
				CreateData( canSavePreviousState );
				SpaceBoundsUpdate();
			}
		}

		public void Play()
		{
			RecreateData( true );
		}

		//maybe add GetLinearVelocityByRenderingData()
		bool GetTransformPositionByTime( double time, out Vector3[] position )
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
			position = null;
			return false;
		}

		//public void ResetLodTransitionStates( ViewportRenderingContext resetOnlySpecifiedContext = null )
		//{
		//	SceneLODUtility.ResetLodTransitionStates( ref billboardRenderingContextItems, resetOnlySpecifiedContext );

		//	if( Emitters != null )
		//	{
		//		for( int nEmitter = 0; nEmitter < Emitters.Length; nEmitter++ )
		//		{
		//			ref var emitter = ref Emitters[ nEmitter ];
		//			SceneLODUtility.ResetLodTransitionStates( ref emitter.meshRenderingContextItems, resetOnlySpecifiedContext );
		//		}
		//	}
		//}

		void RemovedObjectsInit()
		{
			if( removedObjects == null )
			{
				int currentCapacity = ObjectsGetCapacity();

				removedObjects = new OpenList<bool>( currentCapacity );
				for( int index = 0; index < currentCapacity; index++ )
					removedObjects.Add( false );

				foreach( var index in freeObjects )
					removedObjects.Data[ index ] = true;
			}
		}

		public unsafe void ObjectsSet( Particle* data, int count )//, bool fullRecreateInternalData )
		{
			//if( fullRecreateInternalData )
			//{

			if( count != 0 )
			{
				var array = new Particle[ count ];
				fixed( Particle* pArray = array )
					NativeUtility.CopyMemory( pArray, data, array.Length * sizeof( Particle ) );
				Objects = array;
			}
			else
				Objects = null;
			removedObjects = null;
			freeObjects.Clear();

			//}
			//else
			//{
			//}
		}

		public unsafe void ObjectsSet( ArraySegment<Particle> data )//, bool fullRecreateInternalData )
		{
			fixed( Particle* pData = data.Array )
				ObjectsSet( pData + data.Offset, data.Count );//, fullRecreateInternalData );
		}

		public unsafe void ObjectsSet( Particle[] data )//, bool fullRecreateInternalData )
		{
			fixed( Particle* pData = data )
				ObjectsSet( pData, data.Length );//, fullRecreateInternalData );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining | (MethodImplOptions)512 )]
		public unsafe void/*int[]*/ ObjectsAdd( Particle* data, int count )
		{
			RemovedObjectsInit();

			//expand capacity
			if( freeObjects.Count < count )
			{
				int currentCapacity = ObjectsGetCapacity();
				int currentCount = ObjectsGetCount();
				int demandCount = currentCount + count;
				int demandCapacity = Math.Max( MathEx.NextPowerOfTwo( demandCount ), currentCapacity );
				if( demandCapacity < 4 ) demandCapacity = 4;

				var newData = new Particle[ demandCapacity ];
				if( Objects != null )
				{
					for( int n = 0; n < currentCapacity; n++ )
						newData[ n ] = Objects[ n ];
				}
				Objects = newData;

				for( int index = demandCapacity - 1; index >= currentCapacity; index-- )
				{
					freeObjects.Push( index );
					removedObjects.Add( true );
				}
			}

			//var addedIndexes = new int[ count ];

			for( int n = 0; n < count; n++ )
			{
				var index = freeObjects.Pop();

				ref var obj = ref Objects[ index ];
				obj = data[ n ];
				//if( obj.UniqueIdentifier == 0 )
				//	obj.UniqueIdentifier = GetUniqueIdentifier();

				removedObjects.Data[ index ] = false;

				//addedIndexes[ n ] = index;
			}

			//return addedIndexes;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining | (MethodImplOptions)512 )]
		public unsafe void/*int[]*/ ObjectsAdd( ArraySegment<Particle> data )
		{
			fixed( Particle* pData = data.Array )
				ObjectsAdd( pData + data.Offset, data.Count );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining | (MethodImplOptions)512 )]
		public unsafe void/*int[]*/ ObjectsAdd( Particle[] data )
		{
			fixed( Particle* pData = data )
				ObjectsAdd( pData, data.Length );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining | (MethodImplOptions)512 )]
		public unsafe void ObjectsRemove( int* indexes, int count )
		{
			RemovedObjectsInit();

			for( int n = count - 1; n >= 0; n-- )
			{
				var index = indexes[ n ];
				freeObjects.Push( index );
				removedObjects.Data[ index ] = true;
			}
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining | (MethodImplOptions)512 )]
		public void ObjectsRemove( List<int> indexes )
		{
			RemovedObjectsInit();

			for( int n = indexes.Count - 1; n >= 0; n-- )
			{
				var index = indexes[ n ];
				freeObjects.Push( index );
				removedObjects.Data[ index ] = true;
			}
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining | (MethodImplOptions)512 )]
		public unsafe void ObjectsRemove( ArraySegment<int> indexes )
		{
			fixed( int* pData = indexes.Array )
				ObjectsRemove( pData + indexes.Offset, indexes.Count );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining | (MethodImplOptions)512 )]
		public unsafe void ObjectsRemove( int[] indexes )
		{
			fixed( int* pData = indexes )
				ObjectsRemove( pData, indexes.Length );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining | (MethodImplOptions)512 )]
		int ObjectsGetCapacity()
		{
			return Objects != null ? Objects.Length : 0;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining | (MethodImplOptions)512 )]
		public int ObjectsGetCount()
		{
			return ObjectsGetCapacity() - freeObjects.Count;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining | (MethodImplOptions)512 )]
		public List<int> ObjectsGetAll()
		{
			int capacity = ObjectsGetCapacity();
			var result = new List<int>( capacity );
			for( int index = 0; index < capacity; index++ )
			{
				//check is removed
				if( removedObjects != null && removedObjects.Data[ index ] )
					continue;
				result.Add( index );
			}
			return result;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining | (MethodImplOptions)512 )]
		public Particle[] ObjectsGetData( IList<int> indexes )
		{
			var result = new Particle[ indexes.Count ];
			if( indexes.Count != 0 )
			{
				int currentResult = 0;
				foreach( var index in indexes )
					result[ currentResult++ ] = Objects[ index ];
			}
			return result;
		}

		//public void ObjectsGetData( int index, out ObjectMesh data )
		//{
		//	data = ObjectsMesh[ index ];
		//}

		[MethodImpl( MethodImplOptions.AggressiveInlining | (MethodImplOptions)512 )]
		public ref Particle ObjectGetData( int index )
		{
			return ref Objects[ index ];
		}

		///// <summary>
		///// Removes objects by their unique identifiers.
		///// </summary>
		///// <param name="data"></param>
		//public int ObjectsRemove( Particle[] data )
		//{
		//	var set = new ESet<long>( data.Length );
		//	for( int n = 0; n < data.Length; n++ )
		//	{
		//		if( data[ n ].UniqueIdentifier == 0 )
		//			Log.Fatal( "GroupOfObjects: ObjectsRemove: data[ n ].UniqueIdentifier == 0." );
		//		set.Add( data[ n ].UniqueIdentifier );
		//	}

		//	var indexesToRemove = new List<int>( data.Length );

		//	foreach( var index in ObjectsGetAll() )
		//	{
		//		ref var obj = ref ObjectGetData( index );
		//		if( set.Contains( obj.UniqueIdentifier ) )
		//			indexesToRemove.Add( index );
		//	}

		//	ObjectsRemove( indexesToRemove.ToArray() );

		//	return indexesToRemove.Count;
		//}

		public bool ObjectsExists()
		{
			return ObjectsGetCount() != 0;
		}

		public void ClearObjects()
		{
			ObjectsSet( new Particle[ 0 ] );//, true );
		}

		/////////////////////////////////////////

		[MethodImpl( (MethodImplOptions)512 )]
		void Simulate( float delta, out bool wasUpdated )
		{
			var wasUpdated2 = false;

			//update playing time
			if( !playingEnded )
			{
				if( Activated )
					playingTime += delta;

				//check for replay
				{
					bool replay = true;
					for( int nEmitter = 0; nEmitter < Emitters.Length; nEmitter++ )
					{
						ref var emitter = ref Emitters[ nEmitter ];
						if( playingTime < emitter.StartTime + emitter.Duration )
						{
							replay = false;
							break;
						}
					}

					if( replay )
					{
						playingTime = 0;
						if( !currentParticleSystem.Looping || !Activated )
							playingEnded = true;

						//update emitters
						if( !playingEnded && Emitters.Length == currentParticleSystem.Emitters.Length )
						{
							for( int n = 0; n < Emitters.Length; n++ )
							{
								ref var emitter = ref Emitters[ n ];
								var compiledEmitter = currentParticleSystem.Emitters[ n ];

								emitter.StartTime = compiledEmitter.StartTime.GenerateValue( staticRandom );
								emitter.Duration = compiledEmitter.Duration.GenerateValue( staticRandom );
							}
						}
					}
				}
			}

			double[] tempDoubleArray = null;
			double[] GetTempDoubleArray( int minSize )
			{
				if( tempDoubleArray == null || tempDoubleArray.Length < minSize )
					tempDoubleArray = new double[ minSize ];
				return tempDoubleArray;
			}

			//emit particles
			if( !playingEnded && Activated )
			{
				//!!!!slowly
				var tr = Transform.Value;
				ref var trMatrix = ref tr.ToMatrix4();
				trMatrix.ToMatrix3( out var trMatrix3 );
				trMatrix3.ToMatrix3F( out var trMatrix3F );
				var trScale = tr.Scale;

				for( int nEmitter = 0; nEmitter < Emitters.Length; nEmitter++ )
				{
					ref var emitter = ref Emitters[ nEmitter ];
					var compiledEmitter = currentParticleSystem.Emitters[ nEmitter ];

					if( playingTime >= emitter.StartTime && playingTime < emitter.StartTime + emitter.Duration )
						emitter.AvailableTimeToSpawn += delta;

					//!!!!����� �� �������

					while( emitter.AvailableTimeToSpawn > 0 )
					{
						//calculate spawn time
						float spawnTime = 0;
						{
							var spawnRate = Math.Abs( compiledEmitter.SpawnRate.GenerateValue( staticRandom ) );
							if( spawnRate != 0 )
								spawnTime = 1.0f / spawnRate;
						}

						if( spawnTime <= 0 )
							break;

						//update available time to spawn
						emitter.AvailableTimeToSpawn -= spawnTime;

						//!!!!multithreading

						//emit
						var spawnCount = compiledEmitter.SpawnCount.GenerateValue( staticRandom );
						for( int nSpawn = 0; nSpawn < spawnCount; nSpawn++ )
						{
							if( ObjectsGetCount() < currentParticleSystem.Owner.MaxParticles )
							{
								var particle = new Particle();

								particle.Emitter = nEmitter;
								particle.Lifetime = compiledEmitter.Lifetime.GenerateValue( staticRandom );

								particle.StartSize = compiledEmitter.Size.GenerateValue( staticRandom );
								if( currentParticleSystem.SimulationSpace == NeoAxis.ParticleSystem.SimulationSpaceEnum.World )
									particle.StartSize *= (float)trScale.MaxComponent();
								particle.Size = particle.StartSize;

								particle.GravityMultiplier = compiledEmitter.GravityMultiplier.GenerateValue( staticRandom );

								//get shape
								var shapes = compiledEmitter.Shapes;
								if( shapes.Length > 0 )
								{
									int shapeIndex;
									if( shapes.Length > 1 )
									{
										//unsafe
										//{
										var groupProbabilities = GetTempDoubleArray( shapes.Length );
										//var groupProbabilities = stackalloc double[ shapes.Length ];
										for( int n = 0; n < shapes.Length; n++ )
											groupProbabilities[ n ] = shapes[ n ].Probability;
										shapeIndex = RandomUtility.GetRandomIndexByProbabilities( staticRandom, new ArraySegment<double>( groupProbabilities, 0, shapes.Length ) );
										//}
									}
									else
										shapeIndex = 0;

									var shape = shapes[ shapeIndex ];
									var shapeTransform = shape.Transform.Value;

									Vector3 position;
									{
										shape.PerformGetLocalPosition( staticRandom, out var localPosition );
										if( !shapeTransform.IsIdentity )
											Matrix4.Multiply( ref shapeTransform.ToMatrix4(), ref localPosition, out position );
										else
											position = localPosition;
									}

									//Position
									if( currentParticleSystem.SimulationSpace == NeoAxis.ParticleSystem.SimulationSpaceEnum.World )
										Matrix4.Multiply( ref trMatrix, ref position, out particle.Position );
									else
										particle.Position = position;

									//if( currentParticleSystem.SimulationSpace == ParticleSystem.SimulationSpaceEnum.World )
									//	particle.Scale = Vector3F.One * (float)trScale.MaxComponent();
									//else
									//particle.Scale = Vector3F.One;

									//LinearVelocity
									{
										Vector3F direction;
										if( compiledEmitter.Direction == ParticleEmitter.DirectionEnum.EmitterDirection )
											direction = shapeTransform.Rotation.GetForward().ToVector3F();
										else //if( compiledEmitter.Direction == ParticleEmitter.DirectionEnum.FromCenterOfEmitter )
										{
											var d = position - shapeTransform.Position;
											while( d == Vector3.Zero )
												d = new SphericalDirection( staticRandom.Next( Math.PI * 2 ), staticRandom.Next( -Math.PI / 2, Math.PI / 2 ) ).GetVector();
											direction = d.ToVector3F().GetNormalize();
										}

										var dispersionAngle = compiledEmitter.DispersionAngle.GenerateValue( staticRandom );
										if( dispersionAngle != 0 )
										{
											var axisAngle = staticRandom.Next( MathEx.PI * 2 );

											//!!!!slowly

											var r = QuaternionF.FromDirectionZAxisUp( direction );

											r *= QuaternionF.FromRotateByX( axisAngle ) * QuaternionF.FromRotateByY( MathEx.DegreeToRadian( dispersionAngle ) );

											direction = r.GetForward();
										}

										var speed = compiledEmitter.Speed.GenerateValue( staticRandom );

										if( currentParticleSystem.SimulationSpace == NeoAxis.ParticleSystem.SimulationSpaceEnum.World )
											particle.LinearVelocity = trMatrix3F * ( direction.GetNormalize() * speed );
										else
											particle.LinearVelocity = direction.GetNormalize() * speed;
									}

									//Rotation
									{
										if( compiledEmitter.Owner.RenderingMode.Value == ParticleEmitter.RenderingModeEnum.Mesh )
										{
											if( compiledEmitter.RotateAlongMovement && particle.LinearVelocity != Vector3F.Zero )
											{
												//!!!!slowly
												QuaternionF.FromDirectionZAxisUp( ref particle.LinearVelocity, out var q );
												q.ToMatrix3( out particle.Rotation );
											}
											else
											{
												if( currentParticleSystem.SimulationSpace == NeoAxis.ParticleSystem.SimulationSpaceEnum.World )
													particle.Rotation = trMatrix3F;
												else
													particle.Rotation = Matrix3F.Identity;
											}
										}
										else
											particle.Rotation = Matrix3F.Identity;

										{
											var rotation = compiledEmitter.Rotation.GenerateValue( staticRandom );
											if( rotation.X != 0 )
											{
												Matrix3F.FromRotateByX( MathEx.DegreeToRadian( rotation.X ), out var m );
												Matrix3F.Multiply( ref particle.Rotation, ref m, out var m2 );
												particle.Rotation = m2;
											}
											if( rotation.Y != 0 )
											{
												Matrix3F.FromRotateByY( MathEx.DegreeToRadian( rotation.Y ), out var m );
												Matrix3F.Multiply( ref particle.Rotation, ref m, out var m2 );
												particle.Rotation = m2;
											}
											if( rotation.Z != 0 )
											{
												Matrix3F.FromRotateByZ( MathEx.DegreeToRadian( rotation.Z ), out var m );
												Matrix3F.Multiply( ref particle.Rotation, ref m, out var m2 );
												particle.Rotation = m2;
											}
										}
									}

									//AngularVelocity
									particle.AngularVelocity = compiledEmitter.AngularVelocity.GenerateValue( staticRandom );

									//Color
									compiledEmitter.Color.GenerateValue( staticRandom, out particle.StartColor );
									particle.Color = particle.StartColor;

									//add
									unsafe
									{
										ObjectsAdd( &particle, 1 );
									}

									wasUpdated2 = true;
								}
							}
						}
					}
				}
			}

			//update particles
			if( Objects != null )
			{
				var scene = ParentScene;
				var sceneGravity = Vector3F.Zero;
				if( scene != null )
					sceneGravity = scene.Gravity.Value.ToVector3F();

				//!!!!GC
				List<int> toDelete = new List<int>( 32 );

				//!!!!slowly? ObjectsGetAll
				var allParticles = ObjectsGetAll();

				//foreach( var particleIndex in allParticles )
				//{
				Parallel.For( 0, allParticles.Count, delegate ( int parallelIndex )
				{
					var particleIndex = allParticles[ parallelIndex ];

					ref var particle = ref Objects[ particleIndex ];

					if( particle.Emitter < currentParticleSystem.Emitters.Length )
					{
						var emitter = currentParticleSystem.Emitters[ particle.Emitter ];

						//custom modules
						{
							var items = emitter.CustomModules;
							for( int n = 0; n < items.Length; n++ )
								items[ n ].PerformUpdateBefore( this, delta, ref particle );
						}

						particle.Time += delta;
						if( particle.Time >= particle.Lifetime )
						{
							//delete
							//if( toDelete == null )
							//	toDelete = new List<int>( 32 );
							lock( toDelete )
								toDelete.Add( particleIndex );

							wasUpdated2 = true;
						}
						else
						{
							//update linear, angular velocity
							{
								//LinearVelocityByTime
								{
									var items = emitter.LinearVelocityByTimes;
									for( int n = 0; n < items.Length; n++ )
									{
										ref var item = ref items[ n ];
										item.Calculate( particle.Lifetime, particle.Time, out var value );
										particle.LinearVelocity = value;
									}
								}

								//AngularVelocityByTime
								{
									var items = emitter.AngularVelocityByTimes;
									for( int n = 0; n < items.Length; n++ )
									{
										ref var item = ref items[ n ];
										item.Calculate( particle.Lifetime, particle.Time, out var value );
										particle.AngularVelocity = value;
									}
								}

								//GravityMultiplier
								if( particle.GravityMultiplier != 0 && scene != null )
									particle.LinearVelocity += sceneGravity * particle.GravityMultiplier * delta;

								//LinearAccelerationByTime
								{
									var items = emitter.LinearAccelerationByTimes;
									for( int n = 0; n < items.Length; n++ )
									{
										ref var item = ref items[ n ];
										item.Calculate( particle.Lifetime, particle.Time, out var value );
										particle.LinearVelocity += value * delta;
									}
								}

								//AngularAccelerationByTime
								{
									var items = emitter.AngularAccelerationByTimes;
									for( int n = 0; n < items.Length; n++ )
									{
										ref var item = ref items[ n ];
										item.Calculate( particle.Lifetime, particle.Time, out var value );
										particle.AngularVelocity += value * delta;
									}
								}
							}

							//update position
							{
								float linearSpeedMultiplier = 1;

								//LinearSpeedMultiplierByTime
								{
									var items = emitter.LinearSpeedMultiplierByTimes;
									for( int n = 0; n < items.Length; n++ )
									{
										ref var item = ref items[ n ];
										linearSpeedMultiplier *= item.Calculate( particle.Lifetime, particle.Time );
									}
								}

								particle.Position += particle.LinearVelocity * delta * linearSpeedMultiplier;
							}

							//update rotation
							{
								float angularSpeedMultiplier = 1;

								//AngularSpeedMultiplierByTime
								{
									var items = emitter.AngularSpeedMultiplierByTimes;
									for( int n = 0; n < items.Length; n++ )
									{
										ref var item = ref items[ n ];
										angularSpeedMultiplier *= item.Calculate( particle.Lifetime, particle.Time );
									}
								}

								if( angularSpeedMultiplier != 0 && particle.AngularVelocity != Vector3F.Zero )
								{
									var multiplier = MathEx.DegreeToRadian( delta * angularSpeedMultiplier );

									if( particle.AngularVelocity.X != 0 )
									{
										var v = particle.AngularVelocity.X * multiplier;
										Matrix3F.FromRotateByX( v, out var m );
										Matrix3F.Multiply( ref particle.Rotation, ref m, out var m2 );
										particle.Rotation = m2;
									}
									if( particle.AngularVelocity.Y != 0 )
									{
										var v = particle.AngularVelocity.Y * multiplier;
										Matrix3F.FromRotateByY( v, out var m );
										Matrix3F.Multiply( ref particle.Rotation, ref m, out var m2 );
										particle.Rotation = m2;
									}
									if( particle.AngularVelocity.Z != 0 )
									{
										var v = particle.AngularVelocity.Z * multiplier;
										Matrix3F.FromRotateByZ( v, out var m );
										Matrix3F.Multiply( ref particle.Rotation, ref m, out var m2 );
										particle.Rotation = m2;
									}
								}
							}

							//update size, color
							{
								particle.Size = particle.StartSize;
								particle.Color = particle.StartColor;

								//SizeMultiplierByTime
								{
									var items = emitter.SizeMultiplierByTimes;
									for( int n = 0; n < items.Length; n++ )
									{
										ref var item = ref items[ n ];
										particle.Size *= item.Calculate( particle.Lifetime, particle.Time );
									}
								}

								//ColorMultiplierByTime
								{
									var items = emitter.ColorMultiplierByTimes;
									for( int n = 0; n < items.Length; n++ )
									{
										ref var item = ref items[ n ];
										item.Calculate( particle.Lifetime, particle.Time, out var color );
										particle.Color *= color;
									}
								}
							}

							//custom modules
							{
								var items = emitter.CustomModules;
								for( int n = 0; n < items.Length; n++ )
									items[ n ].PerformUpdateAfter( this, delta, ref particle );
							}

							wasUpdated2 = true;
						}

					}
				} );
				//}

				if( toDelete != null )
					ObjectsRemove( toDelete );
			}

			wasUpdated = wasUpdated2;
		}

		//!!!!��������
		void BatchDestroy()
		{
			//if( batch != null )
			//{
			//	batch.BatchingInstanceBufferMesh?.Dispose();
			//	batch.BatchingInstanceBufferMesh = null;
			//	batch.BatchingInstanceBufferBillboard?.Dispose();
			//	batch.BatchingInstanceBufferBillboard = null;
			//	batch = null;
			//}
		}

		public override ScreenLabelInfo GetScreenLabelInfo()
		{
			return new ScreenLabelInfo( "ParticleSystemInSpace" );
		}
	}
}
