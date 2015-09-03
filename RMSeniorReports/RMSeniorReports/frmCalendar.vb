Public Class frmCalendar

    Private Sub monthStart_DateChanged(sender As Object, e As DateRangeEventArgs) Handles monthStart.DateChanged
        lblStart.Text = Format(monthStart.SelectionRange.Start, "yyyy-MM-dd")
    End Sub

    Private Sub monthEnd_DateChanged(sender As Object, e As DateRangeEventArgs) Handles monthEnd.DateChanged
        lblEnd.Text = Format(monthEnd.SelectionRange.Start, "yyyy-MM-dd")
    End Sub

    Private Sub frmCalendar_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        lblStart.Text = Format(Now, "yyyy-MM-dd")
        lblEnd.Text = Format(Now, "yyyy-MM-dd")
    End Sub

    Private Sub btnStart_Click(sender As Object, e As EventArgs) Handles btnStart.Click
        With frmReport
            .start_Date = lblStart.Text
            .end_Date = lblEnd.Text
        End With
        frmReport.Show()
    End Sub

    Private Sub btnEnd_Click(sender As Object, e As EventArgs) Handles btnEnd.Click
        Me.Close()
    End Sub
End Class