# Tax Integration Service

ERP-facing API that receives orders, asynchronously calculates tax via an external Tax API, and guarantees reliability through idempotency, retry with backoff, and crash recovery.

## Stack

- **ASP.NET Core** — Controllers
- **PostgreSQL + Dapper** — raw SQL, no ORM
- **Channel\<T\>** — in-process event queue
- **.NET 10**

## Running locally

**1. Start Postgres:**
```bash
docker compose up -d
```

**2. Initialize database:**
```powershell
Get-Content TaxIntegration.Api/scripts/init.sql | docker exec -i taxintegration-postgres-1 psql -U dev -d taxintegration
```

**3. Add local config** — create `TaxIntegration.Api/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=taxintegration;Username=dev;Password=dev"
  },
  "TaxApi": {
    "BaseUrl": "http://localhost:5209"
  }
}
```

**4. Run Mock Tax API** (terminal 1):
```bash
dotnet run --project TaxIntegration.MockTaxApi
```

**5. Run API** (terminal 2):
```bash
dotnet run --project TaxIntegration.Api
```

## Endpoints

| Method | Path | Description |
|--------|------|-------------|
| POST | `/erp/orders` | Submit order for tax calculation |
| GET | `/erp/orders/{id}` | Get order with current status |
| GET | `/erp/orders/{id}/tax` | Get tax calculation details |

## Example

```bash
# Create order
curl -X POST http://localhost:5037/erp/orders \
  -H "Content-Type: application/json" \
  -H "Idempotency-Key: order-001" \
  -d '{"externalOrderId":"ORD-001","customerCountry":"HR","totalAmount":10000,"currency":"EUR"}'

# Check status
curl http://localhost:5037/erp/orders/{id}

# Get tax details (available after worker processes the order)
curl http://localhost:5037/erp/orders/{id}/tax
```

## How it works

1. `POST /erp/orders` writes the order and an integration event to the DB, then enqueues the event ID into an in-process `Channel<Guid>`
2. `TaxCalculationWorker` reads from the channel and calls the Tax API
3. On success — order status moves to `TaxCalculated`, tax details saved
4. On transient failure — retried up to 3 times with exponential backoff (1s, 2s, 4s)
5. On startup — `RecoveryWorker` re-enqueues any events left in `Pending` or `Processing` state from a previous crash

## Order statuses

| Status | Meaning |
|--------|---------|
| `PendingTax` | Order received, waiting for tax calculation |
| `TaxCalculated` | Tax successfully calculated |
| `Failed` | Tax calculation failed after all retries |
