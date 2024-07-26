using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Swords;

public abstract class PhasebladeBase : VanillaClone
{
	public override void SetDefaults()
	{
		ItemType = Core.ItemType.Melee;
	}

	public override void HoldItem(Player player)
	{
		base.HoldItem(player);

		if (player.ItemAnimationActive)
		{
			Lighting.AddLight(player.Center, GetRGB(VanillaItemId));
		}
	}

	public static Vector3 GetRGB(int typeToMimick)
	{
		float r = 0.5f;
		float g = 0.5f;
		float b = 0.5f;

		if (typeToMimick == ItemID.BluePhaseblade || typeToMimick == ItemID.BluePhasesaber)
		{
			r *= 0.1f;
			g *= 0.5f;
			b *= 1.2f;
		}
		else if (typeToMimick == ItemID.RedPhaseblade || typeToMimick == ItemID.RedPhasesaber)
		{
			r *= 1f;
			g *= 0.2f;
			b *= 0.1f;
		}
		else if (typeToMimick == ItemID.GreenPhaseblade || typeToMimick == ItemID.GreenPhasesaber)
		{
			r *= 0.1f;
			g *= 1f;
			b *= 0.2f;
		}
		else if (typeToMimick == ItemID.PurplePhaseblade || typeToMimick == ItemID.PurplePhasesaber)
		{
			r *= 0.8f;
			g *= 0.1f;
			b *= 1f;
		}
		else if (typeToMimick == ItemID.WhitePhaseblade || typeToMimick == ItemID.WhitePhasesaber)
		{
			r *= 0.8f;
			g *= 0.9f;
			b *= 1f;
		}
		else if (typeToMimick == ItemID.YellowPhaseblade || typeToMimick == ItemID.YellowPhasesaber)
		{
			r *= 0.8f;
			g *= 0.8f;
			b *= 0f;
		}
		else if (typeToMimick == ItemID.OrangePhaseblade || typeToMimick == ItemID.OrangePhasesaber)
		{
			r *= 0.9f;
			g *= 0.5f;
			b *= 0f;
		}

		return new Vector3(r, g, b);
	}
}

public class BluePhaseblade : PhasebladeBase
{
	protected override short VanillaItemId => ItemID.BluePhaseblade;
}

public class GreenPhaseblade : PhasebladeBase
{
	protected override short VanillaItemId => ItemID.GreenPhaseblade;
}

public class RedPhaseblade : PhasebladeBase
{
	protected override short VanillaItemId => ItemID.RedPhaseblade;
}

public class YellowPhaseblade : PhasebladeBase
{
	protected override short VanillaItemId => ItemID.YellowPhaseblade;
}

public class PurplePhaseblade : PhasebladeBase
{
	protected override short VanillaItemId => ItemID.PurplePhaseblade;
}

public class WhitePhaseblade : PhasebladeBase
{
	protected override short VanillaItemId => ItemID.WhitePhaseblade;
}

public class OrangePhaseblade : PhasebladeBase
{
	protected override short VanillaItemId => ItemID.OrangePhaseblade;
}

// Sabers
public class BluePhasesaber : PhasebladeBase
{
	protected override short VanillaItemId => ItemID.BluePhasesaber;
}

public class GreenPhasesaber : PhasebladeBase
{
	protected override short VanillaItemId => ItemID.GreenPhasesaber;
}

public class RedPhasesaber : PhasebladeBase
{
	protected override short VanillaItemId => ItemID.RedPhasesaber;
}

public class YellowPhasesaber : PhasebladeBase
{
	protected override short VanillaItemId => ItemID.YellowPhasesaber;
}

public class PurplePhasesaber : PhasebladeBase
{
	protected override short VanillaItemId => ItemID.PurplePhasesaber;
}

public class WhitePhasesaber : PhasebladeBase
{
	protected override short VanillaItemId => ItemID.WhitePhasesaber;
}

public class OrangePhasesaber : PhasebladeBase
{
	protected override short VanillaItemId => ItemID.OrangePhasesaber;
}