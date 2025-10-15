USE ShipManagement;
GO

CREATE OR ALTER PROCEDURE GetCrewList
    @ShipCode NVARCHAR(10),
    @SearchTerm NVARCHAR(100) = NULL,
    @SortColumn NVARCHAR(50) = 'RankName',
    @SortDirection NVARCHAR(4) = 'ASC',
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @StatusFilter NVARCHAR(20) = NULL,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate input parameters
    IF @ShipCode IS NULL
    BEGIN
        RAISERROR('ShipCode is required', 16, 1);
        RETURN;
    END
    
    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 SET @PageSize = 10;
    
    -- Validate sort column
    IF @SortColumn NOT IN ('RankName', 'CrewMemberId', 'FirstName', 'LastName', 'Age', 'Nationality', 'SignOnDate', 'Status')
    BEGIN
        SET @SortColumn = 'RankName';
    END
    
    -- Validate sort direction
    IF @SortDirection NOT IN ('ASC', 'DESC')
    BEGIN
        SET @SortDirection = 'ASC';
    END
    
    DECLARE @Statuses TABLE (Status NVARCHAR(20));

    IF @StatusFilter IS NULL
    BEGIN
        INSERT INTO @Statuses VALUES ('Onboard'), ('Relief Due');
    END
    ELSE
    BEGIN
        INSERT INTO @Statuses VALUES (@StatusFilter);
    END
    
    -- Calculate current date for status determination
    DECLARE @CurrentDate DATE = GETDATE();
    
    -- Create a CTE to calculate crew status and apply filters
    ;WITH CrewStatusCTE AS (
        SELECT 
            r.RankName,
            c.CrewMemberId,
            c.FirstName,
            c.LastName,
            DATEDIFF(YEAR, c.BirthDate, @CurrentDate) AS Age,
            c.Nationality,
            csh.SignOnDate,
            csh.SignOffDate,
            CASE
                WHEN csh.SignOnDate > @CurrentDate THEN 'Planned'
                WHEN csh.SignOffDate IS NULL AND csh.EndOfContractDate >= @CurrentDate THEN 'Onboard'
                WHEN csh.SignOffDate IS NULL AND DATEDIFF(DAY, csh.EndOfContractDate, @CurrentDate) > 30 THEN 'Relief Due'
                WHEN csh.SignOffDate IS NOT NULL THEN 'Signed Off'
                ELSE 'Unknown'
            END AS Status
        FROM CrewServiceHistory csh
        INNER JOIN CrewMember c ON csh.CrewMemberId = c.CrewMemberId
        INNER JOIN CrewRank r ON csh.RankId = r.RankId
        WHERE 
            csh.ShipCode = @ShipCode
            AND csh.SignOffDate IS NULL -- Exclude signed off crew
            AND (
                @SearchTerm IS NULL
                OR c.CrewMemberId LIKE '%' + @SearchTerm + '%'
                OR c.FirstName LIKE '%' + @SearchTerm + '%'
                OR c.LastName LIKE '%' + @SearchTerm + '%'
                OR CAST(DATEDIFF(YEAR, c.BirthDate, @CurrentDate) AS NVARCHAR) LIKE '%' + @SearchTerm + '%'
                OR c.Nationality LIKE '%' + @SearchTerm + '%'
                OR r.RankName LIKE '%' + @SearchTerm + '%'
                OR CONVERT(NVARCHAR, csh.SignOnDate, 106) LIKE '%' + @SearchTerm + '%' -- Format: dd MMM yyyy
                OR CONVERT(NVARCHAR, csh.SignOnDate, 105) LIKE '%' + @SearchTerm + '%' -- Format: dd-MM-yyyy
            )
    )
    
    -- Get total count for pagination
    SELECT @TotalCount = COUNT(*)
    FROM CrewStatusCTE
    WHERE Status IN (SELECT Status FROM @Statuses);
    
    ;WITH CrewStatusCTE AS (
        SELECT 
            r.RankName,
            c.CrewMemberId,
            c.FirstName,
            c.LastName,
            DATEDIFF(YEAR, c.BirthDate, @CurrentDate) AS Age,
            c.Nationality,
            csh.SignOnDate,
            csh.SignOffDate,
            CASE
                WHEN csh.SignOnDate > @CurrentDate THEN 'Planned'
                WHEN csh.SignOffDate IS NULL AND csh.EndOfContractDate >= @CurrentDate THEN 'Onboard'
                WHEN csh.SignOffDate IS NULL AND DATEDIFF(DAY, csh.EndOfContractDate, @CurrentDate) > 30 THEN 'Relief Due'
                WHEN csh.SignOffDate IS NOT NULL THEN 'Signed Off'
                ELSE 'Unknown'
            END AS Status
        FROM CrewServiceHistory csh
        INNER JOIN CrewMember c ON csh.CrewMemberId = c.CrewMemberId
        INNER JOIN CrewRank r ON csh.RankId = r.RankId
        WHERE 
            csh.ShipCode = @ShipCode
            AND csh.SignOffDate IS NULL -- Exclude signed off crew
            AND (
                @SearchTerm IS NULL
                OR c.CrewMemberId LIKE '%' + @SearchTerm + '%'
                OR c.FirstName LIKE '%' + @SearchTerm + '%'
                OR c.LastName LIKE '%' + @SearchTerm + '%'
                OR CAST(DATEDIFF(YEAR, c.BirthDate, @CurrentDate) AS NVARCHAR) LIKE '%' + @SearchTerm + '%'
                OR c.Nationality LIKE '%' + @SearchTerm + '%'
                OR r.RankName LIKE '%' + @SearchTerm + '%'
                OR CONVERT(NVARCHAR, csh.SignOnDate, 106) LIKE '%' + @SearchTerm + '%'
                OR CONVERT(NVARCHAR, csh.SignOnDate, 105) LIKE '%' + @SearchTerm + '%'
            )
    )
    
    -- Get paginated results with dynamic sorting
    SELECT 
        @ShipCode AS ShipCode,
        RankName,
        CrewMemberId,
        FirstName,
        LastName,
        Age,
        Nationality,
        SignOnDate,
        SignOffDate,
        Status
    FROM CrewStatusCTE
    WHERE Status IN (SELECT Status FROM @Statuses)
    ORDER BY
        CASE WHEN @SortColumn = 'RankName' AND @SortDirection = 'ASC' THEN RankName END ASC,
        CASE WHEN @SortColumn = 'RankName' AND @SortDirection = 'DESC' THEN RankName END DESC,
        CASE WHEN @SortColumn = 'CrewMemberId' AND @SortDirection = 'ASC' THEN CrewMemberId END ASC,
        CASE WHEN @SortColumn = 'CrewMemberId' AND @SortDirection = 'DESC' THEN CrewMemberId END DESC,
        CASE WHEN @SortColumn = 'FirstName' AND @SortDirection = 'ASC' THEN FirstName END ASC,
        CASE WHEN @SortColumn = 'FirstName' AND @SortDirection = 'DESC' THEN FirstName END DESC,
        CASE WHEN @SortColumn = 'LastName' AND @SortDirection = 'ASC' THEN LastName END ASC,
        CASE WHEN @SortColumn = 'LastName' AND @SortDirection = 'DESC' THEN LastName END DESC,
        CASE WHEN @SortColumn = 'Age' AND @SortDirection = 'ASC' THEN Age END ASC,
        CASE WHEN @SortColumn = 'Age' AND @SortDirection = 'DESC' THEN Age END DESC,
        CASE WHEN @SortColumn = 'Nationality' AND @SortDirection = 'ASC' THEN Nationality END ASC,
        CASE WHEN @SortColumn = 'Nationality' AND @SortDirection = 'DESC' THEN Nationality END DESC,
        CASE WHEN @SortColumn = 'SignOnDate' AND @SortDirection = 'ASC' THEN SignOnDate END ASC,
        CASE WHEN @SortColumn = 'SignOnDate' AND @SortDirection = 'DESC' THEN SignOnDate END DESC,
        CASE WHEN @SortColumn = 'Status' AND @SortDirection = 'ASC' THEN Status END ASC,
        CASE WHEN @SortColumn = 'Status' AND @SortDirection = 'DESC' THEN Status END DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- Stored procedure for financial report (detailed)
