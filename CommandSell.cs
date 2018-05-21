using System;
using System.Collections.Generic;
using Rocket.API.Commands;
using Rocket.API.Economy;
using Rocket.API.Eventing;
using Rocket.API.Plugins;
using Rocket.Core.I18N;
using Rocket.Core.Plugins;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;
using ZaupShop.Events;

namespace ZaupShop
{
    public class CommandSell : ICommand
    {
        private readonly IEconomyProvider _economy;
        private readonly IEventManager _eventManager;
        private readonly Plugin _parentPlugin;

        public CommandSell(IPlugin plugin, IEconomyProvider economy, IEventManager eventManager)
        {
            _parentPlugin = (Plugin)plugin;
            _economy = economy;
            _eventManager = eventManager;
        }

        public string Name => "sell";

        public string Summary => "Allows you to sell items to the shop from your inventory.";
        public string Description => null;
        public string Permission => null;

        public string Syntax => "<name or id> [amount]";
        public IChildCommand[] ChildCommands => null;

        public string[] Aliases => null;

        public bool SupportsUser(Type user)
        {
            return typeof(UnturnedUser).IsAssignableFrom(user);
        }

        public void Execute(ICommandContext context)
        {
            var player = ((UnturnedUser) context.User).UnturnedPlayer;
            var parameters = context.Parameters;

            if (parameters.Length == 0 || (parameters.Length > 0 && parameters[0].Trim() == string.Empty))
            {
                // We are going to print how to use
                context.User.SendLocalizedMessage(_parentPlugin.Translations, "sell_command_usage");
                return;
            }
            byte amttosell = 1;
            if (parameters.Length > 1)
            {
                if (!byte.TryParse(parameters[1], out amttosell))
                {
                    context.User.SendLocalizedMessage(_parentPlugin.Translations, "invalid_amt");
                    return;
                }
            }
            byte amt = amttosell;
            if (!ZaupShop.Instance.ConfigurationInstance.CanSellItems)
            {
                context.User.SendLocalizedMessage(_parentPlugin.Translations, "sell_items_off");
                return;
            }

            ItemAsset asset = null;
            if (parameters.TryGet(0, out ushort id))
            {
                foreach (ItemAsset vAsset in Assets.find(EAssetType.ITEM))
                {
                    if (vAsset?.itemName != null && vAsset.itemName.ToLower().Contains(parameters[0].ToLower()))
                    {
                        asset = vAsset;
                        id = vAsset.id;
                        break;
                    }
                }
            }
            if (Assets.find(EAssetType.ITEM, id) == null)
            {
                context.User.SendLocalizedMessage(_parentPlugin.Translations, "could_not_find", parameters[0]);
                return;
            }

            if (asset == null)
            {
                asset = (ItemAsset)Assets.find(EAssetType.ITEM, id);
            }

            if (player.Inventory.has(id) == null)
            {
                context.User.SendLocalizedMessage(_parentPlugin.Translations, "not_have_item_sell", asset.itemName);
                return;
            }
            List<InventorySearch> list = player.Inventory.search(id, true, true);
            if (list.Count == 0 || (asset.amount == 1 && list.Count < amttosell))
            {
                context.User.SendLocalizedMessage(_parentPlugin.Translations, "not_enough_items_sell", amttosell.ToString(), asset.itemName);
                return;
            }
            if (asset.amount > 1)
            {
                int ammomagamt = 0;
                foreach (InventorySearch ins in list)
                {
                    ammomagamt += ins.jar.item.amount;
                }
                if (ammomagamt < amttosell)
                {
                    context.User.SendLocalizedMessage(_parentPlugin.Translations, "not_enough_ammo_sell", asset.itemName);
                    return;
                }
            }

            // We got this far, so let's buy back the items and give them money.
            // Get cost per item.  This will be whatever is set for most items, but changes for ammo and magazines.
            decimal price = ZaupShop.Instance.ShopDB.GetItemBuyPrice(id);
            if (price <= 0.00m)
            {
                context.User.SendLocalizedMessage(_parentPlugin.Translations, "no_sell_price_set", asset.itemName);
                return;
            }
            byte quality = 100;
            decimal peritemprice = 0;
            decimal addmoney = 0;
            switch (asset.amount)
            {
                case 1:
                    // These are single items, not ammo or magazines
                    while (amttosell > 0)
                    {
                        if (player.Player.equipment.checkSelection(list[0].page, list[0].jar.x, list[0].jar.y))
                        {
                            player.Player.equipment.dequip();
                        }
                        if (ZaupShop.Instance.ConfigurationInstance.QualityCounts)
                            quality = list[0].jar.item.durability;
                        peritemprice = decimal.Round(price * (quality / 100.0m), 2);
                        addmoney += peritemprice;
                        player.Inventory.removeItem(list[0].page, player.Inventory.getIndex(list[0].page, list[0].jar.x, list[0].jar.y));
                        list.RemoveAt(0);
                        amttosell--;
                    }
                    break;
                default:
                    // This is ammo or magazines
                    byte amttosell1 = amttosell;
                    while (amttosell > 0)
                    {
                        if (player.Player.equipment.checkSelection(list[0].page, list[0].jar.x, list[0].jar.y))
                        {
                            player.Player.equipment.dequip();
                        }
                        if (list[0].jar.item.amount >= amttosell)
                        {
                            byte left = (byte)(list[0].jar.item.amount - amttosell);
                            list[0].jar.item.amount = left;
                            player.Inventory.sendUpdateAmount(list[0].page, list[0].jar.x, list[0].jar.y, left);
                            amttosell = 0;
                            if (left == 0)
                            {
                                player.Inventory.removeItem(list[0].page, player.Inventory.getIndex(list[0].page, list[0].jar.x, list[0].jar.y));
                                list.RemoveAt(0);
                            }
                        }
                        else
                        {
                            amttosell -= list[0].jar.item.amount;
                            player.Inventory.sendUpdateAmount(list[0].page, list[0].jar.x, list[0].jar.y, 0);
                            player.Inventory.removeItem(list[0].page, player.Inventory.getIndex(list[0].page, list[0].jar.x, list[0].jar.y));
                            list.RemoveAt(0);
                        }
                    }
                    peritemprice = decimal.Round(price * (amttosell1 / (decimal)asset.amount), 2);
                    addmoney += peritemprice;
                    break;
            }

            var zaupSellEvent = new ZaupSellEvent(player, id, true, EventExecutionTargetContext.Sync);
            _eventManager.Emit(_parentPlugin, zaupSellEvent);
            if (zaupSellEvent.IsCancelled)
                return;

            _economy.AddBalance(context.User, addmoney);
            decimal balance = _economy.GetBalance(player);
            context.User.SendLocalizedMessage(_parentPlugin.Translations, "sold_items", amt, asset.itemName, addmoney, _economy.GetBalance(player), balance, _economy.GetBalance(player));
        }
    }
}