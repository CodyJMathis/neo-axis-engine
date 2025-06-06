﻿#if !DEPLOY
// Copyright (C) NeoAxis Group Ltd. 8 Copthall, Roseau Valley, 00152 Commonwealth of Dominica.
using System;
using System.Text;
using System.Collections.Generic;

namespace NeoAxis.Editor
{
	public partial class ImagePreview : PreviewControlWithViewport
	{
		public ImagePreview()
		{
			InitializeComponent();
		}

		protected override void OnLoad( EventArgs e )
		{
			base.OnLoad( e );

			var scene = CreateScene( false );

			var texture = ObjectOfPreview as ImageComponent;
			if( texture?.Result?.TextureType == ImageComponent.TypeEnum.Cube )
			{
				var sky = scene.CreateComponent<Sky>();
				sky.Cubemap = texture;
				sky.AllowProcessEnvironmentCubemap = false;
			}

			scene.Enabled = true;
		}

		protected override void Viewport_UpdateBeforeOutput( Viewport viewport )
		{
			base.Viewport_UpdateBeforeOutput( viewport );

			var texture = ObjectOfPreview as ImageComponent;
			if( texture?.Result?.TextureType == ImageComponent.TypeEnum._2D )
			{
				double viewScale = 0.95;

				double scale = Math.Min(
					(double)viewport.SizeInPixels.X / (double)texture.Result.ResultSize.X,
					(double)viewport.SizeInPixels.Y / (double)texture.Result.ResultSize.Y );
				Vector2 size = texture.Result.ResultSize.ToVector2() * scale * viewScale;
				Vector2 center = viewport.SizeInPixels.ToVector2() / 2;
				Rectangle rectInPixels = new Rectangle( center - size / 2, center + size / 2 );

				Rectangle rect = rectInPixels / viewport.SizeInPixels.ToVector2();

				var renderer = viewport.CanvasRenderer;

				var pointFiltering = false;
				if( rectInPixels.Size.X >= texture.Result.ResultSize.X && rectInPixels.Size.Y >= texture.Result.ResultSize.Y )
					pointFiltering = true;

				if( pointFiltering )
					renderer.PushTextureFilteringMode( CanvasRenderer.TextureFilteringMode.Point );
				renderer.AddQuad( rect, new Rectangle( 0, 0, 1, 1 ), texture );
				if( pointFiltering )
					renderer.PopTextureFilteringMode();
			}
		}

		static string Translate( string text )
		{
			return EditorLocalization2.Translate( "ImagePreviewControl", text );
		}

		protected override void GetTextInfoLeftTopCorner( List<string> lines )
		{
			var texture = ObjectOfPreview as ImageComponent;
			if( texture != null )
			{
				var result = texture.Result;
				if( result != null )
				{
					lines.Add( Translate( "Source" ) + $": {result.SourceSize}, {TypeUtility.DisplayNameAddSpaces( result.SourceFormat.ToString() )}" );
					lines.Add( Translate( "Processed" ) + $": {result.ResultSize}, {TypeUtility.DisplayNameAddSpaces( result.ResultFormat.ToString() )}" );
					lines.Add( Translate( "Type" ) + $": " + TypeUtility.DisplayNameAddSpaces( result.TextureType.ToString() ) );
				}
				else
					lines.Add( Translate( "No data" ) );
			}
		}
	}
}

#endif