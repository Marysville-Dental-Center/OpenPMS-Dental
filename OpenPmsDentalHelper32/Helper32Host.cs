// The request loop of the 32-bit helper program (S3a-13 of the .NET 10 move, docs/port-notes/S3a-13.md, section 3).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenPmsDentalHelper32 {
	///<summary>Reads one request at a time from the program's pipe, gives it to its module and writes the module's reply.</summary>
	internal sealed class Helper32Host {
		private readonly List<IHelper32Module> _listModules;

		public Helper32Host(List<IHelper32Module> listModules) {
			_listModules=listModules;
		}

		///<summary>Answers requests until the program closes the pipe (or ends). Returns the helper's exit code.</summary>
		public int Run(Stream stream) {
			while(true) {
				Helper32Request request;
				try {
					request=Helper32Protocol.ReadRequest(stream);
				}
				catch(EndOfStreamException) {
					return Helper32Protocol.EXIT_PROTOCOL;//a message cut short
				}
				catch(InvalidDataException) {
					return Helper32Protocol.EXIT_PROTOCOL;
				}
				catch(IOException) {
					return Helper32Protocol.EXIT_OK;//the program's end of the pipe is gone
				}
				catch(ObjectDisposedException) {
					return Helper32Protocol.EXIT_OK;
				}
				if(request==null) {
					return Helper32Protocol.EXIT_OK;//the program closed the pipe
				}
				Helper32Reply reply=Dispatch(request);
				try {
					try {
						Helper32Protocol.WriteReply(stream,reply);
					}
					catch(InvalidDataException exception) {
						//A reply longer than the protocol allows: the program gets the reason instead of the results.
						Helper32Protocol.WriteReply(stream,Helper32Reply.FromException(exception));
					}
				}
				catch(IOException) {
					return Helper32Protocol.EXIT_OK;
				}
				catch(ObjectDisposedException) {
					return Helper32Protocol.EXIT_OK;
				}
			}
		}

		///<summary>The module's reply, or an error reply with the exception it threw, or with the reason no module took the request.</summary>
		public Helper32Reply Dispatch(Helper32Request request) {
			IHelper32Module module=_listModules.FirstOrDefault(x => x.Name==request.Module);
			if(module==null) {
				return Helper32Reply.FromException(new InvalidOperationException("The helper has no module "+request.Module+"."));
			}
			try {
				return module.Handle(request)??Helper32Reply.Ok(null,null);
			}
			catch(Exception exception) {
				return Helper32Reply.FromException(exception);
			}
		}
	}

	///<summary>The modules of the helper. S3a-14 adds Schick, Topaz and Guru here; nothing else changes for a new module.</summary>
	internal static class Helper32Modules {
		public static List<IHelper32Module> CreateModules(System.IO.Pipes.PipeStream pipe,HelperArguments helperArguments) {
			List<IHelper32Module> listModules=new List<IHelper32Module>();
			listModules.Add(new HostModule(pipe));
			listModules.Add(new QuickBooks.QuickBooksModule());
			if(helperArguments.TestModuleAssembly!=null) {
				//A test's module takes the place of a module of the same name (VendorDirectTests' stand-in for QuickBooks).
				listModules.RemoveAll(x => x.Name==helperArguments.TestModuleName);
				listModules.Add(TestModule.Load(helperArguments.TestModuleAssembly,helperArguments.TestModuleType,helperArguments.TestModuleName));
			}
			return listModules;
		}
	}
}
