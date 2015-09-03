Imports MySql.Data.MySqlClient
Imports System.IO

Public Class Form2

    Private Sub Form2_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        StationNum = GetStationNum()
        PrintReceipt()
    End Sub

    Sub PrintReceipt()
        Try
            ConnectToServer()
            'checkStation()
            LoadConfig()
            GetBill()
            UpdateCurrentBillNo(0)
        Catch ex As Exception
            MsgBox(ex.Message)
            UpdateCurrentBillNo(or_no)
        Finally
            DisconnectFromServer()
            Me.Close()
        End Try
    End Sub

End Class