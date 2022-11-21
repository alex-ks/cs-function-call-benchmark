# Метод барона Мюнхгаузена, или как быстро вызвать функцию, о которой (почти) ничего не знаешь

Эта история началась с вдохновения Lego Mindstorms и ASP.NET, а закончилась микробенчмарканьем пустых функций и поисками наиболее изощрённых использований дженериков. Если вам интересны рефлексия в C#, метапрограммирование, оптимизации кода и просто чужие страдания - добро пожаловать в статью.

В один прекрасный день я приобрёл себе Lego Mindstorms. Всласть наигравшись с конструктором, я очень скоро разочаровался в его среде разработки и задался целью написать для него фреймворк для разработки на C#. Примерно в то же время я изучал ASP.NET Core, поэтому сильно вдохновлялся дизайном его контроллеров: логика робота описывается одним классом, методы которого – шаги одной итерации event loop'а робота, получающие в качестве аргументов значения сенсоров. Для реализации этого нужно научиться вызывать методы произвольной сигнатуры в рантайме, а с учётом того, что работает это всё на дохленьком ARM9 – это нужно делать *быстро*. Как говорили классики, "и заверте..."

Чтобы этот текст был максимально простым и вместе с тем полезным, я постараюсь абстрагироваться от изначальной задачи. Так, я не буду описывать, как роботогенератор находит нужные в какой-то момент времени функции и их аргументы, и сосредоточусь только на том, как эти функции, собственно, вызвать. Ещё одно упрощение – нас будут интересовать только методы без возвращаемого значения (void). Никаких принципиальных отличий при работе с возвращаемыми значениями в приведённых ниже схемах не будет.

## Первое приближение

Первое, что пришло в голову для такой задачи – воспользоваться рефлексией:

```C#
var method = model.GetType().GetMethod(methodName,
                                       BindingFlags.Public | BindingFlags.Instance);
method.Invoke(target, new [] { arg1, arg2 /*, ... */ });
```

