/*
    Creates tables used by DatasetConfigDbContext:
    - DatasetDefinitions
    - SelectedColumns
    - FilterRules
*/

IF OBJECT_ID(N'dbo.FilterRules', N'U') IS NOT NULL
    DROP TABLE dbo.FilterRules;

IF OBJECT_ID(N'dbo.SelectedColumns', N'U') IS NOT NULL
    DROP TABLE dbo.SelectedColumns;

IF OBJECT_ID(N'dbo.DatasetDefinitions', N'U') IS NOT NULL
    DROP TABLE dbo.DatasetDefinitions;

CREATE TABLE dbo.DatasetDefinitions
(
    Id UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT PK_DatasetDefinitions PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    SourceTable NVARCHAR(256) NOT NULL,
    CreatedAtUtc DATETIMEOFFSET(7) NOT NULL
        CONSTRAINT DF_DatasetDefinitions_CreatedAtUtc DEFAULT SYSUTCDATETIME()
);

CREATE TABLE dbo.SelectedColumns
(
    Id UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT PK_SelectedColumns PRIMARY KEY,
    DatasetDefinitionId UNIQUEIDENTIFIER NOT NULL,
    ColumnName NVARCHAR(256) NOT NULL,
    Alias NVARCHAR(256) NULL,
    CONSTRAINT FK_SelectedColumns_DatasetDefinitions
        FOREIGN KEY (DatasetDefinitionId)
        REFERENCES dbo.DatasetDefinitions(Id)
        ON DELETE CASCADE
);

CREATE TABLE dbo.FilterRules
(
    Id UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT PK_FilterRules PRIMARY KEY,
    DatasetDefinitionId UNIQUEIDENTIFIER NOT NULL,
    ColumnName NVARCHAR(256) NOT NULL,
    [Operator] NVARCHAR(50) NOT NULL,
    [Value] NVARCHAR(MAX) NOT NULL,
    CONSTRAINT FK_FilterRules_DatasetDefinitions
        FOREIGN KEY (DatasetDefinitionId)
        REFERENCES dbo.DatasetDefinitions(Id)
        ON DELETE CASCADE
);

CREATE INDEX IX_SelectedColumns_DatasetDefinitionId
    ON dbo.SelectedColumns (DatasetDefinitionId);

CREATE INDEX IX_FilterRules_DatasetDefinitionId
    ON dbo.FilterRules (DatasetDefinitionId);
