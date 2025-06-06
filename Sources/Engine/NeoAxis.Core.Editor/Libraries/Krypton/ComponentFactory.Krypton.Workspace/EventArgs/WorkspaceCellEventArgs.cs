#if !DEPLOY
// *****************************************************************************
// 
//  © Component Factory Pty Ltd 2012. All rights reserved.
//	The software and associated documentation supplied hereunder are the 
//  proprietary information of Component Factory Pty Ltd, 17/267 Nepean Hwy, 
//  Seaford, Vic 3198, Australia and are supplied subject to licence terms.
// 
//
// *****************************************************************************

using System;
using System.Drawing;
using System.Diagnostics;

namespace Internal.ComponentFactory.Krypton.Workspace
{
	/// <summary>
	/// Workspace cell event data.
	/// </summary>
	public class WorkspaceCellEventArgs : EventArgs
	{
		#region Instance Fields
        private KryptonWorkspaceCell _cell;
		#endregion

		#region Identity
		/// <summary>
        /// Initialize a new instance of the WorkspaceCellEventArgs class.
		/// </summary>
        /// <param name="cell">Workspace cell associated with the event.</param>
        public WorkspaceCellEventArgs(KryptonWorkspaceCell cell)
		{
            _cell = cell;
		}
		#endregion

		#region Public
		/// <summary>
        /// Gets the cell reference.
		/// </summary>
        public KryptonWorkspaceCell Cell
		{
            get { return _cell; }
		}
		#endregion
	}
}

#endif