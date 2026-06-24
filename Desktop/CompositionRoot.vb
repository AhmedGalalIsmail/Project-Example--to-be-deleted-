'Filename: Desktop/CompositionRoot.vb
Option Strict On
Imports System.IO
Imports Infrastructure.Config
Imports Infrastructure.Crypto
Imports Infrastructure.Logging


Public Class CompositionRoot
    Public Shared Logger As ILogger
    Public Shared Config As AppConfig
    Public Shared Crypto As CryptoService

    Public Shared Sub Bootstrap()
        Config = AppConfig.Load("appsettings.json")

        Dim baseDir = AppDomain.CurrentDomain.BaseDirectory
        Dim logDir = Path.Combine(baseDir, Config.LogDir)

        Logger = New FileLogger(Config.AppName, logDir)
        Crypto = New CryptoService()
        Logger.Info("Bootstrap complete")
    End Sub
End Class