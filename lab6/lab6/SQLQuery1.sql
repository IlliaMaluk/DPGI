BULK INSERT dbo.Words
FROM 'C:\Data\words.txt'
WITH (
    DATAFILETYPE = 'char',
    FIELDTERMINATOR = '\n',
    ROWTERMINATOR   = '\n',
    FIRSTROW        = 1
);
