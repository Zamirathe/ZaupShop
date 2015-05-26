using System;
using Rocket.API;
using Rocket.Unturned.Commands;
using Rocket.Unturned.Player;
using SDG;
using UnityEngine;
using unturned.ROCKS.Uconomy;
using Steamworks;

namespace UconomyBasicShop
{
    public class CommandCost : IRocketCommand
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
                return "cost";
            }
        }
        public string Help
        {
            get
            {
                return "Tells you the cost of a selected item.";
            }
        }
        public string Syntax
        {
            get
            {
                return "[v.]<name or id>";
            }
        }

        public void Execute(RocketPlayer playerid, string[] msg)
        {
            UconomyBasicShop.Instance.Cost(playerid, msg);
        }
    }
}