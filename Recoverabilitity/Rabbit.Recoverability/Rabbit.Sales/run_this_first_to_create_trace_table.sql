-- First create a database called NsbRabbitMqRecoverability

USE [NsbRabbitMqRecoverability]
GO

CREATE TABLE [dbo].[RetriesTrace](
	[RetriesTraceId] [int] IDENTITY(1,1) NOT NULL,
	[OrderId] [int] NOT NULL,
	[AppEntryTime] [datetime] NOT NULL,
	[DbEntryTime] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[RetriesTraceId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
)