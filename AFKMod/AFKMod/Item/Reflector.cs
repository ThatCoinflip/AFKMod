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

namespace AFKMod.Item
{
    public static class Reflector
    {
        public static ItemDef reflector;
        public static float reflectorDamage = 0.2f;

        public static void Init()
        {
            reflector = ScriptableObject.CreateInstance<ItemDef>();

            reflector.name = "Hand Mirror";
            reflector.nameToken = "Hand Mirror";
            reflector.pickupToken = "Hand Mirror_PICKUP";
            reflector.descriptionToken = "reflect n* 15% damage taken to the attacker";
            reflector.loreToken = "In need of some self reflection";

            reflector._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier1Def.asset").WaitForCompletion();
            reflector.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("AFKMod/Item/Sprite/reflector.png").WaitForCompletion();
            reflector.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();

            reflector.canRemove = true;
            reflector.hidden = false;

            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(reflector, displayRules));

            GlobalEventManager.onServerDamageDealt += OnDamageTaken;

            Debug.Log("Reflector item initialized.");
        }

        private static void OnDamageTaken(DamageReport report)
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