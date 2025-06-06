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

namespace Internal.ComponentFactory.Krypton.Navigator
{
	/// <summary>
	/// Details for page related events that can be cancelled.
	/// </summary>
	public class KryptonPageCancelEventArgs : KryptonPageEventArgs
	{
		#region Instance Fields
		private bool _cancel;
		#endregion

		#region Identity
		/// <summary>
		/// Initialize a new instance of the KryptonCancelPageEventArgs class.
		/// </summary>
		/// <param name="page">Page effected by event.</param>
		/// <param name="index">Index of page in the owning collection.</param>
		public KryptonPageCancelEventArgs(KryptonPage page, int index)
			: base(page, index)
		{
		}
		#endregion

		#region Cancel
		/// <summary>
		/// Gets the page associated with the event.
		/// </summary>
		public bool Cancel
		{
			get { return _cancel; }
			set { _cancel = value; }
		}
		#endregion
	}
}

#endif