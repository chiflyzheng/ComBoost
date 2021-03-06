﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wodsoft.ComBoost
{
    public class DomainContextEventManager : DomainServiceEventManager
    {
        private Dictionary<DomainServiceEventRoute, Delegate> _Events;
        public DomainContextEventManager()
        {
            _Events = new Dictionary<DomainServiceEventRoute, Delegate>();
        }

        public override void RegisterEventRoute(DomainServiceEventRoute route)
        {
            throw new NotSupportedException("领域上下文当中的事件管理器不支持该方法。");
        }

        protected override Delegate GetEventRouteDelegate(DomainServiceEventRoute route)
        {
            Delegate d;
            if (!_Events.TryGetValue(route, out d))
                return null;
            return d;
        }

        protected override void SetEventRouteDelegate(DomainServiceEventRoute route, Delegate value)
        {
            if (_Events.ContainsKey(route))
                _Events[route] = value;
            else
                _Events.Add(route, value);
        }

        public override void RaiseEvent(DomainServiceEventRoute route, IDomainExecutionContext context)
        {
            base.RaiseEvent(route, context);
            DomainContextEventManager.GlobalEventManager.RaiseEvent(route, context);
        }

        public override void RaiseEvent<T>(DomainServiceEventRoute route, IDomainExecutionContext context, T eventArgs)
        {
            base.RaiseEvent<T>(route, context, eventArgs);
            DomainContextEventManager.GlobalEventManager.RaiseEvent(route, context, eventArgs);
        }

        public override async Task RaiseAsyncEvent(DomainServiceEventRoute route, IDomainExecutionContext context)
        {
            await base.RaiseAsyncEvent(route, context);
            await DomainContextEventManager.GlobalEventManager.RaiseAsyncEvent(route, context);
        }

        public override async Task RaiseAsyncEvent<T>(DomainServiceEventRoute route, IDomainExecutionContext context, T eventArgs)
        {
            await base.RaiseAsyncEvent<T>(route, context, eventArgs);
            await DomainContextEventManager.GlobalEventManager.RaiseAsyncEvent(route, context, eventArgs);
        }
    }
}
