using System;
using System.Runtime.Remoting.Messaging;
using Rocket.API.Commands;
using Rocket.API.Plugins;
using Rocket.Core.Commands;
using Rocket.Core.I18N;
using Rocket.Core.Plugins;
using SDG.Unturned;

namespace ZaupShop
{
    public class CommandShop : ICommand
    {
        private readonly Plugin _parentPlugin;

        public CommandShop(IPlugin plugin)
        {
            _parentPlugin = (Plugin)plugin;
        }
        public string Name => "shop";
        public string Summary => "Allows admins to change, add, or remove items/vehicles from the shop.";
        public string Description => null;
        public string Permission => null;
        public string Syntax => "<add | rem | chng | buy> [v.]<itemid> <cost>";
        public IChildCommand[] ChildCommands => null;
        public string[] Aliases => null;
        public string Permissions => "shop";

        public bool SupportsUser(Type user)
        {
            return true;
        }

        public void Execute(ICommandContext context)
        {
            var parameters = context.Parameters;
            if (parameters.Length == 0)
            {
                throw new CommandWrongUsageException();
            }
            if (parameters.Length < 2)
            {
                throw new CommandWrongUsageException(_parentPlugin.Translations.Get("no_itemid_given"));
            }
            if (parameters.Length == 2 && parameters[0] != "rem")
            {
                throw new CommandWrongUsageException(_parentPlugin.Translations.Get("no_cost_given"));
            }

            if (parameters.Length >= 2)
            {
                string[] type = Parser.getComponentsFromSerial(parameters[1], '.');
                if (type.Length > 1 && type[0] != "v")
                {
                    throw new CommandWrongUsageException(_parentPlugin.Translations.Get("v_not_provided"));
                }
                ushort id;
                if (type.Length > 1)
                {
                    if (!ushort.TryParse(type[1], out id))
                    {
                        throw new CommandWrongUsageException(_parentPlugin.Translations.Get("invalid_id_given"));
                    }
                }
                else
                {
                    if (!ushort.TryParse(type[0], out id))
                    {
                        throw new CommandWrongUsageException(_parentPlugin.Translations.Get("invalid_id_given"));
                    }
                }
                // All basic checks complete.  Let's get down to business.
                bool success = false;
                bool change = false;
                bool pass = false;
                switch (parameters[0])
                {
                    case "chng":
                        change = true;
                        pass = true;
                        goto case "add";
                    case "add":
                        string ac = (pass) ? _parentPlugin.Translations.Get("changed") : _parentPlugin.Translations.Get("added");
                        switch (type[0])
                        {
                            case "v":
                                if (!IsAsset(id, "v"))
                                {
                                    throw new CommandWrongUsageException(_parentPlugin.Translations.Get("invalid_id_given"));
                                }
                                VehicleAsset va = (VehicleAsset)Assets.find(EAssetType.VEHICLE, id);
                                success = ZaupShop.Instance.ShopDB.AddVehicle(id, va.vehicleName, decimal.Parse(parameters[2]), change);
                                if (!success)
                                {
                                    context.User.SendLocalizedMessage(_parentPlugin.Translations, "error_adding_or_changing", va.vehicleName);
                                    return;
                                }
                                context.User.SendLocalizedMessage(_parentPlugin.Translations, "changed_or_added_to_shop", ac, va.vehicleName, parameters[2]);
                                break;
                            default:
                                if (!IsAsset(id, "i"))
                                {
                                    throw new CommandWrongUsageException(_parentPlugin.Translations.Get("invalid_id_given"));
                                }

                                ItemAsset ia = (ItemAsset)Assets.find(EAssetType.ITEM, id);
                                success = ZaupShop.Instance.ShopDB.AddItem(id, ia.itemName, decimal.Parse(parameters[2]), change);
                                if (!success)
                                {
                                    context.User.SendLocalizedMessage(_parentPlugin.Translations, "error_adding_or_changing", ia.itemName);
                                    return;
                                }

                                context.User.SendLocalizedMessage(_parentPlugin.Translations, "changed_or_added_to_shop", ac, ia.itemName, parameters[2]);
                                break;
                        }
                        break;
                    case "rem":
                        switch (type[0])
                        {
                            case "v":
                                if (!IsAsset(id, "i"))
                                {
                                    throw new CommandWrongUsageException(_parentPlugin.Translations.Get("invalid_id_given"));
                                }

                                VehicleAsset va = (VehicleAsset)Assets.find(EAssetType.VEHICLE, id);
                                success = ZaupShop.Instance.ShopDB.DeleteVehicle(id);
                                if (!success)
                                {
                                    context.User.SendLocalizedMessage(_parentPlugin.Translations, "not_in_shop_to_remove", va.vehicleName);
                                    return;
                                }

                                context.User.SendLocalizedMessage(_parentPlugin.Translations, "removed_from_shop", va.vehicleName);
                                break;
                            default:
                                if (!IsAsset(id, "i"))
                                {
                                    throw new CommandWrongUsageException(_parentPlugin.Translations.Get("invalid_id_given"));
                                }

                                ItemAsset ia = (ItemAsset)Assets.find(EAssetType.ITEM, id);
                                success = ZaupShop.Instance.ShopDB.DeleteItem(id);
                                if (!success)
                                {
                                    context.User.SendLocalizedMessage(_parentPlugin.Translations, "not_in_shop_to_remove", ia.itemName);
                                    return;
                                }

                                context.User.SendLocalizedMessage(_parentPlugin.Translations, "removed_from_shop", ia.itemName);
                                break;
                        }
                        break;
                    case "buy":
                        if (!IsAsset(id, "i"))
                        {
                            throw new CommandWrongUsageException(_parentPlugin.Translations.Get("invalid_id_given"));
                        }
                        ItemAsset iab = (ItemAsset)Assets.find(EAssetType.ITEM, id);

                        var buyb = parameters.Get<decimal>(2);
                        success = ZaupShop.Instance.ShopDB.SetBuyPrice(id, buyb);
                        if (!success)
                        {
                            context.User.SendLocalizedMessage(_parentPlugin.Translations, "not_in_shop_to_buyback", iab.itemName);
                            break;
                        }

                        context.User.SendLocalizedMessage(_parentPlugin.Translations, "set_buyback_price", iab.itemName, buyb.ToString());
                        break;
                    default:
                        // We shouldn't get this, but if we do send an error.
                        context.User.SendLocalizedMessage(_parentPlugin.Translations, "not_in_shop_to_remove");
                        return;
                }
            }
        }
        private bool IsAsset(ushort id, string type)
        {
            // Check for valid Item/Vehicle Id.
            switch (type)
            {
                case "i":
                    if (Assets.find(EAssetType.ITEM, id) != null)
                    {
                        return true;
                    }
                    return false;
                case "v":
                    if (Assets.find(EAssetType.VEHICLE, id) != null)
                    {
                        return true;
                    }
                    return false;
                default:
                    return false;
            }
        }
    }
}
