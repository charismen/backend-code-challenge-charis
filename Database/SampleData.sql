USE ShipManagement;
GO

-- Insert Ship data
INSERT INTO Ship (Code, Name, FiscalYear, Status) VALUES
('SHIP01', 'Flying Dutchman', '0112', 'Active'),
('SHIP02', 'Thousand Sunny', '0403', 'Active'),
('SHIP03', 'Black Pearl', '0112', 'Active'),
('SHIP04', 'Nautilus', '0706', 'Active'),
('SHIP05', 'Queen Anne', '0403', 'Inactive');
GO

-- Insert User data
INSERT INTO AppUser (Name, Role) VALUES
('John Smith', 'Administrator'),
('Jane Doe', 'Manager'),
('Robert Johnson', 'Operator'),
('Emily Wilson', 'Analyst'),
('Michael Brown', 'Support');
GO

-- Insert UserShipAssignment data
INSERT INTO UserShipAssignment (UserId, ShipCode) VALUES
(1, 'SHIP01'), (1, 'SHIP02'), (1, 'SHIP03'),
(2, 'SHIP01'), (2, 'SHIP04'),
(3, 'SHIP02'), (3, 'SHIP03'),
(4, 'SHIP04'), (4, 'SHIP05'),
(5, 'SHIP01'), (5, 'SHIP05');
GO

-- Insert CrewRank data
INSERT INTO CrewRank (RankName, Description) VALUES
('Master', 'Captain of the ship'),
('Chief Engineer', 'Head of the engineering department'),
('Chief Officer', 'Second in command after the Master'),
('Second Engineer', 'Second in command of the engineering department'),
('Second Officer', 'Third in command after the Chief Officer'),
('Third Engineer', 'Third in command of the engineering department'),
('Third Officer', 'Fourth in command after the Second Officer'),
('Bosun', 'In charge of deck crew and equipment'),
('Able Seaman', 'Experienced deck crew member'),
('Ordinary Seaman', 'Entry-level deck crew member'),
('Oiler', 'Entry-level engine room crew member'),
('Cook', 'In charge of food preparation'),
('Steward', 'In charge of accommodation and service'),
('Cadet', 'Trainee officer');
GO

-- Insert CrewMember data (20+ per ship)
-- First, create a procedure to generate crew members
CREATE OR ALTER PROCEDURE GenerateCrewMembers
AS
BEGIN
    DECLARE @counter INT = 1;
    DECLARE @crewId NVARCHAR(20);
    DECLARE @firstName NVARCHAR(50);
    DECLARE @lastName NVARCHAR(50);
    DECLARE @birthDate DATE;
    DECLARE @nationality NVARCHAR(50);
    
    -- List of sample first names
    DECLARE @firstNames TABLE (Name NVARCHAR(50));
    INSERT INTO @firstNames VALUES 
    ('John'), ('James'), ('Robert'), ('Michael'), ('William'), ('David'), ('Richard'), ('Joseph'), ('Thomas'), ('Charles'),
    ('Mary'), ('Patricia'), ('Jennifer'), ('Linda'), ('Elizabeth'), ('Barbara'), ('Susan'), ('Jessica'), ('Sarah'), ('Karen'),
    ('Daniel'), ('Matthew'), ('Anthony'), ('Mark'), ('Donald'), ('Steven'), ('Paul'), ('Andrew'), ('Joshua'), ('Kenneth'),
    ('Lisa'), ('Nancy'), ('Betty'), ('Sandra'), ('Margaret'), ('Ashley'), ('Kimberly'), ('Emily'), ('Donna'), ('Michelle');
    
    -- List of sample last names
    DECLARE @lastNames TABLE (Name NVARCHAR(50));
    INSERT INTO @lastNames VALUES 
    ('Smith'), ('Johnson'), ('Williams'), ('Jones'), ('Brown'), ('Davis'), ('Miller'), ('Wilson'), ('Moore'), ('Taylor'),
    ('Anderson'), ('Thomas'), ('Jackson'), ('White'), ('Harris'), ('Martin'), ('Thompson'), ('Garcia'), ('Martinez'), ('Robinson'),
    ('Clark'), ('Rodriguez'), ('Lewis'), ('Lee'), ('Walker'), ('Hall'), ('Allen'), ('Young'), ('Hernandez'), ('King'),
    ('Wright'), ('Lopez'), ('Hill'), ('Scott'), ('Green'), ('Adams'), ('Baker'), ('Gonzalez'), ('Nelson'), ('Carter');
    
    -- List of sample nationalities
    DECLARE @nationalities TABLE (Name NVARCHAR(50));
    INSERT INTO @nationalities VALUES 
    ('American'), ('British'), ('Canadian'), ('Australian'), ('Greek'), ('Italian'), ('Spanish'), ('Portuguese'), ('Filipino'), ('Indian'),
    ('Russian'), ('Ukrainian'), ('Polish'), ('Norwegian'), ('Swedish'), ('Danish'), ('Finnish'), ('Dutch'), ('Belgian'), ('French');
    
    -- Generate 120 crew members (more than 20 per ship)
    WHILE @counter <= 120
    BEGIN
        SET @crewId = 'CREW' + RIGHT('000' + CAST(@counter AS NVARCHAR(3)), 3);
        
        -- Select random first name
        SELECT TOP 1 @firstName = Name FROM @firstNames ORDER BY NEWID();
        
        -- Select random last name
        SELECT TOP 1 @lastName = Name FROM @lastNames ORDER BY NEWID();
        
        -- Generate random birth date (between 25 and 60 years ago)
        SET @birthDate = DATEADD(YEAR, -25 - (ABS(CHECKSUM(NEWID())) % 35), GETDATE());
        
        -- Select random nationality
        SELECT TOP 1 @nationality = Name FROM @nationalities ORDER BY NEWID();
        
        -- Insert crew member if not exists
        IF NOT EXISTS (SELECT 1 FROM CrewMember WHERE CrewMemberId = @crewId)
        BEGIN
            INSERT INTO CrewMember (CrewMemberId, FirstName, LastName, BirthDate, Nationality)
            VALUES (@crewId, @firstName, @lastName, @birthDate, @nationality);
        END
        
        SET @counter = @counter + 1;
    END
