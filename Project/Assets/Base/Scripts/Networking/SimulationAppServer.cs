// Copyright (C) NeoAxis Group Ltd. 8 Copthall, Roseau Valley, 00152 Commonwealth of Dominica.
#if !CLIENT
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NeoAxis;
using NeoAxis.Networking;

namespace Project
{
	/// <summary>
	/// The class for general management of the server.
	/// </summary>
	public static class SimulationAppServer
	{
		static NetworkModeEnum networkMode;
		static int serverPort;
		static long projectID;
		static string directModePassword = "";

		static SimulationAppServerNode server;
		static string LastError { get; set; } = "";

		static DateTime lastSendInfoFromApp;

		//public static string StatusFromProjectApp { get; set; } = "";//"Welcome to the project!";

		static DateTime lastTouchUserVerificationCodes;

		/////////////////////////////////////////

		public class ClientData
		{
			//CloudProject mode
			public string VerificationCode;
			public bool Verified;
			//the ability to reconnect and update user settings (avatar)
			public DateTime LastTouchUserVerificationCode;

			//Direct mode
			public string Username;
		}

		/////////////////////////////////////////

		public enum NetworkModeEnum
		{
			CloudProject,
			Direct,//usual multiplayer
		}

		/////////////////////////////////////////

		public class SimulationAppServerNode : ServerNode
		{
			//services
			ServerNetworkService_Messages messages;
			ServerNetworkService_Users users;
			ServerNetworkService_Chat chat;
			ServerNetworkService_Components components;
			ServerNetworkService_FileSync fileSync;

			//

			public SimulationAppServerNode( string serverName, string serverVersion, int maxConnections, double defaultMaxLifetime )
				: base( serverName, serverVersion, maxConnections, defaultMaxLifetime )
			{
				//register messages service
				messages = new ServerNetworkService_Messages();
				RegisterService( messages );

				//register users service
				users = new ServerNetworkService_Users();
				RegisterService( users );

				//register chat service
				chat = new ServerNetworkService_Chat( users );
				RegisterService( chat );

				//register components service
				components = new ServerNetworkService_Components( users );
				RegisterService( components );

				//register file sync service
				fileSync = new ServerNetworkService_FileSync();
				RegisterService( fileSync );
			}

			public ServerNetworkService_Messages Messages
			{
				get { return messages; }
			}

			public ServerNetworkService_Users Users
			{
				get { return users; }
			}

			public ServerNetworkService_Chat Chat
			{
				get { return chat; }
			}

			public ServerNetworkService_Components Components
			{
				get { return components; }
			}

			public ServerNetworkService_FileSync FileSync
			{
				get { return fileSync; }
			}
		}

		/////////////////////////////////////////

		public static NetworkModeEnum NetworkMode
		{
			get { return networkMode; }
		}

		public static int ServerPort
		{
			get { return serverPort; }
		}

		public static long ProjectID
		{
			get { return projectID; }
		}

		public static string DirectModePassword
		{
			get { return directModePassword; }
		}

		public static bool Created
		{
			get { return Server != null; }
		}

		public static SimulationAppServerNode Server
		{
			get { return server; }
		}

		public static void Init()
		{
			if( SystemSettings.CommandLineParameters.TryGetValue( "-server", out var projectServer ) && projectServer == "1" )
			{
				EngineInfo.SetEngineMode( EngineInfo.EngineModeEnum.CloudServer, null );

				EngineApp.AppDestroy += EngineApp_AppDestroy;
				SimulationApp.MainViewportRenderUI += SimulationApp_MainViewportRenderUI;


				//get network mode
				if( SystemSettings.CommandLineParameters.TryGetValue( "-networkMode", out var networkModeString ) )
				{
					if( !Enum.TryParse( networkModeString, out networkMode ) )
					{
						Log.Fatal( "SimulationAppServer: Init: '-networkMode' unknown mode." );
						return;
					}
				}


				//get server port
				if( !SystemSettings.CommandLineParameters.TryGetValue( "-serverPort", out var serverPortString ) )
				{
					Log.Fatal( "SimulationAppServer: Init: '-serverPort' is not specified." );
					return;
				}
				if( !int.TryParse( serverPortString, out serverPort ) )
				{
					Log.Fatal( "SimulationAppServer: Init: '-serverPort' invalid value." );
					return;

				}

				//get appContainer
				if( SystemSettings.CommandLineParameters.TryGetValue( "-appContainer", out var appContainer ) && appContainer == "1" )
					SystemSettings.AppContainer = true;

				if( networkMode == NetworkModeEnum.CloudProject )
				{
					//get project ID
					if( !SystemSettings.CommandLineParameters.TryGetValue( "-projectID", out var projectIDString ) )
					{
						Log.Fatal( "SimulationAppServer: Init: '-projectID' is not specified." );
						return;
					}
					if( !long.TryParse( projectIDString, out projectID ) )
					{
						Log.Fatal( "SimulationAppServer: Init: '-projectID' invalid value." );
						return;
					}
				}

				if( networkMode == NetworkModeEnum.Direct )
				{
					//get password
					if( SystemSettings.CommandLineParameters.TryGetValue( "-password", out var passwordBase64 ) )
						directModePassword = Encoding.UTF8.GetString( Convert.FromBase64String( passwordBase64 ) );
					else
						directModePassword = "";
				}

				if( !RunOneSceneConfiguration( serverPort, out var error ) )
				{
					LastError = error;
				}

				EngineApp.Tick += EngineApp_Tick;
			}
		}

