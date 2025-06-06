﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Shared.Utilities;
using Roslyn.Utilities;

namespace RoslynPad.Roslyn
{
	public class RoslynHostReferences
	{
		private static readonly Lazy<(string assemblyPath, string docPath)> _referenceAssembliesPath =
			new Lazy<(string, string)>( GetReferenceAssembliesPath );

		private static readonly Lazy<RoslynHostReferences> _default = new Lazy<RoslynHostReferences>( () =>
		 {
			 var result = Empty.With( typeNamespaceImports: new[]
			 {
				typeof(object),
                //typeof(Thread),
                typeof(Task),
				typeof(List<>),
				typeof(Regex),
				typeof(StringBuilder),
				typeof(Uri),
				typeof(Enumerable),
				typeof(IEnumerable),
				typeof(Path),
				typeof(Assembly),
				typeof(SearchOption)
			 }, assemblyReferences: new[]
			 {
				typeof(Microsoft.CSharp.RuntimeBinder.Binder).GetTypeInfo().Assembly
			 } );

			 var objectAssemblyPath = typeof( object ).GetTypeInfo().Assembly.GetLocation();
			 var mscorlibPath = Path.Combine( Path.GetDirectoryName( objectAssemblyPath ), "mscorlib.dll" );
			 if( File.Exists( mscorlibPath ) )
				 result = result.With( assemblyPathReferences: new[] { mscorlibPath } );

			 var facadeAssemblies = TryGetFacadeAssemblies( _referenceAssembliesPath.Value.assemblyPath );
			 if( facadeAssemblies != null )
				 result = result.With( assemblyPathReferences: facadeAssemblies );
			 else
			 {
				 var systemRuntimePath = Path.Combine( Path.GetDirectoryName( objectAssemblyPath ), "System.Runtime.dll" );
				 if( File.Exists( systemRuntimePath ) )
					 result = result.With( assemblyPathReferences: new[] { systemRuntimePath } );
			 }

			 return result;
		 } );


		public static RoslynHostReferences Empty { get; } = new RoslynHostReferences(
			ImmutableArray<MetadataReference>.Empty,
			ImmutableDictionary<string, string>.Empty.WithComparers( StringComparer.OrdinalIgnoreCase ) );

		public static RoslynHostReferences Default => _default.Value;

		internal static (string assemblyPath, string docPath) ReferenceAssembliesPath => _referenceAssembliesPath.Value;

		public RoslynHostReferences With( IEnumerable<MetadataReference> references = null,
			IEnumerable<Assembly> assemblyReferences = null, IEnumerable<string> assemblyPathReferences = null, IEnumerable<Type> typeNamespaceImports = null )
		{
			var referenceLocations = _referenceLocations;

			var locations =
				assemblyReferences.WhereNotNull().Select( c => c.GetLocation() ).Concat(
				assemblyPathReferences.WhereNotNull() );

			foreach( var location in locations )
				referenceLocations = referenceLocations.SetItem( location, null );

			foreach( var type in typeNamespaceImports.WhereNotNull() )
			{
				var location = type.GetTypeInfo().Assembly.GetLocation();
				referenceLocations = referenceLocations.SetItem( location, null );
			}

			return new RoslynHostReferences( _references.AddRange( references.WhereNotNull() ), referenceLocations );
		}

		private RoslynHostReferences( ImmutableArray<MetadataReference> references, ImmutableDictionary<string, string> referenceLocations )
		{
			_references = references;
			_referenceLocations = referenceLocations;
		}

		private readonly ImmutableArray<MetadataReference> _references;

		private readonly ImmutableDictionary<string, string> _referenceLocations;

		public ImmutableArray<MetadataReference> GetReferences( Func<string, DocumentationProvider> documentationProviderFactory = null )
		{
			var refs = Enumerable.Select( _referenceLocations,
				c => MetadataReference.CreateFromFile( c.Key, documentation: documentationProviderFactory?.Invoke( c.Key ) ) );
			return Enumerable.Concat( _references, refs ).ToImmutableArray();
		}

		private static (string assemblyPath, string docPath) GetReferenceAssembliesPath()
		{
			string assemblyPath = null;
			string docPath = null;

			// TODO: reference assemblies xplat?
			if( !RuntimeInformation.IsOSPlatform( OSPlatform.Windows ) || RuntimeInformation.FrameworkDescription.Contains( ".NET Core" ) )
				return (assemblyPath, docPath);

			var programFiles = Environment.GetEnvironmentVariable( "ProgramFiles(x86)" );

			if( string.IsNullOrEmpty( programFiles ) )
				programFiles = Environment.GetEnvironmentVariable( "ProgramFiles" );

			if( string.IsNullOrEmpty( programFiles ) )
				return (assemblyPath, docPath);

			var path = Path.Combine( programFiles, @"Reference Assemblies\Microsoft\Framework\.NETFramework" );
			if( Directory.Exists( path ) )
			{
				//!!!!по идее нужно использовать текущую версию (не последнюю)

				assemblyPath = IOUtilities.PerformIO( () => Directory.GetDirectories( path ), Array.Empty<string>() )
					.Select( x => new { path = x, version = GetFxVersionFromPath( x ) } )
					.OrderByDescending( x => x.version )
					.FirstOrDefault( x => File.Exists( Path.Combine( x.path, "System.dll" ) ) )?.path;

				if( assemblyPath == null || !File.Exists( Path.Combine( assemblyPath, "System.xml" ) ) )
					docPath = GetReferenceDocumentationPath( path );
			}

			return (assemblyPath, docPath);
		}

		private static string GetReferenceDocumentationPath( string path )
		{
			string docPath = null;

			var docPathTemp = Path.Combine( path, "V4.X" );
			if( File.Exists( Path.Combine( docPathTemp, "System.xml" ) ) )
				docPath = docPathTemp;
			else
			{
				var localeDirectory = IOUtilities.PerformIO( () => Directory.GetDirectories( docPathTemp ),
					Array.Empty<string>() ).FirstOrDefault();
				if( localeDirectory != null && File.Exists( Path.Combine( localeDirectory, "System.xml" ) ) )
					docPath = localeDirectory;
			}

			return docPath;
		}

		private static Version GetFxVersionFromPath( string path )
		{
			var name = Path.GetFileName( path );
			if( name?.StartsWith( "v", StringComparison.OrdinalIgnoreCase ) == true )
			{
				if( Version.TryParse( name.Substring( 1 ), out var version ) )
					return version;
			}

			return new Version( 0, 0 );
		}

		private static IEnumerable<string> TryGetFacadeAssemblies( string referenceAssembliesPath )
		{
			if( referenceAssembliesPath != null )
			{
				var facadesPath = Path.Combine( referenceAssembliesPath, "Facades" );
				if( Directory.Exists( facadesPath ) )
					return Directory.EnumerateFiles( facadesPath, "*.dll" );
			}

			return null;
		}
	}
}