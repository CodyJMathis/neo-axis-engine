// Copyright (C) NeoAxis Group Ltd. 8 Copthall, Roseau Valley, 00152 Commonwealth of Dominica.
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeoAxis
{
	public partial class RenderingPipeline_Basic
	{
		void DisplayObjectInSpaceBounds( ViewportRenderingContext context, FrameData frameData )
		{
			var context2 = context.ObjectInSpaceRenderingContext;
			var viewport = context.Owner;
			var scene = viewport.AttachedScene;

			var objects = new List<(ObjectInSpace, float)>( frameData.ObjectInSpaces.Count );
			for( int n = 0; n < frameData.ObjectInSpaces.Count; n++ )
			{
				ref var data = ref frameData.ObjectInSpaces.Data[ n ];

				if( data.InsideFrustum )
				{
					var obj = data.ObjectInSpace;
					var center = obj.SpaceBounds.BoundingBox.GetCenter();
					var distanceSquared = ( center - viewport.CameraSettings.Position ).LengthSquared();
					//var distanceSquared = ( obj.TransformV.Position - viewport.CameraSettings.Position ).LengthSquared();
					objects.Add( (obj, (float)distanceSquared) );
				}
			}

			CollectionUtility.MergeSort( objects, delegate ( (ObjectInSpace, float) item1, (ObjectInSpace, float) item2 )
			{
				var distanceSquared1 = item1.Item2;
				var distanceSquared2 = item2.Item2;
				if( distanceSquared1 < distanceSquared2 )
					return -1;
				if( distanceSquared1 > distanceSquared2 )
					return 1;
				return 0;
			}, true );

			int counter = 0;

			foreach( var item in objects )
			{
				var obj = item.Item1;

				ColorValue color = ProjectSettings.Get.Colors.SceneShowObjectInSpaceBoundsColor;
				viewport.Simple3DRenderer.SetColor( color, color * ProjectSettings.Get.Colors.HiddenByOtherObjectsColorMultiplier );

				var bounds = obj.SpaceBoundsOctreeOverride.HasValue ? obj.SpaceBoundsOctreeOverride.Value : obj.SpaceBounds.BoundingBox;

				double lineThickness = 0;
				//precalculate line thickness
				if( bounds.GetSize().MaxComponent() < 10 )
					lineThickness = viewport.Simple3DRenderer.GetThicknessByPixelSize( bounds.GetCenter(), ProjectSettings.Get.Rendering.LineThickness );

				viewport.Simple3DRenderer.AddBounds( bounds, false, lineThickness );

				counter++;
				if( counter >= context2.displayObjectInSpaceBoundsMax )
					break;
			}
		}

		void DisplayPhysicalObjects( ViewportRenderingContext context, FrameData frameData )
		{
			var scene = context.Owner.AttachedScene;
			var viewport = context.Owner;
			var context2 = context.ObjectInSpaceRenderingContext;

			if( ( scene.GetDisplayDevelopmentDataInThisApplication() && scene.DisplayPhysicalObjects ) )//|| context2.selectedObjects.Count != 0 || context2.canSelectObjects.Count != 0 || context2.objectToCreate != null )
			{
				//3D physics
				if( scene.PhysicsWorld != null )
				{

					//!!!!��������� ������ �� ��� �� ��������?


					var objects = new List<(object, float)>( scene.PhysicsWorld.allRigidBodies.Count );
					{
						foreach( var body in scene.PhysicsWorld.allRigidBodies.Values )
						{
							body.GetBounds( out var bounds );
							var center = bounds.GetCenter();

							var distanceSquared = ( center - viewport.CameraSettings.Position ).LengthSquared();
							objects.Add( (body, (float)distanceSquared) );
						}

						foreach( var c in scene.PhysicsWorld.allConstraints )
						{
							var distanceSquared = ( c.Position - viewport.CameraSettings.Position ).LengthSquared();
							objects.Add( (c, (float)distanceSquared) );
						}
					}

					CollectionUtility.MergeSort( objects, delegate ( (object, float) item1, (object, float) item2 )
					{
						var distanceSquared1 = item1.Item2;
						var distanceSquared2 = item2.Item2;
						if( distanceSquared1 < distanceSquared2 )
							return -1;
						if( distanceSquared1 > distanceSquared2 )
							return 1;
						return 0;
					}, true );

					int counterCount = 0;
					int counterVertices = 0;

					foreach( var item in objects )
					{
						var physicalObject = item.Item1;

						var body = physicalObject as Scene.PhysicsWorldClass.Body;
						if( body != null )
						{
							var owner = body.Owner;

							//skip RigidBody because rendering doing from the component
							if( owner as RigidBody != null )
								continue;

							int verticesRendered = 0;
							body.RenderPhysicalObject( context, ref verticesRendered );
							counterCount++;
							counterVertices += verticesRendered;

							if( counterCount >= context2.displayPhysicalObjectsMaxCount )
								break;
							if( counterVertices >= context2.displayPhysicalObjectsMaxVertices )
								break;
						}

						var c = physicalObject as Scene.PhysicsWorldClass.Constraint;
						if( c != null )
						{
							var owner = c.Owner;

							//skip Constraint because rendering doing from the component
							if( owner as Constraint_SixDOF != null )
								continue;

							int verticesRendered = 0;
							c.RenderPhysicalObject( context, ref verticesRendered );
							counterCount++;
							counterVertices += verticesRendered;

							if( counterCount >= context2.displayPhysicalObjectsMaxCount )
								break;
							if( counterVertices >= context2.displayPhysicalObjectsMaxVertices )
								break;
						}
					}


					//var bodies = new List<(Scene.PhysicsWorldClass.Body, float)>( scene.PhysicsWorld.allRigidBodies.Count );
					//foreach( var body in scene.PhysicsWorld.allRigidBodies.Values )
					//{
					//	body.GetBounds( out var bounds );
					//	var center = bounds.GetCenter();

					//	var distanceSquared = ( center - viewport.CameraSettings.Position ).LengthSquared();
					//	bodies.Add( (body, (float)distanceSquared) );
					//}

					//CollectionUtility.MergeSort( bodies, delegate ( (Scene.PhysicsWorldClass.Body, float) item1, (Scene.PhysicsWorldClass.Body, float) item2 )
					//{
					//	var distanceSquared1 = item1.Item2;
					//	var distanceSquared2 = item2.Item2;
					//	if( distanceSquared1 < distanceSquared2 )
					//		return -1;
					//	if( distanceSquared1 > distanceSquared2 )
					//		return 1;
					//	return 0;
					//}, true );

					//int counterCount = 0;
					//int counterVertices = 0;

					//foreach( var item in bodies )
					//{
					//	var body = item.Item1;
					//	var obj = body.Owner;

					//	//skip RigidBody because rendering doing from the component
					//	if( obj as RigidBody != null )
					//		continue;

					//	//bool show = ( context.SceneDisplayDevelopmentDataInThisApplication && scene.DisplayPhysicalObjects ) || ( obj != null && ( context2.selectedObjects.Contains( obj ) || context2.canSelectObjects.Contains( obj ) || context2.objectToCreate == obj ) );
					//	//if( show )
					//	//{

					//	int verticesRendered = 0;

					//	//var rigidBody = obj as RigidBody;
					//	//if( rigidBody != null )
					//	//	rigidBody.RenderPhysicalObject( context, out verticesRendered );
					//	//else
					//	body.RenderPhysicalObject( context, ref verticesRendered );

					//	counterCount++;
					//	counterVertices += verticesRendered;

					//	if( counterCount >= context2.displayPhysicalObjectsMaxCount )
					//		break;
					//	if( counterVertices >= context2.displayPhysicalObjectsMaxVertices )
					//		break;

					//	//}
					//}
				}


				//2D physics
				var physicsWorld2D = scene.Physics2DGetWorld( false );
				if( physicsWorld2D != null )
				{
					int counterCount = 0;
					int counterVertices = 0;

					foreach( var body in physicsWorld2D.BodyList )
					{
						var body2 = body.Tag as RigidBody2D;
						if( body2 != null )
						{
							int verticesRendered;
							body2.RenderPhysicalObject( context, out verticesRendered );

							counterCount++;
							counterVertices += verticesRendered;

							if( counterCount >= context2.displayPhysicalObjectsMaxCount )
								break;
							if( counterVertices >= context2.displayPhysicalObjectsMaxVertices )
								break;
						}
					}

					foreach( var joint in physicsWorld2D.JointList )
					{
						var constraint = joint.Tag as Constraint2D;
						if( constraint != null )
						{
							int verticesRendered;
							constraint.RenderPhysicalObject( context, out verticesRendered );

							counterCount++;
							counterVertices += verticesRendered;

							if( counterCount >= context2.displayPhysicalObjectsMaxCount )
								break;
							if( counterVertices >= context2.displayPhysicalObjectsMaxVertices )
								break;
						}
					}
				}
			}


			//if( ( scene.GetDisplayDevelopmentDataInThisApplication() && scene.DisplayPhysicalObjects ) ||
			//	context2.selectedObjects.Count != 0 || context2.canSelectObjects.Count != 0 || context2.objectToCreate != null )
			//{
			//	var objects = new List<(IPhysicalObject, float)>( frameData.ObjectInSpaces.Count );
			//	for( int n = 0; n < frameData.ObjectInSpaces.Count; n++ )
			//	{
			//		ref var data = ref frameData.ObjectInSpaces.Data[ n ];

			//		if( data.InsideFrustum )
			//		{
			//			var obj = data.ObjectInSpace;

			//			//IPhysicalObject
			//			if( obj is IPhysicalObject physicalObject )
			//			{
			//				var center = obj.SpaceBounds.CalculatedBoundingBox.GetCenter();
			//				var distanceSquared = ( center - viewport.CameraSettings.Position ).LengthSquared();
			//				//var distanceSquared = ( obj.TransformV.Position - viewport.CameraSettings.Position ).LengthSquared();
			//				objects.Add( (physicalObject, (float)distanceSquared) );
			//			}

			//			//GroupOfObjects
			//			{
			//				var sector = obj.AnyData as GroupOfObjects.Sector;
			//				if( sector != null && sector.CollisionBodies.Count != 0 )
			//				{
			//					foreach( var rigidBodyItem in sector.CollisionBodies )
			//					{
			//						rigidBodyItem.GetBounds( out var bounds );
			//						var center = bounds.GetCenter();
			//						var distanceSquared = ( center - viewport.CameraSettings.Position ).LengthSquared();
			//						objects.Add( (rigidBodyItem, (float)distanceSquared) );
			//					}
			//				}
			//			}

			//			//Terrain
			//			{
			//				var tile = obj.AnyData as Terrain.Tile;
			//				if( tile != null && tile.SurfaceObjectsCollisionBodies.Count != 0 )
			//				{
			//					foreach( var rigidBodyItem in tile.SurfaceObjectsCollisionBodies )
			//					{
			//						rigidBodyItem.GetBounds( out var bounds );
			//						var center = bounds.GetCenter();
			//						var distanceSquared = ( center - viewport.CameraSettings.Position ).LengthSquared();
			//						objects.Add( (rigidBodyItem, (float)distanceSquared) );
			//					}
			//				}
			//			}
			//		}
			//	}

			//	CollectionUtility.MergeSort( objects, delegate ( (IPhysicalObject, float) item1, (IPhysicalObject, float) item2 )
			//	{
			//		var distanceSquared1 = item1.Item2;
			//		var distanceSquared2 = item2.Item2;
			//		if( distanceSquared1 < distanceSquared2 )
			//			return -1;
			//		if( distanceSquared1 > distanceSquared2 )
			//			return 1;
			//		return 0;
			//	}, true );

			//	int counterCount = 0;
			//	int counterVertices = 0;

			//	foreach( var item in objects )
			//	{
			//		var obj = item.Item1;

			//		bool show = ( context.SceneDisplayDevelopmentDataInThisApplication && scene.DisplayPhysicalObjects ) ||
			//			context2.selectedObjects.Contains( obj ) || context2.canSelectObjects.Contains( obj ) || context2.objectToCreate == obj;
			//		if( show )
			//		{
			//			int verticesRendered;

			//			var rigidBodyItem = obj as Scene.PhysicsWorldClass.Body;
			//			if( rigidBodyItem != null )
			//				rigidBodyItem.RenderPhysicalObject( context, out verticesRendered );
			//			else
			//				obj.RenderPhysicalObject( context, out verticesRendered );

			//			counterCount++;
			//			counterVertices += verticesRendered;

			//			if( counterCount >= context2.displayPhysicalObjectsMaxCount )
			//				break;
			//			if( counterVertices >= context2.displayPhysicalObjectsMaxVertices )
			//				break;
			//		}
			//	}
			//}
		}

		void SortObjectInSpaceLabels( ViewportRenderingContext context )
		{
			var array = context.Owner.LastFrameScreenLabels.ToArray();

			CollectionUtility.MergeSort( array, delegate ( Viewport.LastFrameScreenLabelItem item1, Viewport.LastFrameScreenLabelItem item2 )
			{
				if( item1.DistanceToCamera > item2.DistanceToCamera )
					return -1;
				if( item1.DistanceToCamera < item2.DistanceToCamera )
					return 1;
				return 0;
			}, true );

			context.Owner.LastFrameScreenLabels.Clear();
			foreach( var item in array )
				context.Owner.LastFrameScreenLabels.AddLast( item );
		}

		void DisplayObjectInSpaceLabels( ViewportRenderingContext context )
		{
			var viewport = context.Owner;
			//var context2 = context.objectInSpaceRenderingContext;

			var currentImage = "Default";
			var triangles = new List<CanvasRenderer.TriangleVertex>( context.Owner.LastFrameScreenLabels.Count * 6 );

			var maxSize = ProjectSettings.Get.SceneEditor.ScreenLabelMaxSize.Value;
			var add = "";
			if( maxSize <= 24 )
				add = @"24\";
			else if( maxSize <= 32 )
				add = @"32\";

			//calculate screen rectangle for display in corner item
			{
				var list = new List<Viewport.LastFrameScreenLabelItem>();
				foreach( var item in viewport.LastFrameScreenLabels )
				{
					if( item.DistanceToCamera < 0 )
						list.Add( item );
				}

				Vector2 sizeInPixels = new Vector2( maxSize, maxSize );
				Vector2 screenSize = sizeInPixels / viewport.SizeInPixels.ToVector2();

				double pos = 1.0 - screenSize.X * 0.25;

				for( int n = list.Count - 1; n >= 0; n-- )
				{
					var item = list[ n ];
					//var obj = item.Object;

					var rect = new Rectangle( pos - screenSize.X, screenSize.Y * 0.25, pos, screenSize.Y * 1.25 ).ToRectangleF();

					pos -= screenSize.X * 1.25;

					//ColorValue color;
					//if( context2.selectedObjects.Contains( obj ) )
					//	color = ProjectSettings.Get.SelectedColor;
					//else if( context2.canSelectObjects.Contains( obj ) )
					//	color = ProjectSettings.Get.CanSelectColor;
					//else
					//	color = ProjectSettings.Get.ScreenLabelColor;

					//var item = new Viewport.LastFrameScreenLabelItem();
					//item.Object = obj;
					//zzzz;
					//item.DistanceToCamera = -1;
					item.ScreenRectangle = rect;
					//item.Color = color;
					//if( !obj.EnabledInHierarchy )
					//	item.Color.Alpha *= 0.5f;

					//item.AlwaysVisible = true;
					//viewport.LastFrameScreenLabels.Add( item );
					//viewport.LastFrameScreenLabelByObjectInSpace[ obj ] = item;


					//screenPositionY = item.ScreenRectangle.Bottom;
				}
			}

			foreach( var label in viewport.LastFrameScreenLabels )
			{
				if( label.Color.Alpha > 0 )
				{
					string imageName = "Default";
					if( ProjectSettings.Get.SceneEditor.ScreenLabelDisplayIcons && label.Object != null )
					{
						var name = label.Object.GetScreenLabelInfo().LabelName;
						if( !string.IsNullOrEmpty( name ) )
							imageName = name;
					}

					if( imageName != currentImage && triangles.Count != 0 )
					{
						var texture = ResourceManager.LoadResource<ImageComponent>( @"Base\UI\Images\Labels\" + add + currentImage + ".png" );
						viewport.CanvasRenderer.AddTriangles( triangles, texture, true );

						triangles.Clear();
					}

					currentImage = imageName;

					var rect = label.ScreenRectangle.ToRectangleF();

					var v0 = new CanvasRenderer.TriangleVertex( rect.LeftTop, label.Color, new Vector2F( 0, 0 ) );
					var v1 = new CanvasRenderer.TriangleVertex( rect.RightTop, label.Color, new Vector2F( 1, 0 ) );
					var v2 = new CanvasRenderer.TriangleVertex( rect.RightBottom, label.Color, new Vector2F( 1, 1 ) );
					var v3 = new CanvasRenderer.TriangleVertex( rect.LeftBottom, label.Color, new Vector2F( 0, 1 ) );

					triangles.Add( v0 );
					triangles.Add( v1 );
					triangles.Add( v2 );
					triangles.Add( v2 );
					triangles.Add( v3 );
					triangles.Add( v0 );
				}
			}

			if( triangles.Count != 0 )
			{
				var texture = ResourceManager.LoadResource<ImageComponent>( @"Base\UI\Images\Labels\" + add + currentImage + ".png" );
				viewport.CanvasRenderer.AddTriangles( triangles, texture, true );
			}
		}

		//!!!!

		////!!!!������� ��� ������������ RenderingMethod. ��������� �������
		////!!!!������� ��� ���������� �������. ��� ���
		////!!!!������� ��� �������� ���������

		//class PhysicallyCorrectRenderingData
		//{
		//	public ImageComponent cameraDistancesTexture;
		//	public ImageComponent cameraDistancesTextureRead;
		//	public IntPtr depthData;
		//	public int depthDataDemandedFrame;
		//	public bool depthDataDone;

		//	//!!!!temp deferred?
		//	public ImageComponent deferredLightingTexture;

		//	//!!!!

		//	public void Clear()
		//	{
		//		//!!!!

		//		cameraDistancesTexture?.Dispose();
		//		cameraDistancesTexture = null;
		//		if( depthData != IntPtr.Zero )
		//		{
		//			NativeUtility.Free( depthData );
		//			depthData = IntPtr.Zero;
		//		}
		//		depthDataDemandedFrame = 0;
		//		depthDataDone = false;

		//		deferredLightingTexture?.Dispose();
		//		deferredLightingTexture = null;
		//	}
		//}
		//PhysicallyCorrectRenderingData physicallyCorrectRenderingData;

		//public override void PhysicallyCorrectRendering_ResetFrame()
		//{
		//	physicallyCorrectRenderingData?.Clear();
		//}

		//void PhysicallyCorrectRendering_CallSaveDepthBufferToTexture( ViewportRenderingContext context )
		//{
		//	if( RenderingMethod.Value != RenderingMethodEnum.PhysicallyCorrect )
		//		return;

		//	if( physicallyCorrectRenderingData == null )
		//		physicallyCorrectRenderingData = new PhysicallyCorrectRenderingData();

		//	var owner = context.Owner;

		//	//!!!!double
		//	Matrix4F viewMatrix = owner.CameraSettings.ViewMatrix.ToMatrix4F();
		//	Matrix4F projectionMatrix = owner.CameraSettings.ProjectionMatrix.ToMatrix4F();

		//	context.objectsDuringUpdate.namedTextures.TryGetValue( "depthTexture", out var depthTexture );
		//	if( depthTexture == null )
		//	{
		//		//!!!!
		//		return;
		//	}

		//	if( physicallyCorrectRenderingData.cameraDistancesTexture == null )
		//	{
		//		var resolution = context.owner.SizeInPixels;
		//		var format = PixelFormat.Float32R;

		//		var texture = ComponentUtility.CreateComponent<ImageComponent>( null, true, false );
		//		physicallyCorrectRenderingData.cameraDistancesTexture = texture;
		//		texture.CreateType = ImageComponent.TypeEnum._2D;
		//		texture.CreateSize = resolution;
		//		texture.CreateMipmaps = false;
		//		texture.CreateFormat = format;
		//		texture.CreateUsage = ImageComponent.Usages.RenderTarget;
		//		texture.CreateFSAA = 0;
		//		texture.Enabled = true;

		//		var renderTexture = texture.Result.GetRenderTarget();
		//		var viewport = renderTexture.AddViewport( false, true );
		//		//!!!!
		//		viewport.RenderingPipelineCreate();
		//		viewport.RenderingPipelineCreated.UseRenderTargets = false;

		//		var textureRead = ComponentUtility.CreateComponent<ImageComponent>( null, true, false );
		//		physicallyCorrectRenderingData.cameraDistancesTextureRead = texture;
		//		textureRead.CreateType = ImageComponent.TypeEnum._2D;
		//		textureRead.CreateSize = resolution;
		//		textureRead.CreateMipmaps = false;
		//		textureRead.CreateFormat = format;
		//		textureRead.CreateUsage = ImageComponent.Usages.ReadBack | ImageComponent.Usages.BlitDestination;
		//		textureRead.CreateFSAA = 0;
		//		textureRead.Enabled = true;


		//		//render
		//		{
		//			context.SetViewport( viewport, viewMatrix, projectionMatrix );

		//			CanvasRenderer.ShaderItem shader = new CanvasRenderer.ShaderItem();
		//			shader.VertexProgramFileName = @"Base\Shaders\EffectsCommon_vs.sc";
		//			shader.FragmentProgramFileName = @"Base\Shaders\PhysicallyCorrect_CameraDistances_fs.sc";

		//			shader.Parameters.Set( new ViewportRenderingContext.BindTextureData( 0/*"depthTexture"*/,
		//				depthTexture, TextureAddressingMode.Clamp, FilterOption.Point, FilterOption.Point, FilterOption.None ) );

		//			//shader.Parameters.Set( "affectBackground", new Vector4F( (float)frameData.Fog.AffectBackground.Value, 0, 0, 0 ) );

		//			context.RenderQuadToCurrentViewport( shader, CanvasRenderer.BlendingType.Opaque );

		//			//viewport.Update( true, cameraSettings );

		//			//!!!!
		//			//clear temp data
		//			viewport.RenderingContext?.MultiRenderTarget_DestroyAll();
		//			viewport.RenderingContext?.DynamicTexture_DestroyAll();
		//		}

		//		//blit to textureRead
		//		//!!!!
		//		texture.Result.RealObject.BlitTo( context.CurrentViewNumber, textureRead.Result.RealObject, 0, 0 );
		//		//texture.Result.RealObject.BlitTo( viewport.RenderingContext.CurrentViewNumber, textureRead.Result.RealObject, 0, 0 );

		//		//begin reading
		//		var totalBytes = PixelFormatUtility.GetNumElemBytes( format ) * resolution.X * resolution.Y;
		//		physicallyCorrectRenderingData.depthData = NativeUtility.Alloc( NativeUtility.MemoryAllocationType.Renderer, totalBytes );
		//		physicallyCorrectRenderingData.depthDataDemandedFrame = textureRead.Result.RealObject.Read( physicallyCorrectRenderingData.depthData, 0 );
		//	}

		//	//check reading is done
		//	if( RenderingSystem.LastFrameNumber >= physicallyCorrectRenderingData.depthDataDemandedFrame )
		//		physicallyCorrectRenderingData.depthDataDone = true;


		//	//!!!!

		//}

		//void PhysicallyCorrectRendering_RenderLightsDeferred( ViewportRenderingContext context, out bool handled )
		//{
		//	handled = false;

		//	if( RenderingMethod.Value != RenderingMethodEnum.PhysicallyCorrect )
		//		return;
		//	if( physicallyCorrectRenderingData.cameraDistancesTexture == null )
		//		return;
		//	if( !physicallyCorrectRenderingData.depthDataDone )
		//		return;

		//	handled = true;

		//	var resolution = context.owner.SizeInPixels;

		//	//deferredLightingTexture = zzzzzzz;

		//	//create texture
		//	if( physicallyCorrectRenderingData.deferredLightingTexture == null )
		//	{
		//		var mipmaps = false;

		//		var usage = ImageComponent.Usages.WriteOnly;
		//		//if( mipmaps )
		//		//	usage |= ImageComponent.Usages.AutoMipmaps;

		//		var texture2 = ComponentUtility.CreateComponent<ImageComponent>( null, true, false );
		//		physicallyCorrectRenderingData.deferredLightingTexture = texture2;
		//		texture2.CreateType = ImageComponent.TypeEnum._2D;
		//		texture2.CreateSize = resolution;
		//		texture2.CreateMipmaps = mipmaps;

		//		//!!!!temp
		//		texture2.CreateFormat = PixelFormat.Float32RGBA;//A8R8G8B8

		//		texture2.CreateUsage = usage;
		//		texture2.Enabled = true;
		//	}

		//	//!!!!stride. ��� ���

		//	var texture = physicallyCorrectRenderingData.deferredLightingTexture;
		//	var format = texture.Result.ResultFormat;

		//	//update texture
		//	var data = new byte[ PixelFormatUtility.GetNumElemBytes( format ) * resolution.X * resolution.Y ];

		//	unsafe
		//	{
		//		fixed( byte* pData = data )
		//		{
		//			Vector4F* data2 = (Vector4F*)pData;

		//			for( int y = 0; y < resolution.Y; y++ )
		//			{
		//				for( int x = 0; x < resolution.X; x++ )
		//				{
		//					data2[ y * resolution.X + x ] = new Vector4F( (float)x / 1000.0f, 0, 0, 0 );
		//				}
		//			}
		//		}
		//	}

		//	texture.Result.SetData( new GpuTexture.SurfaceData[] { new GpuTexture.SurfaceData( 0, 0, data ) } );

		//	////!!!!
		//	//var sourceTexture = physicallyCorrectRenderingData.cameraDistancesTexture;

		//	//generate compile arguments
		//	var vertexDefines = new List<(string, string)>();
		//	var fragmentDefines = new List<(string, string)>();

		//	string error2;

		//	//vertex program
		//	var vertexProgram = GpuProgramManager.GetProgram( "PhysicallyCorrect_DeferredLight_",
		//		GpuProgramType.Vertex, @"Base\Shaders\PhysicallyCorrect_DeferredLight_vs.sc", vertexDefines, out error2 );
		//	if( !string.IsNullOrEmpty( error2 ) )
		//	{
		//		Log.Fatal( error2 );
		//		return;
		//	}

		//	var fragmentProgram = GpuProgramManager.GetProgram( "PhysicallyCorrect_DeferredLight_",
		//		GpuProgramType.Fragment, @"Base\Shaders\PhysicallyCorrect_DeferredLight_fs.sc", fragmentDefines, out error2 );
		//	if( !string.IsNullOrEmpty( error2 ) )
		//	{
		//		Log.Fatal( error2 );
		//		return;
		//	}


		//	{
		//		var generalContainer = new ParameterContainer();

		//		{
		//			//bind textures

		//			generalContainer.Set( new ViewportRenderingContext.BindTextureData( 0/* "sceneTexture"*/, texture,
		//				TextureAddressingMode.Clamp, FilterOption.Point, FilterOption.Point, FilterOption.Point ) );

		//			//generalContainer.Set( new ViewportRenderingContext.BindTextureData( 0/* "sceneTexture"*/, sceneTexture,
		//			//	TextureAddressingMode.Clamp, FilterOption.Point, FilterOption.Point, FilterOption.Point ) );

		//			//generalContainer.Set( new ViewportRenderingContext.BindTextureData( 1/* "normalTexture"*/, normalTexture,
		//			//	TextureAddressingMode.Clamp, FilterOption.Point, FilterOption.Point, FilterOption.Point ) );

		//			//generalContainer.Set( new ViewportRenderingContext.BindTextureData( 2/* "gBuffer2Texture"*/, gBuffer2Texture,
		//			//	TextureAddressingMode.Clamp, FilterOption.Point, FilterOption.Point, FilterOption.Point ) );

		//			//generalContainer.Set( new ViewportRenderingContext.BindTextureData( 8/*"s_brdfLUT"*/, BrdfLUT,
		//			//	TextureAddressingMode.Clamp, FilterOption.Linear, FilterOption.Linear, FilterOption.Linear ) );

		//			//generalContainer.Set( new ViewportRenderingContext.BindTextureData( 9/* "gBuffer2Texture"*/, gBuffer3Texture,
		//			//	TextureAddressingMode.Clamp, FilterOption.Point, FilterOption.Point, FilterOption.Point ) );

		//			//generalContainer.Set( new ViewportRenderingContext.BindTextureData( 3/* "depthTexture"*/, depthTexture,
		//			//	TextureAddressingMode.Clamp, FilterOption.Point, FilterOption.Point, FilterOption.Point ) );

		//			//generalContainer.Set( new ViewportRenderingContext.BindTextureData( 10/* "gBuffer4Texture"*/, gBuffer4Texture,
		//			//	TextureAddressingMode.Clamp, FilterOption.Point, FilterOption.Point, FilterOption.Point ) );
		//		}

		//		{
		//			var shader = new CanvasRenderer.ShaderItem();
		//			shader.CompiledVertexProgram = vertexProgram;
		//			shader.CompiledFragmentProgram = fragmentProgram;
		//			shader.AdditionalParameterContainers.Add( generalContainer );

		//			context.RenderQuadToCurrentViewport( shader, CanvasRenderer.BlendingType.Add );
		//		}
		//	}

		//}

	}
}