CREATE OR ALTER PROCEDURE GetFinancialReportDetail
    @ShipCode NVARCHAR(10),
    @AccountPeriod DATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate input parameters
    IF @ShipCode IS NULL OR @AccountPeriod IS NULL
    BEGIN
        RAISERROR('ShipCode and AccountPeriod are required', 16, 1);
        RETURN;
    END
    
    -- Ensure AccountPeriod is the first day of a month
    SET @AccountPeriod = DATEFROMPARTS(YEAR(@AccountPeriod), MONTH(@AccountPeriod), 1);
    
    -- Get ship's fiscal year
    DECLARE @FiscalYear NVARCHAR(4);
    SELECT @FiscalYear = FiscalYear FROM Ship WHERE Code = @ShipCode;
    
    IF @FiscalYear IS NULL
    BEGIN
        RAISERROR('Ship not found', 16, 1);
        RETURN;
    END
    
    -- Parse fiscal year start month and end month
    DECLARE @FiscalStartMonth INT = CAST(LEFT(@FiscalYear, 2) AS INT);
    DECLARE @FiscalEndMonth INT = CAST(RIGHT(@FiscalYear, 2) AS INT);
    
    -- Calculate fiscal year start date for YTD calculations
    DECLARE @FiscalYearStartDate DATE;
    
    -- If current month is before fiscal start month, use previous year
    IF MONTH(@AccountPeriod) < @FiscalStartMonth
    BEGIN
        SET @FiscalYearStartDate = DATEFROMPARTS(YEAR(@AccountPeriod) - 1, @FiscalStartMonth, 1);
    END
    ELSE
    BEGIN
        SET @FiscalYearStartDate = DATEFROMPARTS(YEAR(@AccountPeriod), @FiscalStartMonth, 1);
    END

    DECLARE @AccountPeriodLabel NVARCHAR(12) = LEFT(DATENAME(MONTH, @AccountPeriod), 3) + ' ' + CAST(YEAR(@AccountPeriod) AS NVARCHAR(4));
    DECLARE @FiscalYearStartLabel NVARCHAR(12) = LEFT(DATENAME(MONTH, @FiscalYearStartDate), 3) + ' ' + CAST(YEAR(@FiscalYearStartDate) AS NVARCHAR(4));
    DECLARE @FiscalYearEndLabel NVARCHAR(12) = @AccountPeriodLabel;
    
    -- Build account hierarchy including levels
    ;WITH AccountHierarchyCTE AS (
        SELECT 
            coa.AccountNumber,
            coa.Description,
            coa.ParentAccountNumber,
            coa.AccountType,
            0 AS Level
        FROM ChartOfAccounts coa
        WHERE coa.ParentAccountNumber IS NULL
        
        UNION ALL
        
        SELECT 
            child.AccountNumber,
            child.Description,
            child.ParentAccountNumber,
            child.AccountType,
            parent.Level + 1
        FROM ChartOfAccounts child
        INNER JOIN AccountHierarchyCTE parent ON child.ParentAccountNumber = parent.AccountNumber
    ),
    
    -- Map every account to its descendants for roll-up aggregation
    AccountClosureCTE AS (
        SELECT 
            ah.AccountNumber AS AncestorAccountNumber,
            ah.AccountNumber AS DescendantAccountNumber
        FROM AccountHierarchyCTE ah
        
        UNION ALL
        
        SELECT 
            parent.AccountNumber AS AncestorAccountNumber,
            closure.DescendantAccountNumber
        FROM AccountClosureCTE closure
        INNER JOIN AccountHierarchyCTE ancestor ON ancestor.AccountNumber = closure.AncestorAccountNumber
        INNER JOIN AccountHierarchyCTE parent ON ancestor.ParentAccountNumber = parent.AccountNumber
    ),
    
    -- CTE for budget data for the selected period
    BudgetCTE AS (
        SELECT 
            b.AccountNumber,
            SUM(b.BudgetValue) AS BudgetValue
        FROM BudgetData b
        WHERE 
            b.ShipCode = @ShipCode
            AND b.AccountPeriod = @AccountPeriod
        GROUP BY b.AccountNumber
    ),
    
    -- CTE for actual data for the selected period
    ActualCTE AS (
        SELECT 
            a.AccountNumber,
            SUM(a.ActualValue) AS ActualValue
        FROM AccountTransaction a
        WHERE 
            a.ShipCode = @ShipCode
            AND a.AccountPeriod = @AccountPeriod
        GROUP BY a.AccountNumber
    ),
    
    -- CTE for YTD budget data
    BudgetYTDCTE AS (
        SELECT 
            b.AccountNumber,
            SUM(b.BudgetValue) AS BudgetValueYTD
        FROM BudgetData b
        WHERE 
            b.ShipCode = @ShipCode
            AND b.AccountPeriod >= @FiscalYearStartDate
            AND b.AccountPeriod <= @AccountPeriod
        GROUP BY b.AccountNumber
    ),
    
    -- CTE for YTD actual data
    ActualYTDCTE AS (
        SELECT 
            a.AccountNumber,
            SUM(a.ActualValue) AS ActualValueYTD
        FROM AccountTransaction a
        WHERE 
            a.ShipCode = @ShipCode
            AND a.AccountPeriod >= @FiscalYearStartDate
            AND a.AccountPeriod <= @AccountPeriod
        GROUP BY a.AccountNumber
    ),
    
    -- CTE to combine base financial data per account
    FinancialDataCTE AS (
        SELECT 
            ah.AccountNumber,
            ah.Description,
            ah.ParentAccountNumber,
            ah.AccountType,
            ah.Level,
            ISNULL(b.BudgetValue, 0) AS BudgetValue,
            ISNULL(a.ActualValue, 0) AS ActualValue,
            ISNULL(byb.BudgetValueYTD, 0) AS BudgetValueYTD,
            ISNULL(ay.ActualValueYTD, 0) AS ActualValueYTD
        FROM AccountHierarchyCTE ah
        LEFT JOIN BudgetCTE b ON ah.AccountNumber = b.AccountNumber
        LEFT JOIN ActualCTE a ON ah.AccountNumber = a.AccountNumber
        LEFT JOIN BudgetYTDCTE byb ON ah.AccountNumber = byb.AccountNumber
        LEFT JOIN ActualYTDCTE ay ON ah.AccountNumber = ay.AccountNumber
    ),
    
    -- Aggregate descendant values up to each ancestor account
    AggregatedFinancialDataCTE AS (
        SELECT 
            anc.AccountNumber,
            anc.Description,
            anc.ParentAccountNumber,
            anc.AccountType,
            anc.Level,
            SUM(fd.BudgetValue) AS BudgetValue,
            SUM(fd.ActualValue) AS ActualValue,
            SUM(fd.BudgetValueYTD) AS BudgetValueYTD,
            SUM(fd.ActualValueYTD) AS ActualValueYTD
        FROM FinancialDataCTE anc
        LEFT JOIN AccountClosureCTE closure ON anc.AccountNumber = closure.AncestorAccountNumber
        LEFT JOIN FinancialDataCTE fd ON closure.DescendantAccountNumber = fd.AccountNumber
        GROUP BY anc.AccountNumber, anc.Description, anc.ParentAccountNumber, anc.AccountType, anc.Level
    )
    
    -- Final result with variances and period labels
    SELECT 
        afd.Description AS AccountDescription,
        afd.AccountNumber,
        afd.ActualValue,
        afd.BudgetValue,
        (afd.ActualValue - afd.BudgetValue) AS VarianceActual,
        afd.ActualValueYTD,
        afd.BudgetValueYTD,
        (afd.ActualValueYTD - afd.BudgetValueYTD) AS VarianceYTD,
        @AccountPeriodLabel AS AccountPeriodLabel,
        @FiscalYearStartLabel AS FiscalYearStartLabel,
        @FiscalYearEndLabel AS FiscalYearEndLabel
    FROM AggregatedFinancialDataCTE afd
    WHERE 
        (afd.BudgetValue <> 0 OR afd.ActualValue <> 0 OR afd.BudgetValueYTD <> 0 OR afd.ActualValueYTD <> 0)
    ORDER BY afd.AccountNumber
    OPTION (MAXRECURSION 32767);
