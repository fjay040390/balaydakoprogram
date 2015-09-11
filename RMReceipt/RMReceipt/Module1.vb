Imports ADODB
Imports MySql.Data.MySqlClient
Imports System.Configuration
Imports System.IO
Imports Microsoft.Win32

Module Module1

#Region "Variables"

#Region "Database Variables"
    'MySQL Variables
    Public sqlCon As MySqlConnection
    Public sqlCmd As MySqlCommand
    Public sqlDA As MySqlDataAdapter
    Public sqlDR As MySqlDataReader
    Public sqlStr As String
#End Region

#Region "New Variables"
    Public rmpath As String
    Public pos_printer_path As String
    Public init_table As String
    Public con As ADODB.Connection
    Public rs As ADODB.Recordset
    Public str As String
    Public button_text As String
    Public db_name As String
    'Public date_sfx As String = Format(Today, "MMyy")
    Public date_sfx As String = "0690"

    Public table_name As String
    Public or_no As String
    Public table_no As String
    Public cust_no As Integer
    Public cashier_name As String
    Public button_group As String = "COMMEDOR 1"
    Public bill_amout As Double

    'Stations
    Public StationNum As Integer

#Region "Form 2 Variables"
    Dim ORVat As Decimal
    Dim ORNoVat As Decimal
    Dim ORServiceCharge As Decimal

    Dim CustCount As Integer
    Dim SRCount As Integer

    Dim SRAmount As Decimal
    Dim SRDiscount As Decimal
    Dim SRServChg As Decimal
    Dim SRTotal As Decimal
#End Region

#End Region

#End Region

    'Get Text of Dynamically Generated Button
    'CType(CType(sender, System.Windows.Forms.Button).Text, String)
#Region "DB Connections"

    Public Sub openDBFconnection()
        Try
            'str = "Driver={Microsoft Visual FoxPro Driver};SourceType=DBF;SourceDB=" & rmpath & ";Exclusive=No;Collate=Machine;NULL=NO;DELETED=NO;BACKGROUNDFETCH=NO"
            con = New ADODB.Connection
            'con.ConnectionString = "Provider=MSDASQL.1;Persist Security Info=False;Extended Properties= " & str & ""
            con.ConnectionString = "Provider=Microsoft.Jet.Oledb.4.0;Data Source=" & rmpath & ";Extended Properties=dBASE IV;User ID=Admin;Password=;"
            con.CursorLocation = CursorLocationEnum.adUseClient
            con.Open()
            Exit Sub
        Catch ex As Exception
            MsgBox(ex.Message, vbCritical, "Error")
            closeDBFconnection()
        End Try
    End Sub

    Public Sub closeDBFconnection()
        On Error Resume Next
        con.Close()
        con = Nothing
    End Sub

    Public Sub ConnectToServer()
        sqlStr = ConfigurationManager.ConnectionStrings("ConString").ConnectionString
        sqlCon = New MySqlConnection(sqlStr)
        'sqlCon.ConnectionString = sqlStr
        Try
            If Not sqlCon.State = ConnectionState.Open Then sqlCon.Close()
            sqlCon.Open()
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
            DisconnectFromServer()
        End Try
    End Sub

    Public Sub DisconnectFromServer()
        If sqlCon.State = ConnectionState.Open Then sqlCon.Close()
        sqlCon.Dispose()
    End Sub

    Public Sub checkStation()
        StationNum = GetStationNum()
        date_sfx = GetDateSuffix()
    End Sub

    Public Sub LoadConfig()
        Try
            sqlCmd = New MySqlCommand("Select * from config where station_num = " & StationNum, sqlCon)
            sqlDR = sqlCmd.ExecuteReader
            If sqlDR.HasRows Then
                While sqlDR.Read
                    rmpath = sqlDR.Item(1).ToString
                    pos_printer_path = sqlDR.Item(2).ToString
                    init_table = sqlDR.Item(3).ToString
                End While
            End If
            sqlDR.Close()
        Catch ex As Exception
            DisconnectFromServer()
        End Try
    End Sub

#End Region

#Region "Receipt Functions"

    Public Sub PrintBillOut()
        Dim generateOR As New classGenerateOR
        generateOR.ORNo = Form1.TextBox1.Text
        generateOR.isBillOut = False
        generateOR.fileP = pos_printer_path
        generateOR.Print()
    End Sub

    Public Sub PrintBillOutAuto()
        Dim generateORAuto As New classGenerateOR
        generateORAuto.ORNo = or_no
        generateORAuto.isBillOut = False
        generateORAuto.fileP = pos_printer_path
        generateORAuto.Print()
    End Sub

    Public Sub GetBill()
        sqlCmd = New MySqlCommand("getCurrentBill", sqlCon)
        sqlCmd.CommandType = CommandType.StoredProcedure
        sqlCmd.Parameters.AddWithValue("@stationNum", StationNum)
        sqlCmd.Parameters("@stationNum").Direction = ParameterDirection.Input
        sqlCmd.Parameters.Add("@billNo", MySqlDbType.String)
        sqlCmd.Parameters("@billNo").Direction = ParameterDirection.Output
        sqlCmd.ExecuteNonQuery()
        or_no = sqlCmd.Parameters("@billNo").Value.ToString
        If Not or_no = "0" Then PrintBillOutAuto()
    End Sub

    Public Sub UpdateCurrentBillNo(ByVal orNo As String)
        sqlCmd = New MySqlCommand("UpdateCurrentOR", sqlCon)
        sqlCmd.CommandType = CommandType.StoredProcedure
        sqlCmd.Parameters.AddWithValue("@billNo", orNo)
        sqlCmd.Parameters("@billNo").Direction = ParameterDirection.Input
        sqlCmd.Parameters.AddWithValue("@stationNum", StationNum)
        sqlCmd.Parameters("@stationNum").Direction = ParameterDirection.Input
        sqlCmd.ExecuteNonQuery()
    End Sub
#End Region

#Region "Station Registry"

    Public Function GetStationNum() As Integer
        Dim statNum = My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\RM", "statNum", Nothing)
        Return statNum
    End Function

    Public Function GetDateSuffix() As String
        Dim date_sfx = My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\RM", "date_suffx", Nothing)
        Return date_sfx
    End Function

#End Region

End Module
