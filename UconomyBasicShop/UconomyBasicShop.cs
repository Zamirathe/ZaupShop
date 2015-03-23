using System;
using System.Collections.Generic;
using Rocket.RocketAPI;
using unturned.ROCKS.Uconomy;

namespace UconomyBasicShop
{
    public class UconomyBasicShop : RocketPlugin<UconomyBasicShopConfiguration>
    {
        public DatabaseMgr ShopDB;
        public static UconomyBasicShop Instance;
        
        protected override void Load()
        {
            UconomyBasicShop.Instance = this;
            this.ShopDB = new DatabaseMgr();
        }
    }
}
