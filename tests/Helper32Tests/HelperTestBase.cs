// The checks every helper test ends with (S3a-13, docs/port-notes/S3a-13.md, section 8): no helper that the test started is left.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Helper32Tests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Helper32Tests {
	public abstract class HelperTestBase {
		private readonly List<HelperCopy> _listCopies=new List<HelperCopy>();

		///<summary>A copy of the helper for this test; deleted after the test, when its processes are gone.</summary>
		protected HelperCopy NewCopy() {
			HelperCopy helperCopy=HelperCopy.Create();
			_listCopies.Add(helperCopy);
			return helperCopy;
		}

		///<summary>None of the test's helpers is left: no process of a copy's name runs (anywhere on the machine), and no helper copy is in this
		///test process's tree. Then the copies are deleted.</summary>
		[TestCleanup]
		public void NoHelperLeft() {
			try {
				DateTime dateTimeDeadline=DateTime.UtcNow.AddSeconds(15);
				List<string> listLeft;
				while(true) {
					listLeft=_listCopies.SelectMany(x => x.GetProcesses().Select(y => x.FileName+" "+y.ProcessId))
						.Concat(ProcessList.GetInOwnTree(HelperCopy.COPY_PREFIX).Select(x => x.ExeFile+" "+x.ProcessId+" (in this test's process tree)"))
						.Distinct().ToList();
					if(listLeft.Count==0 || DateTime.UtcNow>dateTimeDeadline) {
						break;
					}
					Thread.Sleep(100);
				}
				Assert.AreEqual(0,listLeft.Count,"Helper processes left after the test: "+string.Join(", ",listLeft));
			}
			finally {
				foreach(HelperCopy helperCopy in _listCopies) {
					helperCopy.Dispose();
				}
				_listCopies.Clear();
			}
		}
	}
}
