using FluentAssertions;
using SaaSCommon.Domain;

namespace SaaSCommon.Domain.Tests;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        var result = Result<int>.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Failure_ShouldCreateFailedResult()
    {
        var error = Error.NotFoundWithDetails("Item not found");
        var result = Result<int>.Failure(error);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public void Value_OnFailedResult_ShouldThrow()
    {
        var result = Result<string>.Failure(Error.Validation);

        Action act = () => _ = result.Value;
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Error_OnSuccessfulResult_ShouldThrow()
    {
        var result = Result<int>.Success(1);

        Action act = () => _ = result.Error;
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Match_OnSuccess_ShouldInvokeOnSuccess()
    {
        var result = Result<string>.Success("hello");
        var value = result.Match(v => v.ToUpper(), _ => "error");

        value.Should().Be("HELLO");
    }

    [Fact]
    public void Match_OnFailure_ShouldInvokeOnFailure()
    {
        var result = Result<string>.Failure(Error.NotFound);
        var value = result.Match(v => v, err => err.Code);

        value.Should().Be("Error.NotFound");
    }

    [Fact]
    public void Bind_OnSuccess_ShouldChain()
    {
        var result = Result<int>.Success(5);
        var next = result.Bind(x => Result<string>.Success((x * 2).ToString()));

        next.IsSuccess.Should().BeTrue();
        next.Value.Should().Be("10");
    }

    [Fact]
    public void Bind_OnFailure_ShouldShortCircuit()
    {
        var error = Error.Conflict;
        var result = Result<int>.Failure(error);
        var next = result.Bind(x => Result<string>.Success(x.ToString()));

        next.IsFailure.Should().BeTrue();
        next.Error.Code.Should().Be(error.Code);
    }

    [Fact]
    public void Map_OnSuccess_ShouldTransform()
    {
        var result = Result<int>.Success(3);
        var mapped = result.Map(x => x * x);

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(9);
    }

    [Fact]
    public void Map_OnFailure_ShouldPropagateError()
    {
        var result = Result<int>.Failure(Error.Unauthorized);
        var mapped = result.Map(x => x * x);

        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Code.Should().Be("Error.Unauthorized");
    }

    [Fact]
    public void ResultStatic_Success_ShouldCreateResultObject()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ResultStatic_Failure_ShouldCreateFailedResult()
    {
        var result = Result.Failure(Error.Validation);

        result.IsFailure.Should().BeTrue();
    }
}
