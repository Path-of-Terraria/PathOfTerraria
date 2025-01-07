using System.Runtime.CompilerServices;
using Terraria.GameContent.UI.BigProgressBar;

namespace PathOfTerraria.Common.Systems.ModPlayers;

/// <summary>
/// This is used to "unset" boss bars when entering a world.<br/>
/// Otherwise, dying in a boss domain and returning before the boss despawns would have the boss bar persist;<br/>this is because
/// the bar only checks if the npc at slot X is alive, not if it's the right type.<br/>
/// It's annoying, but oh well.
/// </summary>
internal class BossBarFixer : ModPlayer
{
	public override void OnEnterWorld()
	{
		ref IBigProgressBar bar = ref GetCurrentBar(Main.BigBossProgressBar);
		bar = null;

		[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_currentBar")]
		extern static ref IBigProgressBar GetCurrentBar(BigProgressBarSystem barSystem);
	}
}
