using System.Collections.Generic;
using Terraria.IO;
using Terraria.Localization;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.World;

internal abstract class AutoGenStep : ModType
{
	public static List<AutoGenStep> Steps = [];

	public virtual string GenTitle => Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.{Name}");

	protected override void Register()
	{
		Steps.Add(this);
		ModTypeLookup<AutoGenStep>.Register(this);

		Language.GetOrRegister($"Mods.{PoTMod.ModName}.Generation.{Name}", () => "");
	}

	public abstract int GenIndex(List<GenPass> tasks);
	public abstract void Generate(GenerationProgress progress, GameConfiguration config);
}
