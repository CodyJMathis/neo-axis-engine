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

using Internal.ComponentFactory.Krypton.Toolkit;

namespace Internal.ComponentFactory.Krypton.Navigator
{
    /// <summary>
    /// Custom type converter so that MapKryptonPageImage values appear as neat text at design time.
    /// </summary>
    public class MapKryptonPageImageConverter : StringLookupConverter
    {
        #region Static Fields
        private Pair[] _pairs = new Pair[] { new Pair(MapKryptonPageImage.None,             "None (Null image)"),
                                             new Pair(MapKryptonPageImage.Small,            "Small"),
                                             new Pair(MapKryptonPageImage.SmallMedium,      "Small - Medium"), 
                                             new Pair(MapKryptonPageImage.SmallMediumLarge, "Small - Medium - Large"),
                                             new Pair(MapKryptonPageImage.Medium,           "Medium"), 
                                             new Pair(MapKryptonPageImage.MediumSmall,      "Medium - Small"), 
                                             new Pair(MapKryptonPageImage.MediumLarge,      "Medium - Large"),
                                             new Pair(MapKryptonPageImage.Large,            "Large"),
                                             new Pair(MapKryptonPageImage.LargeMedium,      "Large - Medium"),
                                             new Pair(MapKryptonPageImage.LargeMediumSmall, "Large - Medium - Small"),
                                             new Pair(MapKryptonPageImage.ToolTip,          "ToolTip") };
        #endregion
                                             
        #region Identity
        /// <summary>
        /// Initialize a new instance of the MapKryptonPageImageConverter clas.
        /// </summary>
        public MapKryptonPageImageConverter()
            : base(typeof(MapKryptonPageImage))
        {
        }
        #endregion

        #region Protected
        /// <summary>
        /// Gets an array of lookup pairs.
        /// </summary>
        protected override Pair[] Pairs 
        {
            get { return _pairs; }
        }
        #endregion
    }
}

#endif