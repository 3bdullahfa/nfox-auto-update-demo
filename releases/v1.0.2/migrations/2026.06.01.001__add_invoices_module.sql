CREATE TABLE IF NOT EXISTS invoices (
    id INT PRIMARY KEY,
    customer_id INT NOT NULL,
    invoice_no VARCHAR(50) NOT NULL,
    invoice_date DATE NOT NULL,
    total_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
    notes TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

ALTER TABLE customers
ADD COLUMN IF NOT EXISTS customer_category VARCHAR(50);

UPDATE customers
SET customer_category = 'Regular'
WHERE customer_category IS NULL;

INSERT INTO invoices (id, customer_id, invoice_no, invoice_date, total_amount, notes)
VALUES
(1, 1, 'INV-1001', CURRENT_DATE - INTERVAL '10 days', 1500, 'Demo invoice 1'),
(2, 2, 'INV-1002', CURRENT_DATE - INTERVAL '5 days', 2750, 'Demo invoice 2'),
(3, 1, 'INV-1003', CURRENT_DATE, 900, 'Demo invoice 3')
ON CONFLICT (id) DO NOTHING;
