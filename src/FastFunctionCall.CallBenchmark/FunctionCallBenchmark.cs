using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Attributes;

namespace FastFunctionCall.CallBenchmark
{
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
        private Action _reflectionBasedOneArgAction;
        private Action _reflectionBasedTwoArgAction;
        private Action _reflectionBasedThreeArgAction;
        private Action _reflectionBasedFourArgAction;
        private Action _reflectionBasedFiveArgAction;
        private Action _reflectionBasedSixArgAction;

        // Generic-based:
        private Action _genericBasedZeroArgAction;
        private Action _genericBasedOneArgAction;
        private Action _genericBasedTwoArgAction;
        private Action _genericBasedThreeArgAction;
        private Action _genericBasedFourArgAction;
        private Action _genericBasedFiveArgAction;
        private Action _genericBasedSixArgAction;

        // Strict:
        private Action _strictZeroArgAction;
        private Action _strictOneArgAction;
        private Action _strictTwoArgAction;
        private Action _strictThreeArgAction;
        private Action _strictFourArgAction;
        private Action _strictFiveArgAction;
        private Action _strictSixArgAction;

        // Expression-based:
        private Action _expressionBasedZeroArgAction;
        private Action _expressionBasedOneArgAction;
        private Action _expressionBasedTwoArgAction;
        private Action _expressionBasedThreeArgAction;
        private Action _expressionBasedFourArgAction;
        private Action _expressionBasedFiveArgAction;
        private Action _expressionBasedSixArgAction;

        // Expression-strict:
        private Action _expressionStrictZeroArgAction;
        private Action _expressionStrictOneArgAction;
        private Action _expressionStrictTwoArgAction;
        private Action _expressionStrictThreeArgAction;
        private Action _expressionStrictFourArgAction;
        private Action _expressionStrictFiveArgAction;
        private Action _expressionStrictSixArgAction;

        // A little help to the compiler to deduce lambda types.
        private Func<T> Get<T>(Func<T> getter)
        {
            return getter;
        }

