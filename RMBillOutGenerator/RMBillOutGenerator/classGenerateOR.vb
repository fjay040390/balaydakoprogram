Imports MySql.Data.MySqlClient

Public Class classGenerateOR

    Private _OR As String
    Private _TableNo As String
    Private _Cashier As String
    Private _CustNo As String
    Private _SeniorName As String
    Private _SeniorID As String
    Private _BillTIME As String
    Private _isBillOut As Boolean

    Private _fileP As String
    Private Const fileN As String = "BillOR.spl"

#Region "Properties"
    Public Property ORNo() As String
        Get
            Return _OR
        End Get

        Set(ByVal ORNo As String)
            _OR = ORNo
        End Set
    End Property

    Public Property TableNo() As String
        Get
            Return _TableNo
        End Get

        Set(ByVal TableNo As String)
            _TableNo = TableNo
        End Set
    End Property

    Public Property Cashier() As String
        Get
            Return _Cashier
        End Get

        Set(ByVal Cashier As String)
            _Cashier = Cashier
        End Set
    End Property

    Public Property CustNo() As String
        Get
            Return _CustNo
        End Get

        Set(ByVal CustNo As String)
            _CustNo = CustNo
        End Set
    End Property

    Public Property SeniorName() As String
        Get
            Return _SeniorName
        End Get

        Set(ByVal SeniorName As String)
            _SeniorName = SeniorName
        End Set
    End Property

    Public Property SeniorID() As String
        Get
            Return _SeniorID
        End Get

        Set(ByVal SeniorID As String)
            _SeniorID = SeniorID
        End Set
    End Property

    Public Property BillTIME() As String
        Get
            Return _BillTIME
        End Get

        Set(ByVal BillTIME As String)
            _BillTIME = BillTIME
        End Set
    End Property

    Public Property isBillOut() As Boolean
        Get
            Return _isBillOut
        End Get

        Set(ByVal isBillOut As Boolean)
            _isBillOut = isBillOut
        End Set
    End Property

    Public Property fileP() As String
        Get
            Return _fileP
        End Get

        Set(ByVal fileP As String)
            _fileP = fileP
        End Set
    End Property
