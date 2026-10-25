Option Strict On
Option Explicit On

Imports System.Text.RegularExpressions
Imports System.Linq

Public Module AmdFsrSupportService
    Public Const DriverNotesUrl As String = "https://www.amd.com/en/resources/support-articles/release-notes/RN-RAD-WIN-26-10-1.html"
    Public Const SupportGuideUrl As String = "https://github.com/Wimukthi/OptiScalerInstaller/blob/main/docs/amd-fsr-support.md"

    ' Names identify a family, not the driver's active capabilities or the GPU rendering a game.
    Public Function ClassifyAdapter(adapterName As String) As AmdRdnaGeneration
        If String.IsNullOrWhiteSpace(adapterName) Then Return AmdRdnaGeneration.None
        Dim name As String = adapterName.ToUpperInvariant()
        If Not name.Contains("AMD") AndAlso Not name.Contains("RADEON") Then Return AmdRdnaGeneration.None

        Dim rx As Match = Regex.Match(name, "\bRX\s*([5679])\d{3}(?=\b|XT\b|XTX\b|GRE\b|M\b|S\b)")
        If rx.Success Then
            Select Case rx.Groups(1).Value
                Case "5" : Return AmdRdnaGeneration.Rdna1
                Case "6" : Return AmdRdnaGeneration.Rdna2
                Case "7" : Return AmdRdnaGeneration.Rdna3
                Case "9" : Return AmdRdnaGeneration.Rdna4
            End Select
        End If

        Dim workstation As Match = Regex.Match(name, "\bPRO\s+W([567])\d{3}\b")
        If workstation.Success Then
            Select Case workstation.Groups(1).Value
                Case "5" : Return AmdRdnaGeneration.Rdna1
                Case "6" : Return AmdRdnaGeneration.Rdna2
                Case "7" : Return AmdRdnaGeneration.Rdna3
            End Select
        End If
        If Regex.IsMatch(name, "\b(610M|660M|680M)\b") Then Return AmdRdnaGeneration.Rdna2
        If Regex.IsMatch(name, "\b(740M|760M|780M|880M|890M|8040S|8050S|8060S)\b") Then Return AmdRdnaGeneration.Rdna3
        If Regex.IsMatch(name, "\bVEGA(?:\s*\d+)?\b|\bRX\s*\d{3}\b|\bRADEON\s+R[579]\b") Then Return AmdRdnaGeneration.None
        If name.Contains("RDNA") OrElse Regex.IsMatch(name, "\bRADEON(?:\(TM\))?\s+(?:PRO\s+)?(?:GRAPHICS|W\d|AI\b)") Then
            Return AmdRdnaGeneration.Unknown
        End If
        Return AmdRdnaGeneration.None
    End Function

    Public Function BuildGuidance(adapterNames As IEnumerable(Of String)) As String
        Dim names As String() = If(adapterNames, Enumerable.Empty(Of String)()).ToArray()
        Dim desktop As String = names.FirstOrDefault(Function(name) name IsNot Nothing AndAlso Regex.IsMatch(name, "(?i)\b(?:AMD|RADEON)\b") AndAlso Regex.IsMatch(name, "(?i)\bRX\s*[679]\d{3}(?!\d)(?!\s*[MS]\b)(?=\b|XT\b|XTX\b|GRE\b)"))
        If desktop IsNot Nothing Then
            Dim displayName As String = desktop.Split(";"c)(0).Trim()
            Return displayName & ": AMD supports FSR Upscaling 4.1 across RX 6000, RX 7000 and RX 9000 cards. RX 6000 support starts with Adrenalin 26.10.1. Use a compatible OptiScaler build and check the active upscaler in its in-game overlay."
        End If
        If names.Any(Function(name) name IsNot Nothing AndAlso Regex.IsMatch(name, "(?i)\b(?:AMD|RADEON)\b.*\bPRO\b")) Then
            Return "AMD workstation GPU detected. Verify FSR support for the exact Radeon PRO model and installed PRO driver; RX desktop driver support does not establish workstation support. Match the OptiScaler build and check the active backend in game."
        End If
        If names.Any(Function(name) ClassifyAdapter(name) = AmdRdnaGeneration.Rdna2 OrElse ClassifyAdapter(name) = AmdRdnaGeneration.Rdna3 OrElse ClassifyAdapter(name) = AmdRdnaGeneration.Rdna4) Then
            Return "AMD RDNA GPU detected. Check your exact mobile/APU model and OEM driver: 26.10.1 adds FSR 4.1 for Ryzen AI Max 300/400 and Ryzen AI 9 300/400. RDNA generation alone does not establish APU support."
        End If
        If names.Any(Function(name) ClassifyAdapter(name) = AmdRdnaGeneration.Rdna1) Then
            Return "RDNA 1 detected. AMD's FSR Upscaling 4.1 support list starts at RX 6000 (RDNA 2). Use the upscalers supported by your hardware and OptiScaler build."
        End If
        Return "FSR Upscaling 4.1 supports Radeon RX 6000/7000/9000 with suitable drivers. GPU support is unconfirmed here; check the exact adapter and OptiScaler build. Selected Ryzen AI families also require a matching driver."
    End Function
End Module
