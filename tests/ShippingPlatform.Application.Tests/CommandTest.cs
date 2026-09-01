using MediatR;
using ShippingPlatform.Application.Tests.Fakes;

namespace ShippingPlatform.Application.Tests;

public class CommandTest
{
    [Fact]
    public async Task Normal_command_should_not_start_transaction()
    {
        var fakeTransactionManager = new FakeTransactionManager();
        var transactionBehavior = new TransactionBehavior<TestCommand, string>(fakeTransactionManager);
        
        var command = new TestCommand();
        var handlerWasExecuted = false;
        RequestHandlerDelegate<string> next = _ =>
        {
            handlerWasExecuted = true;
            return Task.FromResult("OK");
        };

        var result = await transactionBehavior.Handle(command, next, CancellationToken.None);
        
        Assert.False(fakeTransactionManager.WasExecuted);
        Assert.True(handlerWasExecuted);
        Assert.Equal("OK", result);
    }
}