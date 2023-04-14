using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using System.Reflection;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


[BepInPlugin("devopsdinosaur.bounty_of_one.quality_of_life", "Quality of Life", "0.0.1")]
public class ActionSpeedPlugin : BaseUnityPlugin {

	private Harmony m_harmony = new Harmony("devopsdinosaur.bounty_of_one.quality_of_life");
	public static ManualLogSource logger;

	private static ConfigEntry<bool> m_enabled;
	private static ConfigEntry<bool> m_invincibility;
	private static ConfigEntry<float> m_xp_multipler;
	private static ConfigEntry<float> m_time_multipler;
	private static ConfigEntry<float> m_elite_frequency;
	private static ConfigEntry<int> m_num_elite_chest_cards;
	private static ConfigEntry<string> m_favorite_elite_cards;

	private static Transform m_player = null;
	
	public static bool list_descendants(Transform parent, Func<Transform, bool> callback, int indent) {
		Transform child;
		string indent_string = "";
		for (int counter = 0; counter < indent; counter++) {
			indent_string += " => ";
		}
		for (int index = 0; index < parent.childCount; index++) {
			child = parent.GetChild(index);
			logger.LogInfo(indent_string + child.gameObject.name);
			if (callback != null) {
				if (callback(child) == false) {
					return false;
				}
			}
			list_descendants(child, callback, indent + 1);
		}
		return true;
	}

	public static bool enum_descendants(Transform parent, Func<Transform, bool> callback) {
		Transform child;
		for (int index = 0; index < parent.childCount; index++) {
			child = parent.GetChild(index);
			if (callback != null) {
				if (callback(child) == false) {
					return false;
				}
			}
			enum_descendants(child, callback);
		}
		return true;
	}

	public static void list_component_types(Transform obj) {
		foreach (Component component in obj.GetComponents<Component>()) {
			logger.LogInfo(component.GetType().ToString());
		}
	}

	
// 180NoScope
// PlotArmor
// HeroLanding
// Shockwave
// NitroCrit
// LittleFriend
// Gluttony
// IntimidatingReward
// Onion
// RotatingArrows
// RotatinShield
// Sniper
// TequilaBottle
// TripleShot
// MagnetizedHorseShoe
// WarningShots
// TeslaField
// ValiantHeart
// ProudMagnet
// SpringBullets
// ThunderDance
// CorkscrewBullet
// Pong
// BigBertha
// Spread Shot
// GuidedShot
// Chakra
// LuckyBelt
// EtherealFlame
// InnerStrength
// BloodyRage
// MezcalMantle
// Ninja
// PartingGift
// SteamtechTurret
// SeismicDance
// Taunt
// GodlyDie
// Adrenaline
// DarkRevenge
// NitroSuit
// Sheriffken
// Guidance System
// LastPull
// EmergencyKit
// RelentlessMomentum
// Surprise Attack
// SteamBoots
// PanicAttack
// KnifeVolley
// HawkEye
// SilverDie
// DirtyNigel
// SoothingEscape
// LuckyStrike
// Coffee
// MezcalMightyStaff
// FangedArrows
// BinarySystem
// MezcaleDefiance
// MezcalGrenade
// BronzeDie
// GoldenDie
// RunningOnFumes
// TrueSurvivor
// RecklessRush
// BloodlustIdol
// Scrap Gun

