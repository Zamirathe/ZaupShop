using Rocket.API.Eventing;
using Rocket.API.Player;
using Rocket.Core.Eventing;

namespace ZaupShop.Events
{
    public class ZaupSellEvent : Event, ICancellableEvent
    {
        public IPlayer Seller { get; }
        public ushort Id { get; }

        public bool IsVehicle { get; }
        public ZaupSellEvent(IPlayer seller, ushort id, bool isVehicle, bool global = true) : base(global)
        {
            Seller = seller;
            Id = id;
            IsVehicle = isVehicle;
        }

        public ZaupSellEvent(IPlayer seller, ushort id, bool isVehicle, EventExecutionTargetContext executionTarget = EventExecutionTargetContext.Sync, bool global = true) : base(executionTarget, global)
        {
            Seller = seller;
            Id = id;
            IsVehicle = isVehicle;
        }

        public ZaupSellEvent(IPlayer seller, ushort id, bool isVehicle, string name = null, EventExecutionTargetContext executionTarget = EventExecutionTargetContext.Sync, bool global = true) : base(name, executionTarget, global)
        {
            Seller = seller;
            Id = id;
            IsVehicle = isVehicle;
        }

        public bool IsCancelled { get; set; }
    }
}