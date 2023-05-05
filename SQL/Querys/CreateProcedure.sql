IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spLogs_NewLog]') AND type in (N'P', N'PC'))
BEGIN
    EXECUTE('CREATE PROCEDURE [dbo].[spLogs_NewLog]
    @Message varchar(MAX), @Date datetime, @Type varchar(10), @Id int = null
    AS
    BEGIN
        INSERT INTO [Logs] ([Message],[Date],[Type])
        VALUES (@Message,@Date,@Type)
    END')
END
