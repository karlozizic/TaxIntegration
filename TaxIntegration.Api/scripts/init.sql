DROP TABLE IF EXISTS tax_calculations;
DROP TABLE IF EXISTS integration_events;
DROP TABLE IF EXISTS orders;

CREATE TABLE orders (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    external_order_id VARCHAR(100) NOT NULL UNIQUE,
    customer_country VARCHAR(2) NOT NULL,
    total_amount BIGINT NOT NULL,
    currency VARCHAR(3) NOT NULL DEFAULT 'EUR',
    status VARCHAR(20) NOT NULL DEFAULT 'PendingTax',
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE tax_calculations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID NOT NULL REFERENCES orders(id),
    tax_rate DECIMAL(5,2) NOT NULL,
    tax_amount BIGINT NOT NULL,
    tax_provider_reference VARCHAR(200),
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE integration_events (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID NOT NULL REFERENCES orders(id),
    event_type VARCHAR(50) NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'Pending',
    retry_count INT NOT NULL DEFAULT 0,
    max_retries INT NOT NULL DEFAULT 3,
    last_attempt_at TIMESTAMPTZ,
    error_message TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE idempotency_keys (
    key VARCHAR(200) PRIMARY KEY,
    order_id UUID NOT NULL,
    response_body JSONB NOT NULL,
    response_status INT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);