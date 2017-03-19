WITH CTE AS (
SELECT ROW_NUMBER() OVER (PARTITION BY OrderId ORDER BY RetriesTraceId) AS RowNo
	  ,[RetriesTraceId]
      ,[OrderId]
      ,[AppEntryTime]
      ,[DbEntryTime]
  FROM [NsbRabbitMqRecoverability].[dbo].[RetriesTrace]
)

SELECT CTE1.*, DATEDIFF(SECOND, CTE2.AppEntryTime, CTE2.AppEntryTime) AS SecondsDelay
FROM CTE as CTE1
JOIN CTE as CTE2 ON CTE1.OrderId = CTE2.OrderId
	AND CTE1.RowNo - 1 = CTE2.RowNo