Imports MySql.Data.MySqlClient
Imports System.Configuration

Module Module1

    Public sqlCMD As MySqlCommand
    Public sqlDA As MySqlDataAdapter
    Public sqlSTR As String
    Public sqlCon As MySqlConnection

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
        sqlCon.Close()
        sqlCon.Dispose()
    End Sub

End Module
