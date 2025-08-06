using PathOfTerraria.Common.UI.Guide;
using PathOfTerraria.Common.World.Generation;
using System.Linq;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.ModPlayers;

/*
 * TODO: Implement gear sets:
 *		Gear sets should stop the player from wearing specific types of weapons and
 *		offhand accessories at the same time. For example, the player should not be
 *		able to hold a sword and equip a quiver, or hold a bow and equip a shield,
 *		etc.
 */
public sealed class GearSwapManager : ModPlayer
{
	/// <summary>
	///     The player's gear inventory, reserved for a weapon and an offhand accessory.
	/// </summary>
	public Item?[] Inventory = [new(ItemID.None), new(ItemID.None)];

	public override void UpdateEquips()
	{
		if (Main.mouseLeft && Main.GameUpdateCount % 10 == 0)
		{
			bool flip = WorldGen.genRand.NextBool(2);

			// Generate base points
			Vector2[] points = [new Vector2(250, 0),
				new Vector2(GenerateEdgeX(ref flip), WorldGen.genRand.Next(160, 190)),
				new Vector2(GenerateEdgeX(ref flip), WorldGen.genRand.Next(220, 250)),
				new Vector2(GenerateEdgeX(ref flip), WorldGen.genRand.Next(280, 310)),
				new Vector2(GenerateEdgeX(ref flip), WorldGen.genRand.Next(330, 350)),
				new	Vector2(250, 400)];

			Vector2[] tunnel = Tunnel.GeneratePoints(points, out Vector2[] basePoints, 30, 12, 0);

			foreach (var point in points)
			{
				Dust.NewDustPerfect(point + Main.MouseWorld, DustID.UltraBrightTorch, Vector2.Zero, 0, Scale: 2f).noGravity = true;
			}

			foreach (var point in tunnel)
			{
				Dust.NewDustPerfect(point + Main.MouseWorld, DustID.Torch, Vector2.Zero, 0, Scale: 3f).noGravity = true;
			}

			foreach (var point in basePoints)
			{
				Dust.NewDustPerfect(point + Main.MouseWorld + Vector2.UnitX * 500, DustID.CursedTorch, Vector2.Zero, 0, Scale: 3f).noGravity = true;
			}
		}

		if (Main.dedServ || !GearSwapKeybind.SwapKeybind.JustPressed || Main.LocalPlayer.itemTime > 0)
		{
			return;
		}

		Player.GetModPlayer<TutorialPlayer>().TutorialChecks.Add(TutorialCheck.SwappedWeapon);

		Item previousWeapon = Player.inventory[0];
		Item previousOffhand = Player.armor[6];

		Player.inventory[0] = Inventory[0];
		Player.armor[6] = Inventory[1];

		Inventory[0] = previousWeapon;
		Inventory[1] = previousOffhand;

		SoundEngine.PlaySound(
			SoundID.MenuTick with
			{
				Pitch = -0.25f,
				MaxInstances = 1
			}
		);
	}

	static int GenerateEdgeX(ref bool flip)
	{
		flip = !flip;
		return 250 + WorldGen.genRand.Next(70, 160) * (flip ? -1 : 1);
	}

	public override void SaveData(TagCompound tag)
	{
		tag["inventory"] = Inventory.Select(ItemIO.Save).ToList();
	}

	public override void LoadData(TagCompound tag)
	{
		Inventory = tag.GetList<TagCompound>("inventory").Select(ItemIO.Load).ToArray();
	}
}