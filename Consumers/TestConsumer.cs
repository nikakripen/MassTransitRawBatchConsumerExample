namespace Company.Consumers
{
    using System.Threading.Tasks;
    using MassTransit;
    using Contracts;
    using Microsoft.Extensions.Logging;

    public class TestConsumer :
        IConsumer<Batch<TestContract>>
    {
        readonly ILogger<TestConsumer> _logger;
        public TestConsumer(ILogger<TestConsumer> logger)
        {
            _logger = logger;
        }
        public Task Consume(ConsumeContext<Batch<TestContract>> context)
        {
            for (int i = 0; i < context.Message.Length; i++)
            {
                ConsumeContext<TestContract> message = context.Message[i];

                _logger.LogInformation("Received Text: {Text}", message.Message.Text);
            }
            return Task.CompletedTask;
        }
    }
}