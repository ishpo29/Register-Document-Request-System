Imports MySql.Data.MySqlClient

Public Class frmRequestDetails

    ' =========================================================
    ' REQUEST ID FROM frmRequestList
    ' =========================================================

    Public RequestIDToView As Integer = 0


    ' =========================================================
    ' FORM LOAD
    ' =========================================================

    Private Sub frmRequestDetails_Load(
        sender As Object,
        e As EventArgs
    ) Handles MyBase.Load

        ThemeForm(Me)

        ApplyRoleAccess(Me, btnUsers, PictureBoxUsers)


        ' =====================================================
        ' DATE VALIDATION
        ' =====================================================

        ' Request Date can only be today
        dtpRequestDate.Value = DateTime.Today
        dtpRequestDate.MinDate = DateTime.Today
        dtpRequestDate.MaxDate = DateTime.Today

        ' OR Date can only be today
        dtpORDate.Value = DateTime.Today
        dtpORDate.MinDate = DateTime.Today
        dtpORDate.MaxDate = DateTime.Today


        ' =====================================================
        ' REQUEST STATUS
        ' =====================================================

        cboRequestStatus.Items.Clear()

        cboRequestStatus.Items.Add("Pending")
        cboRequestStatus.Items.Add("Processing")
        cboRequestStatus.Items.Add("Ready for Release")
        cboRequestStatus.Items.Add("Released")
        cboRequestStatus.Items.Add("Cancelled")

        ' Prevent manual typing
        cboRequestStatus.DropDownStyle =
            ComboBoxStyle.DropDownList


        ' =====================================================
        ' MAKE INFORMATION FIELDS READ-ONLY
        ' =====================================================

        txtRequestNo.ReadOnly = True

        txtStudentID.ReadOnly = True

        txtStudentName.ReadOnly = True

        txtCourse.ReadOnly = True

        txtYearLevel.ReadOnly = True

        txtORNo.ReadOnly = True


        ' =====================================================
        ' LOAD REQUEST
        ' =====================================================

        If RequestIDToView > 0 Then

            LoadRequestDetails()

        Else

            MessageBox.Show(
                "No request was selected.",
                "Request Details",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

        End If

    End Sub


    ' =========================================================
    ' LOAD REQUEST DETAILS
    ' =========================================================

    Private Sub LoadRequestDetails()

        Try

            connection()


            ' =====================================================
            ' GET REQUEST + STUDENT INFORMATION
            ' =====================================================

            sql = "SELECT " &
                  "r.RequestNo, " &
                  "r.RequestDate, " &
                  "r.StudentID, " &
                  "CONCAT(s.FirstName, ' ', " &
                  "s.MiddleName, ' ', " &
                  "s.LastName) AS StudentName, " &
                  "s.Course, " &
                  "s.YearLevel, " &
                  "r.TotalAmount, " &
                  "r.PaymentStatus, " &
                  "r.ORNo, " &
                  "r.ORDate, " &
                  "r.Status, " &
                  "r.CreatedBy " &
                  "FROM tblrequest r " &
                  "INNER JOIN tblstudents s " &
                  "ON r.StudentID = s.StudentID " &
                  "WHERE r.RequestID = @RequestID"


            cmd = New MySqlCommand(sql, cn)

            cmd.Parameters.AddWithValue(
                "@RequestID",
                RequestIDToView)


            dr = cmd.ExecuteReader()


            If dr.Read() Then

                ' =================================================
                ' REQUEST INFORMATION
                ' =================================================

                txtRequestNo.Text =
                    dr("RequestNo").ToString()


                ' Keep Request Date as today's date
                ' because this form only allows today's date
                dtpRequestDate.Value =
                    DateTime.Today


                ' =================================================
                ' STUDENT INFORMATION
                ' =================================================

                txtStudentID.Text =
                    dr("StudentID").ToString()


                txtStudentName.Text =
                    dr("StudentName").ToString()


                txtCourse.Text =
                    dr("Course").ToString()


                txtYearLevel.Text =
                    dr("YearLevel").ToString()


                ' =================================================
                ' TOTAL AMOUNT
                ' =================================================

                If Not IsDBNull(dr("TotalAmount")) Then

                    Dim totalAmount As Decimal =
                        Convert.ToDecimal(dr("TotalAmount"))

                    lblTotalAmount.Text =
                        "₱" & totalAmount.ToString("N2")

                Else

                    lblTotalAmount.Text =
                        "₱0.00"

                End If


                ' =================================================
                ' PAYMENT STATUS
                ' =================================================

                If Not IsDBNull(dr("PaymentStatus")) Then

                    lblPaymentStatus.Text =
                        dr("PaymentStatus").ToString()

                Else

                    lblPaymentStatus.Text =
                        "Pending"

                End If


                ' =================================================
                ' AMOUNT PAID
                ' =================================================

                If lblPaymentStatus.Text = "Paid" AndAlso
                   Not IsDBNull(dr("TotalAmount")) Then

                    Dim totalAmount As Decimal =
                        Convert.ToDecimal(dr("TotalAmount"))

                    lblAmountPaid.Text =
                        "₱" & totalAmount.ToString("N2")

                Else

                    lblAmountPaid.Text =
                        "₱0.00"

                End If


                ' =================================================
                ' OR NUMBER
                ' =================================================

                If Not IsDBNull(dr("ORNo")) Then

                    txtORNo.Text =
                        dr("ORNo").ToString()

                Else

                    txtORNo.Clear()

                End If


                ' =================================================
                ' OR DATE
                ' =================================================

                ' Only today's date is allowed
                dtpORDate.Value =
                    DateTime.Today


                ' =================================================
                ' REQUEST STATUS
                ' =================================================

                If Not IsDBNull(dr("Status")) Then

                    Dim requestStatus As String =
                        dr("Status").ToString()

                    If cboRequestStatus.Items.Contains(
                        requestStatus) Then

                        cboRequestStatus.SelectedItem =
                            requestStatus

                    Else

                        cboRequestStatus.SelectedIndex = -1

                    End If

                Else

                    cboRequestStatus.SelectedIndex = -1

                End If


                ' =================================================
                ' PROCESSED BY
                ' =================================================

                If Not IsDBNull(dr("CreatedBy")) Then

                    lblProcessedBy.Text =
                        dr("CreatedBy").ToString()

                Else

                    lblProcessedBy.Text =
                        "User"

                End If


            Else

                dr.Close()

                MessageBox.Show(
                    "Request not found.",
                    "Request Details",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning)

                Exit Sub

            End If


            dr.Close()


            ' =====================================================
            ' LOAD REQUESTED DOCUMENTS
            ' =====================================================

            LoadRequestDocuments()


        Catch ex As Exception

            If dr IsNot Nothing AndAlso
               Not dr.IsClosed Then

                dr.Close()

            End If


            MessageBox.Show(
                "Error loading request details: " &
                ex.Message,
                "Database Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)


        Finally

            If cn.State = ConnectionState.Open Then

                cn.Close()

            End If

        End Try

    End Sub


    ' =========================================================
    ' LOAD REQUESTED DOCUMENTS
    ' =========================================================

    Private Sub LoadRequestDocuments()

        Try

            connection()


            sql = "SELECT " &
                  "d.DocumentName, " &
                  "rd.Quantity, " &
                  "rd.Amount, " &
                  "rd.SubTotal " &
                  "FROM tblrequestdetails rd " &
                  "INNER JOIN tbldocuments d " &
                  "ON rd.DocumentID = d.DocumentID " &
                  "WHERE rd.RequestID = @RequestID " &
                  "ORDER BY rd.RequestDetailID"


            cmd = New MySqlCommand(sql, cn)

            cmd.Parameters.AddWithValue(
                "@RequestID",
                RequestIDToView)


            da = New MySqlDataAdapter(cmd)

            dt = New DataTable()

            da.Fill(dt)


            dgvRequestDocuments.DataSource = dt


            ' =====================================================
            ' DATAGRIDVIEW SETTINGS
            ' =====================================================

            dgvRequestDocuments.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill

            dgvRequestDocuments.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect

            dgvRequestDocuments.MultiSelect = False

            dgvRequestDocuments.ReadOnly = True

            dgvRequestDocuments.AllowUserToAddRows = False

            dgvRequestDocuments.AllowUserToDeleteRows = False

            dgvRequestDocuments.AllowUserToResizeColumns = False

            dgvRequestDocuments.RowHeadersVisible = False


            ' =====================================================
            ' DISABLE COLUMN SORTING
            ' =====================================================

            For Each column As DataGridViewColumn In
                dgvRequestDocuments.Columns

                column.ReadOnly = True

                column.SortMode =
                    DataGridViewColumnSortMode.NotSortable

            Next


            ' =====================================================
            ' COLUMN HEADERS
            ' =====================================================

            If dgvRequestDocuments.Columns.Contains(
                "DocumentName") Then

                dgvRequestDocuments.Columns(
                    "DocumentName").HeaderText =
                    "Document"

            End If


            If dgvRequestDocuments.Columns.Contains(
                "Quantity") Then

                dgvRequestDocuments.Columns(
                    "Quantity").HeaderText =
                    "Quantity"

            End If


            If dgvRequestDocuments.Columns.Contains(
                "Amount") Then

                dgvRequestDocuments.Columns(
                    "Amount").HeaderText =
                    "Fee"

                dgvRequestDocuments.Columns(
                    "Amount").DefaultCellStyle.Format =
                    "₱#,##0.00"

            End If


            If dgvRequestDocuments.Columns.Contains(
                "SubTotal") Then

                dgvRequestDocuments.Columns(
                    "SubTotal").HeaderText =
                    "Subtotal"

                dgvRequestDocuments.Columns(
                    "SubTotal").DefaultCellStyle.Format =
                    "₱#,##0.00"

            End If


        Catch ex As Exception

            MessageBox.Show(
                "Error loading requested documents: " &
                ex.Message,
                "Database Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)

        Finally

            If cn.State = ConnectionState.Open Then

                cn.Close()

            End If

        End Try

    End Sub


    ' =========================================================
    ' UPDATE STATUS
    ' =========================================================

    Private Sub btnUpdateStatus_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnUpdateStatus.Click


        ' =====================================================
        ' CHECK REQUEST
        ' =====================================================

        If RequestIDToView <= 0 Then

            MessageBox.Show(
                "No request selected.",
                "Update Status",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            Exit Sub

        End If


        ' =====================================================
        ' CHECK STATUS
        ' =====================================================

        If cboRequestStatus.SelectedIndex = -1 Then

            MessageBox.Show(
                "Please select a request status.",
                "Update Status",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            cboRequestStatus.Focus()

            Exit Sub

        End If


        ' =====================================================
        ' CONFIRMATION
        ' =====================================================

        Dim result As DialogResult =
            MessageBox.Show(
                "Are you sure you want to update the request status to:" &
                vbCrLf & vbCrLf &
                cboRequestStatus.Text & "?",
                "Update Status",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question)


        If result <> DialogResult.Yes Then

            Exit Sub

        End If


        Try

            connection()


            ' =====================================================
            ' UPDATE STATUS
            ' =====================================================

            sql = "UPDATE tblrequest " &
                  "SET Status = @Status " &
                  "WHERE RequestID = @RequestID"


            cmd = New MySqlCommand(sql, cn)


            cmd.Parameters.AddWithValue(
                "@Status",
                cboRequestStatus.Text)


            cmd.Parameters.AddWithValue(
                "@RequestID",
                RequestIDToView)


            cmd.ExecuteNonQuery()


            MessageBox.Show(
                "Request status updated successfully.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)


            ' =====================================================
            ' RELOAD CURRENT INFORMATION
            ' =====================================================

            LoadRequestDetails()


        Catch ex As Exception

            MessageBox.Show(
                "Error updating request status: " &
                ex.Message,
                "Database Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)


        Finally

            If cn.State = ConnectionState.Open Then

                cn.Close()

            End If

        End Try

    End Sub


    ' =========================================================
    ' BACK
    ' =========================================================

    Private Sub btnBack_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnBack.Click

        frmRequestList.LoadRequests()

        frmRequestList.Show()

        Me.Close()

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
    ' REPORTS
    ' =========================================================

    Private Sub btnReports_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnReports.Click

        frmReports.Show()

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

End Class