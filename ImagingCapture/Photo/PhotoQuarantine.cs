// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CodeBase;
using ImagingDeviceManager.Config;
using OpenPmsDentalBusiness;

namespace ImagingCapture.Photo {
	///<summary>A photo of a camera session that could not be saved to the patient's images, kept on this computer with what it was for.</summary>
	public sealed class PhotoUnsaved {
		///<summary>The .jpg; the sidecar is the same name with .txt.</summary>
		public string PathImage { get; set; }="";
		public long PatNum { get; set; }
		public string PatientName { get; set; }="";
		public long MountNum { get; set; }
		public long MountItemNum { get; set; }
		public string Slot { get; set; }="";
		public bool IsFlipped { get; set; }
		public int DegreesRotated { get; set; }
		public string ToothNumbers { get; set; }="";
		public long CategoryDefNum { get; set; }
		public DateTime DateTimeTaken { get; set; }
		public string Reason { get; set; }="";

		public string PathSidecar => Path.ChangeExtension(PathImage,".txt");
	}

	///<summary>The camera session's quarantine (owner imaging design, camera and video, section 6, "Photo could not be saved"; IMG-P4b-3): a photo
	///whose save failed is written as a JPEG with a text sidecar to %ProgramData%\OpenPMS-Dental\Imaging\Photos\unsaved, the protected Imaging
	///folder of capture.ini and the X-ray capture quarantine, and is filed later (Camera, Unsaved photos) to the patient it was taken for: into
	///its slot when that is still empty, else unmounted. The X-ray quarantine stays as it is; its recovery reads radiographs only.</summary>
	public static class PhotoQuarantine {
		public const string Subfolder=@"Photos\unsaved";
		private const string SidecarHeader="Open PMS - Dental camera session: a photo that could not be saved to the patient's images.";

		///<summary>The folder, or null when the Imaging folder is unknown.</summary>
		public static string GetFolder() {
			string pathConfig=LocalCaptureConfig.DefaultPath;
			return pathConfig==null ? null : Path.Combine(Path.GetDirectoryName(pathConfig),Subfolder);
		}

		///<summary>Writes the photo and its sidecar. Returns the image path, or null with error. Never throws.</summary>
		public static string Write(Bitmap bitmap,PhotoUnsaved photoUnsaved,out string error) {
			error="";
			try {
				string errorFolder;
				if(!LocalCaptureConfig.EnsureImagingFolder(Subfolder,out errorFolder)) {
					error="The folder for unsaved photos is not usable. "+errorFolder;
					return null;
				}
				return WriteToFolder(GetFolder(),bitmap,photoUnsaved,out error);
			}
			catch(Exception ex) {
				error=ex.Message;
				return null;
			}
		}

		///<summary>Write into folder (tests use a folder of their own).</summary>
		public static string WriteToFolder(string folder,Bitmap bitmap,PhotoUnsaved photoUnsaved,out string error) {
			error="";
			try {
				Directory.CreateDirectory(folder);
				string baseName=photoUnsaved.DateTimeTaken.ToString("yyyyMMdd-HHmmss-fff",CultureInfo.InvariantCulture)+"_P"
					+photoUnsaved.PatNum.ToString(CultureInfo.InvariantCulture);
				string pathImage=Path.Combine(folder,baseName+".jpg");
				for(int i=2;File.Exists(pathImage);i++) {
					pathImage=Path.Combine(folder,baseName+"_"+i.ToString(CultureInfo.InvariantCulture)+".jpg");
				}
				ImageCodecInfo imageCodecInfo=ImageCodecInfo.GetImageEncoders().First(x => x.MimeType=="image/jpeg");
				using(EncoderParameters encoderParameters=new EncoderParameters(1)) {
					encoderParameters.Param[0]=new EncoderParameter(System.Drawing.Imaging.Encoder.Quality,100L);//as ImageStore.Import saves a photo
					bitmap.Save(pathImage,imageCodecInfo,encoderParameters);
				}
				photoUnsaved.PathImage=pathImage;
				File.WriteAllText(photoUnsaved.PathSidecar,BuildSidecar(photoUnsaved),Encoding.UTF8);
				return pathImage;
			}
			catch(Exception ex) {
				error=ex.Message;
				return null;
			}
		}

		public static string BuildSidecar(PhotoUnsaved photoUnsaved) {
			StringBuilder stringBuilder=new StringBuilder();
			stringBuilder.AppendLine(SidecarHeader);
			void Add(string key,string value) {
				stringBuilder.AppendLine(key+"="+(value??"").Replace("\r"," ").Replace("\n"," "));
			}
			Add("PatNum",photoUnsaved.PatNum.ToString(CultureInfo.InvariantCulture));
			Add("Patient",photoUnsaved.PatientName);
			Add("MountNum",photoUnsaved.MountNum.ToString(CultureInfo.InvariantCulture));
			Add("MountItemNum",photoUnsaved.MountItemNum.ToString(CultureInfo.InvariantCulture));
			Add("Slot",photoUnsaved.Slot);
			Add("IsFlipped",photoUnsaved.IsFlipped ? "1" : "0");
			Add("DegreesRotated",photoUnsaved.DegreesRotated.ToString(CultureInfo.InvariantCulture));
			Add("ToothNumbers",photoUnsaved.ToothNumbers);
			Add("CategoryDefNum",photoUnsaved.CategoryDefNum.ToString(CultureInfo.InvariantCulture));
			Add("Taken",photoUnsaved.DateTimeTaken.ToString("yyyy-MM-dd HH:mm:ss",CultureInfo.InvariantCulture));
			Add("Computer",Environment.MachineName);
			Add("Reason",photoUnsaved.Reason);
			return stringBuilder.ToString();
		}

