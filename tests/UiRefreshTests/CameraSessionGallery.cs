// Pictures of the camera session in the Imaging module (IMG-P4b, owner imaging design, camera and video) for the owner's gallery. Opt-in:
// OPMS_IMAGE_VIEWER_GALLERY names the folder. The session is filled with a synthetic live picture and photos through its private fields and a
// store of its own: no camera, no program, no database. Licensed under the GNU General Public License, version 2 or later. See LICENSE in the
// repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using ImagingCapture.Photo;
using ImagingCapture.Templates;
using ImagingCapture.Video;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDentalBusiness;
using UiRefreshTests.Scan;

namespace UiRefreshTests {
	[TestClass]
	public class CameraSessionGallery {
		private const string VARIABLE_FOLDER="OPMS_IMAGE_VIEWER_GALLERY";

		[TestMethod]
		public void Gallery_CameraSession() {
			string folder=Environment.GetEnvironmentVariable(VARIABLE_FOLDER);
			if(string.IsNullOrEmpty(folder)) {
				Assert.Inconclusive("Opt-in: "+VARIABLE_FOLDER+" names the folder for the camera session pictures.");
			}
			Directory.CreateDirectory(folder);
			List<string> listFailed=new List<string>();
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				DesignSheetWindows.IsPrintingWithoutChildren=true;
				Save(folder,"camera-intraoral5-live",listFailed,StandardTemplateKind.PhotoIntraoral5,countFilled:3,idxNext:3,VideoStatusText.Streaming(),
					"Saved: Left buccal");
				Save(folder,"camera-ortho8-start",listFailed,StandardTemplateKind.PhotoOrtho8,countFilled:0,idxNext:0,VideoStatusText.Streaming(),"");
				Save(folder,"camera-intraoral5-complete",listFailed,StandardTemplateKind.PhotoIntraoral5,countFilled:5,idxNext:-1,VideoStatusText.Streaming(),
					"Saved: Lower occlusal");
				Save(folder,"camera-quick-photos",listFailed,null,countFilled:4,idxNext:-1,VideoStatusText.Streaming(),"Photo saved");
				Save(folder,"camera-no-camera",listFailed,StandardTemplateKind.PhotoIntraoral5,countFilled:0,idxNext:0,VideoStatusText.NoCamera(),"");
				Save(folder,"camera-in-use",listFailed,StandardTemplateKind.PhotoBeforeAfter,countFilled:1,idxNext:1,
					new VideoStatus(VideoStatusKind.InUse,VideoStatusText.TextInUse,"",VideoStatusAction.Refresh),"");
				Save(folder,"camera-disconnected",listFailed,StandardTemplateKind.PhotoIntraoral5,countFilled:2,idxNext:2,
					new VideoStatus(VideoStatusKind.Disconnected,VideoStatusText.TextDisconnected,"",VideoStatusAction.Reconnect),"");
				Save(folder,"camera-intraoral5-frozen",listFailed,StandardTemplateKind.PhotoIntraoral5,countFilled:3,idxNext:3,VideoStatusText.Streaming(),
					"Frozen: Capture saves it, F goes live.",isFrozen:true);
				Save(folder,"camera-recording",listFailed,null,countFilled:2,idxNext:-1,VideoStatusText.Streaming(),
					"Recording. R stops, P pauses, Space still takes a photo.",recording:"\u25CF REC 1:47 / 2:00  stops at 2:00");
				Save(folder,"camera-asleep",listFailed,StandardTemplateKind.PhotoIntraoral5,countFilled:2,idxNext:2,VideoStatusText.Streaming(),"",isAsleep:true);
				Save(folder,"camera-unsaved",listFailed,StandardTemplateKind.PhotoIntraoral5,countFilled:2,idxNext:2,VideoStatusText.Streaming(),"",
					alert:"Photo could not be saved. It is kept on this computer; Camera, Unsaved photos files it later.\r\nThe photo could not be saved: the A to Z folder is not reachable.");
			});
			DesignSheetWindows.IsPrintingWithoutChildren=false;
			File.WriteAllLines(Path.Combine(folder,"gallery-camera-failed.txt"),listFailed);
			Assert.AreEqual(0,listFailed.Count,string.Join("; ",listFailed));
		}

		///<summary>A store holding one photo mount of a template, its first countFilled slots filled.</summary>
		private sealed class GalleryStore:IPhotoStore {
			public PhotoMountState PhotoMountState;

			public List<MountDef> GetPhotoMountDefs() {
				return new List<MountDef>();
			}

			public Mount CreateMount(MountDef mountDef,long patNum,long categoryDefNum) {
				throw new NotSupportedException();
			}

			public PhotoMountState GetMountState(long mountNum) {
				return PhotoMountState;
			}

			public Document ImportPhoto(Bitmap bitmap,long categoryDefNum,long patNum) {
				throw new NotSupportedException();
			}

			public void UpdateDocument(Document document,Document documentOld) {
				throw new NotSupportedException();
			}

			public Document GetDocument(long docNum) {
				return null;
			}

			public void MoveToRetake(Document document,long mountNum,string slotLabel,long patNum) {
				throw new NotSupportedException();
			}

			public void DeleteDocument(Document document,long patNum) {
				throw new NotSupportedException();
			}

			public void LogEdit(long patNum,string logText,long docNum) {
			}

			public Document ImportVideo(string pathVideo,long categoryDefNum,long patNum) {
				throw new NotSupportedException();
			}
		}

		private static void Save(string folder,string name,List<string> listFailed,StandardTemplateKind? kind,int countFilled,int idxNext,
			VideoStatus videoStatus,string info,bool isFrozen=false,bool isAsleep=false,string alert="",string recording="")
		{
			try {
				GalleryStore store=new GalleryStore();
				Mount mount=null;
				if(kind!=null) {
					StandardTemplate template=StandardMountTemplates.Get(kind.Value);
					mount=new Mount { MountNum=1,Description=template.Description,Width=template.Width,Height=template.Height,DocCategory=5,
						DateCreated=new DateTime(2026,11,23) };
					store.PhotoMountState=new PhotoMountState {
						Mount=mount,
						ListSlots=template.ListSlots.Select((x,i) => new PhotoSlot { MountItemNum=10+i,ItemOrder=x.ItemOrder,Xpos=x.Xpos,Ypos=x.Ypos,
							Width=x.Width,Height=x.Height,ToothNumbers=x.ToothNumbers,GuideLabel=x.GuideLabel,IsMirror=x.IsMirror,SlotKind=x.SlotKind,
							IsFilled=i<countFilled }).ToList(),
					};
				}
				Patient patient=new Patient { PatNum=10442,LName="Test",FName="Sensor",Preferred="",MiddleI="",Birthdate=new DateTime(1980,1,1),ChartNumber="10442" };
				using ControlCameraSession controlCameraSession=new ControlCameraSession(store,x => x);
				controlCameraSession.Size=new Size(1100,640);
				controlCameraSession.FuncDisplayTeeth=x => x.Length>12 ? "upper arch" : x;//no database: no numbering preference
				string error=controlCameraSession.Start(patient,mount,5);
				Assert.AreEqual("",error);
				PhotoSessionModel model=controlCameraSession.Model;
				if(idxNext>=0) {
					model.SelectSlot(idxNext);
				}
				//the photos taken so far: thumbnails in their slots, or the quick photos
				Dictionary<long,Bitmap> dictThumbs=(Dictionary<long,Bitmap>)GetField(controlCameraSession,"_dictThumbs");
				List<Bitmap> listThumbsQuick=(List<Bitmap>)GetField(controlCameraSession,"_listThumbsQuick");
				if(model.IsQuickPhotos) {
					for(int i=0;i<countFilled;i++) {
						listThumbsQuick.Add(MakePhoto(160,120,i));
						model.MarkSaved(model.PlanShot());
					}
				}
				else {
					foreach(PhotoSlot slot in model.ListSlots.Where(x => x.IsFilled)) {
						dictThumbs[slot.MountItemNum]=ControlCameraSession.MakeThumb(MakePhoto(slot.SlotKind==MountSlotKind.Extraoral ? 120 : 160,
							slot.SlotKind==MountSlotKind.Extraoral ? 160 : 120,slot.ItemOrder),slot.IsMirror,0,160);
					}
				}
				Invoke(controlCameraSession,"RefreshGuide");
				Invoke(controlCameraSession,"ShowStatus",videoStatus);
				if(videoStatus.IsStreaming) {
					((PictureBox)GetField(controlCameraSession,"pictureLive")).Image=MakePhoto(1280,720,7);
				}
				((Label)GetField(controlCameraSession,"labelCamera")).Text=videoStatus.IsStreaming ? "Camera: USB intraoral camera\r\nPicture: 1920x1080 MJPG 30 fps"
					: "Camera: none";
				if(isFrozen) {
					controlCameraSession.GetType().GetField("_bitmapFrozen",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(controlCameraSession,MakePhoto(1280,720,7));
					Invoke(controlCameraSession,"RefreshGuide");
				}
				if(recording!="") {
					Label labelRec=(Label)GetField(controlCameraSession,"labelRec");
					labelRec.Text=recording;
					labelRec.Visible=true;
					((OpenPmsDental.UI.Button)GetField(controlCameraSession,"butRecord")).Text="Stop recording (R)";
				}
				if(alert!="") {
					Invoke(controlCameraSession,"ShowAlert",alert);
				}
				if(isAsleep) {
					controlCameraSession.Sleep();
				}
				((Label)GetField(controlCameraSession,"labelInfo")).Text=info;
				controlCameraSession.PerformLayout();
				using Bitmap bitmapPicture=DesignSheetWindows.Capture(controlCameraSession,Rectangle.Empty);
				bitmapPicture.Save(Path.Combine(folder,name+".png"),ImageFormat.Png);
				controlCameraSession.End();
			}
			catch(Exception exception) {
				listFailed.Add(name+": "+exception);
			}
		}

		///<summary>A made-up intraoral picture: pink gum, three pale teeth, a darker mouth.</summary>
		private static Bitmap MakePhoto(int width,int height,int seed) {
			Bitmap bitmap=new Bitmap(width,height);
			using Graphics g=Graphics.FromImage(bitmap);
			g.SmoothingMode=SmoothingMode.AntiAlias;
			g.Clear(Color.FromArgb(90,45,41));
			using Brush brushGum=new SolidBrush(Color.FromArgb(181,103,95));
			g.FillEllipse(brushGum,-width/6,height*2/3,width*4/3,height/2);
			g.FillEllipse(brushGum,-width/6,-height/6,width*4/3,height/3);
			using Brush brushTooth=new SolidBrush(Color.FromArgb(240-seed%3*6,232,222));
			for(int i=0;i<3;i++) {
				g.FillEllipse(brushTooth,width*(2+i*3)/12,height/4,width/5,height/2);
			}
			return bitmap;
		}

		private static object GetField(object target,string name) {
			return target.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(target);
		}

		private static void Invoke(object target,string name,params object[] arrayArgs) {
			target.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(target,arrayArgs);
		}
	}
}
