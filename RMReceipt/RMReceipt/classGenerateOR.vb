Imports MySql.Data.MySqlClient

Public Class classGenerateOR

    Private _OR As String
    Private _TableNo As String
    Private _Cashier As String
    Private _CustNo As String
    Private _SeniorName As String
    Private _SeniorID As String
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
        sqlCmd.Parameters.Add("@seniorID", MySqlDbType.String)
        sqlCmd.Parameters("@seniorID").Direction = ParameterDirection.Output
        sqlCmd.Parameters.Add("@seniorName", MySqlDbType.String)
        sqlCmd.Parameters("@seniorName").Direction = ParameterDirection.Output
        sqlCmd.ExecuteNonQuery()
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        Cashier = sqlCmd.Parameters("@empName").Value.ToString
        TableNo = sqlCmd.Parameters("@tableNo").Value.ToString
        CustNo = sqlCmd.Parameters("@custCount").Value.ToString
        SeniorID = sqlCmd.Parameters("@seniorID").Value.ToString
        SeniorName = sqlCmd.Parameters("@seniorName").Value.ToString
    End Sub

    Protected Sub PrintBillOut()
        Dim sw As New System.IO.StreamWriter(fileP & fileN, False)
        Dim qty As Integer
        Dim itemName As String
        Dim itemPrice As String
        Dim subTotal, sPreTax, lessVat, seniorDiscount, serviceCharge, vat, amountDue, seniorAmount As Decimal
        Dim seniorCount As Integer
        With sw
            .WriteLine(Space((40 - Len("BALAY DAKO")) / 2) & "BALAY DAKO")
            .WriteLine(Space((40 - Len("TAGAYTAY CITY")) / 2) & "TAGAYTAY CITY")
            .WriteLine(Space((40 - Len("Tel. (02) xxx-xxxx")) / 2) & "Tel. (02) xxx-xxxx")
            .WriteLine(Space((40 - Len("TIN  008-524-024-000 VAT")) / 2) & "TIN  008-524-024-000 VAT")
            .WriteLine()
            .WriteLine("****************************************")
            .WriteLine(Space((40 - Len("OFFICIAL RECEIPT")) / 2) & "OFFICIAL RECEIPT")
            .WriteLine("****************************************")
            If isBillOut = True Then
                .WriteLine("Bill No: " & ORNo)
            Else
                .WriteLine("Receipt No: " & ORNo)
            End If
            .WriteLine("Table: " & TableNo)
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
                    'Sub Total
                    subTotal = sqlDR("trans_gross").ToString
                    'less VAT
                    lessVat = sqlDR("less_vat").ToString
                    'Vat
                    vat = sqlDR("vat").ToString
                    'Sub Total Pre Tax
                    sPreTax = subTotal - vat
                    'Senior Count
                    seniorCount = sqlDR("senior_count").ToString
                    'Senior Amount
                    seniorAmount = sqlDR("senior_amount").ToString
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
            .WriteLine("Sales Summary")
            .WriteLine("----------------------------------------")
            .WriteLine(Space(9) & "VAT Sale:" & Space(40 - Len(Space(9) & "VAT Sale:" & FormatAmount((subTotal - seniorAmount) / 1.12))) & FormatAmount((subTotal - seniorAmount) / 1.12))
            .WriteLine(Space(9) & "12% VAT:" & Space(40 - Len(Space(9) & "12% VAT:" & FormatAmount(vat - lessVat))) & FormatAmount(vat - lessVat))
            .WriteLine(Space(9) & "VAT Exempt Sales:" & Space(40 - Len(Space(9) & "VAT Exempt Sales:" & FormatAmount(seniorAmount))) & FormatAmount(seniorAmount))
            .WriteLine(Space(9) & "Zero Rated:" & Space(40 - Len(Space(9) & "Zero Rated:" & "0.00")) & "0.00")
            .WriteLine("----------------------------------------")
            .WriteLine()
            .WriteLine()
            .WriteLine("Signature: _____________________________")
            .WriteLine("Name     : " & SeniorName)
            .WriteLine("Senior ID: " & SeniorID)
            While seniorCount > 0
                .WriteLine()
                .WriteLine("Signature: _____________________________")
                .WriteLine("Name     : ")
                .WriteLine("Senior ID: ")
                seniorCount -= 1
            End While
            .WriteLine()
            .WriteLine()
            If isBillOut = False Then
                .WriteLine(Space((40 - Len("MIN: 15052511205400030")) / 2) & "MIN: 15052511205400030")
                .WriteLine(Space((40 - Len("Serial No: WMC3F1730294")) / 2) & "Serial No: WMC3F1730294")
                .WriteLine(Space((40 - Len("Y80414B00310")) / 2) & "Y80414B00310")
                .WriteLine(Space((40 - Len("Y80414B00304")) / 2) & "Y80414B00304")
                .WriteLine(Space((40 - Len("Y80414B00345")) / 2) & "Y80414B00345")
                .WriteLine(Space((40 - Len("Permit No: FP052015-54A-0034569-00000")) / 2) & "Permit No: FP052015-54A-0034569-00000")
                .WriteLine(Space((40 - Len("Accr No:05000824300300057870683")) / 2) & "Accr No:05000824300300057870683")
                .WriteLine(Space((40 - Len("This serves as your Official Receipt")) / 2) & "This serves as your Official Receipt")
                .WriteLine(Space((40 - Len("Thank you for visiting us")) / 2) & "Thank you for visiting us")
            End If
            .WriteLine()
            .WriteLine()
            .WriteLine()
            .WriteLine()
            .WriteLine()
            .Close() 'Close File
        End With
    End Sub

End Class
