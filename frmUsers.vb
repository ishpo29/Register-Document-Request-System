Imports MySql.Data.MySqlClient

Public Class frmUsers

    ' ==========================================================
    ' VARIABLES
    ' ==========================================================

    Private SelectedUserID As Integer = 0

    Private ep As ErrorProvider


    ' ==========================================================
    ' FORM LOAD
    ' ==========================================================

    Private Sub frmUsers_Load(
        sender As Object,
        e As EventArgs
    ) Handles MyBase.Load

        ThemeForm(Me)

        ep = NewErrorProvider(Me)

        ' ------------------------------------------------------
        ' TEXTBOX SETTINGS
        ' ------------------------------------------------------

        txtUsername.MaxLength = 30
        txtPassword.MaxLength = 64
        txtPassword.UseSystemPasswordChar = True

        txtFullName.MaxLength = 100

        RestrictToName(txtFullName, 100)

        ClearErrorOnEdit(
            ep,
            txtUsername,
            txtPassword,
            txtFullName,
            cboRole
        )


        ' ------------------------------------------------------
        ' TOOL TIPS
        ' ------------------------------------------------------

        Dim tip As New ToolTip()

        tip.SetToolTip(
            txtUsername,
            "4-30 letters, numbers, dot or underscore"
        )

        tip.SetToolTip(
            txtPassword,
            "New user: min 8 characters with letters and numbers." &
            vbCrLf &
            "Existing user: leave blank to keep the current password."
        )

        tip.SetToolTip(
            txtSearch,
            "Search by username, full name, role, or status."
        )


        ' ------------------------------------------------------
        ' ROLE COMBOBOX
        ' ------------------------------------------------------

        If cboRole.Items.Count = 0 Then

            cboRole.Items.AddRange(
                New Object() {
                    "Administrator",
                    "Registrar Staff"
                }
            )

        End If

        cboRole.DropDownStyle = ComboBoxStyle.DropDownList


        ' ------------------------------------------------------
        ' STATUS COMBOBOX
        ' ------------------------------------------------------

        If cboStatus.Items.Count = 0 Then

            cboStatus.Items.AddRange(
                New Object() {
                    "Active",
                    "Inactive"
                }
            )

        End If

        cboStatus.DropDownStyle = ComboBoxStyle.DropDownList


        ' ------------------------------------------------------
        ' ROLE ACCESS
        ' ------------------------------------------------------

        ApplyRoleAccess(
            Me,
            btnUsers,
            PictureBoxUsers
        )


        ' ------------------------------------------------------
        ' LOAD USERS
        ' ------------------------------------------------------

        LoadUsers()


        ' ------------------------------------------------------
        ' CLEAR FORM
        ' ------------------------------------------------------

        ClearFields()

    End Sub


    ' ==========================================================
    ' LOAD USERS
    ' ==========================================================

    Public Sub LoadUsers()

        Dim searchText As String = txtSearch.Text.Trim()

        Try

            connection()


            ' --------------------------------------------------
            ' SQL
            ' --------------------------------------------------

            sql =
                "SELECT " &
                "UserID, " &
                "Username, " &
                "FullName, " &
                "Role, " &
                "Status " &
                "FROM tblusers "


            ' --------------------------------------------------
            ' LIVE SEARCH
            ' --------------------------------------------------

            If searchText <> "" Then

                sql &=
                    "WHERE Username LIKE @Search " &
                    "OR FullName LIKE @Search " &
                    "OR Role LIKE @Search " &
                    "OR Status LIKE @Search "

            End If


            sql &=
                "ORDER BY UserID ASC"


            ' --------------------------------------------------
            ' DATA ADAPTER
            ' --------------------------------------------------

            da = New MySqlDataAdapter(sql, cn)


            If searchText <> "" Then

                da.SelectCommand.Parameters.AddWithValue(
                    "@Search",
                    "%" & searchText & "%"
                )

            End If


            dt = New DataTable()

            da.Fill(dt)

            dgvUsers.DataSource = dt


            ' --------------------------------------------------
            ' HIDE USER ID
            ' --------------------------------------------------

            If dgvUsers.Columns.Contains("UserID") Then

                dgvUsers.Columns("UserID").Visible = False

            End If


            ' --------------------------------------------------
            ' COLUMN HEADERS
            ' --------------------------------------------------

            If dgvUsers.Columns.Contains("Username") Then

                dgvUsers.Columns("Username").HeaderText =
                    "Username"

            End If


            If dgvUsers.Columns.Contains("FullName") Then

                dgvUsers.Columns("FullName").HeaderText =
                    "Full Name"

            End If


            If dgvUsers.Columns.Contains("Role") Then

                dgvUsers.Columns("Role").HeaderText =
                    "Role"

            End If


            If dgvUsers.Columns.Contains("Status") Then

                dgvUsers.Columns("Status").HeaderText =
                    "Status"

            End If


            ' --------------------------------------------------
            ' DATAGRIDVIEW SETTINGS
            ' --------------------------------------------------

            dgvUsers.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill

            dgvUsers.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect

            dgvUsers.MultiSelect = False

            dgvUsers.ReadOnly = True

            dgvUsers.AllowUserToAddRows = False

            dgvUsers.AllowUserToDeleteRows = False

            dgvUsers.RowHeadersVisible = False

            dgvUsers.AutoGenerateColumns = True


        Catch ex As Exception

            MessageBox.Show(
                "Error loading users: " & ex.Message,
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


    ' ==========================================================
    ' LIVE SEARCH
    ' ==========================================================

    Private Sub txtSearch_TextChanged(
        sender As Object,
        e As EventArgs
    ) Handles txtSearch.TextChanged

        LoadUsers()

    End Sub


    ' ==========================================================
    ' SELECT USER FROM DATAGRIDVIEW
    ' ==========================================================

    Private Sub dgvUsers_CellClick(
        sender As Object,
        e As DataGridViewCellEventArgs
    ) Handles dgvUsers.CellClick

        If e.RowIndex < 0 Then

            Exit Sub

        End If


        Try

            Dim row As DataGridViewRow =
                dgvUsers.Rows(e.RowIndex)


            ' --------------------------------------------------
            ' GET USER ID
            ' --------------------------------------------------

            SelectedUserID =
                Convert.ToInt32(
                    row.Cells("UserID").Value
                )


            ' --------------------------------------------------
            ' DISPLAY USER INFORMATION
            ' --------------------------------------------------

            txtUsername.Text =
                row.Cells("Username").Value.ToString()


            txtFullName.Text =
                row.Cells("FullName").Value.ToString()


            cboRole.Text =
                row.Cells("Role").Value.ToString()


            cboStatus.Text =
                row.Cells("Status").Value.ToString()


            ' --------------------------------------------------
            ' PASSWORD
            ' --------------------------------------------------

            ' Password is hashed in the database.
            ' We do not display the stored password.
            txtPassword.Clear()


            ' --------------------------------------------------
            ' USERNAME CANNOT BE CHANGED
            ' --------------------------------------------------

            If txtUsername.Text.Trim().ToLower() = "admin" Then
                txtUsername.ReadOnly = True
            Else
                txtUsername.ReadOnly = False
            End If


            ' --------------------------------------------------
            ' CLEAR ERRORS
            ' --------------------------------------------------

            If ep IsNot Nothing Then

                ClearErrors(
                    ep,
                    txtUsername,
                    txtPassword,
                    txtFullName,
                    cboRole
                )

            End If


        Catch ex As Exception

            MessageBox.Show(
                "Unable to select user: " & ex.Message,
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )

        End Try

    End Sub


    ' ==========================================================
    ' SAVE USER
    ' ==========================================================

    Private Sub btnSave_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnSave.Click

        SaveUser()

    End Sub


    ' ==========================================================
    ' ADD / UPDATE USER
    ' ==========================================================

    Private Sub SaveUser()

        Dim isEdit As Boolean =
            (SelectedUserID <> 0)

        Dim ok As Boolean = True


        ' ======================================================
        ' VALIDATION
        ' ======================================================

        ' ------------------------------------------------------
        ' USERNAME
        ' ------------------------------------------------------

        If Not isEdit Then

            ok =
                CheckPattern(
                    ep,
                    txtUsername,
                    "Username",
                    PatUsername,
                    "Username must be 4-30 letters, numbers, dots or underscores (no spaces)."
                ) And ok

        End If


        ' ------------------------------------------------------
        ' PASSWORD
        ' ------------------------------------------------------

        If Not isEdit OrElse txtPassword.Text <> "" Then

            If txtPassword.Text = "" Then

                ok =
                    Fail(
                        ep,
                        txtPassword,
                        "Password is required."
                    ) And ok

            Else

                Dim pwProblem As String =
                    PasswordProblem(
                        txtPassword.Text,
                        txtUsername.Text.Trim()
                    )


                If pwProblem <> "" Then

                    ok =
                        Fail(
                            ep,
                            txtPassword,
                            pwProblem
                        ) And ok

                Else

                    ep.SetError(
                        txtPassword,
                        ""
                    )

                End If

            End If

        End If


        ' ------------------------------------------------------
        ' FULL NAME
        ' ------------------------------------------------------

        ok =
            CheckPattern(
                ep,
                txtFullName,
                "Full name",
                PatName,
                "Full name may only contain letters, spaces, - ' and ."
            ) And ok


        ' ------------------------------------------------------
        ' ROLE
        ' ------------------------------------------------------

        If cboRole.Text.Trim() = "" Then

            ok =
                Fail(
                    ep,
                    cboRole,
                    "Please choose a role."
                ) And ok

        Else

            ep.SetError(
                cboRole,
                ""
            )

        End If


        ' ------------------------------------------------------
        ' STATUS
        ' ------------------------------------------------------

        If cboStatus.Text.Trim() = "" Then

            cboStatus.Text = "Active"

        End If


        ' ------------------------------------------------------
        ' STOP IF VALIDATION FAILED
        ' ------------------------------------------------------

        If Not ok Then

            FocusFirstError(
                ep,
                txtUsername,
                txtPassword,
                txtFullName,
                cboRole
            )


            ShowWarn(
                "Some fields need your attention. They are marked with a red icon.",
                "Check the form"
            )


            Exit Sub

        End If


        ' ======================================================
        ' PROTECT ADMINISTRATOR ACCOUNT
        ' ======================================================

        If isEdit AndAlso
           txtUsername.Text.Trim().ToLower() = "admin" Then


            If cboRole.Text <> "Administrator" OrElse
               cboStatus.Text = "Inactive" Then

                ShowWarn(
                    "The admin account must remain an active Administrator.",
                    "Protected account"
                )

                Exit Sub

            End If

        End If


        ' ======================================================
        ' CONFIRM UPDATE
        ' ======================================================

        If isEdit Then

            If Not Confirm(
                "Save changes to user '" &
                txtUsername.Text.Trim() &
                "'?",
                "Edit User"
            ) Then

                Exit Sub

            End If

        End If


        Try

            connection()


            ' ==================================================
            ' ADD NEW USER
            ' ==================================================

            If Not isEdit Then


                ' ------------------------------------------------
                ' CHECK DUPLICATE USERNAME
                ' ------------------------------------------------

                sql =
                    "SELECT COUNT(*) " &
                    "FROM tblusers " &
                    "WHERE LOWER(Username) = LOWER(@Username)"


                cmd =
                    New MySqlCommand(
                        sql,
                        cn
                    )


                cmd.Parameters.AddWithValue(
                    "@Username",
                    txtUsername.Text.Trim()
                )


                If Convert.ToInt32(
                    cmd.ExecuteScalar()
                ) > 0 Then


                    Fail(
                        ep,
                        txtUsername,
                        "This username is already taken."
                    )


                    txtUsername.Focus()

                    Exit Sub

                End If


                ' ------------------------------------------------
                ' INSERT USER
                ' ------------------------------------------------

                sql =
                    "INSERT INTO tblusers " &
                    "(Username, Password, FullName, Role, Status) " &
                    "VALUES " &
                    "(@Username, @Password, @FullName, @Role, @Status)"


                cmd =
                    New MySqlCommand(
                        sql,
                        cn
                    )


                cmd.Parameters.AddWithValue(
                    "@Username",
                    txtUsername.Text.Trim()
                )


                cmd.Parameters.AddWithValue(
                    "@Password",
                    HashPassword(
                        txtPassword.Text
                    )
                )


                ' ==================================================
                ' UPDATE USER WITH NEW PASSWORD
                ' ==================================================

            ElseIf txtPassword.Text <> "" Then


                sql =
                    "UPDATE tblusers SET " &
                    "Password = @Password, " &
                    "FullName = @FullName, " &
                    "Role = @Role, " &
                    "Status = @Status " &
                    "WHERE UserID = @UserID"


                cmd =
                    New MySqlCommand(
                        sql,
                        cn
                    )


                cmd.Parameters.AddWithValue(
                    "@Password",
                    HashPassword(
                        txtPassword.Text
                    )
                )


                cmd.Parameters.AddWithValue(
                    "@UserID",
                    SelectedUserID
                )


                ' ==================================================
                ' UPDATE USER WITHOUT CHANGING PASSWORD
                ' ==================================================

            Else


                sql =
                    "UPDATE tblusers SET " &
                    "FullName = @FullName, " &
                    "Role = @Role, " &
                    "Status = @Status " &
                    "WHERE UserID = @UserID"


                cmd =
                    New MySqlCommand(
                        sql,
                        cn
                    )


                cmd.Parameters.AddWithValue(
                    "@UserID",
                    SelectedUserID
                )


            End If


            ' ==================================================
            ' COMMON PARAMETERS
            ' ==================================================

            cmd.Parameters.AddWithValue(
                "@FullName",
                txtFullName.Text.Trim()
            )


            cmd.Parameters.AddWithValue(
                "@Role",
                cboRole.Text.Trim()
            )


            cmd.Parameters.AddWithValue(
                "@Status",
                cboStatus.Text.Trim()
            )


            ' ==================================================
            ' EXECUTE
            ' ==================================================

            cmd.ExecuteNonQuery()


            ' ==================================================
            ' SUCCESS MESSAGE
            ' ==================================================

            If isEdit Then

                ShowInfo(
                    "User updated successfully."
                )

            Else

                ShowInfo(
                    "User added successfully."
                )

            End If


            ' ==================================================
            ' REFRESH
            ' ==================================================

            LoadUsers()

            ClearFields()


        Catch ex As Exception

            ShowError(
                FriendlyDbError(
                    ex,
                    "save the user"
                )
            )


        Finally

            If cn.State = ConnectionState.Open Then

                cn.Close()

            End If

        End Try

    End Sub


    ' ==========================================================
    ' ADD USERS BUTTON
    ' ==========================================================

    Private Sub btnAddUsers_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnAddUsers.Click

        ClearFields()

        txtUsername.Focus()

    End Sub


    ' ==========================================================
    ' DELETE USER
    ' ==========================================================

    Private Sub btnDelete_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnDelete.Click


        If SelectedUserID = 0 Then

            MessageBox.Show(
                "Please select a user first.",
                "Delete User",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            )

            Exit Sub

        End If


        ' ======================================================
        ' ADMIN CANNOT BE DELETED
        ' ======================================================

        If txtUsername.Text.Trim().ToLower() = "admin" Then

            MessageBox.Show(
                "The Administrator account cannot be deleted.",
                "Delete User",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            )

            Exit Sub

        End If


        ' ======================================================
        ' CONFIRM DELETE
        ' ======================================================

        Dim result As DialogResult =
            MessageBox.Show(
                "Permanently delete this user?" &
                vbCrLf &
                vbCrLf &
                "This cannot be undone." &
                vbCrLf &
                "Use Deactivate to keep the record.",
                "Delete User",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            )


        If result <> DialogResult.Yes Then

            Exit Sub

        End If


        Try

            connection()


            ' --------------------------------------------------
            ' DELETE USER
            ' --------------------------------------------------

            sql =
                "DELETE FROM tblusers " &
                "WHERE UserID = @UserID " &
                "AND Username <> 'admin'"


            cmd =
                New MySqlCommand(
                    sql,
                    cn
                )


            cmd.Parameters.AddWithValue(
                "@UserID",
                SelectedUserID
            )


            Dim affectedRows As Integer =
                cmd.ExecuteNonQuery()


            ' --------------------------------------------------
            ' CHECK RESULT
            ' --------------------------------------------------

            If affectedRows > 0 Then

                MessageBox.Show(
                    "User deleted successfully.",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                )

            Else

                MessageBox.Show(
                    "The Administrator account cannot be deleted.",
                    "Delete User",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                )

            End If


            ' --------------------------------------------------
            ' REFRESH
            ' --------------------------------------------------

            LoadUsers()

            ClearFields()


        Catch ex As Exception

            MessageBox.Show(
                "Error deleting user: " & ex.Message,
                "Delete Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )


        Finally

            If cn.State = ConnectionState.Open Then

                cn.Close()

            End If

        End Try

    End Sub


    ' ==========================================================
    ' DEACTIVATE USER
    ' ==========================================================

    Private Sub btnDeactivate_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnDeactivate.Click


        If SelectedUserID = 0 Then

            MessageBox.Show(
                "Please select a user first.",
                "Deactivate User",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            )

            Exit Sub

        End If


        ' ======================================================
        ' ADMIN CANNOT BE DEACTIVATED
        ' ======================================================

        If txtUsername.Text.Trim().ToLower() = "admin" Then

            MessageBox.Show(
                "The Administrator account cannot be deactivated.",
                "Deactivate User",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            )

            Exit Sub

        End If


        ' ======================================================
        ' CONFIRM
        ' ======================================================

        Dim result As DialogResult =
            MessageBox.Show(
                "Are you sure you want to deactivate this user?",
                "Deactivate User",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            )


        If result <> DialogResult.Yes Then

            Exit Sub

        End If


        Try

            connection()


            ' --------------------------------------------------
            ' DEACTIVATE
            ' --------------------------------------------------

            sql =
                "UPDATE tblusers " &
                "SET Status = 'Inactive' " &
                "WHERE UserID = @UserID " &
                "AND Username <> 'admin'"


            cmd =
                New MySqlCommand(
                    sql,
                    cn
                )


            cmd.Parameters.AddWithValue(
                "@UserID",
                SelectedUserID
            )


            cmd.ExecuteNonQuery()


            ' --------------------------------------------------
            ' SUCCESS
            ' --------------------------------------------------

            MessageBox.Show(
                "User deactivated successfully.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            )


            ' --------------------------------------------------
            ' REFRESH
            ' --------------------------------------------------

            LoadUsers()

            ClearFields()


        Catch ex As Exception

            MessageBox.Show(
                "Error deactivating user: " & ex.Message,
                "Deactivate Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )


        Finally

            If cn.State = ConnectionState.Open Then

                cn.Close()

            End If

        End Try

    End Sub


    ' ==========================================================
    ' CLEAR FIELDS
    ' ==========================================================

    Private Sub ClearFields()

        SelectedUserID = 0


        ' ------------------------------------------------------
        ' USERNAME
        ' ------------------------------------------------------

        txtUsername.ReadOnly = False

        txtUsername.Clear()


        ' ------------------------------------------------------
        ' PASSWORD
        ' ------------------------------------------------------

        txtPassword.Clear()


        ' ------------------------------------------------------
        ' FULL NAME
        ' ------------------------------------------------------

        txtFullName.Clear()


        ' ------------------------------------------------------
        ' ROLE
        ' ------------------------------------------------------

        cboRole.SelectedIndex = -1


        ' ------------------------------------------------------
        ' STATUS
        ' ------------------------------------------------------

        cboStatus.Text = "Active"


        ' ------------------------------------------------------
        ' ERRORS
        ' ------------------------------------------------------

        If ep IsNot Nothing Then

            ClearErrors(
                ep,
                txtUsername,
                txtPassword,
                txtFullName,
                cboRole
            )

        End If


        ' ------------------------------------------------------
        ' DATAGRIDVIEW
        ' ------------------------------------------------------

        If dgvUsers IsNot Nothing Then

            dgvUsers.ClearSelection()

        End If

    End Sub


    ' ==========================================================
    ' DASHBOARD
    ' ==========================================================

    Private Sub btnDashboard_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnDashboard.Click

        frmMainMenu.Show()

        Me.Close()

    End Sub


    ' ==========================================================
    ' STUDENTS
    ' ==========================================================

    Private Sub btnStudents_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnStudents.Click

        frmStudents.Show()

        Me.Close()

    End Sub


    ' ==========================================================
    ' DOCUMENTS
    ' ==========================================================

    Private Sub btnDocuments_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnDocuments.Click

        frmDocuments.Show()

        Me.Close()

    End Sub


    ' ==========================================================
    ' REQUESTS
    ' ==========================================================

    Private Sub btnRequests_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnRequests.Click

        frmRequestList.Show()

        Me.Close()

    End Sub


    ' ==========================================================
    ' REPORTS
    ' ==========================================================

    Private Sub btnReports_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnReports.Click

        frmReports.Show()

        Me.Close()

    End Sub


    ' ==========================================================
    ' USERS
    ' ==========================================================

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


        ' Already on Users form

    End Sub


    ' ==========================================================
    ' LOGOUT
    ' ==========================================================

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

            Me.Close()

        End If

    End Sub

    Private Sub btnEdit_Click(sender As Object, e As EventArgs)

    End Sub
End Class