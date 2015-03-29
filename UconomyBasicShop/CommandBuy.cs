using System;
using Rocket.RocketAPI;
using SDG;
using UnityEngine;
using unturned.ROCKS.Uconomy;
using Steamworks;

namespace UconomyBasicShop
{
    public class CommandBuy : IRocketCommand
    {
        public bool RunFromConsole
        {
            get
            {
                return false;
            }
        }
        public string Name
        {
            get
            {
                return "buy";
            }
        }
        public string Help
        {
            get
            {
                return "Allows you to buy items from the shop.";
            }
        }

        public void Execute(CSteamID playerid, string msg)
        {
            string message;
            if (string.IsNullOrEmpty(msg))
            {
                message = "Usage: /buy <v or i>.<name or id>/<amount> (optional).";
                // We are going to print how to use
                RocketChatManager.Say(playerid, message);
                return;
            }
            byte amttobuy = 1;
            string[] components0 = Parser.getComponentsFromSerial(msg, '/');
            if (components0.Length > 1)
            {
                amttobuy = byte.Parse(components0[1]);
            }
            string[] components = Parser.getComponentsFromSerial(components0[0], '.');
            if (components.Length < 2)
            {
                message = "Usage: /buy <v or i>.<name or id>/<amount> (optional).";
                // We are going to print how to use
                RocketChatManager.Say(playerid, message);
                return;
            }
            ushort id;
            switch (components[0])
            {
                case "v":
                    if (!UconomyBasicShop.Instance.Configuration.CanBuyVehicles)
                    {
                        RocketChatManager.Say(playerid, UconomyBasicShop.Instance.Configuration.BuyVehiclesOff);
                        return;
                    }
                    string name = "";
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
                        RocketChatManager.Say(playerid, message);
                        return;
                    }
                    else if (name == null && id != 0)
                    {
                        name = ((VehicleAsset)Assets.find(EAssetType.Vehicle, id)).Name;
                    }
                    decimal cost = UconomyBasicShop.Instance.ShopDB.GetVehicleCost(id);
                    decimal balance = Uconomy.Instance.Database.GetBalance(playerid);
                    if (cost <= 0m)
                    {
                        message = String.Format(UconomyBasicShop.Instance.Configuration.VehicleNotAvailable, name);
                        RocketChatManager.Say(playerid, message);
                        return;
                    }
                    if (balance < cost)
                    {
                        message = String.Format(UconomyBasicShop.Instance.Configuration.NotEnoughCurrencyMsg, Uconomy.Instance.Configuration.MoneyName, name);
                        RocketChatManager.Say(playerid, message);
                        return;
                    }
                    Player player = PlayerTool.getPlayer(playerid);
                    if (!VehicleTool.giveVehicle(player, id))
                    {
                        RocketChatManager.Say(playerid, "There was an error giving you " + name + ".  You have not been charged.");
                        return;
                    }
                    decimal newbal = Uconomy.Instance.Database.IncreaseBalance(playerid, (cost * -1));
                    message = String.Format(UconomyBasicShop.Instance.Configuration.VehicleBuyMsg, name, cost, Uconomy.Instance.Configuration.MoneyName, newbal, Uconomy.Instance.Configuration.MoneyName);
                    message = "You bought " + name + " for " + cost.ToString() + " " + Uconomy.Instance.Configuration.MoneyName + ".";
                    RocketChatManager.Say(playerid, message);
                    break;
                default:
                    if (!UconomyBasicShop.Instance.Configuration.CanBuyItems)
                    {
                        RocketChatManager.Say(playerid, UconomyBasicShop.Instance.Configuration.BuyItemsOff);
                        return;
                    }
                    name = null;
                    if (!ushort.TryParse(components[1], out id))
                    {
                        Asset[] array = Assets.find(EAssetType.Item);
                        Asset[] array2 = array;
                        for (int i = 0; i < array2.Length; i++)
                        {
                            ItemAsset vAsset = (ItemAsset)array2[i];
                            if (vAsset != null && vAsset.Name != null && vAsset.Name.ToLower().Contains(components[1].ToLower()))
                            {
                                id = vAsset.Id;
                                name = vAsset.Name;
                                break;
                            }
                        }
                    }
                    if (name == null && id == null)
                    {
                        message = String.Format(UconomyBasicShop.Instance.Configuration.CouldNotFind, components[1]);
                        RocketChatManager.Say(playerid, message);
                        return;
                        
                    }
                    else if (name == null && id != null)
                    {
                        name = ((ItemAsset)Assets.find(EAssetType.Item, id)).Name;
                    }
                    cost = UconomyBasicShop.Instance.ShopDB.GetItemCost(id) * amttobuy;
                    balance = Uconomy.Instance.Database.GetBalance(playerid);
                    if (cost <= 0m)
                    {
                        message = String.Format(UconomyBasicShop.Instance.Configuration.ItemNotAvailable, name);
                        RocketChatManager.Say(playerid, message);
                        return;
                    }
                    if (balance < cost)
                    {
                        message = String.Format(UconomyBasicShop.Instance.Configuration.NotEnoughCurrencyMsg, Uconomy.Instance.Configuration.MoneyName, amttobuy, name);
                        RocketChatManager.Say(playerid, message);
                        return;
                    }
                    player = PlayerTool.getPlayer(playerid);
                        if (!ItemTool.tryForceGiveItem(player, id, amttobuy))
                        {
                            RocketChatManager.Say(playerid, "There was an error giving you " + name + ".  You have not been charged.");
                            return;
                        }
                    newbal = Uconomy.Instance.Database.IncreaseBalance(playerid, (cost * -1));
                    message = String.Format(UconomyBasicShop.Instance.Configuration.ItemBuyMsg, name, cost, Uconomy.Instance.Configuration.MoneyName, newbal, Uconomy.Instance.Configuration.MoneyName, amttobuy);
                    RocketChatManager.Say(playerid, message);
                    break;
            }
        }
    }
}