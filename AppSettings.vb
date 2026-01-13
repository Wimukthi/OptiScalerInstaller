Option Strict On
Option Explicit On

Imports System.IO
Imports System.Text.Json

Friend Module AppSettings
    Private ReadOnly SettingsPath As String = Path.Combine(AppContext.BaseDirectory, "OptiScalerInstaller.settings.json")
    Private ReadOnly DefaultSettingsPath As String = Path.Combine(AppContext.BaseDirectory, "Data", "DefaultSettings.json")
    Private ReadOnly SyncRoot As New Object()
    Private _cache As AppSettingsModel

    Public Function Load() As AppSettingsModel
        Dim shouldSave As Boolean = False
        Dim settings As AppSettingsModel = Nothing
        Dim defaults As AppSettingsModel = Nothing

        SyncLock SyncRoot
            If _cache IsNot Nothing Then
                Return _cache
            End If

            settings = TryLoadFromFile(SettingsPath)
            defaults = TryLoadFromFile(DefaultSettingsPath)

            If settings Is Nothing Then
                settings = New AppSettingsModel()
                shouldSave = True
            End If

            If defaults IsNot Nothing Then
                If settings.ApplyDefaults(defaults) Then
                    shouldSave = True
                End If
            End If

            _cache = settings
        End SyncLock

        If shouldSave Then
            Save(settings)
        End If

        Return settings
    End Function

    Public Sub Save(settings As AppSettingsModel)
        If settings Is Nothing Then
            Return
        End If

        SyncLock SyncRoot
            _cache = settings
            Try
                Dim options As New JsonSerializerOptions With {
                    .WriteIndented = True
                }
                Dim json As String = JsonSerializer.Serialize(settings, options)
                File.WriteAllText(SettingsPath, json)
            Catch
            End Try
        End SyncLock
    End Sub

    Public Function GetSettingsPath() As String
        Return SettingsPath
    End Function

    Public Function LoadDefaults() As AppSettingsModel
        Return TryLoadFromFile(DefaultSettingsPath)
    End Function

    Public Sub Reload()
        SyncLock SyncRoot
            _cache = Nothing
        End SyncLock
    End Sub

    Private Function TryLoadFromFile(path As String) As AppSettingsModel
        If String.IsNullOrWhiteSpace(path) OrElse Not File.Exists(path) Then
            Return Nothing
        End If

        Try
            Dim json As String = File.ReadAllText(path)
            Dim settings As AppSettingsModel = JsonSerializer.Deserialize(Of AppSettingsModel)(json)
            Return settings
        Catch
            Return Nothing
        End Try
    End Function
End Module

Friend Class AppSettingsModel
    Public Property Theme As String
    Public Property WindowX As Integer?
    Public Property WindowY As Integer?
    Public Property WindowWidth As Integer?
    Public Property WindowHeight As Integer?
    Public Property WindowState As String
    Public Property CompatibilityListUrl As String
    Public Property Fsr4ListUrl As String
    Public Property WikiBaseUrl As String
    Public Property StableReleaseUrl As String
    Public Property NightlyReleaseUrl As String

    Public Function ApplyDefaults(defaults As AppSettingsModel) As Boolean
        If defaults Is Nothing Then
            Return False
        End If

        Dim changed As Boolean = False
        If String.IsNullOrWhiteSpace(CompatibilityListUrl) AndAlso Not String.IsNullOrWhiteSpace(defaults.CompatibilityListUrl) Then
            CompatibilityListUrl = defaults.CompatibilityListUrl
            changed = True
        End If
        If String.IsNullOrWhiteSpace(Fsr4ListUrl) AndAlso Not String.IsNullOrWhiteSpace(defaults.Fsr4ListUrl) Then
            Fsr4ListUrl = defaults.Fsr4ListUrl
            changed = True
        End If
        If String.IsNullOrWhiteSpace(WikiBaseUrl) AndAlso Not String.IsNullOrWhiteSpace(defaults.WikiBaseUrl) Then
            WikiBaseUrl = defaults.WikiBaseUrl
            changed = True
        End If
        If String.IsNullOrWhiteSpace(StableReleaseUrl) AndAlso Not String.IsNullOrWhiteSpace(defaults.StableReleaseUrl) Then
            StableReleaseUrl = defaults.StableReleaseUrl
            changed = True
        End If
        If String.IsNullOrWhiteSpace(NightlyReleaseUrl) AndAlso Not String.IsNullOrWhiteSpace(defaults.NightlyReleaseUrl) Then
            NightlyReleaseUrl = defaults.NightlyReleaseUrl
            changed = True
        End If

        Return changed
    End Function
End Class
