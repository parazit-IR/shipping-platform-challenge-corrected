DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'commercial') THEN
        CREATE SCHEMA commercial;
    END IF;
END $EF$;
CREATE TABLE IF NOT EXISTS commercial."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM commercial."__EFMigrationsHistory" WHERE "MigrationId" = '20260819203952_InitialCommercial') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'commercial') THEN
            CREATE SCHEMA commercial;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM commercial."__EFMigrationsHistory" WHERE "MigrationId" = '20260819203952_InitialCommercial') THEN
    CREATE TABLE commercial.customers (
        customer_id character varying(64) NOT NULL,
        CONSTRAINT "PK_customers" PRIMARY KEY (customer_id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM commercial."__EFMigrationsHistory" WHERE "MigrationId" = '20260819203952_InitialCommercial') THEN
    CREATE TABLE commercial.agreements (
        agreement_id character varying(64) NOT NULL,
        customer_id character varying(64) NOT NULL,
        status character varying(32) NOT NULL,
        CONSTRAINT "PK_agreements" PRIMARY KEY (agreement_id),
        CONSTRAINT "FK_agreements_customers_customer_id" FOREIGN KEY (customer_id) REFERENCES commercial.customers (customer_id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM commercial."__EFMigrationsHistory" WHERE "MigrationId" = '20260819203952_InitialCommercial') THEN
    CREATE INDEX "IX_agreements_customer_id" ON commercial.agreements (customer_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM commercial."__EFMigrationsHistory" WHERE "MigrationId" = '20260819203952_InitialCommercial') THEN
    INSERT INTO commercial."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260819203952_InitialCommercial', '10.0.11');
    END IF;
END $EF$;
COMMIT;

