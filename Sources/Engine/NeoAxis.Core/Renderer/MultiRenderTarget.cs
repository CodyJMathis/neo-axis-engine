// Copyright (C) 2021 NeoAxis Group Ltd. 8 Copthall, Roseau Valley, 00152 Commonwealth of Dominica.
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using SharpBgfx;

namespace NeoAxis
{
	/// <summary>
	/// This class represents a multi render target that renders to a <see cref="GpuTexture"/>.
	/// </summary>
	public sealed class MultiRenderTarget : RenderTarget
	{
		Item[] items;

		//

		/// <summary>
		/// Represents an item of <see cref="MultiRenderTarget"/>.
		/// </summary>
		public class Item
		{
			public Component_Image texture;
			public int mip;
			public int layer;

			public Item( Component_Image texture, int mip = 0, int layer = 0 )
			{
				this.texture = texture;
				this.mip = mip;
				this.layer = layer;
			}
		}

		//

		internal MultiRenderTarget( FrameBuffer frameBuffer, Item[] items )
			: base( frameBuffer, true, items[ 0 ].texture.Result.ResultSize )
		{
			this.items = items;
		}

		/// <summary>Releases the resources that are used by the object.</summary>
		public void Dispose()
		{
			EngineThreading.CheckMainThread();

			DisposeInternal();
		}

		public Item[] Items
		{
			get { return items; }
		}
	}
}
