/* ====================================================================
    Copyright (C) 2026  Open PMS - Dental contributors

    This file is part of the fyiReporting RDL project as changed by Open PMS - Dental.

    This library is free software; you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation; either version 2.1 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301  USA
*/
// Added by Open PMS - Dental contributors, 2026-11-16 (package S3b-2b of the .NET 10 move, docs/port-notes/S3b-2.md part B; decision 14 of
// docs/specs/net10-plan.md): a report's <Code> block is compiled with Roslyn's Visual Basic compiler (Microsoft.CodeAnalysis.VisualBasic),
// because CodeDom's VBCodeProvider.CompileAssemblyFromSource, which Code.cs and the designer's syntax check called, throws
// PlatformNotSupportedException on .NET.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.VisualBasic;

namespace fyiReporting.RDL
{
	///<summary>One compile error, as CodeDom's CompilerError gave it: the line in the compiled source (1-based; 0 when the error has no place in
	///the source), the error number (BC30451 and the like) and the text.</summary>
	public class VbCodeError
	{
		public int Line;
		public string ErrorNumber;
		public string ErrorText;
	}

	///<summary>Compiles the Visual Basic source that the engine generates for a report's <Code> block (Code.GetAssembly) and that the report
	///designer generates for its syntax check, as VBCodeProvider.CompileAssemblyFromSource did on .NET Framework with a CompilerParameters that
	///referenced System.dll: the same source, the same compile options and the same errors. What the .NET Framework compile was (measured with
	///the .NET Framework 4.8 vbc that CodeDom runs, tests\RdlCodeTests): Visual Basic 2012 (the newest language that vbc accepts), Option
	///Strict Off, Option Explicit On, Option Infer Off, Option Compare Binary, no project-wide imports (the generated source imports System),
	///errors only (warnings were not reported), in the order of their place in the source. The references cannot be the same assemblies on .NET:
	///they are the runtime's assemblies (the trusted platform assemblies of the folder of the core library: System.*, Microsoft.VisualBasic*,
	///Microsoft.Win32.*, mscorlib and netstandard), so a block can use more of the base library than mscorlib, System.dll and the Visual Basic
	///runtime gave it, and every block that compiled before compiles.</summary>
	public static class VbCodeCompiler
	{
		private static readonly Lazy<List<MetadataReference>> _lazyReferences=new Lazy<List<MetadataReference>>(GetReferences);

		///<summary>Compiles the source and loads the assembly. Returns null and fills listErrors when the source has errors; listErrors is empty
		///otherwise.</summary>
		public static Assembly Compile(string vbcode, out List<VbCodeError> listErrors)
		{
			byte[] image=Emit(vbcode, out listErrors);
			if(image==null)
				return null;
			return Assembly.Load(image);	// in memory: no temporary file, unlike CodeDom (GenerateInMemory=false, Assembly.LoadFrom)
		}

		///<summary>Compiles the source without loading it (the designer's syntax check) and returns the errors, none when it compiles.</summary>
		public static List<VbCodeError> Check(string vbcode)
		{
			List<VbCodeError> listErrors;
			Emit(vbcode, out listErrors);
			return listErrors;
		}

		private static byte[] Emit(string vbcode, out List<VbCodeError> listErrors)
		{
			VisualBasicParseOptions parseOptions=new VisualBasicParseOptions(LanguageVersion.VisualBasic11);
			SyntaxTree syntaxTree=VisualBasicSyntaxTree.ParseText(vbcode, parseOptions);
			VisualBasicCompilationOptions compilationOptions=new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
				optionStrict:OptionStrict.Off, optionInfer:false, optionExplicit:true, optionCompareText:false);
			VisualBasicCompilation compilation=VisualBasicCompilation.Create("RdlCode"+Guid.NewGuid().ToString("N"), new[] { syntaxTree },
				_lazyReferences.Value, compilationOptions);
			using(MemoryStream memoryStream=new MemoryStream())
			{
				EmitResult emitResult=compilation.Emit(memoryStream);
				listErrors=emitResult.Diagnostics
					.Where(x => x.Severity==DiagnosticSeverity.Error)
					.Select(x => new { Diagnostic=x, Span=x.Location.GetLineSpan() })
					.OrderBy(x => x.Diagnostic.Location.IsInSource ? x.Span.StartLinePosition.Line : -1)
					.ThenBy(x => x.Diagnostic.Location.IsInSource ? x.Span.StartLinePosition.Character : -1)
					.Select(x => new VbCodeError() {
						Line=x.Diagnostic.Location.IsInSource ? x.Span.StartLinePosition.Line+1 : 0,
						ErrorNumber=x.Diagnostic.Id,
						ErrorText=x.Diagnostic.GetMessage()
					})
					.ToList();
				if(!emitResult.Success || listErrors.Count>0)
					return null;
				return memoryStream.ToArray();
			}
		}

		///<summary>The runtime's assemblies that a <Code> block compiles against (see the class).</summary>
		private static List<MetadataReference> GetReferences()
		{
			string runtimeFolder=Path.GetDirectoryName(typeof(object).Assembly.Location);
			List<string> listPaths;
			string trustedPlatformAssemblies=AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
			if(!string.IsNullOrEmpty(trustedPlatformAssemblies))
				listPaths=trustedPlatformAssemblies.Split(new[] { Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries).ToList();
			else
				listPaths=Directory.GetFiles(runtimeFolder, "*.dll").Where(x => IsManagedAssembly(x)).ToList();
			return listPaths
				.Where(x => string.Equals(Path.GetDirectoryName(x), runtimeFolder, StringComparison.OrdinalIgnoreCase) && IsRuntimeAssemblyName(Path.GetFileName(x)))
				.OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
				.Select(x => (MetadataReference)MetadataReference.CreateFromFile(x))
				.ToList();
		}

		private static bool IsRuntimeAssemblyName(string fileName)
		{
			return fileName.StartsWith("System.", StringComparison.OrdinalIgnoreCase)
				|| fileName.StartsWith("Microsoft.VisualBasic", StringComparison.OrdinalIgnoreCase)
				|| fileName.StartsWith("Microsoft.Win32.", StringComparison.OrdinalIgnoreCase)
				|| fileName.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase)
				|| fileName.Equals("netstandard.dll", StringComparison.OrdinalIgnoreCase);
		}

		private static bool IsManagedAssembly(string path)
		{
			try
			{
				AssemblyName.GetAssemblyName(path);
				return true;
			}
			catch
			{
				return false;	// a native file of the runtime's folder
			}
		}
	}
}
