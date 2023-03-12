// Copyright (C) NeoAxis Group Ltd. 8 Copthall, Roseau Valley, 00152 Commonwealth of Dominica.
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using NeoAxis.Networking;

namespace NeoAxis
{
	public class ServerNetworkService_Users : ServerNetworkService
	{
		Dictionary<long, UserInfo> usersByID = new Dictionary<long, UserInfo>();
		Dictionary<NetworkNode.ConnectedNode, UserInfo> usersByConnectedNode = new Dictionary<NetworkNode.ConnectedNode, UserInfo>();
		//UserInfo serverUser;

		long botIDCounter = 10000000001L;
		long botUniqueNameCounter = 1;

		///////////////////////////////////////////

		public class UserInfo
		{
			//long userID;
			//string username;
			NetworkNode.ConnectedNode connectedNode;

			long botUserID;
			string botUsername;

			//

			internal UserInfo( /*long userID, string username, */NetworkNode.ConnectedNode connectedNode )
			{
				//this.userID = userID;
				//this.username = username;
				this.connectedNode = connectedNode;
			}

			internal UserInfo( long botUserID, string botUsername )
			{
				this.botUserID = botUserID;
				this.botUsername = botUsername;
			}

			public long UserID
			{
				get { return connectedNode != null ? connectedNode.LoginDataUserID : botUserID; }
			}

			public string Username
			{
				get { return connectedNode != null ? connectedNode.LoginDataUsername : botUsername; }
			}

			public NetworkNode.ConnectedNode ConnectedNode
			{
				get { return connectedNode; }
			}

			public override string ToString()
			{
				string address;
				if( connectedNode != null )
				{
					if( connectedNode.RemoteEndPoint != null )
						address = connectedNode.RemoteEndPoint.ToString();
					else
						address = "Unknown address";
				}
				else
					address = "No connection";

				return string.Format( "{0} ({1})", Username, address );


				//string ipAddressText;
				//if( connectedNode != null )
				//{
				//	IPAddress ipAddress = IPAddress.None;
				//	if( connectedNode.RemoteEndPoint != null )
				//		ipAddress = connectedNode.RemoteEndPoint.Address;
				//	ipAddressText = ipAddress.ToString();
				//}
				//else
				//	ipAddressText = "Local";
				//return string.Format( "{0} ({1})", Username, ipAddressText );
			}

			public bool Bot
			{
				get { return connectedNode == null; }
			}

			public object AnyData { get; set; }
		}

		///////////////////////////////////////////

		public delegate void AddRemoveUserDelegate( ServerNetworkService_Users sender, UserInfo user );
		public event AddRemoveUserDelegate AddUserEvent;
		public event AddRemoveUserDelegate RemoveUserEvent;

		//public delegate void UpdateUserDelegate( ServerNetworkService_Users sender, UserInfo user, ref string name );
		//public event UpdateUserDelegate UpdateUserEvent;

		///////////////////////////////////////////

		public ServerNetworkService_Users()
			: base( "Users", 2 )
		{
			//register message types
			RegisterMessageType( "AddUserToClient", 1 );
			RegisterMessageType( "RemoveUserToClient", 2 );
			//RegisterMessageType( "UpdateUserToClient", 3 );
		}

		protected override void OnDispose()
		{
			while( usersByID.Count != 0 )
			{
				var enumerator = usersByID.GetEnumerator();
				enumerator.MoveNext();
				RemoveUser( enumerator.Current.Value );
			}

			base.OnDispose();
		}

		public ICollection<UserInfo> Users
		{
			get { return usersByID.Values; }
		}

		public UserInfo GetUser( long userID )
		{
			if( usersByID.TryGetValue( userID, out var user ) )
				return user;
			return null;
		}

		//public UserInfo ServerUser
		//{
		//	get { return serverUser; }
		//}

		public UserInfo GetUser( NetworkNode.ConnectedNode connectedNode )
		{
			if( usersByConnectedNode.TryGetValue( connectedNode, out var user ) )
				return user;
			return null;
		}

		//uint GetFreeUserIdentifier()
		//{
		//	uint identifier = 1;
		//	while( usersByIdentifier.ContainsKey( identifier ) )
		//		identifier++;
		//	return identifier;
		//}

