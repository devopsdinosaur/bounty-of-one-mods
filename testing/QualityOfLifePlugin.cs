using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using System.Reflection;


[BepInPlugin("devopsdinosaur.bounty_of_one.quality_of_life", "Quality of Life", "0.0.1")]
public class ActionSpeedPlugin : BaseUnityPlugin {

	private Harmony m_harmony = new Harmony("devopsdinosaur.bounty_of_one.quality_of_life");
	public static ManualLogSource logger;

	private static ConfigEntry<bool> m_enabled;
	private static ConfigEntry<bool> m_invincibility;
	private static ConfigEntry<float> m_xp_multipler;
	private static ConfigEntry<float> m_time_multipler;
	
	private void Awake() {
		logger = this.Logger;
		try {
			m_enabled = this.Config.Bind<bool>("General", "Enabled", true, "Set to false to disable this mod.");
			m_invincibility = this.Config.Bind<bool>("General", "Invincibility", false, "If true then player will take no damage.");
			m_xp_multipler = this.Config.Bind<float>("General", "XP Multipler", 1f, "Multiplier applied to all coin xp gains (float).");
			m_time_multipler = this.Config.Bind<float>("General", "Time Speed Multipler", 1f, "Multiplier applied to time increments, i.e. speed up / slow down the flow of time (float).");
			if (m_enabled.Value) {
				this.m_harmony.PatchAll();
			}
			logger.LogInfo("devopsdinosaur.bounty_of_one.quality_of_life v0.0.1 " + (!m_enabled.Value ? "(disabled by configuration option) " : " ") + "loaded.");
		} catch (Exception e) {
			logger.LogError("** Awake FATAL - " + e);
		}
	}

	/*
	[HarmonyPatch(typeof(LootDropHandler), "DropTheLoot")]
	class HarmonyPatch_Enemy_OnHitPlayer {

		private static bool Prefix(CollectibleSO chosenLoot) {
			try {
				if (!(m_enabled.Value)) {
					return true;
				}
				chosenLoot._xpGain.Value *= (int) Math.Floor(chosenLoot._xpGain.Value * m_xp_multipler.Value);
				return true;
			} catch (Exception e) {
				logger.LogError("LootDropHandler.DropTheLoot_Prefix ERROR - " + e);
			}
			return true;
		}
	}
	*/

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