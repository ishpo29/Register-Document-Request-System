Imports System.Data.SqlClient
Imports MySql.Data.MySqlClient

Module DatabaseConnection

    Public cn As New MySqlConnection
    Public cmd As MySqlCommand
    Public dr As MySqlDataReader
    Public dt As New DataTable
    Public da As MySqlDataAdapter
    Public sql As String

    Public Sub ApplyRoleAccess(frm As Form,
                           usersButton As Button,
                           usersPicture As PictureBox)

        If CurrentRole = "Administrator" Then

            usersButton.Visible = True
            usersPicture.Visible = True

        Else

            usersButton.Visible = False
            usersPicture.Visible = False

        End If

    End Sub
    Public CurrentRole As String = ""
    Public Sub connection()

        cn.Close()

        cn.ConnectionString = "server=localhost;port=3306;user=root;password=;database=registrar_db"

        cn.Open()

    End Sub

End Module