// The scan builds windows without a database (docs/port-notes/UI-0b.md). A window or control that reads the database while it is built must
// fail at once, never reach a server: this guard gives the process a connection string that the program's connection rule
// (DataConnectionBase\DbConnectionSecurity.CheckConnectionString) refuses before any connection is made (a server over the network without
// server verification, CA or credentials; the host is in the reserved .test domain and is never resolved). The type is then listed as needing
// a database, with the refusal as its reason. Without the guard the empty connection would go to this computer's server.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Reflection;
using DataConnectionBase;

namespace UiRefreshTests.Scan {
	public static class NoDatabaseGuard {
		///<summary>Refused by the connection rule: not this computer, no SslMode=VerifyFull, no CA, no password or certificate.</summary>
		public const string CONNECTION_STRING="Server=no-database.example.test;Port=3306;Database=ui0b_none;User ID=ui0b_none;SslMode=none";
		private static readonly object _lock=new object();
		private static bool _isInstalled;

		///<summary>Sets the guard's connection string as the process's connection (DataConnection's static ConnectionString). Once per process.</summary>
		public static void Install() {
			lock(_lock) {
				if(_isInstalled) {
					return;
				}
				PropertyInfo propertyInfo=typeof(DataConnection).GetProperty("ConnectionString",BindingFlags.Static|BindingFlags.NonPublic|BindingFlags.Public);
				if(propertyInfo==null) {
					throw new InvalidOperationException("DataConnection.ConnectionString not found: the scan's no-database guard cannot be set.");
				}
				propertyInfo.SetValue(null,CONNECTION_STRING);
				_isInstalled=true;
			}
		}

		///<summary>True when the exception, or one inside it, comes from the database layer: the guard's refusal or a driver error.</summary>
		public static bool IsDatabaseException(Exception exception) {
			for(Exception e=exception;e!=null;e=e.InnerException) {
				string name=e.GetType().Name;
				if(name=="DbConnectionRefusedException" || name.StartsWith("MySql")) {
					return true;
				}
				if(e is AggregateException aggregateException) {
					foreach(Exception inner in aggregateException.InnerExceptions) {
						if(IsDatabaseException(inner)) {
							return true;
						}
					}
				}
			}
			return false;
		}
	}
}
