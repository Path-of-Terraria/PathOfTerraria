using Microsoft.Xna.Framework.Input;
using PathOfTerraria.Common.Systems.Synchronization.Handlers;
using PathOfTerraria.Content.Projectiles.Utility;
using SubworldLibraryCommunityFork;
using Terraria.GameInput;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds;

internal sealed class ReturnPortalPlayer : ModPlayer
{
	private const int OpenPortalCooldownTicks = 30;

	private static ModKeybind OpenReturnPortalKeybind;

	private int openPortalCooldown;

	public override void Load()
	{
		if (!Main.dedServ)
		{
			OpenReturnPortalKeybind = KeybindLoader.RegisterKeybind(Mod, "OpenReturnPortal", Keys.K);
		}
	}

	public override void Unload()
	{
		OpenReturnPortalKeybind = null;
	}

	public override void PreUpdate()
	{
		if (openPortalCooldown > 0)
		{
			openPortalCooldown--;
		}
	}

	public override void ProcessTriggers(TriggersSet triggersSet)
	{
		if (Player.whoAmI != Main.myPlayer || OpenReturnPortalKeybind?.JustPressed != true || openPortalCooldown > 0 || !CanOpenReturnPortal(Player))
		{
			return;
		}

		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			OpenReturnPortalHandler.Send();
			openPortalCooldown = OpenPortalCooldownTicks;
			return;
		}

		TryOpenReturnPortal(Player);
	}

	internal static bool TryOpenReturnPortal(Player player)
	{
		ReturnPortalPlayer portalPlayer = player.GetModPlayer<ReturnPortalPlayer>();

		if (portalPlayer.openPortalCooldown > 0 || !CanOpenReturnPortal(player) || !ExitPortal.OpenFor(player))
		{
			return false;
		}

		portalPlayer.openPortalCooldown = OpenPortalCooldownTicks;
		return true;
	}

	private static bool CanOpenReturnPortal(Player player)
	{
		return player.active && !player.dead
			&& SubworldSystem.Current is MappingWorld { ReturnPositionMode: not SubworldReturnPositionMode.Disabled };
	}
}
