using MediatR;
using ShippingPlatform.Application.Tests.Fakes;

namespace ShippingPlatform.Application.Tests;

public class TransactionalCommandTest
{
    [Fact]
    public async void Transactional_command_should_execute_inside_transaction()
    {
        var fakeTransactionManager = new FakeTransactionManager();
        var transactionBehavior = new TransactionBehavior<TestTransactionalCommand, string>(fakeTransactionManager);
        
        var command = new TestTransactionalCommand();
        var handleWasExecuted = false;

        RequestHandlerDelegate<string> next = _ =>
        {
            handleWasExecuted = true;
            return Task.FromResult("OK");
        };

        var handleResult = await transactionBehavior.Handle(command, next, CancellationToken.None);
        
        
        Assert.True(fakeTransactionManager.WasExecuted);
        Assert.True(handleWasExecuted);
        Assert.Equal("OK", handleResult);
    }
    
    
}