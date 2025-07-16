using Terraria.DataStructures;

namespace PathOfTerraria.Content.Gores.Misc;

internal class PoisonBubble : ModGore
{
	public override void OnSpawn(Gore gore, IEntitySource source)
	{
		gore.Frame = new SpriteFrame(1, 3, 0, (byte)Main.rand.Next(3));
		gore.scale *= Main.rand.NextFloat(1, 1.6f);
	}

	public override bool Update(Gore gore)
	{
		gore.position += gore.velocity;
		gore.velocity *= 0.94f;
		gore.alpha += 7;

		if (gore.alpha >= 255)
		{
			gore.active = false;
		}

		return false;
	}
}
