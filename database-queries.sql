-- =============================================================================
-- ITSD 81: Laboratory Activity 3 - Database Queries
-- Demonstrating Relational Structure and SQL Operations
-- =============================================================================


-- 1. BASIC RETRIEVAL
-- Retrieve all equipment records.

SELECT
    Id,
    Name,
    IsAvailable
FROM Equipment;


-- 2. FILTERING
-- Retrieve only currently available equipment.

SELECT
    Id,
    Name,
    IsAvailable
FROM Equipment
WHERE IsAvailable = 1;


-- 3. JOIN
-- Retrieve active borrowings together with borrower student name
-- and equipment details.

SELECT
    s.Name AS Student,
    e.Name AS Equipment,
    b.DateBorrowed AS Borrowed,
    b.ExpectedReturnDate AS Due
FROM Borrowings AS b
INNER JOIN Students AS s
    ON b.StudentId = s.Id
INNER JOIN Equipment AS e
    ON b.EquipmentId = e.Id
WHERE b.Status = 0; -- 0 represents BorrowingStatus.Active


-- 4. AGGREGATE QUERY
-- Count the number of active borrowings per student.

SELECT
    s.Id AS StudentId,
    s.Name AS StudentName,
    COUNT(b.Id) AS ActiveBorrowingCount
FROM Students AS s
LEFT JOIN Borrowings AS b
    ON s.Id = b.StudentId
    AND b.Status = 0
GROUP BY
    s.Id,
    s.Name;


-- 5. UPDATE STATEMENT
-- Mark equipment #101 as available and set its borrowing record to returned.

UPDATE Equipment
SET IsAvailable = 1
WHERE Id = 101;

UPDATE Borrowings
SET Status = 1 -- 1 represents BorrowingStatus.Returned
WHERE EquipmentId = 101
    AND Status = 0;