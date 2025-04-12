using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using AFKMod;
using System.Reflection;
using EntityStates.ArtifactShell;
using UnityEngine.Networking;

namespace AFKMod.Item
{
    public static class Obelisk
    {
        public static ItemDef obelisk = null!;  // Define the item
        public static float baseChanceToNegateKnockback = 0.1f; // Base chance to negate knockback
        public static float baseStunFreezeReduction = 0.15f; // Base reduction for stun/freeze duration
        private static float originalKnockBackForce = Hurt.knockbackForce;
        private static float originalknockbackLiftForce = Hurt.knockbackLiftForce;

        public static void Init()
        {
            Debug.Log("Starting Obelisk initialization...");
            // Create the ItemDef instance
            obelisk = ScriptableObject.CreateInstance<ItemDef>();
            if (obelisk == null)
            {
                Debug.LogError("Obelisk instance could not be created!");
                return;
            }

            // Basic item information
            obelisk.name = "Obelisk";
            obelisk.nameToken = "IMMOVABLE_OBELISK_NAME";
            obelisk.pickupToken = "IMMOVABLE_OBELISK_PICKUP";
            obelisk.descriptionToken = "IMMOVABLE_OBELISK_DESC";
            obelisk.loreToken = "IMMOVABLE_OBELISK_LORE";

            //LanguageAPI.Add("IMMOVABLE_OBELISK_NAME", "Immovable Obelisk");
            //LanguageAPI.Add("IMMOVABLE_OBELISK_PICKUP", "Grants 10% chance per stack to negate knockback.");
            //LanguageAPI.Add("IMMOVABLE_OBELISK_DESC", "Grants 10% chance per stack to negate knockback.");
            //LanguageAPI.Add("IMMOVABLE_OBELISK_LORE", "Stand your ground with unyielding strength.");

            var fieldInfo = typeof(ItemDef).GetField("_itemTierDef", BindingFlags.Instance | BindingFlags.NonPublic);
            fieldInfo.SetValue(obelisk, Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier2Def.asset").WaitForCompletion());

            if (fieldInfo == null)
            {
                Debug.LogError("_itemTierDef field not found in ItemDef.");
            }

            obelisk.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();
            obelisk.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();

            obelisk.canRemove = true;
            obelisk.hidden = false;

            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(obelisk, displayRules));

            ItemCatalog.availability.CallWhenAvailable(() =>
            {
                Debug.Log($"Reflector Item Index after catalog availability: {obelisk.itemIndex}");
            });

            Debug.Log($"Obelisk Item Index: {obelisk.itemIndex}");

            PickupCatalog.FindPickupIndex(obelisk.itemIndex);

            // Hook the OnDamageTaken method to the onServerDamageDealt event
            GlobalEventManager.onServerDamageDealt += ApplyObeliskEffects;

            Debug.Log($"Reflector initialization complete. Item Index: {obelisk.itemIndex}");
        }

        private static void ApplyObeliskEffects(DamageReport report)
        {
            if (report == null)
            {
                Debug.LogWarning("DamageReport is null.");
                return;
            }

            var victimBody = report.victimBody;
            if (victimBody == null)
            {
                Debug.LogWarning("Victim body is null.");
                return;
            }

            var inventory = victimBody.inventory;
            if (inventory == null)
            {
                Debug.LogWarning("Inventory is null.");
                return;
            }

            int obeliskCount = inventory.GetItemCount(obelisk.itemIndex);
            if (obeliskCount > 0)
            {
                Debug.Log($"Applying Immovable Obelisk effects. Item count: {obeliskCount}");

                float negateChance = baseChanceToNegateKnockback * obeliskCount;
                if (UnityEngine.Random.value <= negateChance)
                {
                    Hurt.knockbackForce = Hurt.knockbackForce * 0;
                    Hurt.knockbackLiftForce = Hurt.knockbackLiftForce * 0;

                    //victimBody.SetFieldValue("negateKnockback", true); Nope, doesnt work
                    Debug.Log($"Knockback negated with {negateChance * 100}% chance. Knockbackforce: {Hurt.knockbackForce}, Knockbackliftforce: {Hurt.knockbackLiftForce}");
                }

                // Reduce duration of stun and freeze buffs
                //    var buffReductionFactor = 1 - (baseStunFreezeReduction + obeliskCount * 0.1f);
                //    foreach (var buff in victimBody.activeBuffsList)
                //    {
                //        if (IsStunOrFreezeBuff(buff))
                //        {
                //            victimBody.SetBuffCount(buff.buffIndex, Mathf.FloorToInt(buff.buffCount * buffReductionFactor));
                //            Debug.Log($"Reduced buff {buff.buffName} duration by {buffReductionFactor * 100}%.");
                //        }
                //    }
                else
                {
                    Debug.Log("No Immovable Obelisk items found in inventory.");
                }
            }
        }
    }
}