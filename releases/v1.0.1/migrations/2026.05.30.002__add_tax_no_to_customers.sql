ALTER TABLE customers
ADD COLUMN IF NOT EXISTS tax_no VARCHAR(50);

UPDATE customers
SET tax_no = 'TAX-' || id
WHERE tax_no IS NULL;
