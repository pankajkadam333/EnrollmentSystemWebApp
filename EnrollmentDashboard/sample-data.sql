-- ============================================
-- ETO Take-Home Exercise - Sample Data
-- ============================================

-- NOTE: Run database-schema.sql first before running this script

USE EnrollmentSystem; -- Change to your database name
GO

-- ============================================
-- Clear existing data (optional - for testing)
-- ============================================

-- DELETE FROM Enrollments;
-- DELETE FROM Programs;
-- DELETE FROM Participants;
-- DBCC CHECKIDENT ('Enrollments', RESEED, 0);
-- DBCC CHECKIDENT ('Programs', RESEED, 0);
-- DBCC CHECKIDENT ('Participants', RESEED, 0);

-- ============================================
-- Insert Participants
-- ============================================

INSERT INTO Participants (FirstName, LastName, DateOfBirth, Active)
VALUES
    ('John', 'Doe', '1985-03-15', 1),
    ('Jane', 'Smith', '1990-07-22', 1),
    ('Michael', 'Johnson', '1978-11-30', 1),
    ('Sarah', 'Williams', '1995-05-18', 1),
    ('David', 'Brown', '1982-09-08', 0),
    ('Robert', '<script>alert("XSS")</script>', '1988-04-12', 1), -- XSS test case
    ('Emily', 'Davis', '1992-06-25', 1),
    ('James', 'Wilson', '1980-12-10', 1),
    ('Maria', 'Garcia', '1987-02-28', 1),
    ('Christopher', 'Martinez', '1993-09-14', 1),
    ('Jessica', 'Anderson', '1991-04-03', 1),
    ('Daniel', 'Taylor', '1984-08-19', 1),
    ('Ashley', 'Thomas', '1996-01-07', 1),
    ('Matthew', 'Moore', '1979-11-22', 1),
    ('Amanda', 'Jackson', '1989-05-30', 1);

-- ============================================
-- Insert Programs
-- ============================================

INSERT INTO Programs (ProgramName, Description, Active)
VALUES
    ('Leadership Development', 'Management and leadership skills training', 1),
    ('Technical Skills Workshop', 'Hands-on technology training', 1),
    ('Communication Bootcamp', 'Professional communication and presentation skills', 1),
    ('Data Analytics Course', 'Introduction to data analysis and visualization', 1),
    ('Project Management Fundamentals', 'Core project management concepts and tools', 1),
    ('Advanced Excel Training', 'Advanced spreadsheet analysis and automation', 1),
    ('Public Speaking Mastery', 'Overcome fear and deliver impactful presentations', 1),
    ('Customer Service Excellence', 'Building exceptional customer relationships', 0); -- Inactive program

-- ============================================
-- Insert Enrollments
-- ============================================

INSERT INTO Enrollments (ParticipantID, ProgramID, EnrollmentDate, CompletionDate, Status, Notes)
VALUES
    -- Active enrollments
    (1, 1, '2026-01-15', NULL, 'Active', 'Started leadership program'),
    (1, 2, '2026-02-01', NULL, 'Active', 'Enrolled in technical workshop'),
    (2, 3, '2026-01-20', NULL, 'Active', 'Attending weekly sessions'),
    (3, 4, '2026-03-01', NULL, 'Active', 'In progress'),
    (6, 1, '2026-03-20', NULL, 'Active', '<img src=x onerror="alert(''XSS'')"> Test note'), -- XSS in notes
    (7, 2, '2026-04-01', NULL, 'Active', 'Recently started'),
    (8, 5, '2026-03-15', NULL, 'Active', 'Good progress so far'),
    (9, 6, '2026-02-20', NULL, 'Active', 'Learning advanced techniques'),
    (10, 7, '2026-04-10', NULL, 'Active', 'Building confidence'),
    (11, 1, '2026-01-25', NULL, 'Active', 'Strong participant'),

    -- Completed enrollments
    (2, 1, '2025-12-10', '2026-03-15', 'Completed', 'Successfully completed program'),
    (3, 2, '2025-11-05', '2026-01-30', 'Completed', 'Completed all modules'),
    (7, 3, '2025-10-15', '2026-02-28', 'Completed', 'Excellent performance'),
    (8, 4, '2025-09-01', '2025-12-20', 'Completed', 'All requirements met'),
    (11, 2, '2025-08-15', '2025-11-30', 'Completed', 'Outstanding work'),
    (12, 5, '2025-07-10', '2025-10-15', 'Completed', 'Passed final exam'),
    (13, 6, '2025-06-01', '2025-09-30', 'Completed', 'Mastered all concepts'),
    (14, 7, '2025-05-15', '2025-08-20', 'Completed', 'Very impressive'),
    (15, 1, '2025-04-20', '2025-07-25', 'Completed', 'Strong leadership skills'),

    -- Withdrawn enrollments
    (4, 1, '2026-02-14', '2026-03-01', 'Withdrawn', 'Left program early'),
    (12, 1, '2026-01-10', '2026-02-15', 'Withdrawn', 'Personal reasons'),
    (13, 3, '2025-12-01', '2026-01-05', 'Withdrawn', 'Schedule conflict'),

    -- Historical data for testing date ranges
    (5, 2, '2025-01-15', '2025-04-20', 'Completed', 'Completed last year'),
    (5, 3, '2024-09-10', '2024-12-15', 'Completed', 'Old enrollment'),
    (6, 4, '2024-06-01', '2024-09-30', 'Completed', 'Historical data');

-- ============================================
-- Verification Queries
-- ============================================

PRINT 'Sample data inserted successfully!';
PRINT '';

-- Count records
SELECT 'Participants' AS TableName, COUNT(*) AS RecordCount FROM Participants
UNION ALL
SELECT 'Programs', COUNT(*) FROM Programs
UNION ALL
SELECT 'Enrollments', COUNT(*) FROM Enrollments;

PRINT '';
PRINT 'Enrollment Status Breakdown:';

-- Status breakdown
SELECT
    Status,
    COUNT(*) AS Count
FROM Enrollments
GROUP BY Status
ORDER BY Status;

PRINT '';
PRINT 'Recent Enrollments (Last 90 days):';

-- Recent enrollments
SELECT TOP 5
    p.FirstName + ' ' + p.LastName AS ParticipantName,
    pr.ProgramName,
    e.EnrollmentDate,
    e.Status
FROM Enrollments e
INNER JOIN Participants p ON e.ParticipantID = p.ParticipantID
INNER JOIN Programs pr ON e.ProgramID = pr.ProgramID
WHERE e.EnrollmentDate >= DATEADD(DAY, -90, GETDATE())
ORDER BY e.EnrollmentDate DESC;
