using Rocket.API.Eventing;
using Rocket.API.Player;
using Rocket.Core.Eventing;

namespace ZaupShop.Events
{
    public class ZaupBuyEvent : Event, ICancellableEvent
    {
        public IPlayer Buyer { get; }
        public ushort Id { get; }

        public bool IsVehicle { get; }
        public ZaupBuyEvent(IPlayer buyer, ushort id, bool isVehicle, bool global = true) : base(global)
        {
            Buyer = buyer;
            Id = id;
            IsVehicle = isVehicle;
        }

        public ZaupBuyEvent(IPlayer buyer, ushort id, bool isVehicle, EventExecutionTargetContext executionTarget = EventExecutionTargetContext.Sync, bool global = true) : base(executionTarget, global)
        {
            Buyer = buyer;
            Id = id;
            IsVehicle = isVehicle;
        }

        public ZaupBuyEvent(IPlayer buyer, ushort id, bool isVehicle, string name = null, EventExecutionTargetContext executionTarget = EventExecutionTargetContext.Sync, bool global = true) : base(name, executionTarget, global)
        {
            Buyer = buyer;
            Id = id;
            IsVehicle = isVehicle;
        }

        public bool IsCancelled { get; set; }
    }
}