		private static void EngineApp_AppDestroy()
		{
			DestroyServer();
		}

		static bool CreateServer( out string error )
		{
			server = new SimulationAppServerNode( "NeoAxis Project Server", EngineInfo.Version, 100000, 0 );
			server.ProtocolError += Server_ProtocolError;
			server.IncomingConnectionApproval += Server_IncomingConnectionApproval;
			server.ClientStatusChanged += Server_ClientStatusChanged;
			server.Messages.ReceiveMessageString += Messages_ReceiveMessageString;

			if( !server.BeginListen( serverPort, 30, out error ) )
			{
				server.Dispose();// "" );
				server = null;
				return false;
			}

			return true;
		}

		private static void Server_ProtocolError( ServerNode sender, ServerNode.Client client, string message )
		{
			Log.Warning( "SimulationAppServer: Protocol error: " + message );
		}

		static void DestroyServer()// string reason = "The server has been destroyed." )
		{
			if( server != null )
			{
				server.ProtocolError -= Server_ProtocolError;
				server.IncomingConnectionApproval -= Server_IncomingConnectionApproval;
				server.ClientStatusChanged -= Server_ClientStatusChanged;
				server.Messages.ReceiveMessageString -= Messages_ReceiveMessageString;

				server.Dispose();// reason );
				server = null;
			}
		}

		private static void Server_IncomingConnectionApproval( ServerNode sender, ServerNode.Client client, ref string rejectReason )
		{
			//if( !AllowToConnectNewClients )
			//{
			//	rejectReason = "The server does not allow new users to connect now.";
			//	return false;
			//}

			try
			{
				var block = TextBlock.Parse( client.LoginData, out var error );
				if( !string.IsNullOrEmpty( error ) )
				{
					rejectReason = error;
					return;
				}

				var clientData = new ClientData();

				if( networkMode == NetworkModeEnum.CloudProject )
				{
					clientData.VerificationCode = block.GetAttribute( "VerificationCode" );
					clientData.LastTouchUserVerificationCode = DateTime.Now;
				}
				else
				{
					clientData.Username = block.GetAttribute( "Username" );

					if( directModePassword != block.GetAttribute( "Password" ) )
					{
						rejectReason = "Invalid password.";
						return;
					}
				}

				client.Tag = clientData;
			}
			catch( Exception e )
			{
				rejectReason = "Exception: " + e.Message;
				return;
			}

			//check login and password
			//(use this code for rejection)
			//if(false)
			//{
			//	rejectReason = "Login failed";
			//	return false;
			//}
		}

		static bool RunOneSceneConfiguration( int port, out string error )
		{
			error = "";

			DestroyServer();

			//configure the app
			EngineApp.EnginePauseWhenApplicationIsNotActive = false;

			//initialize networking to connect clients
			//!!!!support recreation
			if( !CreateServer( out error ) )
			{
				//!!!!can fail?
				return false;
			}

			return true;
		}

		private static void EngineApp_Tick( float delta )
		{
			if( server != null )
			{
				server.Update();

				//server.Components.Update();

				//send info from app to server manager
				if( server != null && networkMode == NetworkModeEnum.CloudProject )
				{
					//!!!!how often to send
					if( ( DateTime.Now - lastSendInfoFromApp ).TotalSeconds > 2 )
					{
						var block = new TextBlock();

						block.SetAttribute( "Clients", server.ClientCount.ToString() );
						//block.SetAttribute( "StatusFromProjectApp", StatusFromProjectApp );

						var text = block.DumpToString() + "[[!END!]]";

						try
						{
							var fullPath = Path.Combine( VirtualFileSystem.Directories.AllFiles, "MessageToServerManager.txt" );
							File.WriteAllText( fullPath, text );
						}
						catch { }

						lastSendInfoFromApp = DateTime.Now;
					}
				}

				if( server != null && networkMode == NetworkModeEnum.CloudProject )
					TouchUserVerificationCodes();
			}
		}

