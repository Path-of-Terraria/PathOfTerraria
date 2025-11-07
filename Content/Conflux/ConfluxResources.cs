using Terraria.ID;

namespace PathOfTerraria.Content.Conflux;

internal abstract class ConfluxResource : ModItem
{
	public override void SetStaticDefaults()
	{
		ItemID.Sets.ItemNoGravity[Type] = true;
	}

	public override void SetDefaults()
	{
		
	}
}

internal sealed class InfernalConflux : ConfluxResource
{
	public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
	{
		Lighting.AddLight(Item.Center, Color.IndianRed.ToVector3());
	}
}

internal sealed class GlacialConflux : ConfluxResource
{
	public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
	{
		Lighting.AddLight(Item.Center, Color.LightSkyBlue.ToVector3());
	}
}

internal sealed class CelestialConflux : ConfluxResource
{
	public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
	{
		Lighting.AddLight(Item.Center, Color.Violet.ToVector3());
	}
}
