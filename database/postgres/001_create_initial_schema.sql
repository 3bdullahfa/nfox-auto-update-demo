CREATE TABLE IF NOT EXISTS customers (
    id INT PRIMARY KEY,
    customer_name VARCHAR(200) NOT NULL,
    balance NUMERIC(18,2) NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS nfox_schema_version (
    id BIGSERIAL PRIMARY KEY,
    version_no VARCHAR(50) NOT NULL,
    script_name VARCHAR(300) NOT NULL,
    checksum VARCHAR(100),
    applied_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(20) NOT NULL,
    error_message TEXT,
    machine_name VARCHAR(200),
    app_version VARCHAR(50)
);

CREATE TABLE IF NOT EXISTS nfox_update_lock (
    lock_id INT PRIMARY KEY,
    machine_name VARCHAR(200),
    locked_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);
