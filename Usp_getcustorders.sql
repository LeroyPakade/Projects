USE [Northwind]

go 

/****** Object:  StoredProcedure [dbo].[usp_GetCustOrders]    
SET ansi_nulls ON 

go 

SET quoted_identifier ON 

go 

go 

IF Object_id('dbo.usp_GetCustOrders', 'P') IS NOT NULL 
  DROP PROCEDURE dbo.usp_getcustorders; 

go 

CREATE PROCEDURE [dbo].[Usp_getcustorders] @custid   AS NCHAR(5), 
                                           @emplid   AS NCHAR(5), 
                                           @fromdate AS DATETIME, 
                                           @todate   AS DATETIME 
AS 
    SET nocount ON; 

  BEGIN 
      SELECT Concat(Employee.titleofcourtesy, ' ', Employee.firstname, 
             ' ', Employee.lastname)                                    AS 
             EmployeeFullName, 
             [dbo].[shippers].[companyname]                             AS 
             shippersCompanyName, 
             [dbo].[customers].[companyname]                            AS 
             CompanyName 
             , 
             orderdate, 
             Sum(freight)                                               AS 
             TotalFreighCost, 
             Sum(CustD.unitprice * CustD.quantity - ( CustD.discount )) AS 
             TotalOrderValue, 
             Count(DISTINCT ProdDetails.productname)                    AS 
             NumberOfDifferntProducts, 
             Count([dbo].[orders].orderid)                              AS 
             NumberOfOrder 
      FROM   [dbo].[orders] 
             LEFT OUTER JOIN [dbo].[customers] 
                          ON [dbo].[orders].[customerid] = 
                             [dbo].[customers].[customerid] 
             LEFT OUTER JOIN [dbo].[employees] AS Employee 
                          ON [dbo].[orders].[employeeid] = Employee.[employeeid] 
             LEFT OUTER JOIN [dbo].[shippers] 
                          ON [dbo].[orders].[shipvia] = 
                             [dbo].[shippers].[shipperid] 
             LEFT OUTER JOIN [dbo].[order details] AS CustD 
                          ON [dbo].[orders].orderid = CustD.orderid 
             LEFT OUTER JOIN [dbo].[products] AS ProdDetails 
                          ON CustD.[productid] = ProdDetails.productid 
      WHERE  orderdate BETWEEN CONVERT(VARCHAR, @fromdate, 13) AND 
                                      CONVERT(VARCHAR, @todate, 13) 
             AND [dbo].[orders].[customerid] = Isnull(@custid, 
                                               [dbo].[orders].[customerid]) 
             AND [dbo].[orders].[employeeid] = Isnull(@emplid, 
                                               [dbo].[orders].[employeeid]) 
      GROUP  BY [dbo].[shippers].[companyname], 
                [dbo].[customers].[companyname], 
                Concat(Employee.titleofcourtesy, ' ', Employee.firstname, ' ', 
                Employee.lastname), 
                orderdate 
  END 