	private void Awake() {
		logger = this.Logger;
		try {
			m_enabled = this.Config.Bind<bool>("General", "Enabled", true, "Set to false to disable this mod.");
			m_invincibility = this.Config.Bind<bool>("General", "Invincibility", false, "If true then player will take no damage.");
			m_xp_multipler = this.Config.Bind<float>("General", "XP Multipler", 1f, "Multiplier applied to all coin xp gains (float).");
			m_time_multipler = this.Config.Bind<float>("General", "Time Speed Multipler", 1f, "Multiplier applied to time increments, i.e. speed up / slow down the flow of time (float).");
			m_elite_frequency = this.Config.Bind<float>("General", "Elite Spawn Frequency", 10f, "Time (in seconds) to delay between elite spawns (float, this value must be at least 1f and will be rounded up if needed).");
			m_num_elite_chest_cards = this.Config.Bind<int>("General", "Elite Chest Card Count", 12, "[Advanced] This sets the number of cards available in an elite chest.  If this number is higher than ~14 the icons will become too small and disappear (you can however select an option by reading the text, just won't see the picture)");
			m_favorite_elite_cards = this.Config.Bind<string>("General", "Favorite Elite Cards", "Coffee,TequilaBottle,Sheriffken,SteamBoots", "A comma-separated list of cards to always put in an elite chest if you don't yet have them.  They must *exactly* match the names from this list: 180NoScope, PlotArmor, HeroLanding, Shockwave, NitroCrit, LittleFriend, Gluttony, IntimidatingReward, Onion, RotatingArrows, RotatinShield, Sniper, TequilaBottle, TripleShot, MagnetizedHorseShoe, WarningShots, TeslaField, ValiantHeart, ProudMagnet, SpringBullets, ThunderDance, CorkscrewBullet, Pong, BigBertha, Spread Shot, GuidedShot, Chakra, LuckyBelt, EtherealFlame, InnerStrength, BloodyRage, MezcalMantle, Ninja, PartingGift, SteamtechTurret, SeismicDance, Taunt, GodlyDie, Adrenaline, DarkRevenge, NitroSuit, Sheriffken, Guidance System, LastPull, EmergencyKit, RelentlessMomentum, Surprise Attack, SteamBoots, PanicAttack, KnifeVolley, HawkEye, SilverDie, DirtyNigel, SoothingEscape, LuckyStrike, Coffee, MezcalMightyStaff, FangedArrows, BinarySystem, MezcaleDefiance, MezcalGrenade, BronzeDie, GoldenDie, RunningOnFumes, TrueSurvivor, RecklessRush, BloodlustIdol, Scrap Gun.");
			if (m_enabled.Value) {
				this.m_harmony.PatchAll();
			}
			logger.LogInfo("devopsdinosaur.bounty_of_one.quality_of_life v0.0.1 " + (!m_enabled.Value ? "(disabled by configuration option) " : " ") + "loaded.");
		} catch (Exception e) {
			logger.LogError("** Awake FATAL - " + e);
		}
	}

	[HarmonyPatch(typeof(XpHandler), "OnEnable")]
	class HarmonyPatch_XpHandler_OnEnable {

		private static XpHandler m_xp_handler = null;
		private static MethodInfo m_XPChange_info = null;
		
		private static void on_xp_change(float val) {
			m_XPChange_info.Invoke(m_xp_handler, new object[] {val * m_xp_multipler.Value});
		}

		private static bool Prefix(XpHandler __instance, ref float ___totalGainedXp, FloatEventReference ____xpChangeListener) {
			try {
				if (!(m_enabled.Value)) {
					return true;
				}
				m_xp_handler = __instance;
				m_XPChange_info = __instance.GetType().GetMethod("XPChange", BindingFlags.Instance | BindingFlags.NonPublic);
				___totalGainedXp = 0f;
				____xpChangeListener.Event?.Register(on_xp_change);
				return false;
			} catch (Exception e) {
				logger.LogError("XpHandler_OnEnable_Prefix ERROR - " + e);
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(KillByDamageDealingObjTracker), "WhatKilledItCheck")]
	class HarmonyPatch_KillByDamageDealingObjTracker_WhatKilledItCheck {

		private static bool Prefix(
			bool ___damageCanBeTheKiller, 
			IntEventReference ____objKilled, 
			int ___objId,
			VoidBaseEventReference ____dashKilled
		) {
			try {
				if (!m_enabled.Value) {
					return true;
				}
				if (___damageCanBeTheKiller) {
					____objKilled.Event?.Raise(___objId);
					____dashKilled?.Event.Raise();
				}
				return false;
			} catch (Exception e) {
				logger.LogError("KillByDamageDealingObjTracker_WhatKilledItCheck_Prefix ERROR - " + e);
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(HealthHandler), "OnEnable")]
	class HarmonyPatch_HealthHandler_OnEnable {

		private static void Postfix(HealthHandler __instance) {
			try {
				if (!m_enabled.Value || m_player != null) {
					return;
				}
				foreach (GameObject root_object in SceneManager.GetActiveScene().GetRootGameObjects()) {
					if (root_object.name == "Systems(Clone)") {
						m_player = root_object.transform.Find("PlayersInGame").GetChild(0).transform;
						logger.LogInfo(m_player);
						break;
					}
				}
				return;
			} catch (Exception e) {
				logger.LogError("HealthHandler.OnEnable_Postfix ERROR - " + e);
			}
		}
	}

	[HarmonyPatch(typeof(ChoiceGeneratorForUpgradeSO), "NewChoice")]
	class HarmonyPatch_ChoiceGeneratorForUpgradeSO_NewChoice {

