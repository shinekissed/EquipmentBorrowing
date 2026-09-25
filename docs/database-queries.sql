> -- =============================================================================
> -- ITSD 81: Laboratory Activity 3 - Database Queries
-- File: docs/database-queries.sql
-- =============================================================================

-- 1. Basic Retrieval
SELECT Id, Name, IsAvailable FROM Equipment;

-- 2. Filtering
SELECT Id, Name, IsAvailable FROM Equipment WHERE IsAvailable = 1;

-- 3. Join
SELECT 
    s.Name AS Student,
    e.Name AS Equipment,
    b.DateBorrowed AS Borrowed,
    b.ExpectedReturnDate AS Due
FROM Borrowings b
INNER JOIN Students s ON b.StudentId = s.Id
INNER JOIN Equipment e ON b.EquipmentId = e.Id
WHERE b.Status = 0;

-- 4. Aggregate Query
SELECT 
    s.Id AS StudentId,
    s.Name AS StudentName,
    COUNT(b.Id) AS ActiveBorrowingCount
FROM Students s
LEFT JOIN Borrowings b ON s.Id = b.StudentId AND b.Status = 0
GROUP BY s.Id, s.Name;

-- 5. Update Statement
UPDATE Equipment SET IsAvailable = 1 WHERE Id = 101;
UPDATE Borrowings SET Status = 1 WHERE EquipmentId = 101 AND Status = 0;
