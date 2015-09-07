Imports MySql.Data.MySqlClient

Public Class Form1

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        DisconnectFromServer()
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Try
            StationRegistryFunc()
            ConnectToServer()
            LoadConfig()
            DateRegistryFunc()
            LoadInitTables()
        Catch ex As Exception
            MsgBox(ex.Message, vbCritical, "Error")
            Me.Close()
        End Try
    End Sub

#Region "Table Buttons"

    Private Sub TableGrp(ByVal table_no As Integer)
        POSTables.Controls.Clear()
        sqlCmd = New MySqlCommand("Select * from Table_Buttons where table_no = " & table_no, sqlCon)
        sqlDR = sqlCmd.ExecuteReader
        If sqlDR.HasRows Then
            While sqlDR.Read
                Dim Button As New Button
                Button.Name = sqlDR(1).ToString
                Button.Text = sqlDR(2).ToString
                Button.BackColor = System.Drawing.ColorTranslator.FromHtml("#3498db")
                Button.FlatStyle = FlatStyle.Flat
                Button.FlatAppearance.BorderSize = 0
                Button.Width = 150
                Button.Height = 70
                Button.Font = New Font("Arial Narrow", 16, FontStyle.Bold)
                Button.ForeColor = Color.White
                POSTables.Controls.Add(Button)
                AddHandler Button.Click, AddressOf Me.Button_Click
            End While
        End If
        sqlDR.Close()
    End Sub

#End Region

#Region "Table Button Events"

    Private Sub Button_Click(sender As Object, e As EventArgs)
        Try
            ConnectToServer()
            button_text = CType(CType(sender, System.Windows.Forms.Button).Text, String)
            GetTableDetails(button_text)
        Catch ex As Exception
            MsgBox(ex.Message, vbCritical, "Error")
            DisconnectFromServer()
        End Try
    End Sub

    Private Sub btnTableGrp1_Click(sender As Object, e As EventArgs) Handles btnTableGrp1.Click
        TableGrp(1)
        DisableTableGroup("btnTableGrp1")
        button_group = "COMMEDOR 1"
    End Sub

    Private Sub btnTableGrp2_Click(sender As Object, e As EventArgs) Handles btnTableGrp2.Click
        TableGrp(2)
        DisableTableGroup("btnTableGrp2")
        button_group = "COMMEDOR 2"
    End Sub

    Private Sub btnTableGrp3_Click(sender As Object, e As EventArgs) Handles btnTableGrp3.Click
        TableGrp(3)
        DisableTableGroup("btnTableGrp3")
        button_group = "TERAZZA"
    End Sub

    Private Sub btnTableGrp4_Click(sender As Object, e As EventArgs) Handles btnTableGrp4.Click
        TableGrp(4)
        DisableTableGroup("btnTableGrp4")
        button_group = "ALFRESCCO"
    End Sub

    Private Sub btnTableGrp5_Click(sender As Object, e As EventArgs) Handles btnTableGrp5.Click
        TableGrp(5)
        DisableTableGroup("btnTableGrp5")
        button_group = "DELI"
    End Sub

    Private Sub btnTableGrp6_Click(sender As Object, e As EventArgs) Handles btnTableGrp6.Click
        TableGrp(6)
        DisableTableGroup("btnTableGrp6")
        button_group = "TO GO"
    End Sub

#End Region

#Region "Functions"

    Private Sub LoadInitTables()
        TableGrp(init_table)
        If StationNum = 1 Then
            DisableTableGroup("btnTableGrp1")
        ElseIf StationNum = 2 Then
            DisableTableGroup("btnTableGrp3")
        ElseIf StationNum = 3 Then
            DisableTableGroup("btnTableGrp5")
        End If
    End Sub

    Private Sub DisableTableGroup(ByVal tableG As String)
        For Each ctrl In Me.Controls
            If TypeOf ctrl Is Button Then
                If ctrl.Name = tableG Then
                    ctrl.Enabled = False
                Else
                    ctrl.Enabled = True
                End If
            End If
        Next
    End Sub

    Private Sub GetTableDetails(ByVal buttonText As String)
        sqlCmd = New MySqlCommand("Select * from Table_Buttons where button_text = " & buttonText, sqlCon)
        sqlDR = sqlCmd.ExecuteReader
        If sqlDR.HasRows Then
            While sqlDR.Read
                db_name = sqlDR.Item(3).ToString
            End While
            GetBillDetails()
        End If
        sqlDR.Close()
    End Sub

    Private Sub GetBillDetails()
        Dim rsOR As New ADODB.Recordset
        openDBFconnection()
        'Get OR Details
        rsOR = con.Execute("Select bill_no, table, people_no, waiter from SLS" & _
                           date_sfx & " where bill_no in (Select distinct(bill_no) from " & db_name & ")")
        While Not rsOR.EOF
            or_no = rsOR.Fields(0).Value
            table_no = rsOR.Fields(1).Value
            cust_no = rsOR.Fields(2).Value
            cashier_name = rsOR.Fields(3).Value
            rsOR.MoveNext()
        End While

        rsOR = con.Execute("Select sum(raw_price) as total from " & db_name)
        If Not rsOR.EOF Then
            bill_amount = rsOR.Fields(0).Value
        End If

        Dim form2 As New Form2
        form2.ShowDialog()
        closeDBFconnection()
        Exit Sub
    End Sub

#End Region

#Region "Registry Functions"
    Private Sub StationRegistryFunc()
        If CheckIfStationRegistryExists() = False Then
            Dim passStatNum = InputBox("Please enter station number", "Station Number Registration")
            RegisterStationNum(passStatNum)
        End If
        StationNum = GetStationNum()
    End Sub

    Private Sub DateRegistryFunc()
        '' Setting of Date Suffix ''
        'If CheckIfDateSuffixExists() = False Then
        RegisterDateSuffix()
        'End If
        date_sfx = GetDateSuffix()
        '' End Date Suffix ''
    End Sub
#End Region

End Class
