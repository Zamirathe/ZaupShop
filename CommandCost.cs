﻿using System;
using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Commands;
using Rocket.Unturned.Player;
using SDG;
using UnityEngine;
using unturned.ROCKS.Uconomy;
using Steamworks;

namespace ZaupShop
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
        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public void Execute(RocketPlayer playerid, string[] msg)
        {
            ZaupShop.Instance.Cost(playerid, msg);
        }
    }
}