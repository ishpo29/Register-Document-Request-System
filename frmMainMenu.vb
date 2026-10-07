Imports MySql.Data.MySqlClient

Public Class frmMainMenu



    '========================================
    ' FORM LOAD
    '========================================
    Private Sub frmMainMenu_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        ThemeForm(Me)

        LoadDashboard()

        If CurrentRole = "Administrator" Then

            btnUsers.Visible = True
            PictureBoxUsers.Visible = True

        Else

            btnUsers.Visible = False
            PictureBoxUsers.Visible = False

        End If

    End Sub


    '========================================
    ' LOAD DASHBOARD
    '========================================
    Private Sub LoadDashboard()

        LoadStudentCount()
        LoadDocumentCount()
        LoadRequestCount()
        LoadRecentRequests()

    End Sub
    '========================================
    ' REFRESH DASHBOARD
    '========================================
    Private Sub frmMainMenu_Activated(
    sender As Object,
    e As EventArgs
) Handles MyBase.Activated

        LoadDashboard()

    End Sub

    '========================================
    ' STUDENT COUNT
    '========================================
    Private Sub LoadStudentCount()

        Try

            connection()

            sql = "SELECT COUNT(*) FROM tblstudents"

            cmd = New MySqlCommand(sql, cn)

            Dim count As Integer = Convert.ToInt32(cmd.ExecuteScalar())

            lblStudentCount.Text = count.ToString()

        Catch ex As Exception

            MessageBox.Show("Error loading student count: " & ex.Message,
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error)

        Finally

            If cn.State = ConnectionState.Open Then
                cn.Close()
            End If

        End Try

    End Sub


    '========================================
    ' DOCUMENT COUNT
    '========================================
    Private Sub LoadDocumentCount()

        Try

            connection()

            sql = "SELECT COUNT(*) FROM tbldocuments"

            cmd = New MySqlCommand(sql, cn)

            Dim count As Integer = Convert.ToInt32(cmd.ExecuteScalar())

            lblDocumentCount.Text = count.ToString()

        Catch ex As Exception

            MessageBox.Show("Error loading document count: " & ex.Message,
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error)

        Finally

            If cn.State = ConnectionState.Open Then
                cn.Close()
            End If

        End Try

    End Sub


    '========================================
    ' REQUEST COUNT
    '========================================
    Private Sub LoadRequestCount()

        Try

            connection()

            sql = "SELECT COUNT(*) FROM tblrequest"

            cmd = New MySqlCommand(sql, cn)

            Dim count As Integer = Convert.ToInt32(cmd.ExecuteScalar())

            lblRequestCount.Text = count.ToString()

        Catch ex As Exception

            MessageBox.Show("Error loading request count: " & ex.Message,
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error)

        Finally

            If cn.State = ConnectionState.Open Then
                cn.Close()
            End If

        End Try

    End Sub


    '========================================
    ' RECENT REQUESTS
    '========================================
    Private Sub LoadRecentRequests()

        Try

            connection()

            sql = "SELECT r.RequestNo, " &
                  "CONCAT(s.FirstName, ' ', s.LastName) AS StudentName, " &
                  "r.Status, " &
                  "r.TotalAmount " &
                  "FROM tblrequest r " &
                  "INNER JOIN tblstudents s ON r.StudentID = s.StudentID " &
                  "ORDER BY r.RequestDate DESC, r.RequestID DESC " &
                  "LIMIT 10"

            da = New MySqlDataAdapter(sql, cn)

            dt = New DataTable()

            da.Fill(dt)

            dgvRecentRequests.DataSource = dt

            FormatRecentRequests()

        Catch ex As Exception

            MessageBox.Show("Error loading recent requests: " & ex.Message,
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error)

        Finally

            If cn.State = ConnectionState.Open Then
                cn.Close()
            End If

        End Try

    End Sub


    '========================================
    ' FORMAT RECENT REQUESTS
    '========================================
    Private Sub FormatRecentRequests()

        If dgvRecentRequests.Columns.Count = 0 Then
            Exit Sub
        End If

        '========================================
        ' GENERAL DGV SETTINGS
        '========================================

        dgvRecentRequests.AutoSizeColumnsMode =
        DataGridViewAutoSizeColumnsMode.Fill

        dgvRecentRequests.SelectionMode =
        DataGridViewSelectionMode.FullRowSelect

        dgvRecentRequests.MultiSelect = False

        dgvRecentRequests.ReadOnly = True

        dgvRecentRequests.AllowUserToAddRows = False

        dgvRecentRequests.RowHeadersVisible = False


        '========================================
        ' FONT SETTINGS
        '========================================

        ' Main row text
        dgvRecentRequests.DefaultCellStyle.Font =
        New Font("Segoe UI", 11.5F, FontStyle.Regular)

        ' Make sure all rows use the same font
        dgvRecentRequests.RowsDefaultCellStyle.Font =
        New Font("Segoe UI", 11.5F, FontStyle.Regular)

        ' Header text
        dgvRecentRequests.ColumnHeadersDefaultCellStyle.Font =
        New Font("Segoe UI", 12.0F, FontStyle.Bold)


        '========================================
        ' ROW HEIGHT
        '========================================

        dgvRecentRequests.RowTemplate.Height = 32

        dgvRecentRequests.ColumnHeadersHeight = 35


        '========================================
        ' COLUMN HEADERS
        '========================================

        dgvRecentRequests.Columns("RequestNo").HeaderText = "Request No"
        dgvRecentRequests.Columns("StudentName").HeaderText = "Student"
        dgvRecentRequests.Columns("Status").HeaderText = "Status"
        dgvRecentRequests.Columns("TotalAmount").HeaderText = "Amount"


        '========================================
        ' AMOUNT FORMAT
        '========================================

        dgvRecentRequests.Columns("TotalAmount").DefaultCellStyle.Format = "₱#,##0.00"


        '========================================
        ' DISABLE SORTING
        '========================================

        dgvRecentRequests.Columns("RequestNo").SortMode =
    DataGridViewColumnSortMode.NotSortable

        dgvRecentRequests.Columns("StudentName").SortMode =
    DataGridViewColumnSortMode.NotSortable

        dgvRecentRequests.Columns("Status").SortMode =
    DataGridViewColumnSortMode.NotSortable

        dgvRecentRequests.Columns("TotalAmount").SortMode =
    DataGridViewColumnSortMode.NotSortable

    End Sub


    '========================================
    ' STUDENTS
    '========================================
    Private Sub btnStudents_Click(sender As Object, e As EventArgs) Handles btnStudents.Click

        frmStudents.Show()
        Me.Hide()

    End Sub


    '========================================
    ' DOCUMENTS
    '========================================
    Private Sub btnDocuments_Click(sender As Object, e As EventArgs) Handles btnDocuments.Click

        frmDocuments.Show()
        Me.Hide()

    End Sub


    '========================================
    ' REQUESTS
    '========================================
    Private Sub btnRequests_Click(sender As Object, e As EventArgs) Handles btnRequests.Click

        frmRequestList.Show()
        Me.Hide()

    End Sub


    '========================================
    ' REPORTS
    '========================================
    Private Sub btnReports_Click(
    sender As Object,
    e As EventArgs
) Handles btnReports.Click

        frmReports.ResetReportForm()
        frmReports.Show()
        frmReports.BringToFront()

        Me.Hide()

    End Sub


    '========================================
    ' USERS
    '========================================
    Private Sub btnUsers_Click(sender As Object, e As EventArgs) Handles btnUsers.Click

        If CurrentRole <> "Administrator" Then

            MessageBox.Show("You do not have permission to access User Management.",
                            "Access Denied",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning)

            Exit Sub

        End If

        frmUsers.Show()
        Me.Hide()

    End Sub


    '========================================
    ' LOGOUT
    '========================================
    Private Sub btnLogout_Click(sender As Object, e As EventArgs) Handles btnLogout.Click

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


End Class