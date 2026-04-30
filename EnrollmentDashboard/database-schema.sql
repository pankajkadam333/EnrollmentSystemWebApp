-- ============================================
-- ETO Take-Home Exercise - Database Schema
-- ============================================

-- Create database (optional - candidate can use existing database)
-- CREATE DATABASE EnrollmentSystem;
-- GO
-- USE EnrollmentSystem;
-- GO

-- ============================================
-- Tables
-- ============================================

-- Participants Table (people who can enroll in programs)
CREATE TABLE Participants (
    ParticipantID INT PRIMARY KEY IDENTITY(1,1),
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    DateOfBirth DATE NOT NULL,
    Active BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE()
);

-- Programs Table (programs/courses available)
CREATE TABLE Programs (
    ProgramID INT PRIMARY KEY IDENTITY(1,1),
    ProgramName NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    Active BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE()
);

-- Enrollments Table (tracks when participants join programs)
CREATE TABLE Enrollments (
    EnrollmentID INT PRIMARY KEY IDENTITY(1,1),
    ParticipantID INT NOT NULL,
    ProgramID INT NOT NULL,
    EnrollmentDate DATE NOT NULL,
    CompletionDate DATE NULL,
    Status NVARCHAR(50) NOT NULL, -- 'Active', 'Completed', 'Withdrawn'
    Notes NVARCHAR(MAX),
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Enrollments_Participants FOREIGN KEY (ParticipantID)
        REFERENCES Participants(ParticipantID),
    CONSTRAINT FK_Enrollments_Programs FOREIGN KEY (ProgramID)
        REFERENCES Programs(ProgramID),
    CONSTRAINT CHK_Enrollments_Status CHECK (Status IN ('Active', 'Completed', 'Withdrawn'))
);

-- ============================================
-- Indexes (Candidates should consider these)
-- ============================================

-- Index for filtering by enrollment date
CREATE NONCLUSTERED INDEX IX_Enrollments_EnrollmentDate
    ON Enrollments(EnrollmentDate DESC);

-- Index for filtering by status
CREATE NONCLUSTERED INDEX IX_Enrollments_Status
    ON Enrollments(Status)
    INCLUDE (ParticipantID, ProgramID, EnrollmentDate);

-- Index for participant lookups
CREATE NONCLUSTERED INDEX IX_Participants_Name
    ON Participants(LastName, FirstName);

-- Index for active participants
CREATE NONCLUSTERED INDEX IX_Participants_Active
    ON Participants(Active)
    WHERE Active = 1;

-- ============================================
-- Sample Stored Procedure
-- ============================================

-- Example stored procedure for getting enrollments with filters
CREATE PROCEDURE sp_GetEnrollments
    @StartDate DATE = NULL,
    @EndDate DATE = NULL,
    @Status NVARCHAR(50) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    SELECT
        e.EnrollmentID,
        e.ParticipantID,
        p.FirstName,
        p.LastName,
        e.ProgramID,
        pr.ProgramName,
        e.EnrollmentDate,
        e.CompletionDate,
        e.Status,
        e.Notes,
        COUNT(*) OVER() AS TotalRecords
    FROM Enrollments e
    INNER JOIN Participants p ON e.ParticipantID = p.ParticipantID
    INNER JOIN Programs pr ON e.ProgramID = pr.ProgramID
    WHERE
        (@StartDate IS NULL OR e.EnrollmentDate >= @StartDate)
        AND (@EndDate IS NULL OR e.EnrollmentDate <= @EndDate)
        AND (@Status IS NULL OR e.Status = @Status)
    ORDER BY e.EnrollmentDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- ============================================
-- Summary Statistics Stored Procedure
-- ============================================

CREATE PROCEDURE sp_GetEnrollmentSummary
    @StartDate DATE = NULL,
    @EndDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        COUNT(*) AS TotalEnrollments,
        SUM(CASE WHEN Status = 'Active' THEN 1 ELSE 0 END) AS ActiveEnrollments,
        SUM(CASE WHEN Status = 'Completed' THEN 1 ELSE 0 END) AS CompletedEnrollments,
        SUM(CASE WHEN Status = 'Withdrawn' THEN 1 ELSE 0 END) AS WithdrawnEnrollments
    FROM Enrollments
    WHERE
        (@StartDate IS NULL OR EnrollmentDate >= @StartDate)
        AND (@EndDate IS NULL OR EnrollmentDate <= @EndDate);
END;
GO
