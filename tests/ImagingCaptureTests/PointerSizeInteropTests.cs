// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImagingCapture.Tests {
	///<summary>S3a-12 of the .NET 10 move: shipped native calls outside the imaging layers that this assembly can reach, run in the bitness of the
	///test process (the lane runs this assembly as x86 and as x64). CodeBase's SendMessage overloads, the system folder that names the 64-bit
	///Windows tools, and the DPI awareness handles of WpfControlsOD. Nothing is shown: the edit control is never made visible.</summary>
	[TestClass]
	public class PointerSizeInteropTests {
		private const int WM_SETTEXT=0x000C;
		private const int WM_GETTEXTLENGTH=0x000E;
		private const int EM_GETSEL=0x00B0;
		private const int EM_SETSEL=0x00B1;

		///<summary>Runs action on a new STA thread, which a WinForms control wants, and rethrows what it throws.</summary>
		private static void RunSta(Action action) {
			Exception exception=null;
			Thread thread=new Thread(() => {
				try {
					action();
				}
				catch(Exception ex) {
					exception=ex;
				}
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.IsBackground=true;
			thread.Start();
			Assert.IsTrue(thread.Join(60000),"The STA thread did not finish.");
			if(exception!=null) {
				throw new AssertFailedException(exception.GetType().Name+": "+exception.Message,exception);
			}
		}

		[TestMethod]
		public void SendMessage_IntOverloadsReachTheWindow() {
			RunSta(() => {
				using(TextBox textBox=new TextBox()) {
					textBox.Text="pointer size";
					IntPtr hwnd=textBox.Handle;//Creates the window; the control is never shown.
					Assert.AreEqual(12,MiscUtils.SendMessage(hwnd,WM_GETTEXTLENGTH,0,0));
					Assert.AreEqual(12,WindowsApiWrapper.SendMessage(hwnd,WM_GETTEXTLENGTH,0,0));
					Assert.AreEqual(12,WindowsApiWrapper.SendMessage(hwnd,WM_GETTEXTLENGTH,0,IntPtr.Zero));
					Assert.AreEqual(12,WindowsApiWrapper.SendMessage(hwnd,WM_GETTEXTLENGTH,IntPtr.Zero,0));
					WindowsApiWrapper.SendMessage(hwnd,EM_SETSEL,2,7);
					WindowsApiWrapper.SendMessageRef(hwnd,EM_GETSEL,out int start,out int end);
					Assert.AreEqual(2,start);
					Assert.AreEqual(7,end);
					//EM_SETSEL with an end of -1 selects to the end of the text: the -1 reaches the window whole (sign-extended in a 64-bit
					//process, as C widens an int to LPARAM).
					MiscUtils.SendMessage(hwnd,EM_SETSEL,0,-1);
					WindowsApiWrapper.SendMessageRef(hwnd,EM_GETSEL,out start,out end);
					Assert.AreEqual(0,start);
					Assert.AreEqual(12,end);
				}
			});
		}

		[TestMethod]
		public void SystemFolders_IsWow64Process_OnlyA32BitProcessOn64BitWindows() {
			Assert.IsTrue(ODSystemFolders.IsWow64Process(true,false));
			Assert.IsFalse(ODSystemFolders.IsWow64Process(true,true));
			Assert.IsFalse(ODSystemFolders.IsWow64Process(false,false));
			Assert.AreEqual(Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess,ODSystemFolders.IsWow64Process());
			Assert.AreEqual(@"C:\Windows\sysnative",ODSystemFolders.GetNativeSystemFolder(@"C:\Windows",true));
			Assert.AreEqual(@"C:\Windows\system32",ODSystemFolders.GetNativeSystemFolder(@"C:\Windows",false));
		}

		[TestMethod]
		public void SystemFolders_NativeFolderHoldsTheNativeKernel32() {
			//The folder named for this process holds the operating system's own kernel32.dll: x64 on 64-bit Windows from either bitness (through
			//Sysnative from a 32-bit process, System32 from a 64-bit one), x86 on 32-bit Windows.
			string pathKernel32=Path.Combine(ODSystemFolders.GetNativeSystemFolder(),"kernel32.dll");
			Assert.IsTrue(File.Exists(pathKernel32),pathKernel32);
			const ushort IMAGE_FILE_MACHINE_I386=0x014C;
			const ushort IMAGE_FILE_MACHINE_AMD64=0x8664;
			const ushort IMAGE_FILE_MACHINE_ARM64=0xAA64;
			ushort machine=ReadPeMachine(pathKernel32);
			if(Environment.Is64BitOperatingSystem) {
				Assert.IsTrue(machine==IMAGE_FILE_MACHINE_AMD64 || machine==IMAGE_FILE_MACHINE_ARM64,"Machine 0x"+machine.ToString("X4")+" in "+pathKernel32);
			}
			else {
				Assert.AreEqual(IMAGE_FILE_MACHINE_I386,machine,pathKernel32);
			}
		}

		[TestMethod]
		public void Dpi_AwarenessContextHandlesReachUser32() {
			//DPI_AWARENESS_CONTEXT is a handle: PER_MONITOR_AWARE_V2 is ((DPI_AWARENESS_CONTEXT)-4), valid on Windows 10 1703 and later. Passed
			//as a 4-byte enum to a 64-bit user32 it arrived as 0x00000000FFFFFFFC, which is no context.
			//Environment.OSVersion says 6.2 to a process without a Windows 10 manifest, so the version comes from kernel32.dll.
			System.Diagnostics.FileVersionInfo fileVersionInfo=System.Diagnostics.FileVersionInfo.GetVersionInfo(Path.Combine(Environment.SystemDirectory,"kernel32.dll"));
			if(fileVersionInfo.FileMajorPart<10 || fileVersionInfo.FileBuildPart<15063) {
				Assert.Inconclusive("Windows 10 1703 or later is needed, this is "+fileVersionInfo.FileVersion+".");
			}
			Assert.IsTrue(OpenPmsDental.Dpi.SupportsHighDpi());
		}

		private static ushort ReadPeMachine(string path) {
			using(FileStream fileStream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete))
			using(BinaryReader binaryReader=new BinaryReader(fileStream)) {
				fileStream.Position=0x3C;
				int offsetPe=binaryReader.ReadInt32();
				fileStream.Position=offsetPe;
				Assert.AreEqual(0x00004550u,binaryReader.ReadUInt32(),"PE signature in "+path);
				return binaryReader.ReadUInt16();
			}
		}
	}
}