END
GO

-- Stored procedure for financial report (summary)
CREATE OR ALTER PROCEDURE GetFinancialReportSummary
    @ShipCode NVARCHAR(10),
    @AccountPeriod DATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate input parameters
    IF @ShipCode IS NULL OR @AccountPeriod IS NULL
    BEGIN
        RAISERROR('ShipCode and AccountPeriod are required', 16, 1);
        RETURN;
    END
    
    -- Ensure AccountPeriod is the first day of a month
    SET @AccountPeriod = DATEFROMPARTS(YEAR(@AccountPeriod), MONTH(@AccountPeriod), 1);
    
    -- Get ship's fiscal year
    DECLARE @FiscalYear NVARCHAR(4);
    SELECT @FiscalYear = FiscalYear FROM Ship WHERE Code = @ShipCode;
    
    IF @FiscalYear IS NULL
    BEGIN
        RAISERROR('Ship not found', 16, 1);
        RETURN;
    END
    
    -- Parse fiscal year start month and end month
    DECLARE @FiscalStartMonth INT = CAST(LEFT(@FiscalYear, 2) AS INT);
    DECLARE @FiscalEndMonth INT = CAST(RIGHT(@FiscalYear, 2) AS INT);
    
    -- Calculate fiscal year start date for YTD calculations
    DECLARE @FiscalYearStartDate DATE;
    
    -- If current month is before fiscal start month, use previous year
    IF MONTH(@AccountPeriod) < @FiscalStartMonth
    BEGIN
        SET @FiscalYearStartDate = DATEFROMPARTS(YEAR(@AccountPeriod) - 1, @FiscalStartMonth, 1);
    END
    ELSE
    BEGIN
        SET @FiscalYearStartDate = DATEFROMPARTS(YEAR(@AccountPeriod), @FiscalStartMonth, 1);
    END

    DECLARE @AccountPeriodLabelSummary NVARCHAR(12) = LEFT(DATENAME(MONTH, @AccountPeriod), 3) + ' ' + CAST(YEAR(@AccountPeriod) AS NVARCHAR(4));
    DECLARE @FiscalYearStartLabelSummary NVARCHAR(12) = LEFT(DATENAME(MONTH, @FiscalYearStartDate), 3) + ' ' + CAST(YEAR(@FiscalYearStartDate) AS NVARCHAR(4));
    DECLARE @FiscalYearEndLabelSummary NVARCHAR(12) = @AccountPeriodLabelSummary;
    
    -- Get top-level accounts only
    ;WITH TopLevelAccountsCTE AS (
        SELECT 
            a.AccountNumber,
            a.Description
        FROM ChartOfAccounts a
        WHERE a.ParentAccountNumber IS NULL
    ),
    
    -- CTE for budget data for the selected period (aggregated to top level)
    BudgetCTE AS (
        SELECT 
            coa.ParentAccountNumber AS TopLevelAccount,
            SUM(b.BudgetValue) AS BudgetValue
        FROM BudgetData b
        INNER JOIN ChartOfAccounts coa ON b.AccountNumber = coa.AccountNumber
        INNER JOIN ChartOfAccounts parent ON coa.ParentAccountNumber = parent.AccountNumber
        WHERE 
            b.ShipCode = @ShipCode
            AND b.AccountPeriod = @AccountPeriod
            AND parent.ParentAccountNumber IS NULL
        GROUP BY coa.ParentAccountNumber
    ),
    
    -- CTE for actual data for the selected period (aggregated to top level)
    ActualCTE AS (
        SELECT 
            coa.ParentAccountNumber AS TopLevelAccount,
            SUM(a.ActualValue) AS ActualValue
        FROM AccountTransaction a
        INNER JOIN ChartOfAccounts coa ON a.AccountNumber = coa.AccountNumber
        INNER JOIN ChartOfAccounts parent ON coa.ParentAccountNumber = parent.AccountNumber
        WHERE 
            a.ShipCode = @ShipCode
            AND a.AccountPeriod = @AccountPeriod
            AND parent.ParentAccountNumber IS NULL
        GROUP BY coa.ParentAccountNumber
    ),
    
    -- CTE for YTD budget data (aggregated to top level)
    BudgetYTDCTE AS (
        SELECT 
            coa.ParentAccountNumber AS TopLevelAccount,
            SUM(b.BudgetValue) AS BudgetValueYTD
        FROM BudgetData b
        INNER JOIN ChartOfAccounts coa ON b.AccountNumber = coa.AccountNumber
        INNER JOIN ChartOfAccounts parent ON coa.ParentAccountNumber = parent.AccountNumber
        WHERE 
            b.ShipCode = @ShipCode
            AND b.AccountPeriod >= @FiscalYearStartDate
            AND b.AccountPeriod <= @AccountPeriod
            AND parent.ParentAccountNumber IS NULL
        GROUP BY coa.ParentAccountNumber
    ),
    
    -- CTE for YTD actual data (aggregated to top level)
    ActualYTDCTE AS (
        SELECT 
            coa.ParentAccountNumber AS TopLevelAccount,
            SUM(a.ActualValue) AS ActualValueYTD
        FROM AccountTransaction a
        INNER JOIN ChartOfAccounts coa ON a.AccountNumber = coa.AccountNumber
        INNER JOIN ChartOfAccounts parent ON coa.ParentAccountNumber = parent.AccountNumber
        WHERE 
            a.ShipCode = @ShipCode
            AND a.AccountPeriod >= @FiscalYearStartDate
            AND a.AccountPeriod <= @AccountPeriod
            AND parent.ParentAccountNumber IS NULL
        GROUP BY coa.ParentAccountNumber
    )
    
    -- Final result with variances and period labels
    SELECT 
        tla.Description AS AccountDescription,
        tla.AccountNumber,
        ISNULL(a.ActualValue, 0) AS ActualValue,
        ISNULL(b.BudgetValue, 0) AS BudgetValue,
        ISNULL(a.ActualValue, 0) - ISNULL(b.BudgetValue, 0) AS VarianceActual,
        ISNULL(ay.ActualValueYTD, 0) AS ActualValueYTD,
        ISNULL(byc.BudgetValueYTD, 0) AS BudgetValueYTD,
        ISNULL(ay.ActualValueYTD, 0) - ISNULL(byc.BudgetValueYTD, 0) AS VarianceYTD,
        @AccountPeriodLabelSummary AS AccountPeriodLabel,
        @FiscalYearStartLabelSummary AS FiscalYearStartLabel,
        @FiscalYearEndLabelSummary AS FiscalYearEndLabel
    FROM TopLevelAccountsCTE tla
    LEFT JOIN BudgetCTE b ON tla.AccountNumber = b.TopLevelAccount
    LEFT JOIN ActualCTE a ON tla.AccountNumber = a.TopLevelAccount
    LEFT JOIN BudgetYTDCTE byc ON tla.AccountNumber = byc.TopLevelAccount
    LEFT JOIN ActualYTDCTE ay ON tla.AccountNumber = ay.TopLevelAccount
    WHERE 
        ISNULL(b.BudgetValue, 0) <> 0 OR 
        ISNULL(a.ActualValue, 0) <> 0 OR 
        ISNULL(byc.BudgetValueYTD, 0) <> 0 OR 
        ISNULL(ay.ActualValueYTD, 0) <> 0
    ORDER BY tla.AccountNumber;
