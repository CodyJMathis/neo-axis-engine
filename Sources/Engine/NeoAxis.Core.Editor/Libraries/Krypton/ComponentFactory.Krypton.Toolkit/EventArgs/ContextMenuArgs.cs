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
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;

namespace Internal.ComponentFactory.Krypton.Toolkit
{
	/// <summary>
	/// Details for context menu related events.
	/// </summary>
	public class ContextMenuArgs : CancelEventArgs
	{
		#region Instance Fields
        private ContextMenuStrip _cms;
        private KryptonContextMenu _kcm;
        #endregion

		#region Identity
        /// <summary>
        /// Initialize a new instance of the ContextMenuArgs class.
        /// </summary>
        public ContextMenuArgs()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initialize a new instance of the ContextMenuArgs class.
		/// </summary>
        /// <param name="cms">Context menu strip that can be customized.</param>
        public ContextMenuArgs(ContextMenuStrip cms)
            : this(cms, null)
		{
		}

        /// <summary>
        /// Initialize a new instance of the ContextMenuArgs class.
        /// </summary>
        /// <param name="kcm">KryptonContextMenu that can be customized.</param>
        public ContextMenuArgs(KryptonContextMenu kcm)
            : this(null, kcm)
        {
        }

        /// <summary>
        /// Initialize a new instance of the ContextMenuArgs class.
        /// </summary>
        /// <param name="cms">Context menu strip that can be customized.</param>
        /// <param name="kcm">KryptonContextMenu that can be customized.</param>
        public ContextMenuArgs(ContextMenuStrip cms,
                               KryptonContextMenu kcm)
        {
            _cms = cms;
            _kcm = kcm;
        }
        #endregion

		#region Public
		/// <summary>
		/// Gets access to the context menu strip instance.
		/// </summary>
        public ContextMenuStrip ContextMenuStrip
		{
			get { return _cms; }
		}

        /// <summary>
        /// Gets access to the KryptonContextMenu instance.
        /// </summary>
        public KryptonContextMenu KryptonContextMenu
        {
            get { return _kcm; }
        }
        #endregion
	}
}

#endif