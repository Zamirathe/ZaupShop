using System;
using System.Collections.Generic;
using Rocket.RocketAPI;
using Rocket.Logging;
using SDG;
using UnityEngine;
using unturned.ROCKS.Uconomy;
using Steamworks;

namespace UconomyBasicShop
{
    public class UconomyBasicShop : RocketPlugin<UconomyBasicShopConfiguration>
    {
        public DatabaseMgr ShopDB;
        public static UconomyBasicShop Instance;
        
        protected override void Load()
        {
            UconomyBasicShop.Instance = this;
            this.ShopDB = new DatabaseMgr();
        }

        public delegate void PlayerShopBuy(RocketPlayer player, decimal amt, byte items, ushort item);
        public event PlayerShopBuy OnShopBuy;
        public delegate void PlayerShopSell(RocketPlayer player, decimal amt, byte items, ushort item);
        public event PlayerShopSell OnShopSell;

        public void Buy(RocketPlayer playerid, string msg)
        {
            string message;
            if (string.IsNullOrEmpty(msg))
            {
                message = "Usage: /buy [v.]<name or id>/[amount].";
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
            if (components.Length == 2 && components[0] != "v")
            {
                message = "Usage: /buy [v.]<name or id>/[amount].";
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
                    decimal balance = Uconomy.Instance.Database.GetBalance(playerid.CSteamID);
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
                    if (!playerid.GiveVehicle(id))
                    {
                        RocketChatManager.Say(playerid, "There was an error giving you " + name + ".  You have not been charged.");
                        return;
                    }
                    decimal newbal = Uconomy.Instance.Database.IncreaseBalance(playerid.CSteamID, (cost * -1));
                    message = String.Format(UconomyBasicShop.Instance.Configuration.VehicleBuyMsg, name, cost, Uconomy.Instance.Configuration.MoneyName, newbal, Uconomy.Instance.Configuration.MoneyName);
                    message = "You bought " + name + " for " + cost.ToString() + " " + Uconomy.Instance.Configuration.MoneyName + ".";
                    if (UconomyBasicShop.Instance.OnShopBuy != null)
                        UconomyBasicShop.Instance.OnShopBuy(playerid, cost, 1, id);
                    RocketChatManager.Say(playerid, message);
                    break;
                default:
                    if (!UconomyBasicShop.Instance.Configuration.CanBuyItems)
                    {
                        RocketChatManager.Say(playerid, UconomyBasicShop.Instance.Configuration.BuyItemsOff);
                        return;
                    }
                    name = null;
                    if (!ushort.TryParse(components[0], out id))
                    {
                        Asset[] array = Assets.find(EAssetType.Item);
                        Asset[] array2 = array;
                        for (int i = 0; i < array2.Length; i++)
                        {
                            ItemAsset vAsset = (ItemAsset)array2[i];
                            if (vAsset != null && vAsset.Name != null && vAsset.Name.ToLower().Contains(components[0].ToLower()))
                            {
                                id = vAsset.Id;
                                name = vAsset.Name;
                                break;
                            }
                        }
                    }
                    if (name == null && id == 0)
                    {
                        message = String.Format(UconomyBasicShop.Instance.Configuration.CouldNotFind, components[0]);
                        RocketChatManager.Say(playerid, message);
                        return;

                    }
                    else if (name == null && id != 0)
                    {
                        name = ((ItemAsset)Assets.find(EAssetType.Item, id)).Name;
                    }
                    cost = UconomyBasicShop.Instance.ShopDB.GetItemCost(id) * amttobuy;
                    balance = Uconomy.Instance.Database.GetBalance(playerid.CSteamID);
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
                    if (!playerid.GiveItem(id, amttobuy))
                    {
                        RocketChatManager.Say(playerid, "There was an error giving you " + name + ".  You have not been charged.");
                        return;
                    }
                    newbal = Uconomy.Instance.Database.IncreaseBalance(playerid.CSteamID, (cost * -1));
                    message = String.Format(UconomyBasicShop.Instance.Configuration.ItemBuyMsg, name, cost, Uconomy.Instance.Configuration.MoneyName, newbal, Uconomy.Instance.Configuration.MoneyName, amttobuy);
                    if (UconomyBasicShop.Instance.OnShopBuy != null)
                        UconomyBasicShop.Instance.OnShopBuy(playerid, cost, amttobuy, id);
                    RocketChatManager.Say(playerid, message);
                    break;
            }
        }
        public void Cost(RocketPlayer playerid, string msg)
        {
            string message;
            if (string.IsNullOrEmpty(msg))
            {
                message = "Usage: /cost [v.]<name or id>.";
                // We are going to print how to use
                RocketChatManager.Say(playerid, message);
                return;
            }
            string[] components = Parser.getComponentsFromSerial(msg, '.');
            if (components.Length == 2 && components[0] != "v")
            {
                message = "Usage: /cost [v.]<name or id>.";
                // We are going to print how to use
                RocketChatManager.Say(playerid, message);
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
                        RocketChatManager.Say(playerid, message);
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
                    RocketChatManager.Say(playerid, message);
                    break;
                default:
                    name = null;
                    if (!ushort.TryParse(components[0], out id))
                    {
                        Asset[] array = Assets.find(EAssetType.Item);
                        Asset[] array2 = array;
                        for (int i = 0; i < array2.Length; i++)
                        {
                            ItemAsset iAsset = (ItemAsset)array2[i];
                            if (iAsset != null && iAsset.Name != null && iAsset.Name.ToLower().Contains(components[0].ToLower()))
                            {
                                id = iAsset.Id;
                                name = iAsset.Name;
                                break;
                            }
                        }
                    }
                    if (name == null && id == 0)
                    {
                        message = String.Format(UconomyBasicShop.Instance.Configuration.CouldNotFind, components[0]);
                        RocketChatManager.Say(playerid, message);
                        return;
                    }
                    else if (name == null && id != 0)
                    {
                        name = ((ItemAsset)Assets.find(EAssetType.Item, id)).Name;
                    }
                    cost = UconomyBasicShop.Instance.ShopDB.GetItemCost(id);
                    decimal bbp = UconomyBasicShop.Instance.ShopDB.GetItemBuyPrice(id);
                    message = String.Format(UconomyBasicShop.Instance.Configuration.ItemCostMsg, name, cost.ToString(), Uconomy.Instance.Configuration.MoneyName, bbp.ToString(), Uconomy.Instance.Configuration.MoneyName);
                    if (cost <= 0m)
                    {
                        message = "There was an error getting the cost of " + name + "!";
                    }
                    RocketChatManager.Say(playerid, message);
                    break;
            }
        }
        public void Sell(RocketPlayer playerid, string msg)
        {
            string message;
            if (string.IsNullOrEmpty(msg))
            {
                message = "Usage: /sell <name or id>/[amount] (optional).";
                // We are going to print how to use
                RocketChatManager.Say(playerid, message);
                return;
            }
            byte amttosell = 1;
            string[] components = Parser.getComponentsFromSerial(msg, '/');
            if (components.Length > 1)
            {
                amttosell = byte.Parse(components[1]);
            }
            byte amt = amttosell;
            ushort id;
            if (!UconomyBasicShop.Instance.Configuration.CanSellItems)
            {
                RocketChatManager.Say(playerid, UconomyBasicShop.Instance.Configuration.SellItemsOff);
                return;
            }
            string name = null;
            ItemAsset vAsset = null;
            if (!ushort.TryParse(components[0], out id))
            {
                Asset[] array = Assets.find(EAssetType.Item);
                Asset[] array2 = array;
                for (int i = 0; i < array2.Length; i++)
                {
                    vAsset = (ItemAsset)array2[i];
                    if (vAsset != null && vAsset.Name != null && vAsset.Name.ToLower().Contains(components[0].ToLower()))
                    {
                        id = vAsset.Id;
                        name = vAsset.Name;
                        break;
                    }
                }
            }
            if (name == null && id == 0)
            {
                message = String.Format(UconomyBasicShop.Instance.Configuration.CouldNotFind, components[0]);
                RocketChatManager.Say(playerid, message);
                return;
            }
            else if (name == null && id != 0)
            {
                vAsset = (ItemAsset)Assets.find(EAssetType.Item, id);
                name = vAsset.Name;
            }
            // Get how many they have
            if (playerid.Inventory.has(id) == null)
            {
                message = String.Format(UconomyBasicShop.Instance.Configuration.NoHaveItemSell, name);
                RocketChatManager.Say(playerid, message);
                return;
            }
            List<InventorySearch> list = playerid.Inventory.search(id, true, true);
            if (list.Count == 0 || (vAsset.Amount == 1 && list.Count < amttosell))
            {
                message = String.Format(UconomyBasicShop.Instance.Configuration.NotEnoughItemsSell, amttosell.ToString(), name);
                RocketChatManager.Say(playerid, message);
                return;
            }
            if (vAsset.Amount > 1)
            {
                int ammomagamt = 0;
                foreach (InventorySearch ins in list)
                {
                    ammomagamt += ins.ItemJar.Item.Amount;
                }
                if (ammomagamt < amttosell)
                {
                    message = String.Format(UconomyBasicShop.Instance.Configuration.NotEnoughAmmoSell, name);
                    RocketChatManager.Say(playerid, message);
                    return;
                }
            }
            // We got this far, so let's buy back the items and give them money.
            // Get cost per item.  This will be whatever is set for most items, but changes for ammo and magazines.
            decimal price = UconomyBasicShop.Instance.ShopDB.GetItemBuyPrice(id);
            decimal peritemprice = price / vAsset.Amount;
            decimal addmoney = decimal.Round(amttosell * peritemprice, 2);
            switch (vAsset.Amount)
            {
                case 1:
                    // These are single items, not ammo or magazines
                    while (amttosell > 0)
                    {
                        if (playerid.Player.Equipment.checkSelection(list[0].W, list[0].ItemJar.PositionX, list[0].ItemJar.PositionY))
                        {
                            playerid.Player.Equipment.dequip();
                        }
                        playerid.Inventory.removeItem(list[0].W, playerid.Inventory.getIndex(list[0].W, list[0].ItemJar.PositionX, list[0].ItemJar.PositionY));
                        list.RemoveAt(0);
                        amttosell--;
                    }
                    break;
                default:
                    // This is ammo or magazines
                    while (amttosell > 0)
                    {
                        if (playerid.Player.Equipment.checkSelection(list[0].W, list[0].ItemJar.PositionX, list[0].ItemJar.PositionY))
                        {
                            playerid.Player.Equipment.dequip();
                        }
                        if (list[0].ItemJar.Item.Amount >= amttosell)
                        {
                            byte left = (byte)(list[0].ItemJar.Item.Amount - amttosell);
                            playerid.Inventory.sendUpdateAmount(list[0].W, list[0].ItemJar.PositionX, list[0].ItemJar.PositionY, left);
                            amttosell = 0;
                            if (left == 0)
                            {
                                playerid.Inventory.removeItem(list[0].W, playerid.Inventory.getIndex(list[0].W, list[0].ItemJar.PositionX, list[0].ItemJar.PositionY));
                                list.RemoveAt(0);
                            }
                        }
                        else
                        {
                            amttosell -= list[0].ItemJar.Item.Amount;
                            playerid.Inventory.sendUpdateAmount(list[0].W, list[0].ItemJar.PositionX, list[0].ItemJar.PositionY, 0);
                            playerid.Inventory.removeItem(list[0].W, playerid.Inventory.getIndex(list[0].W, list[0].ItemJar.PositionX, list[0].ItemJar.PositionY));
                            list.RemoveAt(0);
                        }
                    }
                    break;
            }
            decimal balance = Uconomy.Instance.Database.IncreaseBalance(playerid.CSteamID, addmoney);
            message = String.Format(UconomyBasicShop.Instance.Configuration.SoldItems, amt, name, addmoney, Uconomy.Instance.Configuration.MoneyName, balance, Uconomy.Instance.Configuration.MoneyName);
            if (UconomyBasicShop.Instance.OnShopSell != null)
                UconomyBasicShop.Instance.OnShopSell(playerid, addmoney, amttosell, id);
            RocketChatManager.Say(playerid, message);
        }

    }
}
