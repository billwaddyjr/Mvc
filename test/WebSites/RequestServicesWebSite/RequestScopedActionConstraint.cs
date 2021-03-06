// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ActionConstraints;
using Microsoft.Framework.DependencyInjection;

namespace RequestServicesWebSite
{
    // Only matches when the requestId is the same as the one passed in the constructor.
    public class RequestScopedActionConstraintAttribute : Attribute, IActionConstraintFactory
    {
        private readonly string _requestId;
        private readonly Func<Type, object, ObjectFactory> CreateFactory =
            (t, s) => ActivatorUtilities.CreateFactory(t, new[] { s.GetType() });
        private readonly ConcurrentDictionary<Type, ObjectFactory> _constraintCache =
               new ConcurrentDictionary<Type, ObjectFactory>();

        public RequestScopedActionConstraintAttribute(string requestId)
        {
            _requestId = requestId;
        }

        public IActionConstraint CreateInstance(IServiceProvider services)
        {
            var constraintType = typeof(Constraint);
            return (Constraint)ActivatorUtilities.CreateInstance(services, typeof(Constraint),new[] { _requestId });
        }

        private class Constraint : IActionConstraint
        {
            private readonly RequestIdService _requestIdService;
            private readonly string _requestId;

            public Constraint(RequestIdService requestIdService, string requestId)
            {
                _requestIdService = requestIdService;
                _requestId = requestId;
            }

            public int Order { get; private set; }

            public bool Accept(ActionConstraintContext context)
            {
                return _requestId == _requestIdService.RequestId;
            }
        }
    }
}