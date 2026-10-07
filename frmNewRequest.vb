Imports MySql.Data.MySqlClient

Public Class frmNewRequest

    ' =========================================================
    ' STORES SELECTED DOCUMENTS
    ' =========================================================

    Private SelectedDocuments As New List(Of DocumentItem)


    ' =========================================================
    ' DOCUMENT ITEM CLASS
    ' =========================================================

    Private Class DocumentItem

        Public Property DocumentID As Integer
        Public Property DocumentName As String
        Public Property Fee As Decimal

        Public Overrides Function ToString() As String

            Return DocumentName &
                   " - ₱" &
                   Fee.ToString("N2")

        End Function

    End Class


    ' =========================================================
    ' FORM LOAD
    ' =========================================================

    Private Sub frmNewRequest_Load(
        sender As Object,
        e As EventArgs
    ) Handles MyBase.Load



        ApplyRoleAccess(
            Me,
            btnUsers,
            PictureBoxUsers)

        txtRequestNo.ReadOnly = True

        btnSaveRequest.Text = "Proceed to Payment"

        ' =====================================================
        ' REQUEST DATE
        ' =====================================================

        dtpRequestDate.MinDate =
            DateTime.Today

        dtpRequestDate.MaxDate =
            New DateTime(9998, 12, 31)

        dtpRequestDate.Value =
            DateTime.Today

        ' =====================================================
        ' GENERATE REQUEST NUMBER
        ' =====================================================

        GenerateRequestNumber()

        ' =====================================================
        ' LOAD AVAILABLE DOCUMENTS
        ' =====================================================

        LoadDocuments()

        ' =====================================================
        ' CLEAR FORM
        ' =====================================================

        ClearNewRequestForm()

    End Sub


    ' =========================================================
    ' GENERATE REQUEST NUMBER
    ' =========================================================

    Private Sub GenerateRequestNumber()

        Try

            connection()

            sql =
                "SELECT RequestNo " &
                "FROM tblrequest " &
                "ORDER BY RequestID DESC " &
                "LIMIT 1"

            cmd =
                New MySqlCommand(
                    sql,
                    cn)

            Dim lastRequest As Object =
                cmd.ExecuteScalar()

            Dim nextNumber As Integer = 1

            If lastRequest IsNot Nothing AndAlso
               lastRequest IsNot DBNull.Value Then

                Dim lastRequestNo As String =
                    lastRequest.ToString()

                Dim parts() As String =
                    lastRequestNo.Split("-"c)

                If parts.Length >= 3 Then

                    Dim previousNumber As Integer

                    If Integer.TryParse(
                        parts(parts.Length - 1),
                        previousNumber) Then

                        nextNumber =
                            previousNumber + 1

                    End If

                End If

            End If

            txtRequestNo.Text =
                "REQ-" &
                DateTime.Now.ToString("yyyyMMdd") &
                "-" &
                nextNumber.ToString("000")

        Catch ex As Exception

            MessageBox.Show(
                "Error generating request number: " &
                ex.Message,
                "Error",
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
    ' LOAD AVAILABLE DOCUMENTS
    ' =========================================================

    Private Sub LoadDocuments()

        Try

            connection()

            sql =
                "SELECT DocumentID, DocumentName, Fee " &
                "FROM tbldocuments " &
                "WHERE Status = 'Active' " &
                "ORDER BY DocumentID ASC"

            cmd =
                New MySqlCommand(
                    sql,
                    cn)

            dr =
                cmd.ExecuteReader()

            clbDocuments.Items.Clear()

            While dr.Read()

                Dim item As New DocumentItem

                item.DocumentID =
                    Convert.ToInt32(
                        dr("DocumentID"))

                item.DocumentName =
                    dr("DocumentName").ToString()

                item.Fee =
                    Convert.ToDecimal(
                        dr("Fee"))

                clbDocuments.Items.Add(item)

            End While

            dr.Close()

        Catch ex As Exception

            If dr IsNot Nothing AndAlso
               Not dr.IsClosed Then

                dr.Close()

            End If

            MessageBox.Show(
                "Error loading documents: " &
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
    ' DOCUMENT CHECKED / UNCHECKED
    ' =========================================================

    Private Sub clbDocuments_ItemCheck(
        sender As Object,
        e As ItemCheckEventArgs
    ) Handles clbDocuments.ItemCheck

        Me.BeginInvoke(
            New MethodInvoker(
                AddressOf UpdateSelectedDocuments))

    End Sub


    ' =========================================================
    ' UPDATE SELECTED DOCUMENTS
    ' =========================================================

    Private Sub UpdateSelectedDocuments()

        SelectedDocuments.Clear()

        lstSelectedDocuments.Items.Clear()

        Dim total As Decimal = 0D

        For Each item As Object
            In clbDocuments.CheckedItems

            Dim document As DocumentItem =
                DirectCast(
                    item,
                    DocumentItem)

            SelectedDocuments.Add(
                document)

            lstSelectedDocuments.Items.Add(
                document.DocumentName &
                " - ₱" &
                document.Fee.ToString("N2"))

            total += document.Fee

        Next

        lblTotalAmount.Text =
            "₱" &
            total.ToString("N2")

    End Sub


    ' =========================================================
    ' SEARCH STUDENT
    ' =========================================================

    Private Sub btnSearchStudent_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnSearchStudent.Click

        If txtStudentID.Text.Trim() = "" Then

            MessageBox.Show(
                "Please enter a Student ID.",
                "Student Search",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            txtStudentID.Focus()

            Exit Sub

        End If


        Try

            connection()

            sql =
                "SELECT StudentID, " &
                "FirstName, " &
                "MiddleName, " &
                "LastName, " &
                "Course, " &
                "YearLevel " &
                "FROM tblstudents " &
                "WHERE StudentID = @StudentID " &
                "AND Status = 'Active'"

            cmd =
                New MySqlCommand(
                    sql,
                    cn)

            cmd.Parameters.AddWithValue(
                "@StudentID",
                txtStudentID.Text.Trim())

            dr =
                cmd.ExecuteReader()

            If dr.Read() Then

                Dim firstName As String =
                    dr("FirstName").ToString().Trim()

                Dim middleName As String =
                    dr("MiddleName").ToString().Trim()

                Dim lastName As String =
                    dr("LastName").ToString().Trim()

                Dim fullName As String =
                    firstName

                If middleName <> "" Then

                    fullName &=
                        " " &
                        middleName

                End If

                If lastName <> "" Then

                    fullName &=
                        " " &
                        lastName

                End If

                txtStudentName.Text =
                    fullName

                txtCourse.Text =
                    dr("Course").ToString()

                txtYearLevel.Text =
                    dr("YearLevel").ToString()

            Else

                MessageBox.Show(
                    "Student not found or is inactive.",
                    "Student Search",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning)

                ClearStudentInformation()

            End If

            dr.Close()

        Catch ex As Exception

            If dr IsNot Nothing AndAlso
               Not dr.IsClosed Then

                dr.Close()

            End If

            MessageBox.Show(
                "Error searching student: " &
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
    ' ENTER KEY FOR STUDENT SEARCH
    ' =========================================================

    Private Sub txtStudentID_KeyDown(
        sender As Object,
        e As KeyEventArgs
    ) Handles txtStudentID.KeyDown

        If e.KeyCode = Keys.Enter Then

            btnSearchStudent.PerformClick()

            e.SuppressKeyPress = True

        End If

    End Sub


    ' =========================================================
    ' CLEAR STUDENT INFORMATION
    ' =========================================================

    Private Sub ClearStudentInformation()

        txtStudentName.Clear()
        txtCourse.Clear()
        txtYearLevel.Clear()

    End Sub


    ' =========================================================
    ' CLEAR ENTIRE NEW REQUEST FORM
    ' =========================================================

    Public Sub ClearNewRequestForm()

        ' Clear student information
        txtStudentID.Clear()
        txtStudentName.Clear()
        txtCourse.Clear()
        txtYearLevel.Clear()

        ' Clear selected document list
        SelectedDocuments.Clear()

        lstSelectedDocuments.Items.Clear()

        ' Uncheck all documents
        For i As Integer =
            0 To clbDocuments.Items.Count - 1

            clbDocuments.SetItemChecked(
                i,
                False)

        Next

        ' Reset total
        lblTotalAmount.Text =
            "₱0.00"

        ' Reset request date
        dtpRequestDate.Value =
            DateTime.Today

        ' Generate a fresh request number
        GenerateRequestNumber()

        ' Return focus
        txtStudentID.Focus()

    End Sub


    ' =========================================================
    ' PUBLIC RESET
    ' =========================================================

    Public Sub ResetForm()

        ClearNewRequestForm()

    End Sub


    ' =========================================================
    ' PROCEED TO PAYMENT
    ' =========================================================

    Private Sub btnSaveRequest_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnSaveRequest.Click

        ' =====================================================
        ' VALIDATE STUDENT ID
        ' =====================================================

        If txtStudentID.Text.Trim() = "" Then

            MessageBox.Show(
                "Please enter a Student ID.",
                "Validation",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            txtStudentID.Focus()

            Exit Sub

        End If


        ' =====================================================
        ' VALIDATE STUDENT
        ' =====================================================

        If txtStudentName.Text.Trim() = "" Then

            MessageBox.Show(
                "Please search and select a valid student.",
                "Validation",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            txtStudentID.Focus()

            Exit Sub

        End If


        ' =====================================================
        ' VALIDATE DOCUMENTS
        ' =====================================================

        If SelectedDocuments.Count = 0 Then

            MessageBox.Show(
                "Please select at least one document.",
                "Validation",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            Exit Sub

        End If


        ' =====================================================
        ' VALIDATE REQUEST DATE
        ' =====================================================

        If dtpRequestDate.Value.Date <
           DateTime.Today Then

            MessageBox.Show(
                "Request Date cannot be earlier than today.",
                "Invalid Request Date",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            dtpRequestDate.Value =
                DateTime.Today

            Exit Sub

        End If


        ' =====================================================
        ' CALCULATE TOTAL
        ' =====================================================

        Dim totalAmount As Decimal = 0D

        For Each document As DocumentItem
            In SelectedDocuments

            totalAmount +=
                document.Fee

        Next


        ' =====================================================
        ' CONFIRM
        ' =====================================================

        Dim confirmation As DialogResult =
            MessageBox.Show(
                "Proceed to payment?" &
                vbCrLf & vbCrLf &
                "Request Number: " &
                txtRequestNo.Text &
                vbCrLf &
                "Student: " &
                txtStudentName.Text &
                vbCrLf &
                "Request Date: " &
                dtpRequestDate.Value.ToString(
                    "MMMM dd, yyyy") &
                vbCrLf &
                "Total Amount: ₱" &
                totalAmount.ToString("N2") &
                vbCrLf & vbCrLf &
                "You will enter the payment information on the next form.",
                "Proceed to Payment",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question)


        If confirmation <>
            DialogResult.Yes Then

            Exit Sub

        End If


        ' =====================================================
        ' SEND INFORMATION TO frmPayment
        ' =====================================================

        Try

            frmPayment.RequestNumberToPay =
                txtRequestNo.Text.Trim()

            frmPayment.RequestDateToPay =
                dtpRequestDate.Value.Date


            frmPayment.StudentIDToPay =
                txtStudentID.Text.Trim()

            frmPayment.StudentNameToPay =
                txtStudentName.Text.Trim()

            frmPayment.CourseToPay =
                txtCourse.Text.Trim()

            frmPayment.YearLevelToPay =
                txtYearLevel.Text.Trim()


            ' Clear previous payment documents
            frmPayment.SelectedDocumentsToPay.Clear()


            ' Send selected documents
            For Each document As DocumentItem
                In SelectedDocuments

                Dim paymentDocument As New frmPayment.PaymentDocumentItem

                paymentDocument.DocumentID =
                    document.DocumentID

                paymentDocument.DocumentName =
                    document.DocumentName

                paymentDocument.Fee =
                    document.Fee

                paymentDocument.Quantity = 1

                frmPayment.SelectedDocumentsToPay.Add(
                    paymentDocument)

            Next


            '=====================================================
            ' REFRESH PAYMENT FORM
            '=====================================================

            ' frmPayment may already exist but be hidden.
            ' Because of that, its Load event will NOT run again.
            ' Explicitly reload the new request information.

            frmPayment.LoadPaymentInformation()

            '=====================================================
            ' OPEN PAYMENT
            '=====================================================

            frmPayment.Show()
            frmPayment.BringToFront()

            Me.Hide()

        Catch ex As Exception

            MessageBox.Show(
                "Unable to proceed to payment: " &
                ex.Message,
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)

        End Try

    End Sub


    ' =========================================================
    ' CANCEL NEW REQUEST
    ' =========================================================

    Private Sub btnCancel_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnCancel.Click

        Dim result As DialogResult =
            MessageBox.Show(
                "Are you sure you want to cancel this request?",
                "Cancel Request",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question)


        If result =
            DialogResult.Yes Then

            ' Clear everything
            ResetForm()

            ' Show refreshed request list
            frmRequestList.Show()

            ' Hide this form
            Me.Hide()

        End If

    End Sub


    ' =========================================================
    ' STUDENTS NAVIGATION
    ' =========================================================

    Private Sub btnStudents_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnStudents.Click

        frmStudents.Show()

        Me.Hide()

    End Sub


    ' =========================================================
    ' DOCUMENTS NAVIGATION
    ' =========================================================

    Private Sub btnDocuments_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnDocuments.Click

        frmDocuments.Show()

        Me.Hide()

    End Sub


    ' =========================================================
    ' REQUESTS NAVIGATION
    ' =========================================================

    Private Sub btnRequests_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnRequests.Click

        frmRequestList.Show()

        Me.Hide()

    End Sub


    ' =========================================================
    ' REPORTS NAVIGATION
    ' =========================================================

    Private Sub btnReports_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnReports.Click

        frmReports.Show()

        Me.Hide()

    End Sub


    ' =========================================================
    ' USERS NAVIGATION
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
    ' REQUEST DATE CHANGED
    ' =========================================================

    Private Sub dtpRequestDate_ValueChanged(
        sender As Object,
        e As EventArgs
    ) Handles dtpRequestDate.ValueChanged

        ' MinDate prevents past dates.
        ' Future dates are allowed.

    End Sub


    ' =========================================================
    ' CLEAR BUTTON
    ' =========================================================

    Private Sub btnClear_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnClear.Click

        ResetForm()

    End Sub


    ' =========================================================
    ' STUDENT NAME READ-ONLY
    ' =========================================================

    Private Sub txtStudentName_TextChanged(
        sender As Object,
        e As EventArgs
    ) Handles txtStudentName.TextChanged

        txtStudentName.ReadOnly = True

    End Sub


    ' =========================================================
    ' COURSE READ-ONLY
    ' =========================================================

    Private Sub txtCourse_TextChanged(
        sender As Object,
        e As EventArgs
    ) Handles txtCourse.TextChanged

        txtCourse.ReadOnly = True

    End Sub


    ' =========================================================
    ' YEAR LEVEL READ-ONLY
    ' =========================================================

    Private Sub txtYearLevel_TextChanged(
        sender As Object,
        e As EventArgs
    ) Handles txtYearLevel.TextChanged

        txtYearLevel.ReadOnly = True

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