Option Strict On
Option Explicit On

Imports System.Linq
Imports System.Text.RegularExpressions

Public Class FrameGenerationConfigPlan
    Public Sub New()
        Updates = New List(Of IniUpdate)()
        ExpectedValues = New List(Of IniUpdate)()
        Warnings = New List(Of String)()
    End Sub

    Public Property Updates As List(Of IniUpdate)
    Public Property ExpectedValues As List(Of IniUpdate)
    Public Property Warnings As List(Of String)
End Class

Public Module FrameGenerationConfigService
    Private Const FrameGenSection As String = "FrameGen"

    ' Builds updates only for keys declared by the installed INI. The same plan
    ' drives post-install verification so it never checks a different schema.
    Public Function CreatePlan(document As IniDocument, selection As FgTypeSelection) As FrameGenerationConfigPlan
        Dim plan As New FrameGenerationConfigPlan()
        If selection = FgTypeSelection.Auto Then Return plan
        If document Is Nothing Then
            plan.Warnings.Add("Frame-generation settings could not be read; the selection was not applied.")
            Return plan
        End If

        Dim hasEnabled As Boolean = document.ContainsKey(FrameGenSection, "Enabled")
        Dim hasInput As Boolean = document.ContainsKey(FrameGenSection, "FGInput")
        Dim hasOutput As Boolean = document.ContainsKey(FrameGenSection, "FGOutput")
        Dim hasModernSchema As Boolean = hasEnabled OrElse hasInput OrElse hasOutput
        If Not hasModernSchema Then
            If document.ContainsKey(FrameGenSection, "FGType") Then
                AddDeclaredKey(plan, document, "FGType", GetLegacyValue(selection))
            Else
                plan.Warnings.Add("Frame-generation schema is unknown: no supported [FrameGen] keys are present; the selection was not applied.")
            End If
            Return plan
        End If

        If selection = FgTypeSelection.OptiFg OrElse selection = FgTypeSelection.Nukem Then
            Dim incomplete As New List(Of String)()
            For Each keyName As String In {"Enabled", "FGInput", "FGOutput"}
                Dim count As Integer = CountKeyOccurrences(document, FrameGenSection, keyName)
                If count = 0 Then
                    incomplete.Add(keyName & " is missing")
                ElseIf count > 1 Then
                    incomplete.Add(keyName & " is duplicated")
                End If
            Next
            If incomplete.Count > 0 Then
                plan.Warnings.Add("The modern [FrameGen] schema cannot fully apply this selection (" &
                                  String.Join(", ", incomplete) & "); no frame-generation keys were changed.")
                Return plan
            End If
        End If

        Dim inputValue As String = "nofg"
        Dim outputValue As String = "nofg"
        Dim enabledValue As String = "false"
        Select Case selection
            Case FgTypeSelection.OptiFg
                enabledValue = "true"
                inputValue = "upscaler"
                outputValue = "fsrfg"
            Case FgTypeSelection.Nukem
                enabledValue = "true"
                Dim tokenWarning As String = ""
                If TryResolveNukemTokens(document, inputValue, outputValue, tokenWarning) Then
                    ' The selected INI explicitly documents the current nvngxfg token.
                ElseIf Not String.IsNullOrWhiteSpace(tokenWarning) Then
                    plan.Warnings.Add(tokenWarning)
                    Return plan
                Else
                    inputValue = "nukems"
                    outputValue = "nukems"
                End If
        End Select

        If hasEnabled Then
            AddDeclaredKey(plan, document, "Enabled", enabledValue)
        Else
            plan.Warnings.Add("The modern [FrameGen] schema does not declare Enabled; the frame-generation enable flag was not written.")
        End If

        If hasInput Then
            AddDeclaredKey(plan, document, "FGInput", inputValue)
        Else
            plan.Warnings.Add("The modern [FrameGen] schema does not declare FGInput; the selected input was not written.")
        End If

        If hasOutput Then
            AddDeclaredKey(plan, document, "FGOutput", outputValue)
        Else
            plan.Warnings.Add("The modern [FrameGen] schema does not declare FGOutput; the selected output was not written.")
        End If

        Return plan
    End Function

    Private Sub AddDeclaredKey(plan As FrameGenerationConfigPlan, document As IniDocument, key As String, value As String)
        Dim occurrences As Integer = CountKeyOccurrences(document, FrameGenSection, key)
        If occurrences <> 1 Then
            plan.Warnings.Add(If(occurrences = 0,
                                 $"[{FrameGenSection}] {key} is not declared; it was not written.",
                                 $"[{FrameGenSection}] {key} has duplicate entries; it was not changed."))
            Return
        End If

        Dim update As New IniUpdate With {.Section = FrameGenSection, .Key = key, .Value = value}
        plan.Updates.Add(update)
        plan.ExpectedValues.Add(New IniUpdate With {.Section = update.Section, .Key = update.Key, .Value = update.Value})
    End Sub

    Private Function GetLegacyValue(selection As FgTypeSelection) As String
        Select Case selection
            Case FgTypeSelection.None : Return "nofg"
            Case FgTypeSelection.OptiFg : Return "optifg"
            Case FgTypeSelection.Nukem : Return "nukems"
            Case Else : Return "auto"
        End Select
    End Function

    Private Function TryResolveNukemTokens(document As IniDocument,
                                           ByRef inputValue As String,
                                           ByRef outputValue As String,
                                           ByRef warning As String) As Boolean
        warning = ""
        If document.ContainsKey("FSR", "Fsr4GpuSupport") Then
            inputValue = "nvngxfg"
            outputValue = "nvngxfg"
            Return True
        End If

        Dim inputDocumented As Boolean = HasDocumentedValue(document, "FGInput", "nvngxfg")
        Dim outputDocumented As Boolean = HasDocumentedValue(document, "FGOutput", "nvngxfg")
        If inputDocumented AndAlso outputDocumented Then
            inputValue = "nvngxfg"
            outputValue = "nvngxfg"
            Return True
        End If

        If inputDocumented Xor outputDocumented Then
            warning = "The current nvngxfg token is documented for only one frame-generation direction; the Nukem selection was not applied."
        End If
        Return False
    End Function

    Private Function HasDocumentedValue(document As IniDocument, key As String, token As String) As Boolean
        Dim inFrameGen As Boolean = False
        Dim comments As New System.Text.StringBuilder()
        For Each rawLine As String In document.ToText().Split({ControlChars.Cr, ControlChars.Lf}, StringSplitOptions.RemoveEmptyEntries)
            Dim line As String = rawLine.Trim()
            If line.StartsWith("[") AndAlso line.EndsWith("]") Then
                inFrameGen = line.Substring(1, line.Length - 2).Trim().Equals(FrameGenSection, StringComparison.OrdinalIgnoreCase)
                comments.Clear()
                Continue For
            End If
            If Not inFrameGen Then Continue For
            If line.StartsWith(";") OrElse line.StartsWith("#") Then
                comments.Append(" ").Append(line.Substring(1))
                Continue For
            End If

            Dim equalsIndex As Integer = line.IndexOf("="c)
            If equalsIndex > 0 Then
                Dim keyName As String = line.Substring(0, equalsIndex).Trim()
                If keyName.Equals(key, StringComparison.OrdinalIgnoreCase) AndAlso
                   Regex.IsMatch(comments.ToString(), "(?i)(?<![A-Za-z0-9_])" & Regex.Escape(token) & "(?![A-Za-z0-9_])", RegexOptions.CultureInvariant) Then
                    Return True
                End If
                comments.Clear()
            ElseIf line.Length > 0 Then
                comments.Clear()
            End If
        Next
        Return False
    End Function

    Private Function CountKeyOccurrences(document As IniDocument, section As String, key As String) As Integer
        Dim currentSection As String = ""
        Dim count As Integer = 0
        For Each rawLine As String In document.ToText().Split({ControlChars.Cr, ControlChars.Lf}, StringSplitOptions.RemoveEmptyEntries)
            Dim line As String = rawLine.Trim()
            If line.StartsWith("[") AndAlso line.EndsWith("]") Then
                currentSection = line.Substring(1, line.Length - 2).Trim()
                Continue For
            End If
            If Not currentSection.Equals(section, StringComparison.OrdinalIgnoreCase) OrElse line.StartsWith(";") OrElse line.StartsWith("#") Then Continue For
            Dim equalsIndex As Integer = line.IndexOf("="c)
            If equalsIndex > 0 AndAlso line.Substring(0, equalsIndex).Trim().Equals(key, StringComparison.OrdinalIgnoreCase) Then count += 1
        Next
        Return count
    End Function
End Module
