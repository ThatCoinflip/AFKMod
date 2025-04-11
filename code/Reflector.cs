using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Reflector
{
    public static class Reflector
    {
        public static ItemDef reflector; // Define the item
        public static float reflectorDamage = 0.2f;

        public static void Init()
        {
            // Create the ItemDef instance
            reflector = ScriptableObject.CreateInstance<ItemDef>();

            // Basic item information
            reflector.name = "Hand Mirror";
            reflector.nameToken = "Hand Mirror";
            reflector.pickupToken = "Hand Mirror";
            reflector.descriptionToken = "Uh....Shiney || reflect n*{reflectorDamage} when hit";
            reflector.loreToken = "Everybody needs some self reflection";

            reflector._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier1Def.asset").WaitForCompletion();
            reflector.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();
            reflector.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();

            reflector.canRemove = true;
            reflector.hidden = false;

            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(reflector, displayRules));

            // Hook the OnDamageTaken method to the onServerDamageDealt event
            GlobalEventManager.onServerDamageDealt += OnDamageTaken;

            Debug.Log("Reflector item successfully initialized and damage hook added!");
        }

        private static void OnDamageTaken(DamageReport report)
        {
            // Ensure valid attacker and victim
            if (report == null || report.attacker == null || report.victimBody == null) return;

            // Get the victim's body and inventory
            var victimBody = report.victimBody;
            var inventory = victimBody.inventory;

            // Check if the victim has the Reflector item
            if (inventory != null)
            {
                int reflectorCount = inventory.GetItemCount(reflector.itemIndex);
                if (reflectorCount > 0)
                {
                    // Calculate reflected damage (15% per item stack)
                    float reflectedDamage = report.damageInfo.damage * (reflectorDamage * reflectorCount);

                    // Minimum damage of 1
                    if (reflectedDamage < 1)
                    {
                        reflectedDamage = 1;
                    }

                    // Apply reflected damage to the attacker
                    var attackerBody = report.attackerBody;
                    if (attackerBody != null && attackerBody.healthComponent != null)
                    {
                        attackerBody.healthComponent.TakeDamage(new DamageInfo
                        {
                            damage = reflectedDamage,
                            attacker = victimBody.gameObject,
                            damageType = DamageType.Generic,
                            procCoefficient = 0f
                        });

                        // Log reflection details
                        Debug.Log($"Reflected {reflectedDamage} damage back to attacker!");
                    }
                }
            }
        }
    }
}
