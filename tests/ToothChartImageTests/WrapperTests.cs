// Tests of the small wrapper and relay changes of package TC-0: the graphics device exception test (gone with the DirectX chart in TC-9a) and
// the relay's perio mode setter.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDentalBusiness;
using SparksToothChart;

namespace ToothChartImageTests {
	[TestClass]
	public class WrapperTests {
		[TestMethod]
		public void GraphicsDeviceException_WentWithTheDirectXChart() {
			//TC-0 added IsGraphicsDeviceException for SharpDX's exception, which the DirectX chart threw when its device was lost; TC-9a deleted it with
			//that chart, and the Chart module's Save to Images no longer catches it (CallerSourceTests). SparksToothChart references no SharpDX.
			Assert.IsNull(typeof(ToothChartWrapper).GetMethod("IsGraphicsDeviceException",BindingFlags.Public|BindingFlags.Static));
			Assert.IsFalse(typeof(ToothChartWrapper).Assembly.GetReferencedAssemblies().Any(x => x.Name.StartsWith("SharpDX")));
		}

		[TestMethod]
		public void Relay_IsPerioMode_SetsTheValueItIsGiven() {
			StaRunner.Run(() => {
				using ToothChartWrapper wrapper=new ToothChartWrapper();
				Assert.AreEqual(DrawingMode.Simple2D,wrapper.DrawMode);
				ToothChartRelay relay=new ToothChartRelay();
				relay.SetToothChartWrapper(wrapper);
				//Until TC-0 the setter always set true, so turning perio mode off threw "Only allowed in DirectX" outside DirectX.
				relay.IsPerioMode=false;
				Assert.IsFalse(wrapper.PerioMode);
				wrapper.TcData.PerioMode=true;//as a DirectX chart in perio mode has it
				relay.IsPerioMode=false;
				Assert.IsFalse(wrapper.PerioMode,"false turns perio mode off.");
				//Turning it on is still the wrapper's rule: since TC-7 the WPF chart (and the DirectX chart until TC-9a); since TC-8 the message names the
				//3D chart, which the graphical perio chart asks for on every computer (O-31).
				Exception ex=Expect.Throws<Exception>(() => relay.IsPerioMode=true);
				StringAssert.Contains(ex.Message,"needs the 3D tooth chart");
				Assert.IsFalse(wrapper.PerioMode);
			});
		}
	}
}
