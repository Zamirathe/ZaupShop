using System;
using Rocket.API;

namespace UconomyBasicShop
{
    public class UconomyBasicShopConfiguration : IRocketPluginConfiguration
    {
        public string ItemShopTableName;
        public string VehicleShopTableName;
        public bool CanBuyItems;
        public bool CanBuyVehicles;
        public bool CanSellItems;
        public bool QualityCounts;

        public IRocketPluginConfiguration DefaultConfiguration
        {
            get
            {
                return new UconomyBasicShopConfiguration
                {
                    ItemShopTableName = "uconomyitemshop",
                    VehicleShopTableName = "uconomyvehicleshop",
                    CanBuyItems = true,
                    CanBuyVehicles = false,
                    CanSellItems = true,
                    QualityCounts = true
                };
            }
        }
    }
}
