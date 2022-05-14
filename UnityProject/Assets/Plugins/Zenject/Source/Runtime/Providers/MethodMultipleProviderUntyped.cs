using System;
using System.Collections.Generic;
using ModestTree;

namespace Zenject
{
    [NoReflectionBaking]
    public class MethodMultipleProviderUntyped : IProvider
    {
        readonly DiContainer _container;
        readonly Action<InjectContext, List<object>> _method;

        public MethodMultipleProviderUntyped(
            Action<InjectContext, List<object>> method,
            DiContainer container)
        {
            _container = container;
            _method = method;
        }

        public bool IsCached
        {
            get { return false; }
        }

        public bool TypeVariesBasedOnMemberType
        {
            get { return false; }
        }

        public Type GetInstanceType(InjectContext context)
        {
            return context.MemberType;
        }

        public void GetAllInstancesWithInjectSplit(
            InjectContext context, List<TypeValuePair> args, out Action injectAction, List<object> buffer)
        {
            Assert.IsEmpty(args);
            Assert.IsNotNull(context);

            injectAction = null;
            if (_container.IsValidating && !TypeAnalyzer.ShouldAllowDuringValidation(context.MemberType))
            {
                buffer.Add(new ValidationMarker(context.MemberType));
            }
            else
            {
                _method(context, buffer);
            }
        }
    }
}

