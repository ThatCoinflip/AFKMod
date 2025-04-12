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

namespace AFKMod.Item
{
    public static class Obelisk
    {
        public static ItemDef obelisk = null!;  // Define the item
        public static float baseChanceToNegateKnockback = 0.1f; // Base chance to negate knockback
        public static float baseStunFreezeReduction = 0.15f; // Base reduction for stun/freeze duration

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
            // Ensure valid victim and attacker information
            if (report == null || report.victimBody == null) return;

            // Get the victim's body and inventory
            var victimBody = report.victimBody;
            var inventory = victimBody.inventory;

            // Check if the victim has the Immovable Obelisk item
            if (inventory != null)
            {
                int obeliskCount = inventory.GetItemCount(obelisk.itemIndex);
                if (obeliskCount > 0)
                {
                    // Calculate chance to negate knockback
                    float negateChance = baseChanceToNegateKnockback * obeliskCount;
                    if (UnityEngine.Random.value <= negateChance)
                    {
                        victimBody.SetFieldValue("negateKnockback", true);
                    }

                    // Apply stun and freeze duration reduction, need to fix
                    // victimBody.stuneffect *= 1 - (baseStunFreezeReduction + obeliskCount * 0.1f);

                    Debug.Log($"Immovable Obelisk effects applied: Negate chance = {negateChance * 100}%, Stun/Freeze reduction = {baseStunFreezeReduction + obeliskCount * 0.1f * 100}%.");
                }
            }
        }
    }
}