using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using FluentAssertions;
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
            probe1.ExpectNoMsg(500); // shouldn't receive any messages
            changeTarget1.Tell(ForceRestart.Instance);

            // need to wait 6 seconds so the 5 second re-delivery interval has a chance to fire first
            var messages = probe1.ReceiveN(3, TimeSpan.FromSeconds(6)).Cast<ConfirmableDelivery>().Select(x => x.Message).ToArray();
            messages.Should().BeEquivalentTo("hit1", "hit2", "hit3");
        }
    }
}