		///<summary>The sidecar's fields, or null when it is not one of these sidecars.</summary>
		public static PhotoUnsaved ParseSidecar(string text,string pathImage) {
			if(text==null || !text.StartsWith(SidecarHeader,StringComparison.Ordinal)) {
				return null;
			}
			Dictionary<string,string> dict=new Dictionary<string,string>();
			foreach(string line in text.Replace("\r\n","\n").Split('\n')) {
				int idx=line.IndexOf('=');
				if(idx>0) {
					dict[line.Substring(0,idx)]=line.Substring(idx+1);
				}
			}
			string Get(string key) {
				return dict.TryGetValue(key,out string value) ? value : "";
			}
			long.TryParse(Get("PatNum"),NumberStyles.Integer,CultureInfo.InvariantCulture,out long patNum);
			long.TryParse(Get("MountNum"),NumberStyles.Integer,CultureInfo.InvariantCulture,out long mountNum);
			long.TryParse(Get("MountItemNum"),NumberStyles.Integer,CultureInfo.InvariantCulture,out long mountItemNum);
			int.TryParse(Get("DegreesRotated"),NumberStyles.Integer,CultureInfo.InvariantCulture,out int degreesRotated);
			long.TryParse(Get("CategoryDefNum"),NumberStyles.Integer,CultureInfo.InvariantCulture,out long categoryDefNum);
			DateTime.TryParseExact(Get("Taken"),"yyyy-MM-dd HH:mm:ss",CultureInfo.InvariantCulture,DateTimeStyles.None,out DateTime dateTimeTaken);
			if(patNum<=0) {
				return null;
			}
			return new PhotoUnsaved {
				PathImage=pathImage,PatNum=patNum,PatientName=Get("Patient"),MountNum=mountNum,MountItemNum=mountItemNum,Slot=Get("Slot"),
				IsFlipped=Get("IsFlipped")=="1",DegreesRotated=degreesRotated,ToothNumbers=Get("ToothNumbers"),CategoryDefNum=categoryDefNum,
				DateTimeTaken=dateTimeTaken,Reason=Get("Reason"),
			};
		}

		///<summary>The unsaved photos in folder (null: the default folder), oldest first. Never throws.</summary>
		public static List<PhotoUnsaved> List(string folder=null) {
			List<PhotoUnsaved> listPhotos=new List<PhotoUnsaved>();
			try {
				folder=folder??GetFolder();
				if(folder==null || !Directory.Exists(folder)) {
					return listPhotos;
				}
				foreach(string pathImage in Directory.GetFiles(folder,"*.jpg").OrderBy(x => x,StringComparer.OrdinalIgnoreCase)) {
					string pathSidecar=Path.ChangeExtension(pathImage,".txt");
					if(!File.Exists(pathSidecar)) {
						continue;
					}
					PhotoUnsaved photoUnsaved=ParseSidecar(File.ReadAllText(pathSidecar),pathImage);
					if(photoUnsaved!=null) {
						listPhotos.Add(photoUnsaved);
					}
				}
			}
			catch(Exception ex) {
				ex.DoNothing();
			}
			return listPhotos;
		}

		///<summary>Files one unsaved photo to its patient, with the time it was taken: into its slot when the slot is still empty and its mount
		///belongs to the same patient, else unmounted in the same category; then removes the files. isFiled: a document was made (the files are
		///then removed). Returns "" when filed as planned; otherwise why not, or a warning when it was filed but not placed.</summary>
		public static string FileOne(IPhotoStore store,PhotoUnsaved photoUnsaved,out bool isFiled) {
			isFiled=false;
			string warning="";
			Bitmap bitmap;
			try {
				using(Bitmap bitmapFile=new Bitmap(photoUnsaved.PathImage)) {
					bitmap=new Bitmap(bitmapFile);//not locking the file
				}
			}
			catch(Exception ex) {
				return "The photo file could not be read: "+ex.Message;
			}
			using(bitmap) {
				PhotoShotPlan plan=new PhotoShotPlan { MountItemNum=0,IsFlipped=photoUnsaved.IsFlipped,DegreesRotated=photoUnsaved.DegreesRotated,
					ToothNumbers=photoUnsaved.ToothNumbers??"",GuideLabel=photoUnsaved.Slot??"",DateTimeTaken=photoUnsaved.DateTimeTaken };
				if(photoUnsaved.MountNum!=0 && photoUnsaved.MountItemNum!=0) {
					PhotoMountState photoMountState=store.GetMountState(photoUnsaved.MountNum);
					//Only the patient's own mount: a deleted mount's number could be given again to another patient's mount.
					PhotoSlot slot=photoMountState==null || photoMountState.Mount.PatNum!=photoUnsaved.PatNum ? null
						: photoMountState.ListSlots.FirstOrDefault(x => x.MountItemNum==photoUnsaved.MountItemNum);
					if(slot!=null && !slot.IsFilled) {
						plan.MountItemNum=slot.MountItemNum;
					}
				}
				PhotoSaveResult result=PhotoSaver.Save(store,bitmap,plan,photoUnsaved.PatNum,photoUnsaved.CategoryDefNum);
				if(result.Document==null) {
					return result.Error;
				}
				isFiled=true;
				warning=result.IsPlaced ? "" : result.Error;//filed, but its slot, flip, rotation, teeth or time could not be set
			}
			try {
				System.IO.File.Delete(photoUnsaved.PathImage);
				System.IO.File.Delete(photoUnsaved.PathSidecar);
			}
			catch(Exception ex) {
				return "Filed, but the files could not be removed (file them again only after checking the patient's images): "+ex.Message;
			}
			return warning;
		}
	}
}
