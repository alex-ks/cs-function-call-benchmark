``` ini

BenchmarkDotNet=v0.12.1, OS=macOS Mojave 10.14.6 (18G4032) [Darwin 18.7.0]
Intel Core i7-4870HQ CPU 2.50GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=3.1.101
  [Host]     : .NET Core 3.1.1 (CoreCLR 4.700.19.60701, CoreFX 4.700.19.60801), X64 RyuJIT
  DefaultJob : .NET Core 3.1.1 (CoreCLR 4.700.19.60701, CoreFX 4.700.19.60801), X64 RyuJIT


```
|                         Method |        Mean |      Error |     StdDev |      Median |
|------------------------------- |------------:|-----------:|-----------:|------------:|
|             RegularZeroArgCall |   0.0020 ns |  0.0058 ns |  0.0048 ns |   0.0000 ns |
|              RegularOneArgCall |   0.0026 ns |  0.0068 ns |  0.0061 ns |   0.0000 ns |
|              RegularTwoArgCall |   0.0189 ns |  0.0102 ns |  0.0090 ns |   0.0184 ns |
|            RegularThreeArgCall |   0.0159 ns |  0.0042 ns |  0.0037 ns |   0.0166 ns |
|             RegularFourArgCall |   0.0019 ns |  0.0033 ns |  0.0028 ns |   0.0000 ns |
|             RegularFiveArgCall |   0.0012 ns |  0.0031 ns |  0.0026 ns |   0.0000 ns |
|              RegularSixArgCall |   0.2742 ns |  0.0105 ns |  0.0088 ns |   0.2752 ns |
|   ReflectionBasedZeroArgAction | 112.7083 ns |  0.7415 ns |  0.5789 ns | 112.7340 ns |
|    ReflectionBasedOneArgAction | 192.6494 ns |  1.8164 ns |  1.6990 ns | 192.1628 ns |
|    ReflectionBasedTwoArgAction | 258.7588 ns |  3.2811 ns |  3.0692 ns | 257.9058 ns |
|  ReflectionBasedThreeArgAction | 322.0511 ns |  3.9985 ns |  3.5445 ns | 322.5660 ns |
|   ReflectionBasedFourArgAction | 400.9057 ns |  3.4695 ns |  3.2454 ns | 400.1694 ns |
|   ReflectionBasedFiveArgAction | 458.1926 ns |  3.4574 ns |  3.0649 ns | 457.5324 ns |
|    ReflectionBasedSixArgAction | 549.6316 ns |  4.5302 ns |  3.7829 ns | 549.4001 ns |
|      GenericBasedZeroArgAction |   1.6626 ns |  0.0132 ns |  0.0110 ns |   1.6637 ns |
|       GenericBasedOneArgAction |   9.7318 ns |  0.1797 ns |  0.1593 ns |   9.6726 ns |
|       GenericBasedTwoArgAction |  17.6455 ns |  0.1385 ns |  0.1228 ns |  17.6031 ns |
|     GenericBasedThreeArgAction |  23.1999 ns |  0.3934 ns |  0.3680 ns |  23.0874 ns |
|      GenericBasedFourArgAction |  32.1915 ns |  0.2011 ns |  0.1783 ns |  32.2297 ns |
|      GenericBasedFiveArgAction |  39.1221 ns |  0.5112 ns |  0.4269 ns |  38.9700 ns |
|       GenericBasedSixArgAction | 657.2011 ns |  5.5504 ns |  4.9202 ns | 655.3558 ns |
|            StrictZeroArgAction |   0.8455 ns |  0.0071 ns |  0.0063 ns |   0.8450 ns |
|             StrictOneArgAction |   3.6826 ns |  0.0478 ns |  0.0447 ns |   3.6674 ns |
|             StrictTwoArgAction |   5.5219 ns |  0.0446 ns |  0.0395 ns |   5.5149 ns |
|           StrictThreeArgAction |   7.3941 ns |  0.0434 ns |  0.0363 ns |   7.3792 ns |
|            StrictFourArgAction |   8.4877 ns |  0.0455 ns |  0.0403 ns |   8.4674 ns |
|            StrictFiveArgAction |  10.0094 ns |  0.1404 ns |  0.1313 ns |   9.9487 ns |
|             StrictSixArgAction | 706.1372 ns | 13.6295 ns | 12.7490 ns | 702.4068 ns |
|   ExpressionBasedZeroArgAction |   2.0401 ns |  0.0432 ns |  0.0404 ns |   2.0329 ns |
|    ExpressionBasedOneArgAction |   9.4030 ns |  0.1680 ns |  0.1403 ns |   9.3999 ns |
|    ExpressionBasedTwoArgAction |  16.3554 ns |  0.2737 ns |  0.2286 ns |  16.3293 ns |
|  ExpressionBasedThreeArgAction |  23.5963 ns |  0.2666 ns |  0.2364 ns |  23.5592 ns |
|   ExpressionBasedFourArgAction |  32.1926 ns |  0.6026 ns |  0.8044 ns |  32.2249 ns |
|   ExpressionBasedFiveArgAction |  34.2504 ns |  0.2791 ns |  0.2475 ns |  34.1452 ns |
|    ExpressionBasedSixArgAction | 659.5997 ns |  4.7805 ns |  4.2378 ns | 658.8155 ns |
|  ExpressionStrictZeroArgAction |   1.3880 ns |  0.0182 ns |  0.0152 ns |   1.3865 ns |
|   ExpressionStrictOneArgAction |   7.0025 ns |  0.1681 ns |  0.2411 ns |   6.8861 ns |
|   ExpressionStrictTwoArgAction |  10.9824 ns |  0.0870 ns |  0.0772 ns |  10.9667 ns |
| ExpressionStrictThreeArgAction |  16.8135 ns |  0.0993 ns |  0.0830 ns |  16.8004 ns |
|  ExpressionStrictFourArgAction |  21.0620 ns |  0.4489 ns |  0.6293 ns |  21.3380 ns |
|  ExpressionStrictFiveArgAction |  26.0780 ns |  0.1807 ns |  0.1602 ns |  26.0514 ns |
|   ExpressionStrictSixArgAction |  31.6301 ns |  0.1896 ns |  0.1680 ns |  31.6275 ns |
