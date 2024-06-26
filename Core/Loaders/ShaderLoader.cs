﻿using System.Linq;
using System.Reflection;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader.Core;
using static Terraria.ModLoader.Core.TmodFile;

namespace PathOfTerraria.Core.Loaders;

class ShaderLoader : ILoadable
{
	public void Load(Mod mod)
	{
		if (Main.dedServ)
		{
			return;
		}

		MethodInfo info = typeof(Mod).GetProperty("File", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true);
		var file = (TmodFile)info.Invoke(mod, null);

		System.Collections.Generic.IEnumerable<FileEntry> shaders = file.Where(n => n.Name.StartsWith("Effects/") && n.Name.EndsWith(".xnb"));

		foreach (FileEntry entry in shaders)
		{
			string name = entry.Name.Replace(".xnb", "").Replace("Effects/", "");
			string path = entry.Name.Replace(".xnb", "");
			LoadShader(name, path);
		}
	}

	public void Unload()
	{

	}

	private static void LoadShader(string name, string path)
	{
		var screenRef = new Ref<Effect>(PathOfTerraria.Instance.Assets.Request<Effect>(path, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value);
		Filters.Scene[name] = new Filter(new ScreenShaderData(screenRef, name + "Pass"), EffectPriority.High);
		Filters.Scene[name].Load();
	}
}