// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.IO;

namespace ImagingCapture.Photo {
	///<summary>The camera session's click on every shot (design section 2), made in code: no sound file ships.</summary>
	public static class PhotoSounds {
		///<summary>A 30 ms click as a 16-bit mono WAV at 22050 Hz: a decaying 2 kHz tone, quiet.</summary>
		public static byte[] MakeClickWav() {
			int sampleRate=22050;
			int countSamples=sampleRate*30/1000;
			using(MemoryStream memoryStream=new MemoryStream())
			using(BinaryWriter binaryWriter=new BinaryWriter(memoryStream)) {
				binaryWriter.Write(new[] { 'R','I','F','F' });
				binaryWriter.Write(36+countSamples*2);
				binaryWriter.Write(new[] { 'W','A','V','E','f','m','t',' ' });
				binaryWriter.Write(16);
				binaryWriter.Write((short)1);//PCM
				binaryWriter.Write((short)1);//mono
				binaryWriter.Write(sampleRate);
				binaryWriter.Write(sampleRate*2);
				binaryWriter.Write((short)2);
				binaryWriter.Write((short)16);
				binaryWriter.Write(new[] { 'd','a','t','a' });
				binaryWriter.Write(countSamples*2);
				for(int i=0;i<countSamples;i++) {
					double decay=Math.Exp(-i/(double)(countSamples/6));
					binaryWriter.Write((short)(6000*decay*Math.Sin(2*Math.PI*2000*i/sampleRate)));
				}
				binaryWriter.Flush();
				return memoryStream.ToArray();
			}
		}
	}
}
