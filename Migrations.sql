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
GO

CREATE TABLE [Clients] (
    [ClientId] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Email] nvarchar(max) NOT NULL,
    [Phone] nvarchar(max) NOT NULL,
    [Region] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Clients] PRIMARY KEY ([ClientId])
);
GO

CREATE TABLE [Contracts] (
    [ContractId] int NOT NULL IDENTITY,
    [ClientId] int NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [Status] int NOT NULL,
    [ServiceLevel] int NOT NULL,
    [SignedAgreementPath] nvarchar(500) NULL,
    CONSTRAINT [PK_Contracts] PRIMARY KEY ([ContractId]),
    CONSTRAINT [FK_Contracts_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([ClientId]) ON DELETE CASCADE
);
GO

CREATE TABLE [ServiceRequests] (
    [ServiceRequestId] int NOT NULL IDENTITY,
    [ContractId] int NOT NULL,
    [Description] nvarchar(500) NOT NULL,
    [AmountUSD] decimal(18,2) NOT NULL,
    [AmountZAR] decimal(18,2) NOT NULL,
    [ExchangeRateUsed] decimal(18,2) NOT NULL,
    [Status] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ServiceRequests] PRIMARY KEY ([ServiceRequestId]),
    CONSTRAINT [FK_ServiceRequests_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([ContractId]) ON DELETE CASCADE
);
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ClientId', N'Email', N'Name', N'Phone', N'Region') AND [object_id] = OBJECT_ID(N'[Clients]'))
    SET IDENTITY_INSERT [Clients] ON;
INSERT INTO [Clients] ([ClientId], [Email], [Name], [Phone], [Region])
VALUES (1, N'info@techmove.co.za', N'TechMove SA', N'+27 11 123 4567', N'Africa'),
(2, N'contact@globalfreight.com', N'Global Freight Ltd', N'+1 212 555 1234', N'North America');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ClientId', N'Email', N'Name', N'Phone', N'Region') AND [object_id] = OBJECT_ID(N'[Clients]'))
    SET IDENTITY_INSERT [Clients] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ContractId', N'ClientId', N'EndDate', N'ServiceLevel', N'SignedAgreementPath', N'StartDate', N'Status') AND [object_id] = OBJECT_ID(N'[Contracts]'))
    SET IDENTITY_INSERT [Contracts] ON;
INSERT INTO [Contracts] ([ContractId], [ClientId], [EndDate], [ServiceLevel], [SignedAgreementPath], [StartDate], [Status])
VALUES (1, 1, '2025-12-31T00:00:00.0000000', 2, NULL, '2024-01-01T00:00:00.0000000', 1),
(2, 2, '2023-12-31T00:00:00.0000000', 0, NULL, '2023-01-01T00:00:00.0000000', 2);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ContractId', N'ClientId', N'EndDate', N'ServiceLevel', N'SignedAgreementPath', N'StartDate', N'Status') AND [object_id] = OBJECT_ID(N'[Contracts]'))
    SET IDENTITY_INSERT [Contracts] OFF;
GO

CREATE INDEX [IX_Contracts_ClientId] ON [Contracts] ([ClientId]);
GO

CREATE INDEX [IX_ServiceRequests_ContractId] ON [ServiceRequests] ([ContractId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260512120716_InitialCreate', N'8.0.0');
GO

COMMIT;
GO

