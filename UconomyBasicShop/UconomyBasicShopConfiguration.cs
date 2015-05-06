using System;
using Rocket.RocketAPI;
using Rocket.Logging;

namespace UconomyBasicShop
{
    public class UconomyBasicShopConfiguration : IRocketConfiguration
    {
        public string ItemShopTableName;
        public string VehicleShopTableName;
        public bool CanBuyItems;
        public bool CanBuyVehicles;
        public bool CanSellItems;
        public bool QualityCounts;

        public IRocketConfiguration DefaultConfiguration
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
