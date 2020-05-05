using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class GenericBasedActionGenerator
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
                var convertedAct = CreateActionZeroArg(target, method);
                return () => convertedAct();
            }
            case 1:
            {
                var convertedAct = CreateAction1Arg(target, method);
                return () => convertedAct(args[0]());
            }
            case 2:
            {
                var convertedAct = CreateAction2Args(target, method);
                return () => convertedAct(args[0](), args[1]());
            }
            case 3:
            {
                var convertedAct = CreateAction3Args(target, method);
                return () => convertedAct(args[0](), args[1](), args[2]());
            }
            case 4:
            {
                var convertedAct = CreateAction4Args(target, method);
                return () => convertedAct(args[0](), args[1](), args[2](), args[3]());
            }
            case 5:
            {
                var convertedAct = CreateAction5Args(target, method);
                return () => convertedAct(args[0](), args[1](), args[2](), args[3](), args[4]());
            }
            default:
                return () => method.Invoke(target, args.Select(getter => getter()).ToArray());
        }
    }

    // Actions
    private static Action CreateActionZeroArg(object target, MethodInfo method)
    {
        var act = Delegate.CreateDelegate(typeof(Action), target, method) as Action;
        return act;
    }

    private static Action<object> CreateAction1ArgGeneric<TArg>(object target, MethodInfo method)
    {
        var act = Delegate.CreateDelegate(typeof(Action<TArg>), target, method) as Action<TArg>;
        return (arg) => act((TArg)arg);
    }

    private static Action<object> CreateAction1Arg(object target, MethodInfo method)
    {
        var genericCreator =
                typeof(GenericBasedActionGenerator).GetMethod(nameof(CreateAction1ArgGeneric),
                                                              BindingFlags.Static | BindingFlags.NonPublic);
        var correctCreator = genericCreator.MakeGenericMethod(method.GetParameters()[0].ParameterType);
        return correctCreator.Invoke(null, new[] { target, method }) as Action<object>;
    }

    private static Action<object, object> CreateAction2ArgsGeneric<TArg1, TArg2>(object target, MethodInfo method)
    {
        var act = Delegate.CreateDelegate(typeof(Action<TArg1, TArg2>),
                                          target,
                                          method) as Action<TArg1, TArg2>;
        return (arg1, arg2) => act((TArg1)arg1, (TArg2)arg2);
    }

    private static Action<object, object> CreateAction2Args(object target, MethodInfo method)
    {
        var genericCreator =
                typeof(GenericBasedActionGenerator).GetMethod(nameof(CreateAction2ArgsGeneric),
                                                              BindingFlags.Static | BindingFlags.NonPublic);
        var correctCreator = genericCreator.MakeGenericMethod(method.GetParameters()[0].ParameterType,
                                                              method.GetParameters()[1].ParameterType);
        return correctCreator.Invoke(null, new[] { target, method }) as Action<object, object>;
    }

    private static Action<object, object, object>
        CreateAction3ArgsGeneric<TArg1, TArg2, TArg3>(object target,
                                                      MethodInfo method)
    {
        var act = Delegate.CreateDelegate(typeof(Action<TArg1, TArg2, TArg3>),
                                          target,
                                          method) as Action<TArg1, TArg2, TArg3>;
        return (arg1, arg2, arg3) => act((TArg1)arg1, (TArg2)arg2, (TArg3)arg3);
    }

    private static Action<object, object, object> CreateAction3Args(object target, MethodInfo method)
    {
        var genericCreator =
                typeof(GenericBasedActionGenerator).GetMethod(nameof(CreateAction3ArgsGeneric),
                                                              BindingFlags.Static | BindingFlags.NonPublic);
        var correctCreator = genericCreator.MakeGenericMethod(method.GetParameters()[0].ParameterType,
                                                              method.GetParameters()[1].ParameterType,
                                                              method.GetParameters()[2].ParameterType);
        return correctCreator.Invoke(null, new[] { target, method }) as Action<object, object, object>;
    }

    private static Action<object, object, object, object>
        CreateAction4ArgsGeneric<TArg1, TArg2, TArg3, TArg4>(object target,
                                                            MethodInfo method)
    {
        var act = Delegate.CreateDelegate(typeof(Action<TArg1, TArg2, TArg3, TArg4>),
                                          target,
                                          method) as Action<TArg1, TArg2, TArg3, TArg4>;
        return (arg1, arg2, arg3, arg4) => act((TArg1)arg1, (TArg2)arg2, (TArg3)arg3, (TArg4)arg4);
    }

    private static Action<object, object, object, object> CreateAction4Args(object target, MethodInfo method)
    {
        var genericCreator =
            typeof(GenericBasedActionGenerator).GetMethod(nameof(CreateAction4ArgsGeneric),
                                                          BindingFlags.Static | BindingFlags.NonPublic);
        var correctCreator = genericCreator.MakeGenericMethod(method.GetParameters()[0].ParameterType,
                                                              method.GetParameters()[1].ParameterType,
                                                              method.GetParameters()[2].ParameterType,
                                                              method.GetParameters()[3].ParameterType);
        return correctCreator.Invoke(null, new[] { target, method })
                as Action<object, object, object, object>;
    }

    private static Action<object, object, object, object, object>
        CreateAction5ArgsGeneric<TArg1, TArg2, TArg3, TArg4, TArg5>(object target,
                                                                         MethodInfo method)
    {
        var act = Delegate.CreateDelegate(typeof(Action<TArg1, TArg2, TArg3, TArg4, TArg5>),
                                           target,
                                           method) as Action<TArg1, TArg2, TArg3, TArg4, TArg5>;
        return (arg1, arg2, arg3, arg4, arg5) => act((TArg1)arg1, (TArg2)arg2, (TArg3)arg3, (TArg4)arg4, (TArg5)arg5);
    }

    private static Action<object, object, object, object, object> CreateAction5Args(object target, MethodInfo method)
    {
        var genericCreator =
                typeof(GenericBasedActionGenerator).GetMethod(nameof(CreateAction5ArgsGeneric),
                                                              BindingFlags.Static | BindingFlags.NonPublic);
        var correctCreator = genericCreator.MakeGenericMethod(method.GetParameters()[0].ParameterType,
                                                              method.GetParameters()[1].ParameterType,
                                                              method.GetParameters()[2].ParameterType,
                                                              method.GetParameters()[3].ParameterType,
                                                              method.GetParameters()[4].ParameterType);
        return correctCreator.Invoke(null, new[] { target, method })
                as Action<object, object, object, object, object>;
    }
}
