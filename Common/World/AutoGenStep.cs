using System.Collections.Generic;
using Terraria.IO;
using Terraria.Localization;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.World;

/// <summary>
/// Defines an automatic generation step to be used in <see cref="GenerationSystem"/>.
/// </summary>
internal abstract class AutoGenStep : ModType
{
	public static List<AutoGenStep> Steps = [];

	/// <summary>
	/// Autoloaded localized string for the gen step. Should be used alongside <see cref="GenerationProgress.Message"/>.
	/// </summary>
	public virtual string GenTitle => Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.{Name}");

	protected override void Register()
	{
		Steps.Add(this);
		ModTypeLookup<AutoGenStep>.Register(this);

		Language.GetOrRegister($"Mods.{PoTMod.ModName}.Generation.{Name}", () => "");
	}

	/// <summary>
	/// Index of the step.
	/// </summary>
	/// <param name="tasks">Tasks to insert into.</param>
	/// <returns>What index to insert into in <paramref name="tasks"/>. -1 means it will not be added.</returns>
	public abstract int GenIndex(List<GenPass> tasks);

	/// <summary>
	/// Actual generation code.
	/// </summary>
	/// <param name="progress">Progress.</param>
	/// <param name="config">Configruation; this is almost always unused.</param>
	public abstract void Generate(GenerationProgress progress, GameConfiguration config);
}
