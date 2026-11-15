// The protocol between the program and its 32-bit helper program (S3a-13 of the .NET 10 move, docs/port-notes/S3a-13.md, section 3). One
// source file, compiled into the helper (OpenPmsDentalHelper32) and linked into CodeBase (Helper32Client), so both ends always read and write the
// same bytes. Every type here is internal: each assembly that compiles the file has its own copy, and none of them is part of a public API.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenPmsDentalHelper32 {
	///<summary>The constants and the framing of the helper's pipe. The program creates the pipe (\\.\pipe\ of the local machine, a random name of
	///its own, only the current user may open it, remote clients rejected) and starts the helper with its name; the helper connects, sends its
	///hello, and then answers one request at a time with one reply, in order, until the program closes the pipe. A message is a 4-byte
	///little-endian length and that many bytes: a kind byte and the fields, each string a presence byte and BinaryWriter's UTF-8 string, each
	///string list a count and its strings, each byte array a length (-1 for null) and its bytes. The UTF-8 is strict: a string that is not
	///well-formed UTF-16 (a lone surrogate) throws EncoderFallbackException while the message is built in memory, so nothing is written
	///(Helper32Client reports it as Protocol; S3a-14f); a module whose texts can hold one sends them as bytes or code units of its own.</summary>
	internal static class Helper32Protocol {
		///<summary>The protocol version. A module, a command or an argument added to a module (S3a-14's Schick, Topaz and Guru modules) does not
		///change it; only a change of the framing or of the hello does.</summary>
		public const int VERSION=1;
		///<summary>Every pipe of the helper is named PIPE_NAME_PREFIX + the program's process id + "-" + 32 random hex digits, in the local
		///machine's pipe namespace (\\.\pipe\).</summary>
		public const string PIPE_NAME_PREFIX="OpenPmsDentalHelper32-";
		///<summary>The helper's arguments: --pipe &lt;name&gt; --parent &lt;the program's process id&gt;.</summary>
		public const string ARG_PIPE="--pipe";
		public const string ARG_PARENT="--parent";
		///<summary>Tests only: --test-module &lt;assembly name&gt; &lt;type&gt; [&lt;module name&gt;] adds a module of a test assembly in the helper's own folder
		///(OpenPmsDentalHelper32\TestModule.cs).</summary>
		public const string ARG_TEST_MODULE="--test-module";
		///<summary>The helper's own module: Ping and PipeSecurity (OpenPmsDentalHelper32\HostModule.cs).</summary>
		public const string MODULE_HOST="Host";
		///<summary>Host Ping: returns the protocol version, the helper's process id, "1" when it is a 64-bit process ("0" for the 32-bit
		///helper) and the runtime's version.</summary>
		public const string COMMAND_PING="Ping";
		///<summary>Host PipeSecurity: returns the owner and DACL of the helper's pipe in SDDL.</summary>
		public const string COMMAND_PIPE_SECURITY="PipeSecurity";
		///<summary>The largest message either end reads; a longer length is a protocol error, never an allocation.</summary>
		public const int MAX_MESSAGE_BYTES=64*1024*1024;
		///<summary>The first field of the hello.</summary>
		public const string HELLO_MAGIC="OpenPmsDentalHelper32";
		///<summary>Exit codes of the helper: 0 when the program closed the pipe or ended; the others before it answered anything.</summary>
		public const int EXIT_OK=0;
		public const int EXIT_USAGE=2;
		public const int EXIT_PARENT=3;
		public const int EXIT_CONNECT=4;
		public const int EXIT_PROTOCOL=5;
		private const byte KIND_HELLO=1;
		private const byte KIND_REQUEST=2;
		private const byte KIND_REPLY=3;

		///<summary>A new pipe name for the program process processId: the prefix, the process id and 128 random bits, so no other process can
		///guess it before the pipe exists.</summary>
		public static string NewPipeName(int processId) {
			byte[] arrayRandom=new byte[16];
			using(RandomNumberGenerator randomNumberGenerator=RandomNumberGenerator.Create()) {
				randomNumberGenerator.GetBytes(arrayRandom);
			}
			StringBuilder stringBuilder=new StringBuilder(PIPE_NAME_PREFIX);
			stringBuilder.Append(processId.ToString(CultureInfo.InvariantCulture));
			stringBuilder.Append('-');
			foreach(byte b in arrayRandom) {
				stringBuilder.Append(b.ToString("x2",CultureInfo.InvariantCulture));
			}
			return stringBuilder.ToString();
		}

		///<summary>True for a name NewPipeName makes. The helper connects to no other pipe.</summary>
		public static bool IsPipeName(string pipeName) {
			return pipeName!=null && Regex.IsMatch(pipeName,"^"+Regex.Escape(PIPE_NAME_PREFIX)+"[0-9]{1,10}-[0-9a-f]{32}$");
		}

		public static void WriteHello(Stream stream,Helper32Hello hello) {
			WriteMessage(stream,KIND_HELLO,writer => {
				WriteString(writer,HELLO_MAGIC);
				writer.Write(hello.Version);
				writer.Write(hello.ProcessId);
			});
		}

		///<summary>The hello, or null when the stream ended before it.</summary>
		public static Helper32Hello ReadHello(Stream stream) {
			BinaryReader reader=ReadMessage(stream,KIND_HELLO);
			if(reader==null) {
				return null;
			}
			if(ReadString(reader)!=HELLO_MAGIC) {
				throw new InvalidDataException("The helper's hello does not start with "+HELLO_MAGIC+".");
			}
			Helper32Hello hello=new Helper32Hello();
			hello.Version=reader.ReadInt32();
			hello.ProcessId=reader.ReadInt32();
			return hello;
		}

		public static void WriteRequest(Stream stream,Helper32Request request) {
			WriteMessage(stream,KIND_REQUEST,writer => {
				WriteString(writer,request.Module);
				WriteString(writer,request.Command);
				WriteStrings(writer,request.Args);
				WriteBytes(writer,request.Data);
			});
		}

		///<summary>The next request, or null when the program closed the pipe between two requests.</summary>
		public static Helper32Request ReadRequest(Stream stream) {
			BinaryReader reader=ReadMessage(stream,KIND_REQUEST);
			if(reader==null) {
				return null;
			}
			Helper32Request request=new Helper32Request();
			request.Module=ReadString(reader);
			request.Command=ReadString(reader);
			request.Args=ReadStrings(reader);
			request.Data=ReadBytes(reader);
			return request;
		}

		public static void WriteReply(Stream stream,Helper32Reply reply) {
			WriteMessage(stream,KIND_REPLY,writer => {
				writer.Write(reply.IsOk);
				WriteStrings(writer,reply.Results);
				WriteBytes(writer,reply.Data);
				WriteString(writer,reply.ErrorType);
				WriteString(writer,reply.ErrorMessage);
			});
		}

		///<summary>The next reply, or null when the helper's end closed before it (the helper ended).</summary>
		public static Helper32Reply ReadReply(Stream stream) {
			BinaryReader reader=ReadMessage(stream,KIND_REPLY);
			if(reader==null) {
				return null;
			}
			Helper32Reply reply=new Helper32Reply();
			reply.IsOk=reader.ReadBoolean();
			reply.Results=ReadStrings(reader);
			reply.Data=ReadBytes(reader);
			reply.ErrorType=ReadString(reader);
			reply.ErrorMessage=ReadString(reader);
			return reply;
		}

		///<summary>Writes the whole message with one Write call and flushes, so the other end never sees half a length.</summary>
		private static void WriteMessage(Stream stream,byte kind,Action<BinaryWriter> actionWriteFields) {
			byte[] arrayPayload;
			using(MemoryStream memoryStream=new MemoryStream())
			using(BinaryWriter writer=new BinaryWriter(memoryStream,new UTF8Encoding(false,true))) {
				writer.Write(kind);
				actionWriteFields(writer);
				writer.Flush();
				arrayPayload=memoryStream.ToArray();
			}
			if(arrayPayload.Length>MAX_MESSAGE_BYTES) {
				throw new InvalidDataException("A helper message of "+arrayPayload.Length+" bytes is longer than "+MAX_MESSAGE_BYTES+".");
			}
			byte[] arrayMessage=new byte[4+arrayPayload.Length];
			BitConverter.GetBytes(arrayPayload.Length).CopyTo(arrayMessage,0);//little-endian on Windows
			arrayPayload.CopyTo(arrayMessage,4);
			stream.Write(arrayMessage,0,arrayMessage.Length);
			stream.Flush();
		}

		///<summary>A reader over the next message's fields, after its kind byte; null when the stream ends before its first byte. Throws
		///EndOfStreamException for a message cut short and InvalidDataException for a wrong kind or length.</summary>
		private static BinaryReader ReadMessage(Stream stream,byte kindExpected) {
			byte[] arrayLength=new byte[4];
			int countRead=ReadFully(stream,arrayLength,0,4);
			if(countRead==0) {
				return null;
			}
			if(countRead<4) {
				throw new EndOfStreamException("The helper's pipe ended inside a message length.");
			}
			int length=BitConverter.ToInt32(arrayLength,0);
			if(length<1 || length>MAX_MESSAGE_BYTES) {
				throw new InvalidDataException("A helper message length of "+length+" bytes is not allowed.");
			}
			byte[] arrayPayload=new byte[length];
			if(ReadFully(stream,arrayPayload,0,length)<length) {
				throw new EndOfStreamException("The helper's pipe ended inside a message.");
			}
			if(arrayPayload[0]!=kindExpected) {
				throw new InvalidDataException("A helper message of kind "+arrayPayload[0]+" came where kind "+kindExpected+" was expected.");
			}
			BinaryReader reader=new BinaryReader(new MemoryStream(arrayPayload,1,length-1,false),new UTF8Encoding(false,true));
			return reader;
		}

		private static int ReadFully(Stream stream,byte[] buffer,int offset,int count) {
			int total=0;
			while(total<count) {
				int read=stream.Read(buffer,offset+total,count-total);
				if(read<=0) {
					break;
				}
				total+=read;
			}
			return total;
		}

		private static void WriteString(BinaryWriter writer,string value) {
			writer.Write(value!=null);
			if(value!=null) {
				writer.Write(value);
			}
		}

		private static string ReadString(BinaryReader reader) {
			if(!reader.ReadBoolean()) {
				return null;
			}
			return reader.ReadString();
		}

		private static void WriteStrings(BinaryWriter writer,string[] arrayValues) {
			if(arrayValues==null) {
				writer.Write(-1);
				return;
			}
			writer.Write(arrayValues.Length);
			foreach(string value in arrayValues) {
				WriteString(writer,value);
			}
		}

		private static string[] ReadStrings(BinaryReader reader) {
			int count=reader.ReadInt32();
			if(count<0) {
				return null;
			}
			if(count>MAX_MESSAGE_BYTES) {
				throw new InvalidDataException("A helper string list of "+count+" items is not allowed.");
			}
			List<string> listValues=new List<string>();
			for(int i=0;i<count;i++) {
				listValues.Add(ReadString(reader));
			}
			return listValues.ToArray();
		}

		private static void WriteBytes(BinaryWriter writer,byte[] arrayBytes) {
			if(arrayBytes==null) {
				writer.Write(-1);
				return;
			}
			writer.Write(arrayBytes.Length);
			writer.Write(arrayBytes);
		}

		private static byte[] ReadBytes(BinaryReader reader) {
			int length=reader.ReadInt32();
			if(length<0) {
				return null;
			}
			if(length>MAX_MESSAGE_BYTES) {
				throw new InvalidDataException("A helper byte field of "+length+" bytes is not allowed.");
			}
			byte[] arrayBytes=reader.ReadBytes(length);
			if(arrayBytes.Length<length) {
				throw new EndOfStreamException("A helper message ended inside a byte field.");
			}
			return arrayBytes;
		}
	}

	///<summary>The helper's first message: the protocol version and its process id, which the program checks against the process it started.
	///</summary>
	internal sealed class Helper32Hello {
		public int Version;
		public int ProcessId;
	}

	///<summary>One call: a module of the helper (Host, QuickBooks; S3a-14: Schick, Topaz, Guru), one of its commands, its string arguments and
	///an optional byte field (a picture, a structure).</summary>
	internal sealed class Helper32Request {
		public string Module;
		public string Command;
		public string[] Args;
		public byte[] Data;
	}

	///<summary>The answer to one request: its results, or the exception the module threw (its type's full name and its message).</summary>
	internal sealed class Helper32Reply {
		public bool IsOk;
		public string[] Results;
		public byte[] Data;
		public string ErrorType;
		public string ErrorMessage;

		public static Helper32Reply Ok(string[] arrayResults,byte[] arrayData) {
			Helper32Reply reply=new Helper32Reply();
			reply.IsOk=true;
			reply.Results=arrayResults??new string[0];
			reply.Data=arrayData;
			return reply;
		}

		public static Helper32Reply FromException(Exception exception) {
			Helper32Reply reply=new Helper32Reply();
			reply.IsOk=false;
			reply.Results=new string[0];
			reply.ErrorType=exception.GetType().FullName;
			reply.ErrorMessage=exception.Message;
			return reply;
		}
	}
}
