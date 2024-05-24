using PathOfTerraria.Core.Systems.TreeSystem;
using System.Linq;
using Terraria.Audio;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Core.Systems
{
	public class AltUseSystem : ModPlayer
	{
		public int AltFunctionCooldown = 0;

		public override void ResetEffects()
		{
			if (AltFunctionCooldown > 0)
				AltFunctionCooldown--;
		}
	}
}