		private static bool Prefix(
			ChoiceGeneratorForUpgradeSO __instance,
			bool removeLastChoiceFromPool,
			UpgradeSOValueList ____upgradesAvailable,
			List<UpgradeSO> ____upgradeFromAllLastChoices,
			UpgradeSOValueList ____upgradeCurrentChoice
		) {
			try {
				if (!m_enabled.Value) {
					return true;
				}
				List<UpgradeSO> all_upgrades = new List<UpgradeSO>();
				all_upgrades.AddRange(____upgradesAvailable);
				if (removeLastChoiceFromPool) {
					all_upgrades = all_upgrades.Except(____upgradeFromAllLastChoices).ToList();
				}
				if (all_upgrades.Count == 0) {
					return true;
				}
				//if (is_elite_chest) {
				//	string text = "";
				//	foreach (UpgradeSO upgrade in all_upgrades) {
				//		text += upgrade.name + ", ";
				//	}
				//	logger.LogInfo(text);
				//}
				____upgradeCurrentChoice.Clear();
				if (all_upgrades[0].type == EffectType.Object) {
					// need to re-parse this every time in case user changed the value in ConfigurationManager
					string[] vals = m_favorite_elite_cards.Value.Split(',');
					List<string> favorites = new List<string>();
					for (int index = 0; index < vals.Length; index++) {
						string val = vals[index].Trim();
						if (val.Length > 0) {
							favorites.Add(val);
						}
					}
					Dictionary<string, UpgradeSO> upgrade_dict = new Dictionary<string, UpgradeSO>();
					foreach (UpgradeSO upgrade in all_upgrades) {
						upgrade_dict[upgrade.name] = upgrade;
					}
					foreach (string card_name in favorites) {
						if (____upgradeCurrentChoice.Count >= m_num_elite_chest_cards.Value) {
							break;
						}
						if (upgrade_dict.ContainsKey(card_name)) {
							____upgradeCurrentChoice.Add(upgrade_dict[card_name]);
							upgrade_dict.Remove(card_name);
						}
					}
					for (;;) {
						if (____upgradeCurrentChoice.Count >= m_num_elite_chest_cards.Value) {
							break;
						}
						string key = new List<string>(upgrade_dict.Keys)[UnityEngine.Random.Range(0, upgrade_dict.Keys.Count)];
						____upgradeCurrentChoice.Add(upgrade_dict[key]);
						upgrade_dict.Remove(key);
					}
				} else {
					foreach (UpgradeSO upgrade in all_upgrades) {
						if (upgrade.rarity == Rarity.Legendary && upgrade.name != "ChoiceLegendary") {
							____upgradeCurrentChoice.Add(upgrade);
						}
					}
				}
				____upgradeFromAllLastChoices.AddRange(____upgradeCurrentChoice);
				return false;
			} catch (Exception e) {
				logger.LogError("ChoiceGeneratorForUpgradeSO_NewChoice_Prefix ERROR - " + e);
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(SpawnEnemiesHandler), "CheckSpawnElite")]
	class HarmonyPatch_SpawnEnemiesHandler_CheckSpawnElite {

		private static bool Prefix(
			FloatReference ____eliteDelay, 
			IntReference ____elitePerChapter,
			ref int ____eliteIndex
		) {
			try {
				if (!m_enabled.Value) {
					return true;
				}
				____eliteDelay.Value = Math.Max(1f, m_elite_frequency.Value);
				____elitePerChapter.Value = ____eliteIndex + 1;
				return true;
			} catch (Exception e) {
				logger.LogError("SpawnEnemiesHandler_CheckSpawnElite_Prefix ERROR - " + e);
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(HealthHandler), "HealthChange")]
	class HarmonyPatch_HealthHandler_HealthChange {

		private static bool Prefix(HealthHandler __instance, ref FloatReference ____healthChangeValue) {
			try {
				if (!(m_enabled.Value && m_invincibility.Value && __instance.GetType().Name == "HealthHandler")) {
					return true;
				}
				____healthChangeValue.Value = 0f;
				return true;
			} catch (Exception e) {
				logger.LogError("HealthHandler.HealthChange_Prefix ERROR - " + e);
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(GameTimeHandler), "Update")]
	class HarmonyPatch_GameTimeHandler_Update {

		private static bool Prefix(GameTimeHandler __instance, bool ___timerIsGoing, ref FloatReference ____timerGlobal) {
			try {
				if (!m_enabled.Value) {
					return true;
				}
				if (__instance.enabled && ___timerIsGoing) {
					____timerGlobal.Value += (Time.deltaTime * m_time_multipler.Value);
				}
				return false;
			} catch (Exception e) {
				logger.LogError("GameTimeHandler.Update_Prefix ERROR - " + e);
			}
			return true;
		}
	}
}