END
GO

-- Execute the procedure to generate crew members
EXEC GenerateCrewMembers;
GO

-- Drop the temporary procedure
DROP PROCEDURE GenerateCrewMembers;
GO

-- Insert CrewServiceHistory data
-- Create a procedure to generate service history
CREATE OR ALTER PROCEDURE GenerateServiceHistory
AS
BEGIN
    DECLARE @shipCodes TABLE (Code NVARCHAR(10));
    INSERT INTO @shipCodes SELECT Code FROM Ship;
    
    DECLARE @rankIds TABLE (Id INT);
    INSERT INTO @rankIds SELECT RankId FROM CrewRank;
    
    DECLARE @crewIds TABLE (Id NVARCHAR(20));
    INSERT INTO @crewIds SELECT CrewMemberId FROM CrewMember;
    
    DECLARE @shipCode NVARCHAR(10);
    DECLARE @rankId INT;
    DECLARE @crewId NVARCHAR(20);
    DECLARE @signOnDate DATE;
    DECLARE @signOffDate DATE;
    DECLARE @endOfContractDate DATE;
    DECLARE @currentDate DATE = GETDATE();
    
    -- Create cursor for ships
    DECLARE ship_cursor CURSOR FOR SELECT Code FROM @shipCodes;
    OPEN ship_cursor;
    FETCH NEXT FROM ship_cursor INTO @shipCode;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- For each ship, assign at least 20 crew members
        DECLARE @crewCount INT = 0;
        DECLARE @totalCrewNeeded INT = 25; -- More than 20 to ensure we have enough
        
        -- Create cursor for crew members
        DECLARE crew_cursor CURSOR FOR SELECT Id FROM @crewIds ORDER BY NEWID();
        OPEN crew_cursor;
        FETCH NEXT FROM crew_cursor INTO @crewId;
        
        WHILE @@FETCH_STATUS = 0 AND @crewCount < @totalCrewNeeded
        BEGIN
            -- Select random rank
            SELECT TOP 1 @rankId = Id FROM @rankIds ORDER BY NEWID();
            
            -- Generate random dates
            -- Some crew will be onboard, some planned, some relief due, some signed off
            DECLARE @scenario INT = ABS(CHECKSUM(NEWID())) % 4; -- 0-3
            
            IF @scenario = 0 -- Onboard
            BEGIN
                SET @signOnDate = DATEADD(DAY, -(ABS(CHECKSUM(NEWID())) % 60), @currentDate); -- 0-60 days ago
                SET @signOffDate = NULL;
                SET @endOfContractDate = DATEADD(DAY, (ABS(CHECKSUM(NEWID())) % 60) + 1, @currentDate); -- 1-60 days in future
            END
            ELSE IF @scenario = 1 -- Planned
            BEGIN
                SET @signOnDate = DATEADD(DAY, (ABS(CHECKSUM(NEWID())) % 60) + 1, @currentDate); -- 1-60 days in future
                SET @signOffDate = NULL;
                SET @endOfContractDate = DATEADD(DAY, (ABS(CHECKSUM(NEWID())) % 120) + 61, @currentDate); -- 61-180 days in future
            END
            ELSE IF @scenario = 2 -- Relief Due
            BEGIN
                SET @signOnDate = DATEADD(DAY, -(ABS(CHECKSUM(NEWID())) % 120) - 60, @currentDate); -- 60-180 days ago
                SET @signOffDate = NULL;
                SET @endOfContractDate = DATEADD(DAY, -(ABS(CHECKSUM(NEWID())) % 30) - 31, @currentDate); -- 31-60 days ago
            END
            ELSE -- Signed Off
            BEGIN
                SET @signOnDate = DATEADD(DAY, -(ABS(CHECKSUM(NEWID())) % 180) - 180, @currentDate); -- 180-360 days ago
                SET @endOfContractDate = DATEADD(DAY, -(ABS(CHECKSUM(NEWID())) % 90) - 30, @currentDate); -- 30-120 days ago
                SET @signOffDate = DATEADD(DAY, (ABS(CHECKSUM(NEWID())) % DATEDIFF(DAY, @signOnDate, @endOfContractDate)), @signOnDate); -- Random date between sign on and end of contract
            END
            
            -- Insert service history
            INSERT INTO CrewServiceHistory (CrewMemberId, ShipCode, RankId, SignOnDate, SignOffDate, EndOfContractDate)
            VALUES (@crewId, @shipCode, @rankId, @signOnDate, @signOffDate, @endOfContractDate);
            
            SET @crewCount = @crewCount + 1;
            FETCH NEXT FROM crew_cursor INTO @crewId;
        END
        
        CLOSE crew_cursor;
        DEALLOCATE crew_cursor;
        
        FETCH NEXT FROM ship_cursor INTO @shipCode;
    END
    
    CLOSE ship_cursor;
    DEALLOCATE ship_cursor;
