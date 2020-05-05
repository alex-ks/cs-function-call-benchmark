using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

public static class ExpressionBasedActionGenerator
{
    public static Action ExtractAction(object target, MethodInfo method, Func<object>[] args)
    {
        if (method.ReturnType != typeof(void))
            throw new ArgumentException("For now we work with void methods only");

        var parametersCount = method.GetParameters().Length;
        if (parametersCount != args.Length)
            throw new ArgumentException("Method arguments count differs from provided arguments");

        // Let the magic begin!
        switch (parametersCount)
        {
            case 0:
            {
                var convertedAct = CreateAction<Action>(target, method);
                return () => convertedAct();
            }
            case 1:
            {
                var convertedAct = CreateAction<Action<object>>(target, method);
                return () => convertedAct(args[0]());
            }
            case 2:
            {
                var convertedAct = CreateAction<Action<object, object>>(target, method);
                return () => convertedAct(args[0](), args[1]());
            }
            case 3:
            {
                var convertedAct = CreateAction<Action<object, object, object>>(target, method);
                return () => convertedAct(args[0](), args[1](), args[2]());
            }
            case 4:
            {
                var convertedAct = CreateAction<Action<object, object, object, object>>(target, method);
                return () => convertedAct(args[0](), args[1](), args[2](), args[3]());
            }
            case 5:
            {
                var convertedAct = CreateAction<Action<object, object, object, object, object>>(target, method);
                return () => convertedAct(args[0](), args[1](), args[2](), args[3](), args[4]());
            }
            default:
                return () => method.Invoke(target, args.Select(getter => getter()).ToArray());
        }
    }

    private static TLambda CreateAction<TLambda>(object target, MethodInfo method)
    {
        var parameters = method.GetParameters();

        var expressionParameters = parameters.Select(p => Expression.Parameter(typeof(object))).ToList();
        var convertedParameters =
                parameters.Zip(expressionParameters, (p, e) => Expression.Convert(e, p.ParameterType));
        var instance = Expression.Constant(target);

        var call = Expression.Call(instance, method, convertedParameters);

        return Expression.Lambda<TLambda>(call, expressionParameters).Compile();
    }
}
