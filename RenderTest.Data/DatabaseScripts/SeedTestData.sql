DECLARE @RowCount INT = 0 
DECLARE @RowLimit INT = 3000000 --Seed 3M rows. 
DECLARE @RowString VARCHAR(10) 
DECLARE @Random INT 
DECLARE @Upper INT 
DECLARE @Lower INT 
DECLARE @InsertDate DATETIME 
DECLARE @AllChars VARCHAR(100) = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789' 

SET @Lower = -730 
SET @Upper = -1 

WHILE @RowCount < @RowLimit 
  BEGIN 
      SET @RowString = Cast(@RowCount AS VARCHAR(10)) 

      SELECT @Random = Round(( ( @Upper - @Lower - 1 ) * Rand() + @Lower ), 0) 

      SET @InsertDate = Dateadd(dd, @Random, Getdate()) 

      INSERT INTO [testtable] 
                  (id, 
                   mystringfield, 
                   mydatefield, 
                   myboolfield, 
                   myintfield, 
                   mymoneyfield) 
      VALUES      ( Newid(), 
                    (SELECT RIGHT(LEFT(@AlLChars, Abs(Binary_checksum(Newid()) % 
                            35) + 
                            1), 1) 
                            + RIGHT(LEFT(@AlLChars, Abs(Binary_checksum(Newid()) 
                            % 
                            35) 
                            + 1), 1) 
                            + RIGHT(LEFT(@AlLChars, Abs(Binary_checksum(Newid()) 
                            % 
                            35) 
                            + 1), 1) 
                            + RIGHT(LEFT(@AlLChars, Abs(Binary_checksum(Newid()) 
                            % 
                            35) 
                            + 1), 1) 
                            + RIGHT(LEFT(@AlLChars, Abs(Binary_checksum(Newid()) 
                            % 
                            35) 
                            + 1), 1)), 
                    Dateadd(dd, 1, @InsertDate), 
                    Cast(Round(Rand(), 0) AS BIT), 
                    Cast(Rand() * 1000000 AS INT), 
                    Abs(Checksum(Newid())) % 14 ) 

      SET @RowCount = @RowCount + 1 
  END 