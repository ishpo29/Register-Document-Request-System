Imports MySql.Data.MySqlClient
Imports System.IO

Public Class frmReports

    ' =========================================================
    ' REPORT VARIABLES
    ' =========================================================

    Private ReportTitle As String = ""


    ' =========================================================
    ' FORM LOAD
    ' =========================================================
    Public Sub ResetReportForm()

        ' Reset report type
        cboReportType.SelectedIndex = -1

        ' Reset dates
        dtpFromDate.Value = DateTime.Today
        dtpToDate.Value = DateTime.Today

        ' Clear report results
        dgvReportResults.DataSource = Nothing
        dgvReportResults.Rows.Clear()

    End Sub
    Private Sub frmReports_Load(
        sender As Object,
        e As EventArgs
    ) Handles MyBase.Load

        ThemeForm(Me)

        ApplyRoleAccess(Me, btnUsers, PictureBoxUsers)


        ' =====================================================
        ' REPORT TYPES
        ' =====================================================

        cboReportType.Items.Clear()

        cboReportType.Items.Add("All Document Requests")
        cboReportType.Items.Add("Pending Requests")
        cboReportType.Items.Add("Released Requests")
        cboReportType.Items.Add("Requests by Date")
        cboReportType.Items.Add("Requests by Document Type")
        cboReportType.Items.Add("Payment Report")


        If cboReportType.Items.Count > 0 Then
            cboReportType.SelectedIndex = 0
        End If


        ' =====================================================
        ' DATE SETTINGS
        ' =====================================================

        dtpFromDate.Value =
            New Date(
                Date.Today.Year,
                Date.Today.Month,
                1)

        dtpToDate.Value = Date.Today


        ' =====================================================
        ' DATAGRIDVIEW SETTINGS
        ' =====================================================

        dgvReportResults.AutoSizeColumnsMode =
            DataGridViewAutoSizeColumnsMode.Fill

        dgvReportResults.SelectionMode =
            DataGridViewSelectionMode.FullRowSelect

        dgvReportResults.MultiSelect = False

        dgvReportResults.ReadOnly = True

        dgvReportResults.AllowUserToAddRows = False

        dgvReportResults.AllowUserToDeleteRows = False

        dgvReportResults.AllowUserToResizeRows = False

        dgvReportResults.AllowUserToOrderColumns = False

        dgvReportResults.RowHeadersVisible = False

    End Sub


    ' =========================================================
    ' GENERATE REPORT
    ' =========================================================

    Private Sub btnGenerate_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnGenerate.Click

        If cboReportType.Text.Trim() = "" Then

            MessageBox.Show(
                "Please select a report type.",
                "Report",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            cboReportType.Focus()

            Exit Sub

        End If


        If dtpFromDate.Value.Date >
           dtpToDate.Value.Date Then

            MessageBox.Show(
                "The From Date cannot be later than the To Date.",
                "Report",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            Exit Sub

        End If


        Select Case cboReportType.Text

            Case "All Document Requests"

                LoadAllRequests()


            Case "Pending Requests"

                LoadPendingRequests()


            Case "Released Requests"

                LoadReleasedRequests()


            Case "Requests by Date"

                LoadRequestsByDate()


            Case "Requests by Document Type"

                LoadRequestsByDocument()


            Case "Payment Report"

                LoadPaymentReport()

        End Select

    End Sub


    ' =========================================================
    ' 1. ALL DOCUMENT REQUESTS
    ' =========================================================

    Private Sub LoadAllRequests()

        Try

            connection()

            sql = "SELECT " &
                  "r.RequestNo AS 'Request No.', " &
                  "DATE_FORMAT(r.RequestDate, '%Y-%m-%d') AS 'Date', " &
                  "r.StudentID AS 'Student ID', " &
                  "CONCAT(s.FirstName, ' ', s.LastName) AS 'Student Name', " &
                  "r.TotalAmount AS 'Total Amount', " &
                  "r.PaymentStatus AS 'Payment', " &
                  "r.Status AS 'Status' " &
                  "FROM tblrequest r " &
                  "INNER JOIN tblstudents s " &
                  "ON r.StudentID = s.StudentID " &
                  "ORDER BY r.RequestID DESC"

            LoadReport(
                sql,
                "All Document Requests")

        Catch ex As Exception

            ShowReportError(ex)

        Finally

            CloseConnection()

        End Try

    End Sub


    ' =========================================================
    ' 2. PENDING REQUESTS
    ' =========================================================

    Private Sub LoadPendingRequests()

        Try

            connection()

            sql = "SELECT " &
                  "r.RequestNo AS 'Request No.', " &
                  "DATE_FORMAT(r.RequestDate, '%Y-%m-%d') AS 'Date', " &
                  "r.StudentID AS 'Student ID', " &
                  "CONCAT(s.FirstName, ' ', s.LastName) AS 'Student Name', " &
                  "r.TotalAmount AS 'Total Amount', " &
                  "r.PaymentStatus AS 'Payment', " &
                  "r.Status AS 'Status' " &
                  "FROM tblrequest r " &
                  "INNER JOIN tblstudents s " &
                  "ON r.StudentID = s.StudentID " &
                  "WHERE r.Status = 'Pending' " &
                  "ORDER BY r.RequestDate DESC"

            LoadReport(
                sql,
                "Pending Requests")

        Catch ex As Exception

            ShowReportError(ex)

        Finally

            CloseConnection()

        End Try

    End Sub


    ' =========================================================
    ' 3. RELEASED REQUESTS
    ' =========================================================

    Private Sub LoadReleasedRequests()

        Try

            connection()

            sql = "SELECT " &
                  "r.RequestNo AS 'Request No.', " &
                  "DATE_FORMAT(r.RequestDate, '%Y-%m-%d') AS 'Date', " &
                  "r.StudentID AS 'Student ID', " &
                  "CONCAT(s.FirstName, ' ', s.LastName) AS 'Student Name', " &
                  "r.TotalAmount AS 'Total Amount', " &
                  "r.PaymentStatus AS 'Payment', " &
                  "r.Status AS 'Status' " &
                  "FROM tblrequest r " &
                  "INNER JOIN tblstudents s " &
                  "ON r.StudentID = s.StudentID " &
                  "WHERE r.Status = 'Released' " &
                  "ORDER BY r.RequestDate DESC"

            LoadReport(
                sql,
                "Released Requests")

        Catch ex As Exception

            ShowReportError(ex)

        Finally

            CloseConnection()

        End Try

    End Sub


    ' =========================================================
    ' 4. REQUESTS BY DATE
    ' =========================================================

    Private Sub LoadRequestsByDate()

        Try

            connection()

            sql = "SELECT " &
                  "r.RequestNo AS 'Request No.', " &
                  "DATE_FORMAT(r.RequestDate, '%Y-%m-%d') AS 'Date', " &
                  "r.StudentID AS 'Student ID', " &
                  "CONCAT(s.FirstName, ' ', s.LastName) AS 'Student Name', " &
                  "r.TotalAmount AS 'Total Amount', " &
                  "r.PaymentStatus AS 'Payment', " &
                  "r.Status AS 'Status' " &
                  "FROM tblrequest r " &
                  "INNER JOIN tblstudents s " &
                  "ON r.StudentID = s.StudentID " &
                  "WHERE DATE(r.RequestDate) " &
                  "BETWEEN @FromDate AND @ToDate " &
                  "ORDER BY r.RequestDate DESC"

            cmd = New MySqlCommand(
                sql,
                cn)

            cmd.Parameters.AddWithValue(
                "@FromDate",
                dtpFromDate.Value.Date)

            cmd.Parameters.AddWithValue(
                "@ToDate",
                dtpToDate.Value.Date)

            LoadReportFromCommand(
                cmd,
                "Requests by Date")

        Catch ex As Exception

            ShowReportError(ex)

        Finally

            CloseConnection()

        End Try

    End Sub


    ' =========================================================
    ' 5. REQUESTS BY DOCUMENT TYPE
    ' =========================================================

    Private Sub LoadRequestsByDocument()

        Try

            connection()

            sql = "SELECT " &
                  "d.DocumentName AS 'Document', " &
                  "COUNT(rd.RequestDetailID) AS 'Number of Requests', " &
                  "SUM(rd.Quantity) AS 'Quantity', " &
                  "SUM(rd.SubTotal) AS 'Total Amount' " &
                  "FROM tblrequestdetails rd " &
                  "INNER JOIN tbldocuments d " &
                  "ON rd.DocumentID = d.DocumentID " &
                  "INNER JOIN tblrequest r " &
                  "ON rd.RequestID = r.RequestID " &
                  "WHERE DATE(r.RequestDate) " &
                  "BETWEEN @FromDate AND @ToDate " &
                  "GROUP BY d.DocumentID, d.DocumentName " &
                  "ORDER BY COUNT(rd.RequestDetailID) DESC"

            cmd = New MySqlCommand(
                sql,
                cn)

            cmd.Parameters.AddWithValue(
                "@FromDate",
                dtpFromDate.Value.Date)

            cmd.Parameters.AddWithValue(
                "@ToDate",
                dtpToDate.Value.Date)

            LoadReportFromCommand(
                cmd,
                "Requests by Document Type")

        Catch ex As Exception

            ShowReportError(ex)

        Finally

            CloseConnection()

        End Try

    End Sub


    ' =========================================================
    ' 6. PAYMENT REPORT
    ' =========================================================

    Private Sub LoadPaymentReport()

        Try

            connection()

            sql = "SELECT " &
                  "r.RequestNo AS 'Request No.', " &
                  "DATE_FORMAT(r.RequestDate, '%Y-%m-%d') AS 'Date', " &
                  "CONCAT(s.FirstName, ' ', s.LastName) AS 'Student Name', " &
                  "r.TotalAmount AS 'Amount', " &
                  "r.PaymentStatus AS 'Payment Status', " &
                  "r.ORNo AS 'OR No.', " &
                  "DATE_FORMAT(r.ORDate, '%Y-%m-%d') AS 'OR Date' " &
                  "FROM tblrequest r " &
                  "INNER JOIN tblstudents s " &
                  "ON r.StudentID = s.StudentID " &
                  "WHERE DATE(r.RequestDate) " &
                  "BETWEEN @FromDate AND @ToDate " &
                  "ORDER BY r.RequestDate DESC"

            cmd = New MySqlCommand(
                sql,
                cn)

            cmd.Parameters.AddWithValue(
                "@FromDate",
                dtpFromDate.Value.Date)

            cmd.Parameters.AddWithValue(
                "@ToDate",
                dtpToDate.Value.Date)

            LoadReportFromCommand(
                cmd,
                "Payment Report")

        Catch ex As Exception

            ShowReportError(ex)

        Finally

            CloseConnection()

        End Try

    End Sub


    ' =========================================================
    ' LOAD REPORT USING SQL STRING
    ' =========================================================

    Private Sub LoadReport(
        query As String,
        title As String)

        sql = query

        cmd = New MySqlCommand(
            sql,
            cn)

        LoadReportFromCommand(
            cmd,
            title)

    End Sub


    ' =========================================================
    ' LOAD REPORT INTO DATAGRIDVIEW
    ' =========================================================

    Private Sub LoadReportFromCommand(
        reportCommand As MySqlCommand,
        title As String)

        da = New MySqlDataAdapter(
            reportCommand)

        dt = New DataTable()

        da.Fill(dt)

        dgvReportResults.DataSource = dt

        ReportTitle = title


        ' =====================================================
        ' DATAGRIDVIEW SETTINGS
        ' =====================================================

        dgvReportResults.AutoSizeColumnsMode =
            DataGridViewAutoSizeColumnsMode.Fill

        dgvReportResults.SelectionMode =
            DataGridViewSelectionMode.FullRowSelect

        dgvReportResults.MultiSelect = False

        dgvReportResults.ReadOnly = True

        dgvReportResults.AllowUserToAddRows = False

        dgvReportResults.AllowUserToDeleteRows = False

        dgvReportResults.AllowUserToResizeRows = False

        dgvReportResults.AllowUserToOrderColumns = False

        dgvReportResults.RowHeadersVisible = False


        ' =====================================================
        ' FORMAT CURRENCY COLUMNS
        ' =====================================================

        For Each column As DataGridViewColumn In
            dgvReportResults.Columns

            If column.HeaderText.Contains("Amount") OrElse
               column.HeaderText.Contains("Total") Then

                column.DefaultCellStyle.Format =
                    "₱#,##0.00"

            End If

        Next


        ' =====================================================
        ' NO DATA MESSAGE
        ' =====================================================

        If dt.Rows.Count = 0 Then

            MessageBox.Show(
                "No records were found for this report.",
                "Report",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)

        End If

    End Sub


    ' =========================================================
    ' EXPORT REPORT
    ' =========================================================

    Private Sub btnExportReport_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnExportReport.Click

        If dgvReportResults.Rows.Count = 0 Then

            MessageBox.Show(
                "Please generate a report first.",
                "Export Report",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            Exit Sub

        End If


        Try

            Dim saveDialog As New SaveFileDialog()

            saveDialog.Filter =
                "CSV File (*.csv)|*.csv"

            saveDialog.Title =
                "Export Report"

            saveDialog.FileName =
                ReportTitle.Replace(" ", "_") &
                "_" &
                Date.Today.ToString("yyyyMMdd") &
                ".csv"


            If saveDialog.ShowDialog() <>
               DialogResult.OK Then

                Exit Sub

            End If


            Using writer As New StreamWriter(
                saveDialog.FileName,
                False,
                System.Text.Encoding.UTF8)


                ' =================================================
                ' REPORT TITLE
                ' =================================================

                writer.WriteLine(
                    "Register Document Request System")

                writer.WriteLine(
                    ReportTitle)

                writer.WriteLine(
                    "Date Range: " &
                    dtpFromDate.Value.ToString("yyyy-MM-dd") &
                    " to " &
                    dtpToDate.Value.ToString("yyyy-MM-dd"))

                writer.WriteLine()


                ' =================================================
                ' COLUMN HEADERS
                ' =================================================

                For i As Integer =
                    0 To dgvReportResults.Columns.Count - 1

                    writer.Write(
                        """" &
                        dgvReportResults.Columns(i).HeaderText.Replace(
                            """",
                            """""") &
                        """")


                    If i <
                       dgvReportResults.Columns.Count - 1 Then

                        writer.Write(",")

                    End If

                Next

                writer.WriteLine()


                ' =================================================
                ' DATA
                ' =================================================

                For Each row As DataGridViewRow In
                    dgvReportResults.Rows

                    If row.IsNewRow Then
                        Continue For
                    End If


                    For i As Integer =
                        0 To dgvReportResults.Columns.Count - 1

                        Dim value As String = ""


                        If row.Cells(i).Value IsNot Nothing Then

                            value =
                                row.Cells(i).Value.ToString()

                        End If


                        writer.Write(
                            """" &
                            value.Replace(
                                """",
                                """""") &
                            """")


                        If i <
                           dgvReportResults.Columns.Count - 1 Then

                            writer.Write(",")

                        End If

                    Next

                    writer.WriteLine()

                Next

            End Using


            MessageBox.Show(
                "Report exported successfully.",
                "Export Report",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)


        Catch ex As Exception

            MessageBox.Show(
                "Error exporting report: " &
                ex.Message,
                "Export Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)

        End Try

    End Sub


    ' =========================================================
    ' ERROR HANDLER
    ' =========================================================

    Private Sub ShowReportError(
        ex As Exception)

        MessageBox.Show(
            "Error generating report: " &
            ex.Message,
            "Report Error",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error)

    End Sub


    ' =========================================================
    ' CLOSE DATABASE CONNECTION
    ' =========================================================

    Private Sub CloseConnection()

        If cn.State =
           ConnectionState.Open Then

            cn.Close()

        End If

    End Sub


    ' =========================================================
    ' DASHBOARD
    ' =========================================================

    Private Sub btnDashboard_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnDashboard.Click

        frmMainMenu.Show()

        Me.Hide()

    End Sub


    ' =========================================================
    ' STUDENTS
    ' =========================================================

    Private Sub btnStudents_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnStudents.Click

        frmStudents.Show()

        Me.Hide()

    End Sub


    ' =========================================================
    ' DOCUMENTS
    ' =========================================================

    Private Sub btnDocuments_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnDocuments.Click

        frmDocuments.Show()

        Me.Hide()

    End Sub


    ' =========================================================
    ' REQUESTS
    ' =========================================================

    Private Sub btnRequests_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnRequests.Click

        frmRequestList.Show()

        Me.Hide()

    End Sub


    ' =========================================================
    ' USERS
    ' =========================================================

    Private Sub btnUsers_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnUsers.Click

        If CurrentRole <> "Administrator" Then

            MessageBox.Show(
                "You do not have permission to access User Management.",
                "Access Denied",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            Exit Sub

        End If


        frmUsers.Show()

        Me.Hide()

    End Sub


    ' =========================================================
    ' LOGOUT
    ' =========================================================

    Private Sub btnLogout_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnLogout.Click

        Dim result As DialogResult =
            MessageBox.Show(
                "Are you sure you want to logout?",
                "Logout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question)


        If result = DialogResult.Yes Then

            CurrentRole = ""

            frmLogin.Show()

            Me.Hide()

        End If

    End Sub

    Private Sub btnReports_Click(sender As Object, e As EventArgs) Handles btnReports.Click

    End Sub
End Class