using System.Collections.Concurrent;

namespace TaxIntegration.MockTaxApi.Services;

public class IdempotencyStore : ConcurrentDictionary<string, string>;