        public FunctionCallBenchmark()
        {
            var objectGetters =
                    new Func<object>[] { () => Arg1, () => Arg2, () => Arg3, () => Arg4, () => Arg5, () => Arg6 };
            var strictGetters =
                    new Delegate[] { Get(() => Arg1), Get(() => Arg2), Get(() => Arg3),
                                     Get(() => Arg4), Get(() => Arg5), Get(() => Arg6) };

            // Reflection-based:
            _reflectionBasedZeroArgAction = GenerateReflectionBasedAction(nameof(ZeroArgMethod), objectGetters.Take(0));
            _reflectionBasedOneArgAction = GenerateReflectionBasedAction(nameof(OneArgMethod), objectGetters.Take(1));
            _reflectionBasedTwoArgAction = GenerateReflectionBasedAction(nameof(TwoArgMethod), objectGetters.Take(2));
            _reflectionBasedThreeArgAction = GenerateReflectionBasedAction(nameof(ThreeArgMethod), objectGetters.Take(3));
            _reflectionBasedFourArgAction = GenerateReflectionBasedAction(nameof(FourArgMethod), objectGetters.Take(4));
            _reflectionBasedFiveArgAction = GenerateReflectionBasedAction(nameof(FiveArgMethod), objectGetters.Take(5));
            _reflectionBasedSixArgAction = GenerateReflectionBasedAction(nameof(SixArgMethod), objectGetters);

            // Generic-based:
            _genericBasedZeroArgAction = GenerateGenericBasedAction(nameof(ZeroArgMethod), objectGetters.Take(0));
            _genericBasedOneArgAction = GenerateGenericBasedAction(nameof(OneArgMethod), objectGetters.Take(1));
            _genericBasedTwoArgAction = GenerateGenericBasedAction(nameof(TwoArgMethod), objectGetters.Take(2));
            _genericBasedThreeArgAction = GenerateGenericBasedAction(nameof(ThreeArgMethod), objectGetters.Take(3));
            _genericBasedFourArgAction = GenerateGenericBasedAction(nameof(FourArgMethod), objectGetters.Take(4));
            _genericBasedFiveArgAction = GenerateGenericBasedAction(nameof(FiveArgMethod), objectGetters.Take(5));
            _genericBasedSixArgAction = GenerateGenericBasedAction(nameof(SixArgMethod), objectGetters);

            // Strict:
            _strictZeroArgAction = GenerateStrictAction(nameof(ZeroArgMethod), strictGetters.Take(0));
            _strictOneArgAction = GenerateStrictAction(nameof(OneArgMethod), strictGetters.Take(1));
            _strictTwoArgAction = GenerateStrictAction(nameof(TwoArgMethod), strictGetters.Take(2));
            _strictThreeArgAction = GenerateStrictAction(nameof(ThreeArgMethod), strictGetters.Take(3));
            _strictFourArgAction = GenerateStrictAction(nameof(FourArgMethod), strictGetters.Take(4));
            _strictFiveArgAction = GenerateStrictAction(nameof(FiveArgMethod), strictGetters.Take(5));
            _strictSixArgAction = GenerateStrictAction(nameof(SixArgMethod), strictGetters);

            // Expression-based:
            _expressionBasedZeroArgAction = GenerateExpressionBasedAction(nameof(ZeroArgMethod), objectGetters.Take(0));
            _expressionBasedOneArgAction = GenerateExpressionBasedAction(nameof(OneArgMethod), objectGetters.Take(1));
            _expressionBasedTwoArgAction = GenerateExpressionBasedAction(nameof(TwoArgMethod), objectGetters.Take(2));
            _expressionBasedThreeArgAction = GenerateExpressionBasedAction(nameof(ThreeArgMethod), objectGetters.Take(3));
            _expressionBasedFourArgAction = GenerateExpressionBasedAction(nameof(FourArgMethod), objectGetters.Take(4));
            _expressionBasedFiveArgAction = GenerateExpressionBasedAction(nameof(FiveArgMethod), objectGetters.Take(5));
            _expressionBasedSixArgAction = GenerateExpressionBasedAction(nameof(SixArgMethod), objectGetters);

            // Expression-strict:
            _expressionStrictZeroArgAction = GenerateExpressionStrictAction(nameof(ZeroArgMethod), strictGetters.Take(0));
            _expressionStrictOneArgAction = GenerateExpressionStrictAction(nameof(OneArgMethod), strictGetters.Take(1));
            _expressionStrictTwoArgAction = GenerateExpressionStrictAction(nameof(TwoArgMethod), strictGetters.Take(2));
            _expressionStrictThreeArgAction = GenerateExpressionStrictAction(nameof(ThreeArgMethod), strictGetters.Take(3));
            _expressionStrictFourArgAction = GenerateExpressionStrictAction(nameof(FourArgMethod), strictGetters.Take(4));
            _expressionStrictFiveArgAction = GenerateExpressionStrictAction(nameof(FiveArgMethod), strictGetters.Take(5));
            _expressionStrictSixArgAction = GenerateExpressionStrictAction(nameof(SixArgMethod), strictGetters);
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

        private Action GenerateGenericBasedAction(string methodName, IEnumerable<Func<object>> parameters)
        {
            var method = this.GetType().GetMethod(methodName,
                                                  BindingFlags.Public | BindingFlags.Instance);
            if (method == null)
                throw new ArgumentNullException(methodName);
            return GenericBasedActionGenerator.ExtractAction(this, method, parameters.ToArray());
        }

        private Action GenerateStrictAction(string methodName, IEnumerable<Delegate> parameters)
        {
            var method = this.GetType().GetMethod(methodName,
                                                  BindingFlags.Public | BindingFlags.Instance);
            if (method == null)
                throw new ArgumentNullException(methodName);
            return NoCastActionGenerator.ExtractAction(this, method, parameters.ToArray());
        }

        private Action GenerateExpressionBasedAction(string methodName, IEnumerable<Func<object>> parameters)
        {
            var method = this.GetType().GetMethod(methodName,
                                                  BindingFlags.Public | BindingFlags.Instance);
            if (method == null)
                throw new ArgumentNullException(methodName);
            return ExpressionBasedActionGenerator.ExtractAction(this, method, parameters.ToArray());
        }

        private Action GenerateExpressionStrictAction(string methodName, IEnumerable<Delegate> parameters)
        {
            var method = this.GetType().GetMethod(methodName,
                                                  BindingFlags.Public | BindingFlags.Instance);
            if (method == null)
                throw new ArgumentNullException(methodName);
            return ExpressionBasedStrictGenerator.ExtractAction(this, method, parameters.ToArray());
        }

        // Regular calls:
        [Benchmark]
        public void RegularZeroArgCall() => ZeroArgMethod();

        [Benchmark]
        public void RegularOneArgCall() => OneArgMethod(Arg1);

        [Benchmark]
        public void RegularTwoArgCall() => TwoArgMethod(Arg1, Arg2);

        [Benchmark]
        public void RegularThreeArgCall() => ThreeArgMethod(Arg1, Arg2, Arg3);

        [Benchmark]
        public void RegularFourArgCall() => FourArgMethod(Arg1, Arg2, Arg3, Arg4);

        [Benchmark]
        public void RegularFiveArgCall() => FiveArgMethod(Arg1, Arg2, Arg3, Arg4, Arg5);

        [Benchmark]
        public void RegularSixArgCall() => SixArgMethod(Arg1, Arg2, Arg3, Arg4, Arg5, Arg6);

        // Reflection-based calls:
        [Benchmark]
        public void ReflectionBasedZeroArgAction() => _reflectionBasedZeroArgAction();

        [Benchmark]
        public void ReflectionBasedOneArgAction() => _reflectionBasedOneArgAction();

        [Benchmark]
        public void ReflectionBasedTwoArgAction() => _reflectionBasedTwoArgAction();

        [Benchmark]
        public void ReflectionBasedThreeArgAction() => _reflectionBasedThreeArgAction();

        [Benchmark]
        public void ReflectionBasedFourArgAction() => _reflectionBasedFourArgAction();

        [Benchmark]
        public void ReflectionBasedFiveArgAction() => _reflectionBasedFiveArgAction();

        [Benchmark]
        public void ReflectionBasedSixArgAction() => _reflectionBasedSixArgAction();

        // Generic-based calls:
        [Benchmark]
        public void GenericBasedZeroArgAction() => _genericBasedZeroArgAction();

        [Benchmark]
        public void GenericBasedOneArgAction() => _genericBasedOneArgAction();

        [Benchmark]
        public void GenericBasedTwoArgAction() => _genericBasedTwoArgAction();

        [Benchmark]
        public void GenericBasedThreeArgAction() => _genericBasedThreeArgAction();

        [Benchmark]
        public void GenericBasedFourArgAction() => _genericBasedFourArgAction();

        [Benchmark]
        public void GenericBasedFiveArgAction() => _genericBasedFiveArgAction();

        [Benchmark]
        public void GenericBasedSixArgAction() => _genericBasedSixArgAction();

        // Strict calls:
        [Benchmark]
        public void StrictZeroArgAction() => _strictZeroArgAction();

        [Benchmark]
        public void StrictOneArgAction() => _strictOneArgAction();

        [Benchmark]
        public void StrictTwoArgAction() => _strictTwoArgAction();

        [Benchmark]
        public void StrictThreeArgAction() => _strictThreeArgAction();

        [Benchmark]
        public void StrictFourArgAction() => _strictFourArgAction();

        [Benchmark]
        public void StrictFiveArgAction() => _strictFiveArgAction();

        [Benchmark]
        public void StrictSixArgAction() => _strictSixArgAction();

        // Expression-based calls:
        [Benchmark]
        public void ExpressionBasedZeroArgAction() => _expressionBasedZeroArgAction();

        [Benchmark]
        public void ExpressionBasedOneArgAction() => _expressionBasedOneArgAction();

        [Benchmark]
        public void ExpressionBasedTwoArgAction() => _expressionBasedTwoArgAction();

        [Benchmark]
        public void ExpressionBasedThreeArgAction() => _expressionBasedThreeArgAction();

        [Benchmark]
        public void ExpressionBasedFourArgAction() => _expressionBasedFourArgAction();

        [Benchmark]
        public void ExpressionBasedFiveArgAction() => _expressionBasedFiveArgAction();

        [Benchmark]
        public void ExpressionBasedSixArgAction() => _expressionBasedSixArgAction();

        // Expression-strict calls:
        [Benchmark]
        public void ExpressionStrictZeroArgAction() => _expressionStrictZeroArgAction();

        [Benchmark]
        public void ExpressionStrictOneArgAction() => _expressionStrictOneArgAction();

        [Benchmark]
        public void ExpressionStrictTwoArgAction() => _expressionStrictTwoArgAction();

        [Benchmark]
        public void ExpressionStrictThreeArgAction() => _expressionStrictThreeArgAction();

        [Benchmark]
        public void ExpressionStrictFourArgAction() => _expressionStrictFourArgAction();

        [Benchmark]
        public void ExpressionStrictFiveArgAction() => _expressionStrictFiveArgAction();

        [Benchmark]
        public void ExpressionStrictSixArgAction() => _expressionStrictSixArgAction();
    }
}