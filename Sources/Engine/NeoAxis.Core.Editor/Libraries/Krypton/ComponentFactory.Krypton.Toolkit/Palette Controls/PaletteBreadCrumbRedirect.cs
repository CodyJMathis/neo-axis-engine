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
using System.Text;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections.Generic;
using System.Diagnostics;

namespace Internal.ComponentFactory.Krypton.Toolkit
{
	/// <summary>
	/// Implement redirected storage for common bread crumb appearance.
	/// </summary>
    public class PaletteBreadCrumbRedirect : PaletteDoubleMetricRedirect
	{
        #region Instance Fields
        private PaletteTripleRedirect _paletteCrumb;
        #endregion

        #region Identity
        /// <summary>
		/// Initialize a new instance of the PaletteBreadCrumbRedirect class.
		/// </summary>
        /// <param name="redirect">Inheritence redirection for bread crumb level.</param>
        /// <param name="needPaint">Delegate for notifying paint requests.</param>
        public PaletteBreadCrumbRedirect(PaletteRedirect redirect,
                                         NeedPaintHandler needPaint)
            : base(redirect, PaletteBackStyle.PanelAlternate, PaletteBorderStyle.ControlClient)
		{
            _paletteCrumb = new PaletteTripleRedirect(redirect, 
                                                      PaletteBackStyle.ButtonBreadCrumb,
                                                      PaletteBorderStyle.ButtonBreadCrumb,
                                                      PaletteContentStyle.ButtonBreadCrumb, 
                                                      needPaint);
        }
		#endregion

        #region IsDefault
        /// <summary>
        /// Gets a value indicating if all values are default.
        /// </summary>
        [Browsable(false)]
        public override bool IsDefault
        {
            get
            {
                return (base.IsDefault && _paletteCrumb.IsDefault);
            }
        }
        #endregion

        #region BreadCrumb
        /// <summary>
        /// Gets access to the bread crumb appearance entries.
        /// </summary>
        [Category("Visuals")]
        [Description("Overrides for defining bread crumb appearance entries.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public PaletteTripleRedirect BreadCrumb
        {
            get { return _paletteCrumb; }
        }

        private bool ShouldSerializeBreadCrumb()
        {
            return !_paletteCrumb.IsDefault;
        }
        #endregion  
    }
}

#endif