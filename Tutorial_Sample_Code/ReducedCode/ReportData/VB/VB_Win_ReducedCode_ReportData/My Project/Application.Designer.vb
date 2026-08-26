Option Strict On
Option Explicit On

Namespace My
    
    Partial Class MyApplication
        
        <Global.System.Diagnostics.DebuggerStepThrough()>  _
        Public Sub New()
            MyBase.New(Microsoft.VisualBasic.ApplicationServices.AuthenticationMode.Windows)
            Me.IsSingleInstance = false
            Me.EnableVisualStyles = true
            Me.SaveMySettingsOnExit = true
            Me.ShutDownStyle = Microsoft.VisualBasic.ApplicationServices.ShutdownMode.AfterMainFormCloses
        End Sub
        
        <Global.System.Diagnostics.DebuggerStepThrough()>  _
        Protected Overrides Sub OnCreateMainForm()
            Me.MainForm = Global.VB_Win_ReducedCode_ReportData.Form1
        End Sub
    End Class
End Namespace
