-- Xóa database nếu tồn tại
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'fintKhaoSat')
BEGIN
    DROP DATABASE fintKhaoSat;
END
GO

-- Tạo database mới
CREATE DATABASE fintKhaoSat;
GO
USE fintKhaoSat;
GO

-- 1. Bảng COMPANIES
CREATE TABLE COMPANIES (
    CompanyId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    Address NVARCHAR(255),
    Industry NVARCHAR(100)
);
GO

-- 2. Bảng DEPARTMENTS
CREATE TABLE DEPARTMENTS (
    DepartmentId INT IDENTITY(1,1) PRIMARY KEY,
    CompanyId INT NOT NULL,
    Name NVARCHAR(255) NOT NULL,
    CONSTRAINT FK_DEPARTMENTS_COMPANIES FOREIGN KEY (CompanyId) REFERENCES COMPANIES(CompanyId)
);
GO

-- 3. Bảng EMPLOYEES
CREATE TABLE EMPLOYEES (
    EmployeeId INT IDENTITY(1,1) PRIMARY KEY,
    DepartmentId INT NOT NULL,
    FullName NVARCHAR(255) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    DateOfBirth DATE,
    HireDate DATE,
    PasswordHash NVARCHAR(255),
    CONSTRAINT FK_EMPLOYEES_DEPARTMENTS FOREIGN KEY (DepartmentId) REFERENCES DEPARTMENTS(DepartmentId)
);


ALTER TABLE Employees
ADD Password NVARCHAR(255) NOT NULL DEFAULT '';

GO

-- 4. Bảng ROLES
CREATE TABLE ROLES (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL
);
GO

-- 5. Bảng EMPLOYEEROLES (many-to-many, chỉ 2 khóa ngoại) => thêm CreatedAt
CREATE TABLE EMPLOYEEROLES (
    EmployeeId INT NOT NULL,
    RoleId INT NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT PK_EMPLOYEEROLES PRIMARY KEY(EmployeeId, RoleId),
    CONSTRAINT FK_EMPLOYEEROLES_EMPLOYEES FOREIGN KEY(EmployeeId) REFERENCES EMPLOYEES(EmployeeId),
    CONSTRAINT FK_EMPLOYEEROLES_ROLES FOREIGN KEY(RoleId) REFERENCES ROLES(RoleId)
);
GO

-- 6. Bảng SKILLS
CREATE TABLE SKILLS (
    SkillId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Level NVARCHAR(50)
);
GO

-- 7. Bảng EMPLOYEESKILLS (many-to-many, chỉ 2 khóa ngoại) => thêm CreatedAt
CREATE TABLE EMPLOYEESKILLS (
    EmployeeId INT NOT NULL,
    SkillId INT NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT PK_EMPLOYEESKILLS PRIMARY KEY(EmployeeId, SkillId),
    CONSTRAINT FK_EMPLOYEESKILLS_EMPLOYEES FOREIGN KEY(EmployeeId) REFERENCES EMPLOYEES(EmployeeId),
    CONSTRAINT FK_EMPLOYEESKILLS_SKILLS FOREIGN KEY(SkillId) REFERENCES SKILLS(SkillId)
);
GO

-- 8. Bảng TRAININGS
CREATE TABLE TRAININGS (
    TrainingId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    Provider NVARCHAR(255),
    Date DATE
);
GO

-- 9. Bảng EMPLOYEETRAININGS (many-to-many nhưng có cột Result) => giữ nguyên
CREATE TABLE EMPLOYEETRAININGS (
    EmployeeId INT NOT NULL,
    TrainingId INT NOT NULL,
    Result NVARCHAR(100),
    CONSTRAINT PK_EMPLOYEETRAININGS PRIMARY KEY(EmployeeId, TrainingId),
    CONSTRAINT FK_EMPLOYEETRAININGS_EMPLOYEES FOREIGN KEY(EmployeeId) REFERENCES EMPLOYEES(EmployeeId),
    CONSTRAINT FK_EMPLOYEETRAININGS_TRAININGS FOREIGN KEY(TrainingId) REFERENCES TRAININGS(TrainingId)
);
GO

-- 10. Bảng TESTS
CREATE TABLE TESTS (
    TestId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    Type NVARCHAR(50),
    DurationMinutes INT,
    PassingScore FLOAT,
    CreatedBy INT,
    CONSTRAINT FK_TESTS_EMPLOYEES FOREIGN KEY(CreatedBy) REFERENCES EMPLOYEES(EmployeeId)
);
GO

