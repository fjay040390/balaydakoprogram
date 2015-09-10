Imports System.Windows.Forms.Form
Imports MySql.Data.MySqlClient

Public Class Form2

#Region "Drag Window"
    Dim drag As Boolean
    Dim mousex As Integer
    Dim mousey As Integer
    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub

    Private Sub Panel1_MouseDown(sender As Object, e As MouseEventArgs) Handles Panel1.MouseDown
        drag = True
        mousex = Windows.Forms.Cursor.Position.X - Me.Left
        mousey = Windows.Forms.Cursor.Position.Y - Me.Top
    End Sub

    Private Sub Panel1_MouseMove(sender As Object, e As MouseEventArgs) Handles Panel1.MouseMove
        If drag Then
            Me.Top = Windows.Forms.Cursor.Position.Y - mousey
            Me.Left = Windows.Forms.Cursor.Position.X - mousex
        End If
    End Sub

    Private Sub Panel1_MouseUp(sender As Object, e As MouseEventArgs) Handles Panel1.MouseUp
        drag = False
    End Sub
#End Region

#Region "Form Events"

    Private Sub Form2_Load(sender As Object, e As EventArgs) Handles Me.Load
        loadDetails()
    End Sub

    Private Sub btnGo_Click(sender As Object, e As EventArgs) Handles btnGo.Click
        'IF ALL ENTRIES ARE SUPPLIED
        If IfInputNotNull() = True Then
            If Val(lblCustNo.Text) >= Val(txtSCcount.Text) And Val(txtSCcount.Text) > 0 Then
                CalculateDiplomat()
                btnPrint.Enabled = True
                SetStatusText("Calculated! Ready for Print", Color.White)
                btnPrint.Focus()
            Else
                SetStatusText("Error: SC Count is larger than total customer count!", Color.Red)
                txtSCcount.Focus()
                txtSCcount.Clear()
            End If
        Else
            MsgBox("Fill out all required fields!", vbCritical, "Error")
        End If
    End Sub

    Private Sub btnPrint_Click(sender As Object, e As EventArgs) Handles btnPrint.Click
        If IfInputNotNull() = True Then
            Try
                SetStatusText("Saving sales data...", Color.White)
                'Connect to mysql server
                ConnectToServer()
                'Delete all existing items at database by OR
                DeleteItemData(lblBillNo.Text)
                'Update Bill Out Time and Status
                UpdateBillTimeDBF(lblBillNo.Text)
                'Insert OR summary to database 
                InsertSummaryData()
                'Insert OR items to database 
                LoadItemDataFromPOS(lblBillNo.Text)
                'Change Status bar
                SetStatusText("Printing...", Color.White)
                'Print Bill Out
                PrintBillOut()
                'Update current OR (To be printed automatically by Receipt OR)
                UpdateCurrentBillNo(lblBillNo.Text)
            Catch ex As Exception
                MsgBox(ex.Message, vbCritical, "Error")
                UpdateCurrentBillNo(0)
            Finally
                DisconnectFromServer()
                Me.Close()
            End Try
        Else
            MsgBox("Fill out all required fields!", vbCritical, "Error")
            btnPrint.Enabled = False
        End If
    End Sub

#End Region

