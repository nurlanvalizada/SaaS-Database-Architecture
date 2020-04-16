using System;
using System.Data.SqlClient;

namespace Saas_Data_Model.DBHandler
{
    public partial class MainDataModel
    {
        public static void TenantProvisioning(string databaseName, string databaseUsername, string databasePassword, string connectionString)
        {
            using (var con = new SqlConnection(connectionString))
            {
                var cmdDatabaseCreate = new SqlCommand(@"CREATE DATABASE " + databaseName, con);
                //cmdDatabaseCreate.Parameters.AddWithValue("@databaseName", databaseName);

                var cmdDatabaseUserCreate = new SqlCommand(@"CREATE LOGIN " + databaseUsername + " WITH PASSWORD = '" + databasePassword + @"', CHECK_POLICY = OFF 
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'" + databaseUsername + @"')
BEGIN
    CREATE USER " + databaseUsername + " FOR LOGIN " + databaseUsername + @"
    EXEC sp_addrolemember N'db_owner', N'" + databaseUsername + @"'
END;", con);
                //cmdDatabaseUserCreate.Parameters.AddWithValue("@defaultUser", databaseUsername);
                //cmdDatabaseUserCreate.Parameters.AddWithValue("@defaultUserPassword", databasePassword);
                con.Open();
                cmdDatabaseCreate.ExecuteNonQuery();
                con.Close();


                con.ConnectionString = String.Format("Data Source=127.0.0.1;Initial Catalog={0};Integrated Security=True", databaseName);
                con.Open();
                cmdDatabaseUserCreate.ExecuteNonQuery();
                con.Close();
                con.ConnectionString = String.Format("Data Source=127.0.0.1;Initial Catalog={0};User Id = {1}; Password = {2}", databaseName, databaseUsername, databasePassword);
                con.Open();
                var cmd = new SqlCommand();
                #region old
               /* var query = @"CREATE TABLE [dbo].[Director](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FirstNameDirector] [nvarchar](50) NOT NULL,
	[LastNameDirector] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


CREATE TABLE [dbo].[Director1](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FirstNameDirector] [nvarchar](50) NOT NULL,
	[LastNameDirector] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


CREATE TABLE [dbo].[Employee](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FirstName] [nvarchar](50) NOT NULL,
	[LastName] [nvarchar](50) NULL,
	[Salary] [int] NULL,
	[ManagerID] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[Manager](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[salam] [nvarchar](max) NULL,
	[salam1] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]


CREATE TABLE [dbo].[Manager1](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FirstName] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


CREATE TABLE [dbo].[Professor](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FirstNameProfessor] [nvarchar](50) NOT NULL,
	[LastNameProfessor] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[MetaData](
	[ID] [int] primary key IDENTITY(1,1) NOT NULL,
	[TableName] [nvarchar](50) NOT NULL,
	[FieldName] [nvarchar](50) NOT NULL,
[FieldType] [nvarchar](50) NOT NULL
)

  CREATE TABLE [dbo].[_MigrationHistory](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SenderTable] [nvarchar](50) NOT NULL,
	[ReceiverTable] [nvarchar](50) NULL,
	[SentColumn] [nvarchar](50) NULL,
	[DateTime] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


CREATE TABLE [dbo].[Professor1](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[LastNameProfessor] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


ALTER TABLE [dbo].[Employee]  WITH CHECK ADD FOREIGN KEY([ManagerID])
REFERENCES [dbo].[Manager] ([ID])

";


                const string query2 = @"CREATE TABLE [dbo].[MetaData](
	[ID] [int] primary key IDENTITY(1,1) NOT NULL,
	[TableName] [nvarchar](50) NOT NULL,
	[FieldName] [nvarchar](50) NOT NULL,
[FieldType] [nvarchar](50) NOT NULL
)

Create Table Employee
(
EmployeeID int primary key identity,
Name nvarchar(50) not null,
Position nvarchar(50),
test1 varchar(20),
test2 varchar(20),
test3 varchar(20),
test4 varchar(20),
test5 varchar(20),
test6 varchar(20),
ManagerID int not null
)

Create Table Manager
(
ManagerID int primary key identity,
Name nvarchar(50) not null,
testt1 varchar(20),
testt2 varchar(20),
testt3 varchar(20),
testt4 varchar(20),
testt5 varchar(20),
testt6 varchar(20),
DirectorID int not null
)

Create Table Director
(
DirectorID int primary key identity,
Name nvarchar(50) not null,
testtt1 varchar(20),
testtt2 varchar(20),
testtt3 varchar(20),
testtt4 varchar(20),
testtt5 varchar(20),
testtt6 varchar(20),
)

Create Table Salary
(
EmployeeID int primary key identity,
SalaryType nvarchar(50) not null,
Amount nvarchar(50) not null,
testttd1 varchar(20),
testttd2 varchar(20),
testttd3 varchar(20),
testttd4 varchar(20),
testttd5 varchar(20),
testttd6 varchar(20),
)


  CREATE TABLE [dbo].[_MigrationHistory](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SenderTable] [nvarchar](50) NOT NULL,
	[ReceiverTable] [nvarchar](50) NULL,
	[SentColumn] [nvarchar](50) NULL,
	[DateTime] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

alter table Employee add constraint FK_Employee_Manager_ID foreign key(ManagerID) references Manager(ManagerID)

alter table Manager add constraint FK_Manager_Director_ID foreign key(DirectorID) references Director(DirectorID)

alter table Salary add constraint FK_Salary_Employee_ID foreign key(EmployeeID) references Employee(EmployeeID)
";*/
                #endregion

                const string query = @"CREATE TABLE [dbo].[_MigrationHistory](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SenderTable] [nvarchar](50) NOT NULL,
	[ReceiverTable] [nvarchar](50) NULL,
	[SentColumn] [nvarchar](50) NULL,
	[DateTime] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


CREATE TABLE [dbo].[Course](
	[CourseId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Duration] [nvarchar](50) NOT NULL,
	[FeeId] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[CourseId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


CREATE TABLE [dbo].[CourseFee](
	[FeeId] [int] IDENTITY(1,1) NOT NULL,
	[Amount] [nvarchar](50) NOT NULL,
	[FeeType] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[FeeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


CREATE TABLE [dbo].[Grade](
	[StdId] [int] NOT NULL,
	[CrsId] [int] NOT NULL,
	[GradeLetter] [nvarchar](50) NOT NULL,
	[Grade] [nvarchar](50) NULL
) ON [PRIMARY]


CREATE TABLE [dbo].[MetaData](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TableName] [nvarchar](50) NOT NULL,
	[FieldName] [nvarchar](50) NOT NULL,
	[FieldType] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


CREATE TABLE [dbo].[Salary](
	[TeacherId] [int] NOT NULL,
	[Amount] [float] NOT NULL,
	[Type] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[TeacherId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


CREATE TABLE [dbo].[Student](
	[StudentId] [int] IDENTITY(1,1) NOT NULL,
	[FirstName] [nvarchar](50) NOT NULL,
	[LastName] [nvarchar](50) NOT NULL,
	[BirthDate] [datetime] NOT NULL,
	[BirthPlace] [nvarchar](70) NOT NULL,
	[Nationality] [nvarchar](30) NULL,
	[TeacherID] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[StudentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


CREATE TABLE [dbo].[Teacher](
	[TeacherId] [int] IDENTITY(1,1) NOT NULL,
	[TeacherFirstName] [nvarchar](50) NOT NULL,
	[TeacherLastName] [nvarchar](50) NOT NULL,
	[BirthDate] [datetime] NOT NULL,
	[Nationality] [nvarchar](30) NULL,
PRIMARY KEY CLUSTERED 
(
	[TeacherId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


ALTER TABLE [dbo].[Course]  WITH CHECK ADD  CONSTRAINT [FK_FeeId_CourseFee] FOREIGN KEY([FeeId])
REFERENCES [dbo].[CourseFee] ([FeeId])

ALTER TABLE [dbo].[Course] CHECK CONSTRAINT [FK_FeeId_CourseFee]

ALTER TABLE [dbo].[Grade]  WITH CHECK ADD  CONSTRAINT [FK_CrsId_Student] FOREIGN KEY([CrsId])
REFERENCES [dbo].[Course] ([CourseId])

ALTER TABLE [dbo].[Grade] CHECK CONSTRAINT [FK_CrsId_Student]

ALTER TABLE [dbo].[Grade]  WITH CHECK ADD  CONSTRAINT [FK_StdId_Student] FOREIGN KEY([StdId])
REFERENCES [dbo].[Student] ([StudentId])

ALTER TABLE [dbo].[Grade] CHECK CONSTRAINT [FK_StdId_Student]

ALTER TABLE [dbo].[Student]  WITH CHECK ADD  CONSTRAINT [FK_TeacherId_Teacher] FOREIGN KEY([TeacherID])
REFERENCES [dbo].[Teacher] ([TeacherId])

ALTER TABLE [dbo].[Student] CHECK CONSTRAINT [FK_TeacherId_Teacher]

ALTER TABLE Salary add constraint FK_TeacherId_Salary foreign key(TeacherId) references Teacher(TeacherID)
";



                cmd.Connection = con;
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
            }


        }
    }
}
