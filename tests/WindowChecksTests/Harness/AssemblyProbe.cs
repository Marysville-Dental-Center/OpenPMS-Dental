// Reads an assembly's identity and attributes without running it (A3's build stamp, A8's serializer pairing): Mono.Cecil reads the metadata
// from a copy of the file's bytes, so nothing is loaded and no file stays locked. Before S3a-8b of the .NET 10 move these were reflection-only
// loads in a throwaway AppDomain, neither of which .NET has; the texts are the same as CustomAttributeData gave them.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;

namespace WindowChecksTests {
	public static class AssemblyProbe {
		///<summary>The AssemblyMetadata attributes of folder\fileName as "key = value" lines, each argument as CustomAttributeTypedArgument.ToString
		///writes it (a string in double quotes).</summary>
		public static string[] GetMetadata(string folder,string fileName) {
			return Read(folder,fileName,moduleDefinition => moduleDefinition.Assembly.CustomAttributes
				.Where(x => x.AttributeType.Name=="AssemblyMetadataAttribute")
				.Select(x => string.Join(" = ",x.ConstructorArguments.Select(y => FormatTypedArgument(y)))).ToArray());
		}

		///<summary>The module version id (MVID) of folder\fileName.</summary>
		public static string GetMvid(string folder,string fileName) {
			return Read(folder,fileName,moduleDefinition => moduleDefinition.Mvid.ToString());
		}

		///<summary>The XmlSerializerVersionAttribute's named arguments of folder\fileName ("ParentAssemblyId = ...; Version = ..."), in the order the
		///attribute's blob holds them, or "" when the assembly has none.</summary>
		public static string GetSerializerVersion(string folder,string fileName) {
			return Read(folder,fileName,moduleDefinition => {
				CustomAttribute customAttribute=moduleDefinition.Assembly.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name=="XmlSerializerVersionAttribute");
				if(customAttribute==null) {
					return "";
				}
				//Mono.Cecil keeps the fields and the properties apart, each in the blob's order; the attribute has properties only, so the order
				//is the blob's, as CustomAttributeData.NamedArguments gave it.
				IEnumerable<CustomAttributeNamedArgument> listNamed=customAttribute.Fields.Concat(customAttribute.Properties);
				return string.Join("; ",listNamed.Select(x => x.Name+" = "+x.Argument.Value));
			});
		}

		///<summary>As CustomAttributeTypedArgument.ToString writes an argument: a string in double quotes, a type as typeof(name), anything else as its
		///value.</summary>
		public static string FormatTypedArgument(CustomAttributeArgument customAttributeArgument) {
			object value=customAttributeArgument.Value;
			if(value is string) {
				return "\""+value+"\"";
			}
			if(value is TypeReference typeReference) {
				return "typeof("+typeReference.FullName+")";
			}
			return value==null ? "null" : value.ToString();
		}

		private static T Read<T>(string folder,string fileName,Func<ModuleDefinition,T> func) {
			using(MemoryStream memoryStream=new MemoryStream(File.ReadAllBytes(Path.Combine(folder,fileName))))
			using(ModuleDefinition moduleDefinition=ModuleDefinition.ReadModule(memoryStream)) {
				return func(moduleDefinition);
			}
		}
	}
}
