# Entity Relationship Diagram (ERD)

## Entities and Relationships

### Main Entities
1. **Ship**
   - Primary Key: Code (string)
   - Name (string)
   - FiscalYear (string) - Format: "MMYY" (e.g., "0112", "0403")
   - Status (string) - "Active" or "Inactive"

2. **User**
   - Primary Key: UserId (int)
   - Name (string)
   - Role (string)

3. **UserShipAssignment**
   - Composite Primary Key: UserId (int), ShipCode (string)
   - Foreign Key: UserId references User(UserId)
   - Foreign Key: ShipCode references Ship(Code)

4. **CrewMember**
   - Primary Key: CrewMemberId (string)
   - FirstName (string)
   - LastName (string)
   - BirthDate (date)
   - Nationality (string)

5. **CrewRank**
   - Primary Key: RankId (int)
   - RankName (string)
   - Description (string)

6. **CrewServiceHistory**
   - Primary Key: ServiceId (int)
   - Foreign Key: CrewMemberId references CrewMember(CrewMemberId)
   - Foreign Key: ShipCode references Ship(Code)
   - Foreign Key: RankId references CrewRank(RankId)
   - SignOnDate (date)
   - SignOffDate (date, nullable)
   - EndOfContractDate (date)

7. **ChartOfAccounts**
   - Primary Key: AccountNumber (string)
   - Description (string)
   - ParentAccountNumber (string, nullable) - Self-referencing foreign key
   - AccountType (string) - "Parent" or "Child"

8. **BudgetData**
   - Primary Key: BudgetId (int)
   - Foreign Key: ShipCode references Ship(Code)
   - Foreign Key: AccountNumber references ChartOfAccounts(AccountNumber)
   - AccountPeriod (date) - First day of the month
   - BudgetValue (decimal)

9. **AccountTransaction**
   - Primary Key: TransactionId (int)
   - Foreign Key: ShipCode references Ship(Code)
   - Foreign Key: AccountNumber references ChartOfAccounts(AccountNumber)
   - AccountPeriod (date) - First day of the month
   - ActualValue (decimal)

## Relationships

1. **User to Ship** (Many-to-Many)
   - Implemented through UserShipAssignment junction table

2. **CrewMember to Ship** (Many-to-Many)
   - Implemented through CrewServiceHistory table
   - Additional attributes: RankId, SignOnDate, SignOffDate, EndOfContractDate

3. **CrewMember to CrewRank** (Many-to-Many)
   - Implemented through CrewServiceHistory table
   - A crew member can have different ranks on different ships or at different times

4. **ChartOfAccounts to ChartOfAccounts** (Self-referencing, One-to-Many)
   - ParentAccountNumber references AccountNumber
   - Represents the hierarchical structure of accounts

5. **Ship to BudgetData** (One-to-Many)
   - A ship can have multiple budget entries

6. **Ship to AccountTransaction** (One-to-Many)
   - A ship can have multiple transaction entries

7. **ChartOfAccounts to BudgetData** (One-to-Many)
   - An account can have multiple budget entries

8. **ChartOfAccounts to AccountTransaction** (One-to-Many)
   - An account can have multiple transaction entries