END
GO

-- Stored procedures for CRUD operations on Ship
CREATE OR ALTER PROCEDURE CreateShip
    @Code NVARCHAR(10),
    @Name NVARCHAR(100),
    @FiscalYear NVARCHAR(4),
    @Status NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM Ship WHERE Code = @Code)
    BEGIN
        RAISERROR('Ship with this code already exists', 16, 1);
        RETURN;
    END
    
    INSERT INTO Ship (Code, Name, FiscalYear, Status)
    VALUES (@Code, @Name, @FiscalYear, @Status);
    
    SELECT * FROM Ship WHERE Code = @Code;
END
GO

CREATE OR ALTER PROCEDURE GetShips
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM Ship;
END
GO

CREATE OR ALTER PROCEDURE GetShipByCode
    @Code NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM Ship WHERE Code = @Code;
END
GO

CREATE OR ALTER PROCEDURE UpdateShip
    @Code NVARCHAR(10),
    @Name NVARCHAR(100),
    @FiscalYear NVARCHAR(4),
    @Status NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    
    IF NOT EXISTS (SELECT 1 FROM Ship WHERE Code = @Code)
    BEGIN
        RAISERROR('Ship not found', 16, 1);
        RETURN;
    END
    
    UPDATE Ship
    SET Name = @Name,
        FiscalYear = @FiscalYear,
        Status = @Status
    WHERE Code = @Code;
    
    SELECT * FROM Ship WHERE Code = @Code;
