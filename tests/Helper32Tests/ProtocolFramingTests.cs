// The helper protocol's messages on a memory stream (S3a-13 of the .NET 10 move, docs/port-notes/S3a-13.md, section 3): what goes in comes
// out, a stream that ends between messages is the end, and anything else is refused without a large allocation.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalHelper32;

namespace Helper32Tests {
	[TestClass]
	public class ProtocolFramingTests {
		[TestMethod]
		public void Messages_RoundTrip_InOrder() {
			using(MemoryStream memoryStream=new MemoryStream()) {
				Helper32Protocol.WriteHello(memoryStream,new Helper32Hello { Version=Helper32Protocol.VERSION,ProcessId=4242 });
				Helper32Request request=new Helper32Request { Module="QuickBooks",Command="CreateDeposit",Args=new[] { "a",null,"","\u00e9\u4e2d\r\n" },
					Data=new byte[] { 0,1,255 } };
				Helper32Protocol.WriteRequest(memoryStream,request);
				Helper32Protocol.WriteRequest(memoryStream,new Helper32Request { Module="Host",Command="Ping" });
				Helper32Protocol.WriteReply(memoryStream,Helper32Reply.Ok(new[] { "x" },new byte[0]));
				Helper32Protocol.WriteReply(memoryStream,Helper32Reply.FromException(new FormatException("bad \u00e9")));
				memoryStream.Position=0;
				Helper32Hello hello=Helper32Protocol.ReadHello(memoryStream);
				Assert.AreEqual(Helper32Protocol.VERSION,hello.Version);
				Assert.AreEqual(4242,hello.ProcessId);
				Helper32Request requestRead=Helper32Protocol.ReadRequest(memoryStream);
				Assert.AreEqual("QuickBooks",requestRead.Module);
				Assert.AreEqual("CreateDeposit",requestRead.Command);
				CollectionAssert.AreEqual(request.Args,requestRead.Args);
				CollectionAssert.AreEqual(request.Data,requestRead.Data);
				Helper32Request requestPing=Helper32Protocol.ReadRequest(memoryStream);
				Assert.AreEqual("Ping",requestPing.Command);
				Assert.IsNull(requestPing.Args,"A null list stays null.");
				Assert.IsNull(requestPing.Data);
				Helper32Reply replyOk=Helper32Protocol.ReadReply(memoryStream);
				Assert.IsTrue(replyOk.IsOk);
				CollectionAssert.AreEqual(new[] { "x" },replyOk.Results);
				Assert.AreEqual(0,replyOk.Data.Length,"An empty byte field stays empty, not null.");
				Helper32Reply replyError=Helper32Protocol.ReadReply(memoryStream);
				Assert.IsFalse(replyError.IsOk);
				Assert.AreEqual("System.FormatException",replyError.ErrorType);
				Assert.AreEqual("bad \u00e9",replyError.ErrorMessage);
				Assert.IsNull(Helper32Protocol.ReadReply(memoryStream),"The stream's end between messages is the end.");
			}
		}

		[TestMethod]
		public void Read_RefusesACutMessage_AWrongKind_AndALengthOverTheLimit() {
			byte[] arrayMessage;
			using(MemoryStream memoryStream=new MemoryStream()) {
				Helper32Protocol.WriteRequest(memoryStream,new Helper32Request { Module="Host",Command="Ping",Args=new[] { "a" } });
				arrayMessage=memoryStream.ToArray();
			}
			Assert.ThrowsExactly<EndOfStreamException>(() => Helper32Protocol.ReadRequest(new MemoryStream(arrayMessage,0,2)));
			Assert.ThrowsExactly<EndOfStreamException>(() => Helper32Protocol.ReadRequest(new MemoryStream(arrayMessage,0,arrayMessage.Length-1)));
			Assert.ThrowsExactly<InvalidDataException>(() => Helper32Protocol.ReadReply(new MemoryStream(arrayMessage)),"A request is not a reply.");
			byte[] arrayTooLong=BitConverter.GetBytes(Helper32Protocol.MAX_MESSAGE_BYTES+1);
			Assert.ThrowsExactly<InvalidDataException>(() => Helper32Protocol.ReadRequest(new MemoryStream(arrayTooLong)));
			Assert.ThrowsExactly<InvalidDataException>(() => Helper32Protocol.ReadRequest(new MemoryStream(BitConverter.GetBytes(-5))));
			Assert.ThrowsExactly<InvalidDataException>(() => Helper32Protocol.ReadRequest(new MemoryStream(BitConverter.GetBytes(0))));
			//A list count inside a message that claims more items than any message could hold.
			byte[] arrayBadCount=(byte[])arrayMessage.Clone();
			int offsetCount=4+1+(1+1+4)+(1+1+4);//length, kind, "Host" and "Ping" (presence, 7-bit length, chars)
			BitConverter.GetBytes(int.MaxValue).CopyTo(arrayBadCount,offsetCount);
			Assert.ThrowsExactly<InvalidDataException>(() => Helper32Protocol.ReadRequest(new MemoryStream(arrayBadCount)));
		}

		[TestMethod]
		public void Hello_WithAnotherMagic_IsRefused() {
			using(MemoryStream memoryStreamPayload=new MemoryStream())
			using(BinaryWriter writer=new BinaryWriter(memoryStreamPayload,new UTF8Encoding(false))) {
				writer.Write((byte)1);//the hello's kind
				writer.Write(true);
				writer.Write("SomeOtherProgram");
				writer.Write(Helper32Protocol.VERSION);
				writer.Write(4242);
				writer.Flush();
				byte[] arrayPayload=memoryStreamPayload.ToArray();
				MemoryStream memoryStream=new MemoryStream();
				memoryStream.Write(BitConverter.GetBytes(arrayPayload.Length),0,4);
				memoryStream.Write(arrayPayload,0,arrayPayload.Length);
				memoryStream.Position=0;
				Assert.ThrowsExactly<InvalidDataException>(() => Helper32Protocol.ReadHello(memoryStream));
			}
		}

		[TestMethod]
		public void PipeNames_AreRandom_AndOnlyTheirFormIsAccepted() {
			string name=Helper32Protocol.NewPipeName(1234);
			StringAssert.StartsWith(name,"OpenPmsDentalHelper32-1234-");
			Assert.AreEqual("OpenPmsDentalHelper32-1234-".Length+32,name.Length);
			Assert.IsTrue(Helper32Protocol.IsPipeName(name));
			Assert.AreNotEqual(name,Helper32Protocol.NewPipeName(1234));
			foreach(string bad in new[] { null,"","OpenPmsDentalHelper32-1234-","OpenPmsDentalHelper32-x-0123456789abcdef0123456789abcdef",
				"OpenPmsDentalHelper32-1-0123456789ABCDEF0123456789ABCDEF","OpenPmsDentalHelper32-1-0123456789abcdef0123456789abcdef\\..\\x",
				@"\\server\pipe\OpenPmsDentalHelper32-1-0123456789abcdef0123456789abcdef" })
			{
				Assert.IsFalse(Helper32Protocol.IsPipeName(bad),bad);
			}
		}
	}
}
