// Copyright (C) NeoAxis Group Ltd. 8 Copthall, Roseau Valley, 00152 Commonwealth of Dominica.
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Threading;
using NeoAxis.Editor;
using System.Threading.Tasks;
using System.Net.Http;

namespace NeoAxis.Networking
{
	public class GeneralManagerExecuteCommand
	{
		static HttpClient httpClient;
		//static string generalManagerAddressCached;

		public string FunctionName = "";
		public bool RequireUserLogin;
		public List<(string, string)> Parameters = new List<(string, string)>();
		public RequestMethodEnum RequestMethod;
		public byte[] ContentData;
		public object Tag;

		public volatile ResultClass Result;

		ThreadItem currentThread;

		///////////////////////////////////////////////

		public enum RequestMethodEnum
		{
			Get,
			Post,
		}

		///////////////////////////////////////////////

		public delegate void ProcessedDelegate( GeneralManagerExecuteCommand command );
		/// <summary>
		/// Called from thread.
		/// </summary>
		public event ProcessedDelegate Processed;

		///////////////////////////////////////////////

		class ThreadItem
		{
			public Thread thread;
			public bool callProcessedEventFromMainThread;
			public bool needStop;
			//public string search;
			//public StoreManager.FilterSettingsClass filterSettings;
		}

		///////////////////////////////////////////////

		public class ResultClass
		{
			public TextBlock Data;
			//public string Data = "";
			public string Error = "";
			public DateTime TimeCreated;
		}

		///////////////////////////////////////////////

		void ThreadFunction( object threadItem2 )
		{
			ThreadItem threadItem = (ThreadItem)threadItem2;

			try
			{
				////get GeneralManager address
				//if( string.IsNullOrEmpty( generalManagerAddressCached ) )
				//	generalManagerAddressCached = GeneralManagerFunctions.RequestGeneralManagerAddress( out var error2 );

				var url = string.Format( @"{0}/{1}/", GeneralManagerFunctions.GetHttpURL( NetworkCommonSettings.CloudServiceHost ), FunctionName );
				//var url = string.Format( @"{0}/{1}/", GeneralManagerFunctions.GetHttpURL( generalManagerAddressCached ), FunctionName );

				var paramsAdded = false;

				if( RequireUserLogin )
				{
					if( !LoginUtility.GetCurrentLicense( out string email, out string hash ) )
						throw new Exception( "Please login to process." );

					var email64 = StringUtility.EncodeToBase64URL( email );
					var hash64 = StringUtility.EncodeToBase64URL( hash );
					url += $"?user={email64}&hash={hash64}";

					paramsAdded = true;
				}

				foreach( var param in Parameters )
				{
					var param64 = StringUtility.EncodeToBase64URL( param.Item2 );

					url += paramsAdded ? "&" : "?";
					url += $"{param.Item1}={param64}";

					paramsAdded = true;
				}


				var request = (HttpWebRequest)WebRequest.Create( url );
				request.Timeout = NetworkCommonSettings.ProjectManagerTimeout;

				if( RequestMethod == RequestMethodEnum.Post )
				{
					var contentData = ContentData ?? new byte[ 0 ];
					request.Method = "POST";
					request.ContentLength = contentData.Length;
					request.ContentType = "application/x-www-form-urlencoded";
					var dataStream = request.GetRequestStream();
					dataStream.Write( contentData, 0, contentData.Length );
					dataStream.Close();
				}

				//if( ContentData != null )
				//{
				//	request.Method = "POST";
				//	request.ContentLength = ContentData.Length;
				//	request.ContentType = "application/x-www-form-urlencoded";
				//	var dataStream = request.GetRequestStream();
				//	dataStream.Write( ContentData, 0, ContentData.Length );
				//	dataStream.Close();
				//}


				string blockString = "";

				using( var response = (HttpWebResponse)request.GetResponse() )
				using( var stream = response.GetResponseStream() )
				using( var reader = new StreamReader( stream ) )
					blockString = reader.ReadToEnd();

				if( threadItem.needStop || EditorAPI.ClosingApplication )
					return;

				var block = TextBlock.Parse( blockString, out var error );
				if( !string.IsNullOrEmpty( error ) )
					throw new Exception( "Error of parsing the response data. " + error );

				if( threadItem.needStop || EditorAPI.ClosingApplication )
					return;

				var result = new ResultClass();

				var errorInResultData = block.GetAttribute( "Error" );
				if( !string.IsNullOrEmpty( errorInResultData ) )
					result.Error = errorInResultData;
				else
					result.Data = block;
				result.TimeCreated = DateTime.Now;

				Result = result;

				if( threadItem.callProcessedEventFromMainThread )
				{
					EngineThreading.ExecuteFromMainThreadLater( delegate ()
					{
						Processed?.Invoke( this );
					} );
				}
				else
					Processed?.Invoke( this );
			}
			catch( Exception e )
			{
				if( threadItem.needStop || EditorAPI.ClosingApplication )
					return;

				var result = new ResultClass();
				result.Error = e.Message;
				result.TimeCreated = DateTime.Now;

				Result = result;

				if( threadItem.callProcessedEventFromMainThread )
				{
					EngineThreading.ExecuteFromMainThreadLater( delegate ()
					{
						Processed?.Invoke( this );
					} );
				}
				else
					Processed?.Invoke( this );
			}
		}

		public void BeginExecution( bool callProcessedEventFromMainThread )
		{
			StopExecution();

			var thread = new Thread( ThreadFunction );
			thread.IsBackground = true;
			var threadItem = new ThreadItem() { thread = thread, callProcessedEventFromMainThread = callProcessedEventFromMainThread };
			currentThread = threadItem;

			thread.Start( threadItem );
		}