END
GO

CREATE OR ALTER PROCEDURE DeleteShip
    @Code NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    
    IF NOT EXISTS (SELECT 1 FROM Ship WHERE Code = @Code)
    BEGIN
        RAISERROR('Ship not found', 16, 1);
        RETURN;
    END
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Delete related records first
        DELETE FROM UserShipAssignment WHERE ShipCode = @Code;
        DELETE FROM CrewServiceHistory WHERE ShipCode = @Code;
        DELETE FROM BudgetData WHERE ShipCode = @Code;
        DELETE FROM AccountTransaction WHERE ShipCode = @Code;
        
        -- Delete the ship
        DELETE FROM Ship WHERE Code = @Code;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- Stored procedures for CRUD operations on User
CREATE OR ALTER PROCEDURE CreateUser
    @Name NVARCHAR(100),
    @Role NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO AppUser (Name, Role)
    VALUES (@Name, @Role);
    
    SELECT * FROM AppUser WHERE UserId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE GetUsers
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM AppUser;
END
GO

CREATE OR ALTER PROCEDURE GetUserById
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM AppUser WHERE UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE UpdateUser
    @UserId INT,
    @Name NVARCHAR(100),
    @Role NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    IF NOT EXISTS (SELECT 1 FROM AppUser WHERE UserId = @UserId)
    BEGIN
        RAISERROR('User not found', 16, 1);
        RETURN;
    END
    
    UPDATE AppUser
    SET Name = @Name,
        Role = @Role
    WHERE UserId = @UserId;
    
    SELECT * FROM AppUser WHERE UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE DeleteUser
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF NOT EXISTS (SELECT 1 FROM AppUser WHERE UserId = @UserId)
    BEGIN
        RAISERROR('User not found', 16, 1);
        RETURN;
    END
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Delete related records first
        DELETE FROM UserShipAssignment WHERE UserId = @UserId;
        
        -- Delete the user
        DELETE FROM AppUser WHERE UserId = @UserId;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- Stored procedures for User-Ship assignments
