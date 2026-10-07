Imports System.Drawing
Imports System.Linq
Imports System.Security.Cryptography
Imports System.Text.RegularExpressions
Imports System.Windows.Forms
Imports MySql.Data.MySqlClient

''' <summary>
''' Shared helpers: inline validation, input filtering, password hashing,
''' friendly messages and consistent styling for every form.
''' </summary>
Module AppHelpers

    '========================================
    ' THEME COLORS
    '========================================
    Public ReadOnly ClrPrimary As Color = Color.FromArgb(37, 99, 235)
    Public ReadOnly ClrPrimaryHover As Color = Color.FromArgb(29, 78, 216)
    Public ReadOnly ClrSecondary As Color = Color.FromArgb(226, 232, 240)
    Public ReadOnly ClrSecondaryText As Color = Color.FromArgb(30, 41, 59)
    Public ReadOnly ClrDanger As Color = Color.FromArgb(220, 38, 38)
    Public ReadOnly ClrGridHeader As Color = Color.FromArgb(30, 41, 59)
    Public ReadOnly ClrGridAlt As Color = Color.FromArgb(241, 245, 249)
    Public ReadOnly ClrGridSelect As Color = Color.FromArgb(191, 219, 254)

    '========================================
    ' MESSAGE HELPERS (consistent titles/icons)
    '========================================
    Public Sub ShowInfo(message As String, Optional title As String = "Success")
        MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Public Sub ShowWarn(message As String, Optional title As String = "Please check")
        MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning)
    End Sub

    Public Sub ShowError(message As String, Optional title As String = "Something went wrong")
        MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub

    Public Function Confirm(message As String, Optional title As String = "Please confirm") As Boolean
        Return MessageBox.Show(message, title, MessageBoxButtons.YesNo,
                               MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) = DialogResult.Yes
    End Function

    ''' <summary>Turns raw database exceptions into messages a registrar can act on.</summary>
    Public Function FriendlyDbError(ex As Exception, Optional action As String = "complete this action") As String
        Dim mex As MySqlException = TryCast(ex, MySqlException)
        If mex IsNot Nothing Then
            Select Case mex.Number
                Case 0, 1042, 2002, 2003
                    Return "Cannot reach the database. Please make sure MySQL (XAMPP) is running, then try again."
                Case 1045
                    Return "The database rejected the login. Please contact the system administrator."
                Case 1049
                    Return "The database 'registrar_db' was not found. Please contact the system administrator."
                Case 1062
                    Return "That record already exists. Please check for duplicate values."
                Case 1451, 1217
                    Return "This record is in use by other records and cannot be removed. Deactivate it instead."
                Case 1452
                    Return "A related record is missing. Please refresh and try again."
                Case 1406
                    Return "One of the values is too long for the database. Please shorten it."
            End Select
        End If
        Return "Unable to " & action & ". Please try again. (" & ex.Message & ")"
    End Function

    '========================================
    ' INLINE VALIDATION (ErrorProvider)
    '========================================
    Public Function NewErrorProvider(frm As Form) As ErrorProvider
        Dim ep As New ErrorProvider(frm)
        ep.BlinkStyle = ErrorBlinkStyle.NeverBlink
        ep.SetIconAlignment(frm, ErrorIconAlignment.MiddleRight)
        Return ep
    End Function

    ''' <summary>Marks a control as invalid, shows the message beside it. Always returns False.</summary>
    Public Function Fail(ep As ErrorProvider, ctl As Control, message As String) As Boolean
        ep.SetIconAlignment(ctl, ErrorIconAlignment.MiddleRight)
        ep.SetIconPadding(ctl, 3)
        ep.SetError(ctl, message)
        Return False
    End Function

    Public Sub ClearErrors(ep As ErrorProvider, ParamArray ctls() As Control)
        For Each c As Control In ctls
            ep.SetError(c, "")
        Next
    End Sub

    ''' <summary>Required text field. Clears the error when valid.</summary>
    Public Function CheckRequired(ep As ErrorProvider, ctl As Control, label As String) As Boolean
        If ctl.Text.Trim() = "" Then
            Return Fail(ep, ctl, label & " is required.")
        End If
        ep.SetError(ctl, "")
        Return True
    End Function

    ''' <summary>Required + regex format check, with a plain-language hint.</summary>
    Public Function CheckPattern(ep As ErrorProvider, ctl As Control, label As String,
                                 pattern As String, hint As String) As Boolean
        If Not CheckRequired(ep, ctl, label) Then Return False
        If Not Regex.IsMatch(ctl.Text.Trim(), pattern) Then
            Return Fail(ep, ctl, hint)
        End If
        ep.SetError(ctl, "")
        Return True
    End Function

    Public Function CheckLength(ep As ErrorProvider, ctl As Control, label As String,
                                minLen As Integer, maxLen As Integer) As Boolean
        If Not CheckRequired(ep, ctl, label) Then Return False
        Dim n As Integer = ctl.Text.Trim().Length
        If n < minLen OrElse n > maxLen Then
            Return Fail(ep, ctl, label & " must be " & minLen & " to " & maxLen & " characters.")
        End If
        ep.SetError(ctl, "")
        Return True
    End Function

    ''' <summary>Focuses the first control in the list that has an error.</summary>
    Public Sub FocusFirstError(ep As ErrorProvider, ParamArray ctls() As Control)
        For Each c As Control In ctls
            If ep.GetError(c) <> "" Then
                c.Focus()
                Exit For
            End If
        Next
    End Sub

    '========================================
    ' COMMON FORMAT RULES
    '========================================
    ' PH Learner Reference Number = 12 digits
    Public Const PatLRN As String = "^\d{12}$"
    ' PH mobile: 09XXXXXXXXX or +639XXXXXXXXX
    Public Const PatPhMobile As String = "^(09\d{9}|\+639\d{9})$"
    ' Letters, spaces, hyphen, apostrophe, period (e.g. "De la Cruz", "O'Brien", "Jr.")
    Public Const PatName As String = "^[A-Za-zÀ-ÿñÑ][A-Za-zÀ-ÿñÑ .'\-]*$"
    ' Student ID like 2024-00123 or 20240123 (letters/digits/hyphen)
    Public Const PatStudentID As String = "^[A-Za-z0-9\-]{4,20}$"
    Public Const PatUsername As String = "^[A-Za-z0-9._]{4,30}$"

    '========================================
    ' INPUT FILTERS (prevent bad keystrokes + pasted junk)
    '========================================
    Public Sub RestrictToDigits(txt As TextBox, maxLen As Integer)
        txt.MaxLength = maxLen
        AddHandler txt.KeyPress,
            Sub(s As Object, e As KeyPressEventArgs)
                If Not Char.IsControl(e.KeyChar) AndAlso Not Char.IsDigit(e.KeyChar) Then e.Handled = True
            End Sub
        AddHandler txt.TextChanged,
            Sub(s As Object, e As EventArgs)
                Dim cleaned As String = Regex.Replace(txt.Text, "\D", "")
                If cleaned <> txt.Text Then
                    Dim pos As Integer = txt.SelectionStart
                    txt.Text = cleaned
                    txt.SelectionStart = Math.Min(pos, txt.Text.Length)
                End If
            End Sub
    End Sub

    ''' <summary>Phone box: digits and a leading + only.</summary>
    Public Sub RestrictToPhone(txt As TextBox)
        txt.MaxLength = 13
        AddHandler txt.KeyPress,
            Sub(s As Object, e As KeyPressEventArgs)
                If Char.IsControl(e.KeyChar) Then Return
                If Char.IsDigit(e.KeyChar) Then Return
                If e.KeyChar = "+"c AndAlso txt.SelectionStart = 0 AndAlso Not txt.Text.Contains("+") Then Return
                e.Handled = True
            End Sub
    End Sub

    Public Sub RestrictToName(txt As TextBox, maxLen As Integer)
        txt.MaxLength = maxLen
        AddHandler txt.KeyPress,
            Sub(s As Object, e As KeyPressEventArgs)
                If Char.IsControl(e.KeyChar) Then Return
                If Char.IsLetter(e.KeyChar) OrElse " .'-".IndexOf(e.KeyChar) >= 0 Then Return
                e.Handled = True
            End Sub
        ' Tidy capitalisation when leaving the field
        AddHandler txt.Leave,
            Sub(s As Object, e As EventArgs)
                txt.Text = Regex.Replace(txt.Text.Trim(), "\s+", " ")
            End Sub
    End Sub

    Public Sub RestrictToMoney(txt As TextBox)
        txt.MaxLength = 10
        AddHandler txt.KeyPress,
            Sub(s As Object, e As KeyPressEventArgs)
                If Char.IsControl(e.KeyChar) Then Return
                If Char.IsDigit(e.KeyChar) Then Return
                If e.KeyChar = "."c AndAlso Not txt.Text.Contains(".") Then Return
                e.Handled = True
            End Sub
    End Sub

    ''' <summary>Clears a control's error as soon as the user edits it.</summary>
    Public Sub ClearErrorOnEdit(ep As ErrorProvider, ParamArray ctls() As Control)
        For Each c As Control In ctls
            Dim captured As Control = c
            AddHandler captured.TextChanged, Sub(s As Object, e As EventArgs) ep.SetError(captured, "")
        Next
    End Sub

    ''' <summary>Pressing Enter moves to the next field instead of doing nothing.</summary>
    Public Sub EnterAsTab(frm As Form)
        For Each c As Control In AllControls(frm)
            If TypeOf c Is TextBox AndAlso Not DirectCast(c, TextBox).Multiline Then
                AddHandler c.KeyDown,
                    Sub(s As Object, e As KeyEventArgs)
                        If e.KeyCode = Keys.Enter Then
                            e.SuppressKeyPress = True
                            frm.SelectNextControl(DirectCast(s, Control), True, True, True, True)
                        End If
                    End Sub
            End If
        Next
    End Sub

    '========================================
    ' PASSWORDS (PBKDF2 with per-user salt)
    '========================================
    Private Const HashPrefix As String = "PBKDF2"
    Private Const HashIterations As Integer = 100000

    Public Function HashPassword(plain As String) As String
        Dim salt(15) As Byte
        Using rng As New RNGCryptoServiceProvider()
            rng.GetBytes(salt)
        End Using
        Using kdf As New Rfc2898DeriveBytes(plain, salt, HashIterations)
            Dim hash() As Byte = kdf.GetBytes(32)
            Return HashPrefix & "$" & HashIterations & "$" &
                   Convert.ToBase64String(salt) & "$" & Convert.ToBase64String(hash)
        End Using
    End Function

    Public Function IsHashed(stored As String) As Boolean
        Return stored IsNot Nothing AndAlso stored.StartsWith(HashPrefix & "$")
    End Function

    ''' <summary>Works with new hashes AND old plain-text passwords (so existing accounts keep working).</summary>
    Public Function VerifyPassword(plain As String, stored As String) As Boolean
        If stored Is Nothing Then Return False
        If Not IsHashed(stored) Then
            Return SlowEquals(System.Text.Encoding.UTF8.GetBytes(plain), System.Text.Encoding.UTF8.GetBytes(stored))
        End If
        Dim parts() As String = stored.Split("$"c)
        If parts.Length <> 4 Then Return False
        Dim iterations As Integer
        If Not Integer.TryParse(parts(1), iterations) Then Return False
        Dim salt() As Byte = Convert.FromBase64String(parts(2))
        Dim expected() As Byte = Convert.FromBase64String(parts(3))
        Using kdf As New Rfc2898DeriveBytes(plain, salt, iterations)
            Return SlowEquals(kdf.GetBytes(expected.Length), expected)
        End Using
    End Function

    Private Function SlowEquals(a() As Byte, b() As Byte) As Boolean
        Dim diff As Integer = a.Length Xor b.Length
        For i As Integer = 0 To Math.Min(a.Length, b.Length) - 1
            diff = diff Or (a(i) Xor b(i))
        Next
        Return diff = 0
    End Function

    ''' <summary>Returns an error message, or "" when the password is acceptable.</summary>
    Public Function PasswordProblem(pw As String, username As String) As String
        If pw.Length < 8 Then Return "Password must be at least 8 characters."
        If pw.Length > 64 Then Return "Password must be 64 characters or fewer."
        If Not Regex.IsMatch(pw, "[A-Za-z]") OrElse Not Regex.IsMatch(pw, "\d") Then
            Return "Password must contain both letters and numbers."
        End If
        If username <> "" AndAlso pw.ToLower().Contains(username.ToLower()) Then
            Return "Password must not contain the username."
        End If
        If pw.Trim() <> pw Then Return "Password must not start or end with a space."
        Return ""
    End Function

    '========================================
    ' CONSISTENT STYLING FOR EVERY FORM
    '========================================
    Public Function AllControls(root As Control) As List(Of Control)
        Dim list As New List(Of Control)
        For Each c As Control In root.Controls
            list.Add(c)
            list.AddRange(AllControls(c))
        Next
        Return list
    End Function

    ''' <summary>Call once from a form's Load event.</summary>
    Public Sub ThemeForm(frm As Form)
        Dim tip As New ToolTip() With {.InitialDelay = 400, .ReshowDelay = 200}

        For Each c As Control In AllControls(frm)

            If TypeOf c Is DataGridView Then
                StyleGrid(DirectCast(c, DataGridView))

            ElseIf TypeOf c Is Button Then
                StyleButton(DirectCast(c, Button), tip)

            ElseIf TypeOf c Is TextBox Then
                DirectCast(c, TextBox).BorderStyle = BorderStyle.FixedSingle

            ElseIf TypeOf c Is ComboBox Then
                Dim cbo As ComboBox = DirectCast(c, ComboBox)
                cbo.FlatStyle = FlatStyle.Flat
                ' Prevent free typing of invalid choices (pick from the list)
                If cbo.Items.Count > 0 AndAlso cbo.DropDownStyle = ComboBoxStyle.DropDown Then
                    cbo.DropDownStyle = ComboBoxStyle.DropDownList
                End If
            End If
        Next
    End Sub

    Private Sub StyleButton(b As Button, tip As ToolTip)
        Dim n As String = b.Name.ToLower()

        ' Leave sidebar navigation alone: they keep the original design
        Dim isNav As Boolean = n = "btndashboard" OrElse n = "btnstudents" OrElse n = "btndocuments" OrElse
                               n = "btnrequests" OrElse n = "btnreports" OrElse n = "btnusers" OrElse
                               n = "btnlogout"
        If isNav Then
            b.Cursor = Cursors.Hand
            Return
        End If

        b.FlatStyle = FlatStyle.Flat
        b.FlatAppearance.BorderSize = 0
        b.Cursor = Cursors.Hand
        b.UseVisualStyleBackColor = False

        Dim back As Color, fore As Color, hover As Color
        If n.Contains("delete") OrElse n.Contains("deactivate") OrElse n.Contains("cancel") Then
            back = ClrDanger : fore = Color.White : hover = Color.FromArgb(185, 28, 28)
        ElseIf n.Contains("clear") OrElse n.Contains("back") OrElse n.Contains("search") OrElse
               n.Contains("refresh") OrElse n.Contains("view") OrElse n.Contains("reset") Then
            back = ClrSecondary : fore = ClrSecondaryText : hover = Color.FromArgb(203, 213, 225)
        Else
            back = ClrPrimary : fore = Color.White : hover = ClrPrimaryHover
        End If

        b.BackColor = back
        b.ForeColor = fore
        b.FlatAppearance.MouseOverBackColor = hover
        b.FlatAppearance.MouseDownBackColor = hover

        If n.Contains("clear") Then tip.SetToolTip(b, "Empty all fields")
        If n.Contains("back") Then tip.SetToolTip(b, "Return to the previous screen without saving")
        If n.Contains("deactivate") Then tip.SetToolTip(b, "Hide this record from new transactions (can be re-activated)")
    End Sub

    Private Sub StyleGrid(g As DataGridView)
        g.EnableHeadersVisualStyles = False
        g.BorderStyle = BorderStyle.None
        g.BackgroundColor = Color.White
        g.RowHeadersVisible = False
        g.AllowUserToAddRows = False
        g.AllowUserToDeleteRows = False
        g.AllowUserToResizeRows = False
        g.ReadOnly = True
        g.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        g.MultiSelect = False
        g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        g.RowTemplate.Height = 30
        g.GridColor = Color.FromArgb(226, 232, 240)
        g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal

        g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None
        g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        g.ColumnHeadersHeight = 36
        g.ColumnHeadersDefaultCellStyle.BackColor = ClrGridHeader
        g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
        g.ColumnHeadersDefaultCellStyle.SelectionBackColor = ClrGridHeader
        g.ColumnHeadersDefaultCellStyle.Font = New Font("Segoe UI Semibold", 9.75F)

        g.DefaultCellStyle.Font = New Font("Segoe UI", 9.75F)
        g.DefaultCellStyle.SelectionBackColor = ClrGridSelect
        g.DefaultCellStyle.SelectionForeColor = Color.Black
        g.AlternatingRowsDefaultCellStyle.BackColor = ClrGridAlt
    End Sub

    '========================================
    ' EMPTY-STATE MESSAGE FOR GRIDS
    '========================================
    ''' <summary>Shows "No records found" text over an empty grid.</summary>
    Public Sub ShowEmptyHint(g As DataGridView, message As String)
        Dim lbl As Label = TryCast(g.Parent.Controls.Find("lblEmpty_" & g.Name, False).FirstOrDefault(), Label)
        If lbl Is Nothing Then
            lbl = New Label() With {
                .Name = "lblEmpty_" & g.Name,
                .AutoSize = False,
                .TextAlign = ContentAlignment.MiddleCenter,
                .ForeColor = Color.FromArgb(100, 116, 139),
                .Font = New Font("Segoe UI", 11.0F, FontStyle.Italic),
                .BackColor = Color.White
            }
            g.Parent.Controls.Add(lbl)
        End If
        lbl.Text = message
        lbl.Bounds = New Rectangle(g.Left, g.Top + 40, g.Width, 60)
        lbl.Visible = (g.Rows.Count = 0)
        lbl.BringToFront()
    End Sub

End Module
