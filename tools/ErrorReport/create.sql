USE [master]
GO
/****** Object:  Database [ErrorReports]    Script Date: 03/08/2006 19:33:29 ******/
CREATE DATABASE [ErrorReports] ON  PRIMARY 
( NAME = N'ErrorReports', FILENAME = N'D:\mssql\data\ErrorReports.mdf' , SIZE = 10240KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'ErrorReports_log', FILENAME = N'D:\mssql\log\ErrorReports_log.ldf' , SIZE = 16576KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
 COLLATE Danish_Norwegian_CI_AS
GO
EXEC dbo.sp_dbcmptlevel @dbname=N'ErrorReports', @new_cmptlevel=90
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [ErrorReports].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [ErrorReports] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [ErrorReports] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [ErrorReports] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [ErrorReports] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [ErrorReports] SET ARITHABORT OFF 
GO
ALTER DATABASE [ErrorReports] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [ErrorReports] SET AUTO_CREATE_STATISTICS ON 
GO
ALTER DATABASE [ErrorReports] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [ErrorReports] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [ErrorReports] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [ErrorReports] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [ErrorReports] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [ErrorReports] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [ErrorReports] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [ErrorReports] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [ErrorReports] SET  ENABLE_BROKER 
GO
ALTER DATABASE [ErrorReports] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [ErrorReports] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [ErrorReports] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [ErrorReports] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [ErrorReports] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [ErrorReports] SET  READ_WRITE 
GO
ALTER DATABASE [ErrorReports] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [ErrorReports] SET  MULTI_USER 
GO
ALTER DATABASE [ErrorReports] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [ErrorReports] SET DB_CHAINING OFF 

USE ErrorReports;
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ErrorReportItems]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ErrorReportItems](
	[ID] [nvarchar](250) NOT NULL,
	[ReceivedTime] [datetime] NULL,
	[SubmitterEmail] [nvarchar](max) NULL,
	[Body] [text] NULL,
	[Subject] [nvarchar](max) NULL,
	[ExceptionType] [nvarchar](max) NULL,
	[ExceptionMessage] [text] NULL,
	[StackTrace] [text] NULL,
	[MajorVersion] [int] NULL,
	[MinorVersion] [int] NULL,
	[PatchVersion] [int] NULL,
	[Revision] [int] NULL,
	[RepliedTo] [bit] NOT NULL,
 CONSTRAINT [PK_ErrorReportItems] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StackTraceLines]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[StackTraceLines](
	[ErrorReportItem] [nvarchar](250) NOT NULL,
	[MethodName] [nvarchar](max) NULL,
	[Parameters] [nvarchar](max) NULL,
	[Filename] [nvarchar](max) NULL,
	[LineNumber] [int] NULL,
	[SequenceNumber] [int] NOT NULL
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ImportErrorItem]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[ImportErrorItem] 
	-- Add the parameters for the stored procedure here
	@ID nvarchar(250) = null, 
	@ReceivedTime DateTime = null,
	@SubmitterEmail nvarchar(max) = null,
	@Body Text = null,
	@Subject nvarchar(max) = null,
	@ExceptionType nvarchar(max) = null,
	@ExceptionMessage text = null,
	@StackTrace text = null,
	@MajorVersion int = null,
	@MinorVersion int = null,
	@PatchVersion int = null,
	@Revision int = null,
	@RepliedTo bit
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF (SELECT COUNT(*) FROM ErrorReportItems WHERE ID=@ID) = 0
	BEGIN
		INSERT INTO ErrorReportItems 
			(ID, ReceivedTime, SubmitterEmail,
				Body, Subject, ExceptionType, 
				ExceptionMessage, StackTrace, MajorVersion,
				MinorVersion, PatchVersion, Revision, RepliedTo)
			VALUES (@ID, @ReceivedTime, @SubmitterEmail, 
					@Body, @Subject, @ExceptionType, 
					@ExceptionMessage, @StackTrace, @MajorVersion,
					@MinorVersion, @PatchVersion, @Revision, @RepliedTo);
		SELECT 1
	END
	ELSE
	BEGIN
		SELECT 0
	END
END

' 
END
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ErrorReportItems_ErrorReportItems]') AND parent_object_id = OBJECT_ID(N'[dbo].[ErrorReportItems]'))
ALTER TABLE [dbo].[ErrorReportItems]  WITH CHECK ADD  CONSTRAINT [FK_ErrorReportItems_ErrorReportItems] FOREIGN KEY([ID])
REFERENCES [dbo].[ErrorReportItems] ([ID])
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_StackTraceLines_ErrorReportItems]') AND parent_object_id = OBJECT_ID(N'[dbo].[StackTraceLines]'))
ALTER TABLE [dbo].[StackTraceLines]  WITH CHECK ADD  CONSTRAINT [FK_StackTraceLines_ErrorReportItems] FOREIGN KEY([ErrorReportItem])
REFERENCES [dbo].[ErrorReportItems] ([ID])
