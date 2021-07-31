﻿// Copyright (C) 2021 NeoAxis Group Ltd. 8 Copthall, Roseau Valley, 00152 Commonwealth of Dominica.
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using NeoAxis.Editor;

namespace NeoAxis
{
	/// <summary>
	/// A result data of <see cref="IComponent_InteractiveObject.ObjectInteractionGetInfo(UIControl, IComponent_GameMode, ref IComponent_InteractiveObject_ObjectInfo)"/>.
	/// </summary>
	public class IComponent_InteractiveObject_ObjectInfo
	{
		public bool AllowInteract;
		public bool DisplaySelectionRectangle = true;
		public List<string> SelectionTextInfo = new List<string>();
	}

	/// <summary>
	/// An interface of interactive object in the scene.
	/// </summary>
	public interface IComponent_InteractiveObject
	{
		void ObjectInteractionGetInfo( UIControl playScreen, IComponent_GameMode gameMode, ref IComponent_InteractiveObject_ObjectInfo info );

		bool ObjectInteractionInputMessage( UIControl playScreen, IComponent_GameMode gameMode, InputMessage message );

		void ObjectInteractionEnter( Component_GameMode_ObjectInteractionContextClass context );
		void ObjectInteractionExit( Component_GameMode_ObjectInteractionContextClass context );
		void ObjectInteractionUpdate( Component_GameMode_ObjectInteractionContextClass context );
	}
}