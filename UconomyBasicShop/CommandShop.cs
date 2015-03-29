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

        public void Execute(CSteamID playerid, string msg)
        {
            bool console = (!string.IsNullOrEmpty(playerid.ToString()) && playerid.ToString() != "0") ? false : true;
            SteamPlayer splayer = PlayerTool.getSteamPlayer(playerid);
            string[] permnames = {"shop.*", "shop.add", "shop.rem", "shop.chng"};
            bool[] perms = {false, false, false, false};
            bool anyuse = false;
            string message;
            string[] permlist = RocketPermissionManager.GetPermissions(playerid);
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
                    case "*":
                        perms[0] = true;
                        perms[1] = true;
                        perms[2] = true;
                        perms[3] = true;
                        anyuse = true;
                        break;
                }
            }
            if (console || splayer.IsAdmin)
            {
                perms[0] = true;
                perms[1] = true;
                perms[2] = true;
                perms[3] = true;
                anyuse = true;
            }
            if (!anyuse) {
                // Assume this is a player
                RocketChatManager.Say(playerid, "You don't have permission to use the /shop command.");
                return;
            }
            if (string.IsNullOrEmpty(msg))
            {
                message = "Usage: /shop <add/rem/chng>/<v or i>.<itemid>/<cost>  <cost> is not required for rem.";
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
            } else if (command.Length >= 2) {
                string[] type = Parser.getComponentsFromSerial(command[1], '.');
                if (type.Length < 2 || (type[0] != "i" && type[0] != "v"))
                {
                    message = "You must specify i for Item or v for vehicle.  Ex. /shop rem/i.101";
                    this.sendMessage(playerid, message, console);
                    return;
                }
                ushort id;
                if (!ushort.TryParse(type[1], out id)) {
                    message = "You need to provide an item or vehicle id.";
                    this.sendMessage(playerid, message, console);
                }
                if ((command[0] == "add" || command[0] == "chng") && command.Length < 3) {
                    message = "You must specify a cost.";
                    this.sendMessage(playerid, message, console);
                    return;
                }
                // All basic checks complete.  Let's get down to business.
                bool success = false;
                bool change = false;
                bool pass = false;
                switch (command[0]) {
                    case "chng":
                        if (!perms[3] && !perms[0]) {
                            message = "You don't have permission to use the shop chng command.";
                            this.sendMessage(playerid, message, console);
                            return;
                        }
                        change = true;
                        pass = true;
                        goto case "add";
                    case "add":
                        if (!pass) {
                            if (!perms[1] && !perms[0]) {
                                message = "You don't have permission to use the shop add command.";
                                this.sendMessage(playerid, message, console);
                                return;
                            }
                        }
                        string ac = (pass) ? "changed" : "added";
                        switch (type[0]) {
                            case "v":
                                VehicleAsset va = (VehicleAsset)Assets.find(EAssetType.Vehicle, id);
                                message = "You have "+ ac +" the " + va.Name + " with cost " + command[2] + " to the shop.";
                                success = UconomyBasicShop.Instance.ShopDB.AddVehicle((int)id, va.Name, decimal.Parse(command[2]), change);
                                if (!success) {
                                    message = "There was an error adding/changing " + va.Name + "!";
                                }
                                this.sendMessage(playerid, message, console);
                                break;
                            default:
                                ItemAsset ia = (ItemAsset)Assets.find(EAssetType.Item, id);
                                message = "You have "+ ac +" the " + ia.Name + " with cost " + command[2] + " to the shop.";
                                success = UconomyBasicShop.Instance.ShopDB.AddItem((int)id, ia.Name, decimal.Parse(command[2]), change);
                                if (!success) {
                                    message = "There was an error adding/changing " + ia.Name + "!";
                                }
                                this.sendMessage(playerid, message, console);
                                break;
                        }
                        break;
                    case "rem":
                        if (!perms[2] && !perms[0]) {
                                message = "You don't have permission to use the shop rem command.";
                                this.sendMessage(playerid, message, console);
                                return;
                            }
                        switch (type[0]) {
                            case "v":
                                VehicleAsset va = (VehicleAsset)Assets.find(EAssetType.Vehicle, id);
                                message = "You have removed the " + va.Name + "from the shop.";
                                success = UconomyBasicShop.Instance.ShopDB.DeleteVehicle((int)id);
                                if (!success) {
                                    message = "There was an error removing " + va.Name + "!";
                                }
                                this.sendMessage(playerid, message, console);
                                break;
                            default:
                                ItemAsset ia = (ItemAsset)Assets.find(EAssetType.Item, id);
                                message = "You have removed the " + ia.Name + " from the shop.";
                                success = UconomyBasicShop.Instance.ShopDB.DeleteItem((int)id);
                                if (!success) {
                                    message = "There was an error removing " + ia.Name + "!";
                                }
                                this.sendMessage(playerid, message, console);
                                break;
                        }
                        break;
                    default:
                        // We shouldn't get this, but if we do send an error.
                        message = "You entered an invalid shop command.";
                        this.sendMessage(playerid, message, console);
                        return;
                }
            }
        }
        private void sendMessage(CSteamID playerid, string message, bool console)
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