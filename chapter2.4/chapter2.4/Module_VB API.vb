﻿Imports pfcls
Imports System.Configuration

Module Module_vbapi
    Public asyncConnection As IpfcAsyncConnection = Nothing '全局变量，用于存储连接会话的句柄
    ''' <summary>
    ''' 连接现有会话
    ''' </summary>
    ''' <returns>是否连接成功</returns>
    Public Function Creo_Connect() As Boolean
        Try
            If asyncConnection Is Nothing OrElse Not asyncConnection.IsRunning Then
                asyncConnection = (New CCpfcAsyncConnection).Connect(Nothing, Nothing, Nothing, Nothing)
                Creo_Connect = True
            Else
                Return False
            End If
        Catch ex As Exception
            Creo_Connect = False
            MsgBox(ex.Message.ToString + Chr(13) + ex.StackTrace.ToString)
        End Try
    End Function

    ''' <summary>
    ''' 打开新会话
    ''' </summary>
    ''' <returns>新建会话是否成功</returns>
    Public Function Creo_New() As Boolean
        Try
            Dim CmdLine As String = ConfigurationManager.AppSettings("CmdLine").ToString()
            Dim TextPath As String = ConfigurationManager.AppSettings("TextPath").ToString()
            asyncConnection = (New CCpfcAsyncConnection).Start(CmdLine, TextPath)
            '''''''''''''''''''''''补充之前的问题，使用config文件'''''''''''''''''''''''''''''''''
            asyncConnection.Session.LoadConfigFile(ConfigurationManager.AppSettings("Configfile").ToString())
            '''''''''''''''''''''''选择工作目录'''''''''''''''''''''''''''''''''
            asyncConnection.Session.ChangeDirectory(ConfigurationManager.AppSettings("WorkDirectory").ToString())
            Creo_New = True
        Catch ex As Exception
            Creo_New = False
            MsgBox(ex.Message.ToString + Chr(13) + ex.StackTrace.ToString)
        End Try
    End Function

    ''' <summary>
    ''' 打开一个模型
    ''' </summary>
    Public Sub Openprt()
        Dim modelDesc As IpfcModelDescriptor
        Dim fileOpenopts As IpfcFileOpenOptions
        Dim filename As String
        Dim retrieveModelOptions As IpfcRetrieveModelOptions
        Dim model As IpfcModel
        Try
            '使用ccpfc类初始化ipfc类，生成creo打开文件的对话框的选项
            fileOpenopts = (New CCpfcFileOpenOptions).Create("*.prt")
            '如果点击取消按钮，会throw一个"pfcExceptions::XToolkitUserAbort" Exception，被下面的catch捕捉
            filename = asyncConnection.Session.UIOpenFile(fileOpenopts)
            '使用ccpfc类初始化ipfc类，生成IpfcModelDescriptor
            modelDesc = (New CCpfcModelDescriptor).Create(EpfcModelType.EpfcMDL_PART, Nothing, Nothing)
            modelDesc.Path = filename
            '使用ccpfc类初始化ipfc类，生成IpfcRetrieveModelOptions
            retrieveModelOptions = (New CCpfcRetrieveModelOptions).Create
            retrieveModelOptions.AskUserAboutReps = False
            '加载零件
            model = asyncConnection.Session.RetrievemodelWithOpts(modelDesc, retrieveModelOptions)
            '显示零件
            model.Display()
            '激活当前窗体
            asyncConnection.Session.CurrentWindow.Activate()
        Catch ex As Exception
            If ex.Message <> "pfcExceptions::XToolkitUserAbort" Then
                MsgBox(ex.Message.ToString + Chr(13) + ex.StackTrace.ToString)
            End If
        End Try
    End Sub

    ''' <summary>
    ''' 保存当前打开的模型
    ''' </summary>
    Public Sub Savepart()
        Dim model As IpfcModel
        Try
            '当前打开的模型，也可以是别的model
            model = asyncConnection.Session.CurrentModel
            If model IsNot Nothing Then
                '执行则保存修改
                model.Save()
            End If
        Catch ex As Exception
            MsgBox(ex.Message.ToString + Chr(13) + ex.StackTrace.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' 枚举当前工作目录下所有prt
    ''' </summary>
    Public Sub ListFiles()
        Dim Files As Cstringseq
        Try
            '枚举工作目录下所有最新版prt文件
            Files = CType(asyncConnection.Session, IpfcBaseSession).ListFiles("*.prt", EpfcFileListOpt.EpfcFILE_LIST_LATEST, asyncConnection.Session.GetCurrentDirectory)
            For Each Str As String In Files
                MessageBox.Show(Str)
            Next
        Catch ex As Exception
            MsgBox(ex.Message.ToString + Chr(13) + ex.StackTrace.ToString)
        End Try
    End Sub
End Module
