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
using System.IO;
using System.Xml;
using System.Text;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using System.Diagnostics;
using Internal.ComponentFactory.Krypton.Toolkit;
using Internal.ComponentFactory.Krypton.Navigator;
using Internal.ComponentFactory.Krypton.Workspace;

namespace Internal.ComponentFactory.Krypton.Docking
{
    /// <summary>
    /// Acts as a placeholder for a KryptonPage so that it can be restored to this location at a later time.
    /// </summary>
    [ToolboxItem(false)]
    [DesignerCategory("code")]
    [DesignTimeVisible(false)]
    public class KryptonStorePage : KryptonPage
    {
        #region Instance Fields
        private string _storeName;
        #endregion

        #region Identity
        /// <summary>
        /// Initialize a new instance of the KryptonStorePage class.
        /// </summary>
        /// <param name="uniqueName">UniqueName of the page this is placeholding.</param>
        /// <param name="storeName">Storage name associated with this page location.</param>
        public KryptonStorePage(string uniqueName, string storeName)
        {
            Visible = false;
            UniqueName = uniqueName;
            _storeName = storeName;
        }
        #endregion

        #region Public
        /// <summary>
        /// As a placeholder this page is never visible.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override bool LastVisibleSet
        {
            get { return false; }
            set { }
        }

        /// <summary>
        /// Gets the storgate name associated with this page.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public string StoreName
        {
            get { return _storeName; }
        }
        #endregion
    }
}

#endif