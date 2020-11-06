// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Dapr.Actors.Test.Runtime
{
    using Dapr.Actors.Runtime;
    using Moq;
    using Xunit;
    using Microsoft.Extensions.Logging;

    public sealed class ActorRuntimeOptionsTests
    {
        [Fact]
        public void TestRegisterActor_SavesActivator()
        {
            var actorType = typeof(TestActor);
            var actorTypeInformation = ActorTypeInformation.Get(actorType);
            var actorService = new ActorService(actorTypeInformation, ActorId.CreateRandom(), new LoggerFactory());
            var actor = new TestActor(actorService);

            var activator = Mock.Of<ActorActivator>();

            var actorRuntimeOptions = new ActorRuntimeOptions();
            actorRuntimeOptions.Actors.RegisterActor<TestActor>(options =>
            {
                options.Activator = activator;
            });

            Assert.Collection(
                actorRuntimeOptions.Actors,
                registration => 
                {
                    Assert.Same(actorTypeInformation, registration.Type);
                    Assert.Same(activator, registration.Activator);
                });
        }
    }
}
