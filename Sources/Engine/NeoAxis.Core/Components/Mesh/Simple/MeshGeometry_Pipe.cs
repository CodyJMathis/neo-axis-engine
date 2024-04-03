﻿// Copyright (C) NeoAxis Group Ltd. 8 Copthall, Roseau Valley, 00152 Commonwealth of Dominica.
using System;
using System.ComponentModel;

namespace NeoAxis
{
	/// <summary>
	/// Mesh geometry in the form of a pipe.
	/// </summary>
	public class MeshGeometry_Pipe : MeshGeometry_Procedural
	{
		/// <summary>
		/// The axis of the pipe.
		/// </summary>
		[Serialize]
		[DefaultValue( 2 )]
		[Range( 0, 2 )]
		public Reference<int> Axis
		{
			get { if( _axis.BeginGet() ) Axis = _axis.Get( this ); return _axis.value; }
			set
			{
				if( value < 0 )
					value = new Reference<int>( 0, value.GetByReference );
				if( value > 2 )
					value = new Reference<int>( 2, value.GetByReference );
				if( _axis.BeginSet( this, ref value ) )
				{
					try
					{
						AxisChanged?.Invoke( this );
						ShouldRecompileMesh();
					}
					finally { _axis.EndSet(); }
				}
			}
		}
		/// <summary>Occurs when the <see cref="Axis"/> property value changes.</summary>
		public event Action<MeshGeometry_Pipe> AxisChanged;
		ReferenceField<int> _axis = 2;

		/// <summary>
		/// The radius of the pipe.
		/// </summary>
		[Serialize]
		[DefaultValue( 0.5 )]
		[Range( 0, 100, RangeAttribute.ConvenientDistributionEnum.Exponential, 4 )]
		public Reference<double> Radius
		{
			get { if( _radius.BeginGet() ) Radius = _radius.Get( this ); return _radius.value; }
			set
			{
				if( value < 0 )
					value = new Reference<double>( 0, value.GetByReference );
				if( _radius.BeginSet( this, ref value ) )
				{
					try
					{
						RadiusChanged?.Invoke( this );
						ShouldRecompileMesh();
					}
					finally { _radius.EndSet(); }
				}
			}
		}
		/// <summary>Occurs when the <see cref="Radius"/> property value changes.</summary>
		public event Action<MeshGeometry_Pipe> RadiusChanged;
		ReferenceField<double> _radius = 0.5;

		/// <summary>
		/// The height of the pipe.
		/// </summary>
		[Serialize]
		[DefaultValue( 1.0 )]
		[Range( 0, 100, RangeAttribute.ConvenientDistributionEnum.Exponential, 4 )]
		public Reference<double> Height
		{
			get { if( _height.BeginGet() ) Height = _height.Get( this ); return _height.value; }
			set
			{
				if( value < 0 )
					value = new Reference<double>( 0, value.GetByReference );
				if( _height.BeginSet( this, ref value ) )
				{
					try
					{
						HeightChanged?.Invoke( this );
						ShouldRecompileMesh();
					}
					finally { _height.EndSet(); }
				}
			}
		}
		/// <summary>Occurs when the <see cref="Height"/> property value changes.</summary>
		public event Action<MeshGeometry_Pipe> HeightChanged;
		ReferenceField<double> _height = 1.0;

		/// <summary>
		/// The thickness of the walls of the pipe.
		/// </summary>
		[Serialize]
		[DefaultValue( 0.1 )]
		[Range( 0, 100, RangeAttribute.ConvenientDistributionEnum.Exponential, 4 )]
		public Reference<double> Thickness
		{
			get { if( _thickness.BeginGet() ) Thickness = _thickness.Get( this ); return _thickness.value; }
			set
			{
				if( value < 0 )
					value = new Reference<double>( 0, value.GetByReference );
				if( _thickness.BeginSet( this, ref value ) )
				{
					try
					{
						ThicknessChanged?.Invoke( this );
						ShouldRecompileMesh();
					}
					finally { _thickness.EndSet(); }
				}
			}
		}
		/// <summary>Occurs when the <see cref="Thickness"/> property value changes.</summary>
		public event Action<MeshGeometry_Pipe> ThicknessChanged;
		ReferenceField<double> _thickness = 0.1;

