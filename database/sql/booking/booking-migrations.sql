DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'booking') THEN
        CREATE SCHEMA booking;
    END IF;
END $EF$;
CREATE TABLE IF NOT EXISTS booking."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM booking."__EFMigrationsHistory" WHERE "MigrationId" = '20260819182427_InitialBooking') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'booking') THEN
            CREATE SCHEMA booking;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM booking."__EFMigrationsHistory" WHERE "MigrationId" = '20260819182427_InitialBooking') THEN
    CREATE TABLE booking.bookings (
        booking_id uuid NOT NULL,
        customer_id character varying(64) NOT NULL,
        agreement_id character varying(64) NOT NULL,
        origin character varying(256) NOT NULL,
        destination character varying(256) NOT NULL,
        voyage_id character varying(64) NOT NULL,
        status character varying(64) NOT NULL,
        created_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_bookings" PRIMARY KEY (booking_id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM booking."__EFMigrationsHistory" WHERE "MigrationId" = '20260819182427_InitialBooking') THEN
    INSERT INTO booking."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260819182427_InitialBooking', '10.0.11');
    END IF;
END $EF$;
COMMIT;

