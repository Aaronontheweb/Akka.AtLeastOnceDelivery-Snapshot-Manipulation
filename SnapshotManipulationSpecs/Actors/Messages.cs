using Akka.Actor;
using Akka.Persistence;

namespace SnapshotManipulationSpecs
{
    /// <summary>
    /// Retrieves a copy of the <see cref="AtLeastOnceDeliverySnapshot"/>
    /// from the current <see cref="AtLeastOnceDeliveryActor"/> instance.
    /// </summary>
    public sealed class GetDeliveryState
    {
        public static readonly GetDeliveryState Instance = new GetDeliveryState();
        private GetDeliveryState() { }
    }

    /// <summary>
    /// Used to guide delivery of specific messages for certain tests
    /// </summary>
    public sealed class DeliverTo
    {
        public DeliverTo(ActorPath target, object message)
        {
            Target = target;
            Message = message;
        }

        public ActorPath Target { get; }

        public object Message { get; }
    }

    public sealed class ConfirmableDelivery
    {
        public ConfirmableDelivery(long deliveryId, object message)
        {
            DeliveryId = deliveryId;
            Message = message;
        }

        public long DeliveryId { get; }

        public object Message { get; }
    }

    public sealed class Confirmed
    {
        public Confirmed(long deliveryId)
        {
            DeliveryId = deliveryId;
        }

        public long DeliveryId { get; }
    }

    /// <summary>
    /// Force the <see cref="AtLeastOnceDeliveryActor"/> to crash and restart.
    /// </summary>
    public sealed class ForceRestart
    {
        public static readonly ForceRestart Instance = new ForceRestart();
        private ForceRestart() { }
    }
}
