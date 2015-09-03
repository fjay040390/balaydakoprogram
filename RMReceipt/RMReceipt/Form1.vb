Public Class Form1

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Try
            ConnectToServer()
            checkStation()
            LoadConfig()
            PrintBillOut()
        Catch ex As Exception
            MsgBox(ex.Message)
        Finally
            DisconnectFromServer()
            TextBox1.Clear()
            TextBox1.Focus()
        End Try
    End Sub

End Class