END
GO

-- Execute the procedure to generate service history
EXEC GenerateServiceHistory;
GO

-- Drop the temporary procedure
DROP PROCEDURE GenerateServiceHistory;
GO

-- Insert ChartOfAccounts data
-- First, create parent accounts
INSERT INTO ChartOfAccounts (AccountNumber, Description, ParentAccountNumber, AccountType) VALUES
('7000000', 'OPERATING EXPENSES', NULL, 'Parent'),
('8000000', 'CREW EXPENSES', NULL, 'Parent'),
('9000000', 'MAINTENANCE EXPENSES', NULL, 'Parent'),
('6000000', 'VOYAGE EXPENSES', NULL, 'Parent'),
('5000000', 'ADMINISTRATIVE EXPENSES', NULL, 'Parent');
GO

-- Create child accounts for OPERATING EXPENSES
INSERT INTO ChartOfAccounts (AccountNumber, Description, ParentAccountNumber, AccountType) VALUES
('7100000', 'AWARD AND GRANT TO INDIVIDUALS', '7000000', 'Parent'),
('7200000', 'SUPPLIES AND MATERIALS', '7000000', 'Parent'),
('7300000', 'EQUIPMENT', '7000000', 'Parent'),
('7400000', 'UTILITIES', '7000000', 'Parent'),
('7500000', 'MISCELLANEOUS OPERATING EXPENSES', '7000000', 'Parent');
GO

-- Create child accounts for AWARD AND GRANT TO INDIVIDUALS
INSERT INTO ChartOfAccounts (AccountNumber, Description, ParentAccountNumber, AccountType) VALUES
('7110000', 'BONUSES', '7100000', 'Child'),
('7120000', 'AWARDS', '7100000', 'Child'),
('7130000', 'GRANTS', '7100000', 'Child'),
('7135000', 'SCHOLARSHIPS', '7100000', 'Child'),
('7140000', 'INCENTIVES', '7100000', 'Child');
GO

