using System;
using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned;
using Rocket.Unturned.Player;
using Rocket.Unturned.Plugins;
using Rocket.Unturned.Logging;
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
        public override Dictionary<string, string> DefaultTranslations
        {
            get
            {
                return new Dictionary<string, string>
                {
                    {
                        "buy_command_usage",
                        "Usage: /buy [v.]<name or id> [amount] [quality of 25, 50, 75, or 100] (last 2 optional and only for items, default 1 amount, 100 quality)."
                    },
                    {
                        "cost_command_usage",
                        "Usage: /cost [v.]<name or id>."
                    },
                    {
                        "sell_command_usage",
                        "Usage: /sell <name or id> [amount] (optional)."
                    },
                    {
                        "shop_command_usage",
                        "Usage: /shop <add/rem/chng/buy> [v.]<itemid> <cost>  <cost> is not required for rem, buy is only for items."
                    },
                    {
                        "error_giving_item",
                        "There was an error giving you {0}.  You have not been charged."
                    },
                    {
                        "error_getting_cost",
                        "There was an error getting the cost of {0}!"
                    },
                    {
                        "item_cost_msg",
                        "The item {0} costs {1} {2} to buy and gives {3} {4} when you sell it."
                    },
                    {
                        "vehicle_cost_msg",
                        "The vehicle {0} costs {1} {2} to buy."
                    },
                    {
                        "item_buy_msg",
                        "You have bought {5} {0} for {1} {2}.  You now have {3} {4}."
                    },
                    {
                        "vehicle_buy_msg",
                        "You have bought 1 {0} for {1} {2}.  You now have {3} {4}."
                    },
                    {
                        "not_enough_currency_msg",
                        "You do not have enough {0} to buy {1} {2}."
                    },
                    {
                        "buy_items_off",
                        "I'm sorry, but the ability to buy items is turned off."
                    },
                    {
                        "buy_vehicles_off",
                        "I'm sorry, but the ability to buy vehicles is turned off."
                    },
                    {
                        "item_not_available",
                        "I'm sorry, but {0} is not available in the shop."
                    },
                    {
                        "vehicle_not_available",
                        "I'm sorry, but {0} is not available in the shop."
                    },
                    {
                        "could_not_find",
                        "I'm sorry, I couldn't find an id for {0}."
                    },
                    {
                        "sell_items_off",
                        "I'm sorry, but the ability to sell items is turned off."
                    },
                    {
                        "not_have_item_sell",
                        "I'm sorry, but you don't have any {0} to sell."
                    },
                    {
                        "not_enough_items_sell",
                        "I'm sorry, but you don't have {0} {1} to sell."
                    },
                    {
                        "not_enough_ammo_sell",
                        "I'm sorry, but you don't have enough ammo in {0} to sell."
                    },
                    {
                        "sold_items",
                        "You have sold {0} {1} to the shop and receive {2} {3} in return.  Your balance is now {4} {5}."
                    },
                    {
                        "no_sell_price_set",
                        "The shop is not buying {0} right now"
                    },
                    {
                        "no_itemid_given",
                        "An itemid is required."
                    },
                    {
                        "no_cost_given",
                        "A cost is required."
                    },
                    {
                        "v_not_provided",
                        "You must specify v for vehicle or just an item id.  Ex. /shop rem/101"
                    },
                    {
                        "invalid_id_given",
                        "You need to provide an item or vehicle id."
                    },
                    {
                        "no_permission_shop_chng",
                        "You don't have permission to use the shop chng msg."
                    },
                    {
                        "no_permission_shop_add",
                        "You don't have permission to use the shop add msg."
                    },
                    {
                        "no_permission_shop_rem",
                        "You don't have permission to use the shop rem msg."
                    },
                    {
                        "no_permission_shop_buy",
                        "You don't have permission to use the shop buy msg."
                    },
                    {
                        "changed",
                        "changed"
                    },
                    {
                        "added",
                        "added"
                    },
                    {
                        "changed_or_added_to_shop",
                        "You have {0} the {1} with cost {2} to the shop."
                    },
                    {
                        "error_adding_or_changing",
                        "There was an error adding/changing {0}!"
                    },
                    {
                        "removed_from_shop",
                        "You have removed the {0} from the shop."
                    },
                    {
                        "not_in_shop_to_remove",
                        "{0} wasn't in the shop, so couldn't be removed."
                    },
                    {
                        "not_in_shop_to_set_buyback",
                        "{0} isn't in the shop so can't set a buyback price."
                    },
                    {
                        "set_buyback_price",
                        "You set the buyback price for {0} to {1} in the shop."
                    },
                    {
                        "invalid_shop_command",
                        "You entered an invalid shop command."
                    }
                };
            }
        }
        
        protected override void Load()
        {
            UconomyBasicShop.Instance = this;
            this.ShopDB = new DatabaseMgr();
        }

        public delegate void PlayerShopBuy(RocketPlayer player, decimal amt, byte items, ushort item, string type="item");
        public event PlayerShopBuy OnShopBuy;
        public delegate void PlayerShopSell(RocketPlayer player, decimal amt, byte items, ushort item);
        public event PlayerShopSell OnShopSell;

        public void Buy(RocketPlayer playerid, string[] components0)
        {
            string message;
            if (components0.Length == 0)
            {
                message = UconomyBasicShop.Instance.Translate("buy_command_usage", new object[] {});
                // We are going to print how to use
                RocketChat.Say(playerid, message);
                return;
            }
            byte amttobuy = 1;
            if (components0.Length > 1)
            {
                amttobuy = byte.Parse(components0[1]);
            }
            string[] components = Parser.getComponentsFromSerial(components0[0], '.');
            if (components.Length == 2 && components[0] != "v")
            {
                message = UconomyBasicShop.Instance.Translate("buy_command_usage", new object[] { });
                // We are going to print how to use
                RocketChat.Say(playerid, message);
                return;
            }
            ushort id;
            switch (components[0])
            {
                case "v":
                    if (!UconomyBasicShop.Instance.Configuration.CanBuyVehicles)
                    {
                        message = UconomyBasicShop.Instance.Translate("buy_vehicles_off", new object[] { });
                        RocketChat.Say(playerid, message);
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
                        message = UconomyBasicShop.Instance.Translate("could_not_find", new object[] {
                            components[1]});
                        RocketChat.Say(playerid, message);
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
                        message = UconomyBasicShop.Instance.Translate("vehicle_not_available", new object[] {name});
                        RocketChat.Say(playerid, message);
                        return;
                    }
                    if (balance < cost)
                    {
                        message = UconomyBasicShop.Instance.Translate("not_enough_currency_msg", new object[] {Uconomy.Instance.Configuration.MoneyName, name});
                        RocketChat.Say(playerid, message);
                        return;
                    }
                    if (!playerid.GiveVehicle(id))
                    {
                        message = UconomyBasicShop.Instance.Translate("error_giving_item", new object[] { name });
                        RocketChat.Say(playerid, message);
                        return;
                    }
                    decimal newbal = Uconomy.Instance.Database.IncreaseBalance(playerid.CSteamID, (cost * -1));
                    message = UconomyBasicShop.Instance.Translate("vehicle_buy_msg", new object[] {name, cost, Uconomy.Instance.Configuration.MoneyName, newbal, Uconomy.Instance.Configuration.MoneyName});
                    if (UconomyBasicShop.Instance.OnShopBuy != null)
                        UconomyBasicShop.Instance.OnShopBuy(playerid, cost, 1, id, "vehicle");
                    playerid.Player.gameObject.SendMessage("ZaupShopOnBuy", new object[] { playerid, cost, amttobuy, id, "vehicle" }, SendMessageOptions.DontRequireReceiver);
                    RocketChat.Say(playerid, message);
                    break;
                default:
                    if (!UconomyBasicShop.Instance.Configuration.CanBuyItems)
                    {
                        message = UconomyBasicShop.Instance.Translate("buy_items_off", new object[] { });
                        RocketChat.Say(playerid, message);
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
                        message = UconomyBasicShop.Instance.Translate("could_not_find", new object[] {components[0]});
                        RocketChat.Say(playerid, message);
                        return;

                    }
                    else if (name == null && id != 0)
                    {
                        name = ((ItemAsset)Assets.find(EAssetType.Item, id)).Name;
                    }
                    cost = decimal.Round(UconomyBasicShop.Instance.ShopDB.GetItemCost(id) * amttobuy, 2);
                    balance = Uconomy.Instance.Database.GetBalance(playerid.CSteamID);
                    if (cost <= 0m)
                    {
                        message = UconomyBasicShop.Instance.Translate("item_not_available", new object[] {name});
                        RocketChat.Say(playerid, message);
                        return;
                    }
                    if (balance < cost)
                    {
                        message = UconomyBasicShop.Instance.Translate("not_enough_currency_msg", new object[] {Uconomy.Instance.Configuration.MoneyName, amttobuy, name});
                        RocketChat.Say(playerid, message);
                        return;
                    }
                    playerid.GiveItem(id, amttobuy);
                    newbal = Uconomy.Instance.Database.IncreaseBalance(playerid.CSteamID, (cost * -1));
                    message = UconomyBasicShop.Instance.Translate("item_buy_msg", new object[] {name, cost, Uconomy.Instance.Configuration.MoneyName, newbal, Uconomy.Instance.Configuration.MoneyName, amttobuy});
                    if (UconomyBasicShop.Instance.OnShopBuy != null)
                        UconomyBasicShop.Instance.OnShopBuy(playerid, cost, amttobuy, id);
                    playerid.Player.gameObject.SendMessage("ZaupShopOnBuy", new object[] { playerid, cost, amttobuy, id, "item" }, SendMessageOptions.DontRequireReceiver);
                    RocketChat.Say(playerid, message);
                    break;
            }
        }
        public void Cost(RocketPlayer playerid, string[] components)
        {
            string message;
            if (components.Length == 0)
            {
                message = UconomyBasicShop.Instance.Translate("cost_command_usage", new object[] { });
                // We are going to print how to use
                RocketChat.Say(playerid, message);
                return;
            }
            if (components.Length == 2 && components[0] != "v")
            {
                message = UconomyBasicShop.Instance.Translate("cost_command_usage", new object[] { });
                // We are going to print how to use
                RocketChat.Say(playerid, message);
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
                        message = UconomyBasicShop.Instance.Translate("could_not_find", new object[] {components[1]});
                        RocketChat.Say(playerid, message);
                        return;
                    }
                    else if (name == null && id != 0)
                    {
                        name = ((VehicleAsset)Assets.find(EAssetType.Vehicle, id)).Name;
                    }
                    decimal cost = UconomyBasicShop.Instance.ShopDB.GetVehicleCost(id);
                    message = UconomyBasicShop.Instance.Translate("vehicle_cost_msg", new object[] {name, cost.ToString(), Uconomy.Instance.Configuration.MoneyName});
                    if (cost <= 0m)
                    {
                        message = UconomyBasicShop.Instance.Translate("error_getting_cost", new object[] {name});
                    }
                    RocketChat.Say(playerid, message);
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
                        message = UconomyBasicShop.Instance.Translate("could_not_find", new object[] {components[0]});
                        RocketChat.Say(playerid, message);
                        return;
                    }
                    else if (name == null && id != 0)
                    {
                        name = ((ItemAsset)Assets.find(EAssetType.Item, id)).Name;
                    }
                    cost = UconomyBasicShop.Instance.ShopDB.GetItemCost(id);
                    decimal bbp = UconomyBasicShop.Instance.ShopDB.GetItemBuyPrice(id);
                    message = UconomyBasicShop.Instance.Translate("item_cost_msg", new object[] {name, cost.ToString(), Uconomy.Instance.Configuration.MoneyName, bbp.ToString(), Uconomy.Instance.Configuration.MoneyName});
                    if (cost <= 0m)
                    {
                        message = UconomyBasicShop.Instance.Translate("error_getting_cost", new object[] {name});
                    }
                    RocketChat.Say(playerid, message);
                    break;
            }
        }
        public void Sell(RocketPlayer playerid, string[] components)
        {
            string message;
            if (components.Length == 0)
            {
                message = UconomyBasicShop.Instance.Translate("sell_command_usage", new object[] { });
                // We are going to print how to use
                RocketChat.Say(playerid, message);
                return;
            }
            byte amttosell = 1;
            if (components.Length > 1)
            {
                amttosell = byte.Parse(components[1]);
            }
            byte amt = amttosell;
            ushort id;
            if (!UconomyBasicShop.Instance.Configuration.CanSellItems)
            {
                message = UconomyBasicShop.Instance.Translate("sell_items_off", new object[] { });
                RocketChat.Say(playerid, message);
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
                message = UconomyBasicShop.Instance.Translate("could_not_find", new object[] {components[0]});
                RocketChat.Say(playerid, message);
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
                message = UconomyBasicShop.Instance.Translate("not_have_item_sell", new object[] {name});
                RocketChat.Say(playerid, message);
                return;
            }
            List<InventorySearch> list = playerid.Inventory.search(id, true, true);
            if (list.Count == 0 || (vAsset.Amount == 1 && list.Count < amttosell))
            {
                message = UconomyBasicShop.Instance.Translate("not_enough_items_sell", new object[] {amttosell.ToString(), name});
                RocketChat.Say(playerid, message);
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
                    message = UconomyBasicShop.Instance.Translate("not_enough_ammo_sell", new object[] {name});
                    RocketChat.Say(playerid, message);
                    return;
                }
            }
            // We got this far, so let's buy back the items and give them money.
            // Get cost per item.  This will be whatever is set for most items, but changes for ammo and magazines.
            decimal price = UconomyBasicShop.Instance.ShopDB.GetItemBuyPrice(id);
            if (price <= 0.00m)
            {
                message = UconomyBasicShop.Instance.Translate("no_sell_price_set", new object[] { name });
                RocketChat.Say(playerid, message);
                return;
            }
            byte quality = 100;
            decimal peritemprice = 0;
            decimal addmoney = 0;
            switch (vAsset.Amount)
            {
                case 1:
                    // These are single items, not ammo or magazines
                    while (amttosell > 0)
                    {
                        if (playerid.Player.Equipment.checkSelection(list[0].InventoryGroup, list[0].ItemJar.PositionX, list[0].ItemJar.PositionY))
                        {
                            playerid.Player.Equipment.dequip();
                        }
                        if (UconomyBasicShop.Instance.Configuration.QualityCounts)
                            quality = list[0].ItemJar.Item.Durability;
                        peritemprice = decimal.Round(price * (quality / 100.0m), 2);
                        addmoney += peritemprice;
                        playerid.Inventory.removeItem(list[0].InventoryGroup, playerid.Inventory.getIndex(list[0].InventoryGroup, list[0].ItemJar.PositionX, list[0].ItemJar.PositionY));
                        list.RemoveAt(0);
                        amttosell--;
                    }
                    break;
                default:
                    // This is ammo or magazines
                    byte amttosell1 = amttosell;
                    while (amttosell > 0)
                    {
                        if (playerid.Player.Equipment.checkSelection(list[0].InventoryGroup, list[0].ItemJar.PositionX, list[0].ItemJar.PositionY))
                        {
                            playerid.Player.Equipment.dequip();
                        }
                        if (list[0].ItemJar.Item.Amount >= amttosell)
                        {
                            byte left = (byte)(list[0].ItemJar.Item.Amount - amttosell);
                            list[0].ItemJar.Item.Amount = left;
                            playerid.Inventory.sendUpdateAmount(list[0].InventoryGroup, list[0].ItemJar.PositionX, list[0].ItemJar.PositionY, left);
                            amttosell = 0;
                            if (left == 0)
                            {
                                playerid.Inventory.removeItem(list[0].InventoryGroup, playerid.Inventory.getIndex(list[0].InventoryGroup, list[0].ItemJar.PositionX, list[0].ItemJar.PositionY));
                                list.RemoveAt(0);
                            }
                        }
                        else
                        {
                            amttosell -= list[0].ItemJar.Item.Amount;
                            playerid.Inventory.sendUpdateAmount(list[0].InventoryGroup, list[0].ItemJar.PositionX, list[0].ItemJar.PositionY, 0);
                            playerid.Inventory.removeItem(list[0].InventoryGroup, playerid.Inventory.getIndex(list[0].InventoryGroup, list[0].ItemJar.PositionX, list[0].ItemJar.PositionY));
                            list.RemoveAt(0);
                        }
                    }
                    peritemprice = decimal.Round(price * ((decimal)amttosell1 / (decimal)vAsset.Amount), 2);
                    addmoney += peritemprice;
                    break;
            }
            decimal balance = Uconomy.Instance.Database.IncreaseBalance(playerid.CSteamID, addmoney);
            message = UconomyBasicShop.Instance.Translate("sold_items", new object[] {amt, name, addmoney, Uconomy.Instance.Configuration.MoneyName, balance, Uconomy.Instance.Configuration.MoneyName});
            if (UconomyBasicShop.Instance.OnShopSell != null)
                UconomyBasicShop.Instance.OnShopSell(playerid, addmoney, amt, id);
            playerid.Player.gameObject.SendMessage("ZaupShopOnSell", new object[] { playerid, addmoney, amt, id }, SendMessageOptions.DontRequireReceiver);
            RocketChat.Say(playerid, message);
        }

    }
}
