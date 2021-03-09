// Copyright (C) NeoAxis Group Ltd. 8 Copthall, Roseau Valley, 00152 Commonwealth of Dominica.
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NeoAxis
{
	//!!!!���� �� �����? ����� ������� ������. ����� ���������� ����� � �������� ���������������� �� bounds ���������?

	//!!!!?
	////!!!!new: immutable. ����� ����� ChangedEvent
	//!!!!!!!!���� ������������ �� �� ����� �������� ���������� �����? � ������ ������� ���� ���� ������, �� class

	/// <summary>
	/// Represents a class to contain axis aligned bounds box and bounding sphere.
	/// </summary>
	public class SpaceBounds
	{
		Bounds? boundingBox;
		Sphere? boundingSphere;

		Bounds calculatedBoundingBox;
		//!!!!��� ����� �������
		volatile bool calculatedBoundingBox_Bool;
		Sphere calculatedBoundingSphere;
		volatile bool calculatedBoundingSphere_Bool;
		//Bounds? calculatedBoundingBox;
		//Sphere? calculatedBoundingSphere;

		//static SpaceBounds _default = new SpaceBounds( null, new Sphere( Vec3.Zero, .5 ) );

		//

		//public static SpaceBounds Default
		//{
		//	get { return _default; }
		//}

		public SpaceBounds()
		{
			//this.boundingBox = _default.boundingBox;
			//this.boundingSphere = _default.boundingSphere;
		}

		//!!!!���� ����� ��������� ������ �� box ������ ����� � ��������
		public SpaceBounds( Bounds? boundingBox, Sphere? boundingSphere )
		{
			this.boundingBox = boundingBox;
			this.boundingSphere = boundingSphere;

			//if( boundingBox == null && boundingSphere == null )
			//{
			//	this.boundingBox = _default.boundingBox;
			//	this.boundingSphere = _default.boundingSphere;
			//}
			//else
			//{
			//	this.boundingBox = boundingBox;
			//	this.boundingSphere = boundingSphere;
			//}
		}

		public SpaceBounds( Bounds boundingBox )
			: this( boundingBox, null )
		{
		}

		public SpaceBounds( Sphere boundingSphere )
			: this( null, boundingSphere )
		{
		}

		//!!!!
		//public bool IsValid()
		//{
		//	return boundingBox.HasValue || boundingSphere.HasValue;
		//}

		//!!!!slowly?
		public Bounds? BoundingBox
		{
			get { return boundingBox; }
			//!!!!
			//set
			//{
			//	if( boundingBox == value )
			//		return;
			//	boundingBox = value;
			//	calculatedBoundingBox_Bool = false;
			//	calculatedBoundingSphere_Bool = false;
			//}
		}

		public Sphere? BoundingSphere
		{
			get { return boundingSphere; }
			//!!!!
			//set
			//{
			//	if( boundingSphere == value )
			//		return;
			//	boundingSphere = value;
			//	calculatedBoundingBox_Bool = false;
			//	calculatedBoundingSphere_Bool = false;
			//}
		}

		public void GetCalculatedBoundingBox( out Bounds result )
		{
			if( !calculatedBoundingBox_Bool )
			{
				if( boundingBox.HasValue )
					calculatedBoundingBox = boundingBox.Value;
				else
					boundingSphere.Value.ToBounds( out calculatedBoundingBox );
				calculatedBoundingBox_Bool = true;
			}
			result = calculatedBoundingBox;
		}

		public Bounds CalculatedBoundingBox
		{
			get
			{
				GetCalculatedBoundingBox( out var result );
				return result;
			}
		}

		//public double CalculatedBoundingBoxMaxSide
		//{
		//	get
		//	{
		//		if( !calculatedBoundingBox_Bool )
		//		{
		//			if( boundingBox.HasValue )
		//				calculatedBoundingBox = boundingBox.Value;
		//			else
		//				calculatedBoundingBox = boundingSphere.Value.ToBounds();
		//			calculatedBoundingBox_Bool = true;
		//		}
		//		//!!!!can be cached
		//		calculatedBoundingBox.GetSize( out var size );
		//		return size.MaxComponent();
		//	}
		//}

		public void GetCalculatedBoundingSphere( out Sphere result )
		{
			if( !calculatedBoundingSphere_Bool )
			{
				if( boundingSphere.HasValue )
					calculatedBoundingSphere = boundingSphere.Value;
				else
					calculatedBoundingSphere = boundingBox.Value.GetBoundingSphere();
				calculatedBoundingSphere_Bool = true;
			}
			result = calculatedBoundingSphere;
		}

		public Sphere CalculatedBoundingSphere
		{
			get
			{
				GetCalculatedBoundingSphere( out var result );
				return result;
			}
		}

		public override bool Equals( object obj )
		{
			return ( obj is SpaceBounds && this == (SpaceBounds)obj );
		}

		public override int GetHashCode()
		{
			//!!!!good?
			return ( boundingBox.GetHashCode() ^ boundingSphere.GetHashCode() );
		}

		public static bool operator ==( SpaceBounds a, SpaceBounds b )
		{
			//!!!!���� ��
			bool aNull = ReferenceEquals( a, null );
			bool bNull = ReferenceEquals( b, null );
			if( aNull || bNull )
				return aNull && bNull;

			return ( a.boundingBox == b.boundingBox && a.boundingSphere == b.boundingSphere );
		}

		public static bool operator !=( SpaceBounds a, SpaceBounds b )
		{
			bool aNull = ReferenceEquals( a, null );
			bool bNull = ReferenceEquals( b, null );
			if( aNull || bNull )
				return !( aNull && bNull );

			return ( a.boundingBox != b.boundingBox || a.boundingSphere != b.boundingSphere );
		}

		public static SpaceBounds Merge( SpaceBounds a, SpaceBounds b )
		{
			if( a == null )
				return b;
			if( b == null )
				return a;

			Bounds? bounds = null;
			if( a.boundingBox != null && b.boundingBox != null )
				bounds = Bounds.Merge( a.boundingBox.Value, b.boundingBox.Value );
			else if( a.boundingBox != null )
				bounds = a.boundingBox.Value;
			else if( b.boundingBox != null )
				bounds = b.boundingBox.Value;

			Sphere? sphere = null;
			if( a.boundingSphere != null && b.boundingSphere != null )
				sphere = Sphere.Merge( a.boundingSphere.Value, b.boundingSphere.Value );
			else if( a.boundingSphere != null )
				sphere = a.boundingSphere.Value;
			else if( b.boundingSphere != null )
				sphere = b.boundingSphere.Value;

			return new SpaceBounds( bounds, sphere );
		}

		public static SpaceBounds Multiply( Transform transform, SpaceBounds spaceBounds )
		{
			Bounds? b = null;
			Sphere? s = null;

			//!!!!!slowly
			//!!!!� ����� ��� ���-�� ��������� ����� � bounds
			//!!!��� � ����� ������� ����� ��� ���-�� ��������������/���������

			if( spaceBounds.boundingBox != null )
				b = transform * spaceBounds.boundingBox.Value;
			if( spaceBounds.boundingSphere != null )
				s = transform * spaceBounds.boundingSphere.Value;

			return new SpaceBounds( b, s );
		}
	}
}
