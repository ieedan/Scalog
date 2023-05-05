IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Logs')
BEGIN
    CREATE TABLE [dbo].[Logs](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Date] datetime NOT NULL,
        [Message] varchar(MAX) NOT NULL,
        [Type] varchar(10) NOT NULL,
        CONSTRAINT [PK_Logs] PRIMARY KEY CLUSTERED 
        (
            [Id] ASC
        )WITH (
            PAD_INDEX = OFF, 
            STATISTICS_NORECOMPUTE = OFF, 
            IGNORE_DUP_KEY = OFF, 
            ALLOW_ROW_LOCKS = ON, 
            ALLOW_PAGE_LOCKS = ON, 
            OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF
        ) ON [PRIMARY]
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END