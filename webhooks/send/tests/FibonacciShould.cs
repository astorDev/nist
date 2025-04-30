using Shouldly;

namespace Nist;

[TestClass]
public sealed class FibonacciAt
{
    [TestMethod] public void Zero() => Fibonacci.At(0).ShouldBe(1);
    [TestMethod] public void One() => Fibonacci.At(1).ShouldBe(1);
    [TestMethod] public void Two() => Fibonacci.At(2).ShouldBe(2);
    [TestMethod] public void Three() => Fibonacci.At(3).ShouldBe(3);
    [TestMethod] public void Four() => Fibonacci.At(4).ShouldBe(5);
    [TestMethod] public void Five() => Fibonacci.At(5).ShouldBe(8);
}
