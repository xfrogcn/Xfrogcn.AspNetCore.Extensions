using System;
using System.Linq.Expressions;

namespace Xfrogcn.AspNetCore.Extensions
{
    public class PredicateBuilder<TModel>
    {
      
        private Expression _expression = null;

        private ParameterExpression _parameter;

        public PredicateBuilder()
        {
            _parameter = Expression.Parameter(typeof(TModel), "m");
        }

        public Expression<Func<TModel, bool>> Predicate
        {
            get
            {
                if(_expression == null)
                {
                    return Expression.Lambda<Func<TModel, bool>>(Expression.Constant(true), _parameter); ;
                }
                return Expression.Lambda<Func<TModel, bool>>(_expression, _parameter);
            }
        }

        public void And(Expression<Func<TModel, bool>> predicate)
        {
            if(predicate == null)
            {
                return;
            }

            var exp = new ParameterExpressionVisitor(predicate.Parameters[0], _parameter).Visit(predicate.Body);
            if(_expression == null)
            {
                _expression = exp;
            }
            else
            {
                _expression = Expression.AndAlso(
                    _expression,
                    exp
                    );
            }
        }


        public void Or(Expression<Func<TModel, bool>> predicate)
        {
            if (predicate == null)
            {
                return;
            }

            var exp = new ParameterExpressionVisitor(predicate.Parameters[0], _parameter).Visit(predicate.Body);
            if (_expression == null)
            {
                _expression = exp;
            }
            else
            {
                _expression = Expression.Or(
                    _expression,
                    exp
                    );
            }
        }

        class ParameterExpressionVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _source;
            private readonly ParameterExpression _target;
            public ParameterExpressionVisitor(ParameterExpression source, ParameterExpression target )
            {
                _source = source;
                _target = target;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if( node == _source)
                {
                    return _target;
                }
                return base.VisitParameter(node);
            }
        }

    }
}
