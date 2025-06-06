﻿// Copyright (C) NeoAxis Group Ltd. 8 Copthall, Roseau Valley, 00152 Commonwealth of Dominica.
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using NeoAxis.Editor;

namespace NeoAxis
{
	public class Item2DAssemblyRegistration : AssemblyRegistration
	{
		public override void OnRegister()
		{
			ResourceManager.RegisterType( "Item 2D Type", new string[] { "item2dtype" }, typeof( Resource ) );

#if !DEPLOY
			if( EngineApp.IsEditor )
			{
				SceneEditorUtility.CreateObjectWhatTypeWillCreatedEvent += Scene_DocumentWindow_CreateObjectWhatTypeWillCreatedEvent;
				SceneEditorUtility.CreateObjectByCreationDataEvent += Scene_DocumentWindow_CreateObjectByCreationDataEvent;

				EditorAPI.PreviewImagesManager_RegisterResourceType( "Item 2D Type" );

				//Product_Store.CreateScreenshot += Product_Store_CreateScreenshot;
			}
#endif
		}

#if !DEPLOY
		private void Scene_DocumentWindow_CreateObjectWhatTypeWillCreatedEvent( Metadata.TypeInfo objectType, string referenceToObject, ref Metadata.TypeInfo type )
		{
			if( MetadataManager.GetTypeOfNetType( typeof( Item2DType ) ).IsAssignableFrom( objectType ) )
				type = MetadataManager.GetTypeOfNetType( typeof( Item2D ) );
		}

		private void Scene_DocumentWindow_CreateObjectByCreationDataEvent( Metadata.TypeInfo objectType, string referenceToObject, object anyData, Component createTo, ref Component newObject )
		{
			if( newObject == null && MetadataManager.GetTypeOfNetType( typeof( Item2DType ) ).IsAssignableFrom( objectType ) )
			{
				var obj = createTo.CreateComponent<Item2D>( enabled: false );
				newObject = obj;
				obj.ItemType = new Reference<Item2DType>( null, referenceToObject );
			}
		}

#endif
	}
}
