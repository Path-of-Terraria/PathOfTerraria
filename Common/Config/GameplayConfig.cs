using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace PathOfTerraria.Common.Config;

public sealed class GameplayConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ClientSide;

	[Header("Gameplay")]
	[DefaultValue(false)]
	public bool NearbyAuras { get; set; } = false;
}