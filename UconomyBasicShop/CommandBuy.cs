using System;
using Rocket.RocketAPI;

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

        public void Execute(RocketPlayer playerid, string[] msg)
        {
            UconomyBasicShop.Instance.Buy(playerid, msg);
        }
    }
}