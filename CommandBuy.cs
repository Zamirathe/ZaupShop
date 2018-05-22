using System;
using Rocket.API.Commands;
using Rocket.API.Economy;
using Rocket.API.Eventing;
using Rocket.API.Plugins;
using Rocket.Core.Commands;
using Rocket.Core.I18N;
using Rocket.Core.Plugins;
using Rocket.Unturned.Player;
using SDG.Unturned;
using ZaupShop.Events;

namespace ZaupShop
{
    public class CommandBuy : ICommand
    {
        private readonly IEconomyProvider _economy;
        private readonly IEventManager _eventManager;
        private readonly ZaupShop _parentPlugin;

        public CommandBuy(IPlugin plugin, IEconomyProvider economy, IEventManager eventManager)
        {
            _parentPlugin = (ZaupShop) plugin;
            _economy = economy;
            _eventManager = eventManager;
        }
        public string Name => "buy";

        public string Summary => "Allows you to buy items from the shop.";

        public string Description => null;

        public string Syntax => "[v.]<name or id> [amount] [25 | 50 | 75 | 100]";

        public IChildCommand[] ChildCommands => null;

        public string[] Aliases => null;

        public string Permission => null;

        public bool SupportsUser(Type user)
        {
            return typeof(UnturnedUser).IsAssignableFrom(user);
        }

        public void Execute(ICommandContext context)
        {
            var player = ((UnturnedUser) context.User).Player;

            var parameters = context.Parameters;
            if (parameters.Length == 0)
                throw new CommandWrongUsageException();
            
            byte amttobuy = 1;
            if (parameters.Length > 1)
            {
                if (!byte.TryParse(parameters[1], out amttobuy))
                {
                    context.User.SendLocalizedMessage(_parentPlugin.Translations, "invalid_amt");
                    return;
                }
            }

            string[] components = Parser.getComponentsFromSerial(parameters[0], '.');
            if ((components.Length == 2 && components[0].Trim() != "v") || (components.Length == 1 && components[0].Trim() == "v") || components.Length > 2 || parameters[0].Trim() == string.Empty)
                throw new CommandWrongUsageException();
            switch (components[0])
            {
                case "v":
                    if (!_parentPlugin.ConfigurationInstance.CanBuyVehicles)
                    {
                        context.User.SendLocalizedMessage(_parentPlugin.Translations, "buy_vehicles_off");
                        return;
                    }
                    string name = null;
                    if (parameters.TryGet(1, out ushort id))
                    {
                         foreach (VehicleAsset vAsset in Assets.find(EAssetType.VEHICLE))
                         {
                             if (vAsset?.vehicleName != null && vAsset.vehicleName.ToLower().Contains(components[1].ToLower()))
                             {
                                 id = vAsset.id;
                                 name = vAsset.vehicleName;
                                 break;
                             }
                         }
                    }
                    if (Assets.find(EAssetType.VEHICLE, id) == null)
                    {
                        context.User.SendLocalizedMessage(_parentPlugin.Translations, "could_not_find", components[1]);
                        return;
                    }
                    else if (name == null && id != 0)
                    {
                        name = ((VehicleAsset)Assets.find(EAssetType.VEHICLE, id)).vehicleName;
                    }
                    decimal cost = _parentPlugin.Database.GetVehicleCost(id);
                    decimal balance = _economy.GetBalance(context.User);

                    if (cost <= 0m)
                    {
                        context.User.SendLocalizedMessage(_parentPlugin.Translations, "vehicle_not_available", name);
                        return;
                    }
                    if (balance < cost)
                    {
                        context.User.SendLocalizedMessage(_parentPlugin.Translations, "not_enough_currency_msg", _economy.DefaultCurrency.Name, "1", name);
                        return;
                    }

                    if (!player.GiveVehicle(id))
                    {
                        context.User.SendLocalizedMessage(_parentPlugin.Translations, "error_giving_item", name);
                        return;
                    }

                    var @event = new ZaupBuyEvent(player, id, true, EventExecutionTargetContext.Sync);
                    _eventManager.Emit(_parentPlugin, @event);
                    if (@event.IsCancelled)
                        return;

                    _economy.RemoveBalance(context.User, cost);
                    var newBal = _economy.GetBalance(context.User);

                    context.User.SendLocalizedMessage(_parentPlugin.Translations, "vehicle_buy_msg", name, cost, _economy.DefaultCurrency.Name, newBal, _economy.DefaultCurrency.Name);
                    return;
                default:
                    if (!_parentPlugin.ConfigurationInstance.CanBuyItems)
                    {
                        player.User.SendLocalizedMessage(_parentPlugin.Translations, "buy_items_off");
                        return;
                    }
                    name = null;
                    if (parameters.TryGet(1, out id))
                    {
                        foreach (ItemAsset vAsset in Assets.find(EAssetType.ITEM))
                        {
                            if (vAsset?.itemName != null && vAsset.itemName.ToLower().Contains(components[0].ToLower()))
                            {
                                id = vAsset.id;
                                name = vAsset.itemName;
                                break;
                            }
                        }
                    }
                    if (Assets.find(EAssetType.ITEM, id) == null)
                    {
                        context.User.SendLocalizedMessage(_parentPlugin.Translations, "could_not_find", components[1]);
                        return;
                    }
                    else if (name == null && id != 0)
                    {
                        name = ((ItemAsset)Assets.find(EAssetType.ITEM, id)).itemName;
                    }
                    cost = decimal.Round(_parentPlugin.Database.GetItemCost(id) * amttobuy, 2);
                    balance = _economy.GetBalance(context.User);

                    if (cost <= 0m)
                    {
                        player.User.SendLocalizedMessage(_parentPlugin.Translations, "item_not_available", name);
                        return;
                    }
                    if (balance < cost)
                    {
                        player.User.SendLocalizedMessage(_parentPlugin.Translations, "not_enough_currency_msg", _economy.DefaultCurrency.Name, amttobuy, name);
                        return;
                    }

                    if (!player.GiveItem(id, amttobuy))
                    {
                        context.User.SendLocalizedMessage(_parentPlugin.Translations, "error_giving_item", name);
                        return;
                    }

                    var itemBuyEvent = new ZaupBuyEvent(player, id, true, EventExecutionTargetContext.Sync);
                    _eventManager.Emit(_parentPlugin, itemBuyEvent);
                    if (itemBuyEvent.IsCancelled)
                        return;

                    _economy.RemoveBalance(context.User, cost);
                    newBal = _economy.GetBalance(context.User);

                    context.User.SendLocalizedMessage(_parentPlugin.Translations, "item_buy_msg", name, cost, _economy.DefaultCurrency.Name, newBal, _economy.DefaultCurrency.Name);
                    return;
            }
        }
    }
}