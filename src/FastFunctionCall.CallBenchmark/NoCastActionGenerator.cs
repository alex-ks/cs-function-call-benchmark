using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class NoCastActionGenerator
{
    public static Action ExtractAction(object target, MethodInfo method, Delegate[] argumentGetters)
    {
        if (method.ReturnType != typeof(void))
            throw new ArgumentException("For now we work with void methods only");

        var parameters = method.GetParameters();
        if (parameters.Length != argumentGetters.Length)
            throw new ArgumentException("Method arguments count differs from provided arguments");

        if (parameters.Length > 5)
        {
            var generalizedGetters = argumentGetters.Select(g => GeneralizeGetter(g)).ToArray();
            return () => method.Invoke(target, generalizedGetters.Select(g => g()).ToArray());
        }

        // Let the magic begin!
        if (parameters.Length == 0)
            return CreateStrictActionZeroArg(target, method);
        string creatorName = null;
        switch (parameters.Length)
        {
            case 1:
                creatorName = nameof(CreateStrictAction1ArgGeneric);
                break;
            case 2:
                creatorName = nameof(CreateStrictAction2ArgsGeneric);
                break;
            case 3:
                creatorName = nameof(CreateStrictAction3ArgsGeneric);
                break;
            case 4:
                creatorName = nameof(CreateStrictAction4ArgsGeneric);
                break;
            case 5:
                creatorName = nameof(CreateStrictAction5ArgsGeneric);
                break;
            default:
                throw new ArgumentException("Method must have less than 5 parameters.");
        }
        var genericCreator =
                typeof(NoCastActionGenerator).GetMethod(creatorName, BindingFlags.Static | BindingFlags.NonPublic);
        var typeParameters = parameters.Select(p => p.ParameterType).ToArray();
        var exactCreator = genericCreator.MakeGenericMethod(typeParameters);
        var exactCreatorArgs = Enumerable.Empty<object>().Append(target).Append(method).Concat(argumentGetters);
        return exactCreator.Invoke(null, exactCreatorArgs.ToArray()) as Action;
    }

    // Actions
    private static Action CreateStrictActionZeroArg(object target, MethodInfo method)
    {
        var act = Delegate.CreateDelegate(typeof(Action), target, method) as Action;
        return act;
    }

    private static Action CreateStrictAction1ArgGeneric<TArg>(object target, MethodInfo method, Delegate argGetter)
    {
        var act = Delegate.CreateDelegate(typeof(Action<TArg>), target, method) as Action<TArg>;
        var getter = argGetter as Func<TArg>;
        if (getter == null)
            throw new ArgumentException("argGetter does not have expected signature.");
        return () => act(getter());
    }

    private static Action CreateStrictAction2ArgsGeneric<TArg1, TArg2>(object target,
                                                                       MethodInfo method,
                                                                       Delegate argGetter1,
                                                                       Delegate argGetter2)
    {
        var act = Delegate.CreateDelegate(typeof(Action<TArg1, TArg2>), target, method) as Action<TArg1, TArg2>;
        var getter1 = argGetter1 as Func<TArg1>;
        var getter2 = argGetter2 as Func<TArg2>;
        if (getter1 == null)
            throw new ArgumentException("argGetter1 does not have expected signature.");
        if (getter2 == null)
            throw new ArgumentException("argGetter2 does not have expected signature.");
        return () => act(getter1(), getter2());
    }

    private static Action CreateStrictAction3ArgsGeneric<TArg1, TArg2, TArg3>(object target,
                                                                              MethodInfo method,
                                                                              Delegate argGetter1,
                                                                              Delegate argGetter2,
                                                                              Delegate argGetter3)
    {
        var act = Delegate.CreateDelegate(typeof(Action<TArg1,
                                                        TArg2,
                                                        TArg3>), target, method) as Action<TArg1, TArg2, TArg3>;
        var getter1 = argGetter1 as Func<TArg1>;
        var getter2 = argGetter2 as Func<TArg2>;
        var getter3 = argGetter3 as Func<TArg3>;
        if (getter1 == null)
            throw new ArgumentException("argGetter1 does not have expected signature.");
        if (getter2 == null)
            throw new ArgumentException("argGetter2 does not have expected signature.");
        if (getter3 == null)
            throw new ArgumentException("argGetter3 does not have expected signature.");
        return () => act(getter1(), getter2(), getter3());
    }

    private static Action CreateStrictAction4ArgsGeneric<TArg1, TArg2, TArg3, TArg4>(object target,
                                                                                     MethodInfo method,
                                                                                     Delegate argGetter1,
                                                                                     Delegate argGetter2,
                                                                                     Delegate argGetter3,
                                                                                     Delegate argGetter4)
    {
        var act = Delegate.CreateDelegate(typeof(Action<TArg1,
                                                        TArg2,
                                                        TArg3,
                                                        TArg4>), target, method) as Action<TArg1,
                                                                                           TArg2,
                                                                                           TArg3,
                                                                                           TArg4>;
        var getter1 = argGetter1 as Func<TArg1>;
        var getter2 = argGetter2 as Func<TArg2>;
        var getter3 = argGetter3 as Func<TArg3>;
        var getter4 = argGetter4 as Func<TArg4>;
        if (getter1 == null)
            throw new ArgumentException("argGetter1 does not have expected signature.");
        if (getter2 == null)
            throw new ArgumentException("argGetter2 does not have expected signature.");
        if (getter3 == null)
            throw new ArgumentException("argGetter3 does not have expected signature.");
        if (getter4 == null)
            throw new ArgumentException("argGetter4 does not have expected signature.");
        return () => act(getter1(), getter2(), getter3(), getter4());
    }

    private static Action CreateStrictAction5ArgsGeneric<TArg1, TArg2, TArg3, TArg4, TArg5>(
            object target,
            MethodInfo method,
            Delegate argGetter1,
            Delegate argGetter2,
            Delegate argGetter3,
            Delegate argGetter4,
            Delegate argGetter5)
    {
        var act =
                Delegate.CreateDelegate(typeof(Action<TArg1, TArg2, TArg3, TArg4, TArg5>),
                                        target, method)
                as Action<TArg1, TArg2, TArg3, TArg4, TArg5>;
        var getter1 = argGetter1 as Func<TArg1>;
        var getter2 = argGetter2 as Func<TArg2>;
        var getter3 = argGetter3 as Func<TArg3>;
        var getter4 = argGetter4 as Func<TArg4>;
        var getter5 = argGetter5 as Func<TArg5>;
        if (getter1 == null)
            throw new ArgumentException("argGetter1 does not have expected signature.");
        if (getter2 == null)
            throw new ArgumentException("argGetter2 does not have expected signature.");
        if (getter3 == null)
            throw new ArgumentException("argGetter3 does not have expected signature.");
        if (getter4 == null)
            throw new ArgumentException("argGetter4 does not have expected signature.");
        if (getter5 == null)
            throw new ArgumentException("argGetter5 does not have expected signature.");
        return () => act(getter1(), getter2(), getter3(), getter4(), getter5());
    }

    private static Func<object> GeneralizeGetterGeneric<T>(Delegate getter)
    {
        var func = getter as Func<T>;
        return () => func();
    }

    private static Func<object> GeneralizeGetter(Delegate getter)
    {
        if (getter.Method.GetParameters().Length != 0)
            throw new ArgumentException("Getter should have no arguments");
        var genericCreator =
                typeof(NoCastActionGenerator).GetMethod(nameof(GeneralizeGetterGeneric),
                                                  BindingFlags.Static | BindingFlags.NonPublic);
        var exactCreator = genericCreator.MakeGenericMethod(getter.Method.ReturnType);
        return exactCreator.Invoke(null, new[] { getter }) as Func<object>;
    }
}
