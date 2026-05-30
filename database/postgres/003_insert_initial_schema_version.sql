INSERT INTO nfox_schema_version
(version_no, script_name, checksum, status, machine_name, app_version)
SELECT
'2026.05.30.001',
'2026.05.30.001__initial_schema.sql',
NULL,
'SUCCESS',
'INITIAL_SETUP',
'1.0.0'
WHERE NOT EXISTS (
    SELECT 1
    FROM nfox_schema_version
    WHERE version_no = '2026.05.30.001'
      AND script_name = '2026.05.30.001__initial_schema.sql'
      AND status = 'SUCCESS'
);
