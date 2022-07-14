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

CREATE TABLE [CtsUsers] (
    [CtsUserID] int NOT NULL IDENTITY,
    [IsActive] bit NOT NULL,
    [Rfid] nvarchar(max) NULL,
    [CreateTime] datetime2 NOT NULL,
    [LastUpdateTime] datetime2 NOT NULL,
    [DetailsAsJson] nvarchar(max) NULL,
    CONSTRAINT [PK_CtsUsers] PRIMARY KEY ([CtsUserID])
);
GO

CREATE TABLE [StationConfigurations] (
    [StationConfigurationID] int NOT NULL IDENTITY,
    [StationID] nvarchar(max) NULL,
    [DetailsAsJson] nvarchar(max) NULL,
    CONSTRAINT [PK_StationConfigurations] PRIMARY KEY ([StationConfigurationID])
);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210312172926_InitialCreate', N'6.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [SurveyQuestionVersions] (
    [SurveyQuestionVersionID] int NOT NULL IDENTITY,
    [QuestionID] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreateTime] datetime2 NOT NULL,
    [DetailsAsJson] nvarchar(max) NULL,
    CONSTRAINT [PK_SurveyQuestionVersions] PRIMARY KEY ([SurveyQuestionVersionID])
);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210328114418_Add-SurveyQuestionVersin', N'6.0.0');
GO

COMMIT;
GO

