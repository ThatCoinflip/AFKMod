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
        public const string PluginVersion = "1.2.0";

        public void Awake()
        {
            try
            {
                Logger.LogInfo("Awake method called. Starting initialization process.");

                // Initialize Reflector
                Reflector.Init();
                if (Reflector.reflector != null)
                {
                    Logger.LogInfo("Reflector initialization completed successfully.");
                }
                else
                {
                    Logger.LogError("Reflector is null. Initialization failed.");
                }

                // Initialize Obelisk
                /*
                Obelisk.Init();
                if (Obelisk.obelisk != null)
                {
                    Logger.LogInfo("Obelisk initialization completed successfully.");
                }
                else
                {
                    Logger.LogError("Obelisk is null. Initialization failed.");
                }
                */

                Logger.LogInfo("Initialization process completed.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error during Awake initialization: {ex.Message}");
            }
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
