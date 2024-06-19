using PathOfTerraria.Content.Passives;
using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Core.Systems.TreeSystem;
using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI;

internal class PassiveSocket : PassiveElement
{
	private readonly JewelSocket _passive;
	private Jewel _socketed;
	
	public PassiveSocket(JewelSocket passive) : base(passive)
	{
		_passive = passive;
		_socketed = passive.Socketed;
	}

	public override void DrawOnto(SpriteBatch spriteBatch, Vector2 center)
	{
		_socketed?.Draw(spriteBatch, center);
	}

	public override void SafeClick(UIMouseEvent evt)
	{
		if (_passive.Level == _passive.MaxLevel)
		{
			if (Main.mouseItem.ModItem is Jewel)
			{
				Item replace = new Item();
				if (_socketed != null)
				{
					replace = _socketed.Item;
				}

				_socketed = Main.mouseItem.ModItem as Jewel;
				Main.mouseItem = replace;
			}
			else if (!Main.mouseItem.active && _socketed != null)
			{
				Main.mouseItem = _socketed.Item;
				_socketed = null;
			}

			_passive.Socketed = _socketed;

			return;
		}

		if (!_passive.CanAllocate(Main.LocalPlayer))
		{
			return;
		}

		_passive.Level++;
		Main.LocalPlayer.GetModPlayer<TreePlayer>().Points--;

		SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier5"));
	}

	public override void SafeRightClick(UIMouseEvent evt)
	{
		if (!_passive.CanDeallocate(Main.LocalPlayer) || _socketed != null)
		{
			return;
		}

		_passive.Level--;
		Main.LocalPlayer.GetModPlayer<TreePlayer>().Points++;

		SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath);
	}
}