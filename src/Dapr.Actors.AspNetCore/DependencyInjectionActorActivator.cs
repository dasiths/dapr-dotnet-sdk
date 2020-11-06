// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Actors.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace Dapr.Actors.AspNetCore
{
    // Implementation of ActorActivator that uses Microsoft.Extensions.DependencyInjection.
    internal class DependencyInjectionActorActivator : ActorActivator
    {
        private readonly IServiceProvider services;
        private readonly ActorTypeInformation type;
        private readonly Func<ObjectFactory> initializer;

        // factory is used to create the actor instance - initialization of the factory is protected
        // by the initialized and @lock fields.
        private ObjectFactory factory;
        private bool initialized;
        private object @lock;

        public DependencyInjectionActorActivator(IServiceProvider services, ActorTypeInformation type)
        {
            this.services = services;
            this.type = type;

            // Will be invoked to initialize the factory.
            initializer = () =>
            {
                return ActivatorUtilities.CreateFactory(this.type.ImplementationType, new Type[]{ typeof(ActorService), });
            };
        }

        public override async Task<ActorActivatorState> CreateAsync(ActorService service)
        {
            var scope = services.CreateScope();
            try
            {
                var factory = LazyInitializer.EnsureInitialized(
                    ref this.factory, 
                    ref this.initialized, 
                    ref this.@lock,
                    this.initializer);

                var actor = (Actor)factory(scope.ServiceProvider, new object[] { service });
                return new State(actor, scope);
            }
            catch
            {
                // Make sure to clean up the scope if we fail to create the actor;
                await DisposeCore(scope);
                throw;
            }
        }

        public override async ValueTask DeleteAsync(ActorActivatorState obj)
        {
            var state = (State)obj;
            await DisposeCore(state.Actor);
            await DisposeCore(state.Scope);
        }

        private async ValueTask DisposeCore(object obj)
        {
            if (obj is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
            else if (obj is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        private class State : ActorActivatorState
        {
            public State(Actor actor, IServiceScope scope)
                : base(actor)
            {
                Scope = scope;
            }

            public IServiceScope Scope { get; }
        }
    }
}
