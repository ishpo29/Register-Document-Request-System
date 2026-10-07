Imports MySql.Data.MySqlClient




Public Class frmStudents


    '========================================
    ' FORM LOAD
    '========================================
    Public Sub frmStudents_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        ThemeForm(Me)

        LoadStudents()
        ApplyRoleAccess(Me, btnUsers, PictureBoxUsers)
    End Sub


    '========================================
    ' LOAD ALL STUDENTS
    '========================================
    Public Sub LoadStudents()

        Try

            connection()

            sql = "SELECT StudentID, LRN, LastName, FirstName, MiddleName, " &
                  "Course, YearLevel, Section, ContactNo, Status " &
                  "FROM tblstudents " &
                  "ORDER BY StudentID"

            da = New MySqlDataAdapter(sql, cn)

            dt = New DataTable()

            da.Fill(dt)

            dgvStudents.DataSource = dt
            ShowEmptyHint(dgvStudents, "No students found. Click Add Student to create one, or clear the search box.")

            FormatStudentGrid()

        Catch ex As Exception

            MessageBox.Show("Error loading student records: " & ex.Message,
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
    ' FORMAT DATAGRIDVIEW
    '========================================
    Private Sub FormatStudentGrid()
        dgvStudents.AllowUserToResizeColumns = False
        If dgvStudents.Columns.Count = 0 Then
            Exit Sub
        End If

        dgvStudents.ReadOnly = True

        dgvStudents.SelectionMode =
        DataGridViewSelectionMode.FullRowSelect

        dgvStudents.MultiSelect = False

        dgvStudents.AllowUserToAddRows = False

        dgvStudents.AllowUserToDeleteRows = False

        dgvStudents.RowHeadersVisible = False

        ' Disable editing and column sorting
        For Each column As DataGridViewColumn In dgvStudents.Columns

            column.ReadOnly = True

            column.SortMode =
            DataGridViewColumnSortMode.NotSortable

        Next

    End Sub

    '========================================
    ' SEARCH BUTTON
    '========================================

    '========================================
    ' SEARCH STUDENTS
    '========================================
    Private Sub SearchStudents()

        Try

            connection()

            sql = "SELECT StudentID, LRN, LastName, FirstName, MiddleName, " &
                  "Course, YearLevel, Section, ContactNo, Status " &
                  "FROM tblstudents " &
                  "WHERE StudentID LIKE @Search " &
                  "OR LRN LIKE @Search " &
                  "OR LastName LIKE @Search " &
                  "OR FirstName LIKE @Search " &
                  "OR Course LIKE @Search " &
                  "ORDER BY StudentID"

            cmd = New MySqlCommand(sql, cn)

            cmd.Parameters.AddWithValue("@Search",
                                         "%" & txtSearch.Text.Trim() & "%")

            da = New MySqlDataAdapter(cmd)

            dt = New DataTable()

            da.Fill(dt)

            dgvStudents.DataSource = dt
            ShowEmptyHint(dgvStudents, "No students found. Click Add Student To create one, Or clear the search box.")

            FormatStudentGrid()

        Catch ex As Exception

            MessageBox.Show("Error searching students: " & ex.Message,
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
    ' SEARCH USING ENTER KEY
    '========================================


    '========================================
    ' ADD STUDENT
    '========================================
    Private Sub btnAddStudent_Click(sender As Object, e As EventArgs) Handles btnAddStudent.Click

        frmAddStudents.Show()
        Me.Hide()

    End Sub


    '========================================
    ' EDIT STUDENT
    '========================================
    Private Sub btnEdit_Click(sender As Object, e As EventArgs) Handles btnEdit.Click

        If dgvStudents.CurrentRow Is Nothing Then

            MessageBox.Show("Please select a student first.",
                            "Edit Student",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning)

            Exit Sub

        End If

        Dim studentID As String =
            dgvStudents.CurrentRow.Cells("StudentID").Value.ToString()

        frmAddStudents.StudentIDToEdit = studentID

        frmAddStudents.Show()

        Me.Hide()

    End Sub


    '========================================
    ' VIEW STUDENT
    '========================================
    Private Sub btnView_Click(sender As Object, e As EventArgs) Handles btnView.Click

        If dgvStudents.CurrentRow Is Nothing Then

            MessageBox.Show("Please select a student first.",
                            "View Student",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning)

            Exit Sub

        End If

        Dim row As DataGridViewRow = dgvStudents.CurrentRow

        Dim information As String =
            "Student ID: " & row.Cells("StudentID").Value.ToString() & vbCrLf &
            "LRN: " & row.Cells("LRN").Value.ToString() & vbCrLf &
            "Last Name: " & row.Cells("LastName").Value.ToString() & vbCrLf &
            "First Name: " & row.Cells("FirstName").Value.ToString() & vbCrLf &
            "Middle Name: " & row.Cells("MiddleName").Value.ToString() & vbCrLf &
            "Course: " & row.Cells("Course").Value.ToString() & vbCrLf &
            "Year Level: " & row.Cells("YearLevel").Value.ToString() & vbCrLf &
            "Section: " & row.Cells("Section").Value.ToString() & vbCrLf &
            "Contact No: " & row.Cells("ContactNo").Value.ToString() & vbCrLf &
            "Status: " & row.Cells("Status").Value.ToString()

        MessageBox.Show(information,
                        "Student Information",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information)

    End Sub


    '========================================
    ' DEACTIVATE STUDENT
    '========================================
    Private Sub btnDeactivate_Click(sender As Object, e As EventArgs) Handles btnDeactivate.Click

        If dgvStudents.CurrentRow Is Nothing Then

            MessageBox.Show("Please select a student first.",
                            "Deactivate Student",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning)

            Exit Sub

        End If

        Dim studentID As String =
            dgvStudents.CurrentRow.Cells("StudentID").Value.ToString()

        Dim studentName As String =
            dgvStudents.CurrentRow.Cells("FirstName").Value.ToString() &
            " " &
            dgvStudents.CurrentRow.Cells("LastName").Value.ToString()

        Dim result As DialogResult =
            MessageBox.Show(
                "Are you sure you want to deactivate " &
                studentName & "?",
                "Deactivate Student",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question)

        If result = DialogResult.No Then
            Exit Sub
        End If

        Try

            connection()

            sql = "UPDATE tblstudents " &
                  "SET Status = 'Inactive' " &
                  "WHERE StudentID = @StudentID"

            cmd = New MySqlCommand(sql, cn)

            cmd.Parameters.AddWithValue("@StudentID", studentID)

            cmd.ExecuteNonQuery()

            MessageBox.Show("Student has been deactivated.",
                            "Success",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information)

            LoadStudents()

        Catch ex As Exception

            MessageBox.Show("Error deactivating student: " & ex.Message,
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
    ' MAIN DASHBOARD
    '========================================
    Private Sub btnDashboard_Click(sender As Object, e As EventArgs) Handles btnDashboard.Click

        frmMainMenu.Show()
        Me.Close()

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

            frmLogin.Show()
            Me.Close()

        End If

    End Sub

    Private Sub txtSearch_TextChanged(
    sender As Object,
    e As EventArgs
) Handles txtSearch.TextChanged

        SearchStudents()

    End Sub
End Class