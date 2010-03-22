﻿#region Copyright (c) Lokad 2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Lokad.Cloud
{
	[Serializable]
	public class RoleConfigurationSettings
	{
		public string DataConnectionString { get; set; }
		public string SelfManagementSubscriptionId { get; set; }
		public string SelfManagementCertificateThumbprint { get; set; }

		public static Maybe<RoleConfigurationSettings> LoadFromRoleEnvironment()
		{
			if (!CloudEnvironment.IsAvailable)
			{
				return Maybe<RoleConfigurationSettings>.Empty;
			}

			var setting = new RoleConfigurationSettings();
			ApplySettingFromRole("DataConnectionString", v => setting.DataConnectionString = v);
			ApplySettingFromRole("SelfManagementSubscriptionId", v => setting.SelfManagementSubscriptionId = v);
			ApplySettingFromRole("SelfManagementCertificateThumbprint", v => setting.SelfManagementCertificateThumbprint = v);
			
			return setting;
		}

		static void ApplySettingFromRole(string setting, Action<string> setter)
		{
			try
			{
				var value = RoleEnvironment.GetConfigurationSettingValue(setting);
				if(!string.IsNullOrEmpty(value))
				{
					value = value.Trim();
				}
				if(string.IsNullOrEmpty(value))
				{
					value = null;
				}
				setter(value);
			}
			catch (RoleEnvironmentException)
			{
				// setting was removed from the csdef, skip
				// (logging is usually not available at that stage)
			}
		}
	}
}