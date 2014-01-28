// Copyright (C) by Upvoid Studios
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>

using System;
using System.IO;

using Engine;
using Engine.Input;
using Engine.Universe;
using Engine.Rendering;
using Engine.Modding;
using Engine.Resources;
using Engine.Scripting;
using Engine.Webserver;
using Engine.Network;

using Newtonsoft.Json;

namespace UpvoidMiner
{
	public static class Settings
    {
		public static void InitSettingsHandlers()
		{
			Webserver.DefaultWebserver.RegisterDynamicContent(UpvoidMiner.ModDomain, "Settings", webSettings);
		}

		[Serializable]
		class SettingsInfo
		{
			public bool afterImage;
			public bool bloom;
			public bool lensFlares;
			public bool noise;
			public bool ssao;
			public bool tonemapping;
			public bool volumetricScattering;
			public bool shadows;
			public bool fog;
		}

		static void webSettings(WebRequest request, WebResponse response)
		{
			// Handle 'Apply' request from the gui
			if (request.GetQuery("applySettings") != "")
				applySettings(request);
			else // If no apply action was sent, return the current settings in json format
				getSettings(response);
		}

		static void applySettings(WebRequest request)
		{
			Scripting.SetUserSetting("Graphics/Enable Lensflares", Boolean.Parse(request.GetQuery("lensFlares")));
			Scripting.SetUserSetting("Graphics/Enable Volumetric Scattering", Boolean.Parse(request.GetQuery("volumetricScattering")));
			Scripting.SetUserSetting("Graphics/Enable Bloom", Boolean.Parse(request.GetQuery("bloom")));
			Scripting.SetUserSetting("Graphics/Enable AfterImage", Boolean.Parse(request.GetQuery("afterImage")));
			Scripting.SetUserSetting("Graphics/Enable Tonemapping", Boolean.Parse(request.GetQuery("tonemapping")));
			Scripting.SetUserSetting("Graphics/Enable Noise", Boolean.Parse(request.GetQuery("noise")));
			Scripting.SetUserSetting("Graphics/Enable Shadows", Boolean.Parse(request.GetQuery("shadows")));
			Scripting.SetUserSetting("Graphics/Enable SSAO", Boolean.Parse(request.GetQuery("ssao")));
			Scripting.SetUserSetting("Graphics/Enable Fog", Boolean.Parse(request.GetQuery("fog")));
		}

		static void getSettings(WebResponse response)
		{
			SettingsInfo info = new SettingsInfo();

			info.lensFlares = Scripting.GetUserSetting("Graphics/Enable Lensflares", false);
			info.volumetricScattering = Scripting.GetUserSetting("Graphics/Enable Volumetric Scattering", true);
			info.bloom = Scripting.GetUserSetting("Graphics/Enable Bloom", true);
			info.afterImage = Scripting.GetUserSetting("Graphics/Enable AfterImage", true);
			info.tonemapping = Scripting.GetUserSetting("Graphics/Enable Tonemapping", true);
			info.noise = Scripting.GetUserSetting("Graphics/Enable Noise", false);
			info.shadows = Scripting.GetUserSetting("Graphics/Enable Shadows", true);
			info.ssao = Scripting.GetUserSetting("Graphics/Enable SSAO", true);
			info.fog = Scripting.GetUserSetting("Graphics/Enable Fog", true);

			StringWriter writer = new StringWriter();
			JsonSerializer json = new JsonSerializer();
			JsonTextWriter jsonWriter = new JsonTextWriter(writer);
			json.Formatting = Formatting.Indented;
			json.Serialize(jsonWriter, info);
			response.AddHeader("Content-Type", "application/json");
			response.AppendBody(writer.GetStringBuilder().ToString());
		}
    }
}

