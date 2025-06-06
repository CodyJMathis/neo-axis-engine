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
	/// Data associated with a change in the active cell.
	/// </summary>
	public class ActiveCellChangedEventArgs : EventArgs
	{
		#region Instance Fields
        private KryptonWorkspaceCell _oldCell;
        private KryptonWorkspaceCell _newCell;
		#endregion

		#region Identity
		/// <summary>
        /// Initialize a new instance of the ActiveCellChangedEventArgs class.
		/// </summary>
        /// <param name="oldCell">Previous active cell value.</param>
        /// <param name="newCell">New active cell value.</param>
        public ActiveCellChangedEventArgs(KryptonWorkspaceCell oldCell,
                                          KryptonWorkspaceCell newCell)
		{
            _oldCell = oldCell;
            _newCell = newCell;
        }
		#endregion

		#region Public
		/// <summary>
        /// Gets the old cell reference.
		/// </summary>
        public KryptonWorkspaceCell OldCell
		{
            get { return _oldCell; }
		}

        /// <summary>
        /// Gets the new cell reference.
        /// </summary>
        public KryptonWorkspaceCell NewCell
        {
            get { return _newCell; }
        }
        #endregion
	}
}

#endif