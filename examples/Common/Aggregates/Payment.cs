namespace Common.Aggregates;

using Commands;
using Entities;
using Events;
using Marty.Net.Aggregates.Contracts;
using Marty.Net.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

public class Payment : Aggregate
{
    private readonly Lazy<List<Refund>> _refunds = new();

    public string CardNumber { get; private set; } = null!;

    public string Cvv { get; private set; } = null!;

    public PaymentStatus? Status { get; private set; }

    public int IntentAmount { get; internal set; }

    public int CapturedAmount { get; internal set; }

    public int RefundedAmount { get; internal set; }

    public bool InRefundedStatus =>
        Status is PaymentStatus.Refunded or PaymentStatus.PartiallyRefunded;

    public bool InCapturedStatus =>
        Status
            is PaymentStatus.Captured
                or PaymentStatus.PartiallyCaptured
                or PaymentStatus.PartiallyRefunded;

    public Refund[] Refunds => _refunds.Value.ToArray();

    public void RequestPayment(RequestPayment command)
    {
        IEvent @event = new PaymentRequested
        {
            Id = Guid.NewGuid().ToString(),
            CardNumber = command.CardNumber,
            Cvv = command.Cvv,
            Amount = command.Amount,
            Timestamp = DateTimeOffset.UtcNow
        };
        ApplyChange(@event);
    }

    public void RequestRefund(RequestPaymentRefund command)
    {
        RefundRequested @event =
            new()
            {
                RefundId = Guid.NewGuid().ToString(),
                PaymentId = command.PaymentId,
                Timestamp = DateTimeOffset.UtcNow,
                Amount = command.Amount ?? IntentAmount
            };

        PaymentStatus? status = Status;
        ApplyChange(@event);
        IEvent? followUpEvent = status switch
        {
            null
                => new RefundDeclined
                {
                    RefundId = @event.RefundId,
                    PaymentId = command.PaymentId,
                    Timestamp = DateTimeOffset.UtcNow,
                    Reason = $"Cannot find payment with id {command.PaymentId}"
                },
            PaymentStatus.RefundInitiated
                => new RefundRequestedRepeated
                {
                    RefundId = @event.RefundId,
                    PaymentId = command.PaymentId,
                    Timestamp = DateTimeOffset.UtcNow,
                    Amount = command.Amount ?? IntentAmount,
                    Message = command.Message
                },
            PaymentStatus.Captured when command.Amount.HasValue && IntentAmount < command.Amount
                => new RefundDeclined
                {
                    RefundId = @event.RefundId,
                    PaymentId = command.PaymentId,
                    Timestamp = DateTimeOffset.UtcNow,
                    Reason =
                        $"Cannot refund a higher amount {command.Amount} than the payment {IntentAmount}"
                },
            PaymentStatus.Captured => null,
            _
                => new RefundDeclined
                {
                    RefundId = @event.RefundId,
                    PaymentId = command.PaymentId,
                    Timestamp = DateTimeOffset.UtcNow,
                    Reason = $"Cannot refund a payment in Status {status}"
                }
        };
        if (followUpEvent is not null)
        {
            ApplyChange(followUpEvent);
        }
    }

    public void CapturePayment(CapturePayment command)
    {
        IEvent @event = new PaymentCaptured
        {
            Timestamp = DateTimeOffset.UtcNow,
            Amount = command.Amount ?? IntentAmount
        };
        ApplyChange(@event);
    }

    public void DeclinePayment(DeclinePayment command)
    {
        IEvent @event = new PaymentDeclined
        {
            PaymentId = command.PaymentId,
            Timestamp = DateTimeOffset.UtcNow,
            Reason = command.Reason
        };
        ApplyChange(@event);
    }

    public void DeclineRefund(DeclineRefund command)
    {
        IEvent @event = new RefundDeclined
        {
            RefundId = command.RefundId,
            TransactionId = command.TransactionId,
            OriginalTransactionId = command.OriginalTransactionId,
            PaymentId = command.PaymentId,
            Timestamp = DateTimeOffset.UtcNow,
            Reason = command.Reason
        };
        ApplyChange(@event);
    }

    public void AcceptRefund(AcceptRefund command)
    {
        IEvent @event = new RefundActionAccepted
        {
            TransactionId = command.TransactionId,
            Timestamp = DateTimeOffset.UtcNow
        };
        ApplyChange(@event);
    }

    public void CompleteRefund(CompleteRefund command)
    {
        IEvent @event = new RefundActionCompleted
        {
            OriginalTransactionId = command.OriginalTransactionId,
            TransactionId = command.TransactionId,
            Timestamp = DateTimeOffset.UtcNow,
            Amount = command.Amount
        };
        ApplyChange(@event);
    }

    private void Apply(PaymentRequested @event)
    {
        CardNumber = @event.CardNumber;
        Cvv = @event.Cvv;
        IntentAmount = @event.Amount;
        Status = PaymentStatus.Requested;
    }

    private void Apply(PaymentCaptured @event)
    {
        Status = PaymentStatus.Captured;
    }

    private void Apply(PaymentRefunded @event)
    {
        Status = PaymentStatus.PartiallyRefunded;
    }

    private void Apply(RefundRequested @event)
    {
        List<Refund> refunds = _refunds.Value;
        refunds.Add(new Refund(@event.RefundId, @event.PaymentId));
        Status = PaymentStatus.Requested;
    }

    private void Apply(RefundDeclined @event)
    {
        List<Refund> refunds = _refunds.Value;
        Refund? refund = refunds.FirstOrDefault(x =>
            x.TransactionId == @event.OriginalTransactionId
        );
        if (refund is null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(@event.TransactionId))
        {
            refund.SetTransactionId(@event.TransactionId!);
        }

        Status = PaymentStatus.RefundDeclined;
    }

    private void Apply(RefundActionAccepted @event)
    {
        List<Refund> refunds = _refunds.Value;
        Refund? refund = refunds.LastOrDefault();
        if (refund is null)
        {
            return;
        }

        refund.SetTransactionId(@event.TransactionId!);
        Status = PaymentStatus.RefundAccepted;
    }

    private void Apply(RefundActionCompleted @event)
    {
        List<Refund> refunds = _refunds.Value;
        Refund? refund = refunds.FirstOrDefault(x =>
            x.TransactionId == @event.OriginalTransactionId
        );
        if (refund is null)
        {
            return;
        }

        RefundedAmount += @event.Amount;
        Status =
            RefundedAmount == CapturedAmount
                ? PaymentStatus.Refunded
                : PaymentStatus.PartiallyRefunded;
    }
}
