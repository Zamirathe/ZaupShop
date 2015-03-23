using System;
using Rocket.RocketAPI;
using Rocket.Logging;

namespace UconomyBasicShop
{
    public class UconomyBasicShopConfiguration : RocketConfiguration
    {
        public string ItemShopTableName;
        public string VehicleShopTableName;
        public bool CanBuyItems;
        public bool CanBuyVehicles;
        public string ItemCostMsg;
        public string VehicleCostMsg;
        public string ItemBuyMsg;
        public string VehicleBuyMsg;
        public string NotEnoughCurrencyMsg;
        public string BuyItemsOff;
        public string BuyVehiclesOff;
        public string ItemNotAvailable;
        public string VehicleNotAvailable;
        public string CouldNotFind;

        public RocketConfiguration DefaultConfiguration
        {
            get
            {
                return new UconomyBasicShopConfiguration
                {
                    ItemShopTableName = "uconomyitemshop",
                    VehicleShopTableName = "uconomyvehicleshop",
                    CanBuyItems = true,
                    CanBuyVehicles = false,
                    ItemCostMsg = "The item {0} costs {1} {2} to buy.",
                    VehicleCostMsg = "The vehicle {0} costs {1} {2} to buy.",
                    ItemBuyMsg = "You have bought {5} {0} for {1} {2}.  You now have {3} {4}.",
                    VehicleBuyMsg = "You have bought 1 {0} for {1} {2}.  You now have {3} {4}.",
                    NotEnoughCurrencyMsg = "You do not have enough {0} to buy {1} {2}.",
                    BuyItemsOff = "I'm sorry, but the ability to buy items is turned off.",
                    BuyVehiclesOff = "I'm sorry, but the ability to buy vehicles is turned off.",
                    ItemNotAvailable = "I'm sorry, but {0} is not available in the shop.",
                    VehicleNotAvailable = "I'm sorry, but {0} is not available in the shop.",
                    CouldNotFind = "I'm sorry, I couldn't find an id for {0}."
                };
            }
        }
    }
}
