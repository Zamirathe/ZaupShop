using System;
using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Commands;
using Rocket.Unturned.Player;

namespace UconomyBasicShop
{
    public class CommandSell : IRocketCommand
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

        public void Execute(RocketPlayer playerid, string[] msg)
        {
            UconomyBasicShop.Instance.Sell(playerid, msg);
        }
    }
}