﻿using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Content.Skills.Melee;
using Microsoft.Xna.Framework.Input;
using Terraria.GameInput;
using Terraria.ModLoader.IO;
using log4net.Core;
using System.Reflection;

namespace PathOfTerraria.Core.Systems.SkillSystem;

internal class SkillPlayer : ModPlayer
{
	public static ModKeybind Skill1Keybind;
	public static ModKeybind Skill2Keybind;
	public static ModKeybind Skill3Keybind;

	public Skill[] Skills = new Skill[3];

	public override void Load()
	{
		if (Main.dedServ)
		{
			return;
		}

		Skill1Keybind = KeybindLoader.RegisterKeybind(Mod, "UseSkill1", Keys.D3);
		Skill2Keybind = KeybindLoader.RegisterKeybind(Mod, "UseSkill2", Keys.D4);
		Skill3Keybind = KeybindLoader.RegisterKeybind(Mod, "UseSkill3", Keys.D5);
	}

	public override void ProcessTriggers(TriggersSet triggersSet)
	{
		//Skills[0] ??= new Berserk(1200, 1200, 1200, 1200, 5, GearType.Sword);

		if (Skill1Keybind.JustPressed && Skills[0] != null)
		{
			if (Skills[0].Timer == 0)
			{
				Skills[0]?.UseSkill(Main.LocalPlayer);
			}
		}

		if (Skill2Keybind.JustPressed && Skills[1] != null)
		{
			if (Skills[1].Timer == 0)
			{
				Skills[1]?.UseSkill(Main.LocalPlayer);
			}
		}

		if (Skill3Keybind.JustPressed && Skills[2] != null)
		{
			if (Skills[2].Timer == 0)
			{
				Skills[2]?.UseSkill(Main.LocalPlayer);
			}
		}
	}

	public override void ResetEffects()
	{
		if (Skills == null || Skills.Length == 0)
		{
			return;
		}

		foreach (Skill skill in Skills)
		{
			if (skill != null && skill.Timer > 0)
			{
				skill.Timer--;
			}
		}
	}

	public override void SaveData(TagCompound tag)
	{
		for (int i = 0; i < Skills.Length; i++)
		{
			Skill skill = Skills[i];

			if (skill is null)
			{
				return;
			}

			TagCompound skillTag = new()
			{
				{ "type", skill.GetType().AssemblyQualifiedName }
			};

			skill.SaveData(skillTag);
			tag.Add("skill" + i, skillTag);
		}
	}

	public override void LoadData(TagCompound tag)
	{
		for (int i = 0; i < Skills.Length; ++i)
		{
			if (!tag.ContainsKey("skill" + i))
			{
				return;
			}

			TagCompound data = tag.GetCompound("skill" + i);
			string type = data.GetString("type");

			var skill = Skill.ReflectSkillInstance(Type.GetType(type));
			skill.LoadData(data);

			Skills[i] = skill;
		}
	}
}