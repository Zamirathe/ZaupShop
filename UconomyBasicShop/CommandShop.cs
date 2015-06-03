using System;
using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned;
using Rocket.Unturned.Commands;
using Rocket.Unturned.Logging;
using Rocket.Unturned.Player;
using SDG;
using UnityEngine;
using Steamworks;

namespace UconomyBasicShop
{
    public class CommandShop : IRocketCommand
    {
        public bool RunFromConsole
        {
            get
            {
                return true;
            }
        }
        public string Name
        {
            get
            {
                return "shop";
            }
        }
        public string Help
        {
            get
            {
                return "Allows admins to change, add, or remove items/vehicles from the shop.";
            }
        }
        public string Syntax
        {
            get
            {
                return "<add | rem | chng | buy> [v.]<itemid> <cost>";
            }
        }
        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public void Execute(RocketPlayer playerid, string[] msg)
        {
            bool console = (playerid == null) ? true : false;
            string[] permnames = { "shop.*", "shop.add", "shop.rem", "shop.chng", "shop.buy" };
            bool[] perms = { false, false, false, false, false };
            bool anyuse = false;
            string message;
            List<string> permlist = (console) ? new List<string>() : playerid.Permissions;
            foreach (string s in permlist)
            {
                switch (s)
                {
                    case "shop.*":
                        perms[0] = true;
                        anyuse = true;
                        break;
                    case "shop.add":
                        perms[1] = true;
                        anyuse = true;
                        break;
                    case "shop.rem":
                        perms[2] = true;
                        anyuse = true;
                        break;
                    case "shop.chng":
                        perms[3] = true;
                        anyuse = true;
                        break;
                    case "shop.buy":
                        perms[4] = true;
                        anyuse = true;
                        break;
                    case "*":
                        perms[0] = true;
                        perms[1] = true;
                        perms[2] = true;
                        perms[3] = true;
                        perms[4] = true;
                        anyuse = true;
                        break;
                }
            }
            if (console || playerid.IsAdmin)
            {
                perms[0] = true;
                perms[1] = true;
                perms[2] = true;
                perms[3] = true;
                perms[4] = true;
                anyuse = true;
            }
            if (!anyuse)
            {
                // Assume this is a player
                RocketChat.Say(playerid, "You don't have permission to use the /shop command.");
                return;
            }
            if (msg.Length == 0)
            {
                message = UconomyBasicShop.Instance.Translate("shop_command_usage", new object[] {});
                // We are going to print how to use
                this.sendMessage(playerid, message, console);
                return;
            }
            if (msg.Length < 2)
            {
                message = UconomyBasicShop.Instance.Translate("no_itemid_given", new object[] {});
                this.sendMessage(playerid, message, console);
                return;
            }
            if (msg.Length == 2 && msg[0] != "rem")
            {
                message = UconomyBasicShop.Instance.Translate("no_cost_given", new object[] { });
                this.sendMessage(playerid, message, console);
                return;
            }
            else if (msg.Length >= 2)
            {
                string[] type = Parser.getComponentsFromSerial(msg[1], '.');
                if (type.Length > 1 && type[0] != "v")
                {
                    message = UconomyBasicShop.Instance.Translate("v_not_provided", new object[] { });
                    this.sendMessage(playerid, message, console);
                    return;
                }
                ushort id;
                if (type.Length > 1)
                {
                    if (!ushort.TryParse(type[1], out id)) {
                        message = UconomyBasicShop.Instance.Translate("invalid_id_given", new object[] { });
                        this.sendMessage(playerid, message, console);
                        return;
                    }
                } else {
                    if (!ushort.TryParse(type[0], out id))
                    {
                        message = UconomyBasicShop.Instance.Translate("invalid_id_given", new object[] { });
                        this.sendMessage(playerid, message, console);
                        return;
                    }
                }
                // All basic checks complete.  Let's get down to business.
                bool success = false;
                bool change = false;
                bool pass = false;
                switch (msg[0])
                {
                    case "chng":
                        if (!perms[3] && !perms[0])
                        {
                            message = UconomyBasicShop.Instance.Translate("no_permission_shop_chng", new object[] { });
                            this.sendMessage(playerid, message, console);
                            return;
                        }
                        change = true;
                        pass = true;
                        goto case "add";
                    case "add":
                        if (!pass)
                        {
                            if (!perms[1] && !perms[0])
                            {
                                message = UconomyBasicShop.Instance.Translate("no_permission_shop_add", new object[] { });
                                this.sendMessage(playerid, message, console);
                                return;
                            }
                        }
                        string ac = (pass) ? UconomyBasicShop.Instance.Translate("changed", new object[] { }) : UconomyBasicShop.Instance.Translate("added", new object[] { });
                        switch (type[0])
                        {
                            case "v":
                                VehicleAsset va = (VehicleAsset)Assets.find(EAssetType.Vehicle, id);
                                message = UconomyBasicShop.Instance.Translate("changed_or_added_to_shop", new object[] { 
                                    ac,
                                    va.Name,
                                    msg[2]
                                });
                                success = UconomyBasicShop.Instance.ShopDB.AddVehicle((int)id, va.Name, decimal.Parse(msg[2]), change);
                                if (!success)
                                {
                                    message = UconomyBasicShop.Instance.Translate("error_adding_or_changing", new object[] { va.Name });
                                }
                                this.sendMessage(playerid, message, console);
                                break;
                            default:
                                ItemAsset ia = (ItemAsset)Assets.find(EAssetType.Item, id);
                                message = UconomyBasicShop.Instance.Translate("changed_or_added_to_shop", new object[] { 
                                    ac,
                                    ia.Name,
                                    msg[2]
                                });
                                success = UconomyBasicShop.Instance.ShopDB.AddItem((int)id, ia.Name, decimal.Parse(msg[2]), change);
                                if (!success)
                                {
                                    message = UconomyBasicShop.Instance.Translate("error_adding_or_changing", new object[] { ia.Name });
                                }
                                this.sendMessage(playerid, message, console);
                                break;
                        }
                        break;
                    case "rem":
                        if (!perms[2] && !perms[0])
                        {
                            message = UconomyBasicShop.Instance.Translate("no_permission_shop_rem", new object[] { });
                            this.sendMessage(playerid, message, console);
                            return;
                        }
                        switch (type[0])
                        {
                            case "v":
                                VehicleAsset va = (VehicleAsset)Assets.find(EAssetType.Vehicle, id);
                                message = UconomyBasicShop.Instance.Translate("removed_from_shop", new object[] { va.Name });
                                success = UconomyBasicShop.Instance.ShopDB.DeleteVehicle((int)id);
                                if (!success)
                                {
                                    message = UconomyBasicShop.Instance.Translate("not_in_shop_to_remove", new object[] { va.Name });
                                }
                                this.sendMessage(playerid, message, console);
                                break;
                            default:
                                ItemAsset ia = (ItemAsset)Assets.find(EAssetType.Item, id);
                                message = UconomyBasicShop.Instance.Translate("removed_from_shop", new object[] { ia.Name });
                                success = UconomyBasicShop.Instance.ShopDB.DeleteItem((int)id);
                                if (!success)
                                {
                                    message = UconomyBasicShop.Instance.Translate("not_in_shop_to_remove", new object[] { ia.Name });
                                }
                                this.sendMessage(playerid, message, console);
                                break;
                        }
                        break;
                    case "buy":
                        if (!perms[4] && !perms[0])
                        {
                            message = UconomyBasicShop.Instance.Translate("no_permission_shop_buy", new object[] { });
                            this.sendMessage(playerid, message, console);
                            return;
                        }
                        ItemAsset iab = (ItemAsset)Assets.find(EAssetType.Item, id);
                        decimal buyb;
                        decimal.TryParse(msg[2], out buyb);
                        message = UconomyBasicShop.Instance.Translate("set_buyback_price", new object[] {
                            iab.Name,
                            buyb.ToString()
                        });
                        success = UconomyBasicShop.Instance.ShopDB.SetBuyPrice((int)id, buyb);
                        if (!success)
                        {
                            message = UconomyBasicShop.Instance.Translate("not_in_shop_to_buyback", new object[] { iab.Name });
                        }
                        this.sendMessage(playerid, message, console);
                        break;
                    default:
                        // We shouldn't get this, but if we do send an error.
                        message = UconomyBasicShop.Instance.Translate("not_in_shop_to_remove", new object[] { });;
                        this.sendMessage(playerid, message, console);
                        return;
                }
            }
        }
        private void sendMessage(RocketPlayer playerid, string message, bool console)
        {
            if (console)
            {
                Logger.Log(message);
            }
            else
            {
                RocketChat.Say(playerid, message);
            }
        }
    }
}