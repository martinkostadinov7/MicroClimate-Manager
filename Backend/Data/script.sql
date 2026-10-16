IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [ActuatorLogs] (
    [Id] int NOT NULL IDENTITY,
    [DeviceName] nvarchar(max) NOT NULL,
    [State] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ActuatorLogs] PRIMARY KEY ([Id])
);

CREATE TABLE [SensorReadings] (
    [Id] int NOT NULL IDENTITY,
    [Temperature] float NOT NULL,
    [Humidity] float NOT NULL,
    [SoilMoisture] float NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_SensorReadings] PRIMARY KEY ([Id])
);

CREATE TABLE [Settings] (
    [Id] int NOT NULL IDENTITY,
    [SettingName] nvarchar(max) NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_Settings] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261007125335_InitialCreate', N'10.0.12');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Settings] ADD [Applied] bit NOT NULL DEFAULT CAST(0 AS bit);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261009073334_appliedSetting', N'10.0.12');

COMMIT;
GO

