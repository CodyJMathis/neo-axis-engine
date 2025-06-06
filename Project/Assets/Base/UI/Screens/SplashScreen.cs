// Copyright (C) NeoAxis Group Ltd. 8 Copthall, Roseau Valley, 00152 Commonwealth of Dominica.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using NeoAxis;

namespace Project
{
	public class SplashScreen : UIControl
	{
		ProjectSettingsPage_General.EngineSplashScreenStyleEnum drawSplashScreen = ProjectSettingsPage_General.EngineSplashScreenStyleEnum.Disabled;

		int resetTimeCounter = 3;

		///////////////////////////////////////////

		bool gotoMainMenu;
		bool gotoMainMenuUpdated;

		///////////////////////////////////////////

		[Browsable( false )]
		public double PoweredByTime
		{
			get
			{
				return drawSplashScreen == ProjectSettingsPage_General.EngineSplashScreenStyleEnum.WhiteBackground || drawSplashScreen == ProjectSettingsPage_General.EngineSplashScreenStyleEnum.BlackBackground ? 2.0 : 0.0;
				//return drawSplashScreen != ProjectSettingsPage_General.EngineSplashScreenStyleEnum.Disabled ? 2.0 : 0.0; 
			}
		}

		[Serialize]
		[DefaultValue( 2.0 )]
		public double ProjectTime { get; set; } = 2.0;

		[Serialize]
		[DefaultValue( 0.5 )]
		public double FadingTime { get; set; } = 0.5;

		///////////////////////////////////////////

		protected override void OnEnabledInSimulation()
		{
			var originalDrawSplashScreen = ProjectSettings.Get.General.EngineSplashScreenStyle.Value;

			////get drawing engine splash settings. engine splash for Windows is rendered from another place
			//if( SystemSettings.CurrentPlatform != SystemSettings.Platform.Windows )
			drawSplashScreen = (ProjectSettingsPage_General.EngineSplashScreenStyleEnum)originalDrawSplashScreen;

			//update background color
			if( drawSplashScreen == ProjectSettingsPage_General.EngineSplashScreenStyleEnum.WhiteBackground || drawSplashScreen == ProjectSettingsPage_General.EngineSplashScreenStyleEnum.WhiteBackgroundSmall )
				BackgroundColor = new ColorValue( 1, 1, 1 );
		}

		protected override bool OnKeyDown( KeyEvent e )
		{
			if( base.OnKeyDown( e ) )
				return true;

			if( e.Key == EKeys.Escape || e.Key == EKeys.Return || e.Key == EKeys.Space )
			{
				gotoMainMenu = true;
				return true;
			}

			return false;
		}

		protected override bool OnMouseDown( EMouseButtons button )
		{
			if( base.OnMouseDown( button ) )
				return true;

			if( button == EMouseButtons.Left || button == EMouseButtons.Right )
			{
				gotoMainMenu = true;
				return true;
			}

			return false;
		}

		protected override void OnUpdate( float delta )
		{
			base.OnUpdate( delta );

			if( EngineApp.IsSimulation )
			{
				if( gotoMainMenuUpdated )
				{
					//restore cursor visibility
					EngineApp.ShowCursor = true;

					SimulationApp.ChangeUIScreen( @"Base\UI\Screens\MainMenuScreen.ui", false );
				}

				if( resetTimeCounter == 0 && Time > GetTotalTime() )
					gotoMainMenu = true;
			}
		}

		double GetTotalTime()
		{
			return 1.0 + FadingTime + PoweredByTime + FadingTime + 1.0 + FadingTime + ProjectTime + FadingTime;
		}

		void GetImagesTransparency( out double engine, out double project )
		{
			var curve = new CurveLine();
			curve.AddPoint( 0, new Vector3( 0, 0, 0 ) );
			curve.AddPoint( 1.0, new Vector3( 0, 0, 0 ) );
			curve.AddPoint( 1.0 + FadingTime, new Vector3( 1, 0, 0 ) );
			curve.AddPoint( 1.0 + FadingTime + PoweredByTime, new Vector3( 1, 0, 0 ) );
			curve.AddPoint( 1.0 + FadingTime + PoweredByTime + FadingTime, new Vector3( 0, 0, 0 ) );
			curve.AddPoint( 1.0 + FadingTime + PoweredByTime + FadingTime + 1.0, new Vector3( 0, 0, 0 ) );
			curve.AddPoint( 1.0 + FadingTime + PoweredByTime + FadingTime + 1.0 + FadingTime, new Vector3( 0, 1, 0 ) );
			curve.AddPoint( 1.0 + FadingTime + PoweredByTime + FadingTime + 1.0 + FadingTime + ProjectTime, new Vector3( 0, 1, 0 ) );
			curve.AddPoint( 1.0 + FadingTime + PoweredByTime + FadingTime + 1.0 + FadingTime + ProjectTime + FadingTime, new Vector3( 0, 0, 0 ) );

			var value = curve.CalculateValueByTime( resetTimeCounter != 0 ? 0 : Time );
			engine = MathEx.Saturate( value.X );
			project = MathEx.Saturate( value.Y );

			if( gotoMainMenu )
			{
				engine = 0;
				project = 0;
			}
		}

		void UpdateImagesTransparency()
		{
			GetImagesTransparency( out var engine, out var project );

			var image = Components[ "NeoAxisLogo_DarkBackground" ] as UIImage;
			if( image != null )
			{
				image.Visible = drawSplashScreen == ProjectSettingsPage_General.EngineSplashScreenStyleEnum.BlackBackground;
				image.ColorMultiplier = new ColorValue( 1, 1, 1, engine );
			}

			image = Components[ "NeoAxisLogo_LightBackground" ] as UIImage;
			if( image != null )
			{
				image.Visible = drawSplashScreen == ProjectSettingsPage_General.EngineSplashScreenStyleEnum.WhiteBackground;
				image.ColorMultiplier = new ColorValue( 1, 1, 1, engine );
			}

			image = Components[ "Project" ] as UIImage;
			if( image != null )
				image.ColorMultiplier = new ColorValue( 1, 1, 1, project );

			image = Components[ "NeoAxisLogo_DarkBackground_Small" ] as UIImage;
			if( image != null )
			{
				image.Visible = drawSplashScreen == ProjectSettingsPage_General.EngineSplashScreenStyleEnum.BlackBackgroundSmall;
				image.ColorMultiplier = new ColorValue( 1, 1, 1, project );
			}

			image = Components[ "NeoAxisLogo_LightBackground_Small" ] as UIImage;
			if( image != null )
			{
				image.Visible = drawSplashScreen == ProjectSettingsPage_General.EngineSplashScreenStyleEnum.WhiteBackgroundSmall;
				image.ColorMultiplier = new ColorValue( 1, 1, 1, project );
			}
		}

		protected override void OnRenderUI( CanvasRenderer renderer )
		{
			if( EngineApp.IsSimulation )
				UpdateImagesTransparency();

			base.OnRenderUI( renderer );

			if( EngineApp.IsSimulation )
			{
				if( gotoMainMenu )
					gotoMainMenuUpdated = true;

				//hide cursor
				EngineApp.ShowCursor = false;

				//reset time for waiting to load images
				if( resetTimeCounter > 0 )
				{
					resetTimeCounter--;
					ResetCreateTime();
				}

				//antialiase for better scaling images
				if( !SystemSettings.LimitedDevice )
					renderer.ScreenAntialisingForThisFrame = true;
			}
		}
	}
}