using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Content.Skills.Melee;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using Terraria.GameInput;

namespace PathOfTerraria.Core.Systems.SkillSystem;

internal class SkillPlayer : ModPlayer
{
	private static ModKeybind _skill1Keybind;
	private static ModKeybind _skill2Keybind;
	private static ModKeybind _skill3Keybind;
	public Skill[] Skills = new Skill[5];

	public override void Load()
	{
		if (Main.dedServ)
			return;

		_skill1Keybind = KeybindLoader.RegisterKeybind(Mod, "UseSkill1", Keys.D3);
		_skill2Keybind = KeybindLoader.RegisterKeybind(Mod, "UseSkill2", Keys.D4);
		_skill3Keybind = KeybindLoader.RegisterKeybind(Mod, "UseSkill3", Keys.D5);
	}

	public override void ProcessTriggers(TriggersSet triggersSet)
	{
		Skills[0] ??= new Berserk(1200, 1200, 1200, 1200, 5, GearType.Sword);
			
		if (_skill1Keybind.JustPressed && Skills[0] != null)
		{
			if (Skills[0].Timer == 0)
				Skills[0]?.UseSkill(Main.LocalPlayer);
		}

		if (_skill2Keybind.JustPressed && Skills[1] != null)
		{
			if (Skills[1].Timer == 0)
				Skills[1]?.UseSkill(Main.LocalPlayer);	
		}

		if (_skill3Keybind.JustPressed && Skills[2] != null)
		{
			if (Skills[2].Timer == 0)
				Skills[2]?.UseSkill(Main.LocalPlayer);	
		}
	}

	public override void ResetEffects()
	{
		if (Skills == null || !Skills.Any())
			return;
			
		foreach (Skill skill in Skills)
		{
			if (skill != null && skill.Timer > 0)
				skill.Timer--;
		}
	}
}