-- 11. Bảng QUESTIONS
CREATE TABLE QUESTIONS (
    QuestionId INT IDENTITY(1,1) PRIMARY KEY,
    Content NVARCHAR(MAX) NOT NULL,
    Type NVARCHAR(50),
    SkillId INT,
    Difficulty NVARCHAR(50),
    CONSTRAINT FK_QUESTIONS_SKILLS FOREIGN KEY(SkillId) REFERENCES SKILLS(SkillId)
);
GO

-- 12. Bảng TESTQUESTIONS (many-to-many nhưng có cột QuestionOrder) => giữ nguyên
CREATE TABLE TESTQUESTIONS (
    TestId INT NOT NULL,
    QuestionId INT NOT NULL,
    QuestionOrder INT,
    CONSTRAINT PK_TESTQUESTIONS PRIMARY KEY(TestId, QuestionId),
    CONSTRAINT FK_TESTQUESTIONS_TESTS FOREIGN KEY(TestId) REFERENCES TESTS(TestId),
    CONSTRAINT FK_TESTQUESTIONS_QUESTIONS FOREIGN KEY(QuestionId) REFERENCES QUESTIONS(QuestionId)
);
GO

-- 13. Bảng EMPLOYEETESTS
CREATE TABLE EMPLOYEETESTS (
    EmployeeTestId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    TestId INT NOT NULL,
    StartTime DATETIME,
    EndTime DATETIME,
    Status NVARCHAR(50),
    TotalScore FLOAT,
    CONSTRAINT FK_EMPLOYEETESTS_EMPLOYEES FOREIGN KEY(EmployeeId) REFERENCES EMPLOYEES(EmployeeId),
    CONSTRAINT FK_EMPLOYEETESTS_TESTS FOREIGN KEY(TestId) REFERENCES TESTS(TestId)
);
GO

-- 14. Bảng EMPLOYEEANSWERS
CREATE TABLE EMPLOYEEANSWERS (
    EmployeeTestId INT NOT NULL,
    QuestionId INT NOT NULL,
    Answer NVARCHAR(MAX),
    Score FLOAT,
    CONSTRAINT PK_EMPLOYEEANSWERS PRIMARY KEY(EmployeeTestId, QuestionId),
    CONSTRAINT FK_EMPLOYEEANSWERS_EMPLOYEETESTS FOREIGN KEY(EmployeeTestId) REFERENCES EMPLOYEETESTS(EmployeeTestId),
    CONSTRAINT FK_EMPLOYEEANSWERS_QUESTIONS FOREIGN KEY(QuestionId) REFERENCES QUESTIONS(QuestionId)
);
GO

-- 15. Bảng FEEDBACKS
CREATE TABLE FEEDBACKS (
    FeedbackId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    TestId INT NOT NULL,
    Content NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_FEEDBACKS_EMPLOYEES FOREIGN KEY(EmployeeId) REFERENCES EMPLOYEES(EmployeeId),
    CONSTRAINT FK_FEEDBACKS_TESTS FOREIGN KEY(TestId) REFERENCES TESTS(TestId)
);
GO

-- 16. Bảng NOTIFICATIONS
CREATE TABLE NOTIFICATIONS (
    NotificationId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    Message NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(20) DEFAULT 'unread',
    CONSTRAINT FK_NOTIFICATIONS_EMPLOYEES FOREIGN KEY(EmployeeId) REFERENCES EMPLOYEES(EmployeeId)
);
GO

-- 17. Bảng AUDITLOGS
CREATE TABLE AUDITLOGS (
    AuditLogId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    Action NVARCHAR(255),
    Target NVARCHAR(255),
    Timestamp DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_AUDITLOGS_EMPLOYEES FOREIGN KEY(EmployeeId) REFERENCES EMPLOYEES(EmployeeId)
);
GO

-- 18. Bảng SYSTEMSETTINGS
CREATE TABLE SYSTEMSETTINGS (
    SettingId INT IDENTITY(1,1) PRIMARY KEY,
    [Key] NVARCHAR(100) NOT NULL UNIQUE,
    [Value] NVARCHAR(255)
);
GO


-- 1. Thêm công ty
INSERT INTO COMPANIES (Name, Address, Industry)
VALUES (N'FINT', N'Hà Nội', N'Công nghệ');

-- 2. Thêm phòng ban TTSX (lấy CompanyId vừa tạo)
INSERT INTO DEPARTMENTS (CompanyId, Name)
VALUES (
    (SELECT CompanyId FROM COMPANIES WHERE Name = N'FINT'),
    N'TTSX'
);