CREATE OR ALTER PROCEDURE AssignShipToUser
    @UserId INT,
    @ShipCode NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    
    IF NOT EXISTS (SELECT 1 FROM AppUser WHERE UserId = @UserId)
    BEGIN
        RAISERROR('User not found', 16, 1);
        RETURN;
    END
    
    IF NOT EXISTS (SELECT 1 FROM Ship WHERE Code = @ShipCode)
    BEGIN
        RAISERROR('Ship not found', 16, 1);
        RETURN;
    END
    
    IF EXISTS (SELECT 1 FROM UserShipAssignment WHERE UserId = @UserId AND ShipCode = @ShipCode)
    BEGIN
        RAISERROR('Assignment already exists', 16, 1);
        RETURN;
    END
    
    INSERT INTO UserShipAssignment (UserId, ShipCode)
    VALUES (@UserId, @ShipCode);
    
    SELECT u.UserId, u.Name, u.Role, s.Code, s.Name, s.FiscalYear, s.Status
    FROM UserShipAssignment usa
    INNER JOIN AppUser u ON usa.UserId = u.UserId
    INNER JOIN Ship s ON usa.ShipCode = s.Code
    WHERE usa.UserId = @UserId AND usa.ShipCode = @ShipCode;
END
GO

CREATE OR ALTER PROCEDURE RemoveShipFromUser
    @UserId INT,
    @ShipCode NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    
    IF NOT EXISTS (SELECT 1 FROM UserShipAssignment WHERE UserId = @UserId AND ShipCode = @ShipCode)
    BEGIN
        RAISERROR('Assignment not found', 16, 1);
        RETURN;
    END
    
    DELETE FROM UserShipAssignment
    WHERE UserId = @UserId AND ShipCode = @ShipCode;
END
GO

CREATE OR ALTER PROCEDURE GetShipsByUser
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF NOT EXISTS (SELECT 1 FROM AppUser WHERE UserId = @UserId)
    BEGIN
        RAISERROR('User not found', 16, 1);
        RETURN;
    END
    
    SELECT s.*
    FROM UserShipAssignment usa
    INNER JOIN Ship s ON usa.ShipCode = s.Code
    WHERE usa.UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE GetUsersByShip
    @ShipCode NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    
    IF NOT EXISTS (SELECT 1 FROM Ship WHERE Code = @ShipCode)
    BEGIN
        RAISERROR('Ship not found', 16, 1);
        RETURN;
    END
    
    SELECT u.*
    FROM UserShipAssignment usa
    INNER JOIN AppUser u ON usa.UserId = u.UserId
    WHERE usa.ShipCode = @ShipCode;
END
GO
