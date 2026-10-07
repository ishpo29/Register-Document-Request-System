Imports MySql.Data.MySqlClient

Public Class frmDocuments

    ' ==========================================
    ' FORM LOAD
    ' ==========================================
    Public Sub frmDocuments_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        ThemeForm(Me)

        ApplyRoleAccess(Me, btnUsers, PictureBoxUsers)

        LoadDocuments()

    End Sub


    ' ==========================================
    ' LOAD DOCUMENTS
    ' ==========================================
    Public Sub LoadDocuments()

        Try

            connection()

            sql = "SELECT " &
                  "DocumentID, " &
                  "DocumentName, " &
                  "Description, " &
                  "Fee, " &
                  "Status " &
                  "FROM tbldocuments " &
                  "ORDER BY DocumentID ASC"

            da = New MySqlDataAdapter(sql, cn)

            dt = New DataTable()

            da.Fill(dt)

            dgvDocuments.DataSource = dt

            ShowEmptyHint(
                dgvDocuments,
                "No documents found. Click Add Document to create one, or clear the search box."
            )

            FormatDocumentGrid()

        Catch ex As Exception

            MessageBox.Show(
                "Error loading documents: " & ex.Message,
                "Database Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )

        Finally

            If cn.State = ConnectionState.Open Then
                cn.Close()
            End If

        End Try

    End Sub


    ' ==========================================
    ' FORMAT DATAGRIDVIEW
    ' ==========================================
    Private Sub FormatDocumentGrid()

        If dgvDocuments.Columns.Count = 0 Then
            Exit Sub
        End If

        dgvDocuments.AutoSizeColumnsMode =
            DataGridViewAutoSizeColumnsMode.Fill

        dgvDocuments.SelectionMode =
            DataGridViewSelectionMode.FullRowSelect

        dgvDocuments.MultiSelect = False

        dgvDocuments.ReadOnly = True

        dgvDocuments.AllowUserToAddRows = False

        dgvDocuments.AllowUserToDeleteRows = False

        dgvDocuments.AllowUserToResizeColumns = False

        dgvDocuments.RowHeadersVisible = False
        ' ==========================================
        ' FONT SETTINGS
        ' ==========================================

        dgvDocuments.DefaultCellStyle.Font =
    New Font("Segoe UI", 14.0F, FontStyle.Regular)

        dgvDocuments.RowsDefaultCellStyle.Font =
    New Font("Segoe UI", 14.0F, FontStyle.Regular)

        dgvDocuments.ColumnHeadersDefaultCellStyle.Font =
    New Font("Segoe UI", 14.0F, FontStyle.Bold)


        ' ==========================================
        ' COLUMN HEADERS
        ' ==========================================

        If dgvDocuments.Columns.Contains("DocumentID") Then

            dgvDocuments.Columns("DocumentID").HeaderText =
                "Document ID"

        End If


        If dgvDocuments.Columns.Contains("DocumentName") Then

            dgvDocuments.Columns("DocumentName").HeaderText =
                "Document Name"

        End If


        If dgvDocuments.Columns.Contains("Description") Then

            dgvDocuments.Columns("Description").HeaderText =
                "Description"

        End If


        If dgvDocuments.Columns.Contains("Fee") Then

            dgvDocuments.Columns("Fee").HeaderText =
                "Fee"

            dgvDocuments.Columns("Fee").DefaultCellStyle.Format =
                "₱#,##0.00"

        End If


        If dgvDocuments.Columns.Contains("Status") Then

            dgvDocuments.Columns("Status").HeaderText =
                "Status"

        End If


        ' ==========================================
        ' DISABLE EDITING AND SORTING
        ' ==========================================

        For Each column As DataGridViewColumn In dgvDocuments.Columns

            column.ReadOnly = True

            column.SortMode =
                DataGridViewColumnSortMode.NotSortable

        Next

    End Sub


    ' ==========================================
    ' LIVE SEARCH
    ' ==========================================
    Private Sub txtSearch_TextChanged(
        sender As Object,
        e As EventArgs
    ) Handles txtSearch.TextChanged

        SearchDocuments()

    End Sub


    ' ==========================================
    ' SEARCH DOCUMENTS
    ' ==========================================
    Private Sub SearchDocuments()

        Try

            connection()

            sql = "SELECT " &
                  "DocumentID, " &
                  "DocumentName, " &
                  "Description, " &
                  "Fee, " &
                  "Status " &
                  "FROM tbldocuments " &
                  "WHERE DocumentID LIKE @Search " &
                  "OR DocumentName LIKE @Search " &
                  "OR Description LIKE @Search " &
                  "OR Status LIKE @Search " &
                  "ORDER BY DocumentID ASC"

            cmd = New MySqlCommand(sql, cn)

            cmd.Parameters.AddWithValue(
                "@Search",
                "%" & txtSearch.Text.Trim() & "%"
            )

            da = New MySqlDataAdapter(cmd)

            dt = New DataTable()

            da.Fill(dt)

            dgvDocuments.DataSource = dt

            ShowEmptyHint(
                dgvDocuments,
                "No documents found. Click Add Document to create one, or clear the search box."
            )

            FormatDocumentGrid()

        Catch ex As Exception

            MessageBox.Show(
                "Error searching documents: " & ex.Message,
                "Search Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )

        Finally

            If cn.State = ConnectionState.Open Then
                cn.Close()
            End If

        End Try

    End Sub


    ' ==========================================
    ' ADD DOCUMENT
    ' ==========================================
    Private Sub btnAddDocument_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnAddDocument.Click

        frmAddDocuments.DocumentIDToEdit = ""

        frmAddDocuments.Show()

        Me.Hide()

    End Sub


    ' ==========================================
    ' EDIT DOCUMENT
    ' ==========================================
    Private Sub btnEdit_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnEdit.Click

        If dgvDocuments.SelectedRows.Count = 0 Then

            MessageBox.Show(
                "Please select a document first.",
                "Edit Document",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            )

            Exit Sub

        End If


        Dim documentID As String =
            dgvDocuments.SelectedRows(0).Cells("DocumentID").Value.ToString()


        frmAddDocuments.DocumentIDToEdit =
            documentID

        frmAddDocuments.Show()

        Me.Hide()

    End Sub


    ' ==========================================
    ' VIEW DOCUMENT
    ' ==========================================
    Private Sub btnView_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnView.Click

        If dgvDocuments.SelectedRows.Count = 0 Then

            MessageBox.Show(
                "Please select a document first.",
                "View Document",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            )

            Exit Sub

        End If


        Dim row As DataGridViewRow =
            dgvDocuments.SelectedRows(0)


        Dim documentID As String =
            row.Cells("DocumentID").Value.ToString()


        Dim documentName As String =
            row.Cells("DocumentName").Value.ToString()


        Dim description As String =
            row.Cells("Description").Value.ToString()


        Dim fee As Decimal =
            Convert.ToDecimal(row.Cells("Fee").Value)


        Dim status As String =
            row.Cells("Status").Value.ToString()


        MessageBox.Show(
            "Document ID: " & documentID & vbCrLf &
            "Document Name: " & documentName & vbCrLf &
            "Description: " & description & vbCrLf &
            "Fee: ₱" & fee.ToString("N2") & vbCrLf &
            "Status: " & status,
            "Document Details",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        )

    End Sub





    ' ==========================================
    ' DASHBOARD
    ' ==========================================
    Private Sub btnDashboard_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnDashboard.Click

        frmMainMenu.Show()

        Me.Hide()

    End Sub


    ' ==========================================
    ' STUDENTS
    ' ==========================================
    Private Sub btnStudents_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnStudents.Click

        frmStudents.Show()

        Me.Hide()

    End Sub


    ' ==========================================
    ' REQUESTS
    ' ==========================================
    Private Sub btnRequests_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnRequests.Click

        frmRequestList.Show()

        Me.Hide()

    End Sub


    ' ==========================================
    ' REPORTS
    ' ==========================================
    Private Sub btnReports_Click(
    sender As Object,
    e As EventArgs
) Handles btnReports.Click

        frmReports.ResetReportForm()
        frmReports.Show()
        frmReports.BringToFront()

        Me.Hide()

    End Sub


    ' ==========================================
    ' USERS
    ' ==========================================
    Private Sub btnUsers_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnUsers.Click

        If CurrentRole <> "Administrator" Then

            MessageBox.Show(
                "You do not have permission to access User Management.",
                "Access Denied",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            )

            Exit Sub

        End If


        frmUsers.Show()

        Me.Hide()

    End Sub


    ' ==========================================
    ' LOGOUT
    ' ==========================================
    Private Sub btnLogout_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnLogout.Click

        Dim result As DialogResult =
            MessageBox.Show(
                "Are you sure you want to logout?",
                "Logout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            )


        If result = DialogResult.Yes Then

            CurrentRole = ""

            frmLogin.Show()

            Me.Hide()

        End If

    End Sub

End Class