using System;
using Rocket.RocketAPI;
using Rocket.Logging;
using SDG;
using UnityEngine;
using unturned.ROCKS.Uconomy;

namespace UconomyBasicShop
{
    public class CommandCost : Command
    {
        public CommandCost()
        {
            this.commandName = "cost";
            this.commandHelp = "Tells you the cost of a selected item.";
            this.commandInfo = this.commandName + " - " + this.commandHelp;
        }

        protected override void execute(SteamPlayerID playerid, string msg)
        {
            if (!RocketCommand.IsPlayer(playerid)) return;
            SteamPlayer splayer = PlayerTool.getSteamPlayer(playerid.CSteamID);
            string[] perms = RocketPermissionManager.GetPermissions(playerid.CSteamID);
            bool perm = false;
            foreach (string p in perms)
            {
                if (p == "cost")
                {
                    perm = true;
                    break;
                }
            }
            if (!perm)
            {
                RocketChatManager.Say(playerid.CSteamID, "You don't have permission to use the /cost command.");
                return;
            }
            string message;
            if (string.IsNullOrEmpty(msg))
            {
                message = "Usage: /cost <v or i>.<name or id>.";
                // We are going to print how to use
                RocketChatManager.Say(playerid.CSteamID, message);
                return;
            }
            string[] components = Parser.getComponentsFromSerial(msg, '.');
            if (components.Length < 2)
            {
                message = "Usage: /cost <v or i>.<name or id>.";
                // We are going to print how to use
                RocketChatManager.Say(playerid.CSteamID, message);
                return;
            }
            ushort id;
            switch (components[0])
            {
                case "v":
                    string name = null;
                    if (!ushort.TryParse(components[1], out id))
                    {
                        Asset[] array = Assets.find(EAssetType.Vehicle);
                        Asset[] array2 = array;
                        for (int i = 0; i < array2.Length; i++)
                        {
                            VehicleAsset vAsset = (VehicleAsset)array2[i];
                            if (vAsset != null && vAsset.Name != null && vAsset.Name.ToLower().Contains(components[1].ToLower()))
                            {
                                id = vAsset.Id;
                                name = vAsset.Name;
                                break;
                            }
                        }
                    }
                    if (name == null && id == 0)
                    {
                        message = String.Format(UconomyBasicShop.Instance.Configuration.CouldNotFind, components[1]);
                        RocketChatManager.Say(playerid.CSteamID, message);
                        return;
                    }
                    else if (name == null && id != 0)
                    {
                        name = ((VehicleAsset)Assets.find(EAssetType.Vehicle, id)).Name;
                    }
                    decimal cost = UconomyBasicShop.Instance.ShopDB.GetVehicleCost(id);
                    message = String.Format(UconomyBasicShop.Instance.Configuration.VehicleCostMsg, name, cost.ToString(), Uconomy.Instance.Configuration.MoneyName);
                    if (cost <= 0m)
                    {
                        message = "There was an error getting the cost of " + name + "!";
                    }
                    RocketChatManager.Say(playerid.CSteamID, message);
                    break;
                default:
                    name = null;
                    if (!ushort.TryParse(components[1], out id))
                    {
                        Asset[] array = Assets.find(EAssetType.Item);
                        Asset[] array2 = array;
                        for (int i = 0; i < array2.Length; i++)
                        {
                            ItemAsset iAsset = (ItemAsset)array2[i];
                            if (iAsset != null && iAsset.Name != null && iAsset.Name.ToLower().Contains(components[1].ToLower()))
                            {
                                id = iAsset.Id;
                                name = iAsset.Name;
                                break;
                            }
                        }
                    }
                    if (name == null && id == null)
                    {
                        message = String.Format(UconomyBasicShop.Instance.Configuration.CouldNotFind, components[1]);
                        RocketChatManager.Say(playerid.CSteamID, message);
                        return;
                    }
                    else if (name == null && id != null)
                    {
                        name = ((ItemAsset)Assets.find(EAssetType.Item, id)).Name;
                    }
                    cost = UconomyBasicShop.Instance.ShopDB.GetItemCost(id);
                    message = String.Format(UconomyBasicShop.Instance.Configuration.ItemCostMsg, name, cost.ToString(), Uconomy.Instance.Configuration.MoneyName);
                    if (cost <= 0m)
                    {
                        message = "There was an error getting the cost of " + name + "!";
                    }
                    RocketChatManager.Say(playerid.CSteamID, message);
                    break;
            }
        }
    }
}