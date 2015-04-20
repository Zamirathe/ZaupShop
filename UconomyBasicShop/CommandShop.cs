using System;
using System.Collections.Generic;
using Rocket.RocketAPI;
using Rocket.Logging;
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

        public void Execute(RocketPlayer playerid, string msg)
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
                RocketChatManager.Say(playerid, "You don't have permission to use the /shop command.");
                return;
            }
            if (string.IsNullOrEmpty(msg))
            {
                message = "Usage: /shop <add/rem/chng/buy>/[v.]<itemid>/<cost>  <cost> is not required for rem, buy is only for items.";
                // We are going to print how to use
                this.sendMessage(playerid, message, console);
                return;
            }
            string[] command = Parser.getComponentsFromSerial(msg, '/');
            if (command.Length < 2)
            {
                message = "An itemid is required.";
                this.sendMessage(playerid, message, console);
                return;
            }
            if (command.Length == 2 && command[0] != "rem")
            {
                message = "A cost is required.";
                this.sendMessage(playerid, message, console);
                return;
            }
            else if (command.Length >= 2)
            {
                string[] type = Parser.getComponentsFromSerial(command[1], '.');
                if (type.Length > 1 && type[0] != "v")
                {
                    message = "You must specify v for vehicle or just an item id.  Ex. /shop rem/101";
                    this.sendMessage(playerid, message, console);
                    return;
                }
                ushort id;
                if (type.Length > 1)
                {
                    if (!ushort.TryParse(type[1], out id)) {
                        message = "You need to provide an item or vehicle id.";
                        this.sendMessage(playerid, message, console);
                        return;
                    }
                } else {
                    if (!ushort.TryParse(type[0], out id))
                    {
                        message = "You need to provide an item or vehicle id.";
                        this.sendMessage(playerid, message, console);
                        return;
                    }
                }
                // All basic checks complete.  Let's get down to business.
                bool success = false;
                bool change = false;
                bool pass = false;
                switch (command[0])
                {
                    case "chng":
                        if (!perms[3] && !perms[0])
                        {
                            message = "You don't have permission to use the shop chng command.";
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
                                message = "You don't have permission to use the shop add command.";
                                this.sendMessage(playerid, message, console);
                                return;
                            }
                        }
                        string ac = (pass) ? "changed" : "added";
                        switch (type[0])
                        {
                            case "v":
                                VehicleAsset va = (VehicleAsset)Assets.find(EAssetType.Vehicle, id);
                                message = "You have " + ac + " the " + va.Name + " with cost " + command[2] + " to the shop.";
                                success = UconomyBasicShop.Instance.ShopDB.AddVehicle((int)id, va.Name, decimal.Parse(command[2]), change);
                                if (!success)
                                {
                                    message = "There was an error adding/changing " + va.Name + "!";
                                }
                                this.sendMessage(playerid, message, console);
                                break;
                            default:
                                ItemAsset ia = (ItemAsset)Assets.find(EAssetType.Item, id);
                                message = "You have " + ac + " the " + ia.Name + " with cost " + command[2] + " to the shop.";
                                success = UconomyBasicShop.Instance.ShopDB.AddItem((int)id, ia.Name, decimal.Parse(command[2]), change);
                                if (!success)
                                {
                                    message = "There was an error adding/changing " + ia.Name + "!";
                                }
                                this.sendMessage(playerid, message, console);
                                break;
                        }
                        break;
                    case "rem":
                        if (!perms[2] && !perms[0])
                        {
                            message = "You don't have permission to use the shop rem command.";
                            this.sendMessage(playerid, message, console);
                            return;
                        }
                        switch (type[0])
                        {
                            case "v":
                                VehicleAsset va = (VehicleAsset)Assets.find(EAssetType.Vehicle, id);
                                message = "You have removed the " + va.Name + "from the shop.";
                                success = UconomyBasicShop.Instance.ShopDB.DeleteVehicle((int)id);
                                if (!success)
                                {
                                    message = va.Name + " wasn't in the shop, so couldn't be removed.";
                                }
                                this.sendMessage(playerid, message, console);
                                break;
                            default:
                                ItemAsset ia = (ItemAsset)Assets.find(EAssetType.Item, id);
                                message = "You have removed the " + ia.Name + " from the shop.";
                                success = UconomyBasicShop.Instance.ShopDB.DeleteItem((int)id);
                                if (!success)
                                {
                                    message = ia.Name + " wasn't in the shop, so couldn't be removed.";
                                }
                                this.sendMessage(playerid, message, console);
                                break;
                        }
                        break;
                    case "buy":
                        if (!perms[4] && !perms[0])
                        {
                            message = "You don't have permission to use the shop buy command.";
                            this.sendMessage(playerid, message, console);
                            return;
                        }
                        ItemAsset iab = (ItemAsset)Assets.find(EAssetType.Item, id);
                        decimal buyb;
                        decimal.TryParse(command[2], out buyb);
                        message = "You set the buyback price for " + iab.Name + " to " + buyb.ToString() + " in the shop.";
                        success = UconomyBasicShop.Instance.ShopDB.SetBuyPrice((int)id, buyb);
                        if (!success)
                        {
                            message = iab.Name + " isn't in the shop so can't set a buyback price.";
                        }
                        this.sendMessage(playerid, message, console);
                        break;
                    default:
                        // We shouldn't get this, but if we do send an error.
                        message = "You entered an invalid shop command.";
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
                RocketChatManager.Say(playerid, message);
            }
        }
    }
}