#End Region

    Sub Print()
        If isBillOut = False Then
            getParamValues()
        End If
        PrintBillOut()
    End Sub

    Protected Function FormatAmount(ByVal str As String)
        Dim outP As String
        outP = FormatNumber(str, 2)
        Return outP
    End Function

    Protected Sub getParamValues()
        sqlCmd = New MySqlCommand("getDatabyOR", sqlCon)
        sqlCmd.CommandType = CommandType.StoredProcedure
        sqlCmd.Parameters.AddWithValue("@orNo", ORNo)
        sqlCmd.Parameters("@orNo").Direction = ParameterDirection.Input
        sqlCmd.Parameters.Add("@empName", MySqlDbType.String)
        sqlCmd.Parameters("@empName").Direction = ParameterDirection.Output
        sqlCmd.Parameters.Add("@tableNo", MySqlDbType.Int16)
        sqlCmd.Parameters("@tableNo").Direction = ParameterDirection.Output
        sqlCmd.Parameters.Add("@custCount", MySqlDbType.Int16)
        sqlCmd.Parameters("@custCount").Direction = ParameterDirection.Output
        sqlCmd.Parameters.Add("@id", MySqlDbType.String)
        sqlCmd.Parameters("@id").Direction = ParameterDirection.Output
        sqlCmd.Parameters.Add("@cName", MySqlDbType.String)
        sqlCmd.Parameters("@cName").Direction = ParameterDirection.Output
        sqlCmd.Parameters.Add("@billTime", MySqlDbType.String)
        sqlCmd.Parameters("@billTime").Direction = ParameterDirection.Output
        sqlCmd.ExecuteNonQuery()
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        Cashier = sqlCmd.Parameters("@empName").Value.ToString
        TableNo = sqlCmd.Parameters("@tableNo").Value.ToString
        CustNo = sqlCmd.Parameters("@custCount").Value.ToString
        SeniorID = sqlCmd.Parameters("@id").Value.ToString
        SeniorName = sqlCmd.Parameters("@cName").Value.ToString
        BillTIME = sqlCmd.Parameters("@billTime").Value.ToString
    End Sub

    Protected Sub PrintBillOut()
        Dim sw As New System.IO.StreamWriter(fileP & fileN, False)
        Dim qty As Integer
        Dim itemName As String
        Dim itemPrice As String
        Dim subTotal, sPreTax, lessVat, seniorDiscount, serviceCharge, vat, amountDue, seniorAmount As String
        Dim seniorCount As Integer
        With sw
            PrintHeader(sw)
            .WriteLine()
            .WriteLine("****************************************")
            .WriteLine(Space((40 - Len("SENIOR BILL OUT")) / 2) & "SENIOR BILL OUT")
            .WriteLine("****************************************")
            If isBillOut = True Then
                .WriteLine("Bill No: " & ORNo)
            Else
                .WriteLine("Receipt No: " & ORNo)
            End If
            .WriteLine("Table: " & TableNo)
            .WriteLine("Bill Time: " & BillTIME)
            .WriteLine("Server:" & Cashier & Space(40 - Len("Server:" & Cashier & "Cust:" & CustNo)) & "Cust:" & CustNo)
            .WriteLine("----------------------------------------")
            'Sales Detail
            sqlCmd = New MySqlCommand("Select * from sales_detail where or_no = " & ORNo, sqlCon)
            sqlDR = sqlCmd.ExecuteReader()
            If sqlDR.HasRows Then
                While sqlDR.Read
                    qty = FormatAmount(sqlDR(3).ToString)
                    itemName = Space(12 - Len(qty)) & sqlDR(2).ToString
                    itemPrice = Space(39 - Len(itemName & FormatAmount(sqlDR(4).ToString))) & FormatAmount(sqlDR(4).ToString)
                    .WriteLine(qty & itemName & itemPrice)
                End While
                qty = 0
                itemName = ""
                itemPrice = ""
            End If
            sqlDR.Close()
            .WriteLine("----------------------------------------")

            'Sales Summary
            sqlCmd = New MySqlCommand("Select * from sales_summary where or_no = " & ORNo, sqlCon)
            sqlDR = sqlCmd.ExecuteReader()
            If sqlDR.HasRows Then
                While sqlDR.Read
                    'Sub Total
                    subTotal = sqlDR("trans_gross").ToString
                    'less VAT
                    lessVat = sqlDR("less_vat").ToString
                    'Vat
                    vat = sqlDR("vat").ToString
                    'Sub Total Pre Tax
                    sPreTax = subTotal - vat
                    'Senior Count
                    seniorCount = sqlDR("count").ToString
                    'Senior Amount
                    seniorAmount = sqlDR("amount").ToString
                    'Senior Discount
                    seniorDiscount = sqlDR("discount").ToString
                    'Service Charge
                    serviceCharge = (sPreTax - seniorDiscount) * 0.1
                    'Amount Due
                    amountDue = (subTotal - seniorDiscount) + serviceCharge
                End While
            End If
            sqlDR.Close()
            .WriteLine()
            .WriteLine(Space(9) & "Sub Total:" & Space(40 - Len(Space(9) & "Sub Total:" & FormatAmount(subTotal))) & FormatAmount(subTotal))
            '.WriteLine(Space(9) & "Less VAT:" & Space(39 - Len(Space(9) & "Less VAT" & FormatAmount(vat * -1))) & FormatAmount(vat * -1))
            '.WriteLine(Space(9) & "Sub Total Pre Tax:" & Space(40 - Len(Space(9) & "Sub Total Pre Tax:" & FormatAmount(sPreTax))) & FormatAmount(sPreTax))
            .WriteLine(Space(9) & "Senior Discount" & "(" & seniorCount & "):" & Space(40 - Len(Space(9) & "Senior Discount" & "(" & seniorCount & "):" & FormatAmount(seniorDiscount * -1))) & FormatAmount(seniorDiscount * -1))
            '.WriteLine(Space(9) & "Senior Less VAT" & "(" & seniorCount & "):" & Space(40 - Len(Space(9) & "Senior Less VAT" & "(" & seniorCount & "):" & FormatAmount(lessVat * -1))) & FormatAmount(lessVat * -1))
            .WriteLine(Space(9) & "Service Charge:" & Space(40 - Len(Space(9) & "Service Charge:" & FormatAmount(serviceCharge))) & FormatAmount(serviceCharge))
            '.WriteLine(Space(9) & "12% VAT:" & Space(40 - Len(Space(9) & "12% VAT:" & FormatAmount(vat - lessVat))) & FormatAmount(vat - lessVat))
            .WriteLine()
            .WriteLine("****************************************")
            .WriteLine(Space(9) & "AMOUNT DUE:" & Space(40 - Len(Space(9) & "AMOUNT DUE:" & FormatAmount(amountDue))) & FormatAmount(amountDue))
            .WriteLine("****************************************")
            .WriteLine()
            .WriteLine("Signature: _____________________________")
            .WriteLine("Name     : " & SeniorName)
            .WriteLine("Senior ID: " & SeniorID)
            While seniorCount > 1
                .WriteLine()
                .WriteLine("Signature: _____________________________")
                .WriteLine("Name     : ")
                .WriteLine("Senior ID: ")
                seniorCount -= 1
            End While
            .WriteLine()
            .WriteLine()
            If isBillOut = False Then
                .WriteLine()
                .WriteLine("Sales Summary")
                .WriteLine("----------------------------------------")
                .WriteLine(Space(9) & "VAT Sale:" & Space(40 - Len(Space(9) & "VAT Sale:" & FormatAmount((subTotal - seniorAmount) / 1.12))) & FormatAmount((subTotal - seniorAmount) / 1.12))
                .WriteLine(Space(9) & "12% VAT:" & Space(40 - Len(Space(9) & "12% VAT:" & FormatAmount(vat - lessVat))) & FormatAmount(vat - lessVat))
                .WriteLine(Space(9) & "VAT Exempt Sales:" & Space(40 - Len(Space(9) & "VAT Exempt Sales:" & FormatAmount(seniorAmount - lessVat))) & FormatAmount(seniorAmount - lessVat))
                .WriteLine(Space(9) & "Zero Rated:" & Space(40 - Len(Space(9) & "Zero Rated:" & "0.00")) & "0.00")
                .WriteLine("----------------------------------------")
                .WriteLine()
                .WriteLine()
                PrintFooter(sw)
            End If
            .WriteLine()
            .WriteLine()
            .WriteLine()
            .WriteLine()
            .WriteLine()
            .Close() 'Close File
        End With
    End Sub

    Protected Sub PrintHeader(ByVal mainSW As System.IO.StreamWriter)
        Dim headerSW As New System.IO.StreamReader(Application.StartupPath & "\header.spl")
        Dim headerSTR As String
        Do While headerSW.Peek
            headerSTR = headerSW.ReadLine()
            If headerSTR = Nothing Then Exit Do
            mainSW.WriteLine(Space((40 - Len(headerSTR)) / 2) & headerSTR)
        Loop
        headerSW.Close()
    End Sub

    Protected Sub PrintFooter(ByVal mainSW As System.IO.StreamWriter)
        Dim headerSW As New System.IO.StreamReader(Application.StartupPath & "\footer.spl")
        Dim headerSTR As String
        Do While headerSW.Peek
            headerSTR = headerSW.ReadLine()
            If headerSTR = Nothing Then Exit Do
            mainSW.WriteLine(Space((40 - Len(headerSTR)) / 2) & headerSTR)
        Loop
        headerSW.Close()
    End Sub
End Class
