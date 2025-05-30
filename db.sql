USE StockManagementDB
GO

PRINT 'Beginning Product entity conflict diagnosis'
PRINT 'Current date/time: 2025-05-30 08:57:52'
PRINT 'Current user: Rvheem'
GO

-- Check for duplicate ProductIds in the Products table
SELECT ProductId, COUNT(*) as DuplicateCount
FROM Products
GROUP BY ProductId
HAVING COUNT(*) > 1
GO

-- Verify the identity column is set up correctly
SELECT 
    name, 
    is_identity
FROM sys.columns
WHERE object_id = OBJECT_ID('Products') 
AND name = 'ProductId'
GO

-- Ensure PK constraint exists on ProductId
SELECT * 
FROM sys.key_constraints 
WHERE parent_object_id = OBJECT_ID('Products') 
AND type = 'PK'
GO

-- Check for products with duplicate References (which might cause conflicts)
SELECT Reference, COUNT(*) as DuplicateCount
FROM Products
GROUP BY Reference
HAVING COUNT(*) > 1
GO

-- Add audit log entry
INSERT INTO Histories (Action, Date, UserId)
SELECT 
    'Diagnosed ProductId tracking conflict', 
    '2025-05-30 08:57:52', 
    UserId 
FROM Users 
WHERE Username = 'Rvheem'
GO

PRINT 'Diagnosis completed at 2025-05-30 08:57:52'
GO