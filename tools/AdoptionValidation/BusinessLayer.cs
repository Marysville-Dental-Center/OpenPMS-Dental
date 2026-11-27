// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenPmsDentalBusiness;
using SyntheticData.Infrastructure;

namespace AdoptionValidation {
	///<summary>Points OpenPmsDentalBusiness at one database and runs the program's own adoption, migration and verifier code against it, exactly
	///as the program does (Adoption.Run is what FormAdoptDatabase calls). Nothing here re-implements those steps.</summary>
	public static class BusinessLayer {
		///<summary>Open Dental's tables with a SecurityHash column. This program keeps the values as they are and neither computes nor checks them
		///(NOHASH); adoption must leave them unchanged.</summary>
		public static readonly string[] ArrayHashedTables={ "appointment","claim","claimproc","inssub","insverify","insverifyhist","patient","payplan","paysplit" };

		///<summary>Connects the business layer (DataCore, Db, the caches) directly to the database. Local server only, on the port the command line
		///gives (through an SSH local forward, runbook step 0.0, that port is the database server's MariaDB). The local test server on port 3306
		///(or a test's relay to it) is reached with TLS and verified against its CA once tools/Enable-LocalDbTls.ps1 has run (DevDbTls); a
		///forward's port as before.</summary>
		public static void Connect(ServerSettings server,string database) {
			LocalOnly.EnsureLocalHost(server.Server);
			LocalOnly.EnsureSafeIdentifier(database);
			RemotingClient.MiddleTierRole=MiddleTierRole.ClientDirect;
			string serverAndPort=ServerAndPort(server);
			new DataConnection().SetDb(serverAndPort,database,server.User,server.Password,"","",DatabaseType.MySql,true,DevDbTls.GetSslCa(serverAndPort));
			if(Security.CurUser==null) {
				Security.CurUser=new Userod();//Adoption runs before anyone logs in; a few log helpers read the current user.
			}
		}

		///<summary>The server as DataConnection takes it, "host:port". DataConnection reads the port only from this string and otherwise uses 3306, so
		///without it the business layer would connect to a different server than the tools' own connections whenever --port is not 3306 (an SSH local
		///forward). An IPv6 address cannot carry a port there, so it is refused.</summary>
		public static string ServerAndPort(ServerSettings server) {
			LocalOnly.EnsureLocalHost(server.Server);
			string host=server.Server.Trim();
			if(host.Contains(":")) {
				throw new ArgumentException("Connect with --server 127.0.0.1 or localhost: the business layer cannot take a port with the IPv6 address "+host+".");
			}
			return host+":"+server.Port.ToString(System.Globalization.CultureInfo.InvariantCulture);
		}

		public static string CurrentDatabase() {
			return DataCore.GetScalar("SELECT DATABASE()");
		}

		///<summary>A preference straight from the table, or null when there is no row.</summary>
		public static string GetPref(string prefName) {
			return ForkVersions.GetMarker(prefName);
		}

		public static long Count(string command) {
			return PIn.Long(DataCore.GetScalar(command));
		}

		public static bool ColumnExists(string table,string column) {
			return Count("SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='"+POut.String(table)+"' "
				+"AND COLUMN_NAME='"+POut.String(column)+"'")>0;
		}

		public static bool IndexExists(string table,string index) {
			return Count("SELECT COUNT(*) FROM information_schema.STATISTICS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='"+POut.String(table)+"' "
				+"AND INDEX_NAME='"+POut.String(index)+"'")>0;
		}

		///<summary>Indexes that include the given column.</summary>
		public static List<string> IndexesOnColumn(string table,string column) {
			DataTable dataTable=DataCore.GetTable("SELECT DISTINCT INDEX_NAME FROM information_schema.STATISTICS WHERE TABLE_SCHEMA=DATABASE() "
				+"AND TABLE_NAME='"+POut.String(table)+"' AND COLUMN_NAME='"+POut.String(column)+"'");
			return dataTable.Rows.Cast<DataRow>().Select(x => x[0].ToString()).ToList();
		}

		///<summary>For each table with a SecurityHash column: its rows, how many values are blank, and a checksum of the values, so a run can
		///show that adoption left every value as it was.</summary>
		public static List<HashCount> CountHashes() {
			List<HashCount> listCounts=new List<HashCount>();
			foreach(string table in ArrayHashedTables) {
				if(!ColumnExists(table,"SecurityHash")) {
					continue;
				}
				DataTable dataTable=DataCore.GetTable("SELECT COUNT(*),IFNULL(SUM(SecurityHash=''),0),"
					+"IFNULL(SUM(CAST(CONV(SUBSTRING(MD5(SecurityHash),1,8),16,10) AS UNSIGNED)),0) FROM `"+table+"`");
				DataRow row=dataTable.Rows[0];
				listCounts.Add(new HashCount {
					Table=table,
					Rows=PIn.Long(row[0].ToString()),
					Blank=PIn.Long(row[1].ToString()),
					Checksum=row[2].ToString(),
				});
			}
			return listCounts;
		}
	}

	public class HashCount {
		public string Table;
		public long Rows;
		public long Blank;
		///<summary>The sum of the first 32 bits of each value's MD5: equal before and after when no value changed.</summary>
		public string Checksum="";

		public override string ToString() {
			return Table+": "+Rows+" rows, "+(Rows-Blank)+" with a SecurityHash value, "+Blank+" blank, checksum "+Checksum;
		}
	}
}
