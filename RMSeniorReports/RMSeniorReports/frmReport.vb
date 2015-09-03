Imports MySql.Data.MySqlClient
Imports Microsoft.Reporting.WinForms

Public Class frmReport

    Public start_Date, end_Date As String

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            ConnectToServer()
            LoadSummaryData()
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
        'LoadItemData()
    End Sub

    Sub LoadSummaryData()
        Dim sqlDS As New datasetRMSenior
        sqlDA = New MySqlDataAdapter
        sqlDA.SelectCommand = New MySqlCommand("Select or_no,or_date,senior_id,senior_name,(senior_amount - less_vat) as senior_amount,discount,emp_name from sales_summary where or_date BETWEEN '" & start_Date & _
                                               "' AND '" & end_Date & "'", sqlCon)
        sqlDA.Fill(sqlDS.Tables("tblORDetail"))
        repView.ProcessingMode = ProcessingMode.Local
        repView.LocalReport.ReportPath = System.Environment.CurrentDirectory & "\repORSummary.rdlc"
        repView.LocalReport.DataSources.Clear()
        repView.LocalReport.DataSources.Add(New ReportDataSource("dsSeniorItem", sqlDS.Tables("tblORDetail")))
        sqlDA.SelectCommand = New MySqlCommand("Select count(or_no) as countOR, sum(senior_amount - less_VAT) as seniorTotal, sum(discount) as discountTotal from sales_summary where or_date BETWEEN '" & start_Date & _
                                               "' AND '" & end_Date & "'", sqlCon)
        sqlDA.Fill(sqlDS.Tables("tblORSummary"))
        repView.LocalReport.DataSources.Add(New ReportDataSource("dsSenior", sqlDS.Tables("tblORSummary")))
        repView.DocumentMapCollapsed = True
        repView.SetDisplayMode(DisplayMode.PrintLayout)
        repView.RefreshReport()
        sqlDA.Dispose()
        DisconnectFromServer()
    End Sub

    Sub LoadItemData()
        Dim sqlDS As New datasetRMSenior
        sqlDA = New MySqlDataAdapter
        sqlDA.SelectCommand = New MySqlCommand("Select * from sales_detail", sqlCon)
        sqlDA.Fill(sqlDS.Tables("tblORItems"))
        repView.ProcessingMode = ProcessingMode.Local
        repView.LocalReport.ReportPath = System.Environment.CurrentDirectory & "\repORItems.rdlc"
        repView.LocalReport.DataSources.Clear()
        repView.LocalReport.DataSources.Add(New ReportDataSource("dsORItems", sqlDS.Tables("tblORItems")))
        repView.DocumentMapCollapsed = True
        repView.SetDisplayMode(DisplayMode.PrintLayout)
        repView.RefreshReport()
        sqlDA.Dispose()
        DisconnectFromServer()
    End Sub

End Class
