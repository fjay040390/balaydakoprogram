Imports ADODB
Imports MySql.Data.MySqlClient
Imports System.Configuration
Imports Microsoft.Win32

Module Module1

#Region "Variables"

#Region "Station Variables"
    'Stations
    Public StationNum As Integer
    'Transaction Type
    Public Const TransType As Integer = 1 'Senior
    'Bill Time
    Public billTime As DateTime
#End Region

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
    Public date_sfx As String

    Public table_name As String
    Public or_no As String
    Public table_no As String
    Public cust_no As Integer
    Public cashier_name As String
    Public button_group As String = "COMMEDOR 1"
    Public bill_amount As Double

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

#Region "DB Routines"

    'Get Text of Dynamically Generated Button
    'CType(CType(sender, System.Windows.Forms.Button).Text, String)

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

#Region "Station Registry"
    '' Station Registry
    Public Sub RegisterStationNum(ByVal statNum As Integer)
        Dim regKey As RegistryKey
        'Open Registry Key
        regKey = My.Computer.Registry.CurrentUser.OpenSubKey("SOFTWARE\RM", True)
        'Set Value for Registry Key
        regKey.SetValue("statNum", statNum)
        regKey.Close()
    End Sub

    Public Function CheckIfStationRegistryExists() As Boolean
        Dim RegKey As RegistryKey
        RegKey = My.Computer.Registry.CurrentUser.OpenSubKey("SOFTWARE", True)
        'Create Registry Key for Station Number at LocalMachine
        My.Computer.Registry.CurrentUser.CreateSubKey("SOFTWARE\RM")
        'Check if Registry Value Exists
        If My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\RM", "statnum", Nothing) Is Nothing Then
            Return False
        Else
            Return True
        End If
        RegKey.Close()
    End Function

    Public Function GetStationNum() As Integer
        Dim statNum = My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\RM", "statnum", Nothing)
        Return statNum
    End Function
    '' End Station Registry ''

    '' Date Suffix Registry ''
    Public Function CheckIfDateSuffixExists()
        Dim RegKey As RegistryKey
        RegKey = My.Computer.Registry.CurrentUser.OpenSubKey("SOFTWARE", True)
        'Create Registry Key for Station Number at LocalMachine
        My.Computer.Registry.CurrentUser.CreateSubKey("SOFTWARE\RM")
        'Check if Registry Value Exists
        If My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\RM", "date_suffx", Nothing) Is Nothing Then
            Return False
        Else
            Return True
        End If
        RegKey.Close()
    End Function

    Public Sub RegisterDateSuffix()
        Dim rsConfig As ADODB.Recordset
        openDBFconnection()
        rsConfig = con.Execute("Select SALES_SUFX from CONFIG")
        Dim regKey As RegistryKey
        'Open Registry Key
        regKey = My.Computer.Registry.CurrentUser.OpenSubKey("SOFTWARE\RM", True)
        'Set Value for Registry Key
        regKey.SetValue("date_suffx", rsConfig.Fields(0).Value)
        regKey.Close()
        closeDBFconnection()
    End Sub

    Public Function GetDateSuffix() As String
        Dim date_sfx = My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\RM", "date_suffx", Nothing)
        Return date_sfx
    End Function
    '' End Suffix Registry ''
#End Region

End Module