-- Create child accounts for SUPPLIES AND MATERIALS
INSERT INTO ChartOfAccounts (AccountNumber, Description, ParentAccountNumber, AccountType) VALUES
('7210000', 'OFFICE SUPPLIES', '7200000', 'Child'),
('7220000', 'CLEANING SUPPLIES', '7200000', 'Child'),
('7230000', 'SAFETY EQUIPMENT', '7200000', 'Child'),
('7240000', 'FOOD SUPPLIES', '7200000', 'Child'),
('7250000', 'MEDICAL SUPPLIES', '7200000', 'Child');
GO

-- Create child accounts for CREW EXPENSES
INSERT INTO ChartOfAccounts (AccountNumber, Description, ParentAccountNumber, AccountType) VALUES
('8100000', 'SALARIES AND WAGES', '8000000', 'Parent'),
('8200000', 'TRAVEL EXPENSES', '8000000', 'Parent'),
('8300000', 'TRAINING', '8000000', 'Parent'),
('8400000', 'MEDICAL EXPENSES', '8000000', 'Parent'),
('8500000', 'CREW WELFARE', '8000000', 'Parent');
GO

-- Create child accounts for SALARIES AND WAGES
INSERT INTO ChartOfAccounts (AccountNumber, Description, ParentAccountNumber, AccountType) VALUES
('8110000', 'BASIC SALARY', '8100000', 'Child'),
('8120000', 'OVERTIME', '8100000', 'Child'),
('8130000', 'ALLOWANCES', '8100000', 'Child'),
('8140000', 'BONUSES', '8100000', 'Child'),
('8150000', 'SOCIAL SECURITY', '8100000', 'Child');
GO

-- Create child accounts for other parent accounts
-- For MAINTENANCE EXPENSES
INSERT INTO ChartOfAccounts (AccountNumber, Description, ParentAccountNumber, AccountType) VALUES
('9100000', 'HULL MAINTENANCE', '9000000', 'Child'),
('9200000', 'ENGINE MAINTENANCE', '9000000', 'Child'),
('9300000', 'DECK EQUIPMENT', '9000000', 'Child'),
('9400000', 'NAVIGATION EQUIPMENT', '9000000', 'Child'),
('9500000', 'SAFETY EQUIPMENT', '9000000', 'Child');
GO

-- For VOYAGE EXPENSES
INSERT INTO ChartOfAccounts (AccountNumber, Description, ParentAccountNumber, AccountType) VALUES
('6100000', 'FUEL COSTS', '6000000', 'Child'),
('6200000', 'PORT CHARGES', '6000000', 'Child'),
('6300000', 'CANAL FEES', '6000000', 'Child'),
('6400000', 'CARGO HANDLING', '6000000', 'Child'),
('6500000', 'AGENCY FEES', '6000000', 'Child');
GO

-- For ADMINISTRATIVE EXPENSES
INSERT INTO ChartOfAccounts (AccountNumber, Description, ParentAccountNumber, AccountType) VALUES
('5100000', 'OFFICE RENT', '5000000', 'Child'),
('5200000', 'COMMUNICATION', '5000000', 'Child'),
('5300000', 'INSURANCE', '5000000', 'Child'),
('5400000', 'LEGAL FEES', '5000000', 'Child'),
('5500000', 'ACCOUNTING FEES', '5000000', 'Child');
GO

