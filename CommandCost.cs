using System;
using System.Globalization;
using Rocket.API.Commands;
using Rocket.API.Economy;
using Rocket.API.Plugins;
using Rocket.Core.I18N;
using Rocket.Core.Plugins;
using Rocket.Core.User;
using SDG.Unturned;

namespace ZaupShop
{
    public class CommandCost : ICommand
    {
        private readonly IEconomyProvider _economy;
        private readonly Plugin _parentPlugin;

        public CommandCost(IPlugin plugin, IEconomyProvider economy)
        {
            _economy = economy;
            _parentPlugin = (Plugin) plugin;
        }

        public string Name => "cost";

        public string Summary => "Tells you the cost of a selected item.";
        public string Description => null;

        public string Syntax => "[v] <name or id>";
        public IChildCommand[] ChildCommands => null;

        public string[] Aliases => null;

        public string Permission => null;

        public bool SupportsUser(Type user)
        {
            return true;
        }

        public void Execute(ICommandContext context)
        {
            var user = context.User;
            var parameters = context.Parameters;
            string message;

            if (parameters.Length == 0 || parameters.Length == 1 && (parameters[0].Trim() == string.Empty || parameters[0].Trim() == "v"))
            {
                // We are going to print how to use
                context.User.SendLocalizedMessage(_parentPlugin.Translations, "cost_command_usage");
                return;
            }
            if (parameters.Length == 2 && (parameters[0] != "v" || parameters[1].Trim() == string.Empty))
            {
                context.User.SendLocalizedMessage(_parentPlugin.Translations, "cost_command_usage");
                return;
            }

            ushort id;
            switch (parameters[0])
            {
                case "v":
                    string name = null;
                    if (!ushort.TryParse(parameters[1], out id))
                    {
                        foreach (VehicleAsset vAsset in Assets.find(EAssetType.VEHICLE))
                        {
                            if (vAsset != null && vAsset.vehicleName != null && vAsset.vehicleName.ToLower().Contains(parameters[1].ToLower()))
                            {
                                id = vAsset.id;
                                name = vAsset.vehicleName;
                                break;
                            }
                        }
                    }
                    if (Assets.find(EAssetType.VEHICLE, id) == null)
                    {
                        context.User.SendLocalizedMessage(_parentPlugin.Translations, "could_not_find", parameters[1]);
                        return;
                    }
                    else if (name == null && id != 0)
                    {
                        name = ((VehicleAsset)Assets.find(EAssetType.VEHICLE, id)).vehicleName;
                    }
                    decimal cost = ZaupShop.Instance.ShopDB.GetVehicleCost(id);
                    message = _parentPlugin.Translations.Get("vehicle_cost_msg", name, cost.ToString(CultureInfo.CurrentCulture), _economy.DefaultCurrency.Name);
                    if (cost <= 0m)
                    {
                        message = _parentPlugin.Translations.Get("error_getting_cost", name);
                    }

                    context.User.SendMessage(message);
                    break;
                default:
                    name = null;
                    if (!ushort.TryParse(parameters[0], out id))
                    {
                        Asset[] array = Assets.find(EAssetType.ITEM);
                        Asset[] array2 = array;
                        for (int i = 0; i < array2.Length; i++)
                        {
                            ItemAsset iAsset = (ItemAsset)array2[i];
                            if (iAsset != null && iAsset.itemName != null && iAsset.itemName.ToLower().Contains(parameters[0].ToLower()))
                            {
                                id = iAsset.id;
                                name = iAsset.itemName;
                                break;
                            }
                        }
                    }
                    if (Assets.find(EAssetType.ITEM, id) == null)
                    {
                        context.User.SendLocalizedMessage(_parentPlugin.Translations, "could_not_find", parameters[0]);
                        return;
                    }
                    else if (name == null && id != 0)
                    {
                        name = ((ItemAsset)Assets.find(EAssetType.ITEM, id)).itemName;
                    }
                    cost = ZaupShop.Instance.ShopDB.GetItemCost(id);
                    decimal bbp = ZaupShop.Instance.ShopDB.GetItemBuyPrice(id);
                    message = _parentPlugin.Translations.Get("item_cost_msg", name, cost.ToString(CultureInfo.CurrentCulture), _economy.DefaultCurrency.Name, bbp.ToString(CultureInfo.CurrentCulture), _economy.DefaultCurrency.Name);
                    if (cost <= 0m)
                    {
                        message = _parentPlugin.Translations.Get("error_getting_cost", name);
                    }
                    context.User.SendMessage(message);
                    break;
            }
        }
    }
}