		public UserInfo AddUser( NetworkNode.ConnectedNode connectedNode )
		//UserInfo CreateUser( string name, NetworkNode.ConnectedNode connectedNode )
		{
			//uint identifier = GetFreeUserIdentifier();

			var newUser = new UserInfo( /*identifier, name, */connectedNode );

			usersByID.Add( newUser.UserID, newUser );
			if( newUser.ConnectedNode != null )
				usersByConnectedNode.Add( newUser.ConnectedNode, newUser );

			{
				var messageType = GetMessageType( "AddUserToClient" );

				//!!!!������������� ����

				//send event about new user to the all users
				foreach( var user in Users )
				{
					if( user.ConnectedNode != null )
					{
						bool thisUserFlag = user == newUser;

						var writer = BeginMessage( user.ConnectedNode, messageType );
						writer.Write( newUser.UserID );//writer.WriteVariableUInt64( (ulong)newUser.UserID );
						writer.Write( newUser.Username );
						writer.Write( newUser.Bot );
						writer.Write( thisUserFlag );
						EndMessage();
					}
				}

				//!!!!������������� ����

				if( newUser.ConnectedNode != null )
				{
					//send list of users to new user
					foreach( var user in Users )
					{
						if( user == newUser )
							continue;

						var writer = BeginMessage( newUser.ConnectedNode, messageType );
						writer.Write( user.UserID );//writer.WriteVariableUInt64( (ulong)user.UserID );
						writer.Write( user.Username );
						writer.Write( user.Bot );
						writer.Write( false );//this user flag
						EndMessage();
					}
				}
			}

			AddUserEvent?.Invoke( this, newUser );

			return newUser;
		}

		public UserInfo AddUserBot( string username = "", object anyData = null )
		{
			var username2 = username;
			if( string.IsNullOrEmpty( username2 ) )
			{
				username2 = "Bot" + botUniqueNameCounter.ToString();
				botUniqueNameCounter++;
			}

			var userID = botIDCounter;
			botIDCounter++;

			var newUser = new UserInfo( userID, username2 );
			newUser.AnyData = anyData;

			usersByID.Add( newUser.UserID, newUser );

			{
				var messageType = GetMessageType( "AddUserToClient" );

				//!!!!������������� ����

				//send event about new user to the all users
				foreach( var user in Users )
				{
					if( user.ConnectedNode != null )
					{
						bool thisUserFlag = user == newUser;

						var writer = BeginMessage( user.ConnectedNode, messageType );
						writer.Write( newUser.UserID );//writer.WriteVariableUInt64( (ulong)newUser.UserID );
						writer.Write( newUser.Username );
						writer.Write( newUser.Bot );
						writer.Write( thisUserFlag );
						EndMessage();
					}
				}
			}

			AddUserEvent?.Invoke( this, newUser );

			return newUser;
		}

		//public UserInfo CreateClientUser( NetworkNode.ConnectedNode connectedNode )
		//{
		//	return CreateUser( connectedNode.LoginName, connectedNode );
		//}

		//public UserInfo CreateServerUser( string name )
		//{
		//	if( serverUser != null )
		//		Log.Fatal( "UserManagementServerNetworkService: CreateServerUser: Server user is already created." );

		//	serverUser = CreateUser( name, null );
		//	return serverUser;
		//}

		public void RemoveUser( UserInfo user )
		{
			//check already removed
			if( !usersByID.ContainsKey( user.UserID ) )
				return;
			//if( !usersByID.ContainsValue( user ) )
			//	return;

			RemoveUserEvent?.Invoke( this, user );

			//remove user
			usersByID.Remove( user.UserID );
			if( user.ConnectedNode != null )
				usersByConnectedNode.Remove( user.ConnectedNode );
			//if( serverUser == user )
			//	serverUser = null;


			//!!!!������������� ����

			//send event to the all users
			{
				var messageType = GetMessageType( "RemoveUserToClient" );

				foreach( var toUser in Users )
				{
					if( toUser.ConnectedNode != null )
					{
						var writer = BeginMessage( toUser.ConnectedNode, messageType );
						writer.Write( user.UserID );//writer.WriteVariableUInt64( (ulong)user.UserID );
						EndMessage();
					}
				}
			}
		}

		public long GetFreeUserID()
		{
			for( long userID = 1; ; userID++ )
			{
				if( GetUser( userID ) == null )
					return userID;
			}
		}

		//public void UpdateUser( UserInfo user, string name )
		//{
		//	if( !usersByIdentifier.ContainsValue( user ) )
		//		return;

		//	UpdateUserEvent?.Invoke( this, user, ref name );

		//	if( user.Name != name )
		//	{
		//		user.Name = name;

