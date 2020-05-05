using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

public static class ExpressionBasedStrictGenerator
{
    public static Action ExtractAction(object target, MethodInfo method, Delegate[] args)
    {
        if (method.ReturnType != typeof(void))
            throw new ArgumentException("For now we work with void methods only");

        var parameters = method.GetParameters();
        if (parameters.Length != args.Length)
            throw new ArgumentException("Method arguments count differs from provided arguments");

        var argumentsExpressions = args.Zip(parameters, (arg, p) =>
        {
            var getterType = typeof(Func<>).MakeGenericType(p.ParameterType);
            var getter = Expression.Constant(arg, getterType);
            return Expression.Invoke(getter);
        });

        var instance = Expression.Constant(target);

        var call = Expression.Call(instance, method, argumentsExpressions);

        return Expression.Lambda<Action>(call).Compile();
    }
}