		/// <summary>
		/// The number of sides for the pipe.
		/// </summary>
		[Serialize]
		[DefaultValue( 32 )]
		[Range( 3, 64 )]
		public Reference<int> Segments
		{
			get { if( _segments.BeginGet() ) Segments = _segments.Get( this ); return _segments.value; }
			set
			{
				if( value < 3 )
					value = new Reference<int>( 3, value.GetByReference );
				if( _segments.BeginSet( this, ref value ) )
				{
					try
					{
						SegmentsChanged?.Invoke( this );
						ShouldRecompileMesh();
					}
					finally { _segments.EndSet(); }
				}
			}
		}
		/// <summary>Occurs when the <see cref="Segments"/> property value changes.</summary>
		public event Action<MeshGeometry_Pipe> SegmentsChanged;
		ReferenceField<int> _segments = 32;

		/// <summary>
		/// The number of divisions to use for the pipe height.
		/// </summary>
		[Serialize]
		[DefaultValue( 1 )]
		[Range( 1, 32 )]
		public Reference<int> SegmentsHeight
		{
			get { if( _segmentsHeight.BeginGet() ) SegmentsHeight = _segmentsHeight.Get( this ); return _segmentsHeight.value; }
			set
			{
				if( value < 1 )
					value = new Reference<int>( 1, value.GetByReference );
				if( _segmentsHeight.BeginSet( this, ref value ) )
				{
					try
					{
						SegmentsHeightChanged?.Invoke( this );
						ShouldRecompileMesh();
					}
					finally { _segmentsHeight.EndSet(); }
				}
			}
		}
		/// <summary>Occurs when the <see cref="SegmentsHeight"/> property value changes.</summary>
		public event Action<MeshGeometry_Pipe> SegmentsHeightChanged;
		ReferenceField<int> _segmentsHeight = 1;

		/// <summary>
		/// Whether the pipe is flipped.
		/// </summary>
		[DefaultValue( false )]
		[Serialize]
		public Reference<bool> InsideOut
		{
			get { if( _insideOut.BeginGet() ) InsideOut = _insideOut.Get( this ); return _insideOut.value; }
			set
			{
				if( _insideOut.BeginSet( this, ref value ) )
				{
					try
					{
						InsideOutChanged?.Invoke( this );
						ShouldRecompileMesh();
					}
					finally { _insideOut.EndSet(); }
				}
			}
		}
		/// <summary>Occurs when the <see cref="InsideOut"/> property value changes.</summary>
		public event Action<MeshGeometry_Pipe> InsideOutChanged;
		ReferenceField<bool> _insideOut = false;

		/////////////////////////////////////////

		public override void GetProceduralGeneratedData( ref VertexElement[] vertexStructure, ref byte[] vertices, ref int[] indices, ref Material material, ref byte[] voxelData, ref byte[] clusterData, ref Mesh.StructureClass structure )
		{
			vertexStructure = StandardVertex.MakeStructure( StandardVertex.Components.StaticOneTexCoord, true, out int vertexSize );
			unsafe
			{
				if( vertexSize != sizeof( StandardVertex.StaticOneTexCoord ) )
					Log.Fatal( "vertexSize != sizeof( StandardVertexF )" );
			}

			SimpleMeshGenerator.GeneratePipe( Axis, Radius, Height, Thickness, Segments, SegmentsHeight, InsideOut, out Vector3F[] positions, out Vector3F[] normals, out Vector4F[] tangents, out Vector2F[] texCoords, out indices, out var faces );

			if( faces != null )
				structure = SimpleMeshGenerator.CreateMeshStructure( faces );

			vertices = new byte[ vertexSize * positions.Length ];
			unsafe
			{
				fixed ( byte* pVertices = vertices )
				{
					StandardVertex.StaticOneTexCoord* pVertex = (StandardVertex.StaticOneTexCoord*)pVertices;

					for( int n = 0; n < positions.Length; n++ )
					{
						pVertex->Position = positions[ n ];
						pVertex->Normal = normals[ n ];
						pVertex->Tangent = tangents[ n ];
						pVertex->Color = new ColorValue( 1, 1, 1, 1 );
						pVertex->TexCoord0 = texCoords[ n ];

						pVertex++;
					}
				}
			}
		}
	}
}