		public void StopExecution()
		{
			var item = currentThread;
			if( item != null )
				item.needStop = true;
			currentThread = null;
		}

		static async Task<string> SendRequestAsync( string url, RequestMethodEnum requestMethod, byte[] contentData )
		{
			if( httpClient == null )
			{
				httpClient = new HttpClient();
				httpClient.Timeout = TimeSpan.FromMilliseconds( NetworkCommonSettings.ProjectManagerTimeout );
			}

			HttpResponseMessage response;

			if( requestMethod == RequestMethodEnum.Post )
			{
				var content = new ByteArrayContent( contentData ?? new byte[ 0 ] );
				content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue( "application/x-www-form-urlencoded" );
				response = await httpClient.PostAsync( url, content );
			}
			else
				response = await httpClient.GetAsync( url );

			//if( contentData != null )
			//{
			//	var content = new ByteArrayContent( contentData );
			//	content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue( "application/x-www-form-urlencoded" );
			//	response = await httpClient.PostAsync( url, content );
			//}
			//else
			//	response = await httpClient.GetAsync( url );

			response.EnsureSuccessStatusCode();

			string blockString = await response.Content.ReadAsStringAsync();
			return blockString;

			//HttpContent content = null;
			//if( postContentData != null )
			//{
			//	content = new ByteArrayContent( postContentData );
			//	content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue( "application/x-www-form-urlencoded" );
			//}

			//// Send the request
			//HttpResponseMessage response;
			//if( postContentData != null )
			//	response = await httpClient.PostAsync( url, content );
			//else
			//	response = await httpClient.GetAsync( url );
			//response.EnsureSuccessStatusCode();

			//string blockString = await response.Content.ReadAsStringAsync();
			//return blockString;
		}

		public async Task<ResultClass> ExecuteAsync()
		{
			try
			{
				////get GeneralManager address
				//if( string.IsNullOrEmpty( generalManagerAddressCached ) )
				//	generalManagerAddressCached = await GeneralManagerFunctions.RequestGeneralManagerAddressAsync();

				var url = string.Format( @"{0}/{1}/", GeneralManagerFunctions.GetHttpURL( NetworkCommonSettings.CloudServiceHost ), FunctionName );
				//var url = string.Format( @"{0}/{1}/", GeneralManagerFunctions.GetHttpURL( generalManagerAddressCached ), FunctionName );

				var paramsAdded = false;

				if( RequireUserLogin )
				{
					if( !LoginUtility.GetCurrentLicense( out string email, out string hash ) )
						throw new Exception( "Please login to process." );

					var email64 = StringUtility.EncodeToBase64URL( email );
					var hash64 = StringUtility.EncodeToBase64URL( hash );
					url += $"?user={email64}&hash={hash64}";

					paramsAdded = true;
				}

				foreach( var param in Parameters )
				{
					var param64 = StringUtility.EncodeToBase64URL( param.Item2 );

					url += paramsAdded ? "&" : "?";
					url += $"{param.Item1}={param64}";

					paramsAdded = true;
				}

				var blockString = await SendRequestAsync( url, RequestMethod, ContentData );

				//var request = (HttpWebRequest)WebRequest.Create( url );
				//request.Timeout = NetworkCommonSettings.ProjectManagerTimeout;

				//if( ContentData != null )
				//{
				//	request.Method = "POST";
				//	request.ContentLength = ContentData.Length;
				//	request.ContentType = "application/x-www-form-urlencoded";
				//	var dataStream = request.GetRequestStream();
				//	dataStream.Write( ContentData, 0, ContentData.Length );
				//	dataStream.Close();
				//}

				//string blockString = "";

				//using( var response = (HttpWebResponse)request.GetResponse() )
				//using( var stream = response.GetResponseStream() )
				//using( var reader = new StreamReader( stream ) )
				//	blockString = reader.ReadToEnd();


				if( /*threadItem.needStop || */EditorAPI.ClosingApplication )
					throw new Exception( "Closing application." );

				var block = TextBlock.Parse( blockString, out var error );
				if( !string.IsNullOrEmpty( error ) )
					throw new Exception( "Error of parsing the response data. " + error );

				if( /*threadItem.needStop || */EditorAPI.ClosingApplication )
					throw new Exception( "Closing application." );

				var result = new ResultClass();

				var errorInResultData = block.GetAttribute( "Error" );
				if( !string.IsNullOrEmpty( errorInResultData ) )
					result.Error = errorInResultData;
				else
					result.Data = block;
				result.TimeCreated = DateTime.Now;

				return result;
				//Result = result;

				//if( threadItem.callProcessedEventFromMainThread )
				//{
				//	EngineThreading.ExecuteFromMainThreadLater( delegate ()
				//	{
				//		Processed?.Invoke( this );
				//	} );
				//}
				//else
				//	Processed?.Invoke( this );
			}
			catch( Exception e )
			{
				//if( /*threadItem.needStop || */EditorAPI.ClosingApplication )
				//	return;

				var result = new ResultClass();
				result.Error = e.Message;
				result.TimeCreated = DateTime.Now;

				return result;
				//Result = result;

				//if( threadItem.callProcessedEventFromMainThread )
				//{
				//	EngineThreading.ExecuteFromMainThreadLater( delegate ()
				//	{
				//		Processed?.Invoke( this );
				//	} );
				//}
				//else
				//	Processed?.Invoke( this );
			}
		}

		//public async Task<ResultClass> ExecuteAsync()
		//{
		//	BeginExecution( false );

		//	while( Result == null )
		//		await Task.Delay( 10 );

		//	return Result;
		//}
	}
}