#Region "Sub Routines"

    Sub loadDetails()
        lblStationNum.Text = "Cashier: " & StationNum
        lblBillNo.Text = or_no
        lblCashier.Text = GetCashierName()
        lblCustNo.Text = cust_no
        lblGross.Text = FormatNumber(bill_amount, 2)
        lblGross2.Text = FormatNumber(bill_amount + ((bill_amount / 1.12) * 0.1), 2)
        lblTable.Text = table_no
        lblTableGroup.Text = button_group
    End Sub

    Sub CalculateDiplomat()
        Dim ORVat As Decimal = CDbl(lblGross.Text)
        Dim ORNoVat As Decimal = ORVat / 1.12
        Dim ORServiceCharge As Decimal = ORNoVat * 0.1

        Dim CustCount As Integer = CInt(lblCustNo.Text)
        Dim SRCount As Integer = CInt(txtSCcount.Text)

        Dim SRAmount As Decimal = (ORVat / CustCount) * SRCount
        Dim SRLessVAT As Decimal = (SRAmount / 1.12) * 0.12
        'Dim SRDiscount As Decimal = (SRAmount - SRLessVAT) * 0.2
        Dim SRDiscount As Decimal = 0
        Dim SRServChg As Decimal = (SRAmount - SRLessVAT - SRDiscount) * 0.1
        Dim SRTotal As Decimal = (SRAmount - SRLessVAT - SRDiscount) + SRServChg

        lblSCGross.Text = FormatNumber(SRAmount, 2)
        'lblSCDisc.Text = FormatNumber(SRDiscount, 2)
        lblSCDisc.Text = FormatNumber(SRLessVAT, 2)
        lblServiceCharge.Text = FormatNumber(SRServChg, 2)
        lblSCTotal.Text = FormatNumber(SRTotal, 2)
    End Sub

    Sub SetStatusText(ByVal msg As String, ByVal clr As Color)
        lblStatus.Text = msg
        lblStatus.ForeColor = clr
    End Sub

    Function IfInputNotNull() As Boolean
        If txtSCcount.Text = "" Or txtSCid.Text = "" Or txtSCname.Text = "" Or Not IsNumeric(txtSCcount.Text) Then
            Return False
        Else
            Return True
        End If
    End Function

    Function CheckIfORExists() As Boolean
        sqlCmd = New MySqlCommand("select * from sales_summary where or_no = @or", sqlCon)
        sqlCmd.Parameters.AddWithValue("@or", lblBillNo.Text)
        sqlDR = sqlCmd.ExecuteReader()
        If sqlDR.HasRows Then
            'Update Data
            sqlDR.Close()
            Return True
        Else
            'Insert Data
            sqlDR.Close()
            Return False
        End If
    End Function

    Function GetCashierName() As String
        openDBFconnection()
        'Cashier Name
        rs = con.Execute("Select a.emp_name from employee a left join " & db_name & " b on a.emp_no = b.emp_no ")
        If Not rs.EOF Then
            cashier_name = rs.Fields(0).Value
        End If
        rs = Nothing
        closeDBFconnection()
        Return cashier_name.ToString
    End Function

#End Region

