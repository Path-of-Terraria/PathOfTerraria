global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using System;
global using Terraria;
global using Terraria.ModLoader;

namespace FunnyExperience
{
	public class FunnyExperience : Mod
	{
		public static FunnyExperience Instance;
		public static string ModName = "FunnyExperience";

		public FunnyExperience()
		{
			Instance = this;
		}
	}
}