		private static void SimulationApp_MainViewportRenderUI()
		{
			if( SimulationApp.NetworkLogging )
			{
				var viewport = SimulationApp.MainViewport;

				var lines = new List<string>();

				lines.Add( "Project server manager is initialized." );
				if( server != null )
					lines.Add( "Project server is created." );

				if( !string.IsNullOrEmpty( LastError ) )
				{
					lines.Add( "" );
					lines.Add( "Last error: " + LastError );
				}

				CanvasRendererUtility.AddTextLinesWithShadow( viewport, lines, new Rectangle( 0.02, 0.02, 1, 1 ), EHorizontalAlignment.Left, EVerticalAlignment.Top, new ColorValue( 1, 1, 1 ) );
			}
		}

		static void GeneralManagerExecuteGetUserByVerificationCodeProcessed( GeneralManagerExecuteCommand command )
		{
			////called from thread

			var connectedNode = (ServerNode.Client)command.Tag;
			//var clientData = (ClientData)connectedNode.Tag;

			if( !string.IsNullOrEmpty( command.Result.Error ) )
			{
				server.DisconnectClient( connectedNode, command.Result.Error );
				//DisconnectConnectedNode( connectedNode, "Error:" + command.Result.Error );
				return;
			}

			//set userID, username
			var block = command.Result.Data;
			connectedNode.LoginDataUserID = long.Parse( block.GetAttribute( "UserID" ) );
			connectedNode.LoginDataUsername = block.GetAttribute( "Username" );

			OnClientConnected( connectedNode );
		}

		private static void Server_ClientStatusChanged( ServerNode sender, ServerNode.Client client, /*NetworkStatus status, */string message )
		{
			if( SimulationApp.NetworkLogging )
				Log.Info( string.Format( "Client connection status changed for {0}: {1}", client.LoginDataUserID, client.Status.ToString() ) );

			switch( client.Status )
			{
			case NetworkStatus.Connected:
				{
					var clientData = (ClientData)client.Tag;

					if( networkMode == NetworkModeEnum.CloudProject )
					{

						//!!!!drop when too long check


						//start checking verification code
						var command = new GeneralManagerExecuteCommand();
						command.FunctionName = "api/get_user_by_verification_code";
						command.Parameters.Add( ("project", projectID.ToString()) );
						command.Parameters.Add( ("purpose", "Enter") );
						command.Parameters.Add( ("code", clientData.VerificationCode) );
						command.Tag = client;
						command.Processed += GeneralManagerExecuteGetUserByVerificationCodeProcessed;
						command.BeginExecution( true );
					}
					else
					{
						client.LoginDataUserID = server.Users.GetFreeUserID();

						var username = clientData.Username;
						if( string.IsNullOrEmpty( username ) )
							username = "User" + client.LoginDataUserID.ToString();
						client.LoginDataUsername = username;

						OnClientConnected( client );
					}
				}
				break;

			case NetworkStatus.Disconnected:
				{
					OnClientDisconnected( client );

					//remove user
					var user = server.Users.GetUser( client );
					if( user != null )
						server.Users.RemoveUser( user );
				}
				break;
			}
		}

		static void GeneralManagerExecuteRequestAvatarSettingsProcessed( GeneralManagerExecuteCommand command )
		{
			////called from thread

			var client = (ServerNode.Client)command.Tag;

			if( !string.IsNullOrEmpty( command.Result.Error ) )
			{
				server.DisconnectClient( client, command.Result.Error );
				return;
			}

			var block = command.Result.Data;
			var avatar = block.GetAttribute( "Avatar" );

			if( server != null )
				server.Messages.SendToClient( client, "AvatarSettings", avatar );
		}

