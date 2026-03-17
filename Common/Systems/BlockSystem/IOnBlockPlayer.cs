using Terraria.ModLoader.Core;

namespace PathOfTerraria.Common.Systems.BlockSystem;

internal interface IOnBlockPlayer
{
	private static readonly HookList<ModPlayer> Hook = PlayerLoader.AddModHook(HookList<ModPlayer>.Create(i => ((IOnBlockPlayer)i).OnBlock));

	public void OnBlock(Player.HurtInfo info);

	public static void Invoke(Player player, Player.HurtInfo info)
	{
		foreach (IOnBlockPlayer g in Hook.Enumerate(player.ModPlayers))
		{
			g.OnBlock(info);
		}
	}
}
