﻿// Copyright (C) NeoAxis Group Ltd. 8 Copthall, Roseau Valley, 00152 Commonwealth of Dominica.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using NeoAxis.Editor;

namespace NeoAxis.Widget
{
	/// <summary>
	/// Represents an engine widget for Windows Forms.
	/// </summary>
	public partial class WidgetControlWinForms : EngineViewportControl
	{
		bool canSaveConfig;
		bool loaded;

		//public bool needClose;

		//

		public WidgetControlWinForms()
		{
			InitializeComponent();

			if( MainWidget )
				DisableRecreationRenderWindow = true;
		}

		[Browsable( false )]
		public bool MainWidget
		{
			get { return AllInstances.Count == 0 || AllInstances[ 0 ] == this; }
		}

		private void WidgetControl_Load( object sender, EventArgs e )
		{
			if( WinFormsUtility.IsDesignerHosted( this ) )
				return;

			if( MainWidget )
			{
				EngineApp.InitSettings.UseApplicationWindowHandle = Handle;

				if( !EngineApp.Create() )
				{
					//!!!!
					Log.Fatal( "EngineApp.Create() failed." );

					//Close();
					return;
				}

				//set viewport control to manage application render window
				RenderWindow = RenderingSystem.ApplicationRenderTarget;
				Viewport = RenderWindow.Viewports[ 0 ];
				PerformResize();
			}

			try
			{
				if( ProjectSettings.Get.General.EngineSplashScreenStyle.Value == ProjectSettingsPage_General.EngineSplashScreenStyleEnumWithoutDisabled.WhiteBackground || ProjectSettings.Get.General.EngineSplashScreenStyle.Value == ProjectSettingsPage_General.EngineSplashScreenStyleEnumWithoutDisabled.WhiteBackgroundSmall )
					BackColor = Color.White;
			}
			catch { }

			loaded = true;
		}

		protected override void OnDestroy()
		{
			if( WinFormsUtility.IsDesignerHosted( this ) )
				return;

			if( MainWidget )
			{
				if( !canSaveConfig )
					EngineApp.NeedSaveConfig = false;
				EngineApp.Shutdown();
			}

			base.OnDestroy();
		}

		private void timer1_Tick( object sender, EventArgs e )
		{
			if( WinFormsUtility.IsDesignerHosted( this ) )
				return;
			if( !loaded )
				return;

			if( MainWidget )
			{
				//!!!!!
				//!!!!где еще вызывать?
				if( EngineApp.Instance != null )
					EngineApp.DoTick();

				//!!!!
				//UpdateSoundSystem();

				//if( needClose )
				//{
				//	needClose = false;
				//	Close();
				//}

				//if( firstTick )
				//{
				//	firstTick = false;
				//}

				canSaveConfig = true;
				//firstTick = false;
			}
		}

		public override bool IsWidget
		{
			get { return true; }
		}
	}
}
