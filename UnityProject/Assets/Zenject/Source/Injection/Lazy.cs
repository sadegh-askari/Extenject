using System;

namespace Zenject
{
    public interface ILazy
    {
        void Validate();
    }

    [ZenjectAllowDuringValidationAttribute]
    public class Lazy<T> : ILazy
    {
        readonly DiContainer _container;
        readonly InjectContext _context;

        bool _hasValue;
        T _value;

        public Lazy(DiContainer container, InjectContext context)
        {
            _container = container;
            _context = context;
        }

        void ILazy.Validate()
        {
            _container.Resolve<T>(_context);
        }

        public T Value
        {
            get
            {
                if (!_hasValue)
                {
                    _value = _container.Resolve<T>(_context);
                    _hasValue = true;
                }

                return _value;
            }
        }
    }
}
