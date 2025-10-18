-- Create database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ShipManagement')
BEGIN
    CREATE DATABASE ShipManagement;
END
GO

USE ShipManagement;
GO

-- Create Ship table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Ship')
BEGIN
    CREATE TABLE Ship (
        Code NVARCHAR(10) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        FiscalYear NVARCHAR(4) NOT NULL CHECK (FiscalYear LIKE '[0-1][0-9][0-1][0-9]'), -- Format: MMYY
        Status BIT NOT NULL CONSTRAINT DF_Ship_Status DEFAULT 1
    );
END
GO

-- Create User table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AppUser')
BEGIN
    CREATE TABLE AppUser (
        UserId INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Role NVARCHAR(50) NOT NULL
    );
END
GO

-- Create UserShipAssignment table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserShipAssignment')
BEGIN
    CREATE TABLE UserShipAssignment (
        UserId INT NOT NULL,
        ShipCode NVARCHAR(10) NOT NULL,
        PRIMARY KEY (UserId, ShipCode),
        FOREIGN KEY (UserId) REFERENCES AppUser(UserId),
        FOREIGN KEY (ShipCode) REFERENCES Ship(Code)
    );
END
GO

-- Create CrewMember table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CrewMember')
BEGIN
    CREATE TABLE CrewMember (
        CrewMemberId NVARCHAR(20) PRIMARY KEY,
        FirstName NVARCHAR(50) NOT NULL,
        LastName NVARCHAR(50) NOT NULL,
        BirthDate DATE NOT NULL,
        Nationality NVARCHAR(50) NOT NULL
    );
END
GO

-- Create CrewRank table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CrewRank')
BEGIN
    CREATE TABLE CrewRank (
        RankId INT IDENTITY(1,1) PRIMARY KEY,
        RankName NVARCHAR(50) NOT NULL UNIQUE,
        Description NVARCHAR(200) NULL
    );
END
GO

-- Create CrewServiceHistory table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CrewServiceHistory')
BEGIN
    CREATE TABLE CrewServiceHistory (
        ServiceId INT IDENTITY(1,1) PRIMARY KEY,
        CrewMemberId NVARCHAR(20) NOT NULL,
        ShipCode NVARCHAR(10) NOT NULL,
        RankId INT NOT NULL,
        SignOnDate DATE NOT NULL,
        SignOffDate DATE NULL,
        EndOfContractDate DATE NOT NULL,
        FOREIGN KEY (CrewMemberId) REFERENCES CrewMember(CrewMemberId),
        FOREIGN KEY (ShipCode) REFERENCES Ship(Code),
        FOREIGN KEY (RankId) REFERENCES CrewRank(RankId),
        CHECK (SignOnDate <= EndOfContractDate),
        CHECK (SignOffDate IS NULL OR (SignOffDate >= SignOnDate AND SignOffDate <= EndOfContractDate))
    );
END
GO

-- Create ChartOfAccounts table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ChartOfAccounts')
BEGIN
    CREATE TABLE ChartOfAccounts (
        AccountNumber NVARCHAR(20) PRIMARY KEY,
        Description NVARCHAR(200) NOT NULL,
        ParentAccountNumber NVARCHAR(20) NULL,
        AccountType NVARCHAR(10) NOT NULL CHECK (AccountType IN ('Parent', 'Child')),
        FOREIGN KEY (ParentAccountNumber) REFERENCES ChartOfAccounts(AccountNumber)
    );
END
GO

-- Create BudgetData table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BudgetData')
BEGIN
    CREATE TABLE BudgetData (
        BudgetId INT IDENTITY(1,1) PRIMARY KEY,
        ShipCode NVARCHAR(10) NOT NULL,
        AccountNumber NVARCHAR(20) NOT NULL,
        AccountPeriod DATE NOT NULL, -- First day of the month
        BudgetValue DECIMAL(18, 2) NOT NULL CHECK (BudgetValue >= 0),
        FOREIGN KEY (ShipCode) REFERENCES Ship(Code),
        FOREIGN KEY (AccountNumber) REFERENCES ChartOfAccounts(AccountNumber),
        -- Ensure AccountPeriod is always the first day of a month
        CHECK (DAY(AccountPeriod) = 1),
        -- Ensure unique combination of ShipCode, AccountNumber, and AccountPeriod
        CONSTRAINT UQ_Budget_Ship_Account_Period UNIQUE (ShipCode, AccountNumber, AccountPeriod)
    );
END
GO

-- Create AccountTransaction table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AccountTransaction')
BEGIN
    CREATE TABLE AccountTransaction (
        TransactionId INT IDENTITY(1,1) PRIMARY KEY,
        ShipCode NVARCHAR(10) NOT NULL,
        AccountNumber NVARCHAR(20) NOT NULL,
        AccountPeriod DATE NOT NULL, -- First day of the month
        ActualValue DECIMAL(18, 2) NOT NULL CHECK (ActualValue >= 0),
        FOREIGN KEY (ShipCode) REFERENCES Ship(Code),
        FOREIGN KEY (AccountNumber) REFERENCES ChartOfAccounts(AccountNumber),
        -- Ensure AccountPeriod is always the first day of a month
        CHECK (DAY(AccountPeriod) = 1)
    );
END
GO

-- Create indexes for performance
CREATE INDEX IX_CrewServiceHistory_ShipCode ON CrewServiceHistory(ShipCode);
CREATE INDEX IX_CrewServiceHistory_CrewMemberId ON CrewServiceHistory(CrewMemberId);
CREATE INDEX IX_BudgetData_ShipCode_AccountPeriod ON BudgetData(ShipCode, AccountPeriod);
CREATE INDEX IX_AccountTransaction_ShipCode_AccountPeriod ON AccountTransaction(ShipCode, AccountPeriod);
CREATE INDEX IX_ChartOfAccounts_ParentAccountNumber ON ChartOfAccounts(ParentAccountNumber);
GO
