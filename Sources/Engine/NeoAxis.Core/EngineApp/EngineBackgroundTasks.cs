//// Copyright (C) 2022 NeoAxis, Inc. Delaware, USA; NeoAxis Group Ltd. 8 Copthall, Roseau Valley, 00152 Commonwealth of Dominica.
//using System;
//using System.ComponentModel;
//using System.Collections.Generic;

//namespace NeoAxis
//{
//	//!!!!!����?

//	/// <summary>
//	/// This class is used to perform engine's operations in a background.
//	/// </summary>
//	public static class EngineBackgroundTasks
//	{
//		public delegate void UpdateDelegate();
//		public static event UpdateDelegate Update;

//		///////////////////////////////////////////

//		public static void PerformUpdate()
//		{
//			//!!!!!��������

//			//!!!!!!
//			Log.Fatal( "impl" );
//			//xx xx;//��������
//			//��� ����: Log.FlushCachedLog();

//			Update?.Invoke();
//		}
//	}
//}
