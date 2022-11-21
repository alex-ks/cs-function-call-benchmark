``` ini

BenchmarkDotNet=v0.13.2, OS=macOS Monterey 12.6 (21G115) [Darwin 21.6.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.402
  [Host]     : .NET 6.0.10 (6.0.1022.47605), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 6.0.10 (6.0.1022.47605), Arm64 RyuJIT AdvSIMD


```
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
|            StrictZeroArgAction |   0.6344 ns | 0.0026 ns | 0.0023 ns |   0.6340 ns |
|             StrictOneArgAction |   2.3428 ns | 0.0051 ns | 0.0045 ns |   2.3411 ns |
|             StrictTwoArgAction |   3.1607 ns | 0.0046 ns | 0.0036 ns |   3.1617 ns |
|           StrictThreeArgAction |   4.1434 ns | 0.0046 ns | 0.0039 ns |   4.1432 ns |
|            StrictFourArgAction |   5.0828 ns | 0.0078 ns | 0.0069 ns |   5.0818 ns |
|            StrictFiveArgAction |   6.3591 ns | 0.0119 ns | 0.0093 ns |   6.3592 ns |
|             StrictSixArgAction | 515.0386 ns | 1.3343 ns | 1.2481 ns | 514.7397 ns |
|   ExpressionBasedZeroArgAction |   1.7778 ns | 0.0033 ns | 0.0029 ns |   1.7771 ns |
|    ExpressionBasedOneArgAction |   6.4550 ns | 0.0073 ns | 0.0065 ns |   6.4532 ns |
|    ExpressionBasedTwoArgAction |  10.8472 ns | 0.0514 ns | 0.0481 ns |  10.8501 ns |
|  ExpressionBasedThreeArgAction |  14.4573 ns | 0.1306 ns | 0.1222 ns |  14.4215 ns |
|   ExpressionBasedFourArgAction |  18.7730 ns | 0.0617 ns | 0.0578 ns |  18.7706 ns |
|   ExpressionBasedFiveArgAction |  19.8925 ns | 0.0604 ns | 0.0565 ns |  19.8935 ns |
|    ExpressionBasedSixArgAction | 507.2038 ns | 1.3393 ns | 1.2527 ns | 507.4734 ns |
|  ExpressionStrictZeroArgAction |   0.9610 ns | 0.0015 ns | 0.0012 ns |   0.9608 ns |
|   ExpressionStrictOneArgAction |   3.1721 ns | 0.0031 ns | 0.0025 ns |   3.1716 ns |
|   ExpressionStrictTwoArgAction |   5.2637 ns | 0.0068 ns | 0.0064 ns |   5.2608 ns |
| ExpressionStrictThreeArgAction |   7.5400 ns | 0.0085 ns | 0.0076 ns |   7.5382 ns |
|  ExpressionStrictFourArgAction |   9.8659 ns | 0.0105 ns | 0.0093 ns |   9.8632 ns |
|  ExpressionStrictFiveArgAction |  12.1958 ns | 0.0396 ns | 0.0351 ns |  12.1858 ns |
|   ExpressionStrictSixArgAction |  14.2210 ns | 0.0159 ns | 0.0141 ns |  14.2150 ns |
