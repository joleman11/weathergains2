using HarmonyLib;
using System;
using System.Linq;
using static UnityEngine.Rendering.HighDefinition.ScalableSettingLevelParameter;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine.ProBuilder;

namespace WeatherGains.Patches;

[HarmonyPatch(typeof(Terminal))]
public class GetEnemies
{
    public static SpawnableMapObject Landmine, Turret, SpikeTrap, Seamine, BigBertha;

    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    public static void GetEnemy(Terminal __instance)
    {
        WeatherGains.Logger.LogInfo($"Start GetEnemy");
        SelectableLevel? refOffense = null;

        foreach (SelectableLevel level in __instance.moonsCatalogueList)
        {
            if (level.name == "OffenseLevel")
            {
                refOffense = level;
                WeatherGains.Logger.LogInfo($"FOUND OFFENSE!");

                foreach (SpawnableMapObject trap in level.spawnableMapObjects)
                {
                    if (trap.prefabToSpawn.name == "Landmine" && Landmine == null)
                        Landmine = trap;
                    else if (trap.prefabToSpawn.name == "TurretContainer" && Turret == null)
                        Turret = trap;
                    else if (trap.prefabToSpawn.name == "SpikeRoofTrapHazard" && SpikeTrap == null)
                        SpikeTrap = trap;
                    else if (trap.prefabToSpawn.name == "Seamine" && Seamine == null)
                        Seamine = trap;
                    else if (trap.prefabToSpawn.name == "Bertha" && BigBertha == null)
                        BigBertha = trap;
                }

                break;
            }
        }

        if (refOffense == null)
            return;

        foreach (SelectableLevel level in __instance.moonsCatalogueList)
        {
            if (level.name == "ExperimentationLevel" || level.name == "AssuranceLevel" || level.name == "VowLevel")
            {
                SpawnableMapObject spike = level.spawnableMapObjects.FirstOrDefault(spawnableMapObject => spawnableMapObject.prefabToSpawn?.name == "SpikeRoofTrapHazard");
                if (spike == null)
                {
                    var list = level.spawnableMapObjects.ToList();
                    //GetEnemies.SpikeTrap.numberToSpawn = new AnimationCurve(new Keyframe(0f, 35), new Keyframe(1f, 35));
                    list.Add(GetEnemies.SpikeTrap);
                    level.spawnableMapObjects = list.ToArray();
                    WeatherGains.Logger.LogInfo($"added SpikeRoofTrapHazard");
                }

                level.factorySizeMultiplier = refOffense.factorySizeMultiplier;

                level.minScrap = refOffense.minScrap; // + 30;
                level.maxScrap = refOffense.maxScrap; // + 30;
                level.minTotalScrapValue = refOffense.minTotalScrapValue;
                level.maxTotalScrapValue = refOffense.maxTotalScrapValue;

                level.maxEnemyPowerCount = refOffense.maxEnemyPowerCount;
                level.maxDaytimeEnemyPowerCount = refOffense.maxDaytimeEnemyPowerCount;
                level.maxOutsideEnemyPowerCount = refOffense.maxOutsideEnemyPowerCount;

                level.daytimeEnemiesProbabilityRange = refOffense.daytimeEnemiesProbabilityRange;
                level.daytimeEnemySpawnChanceThroughDay = refOffense.daytimeEnemySpawnChanceThroughDay;
                level.enemySpawnChanceThroughoutDay = refOffense.enemySpawnChanceThroughoutDay;
                level.outsideEnemySpawnChanceThroughDay = refOffense.outsideEnemySpawnChanceThroughDay;
                level.spawnProbabilityRange = refOffense.spawnProbabilityRange;
            }
        }
    }
}

[HarmonyPatch(typeof(RoundManager))]
public class RoundManagerPatch
{
    [HarmonyPatch("SpawnScrapInLevel")]
    [HarmonyPrefix]
    private static void SpawnScrapInLevelPrefix()
    {
        if (WeatherGains.BoundConfig.ValueMultiEnabled.Value) RoundManager.Instance.scrapValueMultiplier *= WeatherGains.BoundConfig.Multipliers[TimeOfDay.Instance.currentLevelWeather].ValueMultiplier.Value;
        if (WeatherGains.BoundConfig.AmountMultiEnabled.Value)  RoundManager.Instance.scrapAmountMultiplier *= WeatherGains.BoundConfig.Multipliers[TimeOfDay.Instance.currentLevelWeather].AmountMultiplier.Value;
        
        WeatherGains.Logger.LogInfo(
            $"Successfully modified the scrap generation values for weather type {TimeOfDay.Instance.currentLevelWeather} on moon (level) {TimeOfDay.Instance.currentLevel}!\n\n" + 
            $"Modded Value Multiplier: {RoundManager.Instance.scrapValueMultiplier}\n" +
            $"Modded Amount Multiplier: {RoundManager.Instance.scrapAmountMultiplier}"
        );
    }

    [HarmonyPatch("SpawnScrapInLevel")]
    [HarmonyPostfix]
    private static void SpawnScrapInLevelPostfix()
    {
        //if (WeatherGains.BoundConfig.ValueMultiEnabled.Value) RoundManager.Instance.scrapValueMultiplier = 1f;
        //if (WeatherGains.BoundConfig.AmountMultiEnabled.Value)  RoundManager.Instance.scrapAmountMultiplier = 1f;
        if (WeatherGains.BoundConfig.ValueMultiEnabled.Value) RoundManager.Instance.scrapValueMultiplier /= WeatherGains.BoundConfig.Multipliers[TimeOfDay.Instance.currentLevelWeather].ValueMultiplier.Value;
        if (WeatherGains.BoundConfig.AmountMultiEnabled.Value) RoundManager.Instance.scrapAmountMultiplier /= WeatherGains.BoundConfig.Multipliers[TimeOfDay.Instance.currentLevelWeather].AmountMultiplier.Value;

        WeatherGains.Logger.LogInfo(
            $"Successfully reverted the scrap generation values for weather type {TimeOfDay.Instance.currentLevelWeather} on moon (level) {TimeOfDay.Instance.currentLevel}!\n\n" + 
            $"Reverted Value Multiplier: {RoundManager.Instance.scrapValueMultiplier}\n" +
            $"Reverted Amount Multiplier: {RoundManager.Instance.scrapAmountMultiplier}\n\n" +
            $"This is intended functionality... the mod isn't breaking!"
        );
    }
}