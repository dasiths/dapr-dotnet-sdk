// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Dapr.Actors.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace Dapr.Actors.AspNetCore
{
    // Implementation of ActorActivatorFactory that uses Microsoft.Extensions.DependencyInjection.
    internal class DependencyInjectionActorActivatorFactory : ActorActivatorFactory
    {
        private readonly IServiceProvider services;

        public DependencyInjectionActorActivatorFactory(IServiceProvider services)
        {
            this.services = services;
        }

        public override ActorActivator CreateActivator(ActorTypeInformation type)
        {
            return new DependencyInjectionActorActivator(services, type);
        }
    }
}
