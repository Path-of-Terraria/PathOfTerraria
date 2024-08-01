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

	/// <summary>
	/// I don't know why, but <see cref="DisableMining(ILContext)"/> doesn't work due to method order. /shrug<br/>
	/// Use this if <see cref="ConstantStopBuilding"/> doesn't work for some reason.
	/// </summary>
	public bool LastStopBuilding = false;

	public override void Load()
	{
		IL_Player.PickTile += DisableMining;
		IL_Player.PickWall += DisableMining;
		IL_Player.ItemCheck_CutTiles += DisableMining;
	}

	private static void DisableMining(ILContext il)
	{
		ILCursor c = new(il);
		ILLabel label = c.DefineLabel();

		c.Emit(OpCodes.Ldarg_0);
		c.EmitDelegate((Player player) => player.GetModPlayer<StopBuildingPlayer>().LastStopBuilding);
		c.Emit(OpCodes.Brfalse, label);
		c.Emit(OpCodes.Ret);
		c.MarkLabel(label);
	}

	public override void ResetEffects()
	{
		LastStopBuilding = ConstantStopBuilding;
		ConstantStopBuilding = false;
	}

	public override bool CanUseItem(Item item)
	{
		if (item.createTile >= TileID.Dirt || item.createWall > WallID.None || item.type == ItemID.IceRod || item.tileWand >= 0)
		{
			return !LastStopBuilding;
		}

		return true;
	}
}
