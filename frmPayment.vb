Imports MySql.Data.MySqlClient

Public Class frmPayment

    '========================================================
    ' DATA RECEIVED FROM frmNewRequest
    '========================================================

    Public RequestNumberToPay As String = ""
    Public RequestDateToPay As DateTime = DateTime.Today

    Public StudentIDToPay As String = ""
    Public StudentNameToPay As String = ""
    Public CourseToPay As String = ""
    Public YearLevelToPay As String = ""


    '========================================================
    ' DOCUMENT ITEM
    '========================================================

    Public Class PaymentDocumentItem

        Public Property DocumentID As Integer
        Public Property DocumentName As String
        Public Property Fee As Decimal
        Public Property Quantity As Integer

        Public ReadOnly Property SubTotal As Decimal

            Get

                Return Fee

            End Get

        End Property

    End Class


    'Documents received from frmNewRequest
    Public SelectedDocumentsToPay As New List(Of PaymentDocumentItem)


    '========================================================
    ' REQUESTED DOCUMENT DISPLAY
    '========================================================

    Private rtbRequestedDocuments As RichTextBox


    '========================================================
    ' FORM LOAD
    '========================================================

    Private Sub frmPayment_Load(
        sender As Object,
        e As EventArgs
    ) Handles MyBase.Load

        ThemeForm(Me)

        LoadPaymentInformation()

    End Sub


    '========================================================
    ' LOAD PAYMENT INFORMATION
    '========================================================

    Public Sub LoadPaymentInformation()

        txtRequestNumber.Text =
        RequestNumberToPay

        If RequestDateToPay.Date <
       DateTime.Today Then

            RequestDateToPay =
            DateTime.Today

        End If

        dtpRequestDate.MinDate =
        DateTime.Today

        dtpRequestDate.MaxDate =
        New DateTime(9998, 12, 31)

        dtpRequestDate.Value =
        RequestDateToPay.Date

        txtStudentID.Text =
        StudentIDToPay

        txtStudentName.Text =
        StudentNameToPay

        txtCourse.Text =
        CourseToPay

        txtYearLevel.Text =
        YearLevelToPay

        '====================================================
        ' READ-ONLY INFORMATION
        '====================================================

        txtRequestNumber.ReadOnly = True
        txtStudentID.ReadOnly = True
        txtStudentName.ReadOnly = True
        txtCourse.ReadOnly = True
        txtYearLevel.ReadOnly = True

        dtpRequestDate.Enabled = False

        '====================================================
        ' OR DATE
        '====================================================

        dtpORDate.MinDate =
        DateTime.Today

        dtpORDate.MaxDate =
        DateTime.Today

        dtpORDate.Value =
        DateTime.Today

        '====================================================
        ' OR NUMBER
        '====================================================

        txtORNo.Text =
        "OR-2026-"

        txtORNo.SelectionStart =
        txtORNo.Text.Length

        txtORNo.SelectionLength = 0

        '====================================================
        ' CREATE DOCUMENT DISPLAY
        '====================================================

        CreateRequestedDocumentsDisplay()

        '====================================================
        ' DISPLAY SELECTED DOCUMENTS
        '====================================================

        DisplaySelectedDocuments()

        '====================================================
        ' FOCUS OR NUMBER
        '====================================================

        txtORNo.Focus()

    End Sub


    '========================================================
    ' CREATE REQUESTED DOCUMENT DISPLAY
    '========================================================

    Private Sub CreateRequestedDocumentsDisplay()

        If rtbRequestedDocuments IsNot Nothing Then
            Return
        End If


        Dim requestedGroup As GroupBox = Nothing

        requestedGroup =
            FindGroupBox(
                Me,
                "Requested Documents")


        If requestedGroup Is Nothing Then

            MessageBox.Show(
                "The 'Requested Documents' section could not be found.",
                "Form Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            Return

        End If


        rtbRequestedDocuments =
            New RichTextBox()


        With rtbRequestedDocuments

            .Name =
                "rtbRequestedDocuments"

            .ReadOnly =
                True

            .BackColor =
                Color.White

            .ForeColor =
                Color.Black

            .BorderStyle =
                BorderStyle.FixedSingle

            .Font =
                New Font(
                    "Segoe UI",
                    10.0F,
                    FontStyle.Regular)

            .Dock =
                DockStyle.Fill

            .Margin =
                New Padding(5)

            .ScrollBars =
                RichTextBoxScrollBars.Vertical

            .DetectUrls =
                False

            .TabStop =
                False

        End With


        requestedGroup.Controls.Add(
            rtbRequestedDocuments)

        rtbRequestedDocuments.BringToFront()

    End Sub


    '========================================================
    ' FIND GROUPBOX
    '========================================================

    Private Function FindGroupBox(
        parent As Control,
        groupText As String
    ) As GroupBox

        For Each control As Control In parent.Controls

            If TypeOf control Is GroupBox Then

                Dim grp As GroupBox =
                    DirectCast(
                        control,
                        GroupBox)


                If grp.Text.Trim().Equals(
                    groupText,
                    StringComparison.OrdinalIgnoreCase) Then

                    Return grp

                End If

            End If


            If control.HasChildren Then

                Dim result As GroupBox =
                    FindGroupBox(
                        control,
                        groupText)


                If result IsNot Nothing Then

                    Return result

                End If

            End If

        Next


        Return Nothing

    End Function


    '========================================================
    ' DISPLAY SELECTED DOCUMENTS
    '========================================================

    Private Sub DisplaySelectedDocuments()

        If rtbRequestedDocuments Is Nothing Then
            Return
        End If


        rtbRequestedDocuments.Clear()


        If SelectedDocumentsToPay Is Nothing OrElse
           SelectedDocumentsToPay.Count = 0 Then

            rtbRequestedDocuments.Text =
                "No documents were selected."

            lblTotalAmount.Text =
                "₱0.00"

            Return

        End If


        Dim totalAmount As Decimal = 0D

        Dim documentNumber As Integer = 1


        For Each doc As PaymentDocumentItem
            In SelectedDocumentsToPay

            totalAmount +=
                doc.Fee


            rtbRequestedDocuments.AppendText(
                documentNumber.ToString() &
                ". " &
                doc.DocumentName &
                Environment.NewLine)


            rtbRequestedDocuments.AppendText(
                "   Fee: ₱" &
                doc.Fee.ToString("N2") &
                Environment.NewLine)


            rtbRequestedDocuments.AppendText(
                Environment.NewLine)


            documentNumber += 1

        Next


        rtbRequestedDocuments.AppendText(
            "----------------------------------------" &
            Environment.NewLine)


        rtbRequestedDocuments.AppendText(
            "TOTAL AMOUNT: ₱" &
            totalAmount.ToString("N2"))


        lblTotalAmount.Text =
            "₱" &
            totalAmount.ToString("N2")

    End Sub


    '========================================================
    ' GET TOTAL AMOUNT
    '========================================================

    Private Function GetTotalAmount() As Decimal

        Dim total As Decimal = 0D


        If SelectedDocumentsToPay Is Nothing Then

            Return total

        End If


        For Each doc As PaymentDocumentItem
            In SelectedDocumentsToPay

            total +=
                doc.Fee

        Next


        Return total

    End Function


    '========================================================
    ' OR NUMBER - ONLY ALLOW NUMBERS
    '========================================================

    Private Sub txtORNo_KeyPress(
        sender As Object,
        e As KeyPressEventArgs
    ) Handles txtORNo.KeyPress

        'Allow Backspace and other control keys
        If Char.IsControl(e.KeyChar) Then
            Exit Sub
        End If


        'Only allow numbers
        If Not Char.IsDigit(e.KeyChar) Then

            e.Handled = True

            Exit Sub

        End If


        'Protect OR-2026-
        If txtORNo.SelectionStart < 8 Then

            e.Handled = True

            Exit Sub

        End If


        'Maximum 6 digits
        If txtORNo.Text.Length >= 14 AndAlso
           txtORNo.SelectionLength = 0 Then

            e.Handled = True

        End If

    End Sub


    '========================================================
    ' PROTECT FIXED PREFIX
    '========================================================

    Private Sub txtORNo_KeyDown(
        sender As Object,
        e As KeyEventArgs
    ) Handles txtORNo.KeyDown

        If e.KeyCode = Keys.Back AndAlso
           txtORNo.SelectionStart <= 8 Then

            e.SuppressKeyPress = True

        End If


        If e.KeyCode = Keys.Delete AndAlso
           txtORNo.SelectionStart < 8 Then

            e.SuppressKeyPress = True

        End If

    End Sub


    '========================================================
    ' SAVE PAYMENT
    '========================================================

    Private Sub btnSavePayment_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnSavePayment.Click


        '====================================================
        ' VALIDATION
        '====================================================

        If String.IsNullOrWhiteSpace(
            txtRequestNumber.Text) Then

            MessageBox.Show(
                "Request Number is missing.",
                "Validation Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            Exit Sub

        End If


        If String.IsNullOrWhiteSpace(
            txtStudentID.Text) Then

            MessageBox.Show(
                "Student ID is missing.",
                "Validation Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            Exit Sub

        End If


        If SelectedDocumentsToPay Is Nothing OrElse
           SelectedDocumentsToPay.Count = 0 Then

            MessageBox.Show(
                "No documents were selected for this request.",
                "Validation Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            Exit Sub

        End If


        '====================================================
        ' OR NUMBER REQUIRED
        '====================================================

        If String.IsNullOrWhiteSpace(
            txtORNo.Text) Then

            MessageBox.Show(
                "Please enter the OR Number.",
                "Payment Required",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            txtORNo.Focus()

            Exit Sub

        End If


        '====================================================
        ' VALIDATE OR NUMBER FORMAT
        '====================================================

        Dim orNumber As String =
            txtORNo.Text.Trim()


        If orNumber.Length <> 14 Then

            MessageBox.Show(
                "Invalid OR Number." &
                Environment.NewLine &
                Environment.NewLine &
                "Format must be:" &
                Environment.NewLine &
                "OR-2026-000001",
                "Invalid OR Number",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            txtORNo.Focus()

            Exit Sub

        End If


        If Not orNumber.StartsWith(
            "OR-2026-") Then

            MessageBox.Show(
                "Invalid OR Number." &
                Environment.NewLine &
                Environment.NewLine &
                "The OR Number must start with OR-2026-." &
                Environment.NewLine &
                Environment.NewLine &
                "Example: OR-2026-000001",
                "Invalid OR Number",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            txtORNo.Focus()

            Exit Sub

        End If


        Dim orDigits As String =
            orNumber.Substring(8)


        If orDigits.Length <> 6 Then

            MessageBox.Show(
                "Invalid OR Number." &
                Environment.NewLine &
                Environment.NewLine &
                "Exactly 6 digits are required after OR-2026-." &
                Environment.NewLine &
                Environment.NewLine &
                "Example: OR-2026-000001",
                "Invalid OR Number",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            txtORNo.Focus()

            Exit Sub

        End If


        If Not IsNumeric(orDigits) Then

            MessageBox.Show(
                "Invalid OR Number." &
                Environment.NewLine &
                Environment.NewLine &
                "Only numbers are allowed after OR-2026-.",
                "Invalid OR Number",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            txtORNo.Focus()

            Exit Sub

        End If


        '====================================================
        ' OR DATE MUST BE TODAY
        '====================================================

        If dtpORDate.Value.Date <>
           DateTime.Today Then

            MessageBox.Show(
                "OR Date must be today's date.",
                "Invalid OR Date",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            dtpORDate.Value =
                DateTime.Today

            Exit Sub

        End If


        '====================================================
        ' TOTAL AMOUNT
        '====================================================

        Dim totalAmount As Decimal =
            GetTotalAmount()


        If totalAmount <= 0D Then

            MessageBox.Show(
                "Total Amount must be greater than zero.",
                "Invalid Amount",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            Exit Sub

        End If


        '====================================================
        ' CHECK DUPLICATE OR NUMBER
        '====================================================

        Try

            connection()


            Dim checkSQL As String =
                "SELECT COUNT(*) " &
                "FROM tblrequest " &
                "WHERE ORNo = @ORNo"


            Using checkCmd As New MySqlCommand(
                checkSQL,
                cn)

                checkCmd.Parameters.AddWithValue(
                    "@ORNo",
                    orNumber)


                Dim count As Integer =
                    Convert.ToInt32(
                        checkCmd.ExecuteScalar())


                If count > 0 Then

                    MessageBox.Show(
                        "The OR Number already exists." &
                        Environment.NewLine &
                        Environment.NewLine &
                        "Please enter a unique OR Number.",
                        "Duplicate OR Number",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning)

                    txtORNo.Focus()

                    Exit Sub

                End If

            End Using


        Catch ex As Exception

            MessageBox.Show(
                "Unable to verify the OR Number." &
                Environment.NewLine &
                Environment.NewLine &
                ex.Message,
                "Database Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)

            Exit Sub


        Finally

            If cn.State =
                ConnectionState.Open Then

                cn.Close()

            End If

        End Try


        '====================================================
        ' DETERMINE REQUEST STATUS
        '====================================================

        Dim initialStatus As String


        If RequestDateToPay.Date =
           DateTime.Today Then

            initialStatus =
                "Processing"

        Else

            initialStatus =
                "Pending"

        End If


        '====================================================
        ' CONFIRM PAYMENT
        '====================================================

        Dim confirmation As DialogResult =
            MessageBox.Show(
                "Request Number: " &
                txtRequestNumber.Text.Trim() &
                Environment.NewLine &
                "Student: " &
                txtStudentName.Text.Trim() &
                Environment.NewLine &
                "Total Amount: ₱" &
                totalAmount.ToString("N2") &
                Environment.NewLine &
                "OR Number: " &
                orNumber &
                Environment.NewLine &
                "OR Date: " &
                dtpORDate.Value.ToString(
                    "MMMM dd, yyyy") &
                Environment.NewLine &
                "Payment Status: PAID" &
                Environment.NewLine &
                "Request Status: " &
                initialStatus &
                Environment.NewLine &
                Environment.NewLine &
                "Save this payment?",
                "Confirm Payment",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question)


        If confirmation <>
            DialogResult.Yes Then

            Exit Sub

        End If


        '====================================================
        ' SAVE TO DATABASE
        '====================================================

        Try

            connection()


            Dim transaction As MySqlTransaction =
                cn.BeginTransaction()


            Try

                '================================================
                ' GET CURRENT USER
                '================================================

                Dim createdBy As Integer =
                    GetCurrentUserID(
                        transaction)


                '================================================
                ' INSERT REQUEST
                '================================================

                Dim requestSQL As String =
                    "INSERT INTO tblrequest " &
                    "(RequestNo, StudentID, RequestDate, " &
                    "TotalAmount, PaymentStatus, ORNo, " &
                    "ORDate, Status, CreatedBy) " &
                    "VALUES " &
                    "(@RequestNo, @StudentID, @RequestDate, " &
                    "@TotalAmount, @PaymentStatus, @ORNo, " &
                    "@ORDate, @Status, @CreatedBy)"


                Dim requestID As Integer


                Using requestCmd As New MySqlCommand(
                    requestSQL,
                    cn,
                    transaction)

                    requestCmd.Parameters.AddWithValue(
                        "@RequestNo",
                        txtRequestNumber.Text.Trim())

                    requestCmd.Parameters.AddWithValue(
                        "@StudentID",
                        txtStudentID.Text.Trim())

                    requestCmd.Parameters.AddWithValue(
                        "@RequestDate",
                        RequestDateToPay.Date)

                    requestCmd.Parameters.AddWithValue(
                        "@TotalAmount",
                        totalAmount)

                    requestCmd.Parameters.AddWithValue(
                        "@PaymentStatus",
                        "Paid")

                    requestCmd.Parameters.AddWithValue(
                        "@ORNo",
                        orNumber)

                    requestCmd.Parameters.AddWithValue(
                        "@ORDate",
                        dtpORDate.Value.Date)

                    requestCmd.Parameters.AddWithValue(
                        "@Status",
                        initialStatus)

                    requestCmd.Parameters.AddWithValue(
                        "@CreatedBy",
                        createdBy)


                    requestCmd.ExecuteNonQuery()


                    requestID =
                        Convert.ToInt32(
                            requestCmd.LastInsertedId)

                End Using


                '================================================
                ' INSERT DOCUMENT DETAILS
                '================================================

                For Each doc As PaymentDocumentItem
                    In SelectedDocumentsToPay

                    Dim detailSQL As String =
                        "INSERT INTO tblrequestdetails " &
                        "(RequestID, DocumentID, Quantity, " &
                        "Amount, SubTotal) " &
                        "VALUES " &
                        "(@RequestID, @DocumentID, @Quantity, " &
                        "@Amount, @SubTotal)"


                    Using detailCmd As New MySqlCommand(
                        detailSQL,
                        cn,
                        transaction)

                        detailCmd.Parameters.AddWithValue(
                            "@RequestID",
                            requestID)

                        detailCmd.Parameters.AddWithValue(
                            "@DocumentID",
                            doc.DocumentID)

                        detailCmd.Parameters.AddWithValue(
                            "@Quantity",
                            1)

                        detailCmd.Parameters.AddWithValue(
                            "@Amount",
                            doc.Fee)

                        detailCmd.Parameters.AddWithValue(
                            "@SubTotal",
                            doc.Fee)


                        detailCmd.ExecuteNonQuery()

                    End Using

                Next


                '================================================
                ' COMMIT
                '================================================

                transaction.Commit()


                '================================================
                ' SUCCESS MESSAGE
                '================================================

                MessageBox.Show(
                    "Payment saved successfully." &
                    Environment.NewLine &
                    Environment.NewLine &
                    "Request Number: " &
                    txtRequestNumber.Text.Trim() &
                    Environment.NewLine &
                    "Payment Status: PAID" &
                    Environment.NewLine &
                    "Request Status: " &
                    initialStatus &
                    Environment.NewLine &
                    "OR Number: " &
                    orNumber &
                    Environment.NewLine &
                    "OR Date: " &
                    dtpORDate.Value.ToString(
                        "MMMM dd, yyyy") &
                    Environment.NewLine &
                    "Total Amount: ₱" &
                    totalAmount.ToString("N2"),
                    "Payment Successful",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information)


                '================================================
                ' CLEAR PAYMENT FORM
                '================================================

                ResetPaymentForm()


                '================================================
                ' CLEAR NEW REQUEST FORM
                '================================================

                frmNewRequest.ResetForm()


                '================================================
                ' SHOW REQUEST LIST
                '================================================

                frmRequestList.Show()

                Me.Hide()


            Catch ex As Exception

                Try

                    transaction.Rollback()

                Catch

                End Try

                Throw

            End Try


        Catch ex As MySqlException

            MessageBox.Show(
                "Database error while saving the payment." &
                Environment.NewLine &
                Environment.NewLine &
                ex.Message,
                "Database Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)


        Catch ex As Exception

            MessageBox.Show(
                "An error occurred while saving the payment." &
                Environment.NewLine &
                Environment.NewLine &
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


    '========================================================
    ' RESET PAYMENT FORM
    '========================================================

    Public Sub ResetPaymentForm()

        RequestNumberToPay = ""
        RequestDateToPay = DateTime.Today

        StudentIDToPay = ""
        StudentNameToPay = ""
        CourseToPay = ""
        YearLevelToPay = ""

        SelectedDocumentsToPay.Clear()


        txtRequestNumber.Clear()
        txtStudentID.Clear()
        txtStudentName.Clear()
        txtCourse.Clear()
        txtYearLevel.Clear()


        txtORNo.Text =
            "OR-2026-"

        txtORNo.SelectionStart =
            txtORNo.Text.Length

        txtORNo.SelectionLength = 0


        dtpRequestDate.Value =
            DateTime.Today

        dtpORDate.Value =
            DateTime.Today


        If rtbRequestedDocuments IsNot Nothing Then

            rtbRequestedDocuments.Clear()

        End If


        lblTotalAmount.Text =
            "₱0.00"

    End Sub


    '========================================================
    ' GET CURRENT USER ID
    '========================================================

    Private Function GetCurrentUserID(
        transaction As MySqlTransaction
    ) As Integer

        Try

            Dim username As String = ""


            If frmLogin.txtUsername IsNot Nothing Then

                username =
                    frmLogin.txtUsername.Text.Trim()

            End If


            If String.IsNullOrWhiteSpace(
                username) Then

                Return 1

            End If


            Dim userSQL As String =
                "SELECT UserID " &
                "FROM tblusers " &
                "WHERE Username = @Username " &
                "LIMIT 1"


            Using cmdUser As New MySqlCommand(
                userSQL,
                cn,
                transaction)

                cmdUser.Parameters.AddWithValue(
                    "@Username",
                    username)


                Dim result =
                    cmdUser.ExecuteScalar()


                If result IsNot Nothing AndAlso
                   Not IsDBNull(result) Then

                    Return Convert.ToInt32(result)

                End If

            End Using


        Catch

        End Try


        Return 1

    End Function


    '========================================================
    ' CANCEL PAYMENT
    '========================================================

    Private Sub btnCancel_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnCancel.Click

        Dim result As DialogResult =
            MessageBox.Show(
                "Cancel payment and return to a new request?",
                "Cancel Payment",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question)


        If result =
            DialogResult.Yes Then

            ' Clear payment
            ResetPaymentForm()

            ' Clear New Request
            frmNewRequest.ResetForm()

            ' Return to clean New Request
            frmNewRequest.Show()

            Me.Hide()

        End If

    End Sub


    '========================================================
    ' OR DATE
    '========================================================

    Private Sub dtpORDate_ValueChanged(
        sender As Object,
        e As EventArgs
    ) Handles dtpORDate.ValueChanged

        If dtpORDate.Value.Date <>
           DateTime.Today Then

            dtpORDate.Value =
                DateTime.Today

        End If

    End Sub

End Class