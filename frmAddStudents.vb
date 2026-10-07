Imports MySql.Data.MySqlClient

Public Class frmAddStudents

    ' Used when editing a student
    Public StudentIDToEdit As String = ""

    Private ep As ErrorProvider
    Private isDirty As Boolean = False


    '========================================
    ' FORM LOAD
    '========================================
    Private Sub frmAddStudents_Load(
        sender As Object,
        e As EventArgs
    ) Handles MyBase.Load

        ThemeForm(Me)

        ep = NewErrorProvider(Me)

        SetupInputRules()


        If StudentIDToEdit <> "" Then

            '========================================
            ' EDIT MODE
            '========================================

            btnAddStudent.Text = "Update Student"

            ' Student ID cannot be changed
            txtStudentID.ReadOnly = True

            LoadStudentForEdit()

        Else

            '========================================
            ' ADD MODE
            '========================================

            btnAddStudent.Text = "Add Student"

            ' Student ID is automatically generated
            txtStudentID.ReadOnly = True

            GenerateStudentID()

            txtStatus.Text = "Active"

        End If


        ' Status cannot be manually changed
        txtStatus.ReadOnly = True


        ' Generation/loading should not count
        ' as an unsaved change
        isDirty = False


        txtStudentID.Focus()

    End Sub


    '========================================
    ' INPUT RULES + COMBOBOX SETUP
    '========================================
    Private Sub SetupInputRules()

        '========================================
        ' STUDENT ID
        '========================================

        txtStudentID.MaxLength = 20


        '========================================
        ' LRN
        '========================================

        RestrictToDigits(
            txtLRN,
            12
        )


        '========================================
        ' NAMES
        '========================================

        RestrictToName(
            txtLastName,
            50
        )


        RestrictToName(
            txtFirstName,
            50
        )


        RestrictToName(
            txtMiddleName,
            50
        )


        '========================================
        ' CONTACT NUMBER
        '========================================

        RestrictToPhone(
            txtContactNo
        )


        '========================================
        ' COURSE COMBOBOX
        '========================================

        cboCourse.Items.Clear()

        cboCourse.Items.Add("BSIT")
        cboCourse.Items.Add("BSBA")
        cboCourse.Items.Add("BSHM")
        cboCourse.Items.Add("BSED")
        cboCourse.Items.Add("BEED")
        cboCourse.Items.Add("BS Psychology")
        cboCourse.Items.Add("BS Accountancy")

        cboCourse.DropDownStyle =
            ComboBoxStyle.DropDownList


        '========================================
        ' YEAR LEVEL COMBOBOX
        '========================================

        cboYearLevel.Items.Clear()

        cboYearLevel.Items.Add("1st Year")
        cboYearLevel.Items.Add("2nd Year")
        cboYearLevel.Items.Add("3rd Year")
        cboYearLevel.Items.Add("4th Year")

        cboYearLevel.DropDownStyle =
            ComboBoxStyle.DropDownList


        '========================================
        ' SECTION COMBOBOX
        '========================================

        cboSection.Items.Clear()

        cboSection.Items.Add("A")
        cboSection.Items.Add("B")
        cboSection.Items.Add("C")
        cboSection.Items.Add("D")
        cboSection.Items.Add("E")
        cboSection.Items.Add("F")
        cboSection.Items.Add("G")
        cboSection.Items.Add("H")

        cboSection.DropDownStyle =
            ComboBoxStyle.DropDownList


        '========================================
        ' TOOLTIPS
        '========================================

        Dim tip As New ToolTip()


        tip.SetToolTip(
            txtStudentID,
            "Automatically generated Student ID"
        )


        tip.SetToolTip(
            txtLRN,
            "Learner Reference Number: exactly 12 digits"
        )


        tip.SetToolTip(
            txtMiddleName,
            "Optional"
        )


        tip.SetToolTip(
            txtContactNo,
            "Mobile number, e.g. 09171234567 or +639171234567"
        )


        tip.SetToolTip(
            cboCourse,
            "Select the student's course"
        )


        tip.SetToolTip(
            cboYearLevel,
            "Select the student's year level"
        )


        tip.SetToolTip(
            cboSection,
            "Select the student's section"
        )


        '========================================
        ' CLEAR ERRORS WHEN USER EDITS
        '========================================

        ClearErrorOnEdit(
            ep,
            txtLRN,
            txtLastName,
            txtFirstName,
            txtMiddleName,
            txtContactNo
        )


        '========================================
        ' TRACK UNSAVED TEXTBOX CHANGES
        '========================================

        For Each c As TextBox In {
            txtLRN,
            txtLastName,
            txtFirstName,
            txtMiddleName,
            txtContactNo
        }

            AddHandler c.TextChanged,
                Sub(s As Object, ev As EventArgs)

                    isDirty = True

                End Sub

        Next


        '========================================
        ' TRACK COMBOBOX CHANGES
        '========================================

        AddHandler cboCourse.SelectedIndexChanged,
            Sub(s As Object, ev As EventArgs)

                isDirty = True

            End Sub


        AddHandler cboYearLevel.SelectedIndexChanged,
            Sub(s As Object, ev As EventArgs)

                isDirty = True

            End Sub


        AddHandler cboSection.SelectedIndexChanged,
            Sub(s As Object, ev As EventArgs)

                isDirty = True

            End Sub


        EnterAsTab(Me)

    End Sub


    '========================================
    ' GENERATE STUDENT ID
    '========================================
    Private Sub GenerateStudentID()

        Try

            connection()


            ' Get the current year
            Dim currentYear As String =
                DateTime.Now.Year.ToString()


            ' Student IDs for this year start with:
            ' 2026
            '
            ' Example:
            ' 20260001
            ' 20260002
            ' 20260003

            Dim prefix As String =
                currentYear


            sql =
                "SELECT MAX(CAST(StudentID AS UNSIGNED)) " &
                "FROM tblstudents " &
                "WHERE StudentID REGEXP '^[0-9]{8}$' " &
                "AND LEFT(StudentID, 4) = @Prefix"


            cmd = New MySqlCommand(
                sql,
                cn
            )


            cmd.Parameters.AddWithValue(
                "@Prefix",
                prefix
            )


            Dim result As Object =
                cmd.ExecuteScalar()


            Dim nextNumber As Integer = 1


            If result IsNot Nothing AndAlso
               Not IsDBNull(result) Then

                Dim highestID As Long =
                    Convert.ToInt64(result)


                ' Get the last 4 digits
                Dim lastNumber As Integer =
                    CInt(highestID Mod 10000)


                nextNumber =
                    lastNumber + 1

            End If


            ' Make sure the sequence does not exceed 9999
            If nextNumber > 9999 Then

                MessageBox.Show(
                    "The Student ID sequence for " &
                    currentYear &
                    " is already full.",
                    "Student ID Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                )

                txtStudentID.Clear()

                Exit Sub

            End If


            ' Generate the Student ID
            '
            ' Example:
            ' 2026 + 0001
            ' = 20260001

            txtStudentID.Text =
                prefix &
                nextNumber.ToString("0000")


        Catch ex As Exception

            ShowError(
                FriendlyDbError(
                    ex,
                    "generate the Student ID"
                )
            )


        Finally

            If cn.State = ConnectionState.Open Then

                cn.Close()

            End If

        End Try

    End Sub


    '========================================
    ' VALIDATE ALL FIELDS
    '========================================
    Private Function ValidateForm() As Boolean

        Dim ok As Boolean = True


        '========================================
        ' STUDENT ID
        '========================================

        If txtStudentID.Text.Trim() = "" Then

            ep.SetError(
                txtStudentID,
                "Student ID could not be generated."
            )

            ok = False

        Else

            ep.SetError(
                txtStudentID,
                ""
            )

        End If


        '========================================
        ' LRN
        '========================================

        ok =
            CheckPattern(
                ep,
                txtLRN,
                "LRN",
                PatLRN,
                "LRN must be exactly 12 digits."
            ) And ok


        '========================================
        ' LAST NAME
        '========================================

        ok =
            CheckPattern(
                ep,
                txtLastName,
                "Last name",
                PatName,
                "Last name may only contain letters, spaces, - ' and ."
            ) And ok


        '========================================
        ' FIRST NAME
        '========================================

        ok =
            CheckPattern(
                ep,
                txtFirstName,
                "First name",
                PatName,
                "First name may only contain letters, spaces, - ' and ."
            ) And ok


        '========================================
        ' MIDDLE NAME
        '========================================

        If txtMiddleName.Text.Trim() <> "" Then

            ok =
                CheckPattern(
                    ep,
                    txtMiddleName,
                    "Middle name",
                    PatName,
                    "Middle name may only contain letters, spaces, - ' and ."
                ) And ok

        Else

            ep.SetError(
                txtMiddleName,
                ""
            )

        End If


        '========================================
        ' COURSE
        '========================================

        If cboCourse.Text.Trim() = "" Then

            ep.SetError(
                cboCourse,
                "Please select a course."
            )

            ok = False

        Else

            ep.SetError(
                cboCourse,
                ""
            )

        End If


        '========================================
        ' YEAR LEVEL
        '========================================

        If cboYearLevel.Text.Trim() = "" Then

            ep.SetError(
                cboYearLevel,
                "Please select a year level."
            )

            ok = False

        Else

            ep.SetError(
                cboYearLevel,
                ""
            )

        End If


        '========================================
        ' SECTION
        '========================================

        If cboSection.Text.Trim() = "" Then

            ep.SetError(
                cboSection,
                "Please select a section."
            )

            ok = False

        Else

            ep.SetError(
                cboSection,
                ""
            )

        End If


        '========================================
        ' CONTACT NUMBER
        '========================================

        ok =
            CheckPattern(
                ep,
                txtContactNo,
                "Contact number",
                PatPhMobile,
                "Use a mobile number like 09171234567 or +639171234567."
            ) And ok


        '========================================
        ' SHOW ERRORS
        '========================================

        If Not ok Then

            ' Focus the first field that has an error

            If ep.GetError(txtStudentID) <> "" Then

                txtStudentID.Focus()

            ElseIf ep.GetError(txtLRN) <> "" Then

                txtLRN.Focus()

            ElseIf ep.GetError(txtLastName) <> "" Then

                txtLastName.Focus()

            ElseIf ep.GetError(txtFirstName) <> "" Then

                txtFirstName.Focus()

            ElseIf ep.GetError(txtMiddleName) <> "" Then

                txtMiddleName.Focus()

            ElseIf ep.GetError(cboCourse) <> "" Then

                cboCourse.Focus()

            ElseIf ep.GetError(cboYearLevel) <> "" Then

                cboYearLevel.Focus()

            ElseIf ep.GetError(cboSection) <> "" Then

                cboSection.Focus()

            ElseIf ep.GetError(txtContactNo) <> "" Then

                txtContactNo.Focus()

            End If


            ShowWarn(
                "Some fields need your attention. They are marked with a red icon.",
                "Check the form"
            )

        End If


        Return ok

    End Function


    '========================================
    ' DUPLICATE CHECK BEFORE SAVING
    '========================================
    Private Function IsDuplicate() As Boolean

        Dim dup As Boolean = False


        Try

            connection()


            sql =
                "SELECT StudentID, LRN " &
                "FROM tblstudents " &
                "WHERE (StudentID = @StudentID OR LRN = @LRN) " &
                "AND StudentID <> @Self " &
                "LIMIT 1"


            cmd = New MySqlCommand(
                sql,
                cn
            )


            cmd.Parameters.AddWithValue(
                "@StudentID",
                txtStudentID.Text.Trim()
            )


            cmd.Parameters.AddWithValue(
                "@LRN",
                txtLRN.Text.Trim()
            )


            cmd.Parameters.AddWithValue(
                "@Self",
                If(
                    StudentIDToEdit = "",
                    "",
                    StudentIDToEdit
                )
            )


            dr = cmd.ExecuteReader()


            If dr.Read() Then

                dup = True


                '========================================
                ' CHECK DUPLICATE STUDENT ID
                '========================================

                If dr("StudentID").ToString() =
                   txtStudentID.Text.Trim() Then

                    ep.SetError(
                        txtStudentID,
                        "This Student ID is already registered."
                    )

                    txtStudentID.Focus()


                    '========================================
                    ' CHECK DUPLICATE LRN
                    '========================================

                Else

                    ep.SetError(
                        txtLRN,
                        "This LRN already belongs to another student."
                    )

                    txtLRN.Focus()

                End If

            End If


            dr.Close()


        Catch ex As Exception

            If dr IsNot Nothing AndAlso
               Not dr.IsClosed Then

                dr.Close()

            End If


            ShowError(
                FriendlyDbError(
                    ex,
                    "check for duplicates"
                )
            )


            dup = True


        Finally

            If cn.State = ConnectionState.Open Then

                cn.Close()

            End If

        End Try


        Return dup

    End Function


    '========================================
    ' LOAD STUDENT FOR EDITING
    '========================================
    Private Sub LoadStudentForEdit()

        Try

            connection()


            sql =
                "SELECT StudentID, LRN, LastName, FirstName, MiddleName, " &
                "Course, YearLevel, Section, ContactNo, Status " &
                "FROM tblstudents " &
                "WHERE StudentID = @StudentID"


            cmd = New MySqlCommand(
                sql,
                cn
            )


            cmd.Parameters.AddWithValue(
                "@StudentID",
                StudentIDToEdit
            )


            dr = cmd.ExecuteReader()


            If dr.Read() Then

                '========================================
                ' STUDENT ID
                '========================================

                txtStudentID.Text =
                    dr("StudentID").ToString()


                '========================================
                ' LRN
                '========================================

                txtLRN.Text =
                    dr("LRN").ToString()


                '========================================
                ' NAME
                '========================================

                txtLastName.Text =
                    dr("LastName").ToString()


                txtFirstName.Text =
                    dr("FirstName").ToString()


                txtMiddleName.Text =
                    dr("MiddleName").ToString()


                '========================================
                ' COMBOBOX VALUES
                '========================================

                cboCourse.Text =
                    dr("Course").ToString()


                cboYearLevel.Text =
                    dr("YearLevel").ToString()


                cboSection.Text =
                    dr("Section").ToString()


                '========================================
                ' CONTACT
                '========================================

                txtContactNo.Text =
                    dr("ContactNo").ToString()


                '========================================
                ' STATUS
                '========================================

                txtStatus.Text =
                    dr("Status").ToString()


            Else

                MessageBox.Show(
                    "Student record not found.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                )

            End If


            dr.Close()


        Catch ex As Exception

            MessageBox.Show(
                "Error loading student information: " &
                ex.Message,
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )


        Finally

            If dr IsNot Nothing AndAlso
               Not dr.IsClosed Then

                dr.Close()

            End If


            If cn.State = ConnectionState.Open Then

                cn.Close()

            End If

        End Try

    End Sub


    '========================================
    ' ADD / UPDATE BUTTON
    '========================================
    Private Sub btnAddStudent_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnAddStudent.Click


        If Not ValidateForm() Then

            Exit Sub

        End If


        If IsDuplicate() Then

            Exit Sub

        End If


        '========================================
        ' EDIT MODE
        '========================================

        If StudentIDToEdit <> "" Then

            If Not Confirm(
                "Save changes to this student's record?",
                "Update Student"
            ) Then

                Exit Sub

            End If


            UpdateStudent()


        Else

            '========================================
            ' ADD MODE
            '========================================

            AddStudent()

        End If

    End Sub


    '========================================
    ' ADD NEW STUDENT
    '========================================
    Private Sub AddStudent()

        Try

            connection()


            sql =
                "INSERT INTO tblstudents " &
                "(StudentID, LRN, LastName, FirstName, MiddleName, Course, " &
                "YearLevel, Section, ContactNo, Status) " &
                "VALUES " &
                "(@StudentID, @LRN, @LastName, @FirstName, @MiddleName, " &
                "@Course, @YearLevel, @Section, @ContactNo, @Status)"


            cmd = New MySqlCommand(
                sql,
                cn
            )


            cmd.Parameters.AddWithValue(
                "@StudentID",
                txtStudentID.Text.Trim()
            )


            cmd.Parameters.AddWithValue(
                "@LRN",
                txtLRN.Text.Trim()
            )


            cmd.Parameters.AddWithValue(
                "@LastName",
                txtLastName.Text.Trim()
            )


            cmd.Parameters.AddWithValue(
                "@FirstName",
                txtFirstName.Text.Trim()
            )


            cmd.Parameters.AddWithValue(
                "@MiddleName",
                txtMiddleName.Text.Trim()
            )


            cmd.Parameters.AddWithValue(
                "@Course",
                cboCourse.Text.Trim()
            )


            cmd.Parameters.AddWithValue(
                "@YearLevel",
                cboYearLevel.Text.Trim()
            )


            cmd.Parameters.AddWithValue(
                "@Section",
                cboSection.Text.Trim()
            )


            cmd.Parameters.AddWithValue(
                "@ContactNo",
                txtContactNo.Text.Trim()
            )


            cmd.Parameters.AddWithValue(
                "@Status",
                "Active"
            )


            cmd.ExecuteNonQuery()


            MessageBox.Show(
                "Student added successfully." &
                Environment.NewLine &
                "Student ID: " &
                txtStudentID.Text,
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            )


            frmStudents.LoadStudents()

            frmStudents.Show()

            Me.Close()


        Catch ex As MySqlException

            ' Duplicate primary key
            If ex.Number = 1062 Then

                MessageBox.Show(
                    "The generated Student ID already exists." &
                    Environment.NewLine &
                    "Please try again.",
                    "Duplicate Student ID",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                )


                ' Generate another ID
                GenerateStudentID()


            Else

                ShowError(
                    FriendlyDbError(
                        ex,
                        "add the student"
                    )
                )

            End If


        Catch ex As Exception

            ShowError(
                FriendlyDbError(
                    ex,
                    "add the student"
                )
            )


        Finally

            If cn.State = ConnectionState.Open Then

                cn.Close()

            End If

        End Try

    End Sub


    '========================================
    ' UPDATE STUDENT
    '========================================
    Private Sub UpdateStudent()

        Try

            connection()


            sql =
                "UPDATE tblstudents SET " &
                "LRN = @LRN, " &
                "LastName = @LastName, " &
                "FirstName = @FirstName, " &
                "MiddleName = @MiddleName, " &
                "Course = @Course, " &
                "YearLevel = @YearLevel, " &
                "Section = @Section, " &
                "ContactNo = @ContactNo, " &
                "Status = @Status " &
                "WHERE StudentID = @StudentID"


            cmd = New MySqlCommand(
                sql,
                cn
            )


            cmd.Parameters.AddWithValue(
                "@LRN",
                txtLRN.Text.Trim()
            )


            cmd.Parameters.AddWithValue(
                "@LastName",
                txtLastName.Text.Trim()
            )


            cmd.Parameters.AddWithValue(
                "@FirstName",
                txtFirstName.Text.Trim()
            )


            cmd.Parameters.AddWithValue(
                "@MiddleName",
                txtMiddleName.Text.Trim()
            )


            cmd.Parameters.AddWithValue(
                "@Course",
                cboCourse.Text.Trim()
            )


            cmd.Parameters.AddWithValue(
                "@YearLevel",
                cboYearLevel.Text.Trim()
            )


            cmd.Parameters.AddWithValue(
                "@Section",
                cboSection.Text.Trim()
            )


            cmd.Parameters.AddWithValue(
                "@ContactNo",
                txtContactNo.Text.Trim()
            )


            cmd.Parameters.AddWithValue(
                "@Status",
                If(
                    txtStatus.Text.Trim() = "",
                    "Active",
                    txtStatus.Text.Trim()
                )
            )


            '========================================
            ' STUDENT ID
            '========================================
            ' Student ID is only used to locate
            ' the record.
            '
            ' It is NOT changed.

            cmd.Parameters.AddWithValue(
                "@StudentID",
                StudentIDToEdit
            )


            cmd.ExecuteNonQuery()


            MessageBox.Show(
                "Student information updated successfully.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            )


            frmStudents.LoadStudents()

            frmStudents.Show()

            Me.Close()


        Catch ex As Exception

            ShowError(
                FriendlyDbError(
                    ex,
                    "update the student"
                )
            )


        Finally

            If cn.State = ConnectionState.Open Then

                cn.Close()

            End If

        End Try

    End Sub


    '========================================
    ' CLEAR BUTTON
    '========================================
    Private Sub btnClear_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnClear.Click


        If isDirty AndAlso
           Not Confirm(
               "Clear everything you've typed?",
               "Clear form"
           ) Then

            Exit Sub

        End If


        ClearFields()

    End Sub


    '========================================
    ' CLEAR FIELDS
    '========================================
    Private Sub ClearFields()

        '========================================
        ' GENERATE A NEW STUDENT ID
        '========================================

        GenerateStudentID()


        txtLRN.Clear()

        txtLastName.Clear()

        txtFirstName.Clear()

        txtMiddleName.Clear()


        '========================================
        ' CLEAR COMBOBOXES
        '========================================

        cboCourse.SelectedIndex = -1

        cboYearLevel.SelectedIndex = -1

        cboSection.SelectedIndex = -1


        txtContactNo.Clear()


        txtStatus.Text = "Active"


        '========================================
        ' CLEAR ERRORS
        '========================================

        ep.SetError(
            txtStudentID,
            ""
        )

        ep.SetError(
            txtLRN,
            ""
        )

        ep.SetError(
            txtLastName,
            ""
        )

        ep.SetError(
            txtFirstName,
            ""
        )

        ep.SetError(
            txtMiddleName,
            ""
        )

        ep.SetError(
            cboCourse,
            ""
        )

        ep.SetError(
            cboYearLevel,
            ""
        )

        ep.SetError(
            cboSection,
            ""
        )

        ep.SetError(
            txtContactNo,
            ""
        )


        isDirty = False

        txtLRN.Focus()

    End Sub


    '========================================
    ' BACK BUTTON
    '========================================
    Private Sub btnBack_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnBack.Click


        If isDirty AndAlso
           Not Confirm(
               "You have unsaved changes. Leave without saving?",
               "Unsaved changes"
           ) Then

            Exit Sub

        End If


        frmStudents.LoadStudents()

        frmStudents.Show()

        Me.Close()

    End Sub


    '========================================
    ' LOGOUT
    '========================================
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

            frmLogin.Show()

            Me.Close()

        End If

    End Sub

End Class