using System;
using System.Threading;
using System.Threading.Tasks;

namespace StudyPlatform.Domain.Interfaces;

/// <summary>
/// Abstração para provedores de pagamento.
/// </summary>
public interface IPaymentProvider
{
    Task<bool> SimulateCheckoutAsync(Guid userId, int planId, CancellationToken cancellationToken = default);
}
