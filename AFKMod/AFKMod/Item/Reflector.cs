using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using AFKMod;
using System.Reflection;

namespace AFKMod.Item
{
    public static class Reflector
    {
        public static ItemDef reflector = null!;
        public static float reflectorDamage = 0.2f;

        public static void Init()
        {
            {
                Debug.Log("Starting Reflector initialization...");

                reflector = ScriptableObject.CreateInstance<ItemDef>();
                if (reflector == null)
                {
                    Debug.LogError("Reflector instance could not be created!");
                    return;
                }
                reflector.name = "Hand_Mirror";
                reflector.nameToken = "HAND_MIRROR_NAME";
                reflector.pickupToken = "HAND_MIRROR_PICKUP";
                reflector.descriptionToken = "HAND_MIRROR_DESC";
                reflector.loreToken = "HAND_MIRROR_LORE";

                //LanguageAPI.Add("HAND_MIRROR_NAME", "Hand Mirror");
                //LanguageAPI.Add("HAND_MIRROR_PICKUP", "Reflects damage taken back to attackers.");
                //LanguageAPI.Add("HAND_MIRROR_DESC", "Reflects 20% of the damage taken back to the attacker.");
                //LanguageAPI.Add("HAND_MIRROR_LORE", "In need of some self-reflection.");

                var fieldInfo = typeof(ItemDef).GetField("_itemTierDef", BindingFlags.Instance | BindingFlags.NonPublic);
                fieldInfo.SetValue(reflector, Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier1Def.asset").WaitForCompletion());
                if (fieldInfo == null)
                {
                    Debug.LogError("_itemTierDef field not found in ItemDef.");
                }

                reflector.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();
                reflector.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();

                reflector.canRemove = true;
                reflector.hidden = false;

                var displayRules = new ItemDisplayRuleDict(null);
                ItemAPI.Add(new CustomItem(reflector, displayRules));

                ItemCatalog.availability.CallWhenAvailable(() =>
                {
                    Debug.Log($"Reflector Item Index after catalog availability: {reflector.itemIndex}");
                });

                Debug.Log($"Reflector Item Index: {reflector.itemIndex}");

                PickupCatalog.FindPickupIndex(reflector.itemIndex);

                GlobalEventManager.onServerDamageDealt += OnDamageTaken;

                Debug.Log($"Reflector initialization complete. Item Index: {reflector.itemIndex}");
            }

            static void OnDamageTaken(DamageReport report)
            {
                if (report == null || report.attacker == null || report.victimBody == null) return;

                var victim = report.victimBody;
                var inventory = victim.inventory;

                if (inventory == null) return;

                int count = inventory.GetItemCount(reflector.itemIndex);
                if (count <= 0) return;

                float reflectedDamage = report.damageInfo.damage * reflectorDamage * count;
                reflectedDamage = Mathf.Max(reflectedDamage, 1f); // Ensure minimum damage

                var attackerBody = report.attackerBody;
                if (attackerBody != null && attackerBody.healthComponent != null)
                {
                    attackerBody.healthComponent.TakeDamage(new DamageInfo
                    {
                        damage = reflectedDamage,
                        attacker = victim.gameObject,
                        damageType = DamageType.Generic,
                        procCoefficient = 0f,
                        position = attackerBody.corePosition
                    });

                    Debug.Log($"Reflected {reflectedDamage} damage back to attacker.");
                }
            }
        }
    }
}