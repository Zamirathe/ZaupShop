using System;
using System.Collections.Generic;

using Rocket.API;
using Rocket.Unturned.Commands;
using Rocket.Unturned.Player;

namespace ZaupShop
{
    public class CommandSell : IRocketCommand
    {

        public AllowedCaller AllowedCaller
        {
            get
            {
                return AllowedCaller.Player;
            }
        }
        public string Name
        {
            get
            {
                return "sell";
            }
        }
        public string Help
        {
            get
            {
                return "Allows you to sell items to the shop from your inventory.";
            }
        }
        public string Syntax
        {
            get
            {
                return "<name or id> [amount]";
            }
        }
        public List<string> Aliases
        {
            get { return new List<string>(); }
        }
        public List<string> Permissions
        {
            get { return new List<string>(); }
        }
        public void Execute(IRocketPlayer playerid, string[] msg)
        {
            ZaupShop.Instance.Sell((UnturnedPlayer)playerid, msg);
        }
    }
}