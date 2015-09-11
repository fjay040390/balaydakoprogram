Imports MySql.Data.MySqlClient

Public Class classGenerateOR

    Private _OR As String
    Private TableNo As String
    Private Cashier As String
    Private CustNo As String
    Private SeniorName As String
    Private SeniorID As String
    Private SettleTIME As String
    Private TransType As Integer
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
        sqlCmd = New MySqlCommand("getDatabyORSTL", sqlCon)
        sqlCmd.CommandType = CommandType.StoredProcedure
        sqlCmd.Parameters.AddWithValue("@orNo", ORNo)
        sqlCmd.Parameters("@orNo").Direction = ParameterDirection.Input
        sqlCmd.Parameters.Add("@empName", MySqlDbType.String)
        sqlCmd.Parameters("@empName").Direction = ParameterDirection.Output
        sqlCmd.Parameters.Add("@tableNo", MySqlDbType.Int16)
        sqlCmd.Parameters("@tableNo").Direction = ParameterDirection.Output
        sqlCmd.Parameters.Add("@count", MySqlDbType.Int16)
        sqlCmd.Parameters("@count").Direction = ParameterDirection.Output
        sqlCmd.Parameters.Add("@custID", MySqlDbType.String)
        sqlCmd.Parameters("@custID").Direction = ParameterDirection.Output
        sqlCmd.Parameters.Add("@custName", MySqlDbType.String)
        sqlCmd.Parameters("@custName").Direction = ParameterDirection.Output
        sqlCmd.Parameters.Add("@settleTIME", MySqlDbType.String)
        sqlCmd.Parameters("@settleTIME").Direction = ParameterDirection.Output
        sqlCmd.Parameters.Add("@transType", MySqlDbType.String)
        sqlCmd.Parameters("@transType").Direction = ParameterDirection.Output
        sqlCmd.ExecuteNonQuery()
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        Cashier = sqlCmd.Parameters("@empName").Value.ToString
        TableNo = sqlCmd.Parameters("@tableNo").Value.ToString
        CustNo = sqlCmd.Parameters("@count").Value.ToString
        SeniorID = sqlCmd.Parameters("@custID").Value.ToString
        SeniorName = sqlCmd.Parameters("@custName").Value.ToString
        SettleTIME = sqlCmd.Parameters("@settleTIME").Value.ToString
        TransType = sqlCmd.Parameters("@transType").Value.ToString
    End Sub

    Protected Sub PrintBillOut()
        Dim sw As New System.IO.StreamWriter(fileP & fileN, False)
        Dim qty As Integer
        Dim itemName As String
        Dim itemPrice As String
        Dim subTotal, sPreTax, lessVat, seniorDiscount, serviceCharge, vat, amountDue, seniorAmount As Decimal
        Dim seniorCount As Integer
        With sw
            PrintHeader(sw)
            .WriteLine("****************************************")
            If TransType = 1 Then
                .WriteLine(Space((40 - Len("SENIOR CITIZEN")) / 2) & "SENIOR CITIZEN")
            ElseIf TransType = 2 Then
                .WriteLine(Space((40 - Len("DIPLOMAT")) / 2) & "DIPLOMAT")
            ElseIf TransType = 3 Then
                .WriteLine(Space((40 - Len("PWD")) / 2) & "PWD")
            End If
            .WriteLine("****************************************")
            If isBillOut = True Then
                .WriteLine("Bill No: " & ORNo)
            Else
                .WriteLine("Receipt No: " & ORNo)
            End If
            .WriteLine("Table: " & TableNo)
            .WriteLine("Settle Time: " & SettleTIME)
            .WriteLine("Server:" & Cashier & Space(40 - Len("Server:" & Cashier & "Cust:" & CustNo)) & "Cust:" & CustNo)
            .WriteLine("----------------------------------------")
            'Sales Detail
            sqlCmd = New MySqlCommand("Select * from sales_detail where or_no = " & ORNo, sqlCon)
            sqlDR = sqlCmd.ExecuteReader()
            If sqlDR.HasRows Then
                While sqlDR.Read
                    qty = FormatNumber(sqlDR(3).ToString, 2)
                    itemName = Space(12 - Len(qty)) & sqlDR(2).ToString
                    itemPrice = Space(39 - Len(itemName & FormatNumber(sqlDR(4).ToString, 2))) & FormatNumber(sqlDR(4).ToString, 2)
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
                    If TransType = 1 Then
                        subTotal = sqlDR("trans_gross").ToString
                        lessVat = sqlDR("less_vat").ToString
                        vat = sqlDR("vat").ToString
                        sPreTax = subTotal - vat
                        seniorCount = sqlDR("count").ToString
                        seniorAmount = sqlDR("amount").ToString
                        seniorDiscount = sqlDR("discount").ToString
                        serviceCharge = (sPreTax - seniorDiscount) * 0.1
                        amountDue = (subTotal - seniorDiscount) + serviceCharge
                    ElseIf TransType = 2 Then
                        subTotal = sqlDR("trans_gross").ToString
                        lessVat = sqlDR("less_vat").ToString
                        vat = sqlDR("vat").ToString
                        sPreTax = subTotal - vat
                        seniorCount = sqlDR("count").ToString
                        seniorAmount = sqlDR("amount").ToString
                        seniorDiscount = sqlDR("discount").ToString
                        serviceCharge = (sPreTax - lessVat) * 0.1
                        amountDue = (subTotal - lessVat) + serviceCharge
                    ElseIf TransType = 3 Then
                        subTotal = sqlDR("trans_gross").ToString
                        lessVat = sqlDR("less_vat").ToString
                        vat = sqlDR("vat").ToString
                        sPreTax = subTotal - vat
                        seniorCount = sqlDR("count").ToString
                        seniorAmount = sqlDR("amount").ToString
                        seniorDiscount = sqlDR("discount").ToString
                        serviceCharge = (sPreTax - seniorDiscount) * 0.1
                        amountDue = (subTotal - seniorDiscount) + serviceCharge
                    End If
                End While
            End If
            sqlDR.Close()
            .WriteLine()
            If TransType = 1 Then
                .WriteLine(Space(9) & "Sub Total:" & Space(40 - Len(Space(9) & "Sub Total:" & FormatAmount(subTotal))) & FormatAmount(subTotal))
                .WriteLine(Space(9) & "Senior Discount" & "(" & seniorCount & "):" & Space(40 - Len(Space(9) & "Senior Discount" & "(" & seniorCount & "):" & FormatAmount(seniorDiscount * -1))) & FormatAmount(seniorDiscount * -1))
                .WriteLine(Space(9) & "Service Charge:" & Space(40 - Len(Space(9) & "Service Charge:" & FormatAmount(serviceCharge))) & FormatAmount(serviceCharge))
            ElseIf TransType = 2 Then
                .WriteLine(Space(9) & "Sub Total:" & Space(40 - Len(Space(9) & "Sub Total:" & FormatAmount(subTotal))) & FormatAmount(subTotal))
                .WriteLine(Space(9) & "Less VAT:" & "(" & seniorCount & "):" & Space(39 - Len(Space(9) & "Less VAT" & "(" & seniorCount & "):" & FormatAmount(lessVat * -1))) & FormatAmount(lessVat * -1))
                .WriteLine(Space(9) & "Service Charge:" & Space(40 - Len(Space(9) & "Service Charge:" & FormatAmount(serviceCharge))) & FormatAmount(serviceCharge))
            ElseIf TransType = 3 Then
                .WriteLine(Space(9) & "Sub Total:" & Space(40 - Len(Space(9) & "Sub Total:" & FormatAmount(subTotal))) & FormatAmount(subTotal))
                .WriteLine(Space(9) & "PWD Discount" & "(" & seniorCount & "):" & Space(40 - Len(Space(9) & "PWD Discount" & "(" & seniorCount & "):" & FormatAmount(seniorDiscount * -1))) & FormatAmount(seniorDiscount * -1))
                .WriteLine(Space(9) & "Service Charge:" & Space(40 - Len(Space(9) & "Service Charge:" & FormatAmount(serviceCharge))) & FormatAmount(serviceCharge))
            End If
            .WriteLine()
            .WriteLine("****************************************")
            .WriteLine(Space(9) & "AMOUNT DUE:" & Space(40 - Len(Space(9) & "AMOUNT DUE:" & FormatAmount(amountDue))) & FormatAmount(amountDue))
            .WriteLine("****************************************")
            .WriteLine()
            .WriteLine()
            openDBFconnection()
            Dim rs As ADODB.Recordset
            Dim cashBack As String
            rs = con.Execute("Select a.RECEIVED, a.CASH_BACK, b.TPAGO_NAME from PMT" & date_sfx & _
                             " a LEFT JOIN TIPOPAG b on a.PAY_TYPE = b.TPAGO_NO where a.bill_no=" & ORNo)
            While Not rs.EOF
                .WriteLine(rs.Fields(2).Value & Space(40 - Len(rs.Fields(2).Value & FormatNumber(rs.Fields(0).Value, 2))) & FormatNumber(rs.Fields(0).Value, 2))
                cashBack = rs.Fields(1).Value
                rs.MoveNext()
            End While
            .WriteLine("CHANGE:" & Space(40 - Len("CHANGE:" & FormatNumber(cashBack, 2))) & FormatNumber(cashBack, 2))
            closeDBFconnection()
            .WriteLine()
            .WriteLine()
            .WriteLine("Signature: _____________________________")
            .WriteLine("Name     : " & SeniorName)
            If TransType = 1 Then
                .WriteLine("Senior ID: " & SeniorID)
            ElseIf TransType = 2 Then
                .WriteLine("Diplomat: " & SeniorID)
            ElseIf TransType = 3 Then
                .WriteLine("PWD: " & SeniorID)
            End If
            While seniorCount > 1
                .WriteLine()
                .WriteLine("Signature: _____________________________")
                .WriteLine("Name     : ")
                If TransType = 1 Then
                    .WriteLine("Senior ID: " & SeniorID)
                ElseIf TransType = 2 Then
                    .WriteLine("Diplomat: " & SeniorID)
                ElseIf TransType = 3 Then
                    .WriteLine("PWD: " & SeniorID)
                End If
                seniorCount -= 1
            End While
            .WriteLine()
            .WriteLine()
            .WriteLine("Sales Summary")
            .WriteLine("----------------------------------------")
            If TransType = 1 Then
                .WriteLine(Space(9) & "VAT Sale:" & Space(40 - Len(Space(9) & "VAT Sale:" & FormatAmount((subTotal - seniorAmount) / 1.12))) & FormatAmount((subTotal - seniorAmount) / 1.12))
                .WriteLine(Space(9) & "12% VAT:" & Space(40 - Len(Space(9) & "12% VAT:" & FormatAmount(vat - lessVat))) & FormatAmount(vat - lessVat))
                .WriteLine(Space(9) & "VAT Exempt Sales:" & Space(40 - Len(Space(9) & "VAT Exempt Sales:" & FormatAmount(seniorAmount))) & FormatAmount(seniorAmount))
                .WriteLine(Space(9) & "Zero Rated:" & Space(40 - Len(Space(9) & "Zero Rated:" & "0.00")) & "0.00")
            ElseIf TransType = 2 Then
                .WriteLine(Space(9) & "VAT Sale:" & Space(40 - Len(Space(9) & "VAT Sale:" & FormatAmount((subTotal - seniorAmount) / 1.12))) & FormatAmount((subTotal - seniorAmount) / 1.12))
                .WriteLine(Space(9) & "12% VAT:" & Space(40 - Len(Space(9) & "12% VAT:" & FormatAmount(vat - lessVat))) & FormatAmount(vat - lessVat))
                .WriteLine(Space(9) & "VAT Exempt Sales:" & Space(40 - Len(Space(9) & "VAT Exempt Sales:" & "0.00")) & "0.00")
                .WriteLine(Space(9) & "Zero Rated:" & Space(40 - Len(Space(9) & "Zero Rated:" & FormatAmount(seniorAmount - lessVat))) & FormatAmount(seniorAmount - lessVat))
            ElseIf TransType = 3 Then
                .WriteLine(Space(9) & "VAT Sale:" & Space(40 - Len(Space(9) & "VAT Sale:" & FormatAmount((subTotal) / 1.12))) & FormatAmount((subTotal) / 1.12))
                .WriteLine(Space(9) & "12% VAT:" & Space(40 - Len(Space(9) & "12% VAT:" & FormatAmount(vat - lessVat))) & FormatAmount(vat - lessVat))
                .WriteLine(Space(9) & "VAT Exempt Sales:" & Space(40 - Len(Space(9) & "VAT Exempt Sales:" & "0.00")) & "0.00")
                .WriteLine(Space(9) & "Zero Rated:" & Space(40 - Len(Space(9) & "Zero Rated:" & "0.00")) & "0.00")
            End If
            .WriteLine("----------------------------------------")
            .WriteLine()
            .WriteLine()
            PrintFooter(sw)
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
