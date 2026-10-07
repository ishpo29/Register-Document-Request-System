Imports MySql.Data.MySqlClient

Public Class frmLogin

    Private ep As ErrorProvider
    Private chkShow As CheckBox
    Private lblStatus As Label

    ' Simple brute-force protection
    Private failedAttempts As Integer = 0
    Private lockedUntil As DateTime = DateTime.MinValue
    Private Const MaxAttempts As Integer = 5
    Private Const LockSeconds As Integer = 30


    ' ==========================================================
    ' FORM LOAD
    ' ==========================================================

    Private Sub frmLogin_Load(
        sender As Object,
        e As EventArgs
    ) Handles MyBase.Load

        ThemeForm(Me)
        btnExit.BackColor = Color.Red
        btnExit.ForeColor = Color.White
        btnExit.FlatStyle = FlatStyle.Flat
        btnExit.FlatAppearance.BorderSize = 0
        ep = NewErrorProvider(Me)


        ' ------------------------------------------------------
        ' CLEAR LOGIN FIELDS
        ' ------------------------------------------------------

        txtUsername.Clear()
        txtPassword.Clear()


        ' ------------------------------------------------------
        ' PRESSING ENTER SIGNS IN
        ' ------------------------------------------------------

        Me.AcceptButton = btnLogin


        ' ------------------------------------------------------
        ' TEXTBOX SETTINGS
        ' ------------------------------------------------------

        txtUsername.MaxLength = 30

        txtPassword.MaxLength = 64

        txtPassword.UseSystemPasswordChar = True

        ClearErrorOnEdit(
            ep,
            txtUsername,
            txtPassword
        )


        ' ======================================================
        ' SHOW PASSWORD CHECKBOX
        ' ======================================================

        chkShow = New CheckBox() With {
            .Text = "Show password",
            .AutoSize = True,
            .Cursor = Cursors.Hand,
            .Font = New Font("Segoe UI", 9.0F),
            .BackColor = txtPassword.Parent.BackColor,
            .ForeColor = Color.FromArgb(71, 85, 105)
        }


        chkShow.Location = New Point(
            txtPassword.Left,
            txtPassword.Bottom + 6
        )


        AddHandler chkShow.CheckedChanged,
            Sub(s As Object, ev As EventArgs)

                txtPassword.UseSystemPasswordChar =
                    Not chkShow.Checked

            End Sub


        txtPassword.Parent.Controls.Add(chkShow)

        chkShow.BringToFront()


        ' ======================================================
        ' STATUS LABEL
        ' ======================================================

        lblStatus = New Label() With {
            .AutoSize = False,
            .Size = New Size(txtPassword.Width, 20),
            .Font = New Font("Segoe UI", 9.0F),
            .ForeColor = ClrDanger,
            .BackColor = txtPassword.Parent.BackColor,
            .Visible = False
        }


        ' Put the status directly below the Show Password checkbox
        lblStatus.Location = New Point(
            txtPassword.Left,
            chkShow.Bottom + 3
        )


        txtPassword.Parent.Controls.Add(lblStatus)

        lblStatus.BringToFront()


        ' ------------------------------------------------------
        ' FOCUS USERNAME
        ' ------------------------------------------------------

        txtUsername.Focus()

    End Sub


    ' ==========================================================
    ' FORM ACTIVATED
    ' ==========================================================

    Private Sub frmLogin_Activated(
        sender As Object,
        e As EventArgs
    ) Handles MyBase.Activated

        ' Clear the login fields every time the login form
        ' becomes active.

        txtUsername.Clear()

        txtPassword.Clear()


        ' Reset Show Password checkbox

        If chkShow IsNot Nothing Then

            chkShow.Checked = False

        End If


        ' Clear status message

        If lblStatus IsNot Nothing Then

            SetStatus("")

        End If


        ' Focus username

        txtUsername.Focus()

    End Sub


    ' ==========================================================
    ' SET STATUS MESSAGE
    ' ==========================================================

    Private Sub SetStatus(
        message As String
    )

        If lblStatus Is Nothing Then

            Exit Sub

        End If


        lblStatus.Text = message

        lblStatus.Visible =
            (message <> "")

    End Sub


    ' ==========================================================
    ' LOGIN BUTTON
    ' ==========================================================

    Private Sub btnLogin_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnLogin.Click

        SetStatus("")


        ' ======================================================
        ' LOCKOUT CHECK
        ' ======================================================

        If DateTime.Now < lockedUntil Then

            Dim secs As Integer =
                CInt(
                    Math.Ceiling(
                        (lockedUntil - DateTime.Now).TotalSeconds
                    )
                )


            SetStatus(
                "Too many failed attempts. Please wait " &
                secs &
                " seconds before trying again."
            )


            Exit Sub

        End If


        ' ======================================================
        ' FIELD VALIDATION
        ' ======================================================

        Dim ok As Boolean = True


        ok =
            CheckRequired(
                ep,
                txtUsername,
                "Username"
            ) And ok


        ok =
            CheckRequired(
                ep,
                txtPassword,
                "Password"
            ) And ok


        If Not ok Then

            FocusFirstError(
                ep,
                txtUsername,
                txtPassword
            )


            Exit Sub

        End If


        ' ======================================================
        ' DISABLE LOGIN BUTTON
        ' ======================================================

        btnLogin.Enabled = False

        btnLogin.Text = "Signing in..."

        Me.Cursor = Cursors.WaitCursor


        Try

            connection()


            ' ==================================================
            ' GET USER BY USERNAME
            ' ==================================================

            sql =
                "SELECT UserID, Password, FullName, Role, Status " &
                "FROM tblusers " &
                "WHERE Username = @Username " &
                "LIMIT 1"


            cmd =
                New MySqlCommand(
                    sql,
                    cn
                )


            cmd.Parameters.AddWithValue(
                "@Username",
                txtUsername.Text.Trim()
            )


            ' ==================================================
            ' USER VARIABLES
            ' ==================================================

            Dim userId As Integer = 0

            Dim storedPw As String = ""

            Dim fullName As String = ""

            Dim role As String = ""

            Dim status As String = ""

            Dim found As Boolean = False


            ' ==================================================
            ' READ USER
            ' ==================================================

            dr = cmd.ExecuteReader()


            If dr.Read() Then

                found = True


                userId =
                    Convert.ToInt32(
                        dr("UserID")
                    )


                storedPw =
                    dr("Password").ToString()


                fullName =
                    dr("FullName").ToString()


                role =
                    dr("Role").ToString()


                status =
                    dr("Status").ToString()

            End If


            dr.Close()


            ' ==================================================
            ' VERIFY PASSWORD
            ' ==================================================

            Dim passwordOk As Boolean =
                found AndAlso
                VerifyPassword(
                    txtPassword.Text,
                    storedPw
                )


            If Not passwordOk Then

                ' Administrator account is not affected
                ' by the failed-login attempt limit.
                If found AndAlso
       role.Trim().Equals("Administrator",
                          StringComparison.OrdinalIgnoreCase) Then

                    SetStatus("Incorrect username or password.")

                    txtPassword.Clear()
                    txtPassword.Focus()

                    Exit Sub

                End If


                ' Registrar Staff and unknown users
                ' still use the failed-attempt protection.
                RegisterFailure()

                Exit Sub

            End If


            ' ==================================================
            ' CHECK ACCOUNT STATUS
            ' ==================================================

            If status <> "Active" Then

                SetStatus(
                    "This account is inactive. Please contact the administrator."
                )


                txtPassword.Clear()


                Exit Sub

            End If


            ' ==================================================
            ' UPGRADE OLD PLAIN-TEXT PASSWORD
            ' ==================================================

            If Not IsHashed(storedPw) Then

                cmd =
                    New MySqlCommand(
                        "UPDATE tblusers " &
                        "SET Password = @P " &
                        "WHERE UserID = @ID",
                        cn
                    )


                cmd.Parameters.AddWithValue(
                    "@P",
                    HashPassword(
                        txtPassword.Text
                    )
                )


                cmd.Parameters.AddWithValue(
                    "@ID",
                    userId
                )


                cmd.ExecuteNonQuery()

            End If


            ' ==================================================
            ' SUCCESSFUL LOGIN
            ' ==================================================

            failedAttempts = 0

            CurrentRole = role


            txtPassword.Clear()


            If chkShow IsNot Nothing Then

                chkShow.Checked = False

            End If


            SetStatus("")


            ' ==================================================
            ' OPEN MAIN MENU
            ' ==================================================

            Me.Hide()

            frmMainMenu.Show()


        Catch ex As Exception


            ' --------------------------------------------------
            ' CLOSE DATA READER IF STILL OPEN
            ' --------------------------------------------------

            If dr IsNot Nothing AndAlso
               Not dr.IsClosed Then

                dr.Close()

            End If


            SetStatus(
                FriendlyDbError(
                    ex,
                    "sign in"
                )
            )


        Finally


            ' --------------------------------------------------
            ' CLOSE DATABASE CONNECTION
            ' --------------------------------------------------

            If cn.State = ConnectionState.Open Then

                cn.Close()

            End If


            ' --------------------------------------------------
            ' RESTORE LOGIN BUTTON
            ' --------------------------------------------------

            btnLogin.Enabled = True

            btnLogin.Text = "Log in"

            Me.Cursor = Cursors.Default

        End Try

    End Sub


    ' ==========================================================
    ' REGISTER FAILED LOGIN
    ' ==========================================================

    Private Sub RegisterFailure()

        failedAttempts += 1


        ' Clear password after failed login

        txtPassword.Clear()


        ' ======================================================
        ' MAXIMUM ATTEMPTS
        ' ======================================================

        If failedAttempts >= MaxAttempts Then

            lockedUntil =
                DateTime.Now.AddSeconds(
                    LockSeconds
                )


            failedAttempts = 0


            SetStatus(
                "Too many failed attempts. Sign-in is locked for " &
                LockSeconds &
                " seconds."
            )


        Else


            ' Same message for unknown user / wrong password
            ' so the system does not reveal which one is wrong.

            SetStatus(
                "Incorrect username or password. " &
                (MaxAttempts - failedAttempts) &
                " attempt(s) left."
            )

        End If


        txtPassword.Focus()

    End Sub

    Private Sub btnExit_Click(sender As Object, e As EventArgs) Handles btnExit.Click
        Dim result As DialogResult =
        MessageBox.Show(
            "Are you sure you want to exit?",
            "Exit",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        )

        If result = DialogResult.Yes Then
            Application.Exit()
        End If
    End Sub
End Class