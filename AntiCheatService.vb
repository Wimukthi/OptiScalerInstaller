Imports System.IO
Imports System.Linq

Public Class AntiCheatScanResult
    ' Best-effort anti-cheat detection result for a game folder.
    Public Property Detected As Boolean
    Public Property Provider As String
    Public Property Evidence As String
End Class

Public Module AntiCheatService
    ' Token signatures used for quick best-effort anti-cheat identification.
    ' Tokens are matched via Contains() on lowercased folder/file names, so keep them
    ' long enough (5+ chars) to avoid false positives from common substrings.
    Private ReadOnly SignatureMap As Dictionary(Of String, String()) = New Dictionary(Of String, String())(StringComparer.OrdinalIgnoreCase) From {
        {"Easy Anti-Cheat", New String() {"easyanticheat", "easyanticheat_eos", "easy anti-cheat", "start_protected_game"}},
        {"BattlEye", New String() {"battleye", "beservice", "beclient"}},
        {"EA AntiCheat", New String() {"eaanticheat", "eacore_anticheat"}},
        {"Riot Vanguard", New String() {"vanguard"}},
        {"PunkBuster", New String() {"punkbuster", "pbsvc"}},
        {"XIGNCODE3", New String() {"xigncode", "xhunter", "x3.xem"}},
        {"NProtect GameGuard", New String() {"gameguard", "npgg", "nprotect"}}
    }

    ' Scans a bounded folder set to avoid expensive full-drive traversal.
    Public Function Detect(gameFolder As String) As AntiCheatScanResult
        Dim result As New AntiCheatScanResult()
        If String.IsNullOrWhiteSpace(gameFolder) OrElse Not Directory.Exists(gameFolder) Then
            Return result
        End If

        Try
            Dim foldersToScan As List(Of String) = EnumerateFolders(gameFolder, 3, 750)
            For Each folder As String In foldersToScan
                Dim folderName As String = Path.GetFileName(folder).ToLowerInvariant()
                Dim folderMatch As String = MatchProvider(folderName)
                If Not String.IsNullOrWhiteSpace(folderMatch) Then
                    result.Detected = True
                    result.Provider = folderMatch
                    result.Evidence = folder
                    Return result
                End If

                For Each filePath As String In Directory.EnumerateFiles(folder, "*", SearchOption.TopDirectoryOnly)
                    Dim fileName As String = Path.GetFileName(filePath).ToLowerInvariant()
                    Dim fileMatch As String = MatchProvider(fileName)
                    If Not String.IsNullOrWhiteSpace(fileMatch) Then
                        result.Detected = True
                        result.Provider = fileMatch
                        result.Evidence = fileName
                        Return result
                    End If
                Next
            Next
        Catch ex As Exception
            ErrorLogger.Log(ex, "AntiCheatService.Detect")
        End Try

        Return result
    End Function

    ' Matches folder/file names against known anti-cheat tokens.
    Private Function MatchProvider(value As String) As String
        If String.IsNullOrWhiteSpace(value) Then
            Return ""
        End If

        Dim haystack As String = value.ToLowerInvariant()
        For Each kvp As KeyValuePair(Of String, String()) In SignatureMap
            For Each token As String In kvp.Value
                If haystack.Contains(token.ToLowerInvariant()) Then
                    Return kvp.Key
                End If
            Next
        Next

        Return ""
    End Function

    ' Breadth-first folder enumeration with depth and count caps for responsiveness.
    Private Function EnumerateFolders(root As String, maxDepth As Integer, maxFolders As Integer) As List(Of String)
        Dim output As New List(Of String)()
        If String.IsNullOrWhiteSpace(root) OrElse Not Directory.Exists(root) Then
            Return output
        End If

        Dim queue As New Queue(Of ScanNode)()
        queue.Enqueue(New ScanNode With {.FolderPath = root, .Depth = 0})

        While queue.Count > 0 AndAlso output.Count < maxFolders
            Dim node As ScanNode = queue.Dequeue()
            output.Add(node.FolderPath)

            If node.Depth >= maxDepth Then
                Continue While
            End If

            Dim children As IEnumerable(Of String) = Enumerable.Empty(Of String)()
            Try
                children = Directory.EnumerateDirectories(node.FolderPath, "*", SearchOption.TopDirectoryOnly)
            Catch ex As Exception
                ErrorLogger.Log(ex, "AntiCheatService.EnumerateFolders")
            End Try

            For Each child As String In children
                If output.Count + queue.Count >= maxFolders Then
                    Exit For
                End If
                queue.Enqueue(New ScanNode With {.FolderPath = child, .Depth = node.Depth + 1})
            Next
        End While

        Return output
    End Function

    Private Class ScanNode
        Public Property FolderPath As String
        Public Property Depth As Integer
    End Class
End Module
