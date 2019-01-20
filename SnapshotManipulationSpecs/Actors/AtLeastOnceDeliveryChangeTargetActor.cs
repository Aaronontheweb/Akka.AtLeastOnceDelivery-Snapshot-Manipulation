using System;
using System.Linq;
using Akka.Actor;
using Akka.Persistence;

namespace SnapshotManipulationSpecs
{
    public class AtLeastOnceDeliveryChangeTargetActor : AtLeastOnceDeliveryReceiveActor
    {
        private readonly ActorPath _updatedTarget;
        private Predicate<UnconfirmedDelivery> _useNewTarget;

        public AtLeastOnceDeliveryChangeTargetActor(string persistenceId, ActorPath updatedTarget, Predicate<UnconfirmedDelivery> useNewTarget)
        {
            PersistenceId = persistenceId;
            _updatedTarget = updatedTarget;
            _useNewTarget = useNewTarget;

            Recover<SnapshotOffer>(so =>
            {
                if (so.Snapshot is AtLeastOnceDeliverySnapshot ald)
                {
                    /*
                     * Manipulate delivery snapshot by changing the delivery dates to
                     * something immediate, rather than long-term in the future.                     *
                     */
                    var undeliverableMessages = ald.UnconfirmedDeliveries.
                    Select(x =>
                    {
                        // change delivery to new target ActorPath
                        if (_useNewTarget(x))
                        {
                            return new UnconfirmedDelivery(x.DeliveryId, _updatedTarget, x.Message);
                        }

                        // use the original actor if the predicate is false
                        return x;
                    }).ToArray();

                    // create a new AtLeastOnceDeliverySnapshot object
                    var snapshot = new AtLeastOnceDeliverySnapshot(ald.CurrentDeliveryId, undeliverableMessages);

                    // startup this actor with the updated snapshot
                    SetDeliverySnapshot(snapshot);
                }
            });

            Command<DeliverTo>(d =>
            {
                Deliver(d.Target, l => new ConfirmableDelivery(l, d.Message));
                SaveSnapshot(GetDeliverySnapshot());
            });
            Command<Confirmed>(c => { ConfirmDelivery(c.DeliveryId); SaveSnapshot(GetDeliverySnapshot()); });
            Command<GetDeliveryState>(_ => Sender.Tell(GetDeliverySnapshot()));
            Command<SaveSnapshotSuccess>(success =>
            {
                // delete prior snapshots
                DeleteSnapshots(new SnapshotSelectionCriteria(success.Metadata.SequenceNr-1));
            });
            Command<ForceRestart>(_ => throw new ApplicationException("YOLO"));
        }

        public override string PersistenceId { get; }
    }
}
