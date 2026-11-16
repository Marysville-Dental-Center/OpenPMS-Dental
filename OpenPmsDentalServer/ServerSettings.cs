// The Middle Tier server's own settings (package S3b-6 of the .NET 10 move, docs/port-notes/S3b-6.md): the section "MiddleTier" of
// appsettings.json, the command line or the environment (MiddleTier__PathBase), read once at start.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace OpenPmsDentalServer {
	///<summary>What the server reads from its configuration besides Kestrel's own section and the database settings (OpenDentalServerConfig.xml,
	///which the business layer reads on the first request, as before).</summary>
	public class ServerSettings {
		///<summary>The configuration section of these settings.</summary>
		public const string SECTION="MiddleTier";
		///<summary>The application path of the ASMX web service in IIS, which every workstation's saved address names
		///(https://server/OpenDentalServer/ServiceMain.asmx): the default, so those addresses keep working.</summary>
		public const string DEFAULT_PATH_BASE="/OpenDentalServer";
		///<summary>1 GiB: the limits of the old Web.config (httpRuntime maxRequestLength 1048576 KB, requestLimits maxAllowedContentLength
		///1073741824 bytes), since 2020 so that long Middle Tier payloads fit.</summary>
		public const long DEFAULT_MAX_REQUEST_BODY_SIZE=1073741824;

		///<summary>The path the endpoint answers under on Kestrel (the Windows service) and the test server: {PathBase}/ServiceMain.asmx and
		///{PathBase}/. Empty for an endpoint at the site root ("/" or "" in the settings). In IIS the application's virtual path is the path base
		///and this setting is not used.</summary>
		public PathString PathBase { get; private set; }
		///<summary>The largest request body the server reads, in bytes; a larger request gets 413. Kestrel's and IIS's limits are set to it too.
		///</summary>
		public long MaxRequestBodySize { get; private set; }

		///<summary>Reads the settings. Throws InvalidOperationException, with the setting's name, for a value the server cannot use.</summary>
		public static ServerSettings Read(IConfiguration configuration) {
			IConfigurationSection section=configuration.GetSection(SECTION);
			ServerSettings serverSettings=new ServerSettings();
			string pathBase=section["PathBase"];
			if(pathBase==null) {
				pathBase=DEFAULT_PATH_BASE;
			}
			pathBase=pathBase.Trim().TrimEnd('/');
			if(pathBase!="" && !pathBase.StartsWith("/")) {
				throw new InvalidOperationException(SECTION+":PathBase must start with / (for example "+DEFAULT_PATH_BASE+"), or be / for the site root: '"
					+section["PathBase"]+"'.");
			}
			serverSettings.PathBase=new PathString(pathBase);
			string maxRequestBodySize=section["MaxRequestBodySize"];
			if(string.IsNullOrWhiteSpace(maxRequestBodySize)) {
				serverSettings.MaxRequestBodySize=DEFAULT_MAX_REQUEST_BODY_SIZE;
			}
			else {
				long size;
				if(!long.TryParse(maxRequestBodySize.Trim(),NumberStyles.None,CultureInfo.InvariantCulture,out size) || size<=0) {
					throw new InvalidOperationException(SECTION+":MaxRequestBodySize must be a number of bytes greater than 0: '"+maxRequestBodySize+"'.");
				}
				serverSettings.MaxRequestBodySize=size;
			}
			return serverSettings;
		}
	}
}
