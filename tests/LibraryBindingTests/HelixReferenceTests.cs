// TC-4 (docs/port-notes/TC-4.md, docs/specs/wpf-tooth-chart-plan.md): the eight files Helix Toolkit 3.1.2 brings for the WPF tooth chart
// (HelixToolkit, .Geometry, .Maths, .Wpf, the three .NET Community Toolkit files and System.ComponentModel.Annotations) are library files of
// their packages, and every assembly they reference is one of the library files, part of the .NET Framework, or SRTM, which no package holds:
// HelixToolkit.Wpf uses it only in its terrain types, which the chart never uses. No Helix file references System.Net.Http (risk W2 of the plan;
// the package HelixToolkit names for net48 brings no file the build keeps, ReleaseFlowTests' HelixPackageTests). Reads metadata with
// Mono.Cecil; nothing is loaded.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace LibraryBindingTests {
	[TestClass]
	public class HelixReferenceTests {
		///<summary>The files of Helix Toolkit's closure that ship, with their packages and assembly versions (step 1 of TC-4).</summary>
		private static readonly string[][] _arrayHelixFiles=new[] {
			new[] { "HelixToolkit.dll","HelixToolkit","3.1.2.0" },
			new[] { "HelixToolkit.Geometry.dll","HelixToolkit.Geometry","3.1.2.0" },
			new[] { "HelixToolkit.Maths.dll","HelixToolkit.Maths","3.1.2.0" },
			new[] { "HelixToolkit.Wpf.dll","HelixToolkit.Wpf","3.1.2.0" },
			new[] { "CommunityToolkit.Common.dll","CommunityToolkit.Common","8.3.0.0" },
			new[] { "CommunityToolkit.Diagnostics.dll","CommunityToolkit.Diagnostics","8.3.0.0" },
			new[] { "CommunityToolkit.Mvvm.dll","CommunityToolkit.Mvvm","8.3.0.0" },
			new[] { "System.ComponentModel.Annotations.dll","System.ComponentModel.Annotations","4.2.1.0" },
		};

		///<summary>The one reference no shipped file and no part of the .NET Framework answers: HelixToolkit.Wpf's to SRTM 0.0.8.0 (itinero/srtm).</summary>
		private const string NAME_SRTM="SRTM";

		[TestMethod]
		public void HelixFiles_AreTheLibraryFilesOfTheirPackages() {
			List<string> listProblems=new List<string>();
			foreach(string[] arrayRow in _arrayHelixFiles) {
				if(!Libraries.Files.TryGetValue(arrayRow[0],out string path)) {
					listProblems.Add(arrayRow[0]+" is no library file.");
					continue;
				}
				if(!arrayRow[1].Equals(Libraries.PackageOf(arrayRow[0]),StringComparison.OrdinalIgnoreCase)) {
					listProblems.Add(arrayRow[0]+" comes from "+(Libraries.PackageOf(arrayRow[0])??"Required dlls")+", not the package "+arrayRow[1]+".");
				}
				AssemblyFacts assemblyFacts=MetadataReader.Read(path);
				if(!assemblyFacts.IsManaged || assemblyFacts.Version.ToString()!=arrayRow[2]) {
					listProblems.Add(arrayRow[0]+" is "+(assemblyFacts.IsManaged ? assemblyFacts.Version.ToString() : "not managed")+", not "+arrayRow[2]+".");
				}
			}
			Assert.AreEqual(0,listProblems.Count,string.Join("\r\n",listProblems));
		}

		///<summary>Every reference of the eight files is a library file at a version the configs bind (the library file's own, or an older one that
		///a redirect covers, BindingRedirectTests), part of the .NET Framework, or HelixToolkit.Wpf's SRTM and nothing else; and none is
		///System.Net.Http.</summary>
		[TestMethod]
		public void HelixFiles_ReferenceOnlyLibraryFiles_TheFramework_AndSrtm() {
			List<string> listProblems=new List<string>();
			List<string> listSrtm=new List<string>();
			foreach(string[] arrayRow in _arrayHelixFiles) {
				if(!Libraries.Files.TryGetValue(arrayRow[0],out string path)) {
					listProblems.Add(arrayRow[0]+" is no library file.");
					continue;
				}
				foreach(AssemblyReference assemblyReference in MetadataReader.Read(path).ListReferences) {
					if(assemblyReference.Name.Equals("System.Net.Http",StringComparison.OrdinalIgnoreCase)) {
						listProblems.Add(arrayRow[0]+" references System.Net.Http "+assemblyReference.Version+": risk W2 needs a decision (TC-4).");
						continue;
					}
					if(Libraries.Assemblies.TryGetValue(assemblyReference.Name,out AssemblyFacts assemblyFactsLibrary)) {
						if(assemblyReference.Version>assemblyFactsLibrary.Version) {
							listProblems.Add(arrayRow[0]+" references "+assemblyReference.Name+" "+assemblyReference.Version+", newer than the library file's "
								+assemblyFactsLibrary.Version+".");
						}
						continue;
					}
					if(Gac.Has(assemblyReference.Name)) {
						continue;
					}
					if(assemblyReference.Name==NAME_SRTM) {
						listSrtm.Add(arrayRow[0]+" -> "+assemblyReference.Name+" "+assemblyReference.Version);
						continue;
					}
					listProblems.Add(arrayRow[0]+" references "+assemblyReference.Name+" "+assemblyReference.Version+", which is neither a library file nor part of the "
						+".NET Framework.");
				}
			}
			Assert.AreEqual(0,listProblems.Count,string.Join("\r\n",listProblems));
			CollectionAssert.AreEqual(new[] { "HelixToolkit.Wpf.dll -> SRTM 0.0.8.0" },listSrtm.ToArray(),"The one allowed missing reference, exactly.");
			Assert.IsFalse(Libraries.Files.ContainsKey("SRTM.dll"),"No package or file of the repository holds SRTM.dll.");
		}

		///<summary>Only the terrain types of HelixToolkit.Wpf use SRTM (HgtTerrainModel, and the TerrainModel and TerrainVisual3D that load it), so
		///the missing file never loads unless terrain is shown, which the tooth chart never does: every method, field, property and base type that
		///names a type or member of SRTM belongs to a type whose name says terrain or hgt (nested types included).</summary>
		[TestMethod]
		public void Srtm_IsUsedOnlyByHelixTerrainTypes() {
			if(!Libraries.Files.TryGetValue("HelixToolkit.Wpf.dll",out string path)) {
				Assert.Fail("HelixToolkit.Wpf.dll is no library file.");
			}
			HashSet<string> hashSetUsers=new HashSet<string>(StringComparer.Ordinal);
			using(MemoryStream memoryStream=new MemoryStream(File.ReadAllBytes(path)))
			using(ModuleDefinition moduleDefinition=ModuleDefinition.ReadModule(memoryStream)) {
				foreach(TypeDefinition typeDefinition in AllTypes(moduleDefinition.Types)) {
					List<TypeReference> listTypes=new List<TypeReference>();
					if(typeDefinition.BaseType!=null) {
						listTypes.Add(typeDefinition.BaseType);
					}
					listTypes.AddRange(typeDefinition.Interfaces.Select(x => x.InterfaceType));
					listTypes.AddRange(typeDefinition.Fields.Select(x => x.FieldType));
					listTypes.AddRange(typeDefinition.Properties.Select(x => x.PropertyType));
					bool isUser=listTypes.Any(x => IsSrtm(x));
					foreach(MethodDefinition methodDefinition in typeDefinition.Methods) {
						isUser|=IsSrtm(methodDefinition.ReturnType) || methodDefinition.Parameters.Any(x => IsSrtm(x.ParameterType));
						if(!methodDefinition.HasBody) {
							continue;
						}
						isUser|=methodDefinition.Body.Variables.Any(x => IsSrtm(x.VariableType));
						foreach(Instruction instruction in methodDefinition.Body.Instructions) {
							if(instruction.Operand is MemberReference memberReference
								&& (IsSrtm(memberReference as TypeReference) || IsSrtm(memberReference.DeclaringType)))
							{
								isUser=true;
							}
						}
					}
					if(isUser) {
						hashSetUsers.Add(typeDefinition.FullName);
					}
				}
			}
			Assert.IsTrue(hashSetUsers.Count>0,"HelixToolkit.Wpf references SRTM, so some type uses it.");
			List<string> listOthers=hashSetUsers.Where(x => x.IndexOf("Terrain",StringComparison.OrdinalIgnoreCase)<0 && x.IndexOf("Hgt",StringComparison.OrdinalIgnoreCase)<0)
				.OrderBy(x => x).ToList();
			Assert.AreEqual(0,listOthers.Count,"Types other than the terrain types use SRTM: "+string.Join(", ",listOthers)
				+" (all users: "+string.Join(", ",hashSetUsers.OrderBy(x => x))+")");
		}

		private static IEnumerable<TypeDefinition> AllTypes(IEnumerable<TypeDefinition> types) {
			foreach(TypeDefinition typeDefinition in types) {
				yield return typeDefinition;
				foreach(TypeDefinition typeDefinitionNested in AllTypes(typeDefinition.NestedTypes)) {
					yield return typeDefinitionNested;
				}
			}
		}

		///<summary>True for a type of the SRTM assembly, also as an array, pointer, by-ref or generic argument.</summary>
		private static bool IsSrtm(TypeReference typeReference) {
			if(typeReference==null) {
				return false;
			}
			if(typeReference is GenericInstanceType genericInstanceType && genericInstanceType.GenericArguments.Any(x => IsSrtm(x))) {
				return true;
			}
			if(typeReference is TypeSpecification typeSpecification) {
				return IsSrtm(typeSpecification.ElementType);
			}
			return typeReference.Scope is AssemblyNameReference assemblyNameReference && assemblyNameReference.Name==NAME_SRTM;
		}
	}
}