		//		//send event about update user to the all users
		//		MessageType messageType = GetMessageType( "UpdateUserToClient" );
		//		foreach( var user2 in Users )
		//		{
		//			if( user2.ConnectedNode != null )
		//			{
		//				var writer = BeginMessage( user2.ConnectedNode, messageType );
		//				writer.WriteVariableUInt32( user.Identifier );
		//				writer.Write( user.Name );
		//				EndMessage();
		//			}
		//		}
		//	}
		//}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public class ClientNetworkService_Users : ClientNetworkService
	{
		//!!!!�� ��� ����� ����. ������ �� ������� ������ ���
		//key: user identifier
		Dictionary<long, UserInfo> usersByID = new Dictionary<long, UserInfo>();
		UserInfo thisUser;

		///////////////////////////////////////////

		public class UserInfo
		{
			long userID;
			string username;
			bool bot;

			//

			internal UserInfo( long userID, string username, bool bot )
			{
				this.userID = userID;
				this.username = username;
				this.bot = bot;
			}

			public long UserID
			{
				get { return userID; }
			}

			public string Username
			{
				get { return username; }
				//internal set { username = value; }
			}

			public bool Bot
			{
				get { return bot; }
			}

			public override string ToString()
			{
				return Username;
			}
		}

		///////////////////////////////////////////

		public delegate void AddRemoveUserDelegate( ClientNetworkService_Users sender, UserInfo user );
		public event AddRemoveUserDelegate AddUserEvent;
		public event AddRemoveUserDelegate RemoveUserEvent;
		//public event AddRemoveUserDelegate UpdateUserEvent;

		///////////////////////////////////////////

		public ClientNetworkService_Users()
			: base( "Users", 2 )
		{
			//register message types
			RegisterMessageType( "AddUserToClient", 1, ReceiveMessage_AddUserToClient );
			RegisterMessageType( "RemoveUserToClient", 2, ReceiveMessage_RemoveUserToClient );
			//RegisterMessageType( "UpdateUserToClient", 3, ReceiveMessage_UpdateUserToClient );
		}

		protected override void OnDispose()
		{
			while( usersByID.Count != 0 )
			{
				var enumerator = usersByID.GetEnumerator();
				enumerator.MoveNext();
				RemoveUser( enumerator.Current.Value );
			}

			base.OnDispose();
		}

		public ICollection<UserInfo> Users
		{
			get { return usersByID.Values; }
		}

		public UserInfo GetUser( long userID )
		{
			if( usersByID.TryGetValue( userID, out var user ) )
				return user;
			return null;
		}

		bool ReceiveMessage_AddUserToClient( NetworkNode.ConnectedNode sender, MessageType messageType, ArrayDataReader reader, ref string additionalErrorMessage )
		{
			//get data from message
			var userID = reader.ReadInt64();//long userID = (long)reader.ReadVariableUInt64();
			var username = reader.ReadString();
			var bot = reader.ReadBoolean();
			bool thisUserFlag = reader.ReadBoolean();
			if( !reader.Complete() )
				return false;

			AddUser( userID, username, bot, thisUserFlag );

			return true;
		}

		bool ReceiveMessage_RemoveUserToClient( NetworkNode.ConnectedNode sender, MessageType messageType, ArrayDataReader reader, ref string additionalErrorMessage )
		{
			//get data from message
			var userID = reader.ReadInt64();//long userID = (long)reader.ReadVariableUInt64();
			if( !reader.Complete() )
				return false;

			if( usersByID.TryGetValue( userID, out var user ) )
				RemoveUser( user );

			return true;
		}

		//bool ReceiveMessage_UpdateUserToClient( NetworkNode.ConnectedNode sender, MessageType messageType, ArrayDataReader reader, ref string additionalErrorMessage )
		//{
		//	//get data from message
		//	uint identifier = reader.ReadVariableUInt32();
		//	string name = reader.ReadString();
		//	if( !reader.Complete() )
		//		return false;

		//	var user = GetUser( identifier );
		//	if( user != null )
		//	{
		//		user.Username = name;
		//		UpdateUserEvent?.Invoke( this, user );
		//	}

		//	return true;
		//}

		void AddUser( long userID, string username, bool bot, bool thisUserFlag )
		{
			var user = new UserInfo( userID, username, bot );
			usersByID.Add( userID, user );

			if( thisUserFlag )
				thisUser = user;

			AddUserEvent?.Invoke( this, user );
		}

		void RemoveUser( UserInfo user )
		{
			RemoveUserEvent?.Invoke( this, user );

			usersByID.Remove( user.UserID );

			if( thisUser == user )
				thisUser = null;
		}

		public UserInfo ThisUser
		{
			get { return thisUser; }
		}
	}
}