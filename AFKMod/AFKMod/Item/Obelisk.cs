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

namespace AFKMod.Item
{
    public static class Obelisk
    {
        public static ItemDef obelisk; // Define the item
        public static float baseChanceToNegateKnockback = 0.1f; // Base chance to negate knockback
        public static float baseStunFreezeReduction = 0.15f; // Base reduction for stun/freeze duration

        public static void Init()
        {
            // Create the ItemDef instance
            obelisk = ScriptableObject.CreateInstance<ItemDef>();

            // Basic item information
            obelisk.name = "Immoveable Obelisk";
            obelisk.nameToken = "Immoveable Obelisk";
            obelisk.pickupToken = "Immoveable Obelisk";
            obelisk.descriptionToken = "Grants a n * 10% chance to negate knockback. "; //Additionally, reduces the duration of stuns and freezes by 15% + n * 10%. ´that part does not work
            obelisk.loreToken = " Stand your ground with unyielding strength.";

            obelisk._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Uncommon/Tier2Def.asset").WaitForCompletion();
            obelisk.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("AFKMod/Item/Sprite/obelisk.png").WaitForCompletion();
            obelisk.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();

            obelisk.canRemove = true;
            obelisk.hidden = false;

            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(obelisk, displayRules));

            // Hook the OnDamageTaken method to the onServerDamageDealt event
            GlobalEventManager.onServerDamageDealt += ApplyObeliskEffects;

            Debug.Log("Immovable Obelisk item successfully initialized!");
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