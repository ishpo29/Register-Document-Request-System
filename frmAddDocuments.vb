Imports MySql.Data.MySqlClient

Public Class frmAddDocuments

    ' Empty = Add Mode
    ' Has a value = Edit Mode
    Public DocumentIDToEdit As String = ""

    Private ep As ErrorProvider
    Private isDirty As Boolean = False

    ' Letters, numbers, spaces and common punctuation used in document names
    Private Const PatDocName As String = "^[A-Za-z0-9 ()/.,&'\-]{2,100}$"


    '========================================
    ' FORM LOAD
    '========================================
    Private Sub frmAddDocuments_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        ' Status choices (filled BEFORE ThemeForm so it becomes a pick-only list)
        cmbStatus.Items.Clear()
        cmbStatus.Items.Add("Active")

        cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList

        ThemeForm(Me)
        ep = NewErrorProvider(Me)

        txtDocumentName.MaxLength = 100
        txtDescription.MaxLength = 255

        ' Fee can be typed (numbers and one decimal point only)
        txtFee.ReadOnly = False
        RestrictToMoney(txtFee)

        ClearErrorOnEdit(ep, txtDocumentName, txtFee)

        Dim tip As New ToolTip()
        tip.SetToolTip(txtDocumentName, "e.g. Transcript of Records")
        tip.SetToolTip(txtDescription, "Optional")
        tip.SetToolTip(txtFee, "Amount in pesos, e.g. 150 or 150.50")

        EnterAsTab(Me)

        If DocumentIDToEdit = "" Then

            ' ADD MODE
            cmbStatus.SelectedItem = "Active"

        Else

            ' EDIT MODE
            LoadDocument()

        End If

        ' Track typing so Back / Clear can warn about unsaved input
        For Each c As TextBox In {txtDocumentName, txtDescription, txtFee}
            AddHandler c.TextChanged, Sub(s As Object, ev As EventArgs) isDirty = True
        Next

        isDirty = False
        txtDocumentName.Focus()

    End Sub


    '========================================
    ' LOAD DOCUMENT FOR EDITING
    '========================================
    Private Sub LoadDocument()

        Try

            connection()

            sql = "SELECT DocumentID, DocumentName, Description, Fee, Status " &
                  "FROM tbldocuments " &
                  "WHERE DocumentID = @DocumentID"

            cmd = New MySqlCommand(sql, cn)

            cmd.Parameters.AddWithValue("@DocumentID", DocumentIDToEdit)

            dr = cmd.ExecuteReader()

            If dr.Read() Then

                txtDocumentName.Text = dr("DocumentName").ToString()
                txtDescription.Text = dr("Description").ToString()

                If IsDBNull(dr("Fee")) Then
                    txtFee.Text = ""
                Else
                    txtFee.Text = Convert.ToDecimal(dr("Fee")).ToString("0.00")
                End If

                cmbStatus.Text = dr("Status").ToString()

            Else

                ShowError("Document not found. It may have been removed.", "Document missing")

            End If

            dr.Close()

        Catch ex As Exception

            If dr IsNot Nothing AndAlso Not dr.IsClosed Then
                dr.Close()
            End If

            ShowError(FriendlyDbError(ex, "load the document"))

        Finally

            If cn.State = ConnectionState.Open Then
                cn.Close()
            End If

        End Try

    End Sub


    '========================================
    ' DUPLICATE NAME CHECK (ignores the record being edited)
    '========================================
    Private Function NameAlreadyUsed() As Boolean

        Dim found As Boolean = False

        Try

            connection()

            sql = "SELECT COUNT(*) FROM tbldocuments " &
                  "WHERE LOWER(DocumentName) = LOWER(@Name) " &
                  "AND DocumentID <> @Self"

            cmd = New MySqlCommand(sql, cn)

            cmd.Parameters.AddWithValue("@Name", txtDocumentName.Text.Trim())
            cmd.Parameters.AddWithValue("@Self",
                                        If(DocumentIDToEdit = "", 0, Convert.ToInt32(DocumentIDToEdit)))

            found = Convert.ToInt32(cmd.ExecuteScalar()) > 0

            If found Then
                Fail(ep, txtDocumentName, "A document with this name already exists.")
                txtDocumentName.Focus()
            End If

        Catch ex As Exception

            ShowError(FriendlyDbError(ex, "check for duplicate documents"))
            found = True          ' stop the save if we cannot be sure

        Finally

            If cn.State = ConnectionState.Open Then
                cn.Close()
            End If

        End Try

        Return found

    End Function


    '========================================
    ' SAVE (ADD / UPDATE)
    '========================================
    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click

        ' =========================
        ' VALIDATION (inline, all at once)
        ' =========================

        Dim ok As Boolean = True
        Dim fee As Decimal = 0

        ok = CheckPattern(ep, txtDocumentName, "Document name", PatDocName,
                          "Document name must be 2-100 characters (letters, numbers, spaces and ( ) / . , & ' - only).") And ok

        If txtFee.Text.Trim() = "" Then

            ok = Fail(ep, txtFee, "Fee is required. Enter 0 for free documents.") And ok

        ElseIf Not Decimal.TryParse(txtFee.Text.Trim(),
                                    Globalization.NumberStyles.Number,
                                    Globalization.CultureInfo.InvariantCulture,
                                    fee) Then

            ok = Fail(ep, txtFee, "Please enter a valid amount, e.g. 150 or 150.50.") And ok

        ElseIf fee < 0 OrElse fee > 99999.99D Then

            ok = Fail(ep, txtFee, "Fee must be between 0 and 99,999.99.") And ok

        Else

            fee = Math.Round(fee, 2)
            ep.SetError(txtFee, "")

        End If

        If Not ok Then
            FocusFirstError(ep, txtDocumentName, txtFee)
            Exit Sub
        End If

        If cmbStatus.SelectedIndex = -1 Then
            cmbStatus.SelectedItem = "Active"
        End If

        ' Duplicate name (case-insensitive)
        If NameAlreadyUsed() Then Exit Sub

        If DocumentIDToEdit <> "" Then
            If Not Confirm("Save changes to this document?", "Update Document") Then Exit Sub
        End If

        Try

            connection()

            If DocumentIDToEdit = "" Then

                ' =========================
                ' ADD DOCUMENT
                ' =========================

                sql = "INSERT INTO tbldocuments " &
                      "(DocumentName, Description, Fee, Status) " &
                      "VALUES " &
                      "(@DocumentName, @Description, @Fee, @Status)"

                cmd = New MySqlCommand(sql, cn)

            Else

                ' =========================
                ' EDIT DOCUMENT
                ' =========================

                sql = "UPDATE tbldocuments SET " &
                      "DocumentName = @DocumentName, " &
                      "Description = @Description, " &
                      "Fee = @Fee, " &
                      "Status = @Status " &
                      "WHERE DocumentID = @DocumentID"

                cmd = New MySqlCommand(sql, cn)

                cmd.Parameters.AddWithValue("@DocumentID", DocumentIDToEdit)

            End If

            cmd.Parameters.AddWithValue("@DocumentName", txtDocumentName.Text.Trim())
            cmd.Parameters.AddWithValue("@Description", txtDescription.Text.Trim())
            cmd.Parameters.AddWithValue("@Fee", fee)
            cmd.Parameters.AddWithValue("@Status", cmbStatus.Text)

            cmd.ExecuteNonQuery()

            ShowInfo(If(DocumentIDToEdit = "",
                        "Document added successfully.",
                        "Document updated successfully."))

            isDirty = False

            ' Refresh the document list
            frmDocuments.LoadDocuments()

            frmDocuments.Show()

            Me.Close()

        Catch ex As Exception

            ShowError(FriendlyDbError(ex, "save the document"))

        Finally

            If cn.State = ConnectionState.Open Then
                cn.Close()
            End If

        End Try

    End Sub


    '========================================
    ' CLEAR
    '========================================
    Private Sub btnClear_Click(sender As Object, e As EventArgs) Handles btnClear.Click

        If isDirty AndAlso Not Confirm("Clear everything you've typed?", "Clear form") Then Exit Sub

        txtDocumentName.Clear()
        txtDescription.Clear()
        txtFee.Clear()

        cmbStatus.SelectedItem = "Active"

        ClearErrors(ep, txtDocumentName, txtFee)
        isDirty = False

        txtDocumentName.Focus()

    End Sub


    '========================================
    ' BACK
    '========================================
    Private Sub btnBack_Click(sender As Object, e As EventArgs) Handles btnBack.Click

        If isDirty AndAlso Not Confirm("You have unsaved changes. Leave without saving?", "Unsaved changes") Then Exit Sub

        frmDocuments.LoadDocuments()
        frmDocuments.Show()

        Me.Close()

    End Sub

End Class