		private static void Messages_ReceiveMessageString( ServerNetworkService_Messages sender, ServerNode.Client source, string message, string data )
		{
			//get messages from clients

			if( SimulationApp.NetworkLogging )
				Log.Info( string.Format( "Message from {0}: {1}", source.LoginDataUserID, message ) );

			if( server != null )
			{
				if( networkMode == NetworkModeEnum.CloudProject )
				{
					//cloud mode


					//!!!!impl

					if( message == "RequestAvatarSettings" )
					{
						var user = server.Users.GetUser( source );
						if( user != null )
						{
							server?.Messages.SendToClient( source, "AvatarSettings", user.DirectServerAvatar );
						}
					}
					else if( message == "SetAvatarSettings" )
					{
						var user = server.Users.GetUser( source );
						if( user != null )
							user.DirectServerAvatar = data;
					}


					//if( message == "RequestAvatarSettings" )
					//{
					//	var user = server.Users.GetUser( source );
					//	if( user != null )
					//	{
					//		var command = new GeneralManagerExecuteCommand();
					//		command.FunctionName = "api/get_user_settings";
					//		command.Parameters.Add( ("user", user.UserID.ToString()) );
					//		command.Parameters.Add( ("property", "Avatar") );
					//		command.Parameters.Add( ("defaultValue", "") );
					//		command.Tag = source;
					//		command.Processed += GeneralManagerExecuteRequestAvatarSettingsProcessed;
					//		command.BeginExecution( true );
					//	}
					//}
					//else if( message == "SetAvatarSettings" )
					//{
					//	var block = TextBlock.Parse( data, out var error );
					//	//if( !string.IsNullOrEmpty( error ) )
					//	//	Log.Warning( "Unable to parse avatar settings. " + error );

					//	if( block != null )
					//	{
					//		var clientData = (ClientData)source.Tag;
					//		if( clientData != null )
					//		{
					//			var command = new GeneralManagerExecuteCommand();
					//			command.FunctionName = "api/set_user_settings";
					//			command.Parameters.Add( ("project", projectID.ToString()) );
					//			command.Parameters.Add( ("purpose", "Enter") );
					//			command.Parameters.Add( ("code", clientData.VerificationCode) );
					//			command.Parameters.Add( ("property", "Avatar") );
					//			command.ContentData = Encoding.UTF8.GetBytes( block.DumpToString() );
					//			//!!!!check Processed?
					//			command.BeginExecution( true );
					//		}
					//	}
					//}
				}
				else
				{
					//usual multiplayer mode

					if( message == "RequestAvatarSettings" )
					{
						var user = server.Users.GetUser( source );
						if( user != null )
						{
							server?.Messages.SendToClient( source, "AvatarSettings", user.DirectServerAvatar );
						}
					}
					else if( message == "SetAvatarSettings" )
					{
						var user = server.Users.GetUser( source );
						if( user != null )
							user.DirectServerAvatar = data;
					}
				}
			}
		}

		public static void SetScene( Scene scene, string sceneInfo )
		{
			ResetScene();

			if( server != null )
				server.Components.SetScene( scene, sceneInfo );
		}

		public static void ResetScene()
		{
			if( server != null && server.Components.Scene != null )
				server.Components.ResetScene();
		}

		static void OnClientConnected( ServerNode.Client client )
		{
			var clientData = (ClientData)client.Tag;

			//add to users service and send events to clients
			server.Users.AddUser( client );

			//set to Verified state
			clientData.Verified = true;
			server.Messages.SendToClient( client, "Verified", "" );

			//!!!!add code here when don't want send scene to the client

			//send initial scene state
			if( server.Components.Scene != null )
			{
				var clientItem = server.Components.GetClientItem( client );
				if( clientData != null )
					server.Components.SendSceneCreate( clientItem );
			}
		}

		static void OnClientDisconnected( ServerNode.Client client )
		{
			var clientData = (ClientData)client.Tag;

			//if( server.Components.Scene != null )
			//{
			var clientItem = server.Components.GetClientItem( client );
			if( clientData != null )
				server.Components.ClientDisconnected( clientItem );
			//}
		}

		static void TouchUserVerificationCodes()
		{
			var now = DateTime.Now;

			if( ( now - lastTouchUserVerificationCodes ).TotalSeconds > 10 )
			{
				foreach( var client in server.GetClientsArray() )
				{

					//!!!!check


					var clientData = client.Tag as ClientData;
					if( clientData != null && clientData.Verified && ( now - clientData.LastTouchUserVerificationCode ).TotalMinutes > 5 )
					{
						var command = new GeneralManagerExecuteCommand();
						command.FunctionName = "api/get_user_by_verification_code";
						command.Parameters.Add( ("project", projectID.ToString()) );
						command.Parameters.Add( ("purpose", "Enter") );
						command.Parameters.Add( ("code", clientData.VerificationCode) );
						command.Tag = client;
						//command.Processed += GeneralManagerExecuteCommandProcessed;
						command.BeginExecution( false );

						clientData.LastTouchUserVerificationCode = now;
					}
				}

				lastTouchUserVerificationCodes = now;
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public class SimulationAppServerAssemblyRegistration : AssemblyRegistration
	{
		public override void OnRegister()
		{
			if( EngineApp.IsSimulation )
			{
				EngineApp.AppCreateAfter += delegate ()
				{
					SimulationAppServer.Init();
				};
			}
		}
	}

}
#endif