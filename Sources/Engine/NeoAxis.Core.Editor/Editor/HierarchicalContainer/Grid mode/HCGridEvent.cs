﻿#if !DEPLOY
// Copyright (C) NeoAxis Group Ltd. 8 Copthall, Roseau Valley, 00152 Commonwealth of Dominica.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Internal.ComponentFactory.Krypton.Toolkit;

namespace NeoAxis.Editor
{
	public interface IHCEvent
	{
		Label LabelName { get; }
		KryptonButton ButtonAddEventHandler { get; }
		KryptonButton ButtonEditEventHandlers { get; }
		void LabelNameSetToolTip( string value );
		void SetToolTip( Control control, string caption );
	}

	public partial class HCGridEvent : EUserControl, IHCEvent
	{
		Control editorControl;

		//

		public HCGridEvent()
		{
			InitializeComponent();

			buttonEditEventHandlers.Values.Image = EditorResourcesCache.Edit;
			buttonAddEventHandler.Values.Image = EditorResourcesCache.New;

			EditorThemeUtility.ApplyDarkThemeToForm( this );

			eventToolTip.SetToolTip( buttonAddEventHandler, EditorLocalization2.Translate( "SettingsWindow", "Add event handler." ) );
			eventToolTip.SetToolTip( buttonEditEventHandlers, EditorLocalization2.Translate( "SettingsWindow", "Edit event handlers." ) );
		}

		public Label LabelName
		{
			get { return labelName; }
		}

		public void LabelNameSetToolTip( string value )
		{
			if( eventToolTip.GetToolTip( labelName ) == string.Empty )
				eventToolTip.SetToolTip( labelName, value );
		}

		public KryptonButton ButtonAddEventHandler
		{
			get { return buttonAddEventHandler; }
		}

		public KryptonButton ButtonEditEventHandlers
		{
			get { return buttonEditEventHandlers; }
		}

		public void SetToolTip( Control control, string caption )
		{
			if( eventToolTip.GetToolTip( control ) != caption )
				eventToolTip.SetToolTip( control, caption );
		}

		public KryptonButton ButtonType
		{
			get { return buttonEditEventHandlers; }
		}

		public Control EditorControl
		{
			get { return editorControl; }
			set
			{
				if( editorControl != null )
					Controls.Remove( editorControl );
				editorControl = value;
				Controls.Add( editorControl );
			}
		}

		protected override void WndProc( ref Message m )
		{
			// pass mouse events to parent.
			if( m.Msg == Internal.ComponentFactory.Krypton.Toolkit.PI.WM_NCHITTEST )
				m.Result = (IntPtr)Internal.ComponentFactory.Krypton.Toolkit.PI.HTTRANSPARENT;
			else
				base.WndProc( ref m );
		}

		public override string ToString()
		{
			return nameof( HCGridEvent ) + ": " + labelName.Text;
		}
	}
}

#endif