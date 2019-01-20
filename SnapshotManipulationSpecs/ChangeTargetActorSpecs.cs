using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using Xunit;

namespace SnapshotManipulationSpecs
{
    public class ChangeTargetActorSpecs : TestKit
    {
        [Fact(DisplayName = "Should be able to change some un-confirmed messages to a new target upon recover")]
        public void ShouldChangeMessagesToNewTargetUponRecover()
        {
            var probe1 = CreateTestProbe("newTarget");
            var changeTarget1 =
                Sys.ActorOf(
                    Props.Create(() =>
                        new AtLeastOnceDeliveryChangeTargetActor("foobar", probe1.Ref.Path, delivery => true)),
                    "deliveryActor");

            var testActorPath = TestActor.Path;
            changeTarget1.Tell(new DeliverTo(testActorPath, "hit1"));
            changeTarget1.Tell(new DeliverTo(testActorPath, "hit2"));
            changeTarget1.Tell(new DeliverTo(testActorPath, "hit3"));
            ReceiveN(3); // wait to receive all 3 of the message delivery attempts
        }
    }
}
