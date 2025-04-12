using System;
using BepInEx;
using R2API;
using RoR2;
using UnityEngine;
using AFKMod;
using AFKMod.Item;

namespace AFKMod
{
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Base : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "That.Coinflip";
        public const string PluginName = "AFKMod";
        public const string PluginVersion = "1.1.9";

        public void Awake()
        {
            Logger.LogInfo("Start Initialisation process");
            Reflector.Init();
            Logger.LogInfo("Reflector item successfully initialized!");

            Obelisk.Init();
            Logger.LogInfo("Immovable Obelisk item successfully initialized!");
        }

        private void Update()
        {
            // Detect F2 key press
            if (Input.GetKeyDown(KeyCode.F2))
            {
                // Get the player's position
                var playerTransform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                // Spawn the Immovable Obelisk item near the player
                Logger.LogInfo($"Player pressed F2. Spawning Obelisk item at coordinates {playerTransform.position}");
                PickupDropletController.CreatePickupDroplet(
                    PickupCatalog.FindPickupIndex(Obelisk.obelisk.itemIndex),
                    playerTransform.position,
                    playerTransform.forward * 20f
                );
            }
        }
    }
}