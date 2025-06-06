﻿// Copyright (C) NeoAxis Group Ltd. 8 Copthall, Roseau Valley, 00152 Commonwealth of Dominica.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace NeoAxis
{
	/// <summary>
	/// Represents a pop-up window that displays a brief description of a control's purpose when the user rests the pointer on the control. Add this object as a child to <see cref="UIControl"/>.
	/// </summary>
	public class UITooltip : Component
	{
		/// <summary>
		/// The text of the tooltip.
		/// </summary>
		[DefaultValue( "Tooltip" )]
#if !DEPLOY
		[Editor( "NeoAxis.Editor.HCItemTextBoxDropMultiline", typeof( object ) )]
#endif
		public Reference<string> Text
		{
			get { if( _text.BeginGet() ) Text = _text.Get( this ); return _text.value; }
			set { if( _text.BeginSet( this, ref value ) ) { try { TextChanged?.Invoke( this ); } finally { _text.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="Text"/> property value changes.</summary>
		public event Action<UITooltip> TextChanged;
		ReferenceField<string> _text = "Tooltip";

		/// <summary>
		/// Time delay before appearing in seconds.
		/// </summary>
		[DefaultValue( 0.5 )]
		public Reference<double> InitialDelay
		{
			get { if( _initialDelay.BeginGet() ) InitialDelay = _initialDelay.Get( this ); return _initialDelay.value; }
			set { if( _initialDelay.BeginSet( this, ref value ) ) { try { InitialDelayChanged?.Invoke( this ); } finally { _initialDelay.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="InitialDelay"/> property value changes.</summary>
		public event Action<UITooltip> InitialDelayChanged;
		ReferenceField<double> _initialDelay = 0.5;
	}
}
