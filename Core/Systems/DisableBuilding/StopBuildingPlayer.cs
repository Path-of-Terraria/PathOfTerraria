using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.ID;

namespace PathOfTerraria.Core.Systems.DisableBuilding;

internal class StopBuildingPlayer : ModPlayer
{
	/// <summary>
	/// Stops the player from building if true. This is reset every frame.
	/// </summary>
	public bool ConstantStopBuilding = false;

	public override void Load()
	{
		IL_Player.PickTile += DisableMining;
	}

	private static void DisableMining(ILContext il)
	{
		ILCursor c = new(il);
		ILLabel label = c.DefineLabel();

		c.Emit(OpCodes.Ldarg_0);
		c.EmitDelegate((Player player) => player.GetModPlayer<StopBuildingPlayer>().ConstantStopBuilding);
		c.Emit(OpCodes.Brfalse, label);
		c.Emit(OpCodes.Ret);
		c.MarkLabel(label);
	}

	public override void ResetEffects()
	{
		ConstantStopBuilding = false;
	}

	public override bool CanUseItem(Item item)
	{
		if (item.createTile >= TileID.Dirt || item.createWall > WallID.None)
		{
			return !ConstantStopBuilding;
		}

		return true;
	}
}