#Region "Database Functions"

    Sub InsertSummaryData()
        sqlCmd = New MySqlCommand("InsertSummaryData", sqlCon)
        sqlCmd.CommandType = CommandType.StoredProcedure
        'OR No
        sqlCmd.Parameters.AddWithValue("@orNo", lblBillNo.Text)
        sqlCmd.Parameters("@orNo").Direction = ParameterDirection.Input
        'Date
        sqlCmd.Parameters.AddWithValue("@orDate", Format(Now, "yyy-MM-dd"))
        sqlCmd.Parameters("@orDate").Direction = ParameterDirection.Input
        'Cashier Name
        sqlCmd.Parameters.AddWithValue("@empName", lblCashier.Text)
        sqlCmd.Parameters("@empName").Direction = ParameterDirection.Input
        'Customer Count
        sqlCmd.Parameters.AddWithValue("@custCount", lblCustNo.Text)
        sqlCmd.Parameters("@custCount").Direction = ParameterDirection.Input
        'Senior Count
        sqlCmd.Parameters.AddWithValue("@count", txtSCcount.Text)
        sqlCmd.Parameters("@count").Direction = ParameterDirection.Input
        'Senior ID
        sqlCmd.Parameters.AddWithValue("@id", txtSCid.Text)
        sqlCmd.Parameters("@id").Direction = ParameterDirection.Input
        'Senior Name
        sqlCmd.Parameters.AddWithValue("@cName", txtSCname.Text)
        sqlCmd.Parameters("@cName").Direction = ParameterDirection.Input
        'Transaction Gross
        sqlCmd.Parameters.AddWithValue("@transGross", CDbl(lblGross.Text))
        sqlCmd.Parameters("@transGross").Direction = ParameterDirection.Input
        'VAT
        If Val(lblCustNo.Text = txtSCcount.Text) Then
            sqlCmd.Parameters.AddWithValue("@vat", "0")
        Else
            sqlCmd.Parameters.AddWithValue("@vat", CDbl((lblGross.Text / 1.12) * 0.12))
        End If
        sqlCmd.Parameters("@vat").Direction = ParameterDirection.Input
        'Senior Amount
        sqlCmd.Parameters.AddWithValue("@amount", CDbl(lblSCGross.Text))
        sqlCmd.Parameters("@amount").Direction = ParameterDirection.Input
        'Less VAT
        sqlCmd.Parameters.AddWithValue("@lessVAT", CDbl(lblSCDisc.Text))
        sqlCmd.Parameters("@lessVAT").Direction = ParameterDirection.Input
        'Senior Discount
        sqlCmd.Parameters.AddWithValue("@discount", 0)
        sqlCmd.Parameters("@discount").Direction = ParameterDirection.Input
        'Table Group
        sqlCmd.Parameters.AddWithValue("@tableGroup", lblTableGroup.Text)
        sqlCmd.Parameters("@tableGroup").Direction = ParameterDirection.Input
        'Table Number
        sqlCmd.Parameters.AddWithValue("@tableNo", lblTable.Text)
        sqlCmd.Parameters("@tableNo").Direction = ParameterDirection.Input
        'Transaction Type
        sqlCmd.Parameters.AddWithValue("@transType", TransType)
        sqlCmd.Parameters("@transType").Direction = ParameterDirection.Input
        'Bill Time
        sqlCmd.Parameters.AddWithValue("@billTime", billTime)
        sqlCmd.Parameters("@billTime").Direction = ParameterDirection.Input
        'Execute Query
        sqlCmd.ExecuteNonQuery()
    End Sub

    Sub LoadItemDataFromPOS(ByVal orNo As String)
        openDBFconnection()
        rs = con.Execute("Select a.ref_no,b.descript,a.quanty,a.raw_price from " & db_name & " a left join menu b on b.ref_no = a.ref_no")
        With rs
            If Not .EOF Then
                While Not .EOF
                    InsertItemData(orNo,
                                   .Fields(0).Value,
                                   .Fields(1).Value,
                                   .Fields(2).Value,
                                   .Fields(3).Value,
                                   ((((.Fields(2).Value * .Fields(3).Value) / lblCustNo.Text) * txtSCcount.Text) / 1.12) * 0.12,
                                   0
                                   )
                    .MoveNext()
                End While
            End If
        End With
        closeDBFconnection()
        '((((.Fields(2).Value * .Fields(3).Value) / lblCustNo.Text) * txtSCcount.Text) / 1.12) * 0.2
    End Sub

    Sub LoadPaymentDataFromPOS(ByVal orNo As String)
        openDBFconnection()
        rs = con.Execute("Select b.TPAGO_NAME, a.BASE_AMT, a.RECEIVED, a.CASH_BACK, a.TIP_AMT  from PMT" & date_sfx & _
                         "a LEFT JOIN TIPOPAG b on a.PAY_TYPE = b.TPAGO_NO where a.bill_no=" & or_no)
        With rs
            If Not .EOF Then
                While Not .EOF
                    InsertPaymentData(or_no,
                                   .Fields(0).Value,
                                   .Fields(1).Value,
                                   .Fields(2).Value,
                                   .Fields(3).Value,
                                   .Fields(4).Value)
                    .MoveNext()
                End While
            End If
        End With
        closeDBFconnection()
    End Sub

    Sub InsertItemData(ByVal orNo As String, ByVal refNo As String, ByVal itemDesc As String, ByVal quantity As Decimal,
                       ByVal rawPrice As Decimal, ByVal lessVAT As Decimal, ByVal discount As Decimal)
        'Insert new entries
        sqlCmd = New MySqlCommand("InsertItemData", sqlCon)
        sqlCmd.CommandType = CommandType.StoredProcedure
        'OR No
        sqlCmd.Parameters.AddWithValue("@orNo", orNo)
        sqlCmd.Parameters("@orNo").Direction = ParameterDirection.Input
        'POS PLU No
        sqlCmd.Parameters.AddWithValue("@refNo", refNo)
        sqlCmd.Parameters("@refNo").Direction = ParameterDirection.Input
        'Item Name
        sqlCmd.Parameters.AddWithValue("@itemDesc", itemDesc)
        sqlCmd.Parameters("@itemDesc").Direction = ParameterDirection.Input
        'Quantity of Item
        sqlCmd.Parameters.AddWithValue("@quantity", quantity)
        sqlCmd.Parameters("@quantity").Direction = ParameterDirection.Input
        'Raw Price of Item
        sqlCmd.Parameters.AddWithValue("@rawPrice", rawPrice)
        sqlCmd.Parameters("@rawPrice").Direction = ParameterDirection.Input
        'Less VAT
        sqlCmd.Parameters.AddWithValue("@lessVAT", lessVAT)
        sqlCmd.Parameters("@lessVAT").Direction = ParameterDirection.Input
        'Senior Discount
        sqlCmd.Parameters.AddWithValue("@discount", discount)
        sqlCmd.Parameters("@discount").Direction = ParameterDirection.Input
        'Execute Query
        sqlCmd.ExecuteNonQuery()
    End Sub

    Sub InsertPaymentData(ByVal orNo As String, ByVal payDesc As String, ByVal baseAmt As Decimal,
                       ByVal receivedAmt As Decimal, ByVal cashBack As Decimal, ByVal tipAmt As Decimal)
        'Insert new entries
        sqlCmd = New MySqlCommand("InsertPaymentData", sqlCon)
        sqlCmd.CommandType = CommandType.StoredProcedure
        'OR No
        sqlCmd.Parameters.AddWithValue("@orNo", orNo)
        sqlCmd.Parameters("@orNo").Direction = ParameterDirection.Input
        'Payment Description
        sqlCmd.Parameters.AddWithValue("@payDesc", payDesc)
        sqlCmd.Parameters("@payDesc").Direction = ParameterDirection.Input
        'Base Amount
        sqlCmd.Parameters.AddWithValue("@baseAmt", baseAmt)
        sqlCmd.Parameters("@baseAmt").Direction = ParameterDirection.Input
        'Received Amount
        sqlCmd.Parameters.AddWithValue("@receivedAmt", receivedAmt)
        sqlCmd.Parameters("@receivedAmt").Direction = ParameterDirection.Input
        'Cash Back
        sqlCmd.Parameters.AddWithValue("@cashBack", cashBack)
        sqlCmd.Parameters("@cashBack").Direction = ParameterDirection.Input
        'Tip Amount
        sqlCmd.Parameters.AddWithValue("@tipAmt", tipAmt)
        sqlCmd.Parameters("@tipAmt").Direction = ParameterDirection.Input
        'Execute Query
        sqlCmd.ExecuteNonQuery()
    End Sub

    Sub DeleteItemData(ByVal orNo As String)
        'Delete previous entries
        sqlCmd = New MySqlCommand("DeleteItemData", sqlCon)
        sqlCmd.CommandType = CommandType.StoredProcedure
        sqlCmd.Parameters.AddWithValue("@orNo", orNo)
        sqlCmd.Parameters("@orNo").Direction = ParameterDirection.Input
        sqlCmd.ExecuteNonQuery()
    End Sub

    Sub DeleteORPayments(ByVal orNo As String)
        sqlCmd = New MySqlCommand("DeleteORPayments", sqlCon)
        sqlCmd.CommandType = CommandType.StoredProcedure
        sqlCmd.Parameters.AddWithValue("@orNo", orNo)
        sqlCmd.Parameters("@orNo").Direction = ParameterDirection.Input
        sqlCmd.ExecuteNonQuery()
    End Sub

    Sub UpdateCurrentBillNo(ByVal orNo As String)
        sqlCmd = New MySqlCommand("UpdateCurrentOR", sqlCon)
        sqlCmd.CommandType = CommandType.StoredProcedure
        sqlCmd.Parameters.AddWithValue("@billNo", orNo)
        sqlCmd.Parameters("@billNo").Direction = ParameterDirection.Input
        sqlCmd.Parameters.AddWithValue("@stationNum", StationNum)
        sqlCmd.Parameters("@stationNum").Direction = ParameterDirection.Input
        sqlCmd.ExecuteNonQuery()
    End Sub

    Sub UpdateBillTimeDBF(ByVal orNo As String)
        billTime = Format(TimeOfDay, "HH:mm:ss")
        openDBFconnection()
        rs = con.Execute("Update SLS" & date_sfx & " set BILL_TIME ='" & Format(billTime, "HH:mm:ss") & _
                         "', BILL_DATE = '" & Format(Today, "MM/dd/yyyy") & "', PRINTED = TRUE where bill_no = " & orNo)
        closeDBFconnection()
    End Sub

#End Region

#Region "Receipt Printing"

    Sub PrintBillOut()
        Dim generateOR As New classGenerateOR
        generateOR.ORNo = lblBillNo.Text
        generateOR.TableNo = lblTable.Text
        generateOR.CustNo = lblCustNo.Text
        generateOR.Cashier = lblCashier.Text
        generateOR.SeniorID = txtSCid.Text
        generateOR.SeniorName = txtSCname.Text
        generateOR.BillTIME = billTime
        generateOR.isBillOut = True
        generateOR.fileP = pos_printer_path
        generateOR.Print()
    End Sub

#End Region

End Class