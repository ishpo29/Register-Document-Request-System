Imports MySql.Data.MySqlClient

Public Class frmRequestList


    ' =========================================================
    ' FORM LOAD
    ' =========================================================

    Private Sub frmRequestList_Load(
        sender As Object,
        e As EventArgs
    ) Handles MyBase.Load

        ThemeForm(Me)

        ApplyRoleAccess(
            Me,
            btnUsers,
            PictureBoxUsers)


        ' =====================================================
        ' DEFAULT FILTER = ALL
        ' =====================================================

        If cboFilter.Items.Count > 0 Then

            cboFilter.SelectedIndex = 0

        End If


        LoadRequests()

    End Sub


    ' =========================================================
    ' FORM ACTIVATED
    ' Reload whenever returning to this form
    ' =========================================================

    Private Sub frmRequestList_Activated(
        sender As Object,
        e As EventArgs
    ) Handles MyBase.Activated

        LoadRequests()

    End Sub


    ' =========================================================
    ' LOAD / SEARCH / FILTER REQUESTS
    ' =========================================================

    Public Sub LoadRequests()

        Try

            connection()


            sql =
                "SELECT " &
                "r.RequestID, " &
                "r.RequestNo, " &
                "r.RequestDate, " &
                "r.StudentID, " &
                "CONCAT(s.FirstName, ' ', " &
                "s.MiddleName, ' ', " &
                "s.LastName) AS StudentName, " &
                "GROUP_CONCAT(d.DocumentName SEPARATOR ', ') AS Documents, " &
                "r.TotalAmount, " &
                "r.PaymentStatus, " &
                "r.Status, " &
                "CASE " &
                "WHEN r.Status = 'Processing' THEN 'High' " &
                "WHEN r.Status = 'Ready for Release' THEN 'High' " &
                "WHEN r.Status = 'Pending' THEN 'Medium' " &
                "WHEN r.Status = 'Released' THEN 'Completed' " &
                "ELSE 'Medium' " &
                "END AS Priority " &
                "FROM tblrequest r " &
                "INNER JOIN tblstudents s " &
                "ON r.StudentID = s.StudentID " &
                "INNER JOIN tblrequestdetails rd " &
                "ON r.RequestID = rd.RequestID " &
                "INNER JOIN tbldocuments d " &
                "ON rd.DocumentID = d.DocumentID "


            ' =================================================
            ' FILTER
            ' =================================================

            If cboFilter.Text <> "" AndAlso
               cboFilter.Text <> "All" Then


                If cboFilter.Text = "High" Then

                    sql =
                        sql &
                        "WHERE (r.Status = 'Processing' " &
                        "OR r.Status = 'Ready for Release') "


                ElseIf cboFilter.Text = "Medium" Then

                    sql =
                        sql &
                        "WHERE r.Status = 'Pending' "


                ElseIf cboFilter.Text = "Completed" Then

                    sql =
                        sql &
                        "WHERE r.Status = 'Released' "


                Else

                    sql =
                        sql &
                        "WHERE r.Status = @Status "

                End If


                ' =================================================
                ' SEARCH WITH FILTER
                ' =================================================

                If txtSearch.Text.Trim() <> "" Then

                    sql =
                        sql &
                        "AND (" &
                        "r.RequestNo LIKE @Search " &
                        "OR r.StudentID LIKE @Search " &
                        "OR CONCAT(s.FirstName, ' ', " &
                        "s.MiddleName, ' ', " &
                        "s.LastName) LIKE @Search " &
                        "OR d.DocumentName LIKE @Search " &
                        "OR DATE_FORMAT(r.RequestDate, '%Y-%m-%d') LIKE @Search " &
                        "OR r.Status LIKE @Search " &
                        "OR r.PaymentStatus LIKE @Search" &
                        ") "

                End If


            Else

                ' =================================================
                ' SEARCH WITHOUT FILTER
                ' =================================================

                If txtSearch.Text.Trim() <> "" Then

                    sql =
                        sql &
                        "WHERE (" &
                        "r.RequestNo LIKE @Search " &
                        "OR r.StudentID LIKE @Search " &
                        "OR CONCAT(s.FirstName, ' ', " &
                        "s.MiddleName, ' ', " &
                        "s.LastName) LIKE @Search " &
                        "OR d.DocumentName LIKE @Search " &
                        "OR r.Status LIKE @Search " &
                        "OR r.PaymentStatus LIKE @Search" &
                        ") "

                End If

            End If


            ' =================================================
            ' GROUP BY
            ' =================================================

            sql =
                sql &
                "GROUP BY r.RequestID, " &
                "r.RequestNo, " &
                "r.RequestDate, " &
                "r.StudentID, " &
                "s.FirstName, " &
                "s.MiddleName, " &
                "s.LastName, " &
                "r.TotalAmount, " &
                "r.PaymentStatus, " &
                "r.Status "


            ' =================================================
            ' ORDER
            ' =================================================

            sql =
                sql &
                "ORDER BY " &
                "CASE " &
                "WHEN r.Status = 'Processing' THEN 1 " &
                "WHEN r.Status = 'Ready for Release' THEN 2 " &
                "WHEN r.Status = 'Pending' THEN 3 " &
                "WHEN r.Status = 'Released' THEN 4 " &
                "ELSE 5 " &
                "END, " &
                "r.RequestDate ASC, " &
                "r.RequestID DESC"


            ' =================================================
            ' COMMAND
            ' =================================================

            cmd =
                New MySqlCommand(
                    sql,
                    cn)


            ' =================================================
            ' ONLY ADD @STATUS FOR ACTUAL STATUS FILTERS
            ' =================================================

            If cboFilter.Text <> "" AndAlso
               cboFilter.Text <> "All" AndAlso
               cboFilter.Text <> "High" AndAlso
               cboFilter.Text <> "Medium" AndAlso
               cboFilter.Text <> "Completed" Then

                cmd.Parameters.AddWithValue(
                    "@Status",
                    cboFilter.Text)

            End If


            ' =================================================
            ' SEARCH PARAMETER
            ' =================================================

            If txtSearch.Text.Trim() <> "" Then

                cmd.Parameters.AddWithValue(
                    "@Search",
                    "%" &
                    txtSearch.Text.Trim() &
                    "%")

            End If


            ' =================================================
            ' LOAD DATA
            ' =================================================

            da =
                New MySqlDataAdapter(cmd)

            dt =
                New DataTable()

            da.Fill(dt)


            dgvRequests.DataSource =
                dt


            ShowEmptyHint(
                dgvRequests,
                "No requests found for the current filter or search.")


            FormatRequestGrid()


        Catch ex As Exception

            MessageBox.Show(
                "Error loading requests: " &
                ex.Message,
                "Database Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)


        Finally

            If cn.State =
                ConnectionState.Open Then

                cn.Close()

            End If

        End Try

    End Sub


    ' =========================================================
    ' FORMAT DATAGRIDVIEW
    ' =========================================================

    Private Sub FormatRequestGrid()

        If dgvRequests.Columns.Count = 0 Then

            Exit Sub

        End If


        dgvRequests.AutoSizeColumnsMode =
            DataGridViewAutoSizeColumnsMode.Fill

        dgvRequests.SelectionMode =
            DataGridViewSelectionMode.FullRowSelect

        dgvRequests.MultiSelect =
            False

        dgvRequests.ReadOnly =
            True

        dgvRequests.AllowUserToAddRows =
            False

        dgvRequests.AllowUserToDeleteRows =
            False

        dgvRequests.AllowUserToResizeColumns =
            False

        dgvRequests.RowHeadersVisible =
            False


        ' =====================================================
        ' HIDE REQUEST ID
        ' =====================================================

        If dgvRequests.Columns.Contains(
            "RequestID") Then

            dgvRequests.Columns(
                "RequestID").Visible = False

        End If


        ' =====================================================
        ' REQUEST NUMBER
        ' =====================================================

        If dgvRequests.Columns.Contains(
            "RequestNo") Then

            dgvRequests.Columns(
                "RequestNo").HeaderText =
                "Request No."

        End If


        ' =====================================================
        ' REQUEST DATE
        ' =====================================================

        If dgvRequests.Columns.Contains(
            "RequestDate") Then

            dgvRequests.Columns(
                "RequestDate").HeaderText =
                "Request Date"

            dgvRequests.Columns(
                "RequestDate").DefaultCellStyle.Format =
                "MMM dd, yyyy"

        End If


        ' =====================================================
        ' STUDENT ID
        ' =====================================================

        If dgvRequests.Columns.Contains(
            "StudentID") Then

            dgvRequests.Columns(
                "StudentID").HeaderText =
                "Student ID"

        End If


        ' =====================================================
        ' STUDENT NAME
        ' =====================================================

        If dgvRequests.Columns.Contains(
            "StudentName") Then

            dgvRequests.Columns(
                "StudentName").HeaderText =
                "Student Name"

        End If


        ' =====================================================
        ' DOCUMENTS
        ' =====================================================

        If dgvRequests.Columns.Contains(
            "Documents") Then

            dgvRequests.Columns(
                "Documents").HeaderText =
                "Document(s)"

        End If


        ' =====================================================
        ' TOTAL AMOUNT
        ' =====================================================

        If dgvRequests.Columns.Contains(
            "TotalAmount") Then

            dgvRequests.Columns(
                "TotalAmount").HeaderText =
                "Amount"

            dgvRequests.Columns(
                "TotalAmount").DefaultCellStyle.Format =
                "₱#,##0.00"

        End If


        ' =====================================================
        ' PAYMENT STATUS
        ' =====================================================

        If dgvRequests.Columns.Contains(
            "PaymentStatus") Then

            dgvRequests.Columns(
                "PaymentStatus").HeaderText =
                "Payment"

        End If


        ' =====================================================
        ' REQUEST STATUS
        ' =====================================================

        If dgvRequests.Columns.Contains(
            "Status") Then

            dgvRequests.Columns(
                "Status").HeaderText =
                "Status"

        End If


        ' =====================================================
        ' PRIORITY
        ' =====================================================

        If dgvRequests.Columns.Contains(
            "Priority") Then

            dgvRequests.Columns(
                "Priority").HeaderText =
                "Priority"

        End If


        ' =====================================================
        ' FORMAT STATUS AND PRIORITY
        ' =====================================================

        For Each row As DataGridViewRow
            In dgvRequests.Rows

            If row.IsNewRow Then

                Continue For

            End If


            ' =================================================
            ' STATUS
            ' =================================================

            If dgvRequests.Columns.Contains(
                "Status") Then

                Dim status As String =
                    Convert.ToString(
                        row.Cells(
                            "Status").Value)


                Select Case status

                    Case "Processing"

                        row.Cells(
                            "Status").Style.Font =
                            New Font(
                                dgvRequests.Font,
                                FontStyle.Bold)


                    Case "Ready for Release"

                        row.Cells(
                            "Status").Style.Font =
                            New Font(
                                dgvRequests.Font,
                                FontStyle.Bold)


                    Case "Pending"

                        row.Cells(
                            "Status").Style.Font =
                            New Font(
                                dgvRequests.Font,
                                FontStyle.Bold)


                    Case "Released"

                        row.Cells(
                            "Status").Style.Font =
                            New Font(
                                dgvRequests.Font,
                                FontStyle.Bold)

                End Select

            End If


            ' =================================================
            ' PRIORITY
            ' =================================================

            If dgvRequests.Columns.Contains(
                "Priority") Then

                Dim priority As String =
                    Convert.ToString(
                        row.Cells(
                            "Priority").Value)


                Select Case priority

                    Case "High"

                        row.Cells(
                            "Priority").Style.Font =
                            New Font(
                                dgvRequests.Font,
                                FontStyle.Bold)


                    Case "Medium"

                        row.Cells(
                            "Priority").Style.Font =
                            New Font(
                                dgvRequests.Font,
                                FontStyle.Bold)


                    Case "Completed"

                        row.Cells(
                            "Priority").Style.Font =
                            New Font(
                                dgvRequests.Font,
                                FontStyle.Bold)

                End Select

            End If

        Next


        ' =====================================================
        ' DISABLE EDITING AND SORTING
        ' =====================================================

        For Each column As DataGridViewColumn
            In dgvRequests.Columns

            column.ReadOnly =
                True

            column.SortMode =
                DataGridViewColumnSortMode.NotSortable

        Next

    End Sub


    ' =========================================================
    ' LIVE SEARCH
    ' =========================================================

    Private Sub txtSearch_TextChanged(
        sender As Object,
        e As EventArgs
    ) Handles txtSearch.TextChanged

        LoadRequests()

    End Sub


    ' =========================================================
    ' FILTER
    ' =========================================================

    Private Sub cboFilter_SelectedIndexChanged(
        sender As Object,
        e As EventArgs
    ) Handles cboFilter.SelectedIndexChanged

        LoadRequests()

    End Sub


    ' =========================================================
    ' NEW REQUEST
    ' =========================================================

    Private Sub btnNewRequest_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnNewRequest.Click

        'Always start with a clean New Request form
        frmNewRequest.ResetForm()

        frmNewRequest.Show()

        Me.Hide()

    End Sub


    ' =========================================================
    ' VIEW DETAILS
    ' =========================================================

    Private Sub btnViewDetails_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnViewDetails.Click

        If dgvRequests.SelectedRows.Count = 0 Then

            MessageBox.Show(
                "Please select a request first.",
                "View Details",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            Exit Sub

        End If


        Dim requestID As Integer =
            Convert.ToInt32(
                dgvRequests.SelectedRows(0).
                Cells("RequestID").
                Value)


        frmRequestDetails.RequestIDToView =
            requestID


        frmRequestDetails.Show()

        Me.Hide()

    End Sub


    ' =========================================================
    ' DOUBLE CLICK TO VIEW DETAILS
    ' =========================================================

    Private Sub dgvRequests_CellDoubleClick(
        sender As Object,
        e As DataGridViewCellEventArgs
    ) Handles dgvRequests.CellDoubleClick

        If e.RowIndex < 0 Then

            Exit Sub

        End If


        btnViewDetails.PerformClick()

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
    ' REPORTS
    ' =========================================================

    Private Sub btnReports_Click(
    sender As Object,
    e As EventArgs
) Handles btnReports.Click

        frmReports.ResetReportForm()
        frmReports.Show()
        frmReports.BringToFront()

        Me.Hide()

    End Sub


    ' =========================================================
    ' USERS
    ' =========================================================

    Private Sub btnUsers_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnUsers.Click

        If CurrentRole <>
            "Administrator" Then

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


        If result =
            DialogResult.Yes Then

            CurrentRole = ""

            frmLogin.Show()

            Me.Hide()

        End If

    End Sub


    ' =========================================================
    ' PANEL PAINT
    ' =========================================================

    Private Sub Panel2_Paint(
        sender As Object,
        e As PaintEventArgs
    )

    End Sub

End Class