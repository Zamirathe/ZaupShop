using System;
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

        public void Execute(RocketPlayer playerid, string[] msg)
        {
            UconomyBasicShop.Instance.Sell(playerid, msg);
        }
    }
}