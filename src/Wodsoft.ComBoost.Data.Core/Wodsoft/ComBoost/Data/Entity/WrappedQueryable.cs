﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Wodsoft.ComBoost.Data.Entity
{
    public class WrappedQueryable<T, M> : IQueryable<T>
        where T : IEntity
        where M : IEntity, T
    {
        public WrappedQueryable(WrappedQueryableProvider<T, M> provider, Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));
            Expression = expression;
            Provider = provider;
        }

        public Type ElementType { get { return typeof(T); } }

        public Expression Expression { get; private set; }


        public WrappedQueryableProvider<T, M> Provider { get; private set; }

        System.Linq.IQueryProvider IQueryable.Provider { get { return Provider; } }

        public IEnumerator<T> GetEnumerator()
        {
            return new WrappedEnumerator<T, M>(Provider.InnerQueryProvider.CreateQuery<M>(Expression).GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new WrappedEnumerator<T, M>(Provider.InnerQueryProvider.CreateQuery<M>(Expression).GetEnumerator());
        }
    }
}