-- 3. Thêm nhân viên (gắn vào phòng ban TTSX)
INSERT INTO EMPLOYEES (DepartmentId, FullName, Email, DateOfBirth, HireDate, Password, PasswordHash)
VALUES 
(
    (SELECT DepartmentId FROM DEPARTMENTS WHERE Name = N'TTSX'),
    N'Nguyễn Văn A',
    'vana@fint.com',
    '1995-05-20',
    GETDATE(),
    '123456',
    NULL
),
(
    (SELECT DepartmentId FROM DEPARTMENTS WHERE Name = N'TTSX'),
    N'Trần Thị B',
    'thib@fint.com',
    '1998-10-12',
    GETDATE(),
    '123456',
    NULL
),
(
    (SELECT DepartmentId FROM DEPARTMENTS WHERE Name = N'TTSX'),
    N'Lê Văn C',
    'vanc@fint.com',
    '2000-03-08',
    GETDATE(),
    '123456',
    NULL
);

-- 4. Thêm roles cơ bản
INSERT INTO ROLES (Name) VALUES (N'Admin'), (N'Nhân viên');

-- 5. Gán vai trò cho nhân viên
-- Nguyễn Văn A làm Admin
INSERT INTO EMPLOYEEROLES (EmployeeId, RoleId)
VALUES (
    (SELECT EmployeeId FROM EMPLOYEES WHERE Email = 'vana@fint.com'),
    (SELECT RoleId FROM ROLES WHERE Name = N'Admin')
);

-- Trần Thị B và Lê Văn C làm Nhân viên
INSERT INTO EMPLOYEEROLES (EmployeeId, RoleId)
VALUES 
(
    (SELECT EmployeeId FROM EMPLOYEES WHERE Email = 'thib@fint.com'),
    (SELECT RoleId FROM ROLES WHERE Name = N'Nhân viên')
),
(
    (SELECT EmployeeId FROM EMPLOYEES WHERE Email = 'vanc@fint.com'),
    (SELECT RoleId FROM ROLES WHERE Name = N'Nhân viên')
);

select *from COMPANIES
select *from DEPARTMENTS
select *from EMPLOYEES;
select *from ROLES
select *from QUESTIONS
select *from SKILLS

EXEC sp_help 'EMPLOYEES';
SELECT * FROM EMPLOYEES WHERE Email = 'vana@fint.com' AND Password = '123456';




IF OBJECT_ID('sp_GenerateTest', 'P') IS NOT NULL
    DROP PROCEDURE sp_GenerateTest;
GO
CREATE PROCEDURE sp_GenerateTest
    @TestName NVARCHAR(255),
    @Role NVARCHAR(50),              -- FE, BE, Admin, Tester …
    @Difficulty NVARCHAR(50),        -- Intern, Junior, Senior …
    @SoCauHoi INT,
    @DurationMinutes INT = 60,
    @PassingScore FLOAT = 70,
    @CreatedBy INT = 1               -- Mặc định Admin tạo
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TestId INT;

    -- 1. Luôn tạo mới 1 bài test (không cập nhật test cũ)
    INSERT INTO TESTS (Name, Type, DurationMinutes, PassingScore, CreatedBy)
    VALUES (@TestName, @Role, @DurationMinutes, @PassingScore, @CreatedBy);

    SET @TestId = SCOPE_IDENTITY();

    -- 2. Lấy ngẫu nhiên N câu hỏi theo Role + Difficulty
    ;WITH RandomQuestions AS (
        SELECT TOP (@SoCauHoi) QuestionId
        FROM QUESTIONS
        WHERE Role = @Role AND Difficulty = @Difficulty
        ORDER BY NEWID()
    )
    -- 3. Gắn câu hỏi vào TESTQUESTIONS (luôn insert mới)
    INSERT INTO TESTQUESTIONS (TestId, QuestionId, QuestionOrder)
    SELECT @TestId, QuestionId, ROW_NUMBER() OVER (ORDER BY (SELECT NULL))
    FROM RandomQuestions;

    -- 4. Trả kết quả
    SELECT * FROM TESTS WHERE TestId = @TestId;

    SELECT tq.TestId, q.QuestionId, q.Content, q.Role, q.Type, q.Difficulty
    FROM TESTQUESTIONS tq
    JOIN QUESTIONS q ON tq.QuestionId = q.QuestionId
    WHERE tq.TestId = @TestId;
END;
GO

EXEC sp_GenerateTest 
    @TestName = N'Test BE Junior số 2',
    @Role = N'bE',
    @Difficulty = N'Fresher',
    @SoCauHoi = 5,
    @DurationMinutes = 45,
    @PassingScore = 60,
    @CreatedBy = 1;


select *from TESTS
SELECT * FROM TESTQUESTIONS
select *from QuestionMatching
select *from QuestionDragDrop
select *from EMPLOYEES

select *from EMPLOYEETESTS 