Ищем среди публичных методов нужный нам `MethodInfo`, складываем необходимые ему аргументы (нам даже не нужно знать их тип) в массив объектов и вызываем. Коротко и понятно. Но работа с reflection – дело дорогое. А вот насколько дорогое? Чтобы не быть голословными, сделаем небольшой бенчмарк с помощью библиотеки [BenchmarkDotNet](https://benchmarkdotnet.org/):

```C#
public class FunctionCallBenchmark
{
    private int Arg1 => 1;
    private char Arg2 => '2';
    private double Arg3 => 3.0;
    private bool Arg4 => true;
    private string Arg5 => "5";
    private decimal Arg6 => 6.0m;

    public void ZeroArgMethod() { }
    public void OneArgMethod(int arg1) { }
    public void TwoArgMethod(int arg1, char arg2) { }
    public void ThreeArgMethod(int arg1, char arg2, double arg3) { }
    public void FourArgMethod(int arg1, char arg2, double arg3, bool arg4) { }
    public void FiveArgMethod(int arg1, char arg2, double arg3, bool arg4, string arg5) { }
    public void SixArgMethod(int arg1, char arg2, double arg3, bool arg4, string arg5, decimal arg6) { }

    // Reflection-based:
    private Action _reflectionBasedZeroArgAction;
    /* ... */
    private Action _reflectionBasedSixArgAction;

    public FunctionCallBenchmark()
    {
        var objectGetters =
                new Func<object>[] { () => Arg1, () => Arg2, () => Arg3, () => Arg4, () => Arg5, () => Arg6 };

        _reflectionBasedZeroArgAction = GenerateReflectionBasedAction(nameof(ZeroArgMethod), objectGetters.Take(0));
        /* ... */
        _reflectionBasedFiveArgAction = GenerateReflectionBasedAction(nameof(FiveArgMethod), objectGetters.Take(5));
        _reflectionBasedSixArgAction = GenerateReflectionBasedAction(nameof(SixArgMethod), objectGetters);
    }

    private Action GenerateReflectionBasedAction(string methodName, IEnumerable<Func<object>> parameters)
    {
        var method = this.GetType().GetMethod(methodName,
                                              BindingFlags.Public | BindingFlags.Instance);
        if (method == null)
            throw new ArgumentNullException(methodName);

        // Pre-create the arguments array to remove the allocation costs.
        var gettersArray = parameters.ToArray();
        var argsArray = new object[gettersArray.Length];
        return () =>
        {
            for (int i = 0; i < argsArray.Length; ++i)
                argsArray[i] = gettersArray[i]();
            method.Invoke(this, argsArray);
        };
    }

    // Regular calls:
    [Benchmark]
    public void RegularZeroArgCall() => ZeroArgMethod();

    /* ... */

    [Benchmark]
    public void RegularSixArgCall() => SixArgMethod(Arg1, Arg2, Arg3, Arg4, Arg5, Arg6);

    // Reflection-based calls:
    [Benchmark]
    public void ReflectionBasedZeroArgAction() => _reflectionBasedZeroArgAction();

    /* ... */

    [Benchmark]
    public void ReflectionBasedSixArgAction() => _reflectionBasedSixArgAction();
}
```

Сравнивать будем обычные вызовы функций с количеством аргументов от 1 до 6 (почему столько – будет понятно чуть позднее) с этими же функциями, но скрытыми под делегатом `Action`, полученным из Reflection. Чтобы не замерять ненужное нам время на выделение памяти под массив аргументов, создадим его один раз заранее, а потом просто будем заполнять (на практике затраты на выделение массива варьировались от 30% для одного аргумента до 10% для 6 аргументов).

Тестирование проводилось на MacBook Pro M1 2020. Результатов с целевой платформы, увы, [пока не увидеть](https://github.com/mono/mono/issues/12537#issuecomment-554770577), так что ограничимся лишь академическим интересом. Результаты получились следующие:

|                         Method |        Mean |     Error |    StdDev |      Median |
|------------------------------- |------------:|----------:|----------:|------------:|
|             RegularZeroArgCall |   0.0023 ns | 0.0035 ns | 0.0033 ns |   0.0000 ns |
|              RegularOneArgCall |   0.0000 ns | 0.0000 ns | 0.0000 ns |   0.0000 ns |
|              RegularTwoArgCall |   0.0168 ns | 0.0024 ns | 0.0022 ns |   0.0168 ns |
|            RegularThreeArgCall |   0.0008 ns | 0.0014 ns | 0.0013 ns |   0.0000 ns |
|             RegularFourArgCall |   0.0011 ns | 0.0019 ns | 0.0017 ns |   0.0000 ns |
|             RegularFiveArgCall |   0.0037 ns | 0.0038 ns | 0.0032 ns |   0.0032 ns |
|              RegularSixArgCall |   0.0022 ns | 0.0026 ns | 0.0022 ns |   0.0018 ns |
|   ReflectionBasedZeroArgAction |  64.9950 ns | 0.5477 ns | 0.5123 ns |  65.2141 ns |
|    ReflectionBasedOneArgAction | 139.0408 ns | 0.3873 ns | 0.3433 ns | 138.9701 ns |
|    ReflectionBasedTwoArgAction | 201.3532 ns | 0.6453 ns | 0.5389 ns | 201.2620 ns |
|  ReflectionBasedThreeArgAction | 261.2793 ns | 0.8609 ns | 0.7632 ns | 261.1321 ns |
|   ReflectionBasedFourArgAction | 316.6166 ns | 1.2285 ns | 1.1492 ns | 316.6238 ns |
|   ReflectionBasedFiveArgAction | 380.0440 ns | 2.4919 ns | 2.3310 ns | 379.7985 ns |
|    ReflectionBasedSixArgAction | 448.8128 ns | 1.6282 ns | 1.5230 ns | 448.9000 ns |

Вау! Как видно, на чистый вызов пустой функции тратится порядка сотой наносекунды. А вот вызов функции через Reflection занимает аж на 4 порядка больше, да ещё и линейно растёт с количеством аргументов. С таким временем вызова (а мы ведь не только вызывать функции хотим) наш робот далеко не уедет.

*Ремарка: первые тесты я проводил на MacBook Pro Mid 2014 (Core i7-4870HQ) и .NET Core 3.1, и для чистого вызова функции от 6 аргументов наблюдается стабильный прыжок времени до трети наносекунды. Но что это – нехватка регистров или окончание кэш-линии – так и остаётся для меня загадкой.*

## Git the princess

Помимо рефлексии, в C# существует ещё один способ вызова методов с ограниченным знанием о них: с помощью делегатов. Если мы знаем сигнатуру метода, то имея на руках его `MethodInfo`, можно легко и безболезненно сконвертировать его в делегат (аки указатель на функцию) с этой сигнатурой:

```C#
var action = Delegate.CreateDelegate(typeof(Action<int, char>), target, method) as Action<int, char>;
```

Работает это дело практически неотличимо от обычного вызова функции. Но вот беда: сигнатуру-то метода мы не знаем!

Как известно, если есть какая-либо проблема в C#, её уже давно решил Джон Скит. Эта оказалась [не исключением](https://codeblog.jonskeet.uk/2008/08/09/making-reflection-fly-and-exploring-delegates/).

![Jon Skeet joke](img/jon_skeet.jpg)

Основная его идея в том, что в C# всё-таки есть механизм, допускающий использование до определённой степени неизвестной сигнатуры (с точностью до количества аргументов): generics. И вправду, если допустить, что мы всё-таки знаем количество аргументов, то можно сделать следующее:

```C#
private static Action<object, object> CreateAction2ArgsGeneric<TArg1, TArg2>(object target, MethodInfo method)
{
    var act = Delegate.CreateDelegate(typeof(Action<TArg1, TArg2>),
                                      target,
                                      method) as Action<TArg1, TArg2>;
    return (arg1, arg2) => act((TArg1)arg1, (TArg2)arg2);
}
```

Исполнив этот метод, мы получим делегат, который в дальнейшем уже сможем спокойно вызвать, не зная наперёд типов передаваемых аргументов, но зная, что они совпадают с тем, что целевой метод хочет получить на вход – а уж это мы сможем как-нибудь гарантировать.

"Но погодите," – скажете вы: "мы ведь по-прежнему не знаем типы `TArg1` и `TArg2`!"

Почти. Мы не знаем эти типы в *compile-time* – но в *runtime*-то знаем!

Здесь и кроется главная хитрость: нам всё-таки придётся позвать `method.Invoke`, но только не у целевого метода, а у самих себя! Т.к. наш метод – generic, то, получив его `MethodInfo`, мы сможем подставить нужные нам типы аргументов с помощью рефлексии:

```C#
private static Action<object, object> CreateAction2Args(object target, MethodInfo method)
{
    var genericCreator =
            typeof(GenericBasedActionGenerator).GetMethod(nameof(CreateAction2ArgsGeneric),
                                                          BindingFlags.Static | BindingFlags.NonPublic);
    var correctCreator = genericCreator.MakeGenericMethod(method.GetParameters()[0].ParameterType,
                                                          method.GetParameters()[1].ParameterType);
    return correctCreator.Invoke(null, new[] { target, method }) as Action<object, object>;
}
```

Вся эта конструкция живо напомнила мне вот эту картинку:
![Барон Бюнхгаузен вытаскивает себя за волосы](img/baron-miunkhgauzen.jpg)

Отсюда и название статьи.

Таким образом, мы всё-таки зовём медленный `MethodInfo.Invoke`, но только один раз и только для того, чтобы потом звать желанный делегат быстро.

*Небольшая ремарка*: в своём блоге Джон Скит использует открытые делегаты, т.е. делегаты, в сигнатуре которых явно указан объект-`target` как первый аргумент. Поэтому Джон оставляет пометку, что для методов value-типов этот первый аргумент (сам `target`) в сигнатуре должен быть помечен как `ref`, что не даёт использовать нам встроенные типы делегатов `Action` и `Func`. Мы же используем закрытые делегаты (т.е. сигнатуры метода и делегата совпадают, а целевой объект неявно скрывается внутри делегата), передавая `target` в `CreateDelegate`, поэтому эта проблема обходит нас стороной.

Также Джон Скит тактично умолчал об ещё одной проблеме этого подхода, которую вы могли уже заметить: а что же нам делать, если количество аргументов нам тоже наперёд неизвестно?

Увы, для метода барона Мюнхгаузена ответ один: страдать. То есть – копипастить. К сожалению, generics в C# не поддерживают переменное число параметров (привет, variadic templates в C++!), поэтому здесь у нас связаны руки. Мне хватило терпения (и чувства прекрасного) на определения таких конвертеров до 5 аргументов включительно (впрочем, Microsoft не поленился объявить версии `System.Action` до [16 аргументов включительно](https://docs.microsoft.com/dotnet/api/system.action-16), так что чего нам бояться). Именно поэтому мы тестируем методы до 6 аргументов: чтобы увидеть и выигрыш от нашей оптимизации, и скачок до неоптимизированной версии.

Итак, мы ~~накопипастили~~ научились генерировать быстрые вызовы функций с каким-то фиксированным количеством аргументов. Дело за малым: чтобы дальше с этим методом можно было работать обобщённо, нужно превратить работу с N неизвестными аргументами в работу с массивом неизвестных аргументов:

```C#
// Assume that we can get values of our currently unknown arguments via Func<object>.
public static Action ExtractAction(object target, MethodInfo method, Func<object>[] args)
{
    switch (method.GetParameters().Length)
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
        /* Same copy-paste here */
        case 5:
        {
            var convertedAct = CreateAction<Action<object, object, object, object, object>>(target, method);
            return () => convertedAct(args[0](), args[1](), args[2](), args[3](), args[4]());
        }
        default:
            return () => method.Invoke(target, args.Select(getter => getter()).ToArray());
    }
}
```

Мерзенько, но на что только не пойдёшь ради науки.

Ну да хватит, скажете вы, каких-то хтонических конструкций нагородили, а ради чего? Где тесты, Билли?

Вот:
|                         Method |        Mean |     Error |    StdDev |      Median |
|------------------------------- |------------:|----------:|----------:|------------:|
|             RegularZeroArgCall |   0.0023 ns | 0.0035 ns | 0.0033 ns |   0.0000 ns |
|              RegularOneArgCall |   0.0000 ns | 0.0000 ns | 0.0000 ns |   0.0000 ns |
|              RegularTwoArgCall |   0.0168 ns | 0.0024 ns | 0.0022 ns |   0.0168 ns |
|            RegularThreeArgCall |   0.0008 ns | 0.0014 ns | 0.0013 ns |   0.0000 ns |
|             RegularFourArgCall |   0.0011 ns | 0.0019 ns | 0.0017 ns |   0.0000 ns |
|             RegularFiveArgCall |   0.0037 ns | 0.0038 ns | 0.0032 ns |   0.0032 ns |
|              RegularSixArgCall |   0.0022 ns | 0.0026 ns | 0.0022 ns |   0.0018 ns |
|   ReflectionBasedZeroArgAction |  64.9950 ns | 0.5477 ns | 0.5123 ns |  65.2141 ns |
|    ReflectionBasedOneArgAction | 139.0408 ns | 0.3873 ns | 0.3433 ns | 138.9701 ns |
|    ReflectionBasedTwoArgAction | 201.3532 ns | 0.6453 ns | 0.5389 ns | 201.2620 ns |
|  ReflectionBasedThreeArgAction | 261.2793 ns | 0.8609 ns | 0.7632 ns | 261.1321 ns |
|   ReflectionBasedFourArgAction | 316.6166 ns | 1.2285 ns | 1.1492 ns | 316.6238 ns |
|   ReflectionBasedFiveArgAction | 380.0440 ns | 2.4919 ns | 2.3310 ns | 379.7985 ns |
|    ReflectionBasedSixArgAction | 448.8128 ns | 1.6282 ns | 1.5230 ns | 448.9000 ns |
|      GenericBasedZeroArgAction |   1.6060 ns | 0.0036 ns | 0.0028 ns |   1.6070 ns |
|       GenericBasedOneArgAction |   6.5625 ns | 0.0123 ns | 0.0115 ns |   6.5623 ns |
|       GenericBasedTwoArgAction |  10.8101 ns | 0.0373 ns | 0.0331 ns |  10.8021 ns |
|     GenericBasedThreeArgAction |  14.6319 ns | 0.0544 ns | 0.0425 ns |  14.6250 ns |
|      GenericBasedFourArgAction |  19.3778 ns | 0.0706 ns | 0.0551 ns |  19.3872 ns |
|      GenericBasedFiveArgAction |  21.6174 ns | 0.0812 ns | 0.0720 ns |  21.6054 ns |
|       GenericBasedSixArgAction | 509.3339 ns | 1.5230 ns | 1.3501 ns | 509.4516 ns |

*Здесь я уже опущу модификации бенчмарка, в конце поста будет ссылка на полный бенчмарк на GitHub.*

Неплохо! Для "вытянутых из болота" методов мы получили прирост в 20 раз в худшем случае, и в 40 – в лучшем! Но как только мы выходим за пределы оптимизированного числа аргументов, мы сразу тонем в трясине, что неприятно.

В комментариях к посту Скита был предложен ещё один интересный способ: с помощью `System.Linq.Expressions`. В этом способе нам нужно руками собрать лямбда-выражение, которое будет вызывать нужный нам метод. Выглядеть это будет следующим образом:

```C#
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
```

Здесь мы объявляем параметры `expressionParameters`, которые потом и будут параметрами нашей лямбды, создаём константу, на место которой будет подставлен `target`, строим AST вызова метода и компилируем его в лямбду. Благодаря тому, что мы объявили типы параметров как `object` и поставили приведение к типу, полученному из параметра метода, мы можем спокойно указывать в качестве `TLambda` `Action`, принимающие любое количество `object`-ов, не беспокоясь о том, что типы аргументов нам неизвестны.

Пожалуй, этот код действительно понятнее, чем магия барона Мюнхгаузена, но на него распространяется то же ограничение: для приведения типа к какому-то `Action<object, ..., object>` нам всё-таки нужно знать количество его аргументов, что ведёт к той же копипасте и тому страшному свитчу.

Посмотрим, на что способны эти `Expression`:

|                         Method |        Mean |     Error |    StdDev |      Median |
|------------------------------- |------------:|----------:|----------:|------------:|
|      GenericBasedZeroArgAction |   1.6060 ns | 0.0036 ns | 0.0028 ns |   1.6070 ns |
|       GenericBasedOneArgAction |   6.5625 ns | 0.0123 ns | 0.0115 ns |   6.5623 ns |
|       GenericBasedTwoArgAction |  10.8101 ns | 0.0373 ns | 0.0331 ns |  10.8021 ns |
|     GenericBasedThreeArgAction |  14.6319 ns | 0.0544 ns | 0.0425 ns |  14.6250 ns |
|      GenericBasedFourArgAction |  19.3778 ns | 0.0706 ns | 0.0551 ns |  19.3872 ns |
|      GenericBasedFiveArgAction |  21.6174 ns | 0.0812 ns | 0.0720 ns |  21.6054 ns |
|       GenericBasedSixArgAction | 509.3339 ns | 1.5230 ns | 1.3501 ns | 509.4516 ns |
|   ExpressionBasedZeroArgAction |   1.7778 ns | 0.0033 ns | 0.0029 ns |   1.7771 ns |
|    ExpressionBasedOneArgAction |   6.4550 ns | 0.0073 ns | 0.0065 ns |   6.4532 ns |
|    ExpressionBasedTwoArgAction |  10.8472 ns | 0.0514 ns | 0.0481 ns |  10.8501 ns |
|  ExpressionBasedThreeArgAction |  14.4573 ns | 0.1306 ns | 0.1222 ns |  14.4215 ns |
|   ExpressionBasedFourArgAction |  18.7730 ns | 0.0617 ns | 0.0578 ns |  18.7706 ns |
|   ExpressionBasedFiveArgAction |  19.8925 ns | 0.0604 ns | 0.0565 ns |  19.8935 ns |
|    ExpressionBasedSixArgAction | 507.2038 ns | 1.3393 ns | 1.2527 ns | 507.4734 ns |

Результаты практически идентичны.

# We need to go deeper

Стоит заметить, что в обоих "быстрых" вариантах мы делаем одну и ту же дорогую операцию – приведение типов.
Причём дорогую в обе стороны: в случае value-аргументов мы получаем боксинг при приведении к object, и вообще во всех случаях мы получаем долгую проверку типа при приведении `object` обратно к `TArg`. Ведь должен быть способ это как-то обойти?

И способ действительно есть, если осознать, что с источниками аргументов мы можем проделать ровно тот же трюк. Напомню, что в изначальных условиях источники аргументов – это свойства (properties) этого же класса. Для свойств можно получить `MethodInfo` геттера с помощью `PropertyInfo.GetMethod`. Конкретно у этого метода будет очень простая сигнатура: нет аргументов и единственное возрващаемое значение неизвестного *в compile-time* типа. Ну уж с этими мы уже умеем работать:

```C#
public static Delegate CreateFuncZeroArgRaw(object target, MethodInfo method)
{
    var getterType = typeof(Func<>).MakeGenericType(method.ReturnType);
    return Delegate.CreateDelegate(getterType, target, method);
}
```

Здесь мы нагло пользуемся тем свойством (не property), что абсолютно все делегаты любой сигнатуры являются наследниками `System.Delegate`, поэтому мы можем получить вожделенный делегат, ни разу не объявив его возвращаемый тип!

Итого, мы с помощью generics вытянули обобщённый геттер для аргументов, чтобы потом с помощью generics внедрить его в вызов функции, типы аргументов которой мы знаем только в generic-контексте.

![We need to go deeper](/img/deeper.jpeg)

Выглядит это как-то так:

```C#
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
```

Осталось только упаковать методы, принимающие фиксированное число делегатов, в принятие массива делегатов. Поскольку нам уже не надо дифференцировать возвращаемые значения методов по количеству аргументов (т.к. аргументы уже "упакованы" в вызов метода), различие в вызове методов теперь состоит только в имени вызываемого экстрактора:

```C#
public static Action ExtractAction(object target, MethodInfo method, Delegate[] argumentGetters)
{
    var parameters = method.GetParameters();
    if (parameters.Length > 5)
    {
        // GeneralizeGetter simply wrapps Func<TRet> into Func<object> using the same MakeGenericMethod technique.
        var generalizedGetters = argumentGetters.Select(g => GeneralizeGetter(g)).ToArray();
        return () => method.Invoke(target, generalizedGetters.Select(g => g()).ToArray());
    }

    if (parameters.Length == 0)
        return CreateStrictActionZeroArg(target, method);
    string creatorName = (parameters.Length) switch
    {
        1 => nameof(CreateStrictAction1ArgGeneric),
        2 => nameof(CreateStrictAction2ArgsGeneric),
        3 => nameof(CreateStrictAction3ArgsGeneric),
        4 => nameof(CreateStrictAction4ArgsGeneric),
        5 => nameof(CreateStrictAction5ArgsGeneric),
        _ => throw new ArgumentException("Method must have less than 5 parameters.")
    };
    var genericCreator =
            typeof(NoCastActionGenerator).GetMethod(creatorName, BindingFlags.Static | BindingFlags.NonPublic);
    var typeParameters = parameters.Select(p => p.ParameterType).ToArray();
    var exactCreator = genericCreator.MakeGenericMethod(typeParameters);
    var exactCreatorArgs = Enumerable.Empty<object>().Append(target).Append(method).Concat(argumentGetters);
    return exactCreator.Invoke(null, exactCreatorArgs.ToArray()) as Action;
}
```

Что ж, выглядеть оно стало чуть приятнее. Кроме того, мы вынесли все приведения типов на уровень генерации `Action`, избежав во время вызова как проверок, так и боксинга. Должно же это было сказаться на скорости?

Должно:
|                         Method |        Mean |     Error |    StdDev |      Median |
|------------------------------- |------------:|----------:|----------:|------------:|
|      GenericBasedZeroArgAction |   1.6060 ns | 0.0036 ns | 0.0028 ns |   1.6070 ns |
|       GenericBasedOneArgAction |   6.5625 ns | 0.0123 ns | 0.0115 ns |   6.5623 ns |
|       GenericBasedTwoArgAction |  10.8101 ns | 0.0373 ns | 0.0331 ns |  10.8021 ns |
|     GenericBasedThreeArgAction |  14.6319 ns | 0.0544 ns | 0.0425 ns |  14.6250 ns |
|      GenericBasedFourArgAction |  19.3778 ns | 0.0706 ns | 0.0551 ns |  19.3872 ns |
|      GenericBasedFiveArgAction |  21.6174 ns | 0.0812 ns | 0.0720 ns |  21.6054 ns |
|       GenericBasedSixArgAction | 509.3339 ns | 1.5230 ns | 1.3501 ns | 509.4516 ns |
|            StrictZeroArgAction |   0.6344 ns | 0.0026 ns | 0.0023 ns |   0.6340 ns |
|             StrictOneArgAction |   2.3428 ns | 0.0051 ns | 0.0045 ns |   2.3411 ns |
|             StrictTwoArgAction |   3.1607 ns | 0.0046 ns | 0.0036 ns |   3.1617 ns |
|           StrictThreeArgAction |   4.1434 ns | 0.0046 ns | 0.0039 ns |   4.1432 ns |
|            StrictFourArgAction |   5.0828 ns | 0.0078 ns | 0.0069 ns |   5.0818 ns |
|            StrictFiveArgAction |   6.3591 ns | 0.0119 ns | 0.0093 ns |   6.3592 ns |
|             StrictSixArgAction | 515.0386 ns | 1.3343 ns | 1.2481 ns | 514.7397 ns |

*Здесь наше последнее решение названо Strict\*, т.к. мы не производим никаких преобразований и боксингов во время вызова функции.*

Порядок мы, конечно, не сэкономили, но всё равно – трёхкратное ускорение! Мы всё ещё видим линейный рост от количества аргументов, но, по всей видимости, тут он уже связан с получением аргументов с помощью делегатов (которые как минимум в одной из версий .NET делали virtcall для проверки таргет-объекта на `null`).

А что же `Expression`?

Спрятав получение аргументов внутрь вызова метода, мы полностью развязали себе руки и теперь можем написать one generator to rule them all:

```C#
public static Action ExtractAction(object target, MethodInfo method, Delegate[] args)
{
    var parameters = method.GetParameters();

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
```

При совпадающих типах переданных и ожидаемых аргументов, значение делегата-геттера без преобразований присваивается константе с типом `Func<>`, параметризованной ожидаемым типом аргумента, что проверяется только на этапе компиляции целевой лямбды – один раз за работу программы. А дальше мы уже спокойно собираем наш вызов функции, радуясь тому, что возвращаемый тип – всегда `Action`.

И при всей лаконичности этого кода у него есть один недостаток: он *медленней*, чем strict-вариант на дженериках:

|                         Method |        Mean |     Error |    StdDev |      Median |
|------------------------------- |------------:|----------:|----------:|------------:|
|   ExpressionBasedZeroArgAction |   1.7778 ns | 0.0033 ns | 0.0029 ns |   1.7771 ns |
|    ExpressionBasedOneArgAction |   6.4550 ns | 0.0073 ns | 0.0065 ns |   6.4532 ns |
|    ExpressionBasedTwoArgAction |  10.8472 ns | 0.0514 ns | 0.0481 ns |  10.8501 ns |
|  ExpressionBasedThreeArgAction |  14.4573 ns | 0.1306 ns | 0.1222 ns |  14.4215 ns |
|   ExpressionBasedFourArgAction |  18.7730 ns | 0.0617 ns | 0.0578 ns |  18.7706 ns |
|   ExpressionBasedFiveArgAction |  19.8925 ns | 0.0604 ns | 0.0565 ns |  19.8935 ns |
|    ExpressionBasedSixArgAction | 507.2038 ns | 1.3393 ns | 1.2527 ns | 507.4734 ns |
|            StrictZeroArgAction |   0.6344 ns | 0.0026 ns | 0.0023 ns |   0.6340 ns |
|             StrictOneArgAction |   2.3428 ns | 0.0051 ns | 0.0045 ns |   2.3411 ns |
|             StrictTwoArgAction |   3.1607 ns | 0.0046 ns | 0.0036 ns |   3.1617 ns |
|           StrictThreeArgAction |   4.1434 ns | 0.0046 ns | 0.0039 ns |   4.1432 ns |
|            StrictFourArgAction |   5.0828 ns | 0.0078 ns | 0.0069 ns |   5.0818 ns |
|            StrictFiveArgAction |   6.3591 ns | 0.0119 ns | 0.0093 ns |   6.3592 ns |
|             StrictSixArgAction | 515.0386 ns | 1.3343 ns | 1.2481 ns | 514.7397 ns |
|  ExpressionStrictZeroArgAction |   0.9610 ns | 0.0015 ns | 0.0012 ns |   0.9608 ns |
|   ExpressionStrictOneArgAction |   3.1721 ns | 0.0031 ns | 0.0025 ns |   3.1716 ns |
|   ExpressionStrictTwoArgAction |   5.2637 ns | 0.0068 ns | 0.0064 ns |   5.2608 ns |
| ExpressionStrictThreeArgAction |   7.5400 ns | 0.0085 ns | 0.0076 ns |   7.5382 ns |
|  ExpressionStrictFourArgAction |   9.8659 ns | 0.0105 ns | 0.0093 ns |   9.8632 ns |
|  ExpressionStrictFiveArgAction |  12.1958 ns | 0.0396 ns | 0.0351 ns |  12.1858 ns |
|   ExpressionStrictSixArgAction |  14.2210 ns | 0.0159 ns | 0.0141 ns |  14.2150 ns |

По абсолютам кажется, что не так уж и сильно - счёт идёт уже на наносекунды. Но на деле это полтора-два раза. Зато неоспоримо быстрее, чем вариант с приведениями типов.

Почему так? Ответ кроется где-то в реализации компиляции деревьев выражений. Разбор этого, пожалуй, тема для отдельной статьи.

Зато в последних результатах тестирования виден один важный нюанс: строгая реализация на деревьях выражений работает не только для тех количеств аргументов, которые мы ~~накопипастили~~ сумели обработать, а вообще для методов с произвольным количеством аргументов. Поэтому для самого-самого скоростного решения мы можем использовать strict-вариант с generics для разумно небольшого числа аргументов, а для всех остальных – на основе деревьев выражений.

В общем-то, на этом всё. Ниже приведён график времени вызова функции от количества аргументов для всех изложенных в статье способов (за исключением чистого reflection-based и функции от 6 аргументов – из-за огромной разницы в порядках отличия других значений трудно было бы отличить).

![plot](img/plot.png)

Репозиторий с кодом бенчмарка можно найти [в моём профиле GitHub](https://github.com/alex-ks/cs-function-call-benchmark). Почти боевое применение этих наработок можно найти [в репозитории моего фреймворка для Lego Mindstorms](https://github.com/alex-ks/ev3dev_csharp), но злая ирония в том, что способов запустить их в текущем виде на armel я так и не нашёл. Зато это сподвигло меня повторить эти упражнения на Java/Kotlin - и там есть про что написать ещё одну статью ;)

Спасибо за уделённое время!