-- Insert BudgetData and AccountTransaction data
-- Create a procedure to generate financial data
CREATE OR ALTER PROCEDURE GenerateFinancialData
AS
BEGIN
    DECLARE @shipCodes TABLE (Code NVARCHAR(10), FiscalYear NVARCHAR(4));
    INSERT INTO @shipCodes SELECT Code, FiscalYear FROM Ship WHERE Status = 'Active';
    
    DECLARE @childAccounts TABLE (AccountNumber NVARCHAR(20));
    INSERT INTO @childAccounts 
    SELECT AccountNumber FROM ChartOfAccounts 
    WHERE AccountType = 'Child';
    
    DECLARE @shipCode NVARCHAR(10);
    DECLARE @fiscalYear NVARCHAR(4);
    DECLARE @accountNumber NVARCHAR(20);
    DECLARE @accountPeriod DATE;
    DECLARE @budgetValue DECIMAL(18, 2);
    DECLARE @actualValue DECIMAL(18, 2);
    
    -- Create cursor for ships
    DECLARE ship_cursor CURSOR FOR SELECT Code, FiscalYear FROM @shipCodes;
    OPEN ship_cursor;
    FETCH NEXT FROM ship_cursor INTO @shipCode, @fiscalYear;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- For each ship, generate budget and actual data for each child account
        DECLARE account_cursor CURSOR FOR SELECT AccountNumber FROM @childAccounts;
        OPEN account_cursor;
        FETCH NEXT FROM account_cursor INTO @accountNumber;
        
        WHILE @@FETCH_STATUS = 0
        BEGIN
            -- Generate data for 2024 and 2025
            DECLARE @year INT;
            DECLARE @month INT;
            
            -- Determine fiscal year start month
            DECLARE @fiscalStartMonth INT = CAST(LEFT(@fiscalYear, 2) AS INT);
            
            -- Process 2024
            SET @year = 2024;
            SET @month = 1;
            
            WHILE @month <= 12
            BEGIN
                SET @accountPeriod = DATEFROMPARTS(@year, @month, 1);
                
                -- Generate random budget value (0-5000)
                SET @budgetValue = (ABS(CHECKSUM(NEWID())) % 5000) + (ABS(CHECKSUM(NEWID())) % 100) / 100.0;
                
                -- Insert budget data
                INSERT INTO BudgetData (ShipCode, AccountNumber, AccountPeriod, BudgetValue)
                VALUES (@shipCode, @accountNumber, @accountPeriod, @budgetValue);
                
                -- Generate 1-3 actual transactions for this period
                DECLARE @transactionCount INT = (ABS(CHECKSUM(NEWID())) % 3) + 1;
                DECLARE @transactionIndex INT = 1;
                
                WHILE @transactionIndex <= @transactionCount
                BEGIN
                    -- Generate random actual value (0-2000)
                    SET @actualValue = (ABS(CHECKSUM(NEWID())) % 2000) + (ABS(CHECKSUM(NEWID())) % 100) / 100.0;
                    
                    -- Insert transaction data
                    INSERT INTO AccountTransaction (ShipCode, AccountNumber, AccountPeriod, ActualValue)
                    VALUES (@shipCode, @accountNumber, @accountPeriod, @actualValue);
                    
                    SET @transactionIndex = @transactionIndex + 1;
                END
                
                SET @month = @month + 1;
            END
            
            -- Process 2025 (at least 6 periods)
            SET @year = 2025;
            SET @month = 1;
            
            WHILE @month <= 6
            BEGIN
                SET @accountPeriod = DATEFROMPARTS(@year, @month, 1);
                
                -- Generate random budget value (0-5000)
                SET @budgetValue = (ABS(CHECKSUM(NEWID())) % 5000) + (ABS(CHECKSUM(NEWID())) % 100) / 100.0;
                
                -- Insert budget data
                INSERT INTO BudgetData (ShipCode, AccountNumber, AccountPeriod, BudgetValue)
                VALUES (@shipCode, @accountNumber, @accountPeriod, @budgetValue);
                
                -- Generate 1-3 actual transactions for this period
                SET @transactionCount = (ABS(CHECKSUM(NEWID())) % 3) + 1;
                SET @transactionIndex = 1;
                
                WHILE @transactionIndex <= @transactionCount
                BEGIN
                    -- Generate random actual value (0-2000)
                    SET @actualValue = (ABS(CHECKSUM(NEWID())) % 2000) + (ABS(CHECKSUM(NEWID())) % 100) / 100.0;
                    
                    -- Insert transaction data
                    INSERT INTO AccountTransaction (ShipCode, AccountNumber, AccountPeriod, ActualValue)
                    VALUES (@shipCode, @accountNumber, @accountPeriod, @actualValue);
                    
                    SET @transactionIndex = @transactionIndex + 1;
                END
                
                SET @month = @month + 1;
            END
            
            FETCH NEXT FROM account_cursor INTO @accountNumber;
        END
        
        CLOSE account_cursor;
        DEALLOCATE account_cursor;
        
        FETCH NEXT FROM ship_cursor INTO @shipCode, @fiscalYear;
    END
    
    CLOSE ship_cursor;
    DEALLOCATE ship_cursor;
END
GO

-- Execute the procedure to generate financial data
EXEC GenerateFinancialData;
GO

-- Drop the temporary procedure
DROP PROCEDURE GenerateFinancialData;
GO