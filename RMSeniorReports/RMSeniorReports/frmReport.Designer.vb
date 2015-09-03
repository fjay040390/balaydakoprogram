<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmReport
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim ReportDataSource1 As Microsoft.Reporting.WinForms.ReportDataSource = New Microsoft.Reporting.WinForms.ReportDataSource()
        Me.repView = New Microsoft.Reporting.WinForms.ReportViewer()
        Me.datasetRMSenior = New RMSeniorReports.datasetRMSenior()
        Me.tblORDetailBindingSource = New System.Windows.Forms.BindingSource(Me.components)
        CType(Me.datasetRMSenior, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.tblORDetailBindingSource, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'repView
        '
        Me.repView.Dock = System.Windows.Forms.DockStyle.Fill
        ReportDataSource1.Name = "dsSeniorSummary"
        ReportDataSource1.Value = Me.tblORDetailBindingSource
        Me.repView.LocalReport.DataSources.Add(ReportDataSource1)
        Me.repView.LocalReport.ReportEmbeddedResource = "RMSeniorReports.repORSummary.rdlc"
        Me.repView.Location = New System.Drawing.Point(0, 0)
        Me.repView.Name = "repView"
        Me.repView.Size = New System.Drawing.Size(612, 416)
        Me.repView.TabIndex = 0
        '
        'datasetRMSenior
        '
        Me.datasetRMSenior.DataSetName = "datasetRMSenior"
        Me.datasetRMSenior.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema
        '
        'tblORDetailBindingSource
        '
        Me.tblORDetailBindingSource.DataMember = "tblORDetail"
        Me.tblORDetailBindingSource.DataSource = Me.datasetRMSenior
        '
        'frmReport
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(612, 416)
        Me.Controls.Add(Me.repView)
        Me.Name = "frmReport"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Show Report"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        CType(Me.datasetRMSenior, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.tblORDetailBindingSource, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents repView As Microsoft.Reporting.WinForms.ReportViewer
    Friend WithEvents tblORDetailBindingSource As System.Windows.Forms.BindingSource
    Friend WithEvents datasetRMSenior As RMSeniorReports.datasetRMSenior

End Class
