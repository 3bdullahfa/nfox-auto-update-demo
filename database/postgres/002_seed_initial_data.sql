INSERT INTO customers (id, customer_name, balance)
VALUES
(1, 'Customer 1', 1500),
(2, 'Customer 2', 2750),
(3, 'Customer 3', 900)
ON CONFLICT (id) DO NOTHING;
