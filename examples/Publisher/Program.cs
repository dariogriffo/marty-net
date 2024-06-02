using System;
using System.Text.Json;
using Common.Aggregates;
using Common.Commands;
using Marty.Net;
using Marty.Net.Aggregates.Contracts;
using Marty.Net.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Publisher;

ServiceCollection services = new();

services
    .ConfigureEventStoreDbWithLogging()
    .AddMarty(sp => sp.GetEventStoreSettings())
    .AddMartyAggregates();

ServiceProvider provider = services.BuildServiceProvider();
IAggregateStore store = provider.GetRequiredService<IAggregateStore>();
while (true)
{
    Payment payment = new();
    payment.RequestPayment(new RequestPayment("4111111111111111", "123", 100));
    await payment.Create(store);
    payment.CapturePayment(new CapturePayment());
    await payment.Update(store);

    payment.RequestRefund(new RequestPaymentRefund());
    await payment.Update(store);

    string refundTransactionId = Guid.NewGuid().ToString();
    payment.AcceptRefund(new AcceptRefund { TransactionId = refundTransactionId });
    await payment.Update(store);

    Payment payment2 = await store.GetAggregateById<Payment>(payment.Id);
    Console.WriteLine(JsonSerializer.Serialize(payment2));
}
